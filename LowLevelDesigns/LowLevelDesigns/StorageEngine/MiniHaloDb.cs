using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowLevelDesigns.StorageEngine
{
    public class MiniHaloDb
    {
        private readonly string _dataFilePath = "data.db";
        private Dictionary<string, long> _index = new();

        public MiniHaloDb()
        {
            // Create file is their is no file
            if (!File.Exists(_dataFilePath))
                File.Create(_dataFilePath);

            // Rebuild index from existing file
            RebuildIndex();
        }

        // Save key-value to file and update index
        public void Put(string key, string value)
        {
            // Initialize a new instance of FileStream class with specified path, creation mode and read-write access
            using var fileStream = new FileStream(_dataFilePath, FileMode.Append, FileAccess.Write);
            
            // Gets the current position of this stream
            long offset = fileStream.Position;

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
            fileStream.Write(BitConverter.GetBytes(keyBytes.Length));
            fileStream.Write(BitConverter.GetBytes(valueBytes.Length));
            fileStream.Write(keyBytes);
            fileStream.Write(valueBytes);

            // Now that lengths are written we can safely store the data
            // Later when reading,
            // You'll first read first 4 bytes for key length
            // Then read 4 bytes for value length
            // then read key length bytes for the key
            // and value length bytes for the value
            // Final format: [keylength: 4 bytes for key][valuelength: 4 bytes of value][key (Nbytes][value (mbytes)]
            _index[key] = offset;
        }

        // Read value using offset for index
        public string Get(string key)
        {
            if (!_index.ContainsKey(key)) return null;

            long offset =_index[key];
            using var fileStream = new FileStream(_dataFilePath, FileMode.Append, FileAccess.Read);
            fileStream.Seek(offset, SeekOrigin.Begin);

            byte[] intBuffer = new byte[4];

            fileStream.Read(intBuffer, 0, 4);
            int keyLen = BitConverter.ToInt32(intBuffer);

            fileStream.Read(intBuffer, 0, 4);
            int valLen = BitConverter.ToInt32(intBuffer);

            byte[] keyBuffer = new byte[keyLen];
            fileStream.Read(keyBuffer, 0, keyLen);

            byte[] valueBuffer = new byte[valLen];
            fileStream.Read(valueBuffer, 0, valLen);

            return Encoding.UTF8.GetString(valueBuffer);
        }

        private void RebuildIndex()
        {
            throw new NotImplementedException();
        }
    }
}
