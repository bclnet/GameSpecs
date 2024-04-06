using System.NumericsX.OpenStack.Gngine.Render;
using System.Runtime.InteropServices;
using static System.NumericsX.OpenStack.Gngine.Render.R;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine
{
    unsafe static partial class Gngine
    {
        public static FrameData frameData;
        public static readonly BackEndState backEnd;

        const uint NUM_FRAME_DATA = 2;
        static FrameData[] smpFrameData = new FrameData[NUM_FRAME_DATA];
        static volatile uint smpFrame;

        const int MEMORY_BLOCK_SIZE = 0x100000;

        public static ScreenRect R_ScreenRectFromViewFrustumBounds(Bounds bounds)
            => new()
            {
                x1 = (short)MathX.FtoiFast(0.5f * (1f - bounds[1].y) * (tr.viewDef.viewport.x2 - tr.viewDef.viewport.x1)),
                x2 = (short)MathX.FtoiFast(0.5f * (1f - bounds[0].y) * (tr.viewDef.viewport.x2 - tr.viewDef.viewport.x1)),
                y1 = (short)MathX.FtoiFast(0.5f * (1f + bounds[0].z) * (tr.viewDef.viewport.y2 - tr.viewDef.viewport.y1)),
                y2 = (short)MathX.FtoiFast(0.5f * (1f + bounds[1].z) * (tr.viewDef.viewport.y2 - tr.viewDef.viewport.y1))
            };

        static Vector4[] R_ShowColoredScreenRect_colors = { colorRed, colorGreen, colorBlue, colorYellow, colorMagenta, colorCyan, colorWhite, colorPurple };
        public static void R_ShowColoredScreenRect(ScreenRect rect, int colorIndex)
        {
            if (!rect.IsEmpty) tr.viewDef.renderWorld.DebugScreenRect(R_ShowColoredScreenRect_colors[colorIndex & 7], rect, tr.viewDef);
        }

        public static void R_ToggleSmpFrame()
        {
            if (r_lockSurfaces.Bool) return;

            smpFrame++;
            frameData = smpFrameData[smpFrame % NUM_FRAME_DATA];

            R_FreeDeferredTriSurfs(frameData);

            // clear frame-temporary data
            FrameData frame; FrameMemoryBlock* block;

            // update the highwater mark
            R_CountFrameData();

            frame = frameData;

            // reset the memory allocation to the first block
            frame.alloc = frame.memory;

            // clear all the blocks
            for (block = frame.memory; block != null; block = block->next) block->used = 0;

            R_ClearCommandChain();
        }

        public static void R_ShutdownFrameData()
        {
            FrameData frame; FrameMemoryBlock* block;

            for (var n = 0; n < NUM_FRAME_DATA; n++)
            {
                // free any current data
                frame = smpFrameData[n];
                if (frame == null) continue;

                R_FreeDeferredTriSurfs(frame);

                FrameMemoryBlock* nextBlock;
                for (block = frame.memory; block != null; block = nextBlock)
                {
                    nextBlock = block->next;
                    Marshal.FreeHGlobal((IntPtr)block);
                }
                smpFrameData[n] = null;
            }
            frameData = null;
        }

        public static void R_InitFrameData()
        {
            int size; FrameData frame; FrameMemoryBlock* block;

            R_ShutdownFrameData();

            for (var n = 0; n < NUM_FRAME_DATA; n++)
            {
                smpFrameData[n] = new FrameData();
                frame = smpFrameData[n];
                size = MEMORY_BLOCK_SIZE;
                block = (FrameMemoryBlock*)Marshal.AllocHGlobal(size + sizeof(FrameMemoryBlock));
                if (block == null) common.FatalError("R_InitFrameData: Mem_Alloc() failed");
                block->size = size;
                block->used = 0;
                block->next = null;
                frame.memory = block;
                frame.memoryHighwater = 0;
            }

            smpFrame = 0;
            frameData = smpFrameData[0];

            R_ToggleSmpFrame();
        }

        public static int R_CountFrameData()
        {
            FrameData frame; FrameMemoryBlock* block; int count;

            count = 0;
            frame = frameData;
            for (block = frame.memory; block != null; block = block->next)
            {
                count += block->used;
                if (block == frame.alloc) break;
            }

            // note if this is a new highwater mark
            if (count > frame.memoryHighwater) frame.memoryHighwater = count;

            return count;
        }

        public static void* R_StaticAlloc(int bytes)
        {
            tr.pc.c_alloc++;
            tr.staticAllocCount += bytes;

            var buf = (void*)Marshal.AllocHGlobal(bytes);
            // don't exit on failure on zero length allocations since the old code didn't
            if (buf == null && bytes != 0) common.FatalError($"R_StaticAlloc failed on {bytes} bytes");
            return buf;
        }
        public static void* R_ClearedStaticAlloc(int bytes)
        {
            var buf = R_StaticAlloc(bytes);
            Simd.Memset(buf, 0, bytes);
            return buf;
        }
        public static T R_ClearedStaticAllocT<T>() where T : new()
        {
            tr.pc.c_alloc++;
            tr.staticAllocCount += Marshal.SizeOf<T>();
            return new T();
        }

        public static void R_StaticFree(void* data)
        {
            tr.pc.c_free++;
            Marshal.FreeHGlobal((IntPtr)data);
        }
        public static void R_StaticFreeT<T>(ref T data)
        {
            tr.pc.c_free++;
            data = default;
        }

        // This data will be automatically freed when the current frame's back end completes.
        // This should only be called by the front end. The back end shouldn't need to allocate memory.
        // If we passed smpFrame in, the back end could alloc memory, because it will always be a different frameData than the front end is using.
        // All temporary data, like dynamic tesselations and local spaces are allocated here.
        // The memory will not move, but it may not be contiguous with previous allocations even from this frame.
        // The memory is NOT zero filled. Should part of this be inlined in a macro?
        public static void* R_FrameAlloc(int bytes)
        {
            FrameData frame; FrameMemoryBlock* block; void* buf;

            bytes = (bytes + 16) & ~15;
            // see if it can be satisfied in the current block
            frame = frameData;
            block = frame.alloc;

            if (block->size - block->used >= bytes)
            {
                buf = &block->base_ + block->used;
                block->used += bytes;
                return buf;
            }
            // advance to the next memory block if available
            block = block->next;
            // create a new block if we are at the end of the chain
            if (block == null)
            {
                int size;

                size = MEMORY_BLOCK_SIZE;
                block = (FrameMemoryBlock*)Marshal.AllocHGlobal(size + sizeof(FrameMemoryBlock));
                if (block == null) common.FatalError("R_FrameAlloc: Mem_Alloc() failed");
                block->size = size;
                block->used = 0;
                block->next = null;
                frame.alloc->next = block;
            }

            // we could fix this if we needed to...
            if (bytes > block->size) common.FatalError($"R_FrameAlloc of {bytes} exceeded MEMORY_BLOCK_SIZE");

            frame.alloc = block;

            block->used = bytes;

            return &block->base_;
        }

        public static T R_ClearedFrameAllocT<T>() where T : new()
            => new();

        public static void* R_ClearedFrameAlloc(int bytes)
        {
            var r = R_FrameAlloc(bytes);
            Simd.Memset(r, 0, bytes);
            return r;
        }

        // This does nothing at all, as the frame data is reused every frame and can only be stack allocated.
        // The only reason for it's existance is so functions that can use either static or frame memory can set function pointers
        // to both alloc and free.
        public static void R_FrameFree(void* data) { }
    }
}