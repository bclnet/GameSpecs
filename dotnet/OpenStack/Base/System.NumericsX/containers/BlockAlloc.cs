using System.Linq;

namespace System.NumericsX
{
    public class BlockAllocElement<T>
    {
        internal BlockAllocElement<T> _next;
    }

    public class BlockAlloc<T> where T : BlockAllocElement<T>, new()
    {
        class Block
        {
            public BlockAllocElement<T>[] elements;
            public Block next;
        }

        int blockSize;
        BlockAllocElement<T> model;
        Block blocks;
        BlockAllocElement<T> free;
        int total;
        int active;

        public BlockAlloc(int blockSize)
        {
            this.blockSize = blockSize;
            this.model = new T();
        }

        public void Shutdown()
        {
            while (blocks != null)
            {
                var block = blocks;
                blocks = blocks.next;
                if (block is IDisposable block1) block1.Dispose();
            }
            blocks = null;
            free = null;
            total = active = 0;
        }

        public T Alloc()
        {
            if (free == null)
            {
                var block = new Block
                {
                    elements = Enumerable.Repeat(model, blockSize).ToArray(),
                    next = blocks
                };
                blocks = block;
                for (var i = 0; i < blockSize; i++)
                {
                    block.elements[i]._next = free;
                    free = block.elements[i];
                }
                total += blockSize;
            }
            active++;
            var element = free;
            free = free._next;
            element._next = null;
            return (T)element;
        }

        public void Free(T value)
        {
            var element = (BlockAllocElement<T>)value;
            element._next = free;
            free = element;
            active--;
        }

        public int TotalCount => total;
        public int AllocCount => active;
        public int FreeCount => total - active;
    }
}