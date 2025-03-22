using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowLevelDesigns.StorageEngine
{
    public class MiniHaloDb
    {
        private readonly string _dataFilePath = "data.db";
        private readonly string _walFilePath = "wal.log";
        private const long SegmentMaxSize = 10 * 1024 * 1024;
        private string dataDir = "data";
        private int currentSegmentNumber = 0;

        private string activeSegmentPath;
        private FileStream activeSegmentStream;
        private Dictionary<string, (int segmentNumber, long offset)> keyIndex = new();

        private MemoryMappedFile _mmf;
        private MemoryMappedViewAccessor _accessor;
        private Dictionary<string, DateTime> _expiry = new();
        private Dictionary<string, long> _index = new();
        private ReaderWriterLockSlim _lock = new();
        private readonly object ttlLock = new();
        private BloomFilter bloomFilter;
        private LRUCache cache;

        /// <summary>
        /// ctor
        /// </summary>
        public MiniHaloDb()
        {
            bloomFilter = new BloomFilter(10000, 3);
            cache = new LRUCache(10000);
            if (!Directory.Exists(dataDir))
                Directory.CreateDirectory(dataDir);

            LoadLastSegment();
            RebuildIndex();
            StartTTLBackgroundCleanup();

            //RecoverFromWal(); // Needs update
        }


        private void LoadLastSegment()
        {
            var files = Directory.GetFiles(dataDir, "segment_*.db").OrderBy(x => x).ToList();

            if (files.Any())
            {
                string last = files.Last();
                currentSegmentNumber = int.Parse(Path.GetFileNameWithoutExtension(last).Split('_')[1]);
            }

            OpenNewSegment();
        }

        private void OpenNewSegment()
        {
            activeSegmentPath = Path.Combine(dataDir, $"segment_{currentSegmentNumber}.db");
            activeSegmentStream = new FileStream(activeSegmentPath, FileMode.Append, FileAccess.Write, FileShare.Read);
        }

        public void PutWithTTL(string key, string value, TimeSpan ttl)
        {
            Put(key, value);
            lock (ttlLock)
            {
                _expiry[key] = DateTime.UtcNow.Add(ttl);
            }
            AppendToWal("TTL", key, ttl.TotalSeconds.ToString());
        }

        // Save key-value to file and update index
        public void Put(string key, string value)
        {
            try
            {
                _lock.EnterWriteLock();

                cache.AddKey(key, value);
                // Gets the current position of this stream
                long offset = activeSegmentStream.Position;

                // Encodes all the character in a specified string to a sequence of bytes
                // WHy is it required ?
                // Disk files are binary, not text, string needs to be converted to byte arrays for binary writing. We are using UTF-8
                // encoding which is standard and supports all characters.

                byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                byte[] valueBytes = Encoding.UTF8.GetBytes(value);

                // When reading later we need to know:
                // How many bytes to read for the key
                // How many bytes to read for the value
                // BitConverter.GetBytes(int) converts the length to a 4-byte binary format.
                activeSegmentStream.Write(BitConverter.GetBytes(keyBytes.Length));
                activeSegmentStream.Write(BitConverter.GetBytes(valueBytes.Length));
                activeSegmentStream.Write(keyBytes);
                activeSegmentStream.Write(valueBytes);
                activeSegmentStream.Flush();

                // Now that lengths are written we can safely store the data
                // Later when reading,
                // You'll first read first 4 bytes for key length
                // Then read 4 bytes for value length
                // then read key length bytes for the key
                // and value length bytes for the value
                // Final format: [keylength: 4 bytes for key][valuelength: 4 bytes of value][key (Nbytes][value (mbytes)]
                keyIndex[key] = (currentSegmentNumber, offset);
                
                //_index[key] = offset;

                _expiry.Remove(key);
                AppendToWal("PUT", key, value);
                bloomFilter.Add(key);

                if(activeSegmentStream.Length > SegmentMaxSize)
                {
                    activeSegmentStream.Dispose();
                    currentSegmentNumber++;
                    OpenNewSegment();
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        // Read value using offset for index
        public string Get(string key)
        {
            if (!bloomFilter.MightContain(key)) return null;
            
            lock (ttlLock)
            {
                if (_expiry.TryGetValue(key, out var expiry) && DateTime.UtcNow > expiry) return null;
            }

            string valueFromCache = cache.Get(key);
            if (!string.IsNullOrWhiteSpace(valueFromCache)) return valueFromCache;

            _lock.EnterReadLock();
            try
            {
                if (!keyIndex.TryGetValue(key, out var info)) return null;

                int segmentNumber = keyIndex[key].segmentNumber;
                long offset = keyIndex[key].offset;
                /*using var fileStream = new FileStream(_dataFilePath, FileMode.Append, FileAccess.Read);
                fileStream.Seek(offset, SeekOrigin.Begin);

                byte[] intBuffer = new byte[4];

                fileStream.Read(intBuffer, 0, 4);
                int keyLen = BitConverter.ToInt32(intBuffer);

                fileStream.Read(intBuffer, 0, 4);
                int valLen = BitConverter.ToInt32(intBuffer);

                byte[] keyBuffer = new byte[keyLen];
                fileStream.Read(keyBuffer, 0, keyLen);

                byte[] valueBuffer = new byte[valLen];
                fileStream.Read(valueBuffer, 0, valLen);*/

                if(segmentNumber == currentSegmentNumber)
                {
                    int keylen = _accessor.ReadInt32(offset);
                    int vallen = _accessor.ReadInt32(offset + 4);

                    long valStart = offset + 8 + keylen;
                    byte[] valBuf = new byte[vallen];
                    _accessor.ReadArray(valStart, valBuf, 0, vallen);

                    return Encoding.UTF8.GetString(valBuf);
                }
                else
                {
                    string segmentPath = $"segment_{segmentNumber}.db";
                    using var segmentStream = new FileStream(segmentPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    segmentStream.Seek(offset, SeekOrigin.Begin);

                    byte[] buffer = new byte[4];
                    segmentStream.Read(buffer, 0, 4);
                    int keyLen = BitConverter.ToInt32(buffer);

                    segmentStream.Read(buffer, 0, 4);
                    int valueLen = BitConverter.ToInt32(buffer);

                    byte[] keyBuffer = new byte[keyLen];
                    byte[] valBuffer = new byte[valueLen];

                    segmentStream.Read(keyBuffer, 0, keyLen);
                    segmentStream.Read(valBuffer, 0, valueLen);

                    return Encoding.UTF8.GetString(valBuffer);
                }

            }
            finally
            {
                _lock.ExitReadLock();
            }

        }

        public void Delete(string key)
        {
            _lock.EnterWriteLock();
            try
            {
                _index.Remove(key);
                _expiry.Remove(key);
                AppendToWal("DEL", key);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Compact()
        {
            _lock.EnterWriteLock();
            try
            {
                string _compactFilePath = "data_compacted.db";
                var writestream = new FileStream(_compactFilePath, FileMode.Create, FileAccess.Write);
                Dictionary<string, long> newIndex = new();

                foreach (var kvp in _index)
                {
                    string key = kvp.Key;
                    if (_expiry.TryGetValue(key, out var expirytime) && DateTime.UtcNow > expirytime) continue;

                    string value = Get(key);

                    byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                    byte[] valuesBytes = Encoding.UTF8.GetBytes(value);

                    long offset = writestream.Position;

                    writestream.Write(BitConverter.GetBytes(keyBytes.Length));
                    writestream.Write(BitConverter.GetBytes(valuesBytes.Length));
                    writestream.Write(keyBytes);
                    writestream.Write(valuesBytes);

                    newIndex[key] = offset;
                }

                File.Replace(_compactFilePath, _dataFilePath, null);
                _index = newIndex;

                _mmf?.Dispose();
                _accessor.Dispose();
                _mmf = MemoryMappedFile.CreateFromFile(_dataFilePath, FileMode.Open, "mmf");
                _accessor = _mmf.CreateViewAccessor();

                File.WriteAllText(_walFilePath, string.Empty);

            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        // Needs update after implementing segmentation of files
        public void CreateSnapShot(string filePath = "snapshot.meta")
        {
            _lock.EnterWriteLock();
            try
            {
                using var writer = new BinaryWriter(File.Open(filePath, FileMode.Create));

                writer.Write(_index.Count);
                foreach(var kvp in _index)
                {
                    writer.Write(kvp.Key);
                    writer.Write(kvp.Value);
                }

                writer.Write(_expiry.Count);
                foreach (var kvp in _expiry)
                {
                    writer.Write(kvp.Key);
                    writer.Write(kvp.Value.ToBinary());
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        // Needs update after implementing segmentation of files
        public void LoadSnapshot(string filePath = "snapshot.meta")
        {
            _lock.EnterWriteLock();
            try
            {
                using var reader = new BinaryReader(File.Open(filePath, FileMode.Open));
                int count = reader.ReadInt32();
                _index = new Dictionary<string, long>();
                for (int i = 0; i < count; i++)
                {
                    var key = reader.ReadString();
                    var offset = reader.ReadInt64();
                    _index[key] = offset;
                }

                int ttlcount = reader.ReadInt32();
                _expiry = new Dictionary<string, DateTime>();
                for (int i = 0; i < ttlcount; i++)
                {
                    string key = reader.ReadString();
                    DateTime expiry = DateTime.FromBinary(reader.ReadInt64());
                    _expiry[key] = expiry;
                }

            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }


        public void Close()
        {
            _accessor?.Dispose();
            _mmf?.Dispose();
        }

        private void StartTTLBackgroundCleanup()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(5000);
                    var expiredKeys = new List<string>();

                    lock (ttlLock)
                    {
                        foreach(var kvp in _expiry)
                        {
                            var key = kvp.Key;
                            if (DateTime.UtcNow > kvp.Value)
                                expiredKeys.Add(key);
                        }

                        foreach(var key in expiredKeys)
                        {
                            _expiry.Remove(key);
                        }
                    }

                }
            });
        }

        private void AppendToWal(string op, string key, string value = "")
        {
            File.AppendAllText(_walFilePath, $"{op}|{key}|{value}\n");
        }

        private void RecoverFromWal()
        {
            if (!File.Exists(_walFilePath)) return;

            foreach (var line in File.ReadLines(_walFilePath))
            {
                var parts = line.Split("|");
                if (parts.Length < 2) continue;

                string op = parts[0];
                string key = parts[1];

                switch (op)
                {
                    case "PUT":
                        if (parts.Length >= 3) Put(key, parts[2]);
                        break;
                    case "DEL":
                        _index.Remove(key);
                        _expiry.Remove(key);
                        break;
                    case "TTL":
                        if (parts.Length >= 3 && double.TryParse(parts[2], out var seconds))
                            _expiry[key] = DateTime.UtcNow.AddSeconds(seconds);
                        break;

                }
            }
        }

        // Rebuild the index from the data file
        private void RebuildIndex()
        {
            using var stream = new FileStream(activeSegmentPath, FileMode.Open, FileAccess.Read);
            long offset = 0;
            byte[] intBuf = new byte[4];

            while (offset < stream.Length)
            {
                stream.Seek(offset, SeekOrigin.Begin);
                stream.Read(intBuf, 0, 4);

                int keyLength = BitConverter.ToInt32(intBuf);

                stream.Read(intBuf, 0, 4);
                int valueLength = BitConverter.ToInt32(intBuf);

                byte[] keyBuffer = new byte[keyLength];
                stream.Read(keyBuffer, 0, keyLength);

                stream.Seek(valueLength, SeekOrigin.Current);

                string key = Encoding.UTF8.GetString(keyBuffer);

                keyIndex[key] = (currentSegmentNumber, offset);
                //_index[key] = offset;

                offset = stream.Position;
            }

            // Setup the memory mapped file
            _mmf?.Dispose();
            _accessor.Dispose();

            _mmf = MemoryMappedFile.CreateFromFile(activeSegmentPath, FileMode.Open, "mmf");
            _accessor = _mmf.CreateViewAccessor();
        }
    }
}
