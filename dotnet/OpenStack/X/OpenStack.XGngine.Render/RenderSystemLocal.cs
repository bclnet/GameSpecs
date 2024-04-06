using System.NumericsX.OpenStack.Gngine.Framework;
using System.NumericsX.OpenStack.System;
using System.Threading;
using WaveEngine.Bindings.OpenGLES;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.Gngine.Render.QGL;
using static System.NumericsX.OpenStack.Gngine.Render.R;
using static System.NumericsX.OpenStack.Gngine.Render.TR;
using static System.NumericsX.OpenStack.OpenStack;
using GlIndex = System.Int32;

namespace System.NumericsX.OpenStack.Gngine.Render
{
    unsafe partial class TR
    {
        // This prints both front and back end counters, so it should only be called when the back end thread is idle.
        public static void R_PerformanceCounters()
        {
            if (r_showPrimitives.Integer != 0)
            {
                var megaBytes = globalImages.SumOfUsedImages() / (1024 * 1024.0f);
                if (r_showPrimitives.Integer > 1) common.Printf($"v:{tr.pc.c_numViews} ds:{backEnd.pc.c_drawElements + backEnd.pc.c_shadowElements} t:{backEnd.pc.c_drawIndexes / 3}/{(backEnd.pc.c_drawIndexes - backEnd.pc.c_drawRefIndexes) / 3} v:{backEnd.pc.c_drawVertexes}/{(backEnd.pc.c_drawVertexes - backEnd.pc.c_drawRefVertexes)} st:{backEnd.pc.c_shadowIndexes / 3} sv:{backEnd.pc.c_shadowVertexes} image:{megaBytes:5.1} MB\n");
                else common.Printf($"views:{tr.pc.c_numViews} draws:{backEnd.pc.c_drawElements + backEnd.pc.c_shadowElements} tris:{(backEnd.pc.c_drawIndexes + backEnd.pc.c_shadowIndexes) / 3} (shdw:{backEnd.pc.c_shadowIndexes / 3}) (vbo:{backEnd.pc.c_vboIndexes / 3}) image:{megaBytes:5.1} MB\n");
            }

            if (r_showDynamic.Bool) common.Printf($"callback:{tr.pc.c_entityDefCallbacks} md5:{tr.pc.c_generateMd5} dfrmVerts:{tr.pc.c_deformedVerts} dfrmTris:{tr.pc.c_deformedIndexes / 3} tangTris:{tr.pc.c_tangentIndexes / 3} guis:{tr.pc.c_guiSurfs}\n");
            if (r_showCull.Bool) common.Printf($"{tr.pc.c_sphere_cull_in} sin {tr.pc.c_sphere_cull_clip} sclip  {tr.pc.c_sphere_cull_out} sout {tr.pc.c_box_cull_in} bin {tr.pc.c_box_cull_out} bout\n");
            if (r_showAlloc.Bool) common.Printf($"alloc:{tr.pc.c_alloc} free:{tr.pc.c_free}\n");
            if (r_showInteractions.Bool) common.Printf($"createInteractions:{tr.pc.c_createInteractions} createLightTris:{tr.pc.c_createLightTris} createShadowVolumes:{tr.pc.c_createShadowVolumes}\n");
            if (r_showDefs.Bool) common.Printf($"viewEntities:{tr.pc.c_visibleViewEntities}  shadowEntities:{tr.pc.c_shadowViewEntities}  viewLights:{tr.pc.c_viewLights}\n");
            if (r_showUpdates.Bool) common.Printf($"entityUpdates:{tr.pc.c_entityUpdates}  entityRefs:{tr.pc.c_entityReferences}  lightUpdates:{tr.pc.c_lightUpdates}  lightRefs:{tr.pc.c_lightReferences}\n");
            if (r_showMemory.Bool) common.Printf($"frameData: {R_CountFrameData()} ({frameData?.memoryHighwater ?? 0})\n");

            tr.pc.memset();
            backEnd.pc.memset();
        }

        // Called by R_EndFrame each frame
        public static void R_IssueRenderCommands(FrameData fd) //: volatile
        {
            // nothing to issue
            if (fd.cmdHead.commandId == RC.NOP && fd.cmdHead.next == null) return;

            // r_skipBackEnd allows the entire time of the back end to be removed from performance measurements, although
            // nothing will be drawn to the screen. If the prints are going to a file, or r_skipBackEnd is later disabled,
            // usefull data can be received.
            // r_skipRender is usually more usefull, because it will still draw 2D graphics
            if (!r_skipBackEnd.Bool) RB_ExecuteBackEndCommands(fd.cmdHead);
        }

        // Returns memory for a command buffer (stretchPicCommand_t, drawSurfsCommand_t, etc) and links it to the end of the current command chain.
        public static T R_GetCommandBuffer<T>() where T : EmptyCommand, new()
        {
            var cmd = new T { next = null };
            frameData.cmdTail.next = cmd;
            frameData.cmdTail = cmd;
            return cmd;
        }

        // Called after every buffer submission and by R_ToggleSmpFrame
        static void R_ClearCommandChain()
        {
            // clear the command chain
            frameData.cmdHead = frameData.cmdTail = new EmptyCommand();
            frameData.cmdHead.commandId = RC.NOP;
            frameData.cmdHead.next = null;
        }

        static void R_ViewStatistics(ViewDef parms)
        {
            // report statistics about this view
            if (!r_showSurfaces.Bool) return;
            common.Printf($"view:{parms} surfs:{parms.numDrawSurfs}\n");
        }

        // This is the main 3D rendering command.  A single scene may have multiple views if a mirror, portal, or dynamic texture is present.
        static void R_AddDrawViewCmd(ViewDef parms)
        {
            var cmd = R_GetCommandBuffer<DrawSurfsCommand>();
            cmd.commandId = RC.DRAW_VIEW;
            cmd.viewDef = parms;
            // save the command for r_lockSurfaces debugging
            if (parms.viewEntitys != null) tr.lockSurfacesCmd = cmd;
            tr.pc.c_numViews++;
            R_ViewStatistics(parms);
        }


        //=================================================================================



        // r_lockSurfaces allows a developer to move around without changing the composition of the scene, including
        // culling.  The only thing that is modified is the view position and axis, no front end work is done at all
        // 
        // Add the stored off command again, so the new rendering will use EXACTLY the same surfaces, including all the culling, even though the transformation
        // matricies have been changed.  This allow the culling tightness to be evaluated interactively.
        static void R_LockSurfaceScene(ViewDef parms)
        {
            DrawSurfsCommand cmd; ViewEntity vModel;

            // set the matrix for world space to eye space
            R_SetViewMatrix(parms);

            tr.lockSurfacesCmd.viewDef.worldSpace = parms.worldSpace;

            // update the view origin and axis, and all the entity matricies
            for (vModel = tr.lockSurfacesCmd.viewDef.viewEntitys; vModel != null; vModel = vModel.next)
                fixed (float* a = vModel.modelMatrix)
                {
                    fixed (float* b = tr.lockSurfacesCmd.viewDef.worldSpace.u.eyeViewMatrix0, c = vModel.u.eyeViewMatrix0) myGlMultMatrix(a, b, c);
                    fixed (float* b = tr.lockSurfacesCmd.viewDef.worldSpace.u.eyeViewMatrix1, c = vModel.u.eyeViewMatrix1) myGlMultMatrix(a, b, c);
                    fixed (float* b = tr.lockSurfacesCmd.viewDef.worldSpace.u.eyeViewMatrix2, c = vModel.u.eyeViewMatrix2) myGlMultMatrix(a, b, c);
                }

            // add the stored off surface commands again
            cmd = R_GetCommandBuffer<DrawSurfsCommand>();
            cmd.Set(tr.lockSurfacesCmd);
        }

        // See if some cvars that we watch have changed
        public static void R_CheckCvars()
        {
            globalImages.CheckCvars();
            //game.CheckRenderCvars();

            // gamma stuff
            if (r_gamma.IsModified || r_brightness.IsModified)
            {
                r_gamma.ClearModified();
                r_brightness.ClearModified();
                R_SetColorMappings();
            }
        }
    }

    unsafe partial class RenderSystemLocal
    {
        RenderSystemLocal()
            => Clear();

        // This can be used to pass general information to the current material, not just colors
        public override void SetColor(in Vector4 rgba)
            => guiModel.SetColor(rgba.x, rgba.y, rgba.z, hudOpacity * rgba.w);

        public override void SetColor4(float r, float g, float b, float a)
        {
            a = hudOpacity * a;
            guiModel.SetColor(r, g, b, a);
        }

        public override void DrawStretchPic(DrawVert* verts, GlIndex* indexes, int vertCount, int indexCount, Material material, bool clip, float min_x, float min_y, float max_x, float max_y)
            => guiModel.DrawStretchPic(verts, indexes, vertCount, indexCount, material, clip, min_x, min_y, max_x, max_y);

        // x/y/w/h are in the 0,0 to 640,480 range
        public override void DrawStretchPic(float x, float y, float w, float h, float s1, float t1, float s2, float t2, Material material)
            => guiModel.DrawStretchPic(x, y, w, h, s1, t1, s2, t2, material);

        // x/y/w/h are in the 0,0 to 640,480 range
        public override void DrawStretchTri(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 t1, Vector2 t2, Vector2 t3, Material material)
            => guiModel.DrawStretchTri(p1, p2, p3, t1, t2, t3, material);

        public override void GlobalToNormalizedDeviceCoordinates(in Vector3 global, out Vector3 ndc)
            => R_GlobalToNormalizedDeviceCoordinates(global, out ndc);

        public override void GetGLSettings(out int width, out int height)
        {
            width = glConfig.vidWidth;
            height = glConfig.vidHeight;
        }

        // small chars are drawn at native screen resolution
        public override void DrawSmallChar(int x, int y, int ch, Material material)
        {
            int row, col; float frow, fcol, size;

            ch &= 255;

            if (ch == ' ') return;
            if (y < -SMALLCHAR_HEIGHT) return;

            row = ch >> 4;
            col = ch & 15;

            frow = row * 0.0625f;
            fcol = col * 0.0625f;
            size = 0.0625f;

            DrawStretchPic(x, y, SMALLCHAR_WIDTH, SMALLCHAR_HEIGHT, fcol, frow, fcol + size, frow + size, material);
        }

        // Draws a multi-colored string with a drop shadow, optionally forcing to a fixed color.
        // Coordinates are at 640 by 480 virtual resolution
        public override void DrawSmallStringExt(int x, int y, string s, in Vector4 setColor, bool forceColor, Material material)
        {
            Vector4 color; int si, xx;

            // draw the colored text
            si = 0;
            xx = x;
            SetColor(setColor);
            while (si < s.Length)
            {
                if (stringX.IsColor(s, si))
                {
                    if (!forceColor)
                    {
                        if (s[si + 1] == C_COLOR_DEFAULT) SetColor(setColor);
                        else { color = stringX.ColorForIndex(s[si + 1]); color.w = setColor.w; SetColor(color); }
                    }
                    si += 2;
                    continue;
                }
                DrawSmallChar(xx, y, s[si], material);
                xx += SMALLCHAR_WIDTH;
                si++;
            }
            SetColor(colorWhite);
        }

        public override void DrawBigChar(int x, int y, int ch, Material material)
        {
            int row, col; float frow, fcol, size;

            ch &= 255;

            if (ch == ' ' || y < -BIGCHAR_HEIGHT) return;

            row = ch >> 4;
            col = ch & 15;

            frow = row * 0.0625f;
            fcol = col * 0.0625f;
            size = 0.0625f;

            DrawStretchPic(x, y, BIGCHAR_WIDTH, BIGCHAR_HEIGHT, fcol, frow, fcol + size, frow + size, material);
        }

        // Draws a multi-colored string with a drop shadow, optionally forcing to a fixed color.
        // Coordinates are at 640 by 480 virtual resolution
        public override void DrawBigStringExt(int x, int y, string s, in Vector4 setColor, bool forceColor, Material material)
        {
            Vector4 color; int si, xx;

            // draw the colored text
            si = 0;
            xx = x;
            SetColor(setColor);
            while (si < s.Length)
            {
                if (stringX.IsColor(s, si))
                {
                    if (s[si + 1] == C_COLOR_DEFAULT) SetColor(setColor);
                    else { color = stringX.ColorForIndex(s[si + 1]); color.w = setColor.w; SetColor(color); }

                    si += 2;
                    continue;
                }
                DrawBigChar(xx, y, s[si], material);
                xx += BIGCHAR_WIDTH;
                si++;
            }
            SetColor(colorWhite);
        }

        //======================================================================================

        public override void BeginFrame(int windowWidth, int windowHeight)
        {
            if (!glConfig.isInitialized) return;

            guiModel.Clear();

            // for the larger-than-window tiled rendering screenshots
            if (tiledViewport[0] != 0) { windowWidth = tiledViewport[0]; windowHeight = tiledViewport[1]; }

            glConfig.vidWidth = windowWidth;
            glConfig.vidHeight = windowHeight;

            renderCrops[0].x = 0;
            renderCrops[0].y = 0;
            renderCrops[0].width = windowWidth;
            renderCrops[0].height = windowHeight;
            currentRenderCrop = 0;

            // screenFraction is just for quickly testing fill rate limitations
            if (r_screenFraction.Integer != 100) CropRenderSize((int)(SCREEN_WIDTH * r_screenFraction.Integer / 100.0f), (int)(SCREEN_HEIGHT * r_screenFraction.Integer / 100.0f));

            // this is the ONLY place this is modified
            frameCount++;

            // just in case we did a common.Error while this was set
            guiRecursionLevel = 0;

            // the first rendering will be used for commands like screenshot, rather than a possible subsequent remote or mirror render
            // primaryWorld = null;

            // set the time for shader effects in 2D rendering
            frameShaderTime = eventLoop.Milliseconds * 0.001f;

            // draw buffer stuff
            var cmd = R_GetCommandBuffer<SetBufferCommand>();
            cmd.commandId = RC.SET_BUFFER;
            cmd.frameCount = frameCount;
            cmd.buffer = 0;
        }

        public override void WriteDemoPics()
        {
            session.writeDemo.WriteInt((int)VFileDemo.DS.RENDER);
            session.writeDemo.WriteInt((int)DC.GUI_MODEL);
            guiModel.WriteToDemo(session.writeDemo);
        }

        public override void DrawDemoPics()
            => demoGuiModel.EmitFullScreen();

        public static int BackendThreadRunner(object localRenderSystem)
        {
            var local = (IRenderSystem)localRenderSystem;
            local.BackendThread();
            return 0;
        }

        public override void BackendThreadWait()
        {
            while (!backendFinished)
            {
                //Thread.Sleep(1000 * 3);
                ISystem.WaitForEvent(TRIGGER_EVENT.EVENT_BACKEND_FINISHED);
                //Thread.Ssleep(500);
            }
        }

        public override void BackendThread()
        {
            GLimp_ActivateContext();

            while (true)
            {
                if (useSpinLock)
                {
                    while (!backendThreadRun) if (spinLockDelay != 0) Thread.Sleep(spinLockDelay);
                    backendThreadRun = false;
                }
                else
                {
                    //Console.WriteLine("Wait TRIGGER_EVENT_RUN_BACKEND");
                    ISystem.WaitForEvent(TRIGGER_EVENT.EVENT_RUN_BACKEND);
                    //Console.WriteLine("Done TRIGGER_EVENT_RUN_BACKEND");
                }

                // Thread will be woken up to either shutdown or render
                if (backendThreadShutdown)
                {
                    Console.WriteLine("Backend thread ending..");
                    // Release context
                    GLimp_DeactivateContext();

                    // Finish thread
                    break;
                }
                else BackendThreadTask();
            }
        }

        void BackendThreadTask()
        {
            Image img;

            // Purge all images
            while ((img = globalImages.GetNextPurgeImage()) != null) { /*Console.WriteLine("IMAGE PURGE!");*/ img.PurgeImage(); }

            // Load all images
            while ((img = globalImages.GetNextAllocImage()) != null) { /*Console.WriteLine("IMAGE LOAD!");*/ img.ActuallyLoadImage(false); }

            if (useSpinLock) imagesFinished = true;
            else ISystem.TriggerEvent(TRIGGER_EVENT.EVENT_IMAGES_PROCESSES);

            vertexCache.BeginBackEnd(vertListToRender);

            R_IssueRenderCommands(fdToRender);

            // Take screen shot
            if (pixels != null)
            {
                fixed (byte* _ = pixels) qglReadPixels(pixelsCrop.x, pixelsCrop.y, pixelsCrop.width, pixelsCrop.height, PixelFormat.Rgba, VertexAttribPointerType.UnsignedByte, _);
                pixels = null;
                pixelsCrop = null;
            }

            backendFinished = true;
            ISystem.TriggerEvent(TRIGGER_EVENT.EVENT_BACKEND_FINISHED);
        }

        public void BackendThreadExecute()
        {
            //Console.WriteLine("BackendThreadRun called..");
            imagesFinished = false;
            backendFinished = false;

            if (multithreadActive)
            {
                if (renderThread.threadHandle == null)
                {
                    Console.WriteLine("Starting new backend thread");

                    GLimp_DeactivateContext();
                    backendThreadShutdown = false;
                    ISystem.CreateThread(RenderSystemLocal.BackendThreadRunner, this, out renderThread, "renderThread");
                }

                // Start Thread
                if (useSpinLock) backendThreadRun = true;
                else
                {
                    //Console.WriteLine("Trigger TRIGGER_EVENT_RUN_BACKEND");
                    ISystem.TriggerEvent(TRIGGER_EVENT.EVENT_RUN_BACKEND);
                }
            }
            // No multithread, just execute in sequence
            else BackendThreadTask();
        }

        public override void BackendThreadShutdown()
        {
            Console.WriteLine("Shutting down backend thread");

            if (multithreadActive && renderThread.threadHandle != null)
            {
                // Wait for thread to be ready
                BackendThreadWait();
                // Set shutdown flag
                backendThreadShutdown = true;

                // Start Thread
                if (useSpinLock) backendThreadRun = true;
                else
                {
                    //Console.WriteLine("Trigger TRIGGER_EVENT_RUN_BACKEND");
                    ISystem.TriggerEvent(TRIGGER_EVENT.EVENT_RUN_BACKEND);
                }

                // Join thread and wait until finished
                ISystem.DestroyThread(ref renderThread);

                // Clear handle
                renderThread.threadHandle = 0;

                //Take GL context
                GLimp_ActivateContext();
            }
        }

        public void RenderCommands(RenderCrop pc, byte[] pix)
        {
            // Only do rendering if the app is actually active
            if (windowActive)
            {
                // Wait for last backend rendering to finish
                BackendThreadWait();

                // Limit maximum FPS
                var maxFPS = r_maxFps.Integer;
                if (maxFPS != 0)
                {
                    var limit = (uint)(1000 / maxFPS);
                    var currentTime = (uint)SysW.Milliseconds;
                    var timeTook = currentTime - lastRenderTime;
                    if (timeTook < limit) Thread.Sleep((int)(limit - timeTook) * 1000);
                    lastRenderTime = (uint)SysW.Milliseconds;
                }

                //Console.WriteLine("---------------------NEW FRAME---------------------");

                // We have turned off multithreading, we need to shut it down
                if (multithreadActive && !r_multithread.Bool) { BackendThreadShutdown(); multithreadActive = false; }
                else if (!multithreadActive && r_multithread.Bool) multithreadActive = true;

                // Save the current vertexs and framedata to use for next render
                vertListToRender = vertexCache.ListNum;
                fdToRender = frameData;

                //Save the potential pixel
                pixelsCrop = pc;
                pixels = pix;

                BackendThreadExecute();

                // Wait for the backend to load any images, this only really happens at level load time
                // Problem is image loading is not thread safe, hence the wait
                if (useSpinLock) { while (!imagesFinished) if (spinLockDelay != 0) Thread.Sleep(spinLockDelay); }
                else ISystem.WaitForEvent(TRIGGER_EVENT.EVENT_IMAGES_PROCESSES);
            }

            // If we are waiting for pixel data, make sure we wait for the backend to finish
            if (pix != null) BackendThreadWait();

            // use the other buffers next frame, because another CPU
            // may still be rendering into the current buffers
            R_ToggleSmpFrame();

            // we can now release the vertexes used this frame
            vertexCache.EndFrame();

            R_ClearCommandChain();
        }

        // Returns the number of msec spent in the back end
        public override void EndFrame(out int frontEndMsec, out int backEndMsec)
        {
            if (!glConfig.isInitialized) { frontEndMsec = default; backEndMsec = default; return; }

            // close any gui drawing
            guiModel.EmitFullScreen();
            guiModel.Clear();

            // save out timing information
            frontEndMsec = pc.frontEndMsec;
            backEndMsec = backEnd.pc.msec;

            // print any other statistics and clear all of them
            R_PerformanceCounters();

            // check for dynamic changes that require some initialization
            R_CheckCvars();

            // check for errors
            GL_CheckErrors();

            // add the swapbuffers command
            var cmd = R_GetCommandBuffer<EmptyCommand>();
            cmd.commandId = RC.SWAP_BUFFERS;

            // Render the commands. No pixel data passed so it will return immediatle if multithreading
            RenderCommands(null, null);

            if (session.writeDemo != null)
            {
                session.writeDemo.WriteInt((int)VFileDemo.DS.RENDER);
                session.writeDemo.WriteInt((int)DC.END_FRAME);
                if (r_showDemo.Bool) common.Printf("write DC_END_FRAME\n");
            }
        }

        // Converts from SCREEN_WIDTH / SCREEN_HEIGHT coordinates to current cropped pixel coordinates
        public override void RenderViewToViewport(RenderView renderView, out ScreenRect viewport)
        {
            var rc = renderCrops[currentRenderCrop];

            var wRatio = (float)rc.width / SCREEN_WIDTH;
            var hRatio = (float)rc.height / SCREEN_HEIGHT;

            viewport = new();
            viewport.x1 = (short)MathX.Ftoi(rc.x + renderView.x * wRatio);
            viewport.x2 = (short)MathX.Ftoi(rc.x + (float)Math.Floor((renderView.x + renderView.width) * wRatio + 0.5f) - 1);
            viewport.y1 = (short)MathX.Ftoi((rc.y + rc.height) - (float)Math.Floor((renderView.y + renderView.height) * hRatio + 0.5f));
            viewport.y2 = (short)MathX.Ftoi((rc.y + rc.height) - (float)Math.Floor(renderView.y * hRatio + 0.5f) - 1);
        }

        static int RoundDownToPowerOfTwo(int v)
        {
            int i;
            for (i = 0; i < 20; i++)
            {
                if ((1 << i) == v) return v;
                if ((1 << i) > v) return 1 << (i - 1);
            }
            return 1 << i;
        }

        // This automatically halves sizes until it fits in the current window size, so if you specify a power of two size for a texture copy, it may be shrunk down, but still valid.
        public override void CropRenderSize(int width, int height, bool makePowerOfTwo = false, bool forceDimensions = false)
        {
            if (!glConfig.isInitialized) return;

            // close any gui drawing before changing the size
            guiModel.EmitFullScreen();
            guiModel.Clear();

            if (width < 1 || height < 1) common.Error("CropRenderSize: bad sizes");

            if (session.writeDemo != null)
            {
                session.writeDemo.WriteInt((int)VFileDemo.DS.RENDER);
                session.writeDemo.WriteInt((int)DC.CROP_RENDER);
                session.writeDemo.WriteInt(width);
                session.writeDemo.WriteInt(height);
                session.writeDemo.WriteInt(makePowerOfTwo ? 1 : 0);
                if (r_showDemo.Bool) common.Printf("write DC_CROP_RENDER\n");
            }

            // convert from virtual SCREEN_WIDTH/SCREEN_HEIGHT coordinates to physical OpenGL pixels
            RenderView renderView = new();
            renderView.x = 0;
            renderView.y = 0;
            renderView.width = width;
            renderView.height = height;

            RenderViewToViewport(renderView, out var r);
            width = r.x2 - r.x1 + 1;
            height = r.y2 - r.y1 + 1;

            // just give exactly what we ask for
            if (forceDimensions) { width = renderView.width; height = renderView.height; }

            // if makePowerOfTwo, drop to next lower power of two after scaling to physical pixels
            // FIXME: megascreenshots with offset viewports don't work right with this yet
            if (makePowerOfTwo) { width = RoundDownToPowerOfTwo(width); height = RoundDownToPowerOfTwo(height); }

            // we might want to clip these to the crop window instead
            while (width > glConfig.vidWidth) width >>= 1;
            while (height > glConfig.vidHeight) height >>= 1;

            if (currentRenderCrop == RenderCrop.MAX_RENDER_CROPS) common.Error("RenderSystemLocal::CropRenderSize: currentRenderCrop == MAX_RENDER_CROPS");
            currentRenderCrop++;
            var rc = renderCrops[currentRenderCrop];
            rc.x = 0;
            rc.y = 0;
            rc.width = width;
            rc.height = height;
        }

        public override void UnCrop()
        {
            if (!glConfig.isInitialized) return;
            if (currentRenderCrop < 1) common.Error("idRenderSystemLocal::UnCrop: currentRenderCrop < 1");

            // close any gui drawing
            guiModel.EmitFullScreen();
            guiModel.Clear();

            currentRenderCrop--;

            if (session.writeDemo != null)
            {
                session.writeDemo.WriteInt((int)VFileDemo.DS.RENDER);
                session.writeDemo.WriteInt((int)DC.UNCROP_RENDER);
                if (r_showDemo.Bool) common.Printf("write DC_UNCROP\n");
            }
        }

        public override void CaptureRenderToImage(string imageName)
        {
            if (!glConfig.isInitialized) return;

            guiModel.EmitFullScreen();
            guiModel.Clear();

            if (session.writeDemo != null)
            {
                session.writeDemo.WriteInt((int)VFileDemo.DS.RENDER);
                session.writeDemo.WriteInt((int)DC.CAPTURE_RENDER);
                session.writeDemo.WriteHashString(imageName);

                if (r_showDemo.Bool) common.Printf($"write DC_CAPTURE_RENDER: {imageName}\n");
            }

            // look up the image before we create the render command, because it may need to sync to create the image
            var image = globalImages.ImageFromFile(imageName, Image.TF.DEFAULT, true, Image.TR.REPEAT, Image.TD.DEFAULT);

            var rc = renderCrops[currentRenderCrop];

            var cmd = R_GetCommandBuffer<CopyRenderCommand>();
            cmd.commandId = RC.COPY_RENDER;
            cmd.x = rc.x;
            cmd.y = rc.y;
            cmd.imageWidth = rc.width;
            cmd.imageHeight = rc.height;
            cmd.image = image;

            guiModel.Clear();
        }

        public override void CaptureRenderToFile(string fileName, bool fixAlpha)
        {
            if (!glConfig.isInitialized) return;

            var rc = renderCrops[currentRenderCrop];

            guiModel.EmitFullScreen();
            guiModel.Clear();

            // include extra space for OpenGL padding to word boundaries
            var c = (rc.width + 4) * rc.height;
            var data = new byte[c * 4];

            // This will render the commands and will block untill finished and has the pixel data
            RenderCommands(rc, data);

            var data2 = (byte*)R_StaticAlloc(c * 4);
            for (var i = 0; i < c; i++)
            {
                data2[i * 4] = data[i * 4];
                data2[i * 4 + 1] = data[i * 4 + 1];
                data2[i * 4 + 2] = data[i * 4 + 2];
                data2[i * 4 + 3] = 0xff;
            }

            Image.R_WriteTGA(fileName, data2, rc.width, rc.height, true);

            R_StaticFree(data2);
        }

        public override IRenderWorld AllocRenderWorld()
        {
            var rw = new RenderWorldLocal();
            worlds.Add(rw);
            return rw;
        }

        public override void FreeRenderWorld(ref IRenderWorld rw)
        {
            if (primaryWorld == rw) primaryWorld = null;
            worlds.Remove(rw);
            rw = null;
        }

        public override void PrintMemInfo(MemInfo mi)
        {
            // sum up image totals
            globalImages.PrintMemInfo(mi);

            // sum up model totals
            renderModelManager.PrintMemInfo(mi);
        }

        public override bool UploadImage(string imageName, byte* data, int width, int height)
        {
            var image = globalImages.GetImage(imageName);
            if (image == null) return false;
            image.UploadScratch(data, width, height);
            image.SetImageFilterAndRepeat();
            return true;
        }

        public override void DirectFrameBufferStart()
        {
            R_DirectFrameBufferStart();
        }

        public override void DirectFrameBufferEnd()
        {
            R_DirectFrameBufferEnd();
        }
    }
}