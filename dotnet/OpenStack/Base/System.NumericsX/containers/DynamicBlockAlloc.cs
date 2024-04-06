#define DYNAMIC_BLOCK_ALLOC_CHECK
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace System.NumericsX
{
    class DynamicBlock<T> : DynamicElement<T> where T : new()
    {
        public int Size => Math.Abs(size);
        public void SetSize(int s, bool isBaseBlock) => size = isBaseBlock ? -s : s;
        public bool IsBaseBlock => size < 0;

#if DYNAMIC_BLOCK_ALLOC_CHECK
        public int[] id = new int[3];
        public DynamicBlockAlloc<T> allocator;
#endif

        public int size;                   // size in bytes of the block
        public DynamicBlock<T> prev;                   // previous memory block
        public DynamicBlock<T> next;                   // next memory block
        public BTree<DynamicBlock<T>, int>.Node node;			// node in the B-Tree with free blocks
    }

    public class DynamicBlockAlloc<T> where T : new()
    {
        int baseBlockSize;
        int minBlockSize;
        int sizeofT;
        Func<int, T[]> factory;

        DynamicBlock<T> firstBlock;     // first block in list in order of increasing address
        DynamicBlock<T> lastBlock;      // last block in list in order of increasing address
        BTree<DynamicBlock<T>, int> freeTree = new(4);          // B-Tree with free memory blocks
        bool allowAllocs;               // allow base block allocations
        bool pinMemory;                 // lock memory so it cannot get swapped out
        bool lockMemory;                // lock memory so it cannot get swapped out

#if DYNAMIC_BLOCK_ALLOC_CHECK
        int[] blockId = new int[3];
#endif

        int numBaseBlocks;          // number of base blocks
        int baseBlockMemory;        // total memory in base blocks
        int numUsedBlocks;          // number of used blocks
        int usedBlockMemory;        // total memory in used blocks
        int numFreeBlocks;          // number of free blocks
        int freeBlockMemory;        // total memory in free blocks

        int numAllocs;
        int numResizes;
        int numFrees;

        public DynamicBlockAlloc(int baseBlockSize, int minBlockSize, int sizeofT, Func<int, T[]> factory)
        {
            this.baseBlockSize = baseBlockSize;
            this.minBlockSize = minBlockSize;
            this.sizeofT = Marshal.SizeOf<T>();
            this.factory = num => Enumerable.Repeat(new T(), num).ToArray();
        }
        public DynamicBlockAlloc()
            => Clear();

        public void Dispose()
            => Shutdown();

        public void Init()
            => freeTree.Init();

        public void Shutdown()
        {
            DynamicBlock<T> block;

            for (block = firstBlock; block != null; block = block.next) if (block.node == null) FreeInternal(block);

            for (block = firstBlock; block != null; block = firstBlock)
            {
                firstBlock = block.next;
                Debug.Assert(block.IsBaseBlock);
                if (pinMemory) block.ValueHandle.Free();
                if (lockMemory) throw new NotImplementedException(); //sys.UnlockMemory(block, block.Size + 0);
                if (block is IDisposable block1) block1.Dispose();
            }

            freeTree.Shutdown();

            Clear();
        }

        public void SetFixedBlocks(int numBlocks)
        {
            DynamicBlock<T> block;

            for (var i = numBaseBlocks; i < numBlocks; i++)
            {
                block = new DynamicBlock<T> { Value = factory(baseBlockSize) }; // block.Memory = block.Value.AsMemory();
                if (pinMemory) block.ValueHandle = GCHandle.Alloc(block.Value);
                if (lockMemory) throw new NotImplementedException(); //sys.LockMemory(block.Buffer, baseBlockSize);
#if DYNAMIC_BLOCK_ALLOC_CHECK
                Array.Copy(blockId, block.id, block.id.Length);
                block.allocator = this;
#endif
                block.SetSize(baseBlockSize, true);
                block.next = null;
                block.prev = lastBlock;
                if (lastBlock != null) lastBlock.next = block;
                else firstBlock = block;
                lastBlock = block;
                block.node = null;

                FreeInternal(block);

                numBaseBlocks++;
                baseBlockMemory += baseBlockSize;
            }

            allowAllocs = false;
        }

        public void SetPinMemory(bool pin)
            => pinMemory = pin;

        public void SetLockMemory(bool @lock)
            => lockMemory = @lock;

        public void FreeEmptyBaseBlocks()
        {
            DynamicBlock<T> block, next;

            for (block = firstBlock; block != null; block = next)
            {
                next = block.next;

                if (block.IsBaseBlock && block.node != null && (next == null || next.IsBaseBlock))
                {
                    UnlinkFreeInternal(block);
                    if (block.prev != null) block.prev.next = block.next;
                    else firstBlock = block.next;
                    if (block.next != null) block.next.prev = block.prev;
                    else lastBlock = block.prev;
                    if (pinMemory) block.ValueHandle = GCHandle.Alloc(block.Value);
                    if (lockMemory) throw new NotImplementedException(); //sys.UnlockMemory(block, block.Size + 0);
                    numBaseBlocks--;
                    baseBlockMemory -= block.Size;
                    if (block is IDisposable block1) block1.Dispose();
                }
            }

#if DYNAMIC_BLOCK_ALLOC_CHECK
            CheckMemory();
#endif
        }

        public DynamicElement<T> Alloc(int num)
        {
            numAllocs++;

            if (num <= 0) return null;

            var block = AllocInternal(num);
            if (block == null) return null;
            block = ResizeInternal(block, num);
            if (block == null) return null;

#if DYNAMIC_BLOCK_ALLOC_CHECK
            CheckMemory();
#endif

            numUsedBlocks++;
            usedBlockMemory += block.Size;
            return block;
        }

        public DynamicElement<T> Resize(DynamicElement<T> ptr, int num)
        {
            numResizes++;

            if (ptr == null) return Alloc(num);
            if (num <= 0) { Free(ptr); return null; }

            var block = (DynamicBlock<T>)ptr;
            usedBlockMemory -= block.Size;

            block = ResizeInternal(block, num);
            if (block == null) return null;

#if DYNAMIC_BLOCK_ALLOC_CHECK
            CheckMemory();
#endif

            usedBlockMemory += block.Size;
            return block;
        }

        public void Free(DynamicElement<T> ptr)
        {
            numFrees++;
            if (ptr == null) return;

            var block = (DynamicBlock<T>)ptr;
            numUsedBlocks--;
            usedBlockMemory -= block.Size;

            FreeInternal(block);

#if DYNAMIC_BLOCK_ALLOC_CHECK
            CheckMemory();
#endif
        }

        public string CheckMemory(DynamicElement<T> ptr)
        {
            if (ptr == null) return null;

            var block = (DynamicBlock<T>)ptr;
            if (block.node != null) return "memory has been freed";
#if DYNAMIC_BLOCK_ALLOC_CHECK
            if (block.id[0] != 0x11111111 || block.id[1] != 0x22222222 || block.id[2] != 0x33333333) return "memory has invalid id";
            if (block.allocator != this) return "memory was allocated with different allocator";
#endif
            return null;
        }

        public int NumBaseBlocks => numBaseBlocks;
        public int BaseBlockMemory => baseBlockMemory;
        public int NumUsedBlocks => numUsedBlocks;
        public int UsedBlockMemory => usedBlockMemory;
        public int NumFreeBlocks => numFreeBlocks;
        public int FreeBlockMemory => freeBlockMemory;
        public int NumEmptyBaseBlocks
        {
            get
            {
                var numEmptyBaseBlocks = 0;
                for (var block = firstBlock; block != null; block = block.next) if (block.IsBaseBlock && block.node != null && (block.next == null || block.next.IsBaseBlock)) numEmptyBaseBlocks++;
                return numEmptyBaseBlocks;
            }
        }

        void Clear()
        {
            firstBlock = lastBlock = null;
            allowAllocs = true;
            pinMemory = false;
            lockMemory = false;
            numBaseBlocks = 0;
            baseBlockMemory = 0;
            numUsedBlocks = 0;
            usedBlockMemory = 0;
            numFreeBlocks = 0;
            freeBlockMemory = 0;
            numAllocs = 0;
            numResizes = 0;
            numFrees = 0;

#if DYNAMIC_BLOCK_ALLOC_CHECK
            blockId[0] = 0x11111111;
            blockId[1] = 0x22222222;
            blockId[2] = 0x33333333;
#endif
        }

        DynamicBlock<T> AllocInternal(int num)
        {
            var alignedBytes = (num * sizeofT + 15) & ~15;

            var block = freeTree.FindSmallestLargerEqual(alignedBytes);
            if (block != null) UnlinkFreeInternal(block);
            else if (allowAllocs)
            {
                var allocSize = Math.Max(baseBlockSize, alignedBytes);
                block = new DynamicBlock<T> { Value = factory(allocSize) }; //block.Memory = block.Value.AsMemory();
                if (pinMemory) block.ValueHandle = GCHandle.Alloc(block.Value);
                if (lockMemory) throw new NotImplementedException(); //sys.LockMemory(block, baseBlockSize);
#if DYNAMIC_BLOCK_ALLOC_CHECK
                Array.Copy(blockId, block.id, block.id.Length);
                block.allocator = this;
#endif
                block.SetSize(allocSize, true);
                block.next = null;
                block.prev = lastBlock;
                if (lastBlock != null) lastBlock.next = block;
                else firstBlock = block;
                lastBlock = block;
                block.node = null;

                numBaseBlocks++;
                baseBlockMemory += allocSize;
            }

            return block;
        }

        DynamicBlock<T> ResizeInternal(DynamicBlock<T> block, int num)
        {
            var alignedBytes = (num * sizeofT + 15) & ~15;

#if DYNAMIC_BLOCK_ALLOC_CHECK
            Debug.Assert(block.id[0] == 0x11111111 && block.id[1] == 0x22222222 && block.id[2] == 0x33333333 && block.allocator == this);
#endif

            // if the new size is larger
            if (alignedBytes > block.Size)
            {
                var nextBlock = block.next;

                // try to annexate the next block if it's free
                if (nextBlock != null && !nextBlock.IsBaseBlock && nextBlock.node != null && block.Size + 0 + nextBlock.Size >= alignedBytes)
                {
                    UnlinkFreeInternal(nextBlock);
                    block.SetSize(block.Size + nextBlock.Size, block.IsBaseBlock);
                    block.next = nextBlock.next;
                    if (nextBlock.next != null) nextBlock.next.prev = block;
                    else lastBlock = block;
                }
                else
                {
                    // allocate a new block and copy
                    var oldBlock = block;
                    block = AllocInternal(num);
                    if (block == null) return null;
                    Array.Copy(oldBlock.Value, block.Value, oldBlock.Size);
                    FreeInternal(oldBlock);
                }
            }

            // if the unused space at the end of this block is large enough to hold a block with at least one element
            if (block.Size - alignedBytes < Math.Max(minBlockSize, sizeofT)) return block;

            var newBlock = new DynamicBlock<T> { Value = block.Value }; //, Memory = block.Value.AsMemory(alignedBytes) };
#if DYNAMIC_BLOCK_ALLOC_CHECK
            Array.Copy(blockId, newBlock.id, newBlock.id.Length);
            newBlock.allocator = this;
#endif
            newBlock.SetSize(block.Size - alignedBytes, false);
            newBlock.next = block.next;
            newBlock.prev = block;
            if (newBlock.next != null) newBlock.next.prev = newBlock;
            else lastBlock = newBlock;
            newBlock.node = null;
            block.next = newBlock;
            block.SetSize(alignedBytes, block.IsBaseBlock);

            FreeInternal(newBlock);

            return block;
        }

        void FreeInternal(DynamicBlock<T> block)
        {
            Debug.Assert(block.node == null);
#if DYNAMIC_BLOCK_ALLOC_CHECK
            Debug.Assert(block.id[0] == 0x11111111 && block.id[1] == 0x22222222 && block.id[2] == 0x33333333 && block.allocator == this);
#endif

            // try to merge with a next free block
            var nextBlock = block.next;
            if (nextBlock != null && !nextBlock.IsBaseBlock && nextBlock.node != null)
            {
                UnlinkFreeInternal(nextBlock);
                block.SetSize(block.Size + 0 + nextBlock.Size, block.IsBaseBlock);
                block.next = nextBlock.next;
                if (nextBlock.next != null) nextBlock.next.prev = block;
                else lastBlock = block;
            }

            // try to merge with a previous free block
            var prevBlock = block.prev;
            if (prevBlock != null && !block.IsBaseBlock && prevBlock.node != null)
            {
                UnlinkFreeInternal(prevBlock);
                prevBlock.SetSize(prevBlock.Size + 0 + block.Size, prevBlock.IsBaseBlock);
                prevBlock.next = block.next;
                if (block.next != null) block.next.prev = prevBlock;
                else lastBlock = prevBlock;
                LinkFreeInternal(prevBlock);
            }
            else LinkFreeInternal(block);
        }

        void LinkFreeInternal(DynamicBlock<T> block)
        {
            block.node = freeTree.Add(block, block.Size);
            numFreeBlocks++;
            freeBlockMemory += block.Size;
        }

        void UnlinkFreeInternal(DynamicBlock<T> block)
        {
            freeTree.Remove(block.node);
            block.node = null;
            numFreeBlocks--;
            freeBlockMemory -= block.Size;
        }

        void CheckMemory()
        {
            for (var block = firstBlock; block != null; block = block.next)
            {
                // make sure the block is properly linked
                if (block.prev == null) Debug.Assert(firstBlock == block);
                else Debug.Assert(block.prev.next == block);
                if (block.next == null) Debug.Assert(lastBlock == block);
                else Debug.Assert(block.next.prev == block);
            }
        }
    }
}