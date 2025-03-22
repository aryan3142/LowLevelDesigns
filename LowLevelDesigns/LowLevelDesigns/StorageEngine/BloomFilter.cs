using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LowLevelDesigns.StorageEngine
{
    /*
     * High Level Overview:
     * A bitarray to track which positions are set
     * multiple hash functions (via different offsets into a single md5 hash)
     * Add() to insert items
     * MightContain() to check if an item is present
     */

    public class BloomFilter
    {
        private readonly BitArray bitArray;
        private readonly int size;
        private readonly int hashCount;

        public BloomFilter(int size, int hashCount)
        {
            this.size = size;
            this.hashCount = hashCount;
            bitArray = new BitArray(size);
        }

        public void Add(string item)
        {
            // Converts the item into several hashes
            // Each hash value is mapped to a specific position in the bit array
            // (using hash%size to ensure that its within bounds)
            // Sets those position to true
            foreach(int hash in GetHashes(item))
            {
                bitArray[hash % size] = true;
            }
        }

        public bool MightContain(string item)
        {
            //Check if all bit positions derived from the list of hashes are set  to true
            //If any one is false the item is definetely as not present
            foreach (int hash in GetHashes(item))
            {
                if (!bitArray[hash % size])
                    return false;
            }
            return true;
        }

        private IEnumerable<int> GetHashes(string input)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            using var md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(bytes);

            for (int i = 0; i < hashCount; i++)
            {
                // Extracting int values by taking 4 bytes at different offsets
                // Each int value is used as a hash value
                yield return BitConverter.ToInt32(hash, (i * 4) % hash.Length);
            }
        }

        /*
         * The more items you add, the higher the chances of false positives
         * Choosing size and hashcount correctly is important  for good performance
         * MD5 is cryptographically not secure algorithm anymore, but it's fine for Bloom Filter usecases
         */
    }
}
