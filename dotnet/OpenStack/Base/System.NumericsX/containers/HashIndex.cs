using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.NumericsX
{
    public class HashIndex
    {
        const int DEFAULT_HASH_SIZE = 1024;
        const int DEFAULT_HASH_GRANULARITY = 1024;

        int hashSize;
        int[] hash;
        int indexSize;
        int[] indexChain;
        int granularity;
        int hashMask;
        int lookupMask;

        static readonly int[] INVALID_INDEX = { -1 };

        public HashIndex(HashIndex other)
        {
            granularity = other.granularity;
            hashMask = other.hashMask;
            lookupMask = other.lookupMask;

            if (other.lookupMask == 0)
            {
                hashSize = other.hashSize;
                indexSize = other.indexSize;
                Free();
            }
            else
            {
                if (other.hashSize != hashSize || hash == INVALID_INDEX) { hashSize = other.hashSize; hash = new int[hashSize]; }
                if (other.indexSize != indexSize || indexChain == INVALID_INDEX) { indexSize = other.indexSize; indexChain = new int[indexSize]; }
                hash = other.hash;
                indexChain = other.indexChain;
            }
        }
        public HashIndex()
            => Init(DEFAULT_HASH_SIZE, DEFAULT_HASH_SIZE);
        public HashIndex(int initialHashSize, int initialIndexSize)
            => Init(initialHashSize, initialIndexSize);

        // returns total size of allocated memory
        public nint Allocated
            => hashSize * sizeof(int) + indexSize * sizeof(int);

        // returns total size of allocated memory including size of hash index type
        public nint Size
            => 0 + Allocated;

        // add an index to the hash, assumes the index has not yet been added to the hash
        public void Add(int key, int index)
        {
            int h;

            Debug.Assert(index >= 0);
            if (hash == INVALID_INDEX) Allocate(hashSize, index >= indexSize ? index + 1 : indexSize);
            else if (index >= indexSize) ResizeIndex(index + 1);
            h = key & hashMask;
            indexChain[index] = hash[h];
            hash[h] = index;
        }

        // remove an index from the hash
        public void Remove(int key, int index)
        {
            var k = key & hashMask;

            if (hash == INVALID_INDEX) return;
            if (hash[k] == index) hash[k] = indexChain[index];
            else
                for (var i = hash[k]; i != -1; i = indexChain[i]) if (indexChain[i] == index) { indexChain[i] = indexChain[index]; break; }
            indexChain[index] = -1;
        }

        // get the first index from the hash, returns -1 if empty hash entry
        public int First(int key)
            => hash[key & hashMask & lookupMask];

        // get the next index from the hash, returns -1 if at the end of the hash chain
        public int Next(int index)
        {
            Debug.Assert(index >= 0 && index < indexSize);
            return indexChain[index & lookupMask];
        }

        // insert an entry into the index and add it to the hash, increasing all indexes >= index
        public void InsertIndex(int key, int index)
        {
            int i, max;

            if (hash != INVALID_INDEX)
            {
                max = index;
                for (i = 0; i < hashSize; i++) if (hash[i] >= index) { hash[i]++; if (hash[i] > max) max = hash[i]; }
                for (i = 0; i < indexSize; i++) if (indexChain[i] >= index) { indexChain[i]++; if (indexChain[i] > max) max = indexChain[i]; }
                if (max >= indexSize) ResizeIndex(max + 1);
                for (i = max; i > index; i--) indexChain[i] = indexChain[i - 1];
                indexChain[index] = -1;
            }
            Add(key, index);
        }

        // remove an entry from the index and remove it from the hash, decreasing all indexes >= index
        public void RemoveIndex(int key, int index)
        {
            int i, max;

            Remove(key, index);
            if (hash != INVALID_INDEX)
            {
                max = index;
                for (i = 0; i < hashSize; i++) if (hash[i] >= index) { if (hash[i] > max) max = hash[i]; hash[i]--; }
                for (i = 0; i < indexSize; i++) if (indexChain[i] >= index) { if (indexChain[i] > max) max = indexChain[i]; indexChain[i]--; }
                for (i = index; i < max; i++) indexChain[i] = indexChain[i + 1];
                indexChain[max] = -1;
            }
        }

        // clear the hash
        public unsafe void Clear()
        {
            // only clear the hash table because clearing the indexChain is not really needed
            if (hash != INVALID_INDEX) fixed (void* hash_ = hash) Unsafe.InitBlock(hash_, 0xff, (uint)(hashSize * sizeof(int)));
        }

        // clear and resize
        public void Clear(int newHashSize, int newIndexSize)
        {
            Free();
            hashSize = newHashSize;
            indexSize = newIndexSize;
        }

        // free allocated memory
        public void Free()
        {
            if (hash != INVALID_INDEX) hash = INVALID_INDEX;
            if (indexChain != INVALID_INDEX) indexChain = INVALID_INDEX;
            lookupMask = 0;
        }

        // get size of hash table
        public int HashSize
            => hashSize;

        // get size of the index
        public int IndexSize
            => indexSize;

        // set granularity
        public void SetGranularity(int newGranularity)
        {
            Debug.Assert(newGranularity > 0);
            granularity = newGranularity;
        }

        // force resizing the index, current hash table stays intact
        public unsafe void ResizeIndex(int newIndexSize)
        {
            int[] oldIndexChain; int mod, newSize;

            if (newIndexSize <= indexSize) return;

            mod = newIndexSize % granularity;
            newSize = mod == 0 ? newIndexSize : newIndexSize + granularity - mod;

            if (indexChain == INVALID_INDEX) { indexSize = newSize; return; }

            oldIndexChain = indexChain;
            indexChain = new int[newSize];

            fixed (void* indexChain_ = indexChain, indexChain2_ = &indexChain[indexSize], oldIndexChain_ = oldIndexChain)
            {
                Unsafe.CopyBlock(indexChain_, oldIndexChain_, (uint)(indexSize * sizeof(int)));
                Unsafe.InitBlock(indexChain2_, 0xff, (uint)((newSize - indexSize) * sizeof(int)));
            }
            indexSize = newSize;
        }

        // returns number in the range [0-100] representing the spread over the hash table
        public int GetSpread()
        {
            int i, index, totalItems, average, error, e;

            if (hash == INVALID_INDEX) return 100;

            totalItems = 0;
            var numHashItems = new int[hashSize];
            for (i = 0; i < hashSize; i++)
            {
                numHashItems[i] = 0;
                for (index = hash[i]; index >= 0; index = indexChain[index]) numHashItems[i]++;
                totalItems += numHashItems[i];
            }
            // if no items in hash
            if (totalItems <= 1) return 100;
            average = totalItems / hashSize;
            error = 0;
            for (i = 0; i < hashSize; i++)
            {
                e = Math.Abs(numHashItems[i] - average);
                if (e > 1) error += e - 1;
            }
            return 100 - (error * 100 / totalItems);
        }

        // returns a key for a string
        public int GenerateKey(string s, bool caseSensitive = true)
            => caseSensitive
                ? s.GetHashCode() & hashMask
                : s.ToLowerInvariant().GetHashCode() & hashMask;

        // returns a key for a vector
        public int GenerateKey(Vector3 v)
            => (((int)v[0]) + ((int)v[1]) + ((int)v[2])) & hashMask;

        // returns a key for two integers
        public int GenerateKey(int n1, int n2)
            => ((n1 + n2) & hashMask);

        void Init(int initialHashSize, int initialIndexSize)
        {
            Debug.Assert(MathX.IsPowerOfTwo(initialHashSize));

            hashSize = initialHashSize;
            hash = INVALID_INDEX;
            indexSize = initialIndexSize;
            indexChain = INVALID_INDEX;
            granularity = DEFAULT_HASH_GRANULARITY;
            hashMask = hashSize - 1;
            lookupMask = 0;
        }

        unsafe void Allocate(int newHashSize, int newIndexSize)
        {
            Debug.Assert(MathX.IsPowerOfTwo(newHashSize));

            Free();
            hashSize = newHashSize;
            hash = new int[hashSize];
            fixed (void* hash_ = hash) Unsafe.InitBlock(hash_, 0xff, (uint)(hashSize * sizeof(int)));
            indexSize = newIndexSize;
            indexChain = new int[indexSize];
            fixed (void* indexChain_ = indexChain) Unsafe.InitBlock(indexChain_, 0xff, (uint)(indexSize * sizeof(int)));
            hashMask = hashSize - 1;
            lookupMask = -1;
        }
    }
}