using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace System.NumericsX
{
    public class DynamicAlloc<T> where T : new()
    {
        int baseBlockSize;
        int minBlockSize;
        int sizeofT;
        Func<int, T[]> factory;

        int numUsedBlocks;          // number of used blocks
        int usedBlockMemory;        // total memory in used blocks

        int numAllocs;
        int numResizes;
        int numFrees;

        public DynamicAlloc(int baseBlockSize, int minBlockSize)
        {
            this.baseBlockSize = baseBlockSize;
            this.minBlockSize = minBlockSize;
            this.sizeofT = 0;
            this.factory = num => Enumerable.Repeat(new T(), num).ToArray();
            Clear();
        }

        void Clear()
        {
            numUsedBlocks = 0;
            usedBlockMemory = 0;
            numAllocs = 0;
            numResizes = 0;
            numFrees = 0;
        }

        public void Init() { }
        public void Shutdown() => Clear();
        public void SetFixedBlocks(int numBlocks) { }
        public void SetPinMemory(bool pin) { }
        public void SetLockMemory(bool @lock) { }
        public void FreeEmptyBaseBlocks() { }

        public T[] Alloc(int num)
        {
            numAllocs++;

            if (num <= 0) return default;

            numUsedBlocks++;
            usedBlockMemory += num * sizeofT;
            var block = factory(num);
            return block;
        }

        public T[] Resize(T[] ptr, int num)
        {
            numResizes++;
            if (ptr == null) return Alloc(num);
            if (num <= 0) { Free(ptr); return default; }

            Debug.Assert(false);
            return ptr;
        }

        public void Free(T[] ptr)
        {
            numFrees++;
            if (ptr == null) return;
            if (ptr is IDisposable ptr1) ptr1.Dispose();
        }

        public string CheckMemory(T[] ptr) => null;

        public int NumBaseBlocks => 0;
        public int BaseBlockMemory => 0;
        public int NumUsedBlocks => numUsedBlocks;
        public int UsedBlockMemory => usedBlockMemory;
        public int NumFreeBlocks => 0;
        public int FreeBlockMemory => 0;
        public int NumEmptyBaseBlocks => 0;
    }
}