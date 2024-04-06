using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WaveEngine.Bindings.OpenGLES;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.Gngine.Render.QGL;
using static System.NumericsX.OpenStack.Gngine.Render.R;
using static System.NumericsX.OpenStack.OpenStack;
using static WaveEngine.Bindings.OpenGLES.GL;

// vertex cache calls should only be made by the front end
namespace System.NumericsX.OpenStack.Gngine.Render
{
    public class VertCache : BlockAllocElement<VertCache>
    {
        public uint vbo;
        public bool indexBuffer;        // holds indexes instead of vertexes
        public nint offset;
        public int size;                // may be larger than the amount asked for, due to round up and minimum fragment sizes
        public VertexCacheX.TAG tag;                 // a tag of 0 is a free block
        public VertCache user;         // will be set to zero when purged
        public VertCache next, prev;   // may be on the static list or one of the frame lists
        public int frameUsed;           // it can't be purged if near the current frame
        public IntPtr frontEndMemory;
        public bool frontEndMemoryDirty;
    }

    public unsafe class VertexCacheX
    {
        public enum TAG : int
        {
            FREE,
            USED,
            FIXED,    // for the temp buffers
            TEMP    // in frame temp area, not static area
        }

        public const int NUM_VERTEX_FRAMES = 2;
        const int FRAME_MEMORY_BYTES = 0x200000;
        const int EXPAND_HEADERS = 1024;

        static readonly CVar r_showVertexCache = new("r_showVertexCache", "0", CVAR.INTEGER | CVAR.RENDERER, "");
        static readonly CVar r_vertexBufferMegs = new("r_vertexBufferMegs", "128", CVAR.INTEGER | CVAR.RENDERER, "");
        static readonly CVar r_freeVertexBuffer = new("r_freeVertexBuffer", "1", CVAR.BOOL | CVAR.RENDERER, "");

        void R_ListVertexCache_f(CmdArgs args)
            => vertexCache.List();

        int staticCountTotal;
        int staticAllocTotal;    // for end of frame purging

        int staticAllocThisFrame;  // debug counter
        int staticCountThisFrame;
        int[] dynamicAllocThisFrame = new int[NUM_VERTEX_FRAMES];
        int dynamicCountThisFrame;
        int staticAllocThisFrame_Index;  // for Index buffers
        int staticCountThisFrame_Index;
        int[] dynamicAllocThisFrame_Index = new int[NUM_VERTEX_FRAMES];
        int dynamicCountThisFrame_Index;

        int currentFrame;      // for purgable block tracking
        int listNum;        // currentFrame % NUM_VERTEX_FRAMES, determines which tempBuffers to use

        int staticAllocMaximum;
        int dynamicAllocMaximum;
        int dynamicAllocMaximum_Index;

        uint vboMax;

        VertCache[] tempBuffers = new VertCache[NUM_VERTEX_FRAMES];    // allocated at startup
        VertCache[] tempIndexBuffers = new VertCache[NUM_VERTEX_FRAMES];    // allocated at startup (for Index buffers)

        bool tempOverflow;      // had to alloc a temp in static memory

        BlockAlloc<VertCache> headerAllocator = new BlockAlloc<VertCache>(1024);

        VertCache freeStaticHeaders;    // head of doubly linked list
        VertCache freeStaticIndexHeaders;    // head of doubly linked list

        VertCache freeDynamicHeaders;    // head of doubly linked list
        VertCache freeDynamicIndexHeaders;    // head of doubly linked list (Index buffers)

        VertCache[] dynamicHeaders = new VertCache[NUM_VERTEX_FRAMES];      // head of doubly linked list
        VertCache[] dynamicIndexHeaders = new VertCache[NUM_VERTEX_FRAMES];      // head of doubly linked list (Index buffers)

        VertCache staticHeaders;      // head of doubly linked list in MRU order,
        VertCache staticIndexHeaders;      // head of doubly linked list in MRU order,

        VertCache[] deferredFreeList = new VertCache[NUM_VERTEX_FRAMES];    // head of doubly linked list

        int frameBytes;        // for each of NUM_VERTEX_FRAMES frames

        uint currentBoundVBO;
        uint currentBoundVBO_Index;

        unsafe void ActuallyFree(VertCache block)
        {
            if (block == null) common.Error("VertexCache Free: NULL pointer");

            // let the owner know we have purged it
            if (block.user != null) { block.user = null; }

            // temp blocks are in a shared space that won't be freed
            if (block.tag != TAG.TEMP)
            {
                staticAllocTotal -= block.size;
                staticCountTotal--;

                if (block.vbo != VBOEmpty && r_freeVertexBuffer.Bool)
                {
                    if (block.indexBuffer)
                    {
                        if (block.vbo != currentBoundVBO_Index) qglBindBuffer(BufferTargetARB.ElementArrayBuffer, block.vbo);
                        glBindBuffer(BufferTargetARB.ElementArrayBuffer, block.vbo);
                        glBufferData(BufferTargetARB.ElementArrayBuffer, 0, null, BufferUsageARB.StreamDraw);
                        glBindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
                        //glDeleteBuffers(1, &block.vbo); block.vbo = VBOEmpty; // Doing this makes it slow AF
                        currentBoundVBO_Index = VBOEmpty;
                    }
                    else
                    {
                        if (block.vbo != currentBoundVBO) qglBindBuffer(BufferTargetARB.ArrayBuffer, block.vbo);
                        glBindBuffer(BufferTargetARB.ArrayBuffer, block.vbo);
                        glBufferData(BufferTargetARB.ArrayBuffer, 0, null, BufferUsageARB.StreamDraw);
                        glBindBuffer(BufferTargetARB.ArrayBuffer, 0);
                        //glDeleteBuffers(1, &block.vbo); block.vbo = VBOEmpty; // Doing this makes it slow AF
                        currentBoundVBO = VBOEmpty;
                    }
                }
            }

            block.tag = TAG.FREE;      // mark as free

            if (block.frontEndMemory != IntPtr.Zero) { Marshal.FreeHGlobal(block.frontEndMemory); block.frontEndMemory = IntPtr.Zero; }

            block.frontEndMemoryDirty = true;

            // unlink stick it back on the free list
            block.next.prev = block.prev;
            block.prev.next = block.next;

            // stick it on the front of the free list so it will be reused immediately
            if (block.indexBuffer) { block.next = freeStaticIndexHeaders.next; block.prev = freeStaticIndexHeaders; }
            else { block.next = freeStaticHeaders.next; block.prev = freeStaticHeaders; }

            block.next.prev = block;
            block.prev.next = block;
        }

        public void Init()
        {
            common.Printf("Init Vertex Cache\n");

            cmdSystem.AddCommand("listVertexCache", R_ListVertexCache_f, CMD_FL.RENDERER, "lists vertex cache");

            currentBoundVBO = VBOEmpty;
            currentBoundVBO_Index = VBOEmpty;

            if (r_vertexBufferMegs.Integer < 8) r_vertexBufferMegs.Integer = 8;

            // initialize the cache memory blocks
            freeStaticHeaders.next = freeStaticHeaders.prev = freeStaticHeaders;
            staticHeaders.next = staticHeaders.prev = staticHeaders;
            freeStaticIndexHeaders.next = freeStaticIndexHeaders.prev = freeStaticIndexHeaders;
            staticIndexHeaders.next = staticIndexHeaders.prev = staticIndexHeaders;

            freeDynamicHeaders.next = freeDynamicHeaders.prev = freeDynamicHeaders;
            freeDynamicIndexHeaders.next = freeDynamicIndexHeaders.prev = freeDynamicIndexHeaders;

            // set up the dynamic frame memory
            frameBytes = FRAME_MEMORY_BYTES;
            staticAllocTotal = 0;
            staticCountTotal = 0;

            staticAllocMaximum = 0;
            dynamicAllocMaximum = 0;
            dynamicAllocMaximum_Index = 0;

            vboMax = 0;

            // Allocate the temporary buffers (number of temporary buffers is NUM_VERTEX_FRAMES)
            for (var i = 0; i < NUM_VERTEX_FRAMES; i++)
            {
                tempBuffers[i] = CreateTempVbo(frameBytes, false);
                tempIndexBuffers[i] = CreateTempVbo(frameBytes, true);
                dynamicHeaders[i].next = dynamicHeaders[i].prev = dynamicHeaders[i];
                dynamicIndexHeaders[i].next = dynamicIndexHeaders[i].prev = dynamicIndexHeaders[i];
                deferredFreeList[i].next = deferredFreeList[i].prev = deferredFreeList[i];
            }

            EndFrame();
        }

        public void Shutdown()
        {
            //PurgeAll();	// !@#: also purge the temp buffers

            headerAllocator.Shutdown();

            currentBoundVBO = VBOEmpty;
            currentBoundVBO_Index = VBOEmpty;
        }

        // called when vertex programs are enabled or disabled, because the cached data is no longer valid
        // Used when toggling vertex programs on or off, because the cached data isn't valid
        public void PurgeAll()
        {
            while (staticHeaders.next != staticHeaders) ActuallyFree(staticHeaders.next);
            while (staticIndexHeaders.next != staticIndexHeaders) ActuallyFree(staticIndexHeaders.next);

            currentBoundVBO = VBOEmpty;
            currentBoundVBO_Index = VBOEmpty;
        }

        // Tries to allocate space for the given data in fast vertex memory, and copies it over.
        // Alloc does NOT do a touch, which allows purging of things created at level load time even if a frame hasn't passed yet.
        // These allocations can be purged, which will zero the pointer.
        public void Alloc(void* data, int size, out VertCache buffer, bool indexBuffer)
        {
            VertCache block;

            if (size <= 0) common.Error($"VertexCache::Alloc: size = {size}\n");

            // if we can't find anything, it will be NULL
            buffer = null;

            if (indexBuffer)
            {
                // if we don't have any remaining unused headers, allocate some more
                if (freeStaticIndexHeaders.next == freeStaticIndexHeaders)
                    for (var i = 0; i < EXPAND_HEADERS; i++)
                    {
                        block = headerAllocator.Alloc();
                        block.next = freeStaticIndexHeaders.next;
                        block.prev = freeStaticIndexHeaders;
                        block.next.prev = block;
                        block.prev.next = block;
                        block.frontEndMemory = IntPtr.Zero;
                        block.frontEndMemoryDirty = true;
                        block.vbo = VBOEmpty;
                    }
            }
            else
            {
                // if we don't have any remaining unused headers, allocate some more
                if (freeStaticHeaders.next == freeStaticHeaders)
                    for (var i = 0; i < EXPAND_HEADERS; i++)
                    {
                        block = headerAllocator.Alloc();
                        block.next = freeStaticHeaders.next;
                        block.prev = freeStaticHeaders;
                        block.next.prev = block;
                        block.prev.next = block;
                        block.frontEndMemory = IntPtr.Zero;
                        block.frontEndMemoryDirty = true;
                        block.vbo = VBOEmpty;
                    }
            }

            if (indexBuffer)
            {
                // move it from the freeStaticIndexHeaders list to the staticHeaders list
                block = freeStaticIndexHeaders.next;
                block.next.prev = block.prev;
                block.prev.next = block.next;
                block.next = staticIndexHeaders.next;
                block.prev = staticIndexHeaders;
                block.next.prev = block;
                block.prev.next = block;
            }
            else
            {
                // move it from the freeStaticHeaders list to the staticHeaders list
                block = freeStaticHeaders.next;
                block.next.prev = block.prev;
                block.prev.next = block.next;
                block.next = staticHeaders.next;
                block.prev = staticHeaders;
                block.next.prev = block;
                block.prev.next = block;
            }

            block.offset = IntPtr.Zero;
            block.tag = TAG.USED;

            // save data for debugging
            if (indexBuffer) { staticAllocThisFrame_Index += block.size; staticCountThisFrame_Index++; }
            else { staticAllocThisFrame += block.size; staticCountThisFrame++; }
            staticCountTotal++;
            staticAllocTotal += size;
            if (staticAllocTotal > staticAllocMaximum) staticAllocMaximum = staticAllocTotal;

            // this will be set to zero when it is purged
            block.user = buffer;
            buffer = block;

            // allocation doesn't imply used-for-drawing, because at level load time lots of things may be created, but they aren't
            // referenced by the GPU yet, and can be purged if needed.
            block.frameUsed = currentFrame - NUM_VERTEX_FRAMES;

            block.indexBuffer = indexBuffer;

            // TODO, make this more efficient...
            if (block.frontEndMemory != IntPtr.Zero) Marshal.FreeHGlobal(block.frontEndMemory);
            block.frontEndMemory = Marshal.AllocHGlobal(size + 16);
            block.size = size;
            Unsafe.CopyBlock((byte*)block.frontEndMemory, data, (uint)size);
            block.frontEndMemoryDirty = true;

            //Position(block);
        }

        // This will be a real pointer with virtual memory, but it will be an int offset cast to a pointer of ARB_vertex_buffer_object
        // this will be a real pointer with virtual memory, but it will be an int offset cast to a pointer with ARB_vertex_buffer_object
        // The ARB_vertex_buffer_object will be bound
        public void* Position(VertCache buffer)
        {
            if (buffer == null || buffer.tag == TAG.FREE) common.FatalError("VertexCache::Position: bad vertCache_t");

            if (buffer.indexBuffer && !R.r_useIndexVBO.Bool) { UnbindIndex(); return (byte*)buffer.frontEndMemory + buffer.offset; }
            else if (!buffer.indexBuffer && !R.r_useVertexVBO.Bool) { UnbindVertex(); return (byte*)buffer.frontEndMemory + buffer.offset; }

            // Create VBO if does not exist
            if (buffer.vbo == VBOEmpty)
            {
                if (buffer.frontEndMemory == IntPtr.Zero) Console.WriteLine("MEMORY NULL");
                qglGenBuffers(1, out buffer.vbo);

                if (buffer.vbo > vboMax) vboMax = buffer.vbo;
            }

            // the ARB vertex object just uses an offset
            if (r_showVertexCache.Integer == 2)
                common.Printf(buffer.tag == TAG.TEMP
                    ? $"GL_ARRAY_BUFFER_ARB = {buffer.vbo} + {buffer.offset} ({buffer.size} bytes)\n"
                    : $"GL_ARRAY_BUFFER_ARB = {buffer.vbo} ({buffer.size} bytes)\n");
            if (buffer.indexBuffer)
            {
                if (buffer.vbo != currentBoundVBO_Index) { qglBindBuffer(BufferTargetARB.ElementArrayBuffer, buffer.vbo); currentBoundVBO_Index = buffer.vbo; }
            }
            else
            {
                if (buffer.vbo != currentBoundVBO) { qglBindBuffer(BufferTargetARB.ArrayBuffer, buffer.vbo); currentBoundVBO = buffer.vbo; }
            }

            // Update any new data
            if (buffer.frontEndMemoryDirty)
            {
                //Console.Write("Uploading Static vertex");
                if (buffer.indexBuffer) qglBufferData(BufferTargetARB.ElementArrayBuffer, buffer.size, (void*)buffer.frontEndMemory, BufferUsageARB.StaticDraw);
                else qglBufferData(BufferTargetARB.ArrayBuffer, buffer.size, (void*)buffer.frontEndMemory, BufferUsageARB.StaticDraw);
                buffer.frontEndMemoryDirty = false;
            }

            return (void*)buffer.offset;
        }

        public unsafe VertCache CreateTempVbo(int bytes, bool indexBuffer)
        {
            VertCache block = headerAllocator.Alloc();

            block.next = null;
            block.prev = null;
            block.frontEndMemory = IntPtr.Zero;
            block.offset = IntPtr.Zero;
            block.tag = TAG.FIXED;
            block.indexBuffer = indexBuffer;
            block.frontEndMemoryDirty = false;

#if USE_MAP
#else
            block.frontEndMemory = Marshal.AllocHGlobal(bytes + 16);
#endif

            qglGenBuffers(1, out block.vbo);

            if (indexBuffer)
            {
                qglBindBuffer(BufferTargetARB.ElementArrayBuffer, block.vbo);
                currentBoundVBO_Index = block.vbo;
                qglBufferData(BufferTargetARB.ElementArrayBuffer, bytes, null, BufferUsageARB.StaticDraw);
            }
            else
            {
                qglBindBuffer(BufferTargetARB.ArrayBuffer, block.vbo);
                currentBoundVBO = block.vbo;
                qglBufferData(BufferTargetARB.ArrayBuffer, bytes, null, BufferUsageARB.StaticDraw);
            }

            return block;
        }

        // automatically freed at the end of the next frame used for specular texture coordinates and gui drawing, which will change every frame.
        // will return NULL if the vertex cache is completely full As with Position(), this may not actually be a pointer you can access.
        //
        // A frame temp allocation must never be allowed to fail due to overflow. We can't simply sync with the GPU and overwrite what we have, because
        // there may still be future references to dynamically created surfaces.
        public VertCache AllocFrameTemp(void* data, int size, bool indexBuffer)
        {
            VertCache block;

            if (size <= 0) common.Error($"VertexCache::AllocFrameTemp: size = {size}\n");

            if (indexBuffer)
            {
                if (dynamicAllocThisFrame_Index[listNum] + size > frameBytes)
                {
                    Console.WriteLine("WARNING DYNAMIC OVERFLOW!!");
                    // if we don't have enough room in the temp block, allocate a static block, but immediately free it so it will get freed at the next frame
                    tempOverflow = true;
                    Alloc(data, size, out block, indexBuffer);
                    Free(ref block);
                    return block;
                }
            }
            else
            {
                if (dynamicAllocThisFrame[listNum] + size > frameBytes)
                {
                    Console.WriteLine("WARNING DYNAMIC OVERFLOW!!");
                    // if we don't have enough room in the temp block, allocate a static block, but immediately free it so it will get freed at the next frame
                    tempOverflow = true;
                    Alloc(data, size, out block, indexBuffer);
                    Free(ref block);
                    return block;
                }
            }

            // this data is just going on the shared dynamic list

            if (indexBuffer)
            {
                // if we don't have any remaining unused headers, allocate some more
                if (freeDynamicIndexHeaders.next == freeDynamicIndexHeaders)
                    for (var i = 0; i < EXPAND_HEADERS; i++)
                    {
                        block = headerAllocator.Alloc();
                        block.next = freeDynamicIndexHeaders.next;
                        block.prev = freeDynamicIndexHeaders;
                        block.next.prev = block;
                        block.prev.next = block;
                    }
            }
            else
            {
                // if we don't have any remaining unused headers, allocate some more
                if (freeDynamicHeaders.next == freeDynamicHeaders)
                    for (var i = 0; i < EXPAND_HEADERS; i++)
                    {
                        block = headerAllocator.Alloc();
                        block.next = freeDynamicHeaders.next;
                        block.prev = freeDynamicHeaders;
                        block.next.prev = block;
                        block.prev.next = block;
                    }
            }

            if (indexBuffer)
            {
                // move it from the freeIndexDynamicHeaders list to the dynamicIndexHeaders list
                block = freeDynamicIndexHeaders.next;
                block.next.prev = block.prev;
                block.prev.next = block.next;
                block.next = dynamicIndexHeaders[listNum].next;
                block.prev = dynamicIndexHeaders[listNum];
                block.next.prev = block;
                block.prev.next = block;

            }
            else
            {
                // move it from the freeDynamicHeaders list to the dynamicHeaders list
                block = freeDynamicHeaders.next;
                block.next.prev = block.prev;
                block.prev.next = block.next;
                block.next = dynamicHeaders[listNum].next;
                block.prev = dynamicHeaders[listNum];
                block.next.prev = block;
                block.prev.next = block;
            }

            block.frontEndMemory = IntPtr.Zero;
            block.frontEndMemoryDirty = false;

            // Try to align, might be faster
            unchecked { size += 16; size &= (int)0xFFFFFFF0; }
            block.size = size;

            block.tag = TAG.TEMP;
            block.indexBuffer = indexBuffer;
            if (indexBuffer)
            {
                block.offset = dynamicAllocThisFrame_Index[listNum];
                dynamicAllocThisFrame_Index[listNum] += block.size;
                dynamicCountThisFrame_Index++;
            }
            else
            {
                block.offset = dynamicAllocThisFrame[listNum];
                dynamicAllocThisFrame[listNum] += block.size;
                dynamicCountThisFrame++;
            }

            block.user = null;
            block.frameUsed = 0;

            // copy the data
            if (indexBuffer)
            {
                block.vbo = tempIndexBuffers[listNum].vbo;
                Unsafe.CopyBlock((byte*)tempIndexBuffers[listNum].frontEndMemory + block.offset, data, (uint)size);
                block.frontEndMemory = tempIndexBuffers[listNum].frontEndMemory;
            }
            else
            {
                block.vbo = tempBuffers[listNum].vbo;
                Unsafe.CopyBlock((byte*)tempBuffers[listNum].frontEndMemory + block.offset, data, (uint)size);
                block.frontEndMemory = tempBuffers[listNum].frontEndMemory;
            }

            return block;
        }

        // notes that a buffer is used this frame, so it can't be purged out from under the GPU
        public void Touch(VertCache block)
        {
            if (block == null) common.Error("VertexCache Touch: NULL pointer");
            if (block.tag == TAG.FREE) common.FatalError("VertexCache Touch: freed pointer");
            if (block.tag == TAG.TEMP) common.FatalError("VertexCache Touch: temporary pointer");

            block.frameUsed = currentFrame;

            // move to the head of the LRU list
            block.next.prev = block.prev;
            block.prev.next = block.next;

            if (block.indexBuffer)
            {
                block.next = staticIndexHeaders.next;
                block.prev = staticIndexHeaders;
                staticIndexHeaders.next.prev = block;
                staticIndexHeaders.next = block;
            }
            else
            {
                block.next = staticHeaders.next;
                block.prev = staticHeaders;
                staticHeaders.next.prev = block;
                staticHeaders.next = block;
            }
        }

        // this block won't have to zero a buffer pointer when it is purged, but it must still wait for the frames to pass, in case the GPU is still referencing it
        public void Free(ref VertCache block)
        {
            if (block == null) return;
            if (block.tag == TAG.FREE) common.FatalError("VertexCache Free: freed pointer");
            if (block.tag == TAG.TEMP) common.FatalError("VertexCache Free: temporary pointer");

            // this block still can't be purged until the frame count has expired, but it won't need to clear a user pointer when it is
            block.user = null;

            block.next.prev = block.prev;
            block.prev.next = block.next;

            block.next = deferredFreeList[listNum].next;
            block.prev = deferredFreeList[listNum];
            deferredFreeList[listNum].next.prev = block;
            deferredFreeList[listNum].next = block;
        }

        // updates the counter for determining which temp space to use and which blocks can be purged Also prints debugging info when enabled
        static bool EndFrame_once = true;
        public void EndFrame()
        {
            VertCache block;

            // display debug information
            if (r_showVertexCache.Bool)
            {
                int staticUseCount = 0, staticUseSize = 0;
                for (block = staticHeaders.next; block != staticHeaders; block = block.next) if (block.frameUsed == currentFrame) { staticUseCount++; staticUseSize += block.size; }

                var frameOverflow = tempOverflow ? "(OVERFLOW)" : string.Empty;

                common.Printf($"vertex dynamic:{dynamicCountThisFrame + dynamicCountThisFrame_Index}={(dynamicAllocThisFrame[listNum] + dynamicAllocThisFrame_Index[listNum]) / 1024}k{frameOverflow}, " +
                    $"static alloc:{staticCountThisFrame + staticCountThisFrame_Index}={(staticAllocThisFrame + staticAllocThisFrame_Index) / 1024}k used:{staticUseCount}={staticUseSize / 1024}k total:{staticCountTotal}={staticAllocTotal / 1024}k\n");
            }

            if (EndFrame_once && staticAllocTotal > r_vertexBufferMegs.Integer * 1024 * 1024) { common.Printf($"VBO size exceeds {r_vertexBufferMegs.Integer}MB. Consider updating r_vertexBufferMegs.\n"); EndFrame_once = false; }

#if false
            // if our total static count is above our working memory limit, start purging things
            while (staticAllocTotal > r_vertexBufferMegs.Integer * 1024 * 1024) { } // free the least recently used
#endif
            currentFrame = tr.frameCount;

            listNum = currentFrame % NUM_VERTEX_FRAMES;

            staticAllocThisFrame = 0;
            staticCountThisFrame = 0;
            staticAllocThisFrame_Index = 0;
            staticCountThisFrame_Index = 0;
            dynamicAllocThisFrame_Index[listNum] = 0;
            dynamicCountThisFrame_Index = 0;
            dynamicAllocThisFrame[listNum] = 0;
            dynamicCountThisFrame = 0;
            tempOverflow = false;

            // free all the deferred free headers
            while (deferredFreeList[listNum].next != deferredFreeList[listNum]) ActuallyFree(deferredFreeList[listNum].next);

            // free all the frame temp headers
            block = dynamicHeaders[listNum].next;
            if (block != dynamicHeaders[listNum])
            {
                block.prev = freeDynamicHeaders;
                dynamicHeaders[listNum].prev.next = freeDynamicHeaders.next;
                freeDynamicHeaders.next.prev = dynamicHeaders[listNum].prev;
                freeDynamicHeaders.next = block;

                dynamicHeaders[listNum].next = dynamicHeaders[listNum].prev = dynamicHeaders[listNum];
            }

            block = dynamicIndexHeaders[listNum].next;
            if (block != dynamicIndexHeaders[listNum])
            {
                block.prev = freeDynamicIndexHeaders;
                dynamicIndexHeaders[listNum].prev.next = freeDynamicIndexHeaders.next;
                freeDynamicIndexHeaders.next.prev = dynamicIndexHeaders[listNum].prev;
                freeDynamicIndexHeaders.next = block;

                dynamicIndexHeaders[listNum].next = dynamicIndexHeaders[listNum].prev = dynamicIndexHeaders[listNum];
            }
#if false
            if (currentFrame % 60 == 0) common.Printf($"Current static = {staticAllocTotal}, Max static = {staticAllocMaximum:08}, Max dynamic = {dynamicAllocMaximum:08}, Max dynamicI = {dynamicAllocMaximum_Index:08}, vboMax = {vboMax}\n");
#endif
        }

        public const uint GL_MAP_INVALIDATE_BUFFER_BIT = 0x0008;
        public const uint GL_MAP_INVALIDATE_RANGE_BIT = 0x0004;
        public const uint GL_MAP_UNSYNCHRONIZED_BIT = 0x0020;
        public const uint GL_MAP_WRITE_BIT = 0x0002;

        public unsafe void BeginBackEnd(int which)
        {
            //LOGI("BeginBackEnd list = %d, size index = %d, size = %d", listNum,dynamicAllocThisFrame_Index,dynamicAllocThisFrame);

#if USE_MAP
            qglBindBuffer(BufferTargetARB.ElementArrayBuffer, tempIndexBuffers[which].vbo);
            currentBoundVBO_Index = tempIndexBuffers[which].vbo;
            qglUnmapBuffer(BufferTargetARB.ElementArrayBuffer);

            currentBoundVBO_Index = tempIndexBuffers[(which + 1) % NUM_VERTEX_FRAMES].vbo;
            qglBindBuffer(BufferTargetARB.ElementArrayBuffer, currentBoundVBO_Index);
            tempIndexBuffers[(which + 1) % NUM_VERTEX_FRAMES].frontEndMemory = qglMapBufferRange(GL_ELEMENT_ARRAY_BUFFER, 0, FRAME_MEMORY_BYTES, GL_MAP_WRITE_BIT | GL_MAP_INVALIDATE_RANGE_BIT | GL_MAP_UNSYNCHRONIZED_BIT);
#else
            if (R.r_useIndexVBO.Bool)
            {
                qglBindBuffer(BufferTargetARB.ElementArrayBuffer, tempIndexBuffers[which].vbo);
                currentBoundVBO_Index = tempIndexBuffers[which].vbo;
                //qglBufferSubData(BufferTargetARB.ElementArrayBuffer, 0, dynamicAllocThisFrame_Index[which], tempIndexBuffers[which].frontEndMemory);
                qglBufferData(BufferTargetARB.ElementArrayBuffer, dynamicAllocThisFrame_Index[which], (void*)tempIndexBuffers[which].frontEndMemory, BufferUsageARB.StreamDraw);
            }
#endif

#if USE_MAP
            qglBindBuffer(GL_ARRAY_BUFFER, tempBuffers[which].vbo);
            currentBoundVBO = tempBuffers[which].vbo;
            qglUnmapBuffer(GL_ARRAY_BUFFER);
            currentBoundVBO = tempBuffers[(which + 1) % NUM_VERTEX_FRAMES].vbo;

            qglBindBuffer(GL_ARRAY_BUFFER, currentBoundVBO);
            tempBuffers[(which + 1) % NUM_VERTEX_FRAMES].frontEndMemory = qglMapBufferRange(GL_ARRAY_BUFFER, 0, FRAME_MEMORY_BYTES, GL_MAP_WRITE_BIT | GL_MAP_INVALIDATE_RANGE_BIT | GL_MAP_UNSYNCHRONIZED_BIT);
#else
            if (R.r_useVertexVBO.Bool)
            {
                qglBindBuffer(BufferTargetARB.ArrayBuffer, tempBuffers[which].vbo);
                currentBoundVBO = tempBuffers[which].vbo;
                //qglBufferSubData(BufferTargetARB.ArrayBuffer, 0, dynamicAllocThisFrame[which], tempBuffers[which].frontEndMemory);
                qglBufferData(BufferTargetARB.ArrayBuffer, dynamicAllocThisFrame[which], (void*)tempBuffers[which].frontEndMemory, BufferUsageARB.StreamDraw);
            }
#endif

            if (dynamicAllocThisFrame_Index[which] > dynamicAllocMaximum_Index) dynamicAllocMaximum_Index = dynamicAllocThisFrame_Index[which];
            if (dynamicAllocThisFrame[which] > dynamicAllocMaximum) dynamicAllocMaximum = dynamicAllocThisFrame[which];
        }

        public void UnbindIndex()
        {
            if (currentBoundVBO_Index != VBOEmpty) { qglBindBuffer(BufferTargetARB.ElementArrayBuffer, 0); currentBoundVBO_Index = VBOEmpty; }
        }

        public void UnbindVertex()
        {
            if (currentBoundVBO != VBOEmpty) { qglBindBuffer(BufferTargetARB.ArrayBuffer, 0); currentBoundVBO = VBOEmpty; }
        }

        public int ListNum
            => listNum;

        // listVertexCache calls this
        public void List()
        {
            int numActive = 0, frameStatic = 0, totalStatic = 0;

            VertCache block;
            for (block = staticHeaders.next; block != staticHeaders; block = block.next)
            {
                numActive++;
                totalStatic += block.size;
                if (block.frameUsed == currentFrame) frameStatic += block.size;
            }

            int numFreeStaticHeaders = 0, numFreeDynamicHeaders = 0, numFreeDynamicIndexHeaders = 0;
            for (block = freeStaticHeaders.next; block != freeStaticHeaders; block = block.next) numFreeStaticHeaders++;
            for (block = freeDynamicHeaders.next; block != freeDynamicHeaders; block = block.next) numFreeDynamicHeaders++;
            for (block = freeDynamicIndexHeaders.next; block != freeDynamicIndexHeaders; block = block.next) numFreeDynamicIndexHeaders++;

            common.Printf($"{r_vertexBufferMegs.Integer} megs working set\n");
            common.Printf($"{NUM_VERTEX_FRAMES} dynamic temp buffers of {frameBytes / 1024}k\n");
            common.Printf($"{numActive:5} active static headers\n");
            common.Printf($"{numFreeStaticHeaders:5} free static headers\n");
            common.Printf($"{numFreeDynamicHeaders + numFreeDynamicIndexHeaders:5} free dynamic headers\n");
        }

        //void InitMemoryBlocks(int size);
    }
}
