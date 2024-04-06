using System.NumericsX.OpenStack.Gngine.Framework;
using System.NumericsX.OpenStack.System;
using System.Runtime.CompilerServices;
using WaveEngine.Bindings.OpenGLES;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.Gngine.Render.QGL;
using static System.NumericsX.OpenStack.Gngine.Render.R;
using static System.NumericsX.OpenStack.Gngine.Render.TR;
using static System.NumericsX.OpenStack.Gngine.Render.Interaction;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.Render
{
    unsafe partial class TR
    {
        public static bool R_CheckExtension(string name)
        {
            if (!glConfig.extensions_string.Contains(name)) { common.Printf($"X..{name} not found\n"); return false; }

            common.Printf($"...using {name}\n");
            return true;
        }

        static void R_CheckPortableExtensions()
        {
            // GL_EXT_texture_filter_anisotropic (extension only)
            glConfig.anisotropicAvailable = R_CheckExtension("GL_EXT_texture_filter_anisotropic");
            glConfig.npotAvailable = R_CheckExtension("GL_OES_texture_npot"); common.Printf($" npotAvailable: {glConfig.npotAvailable}\n");
            glConfig.depthStencilAvailable = R_CheckExtension("GL_OES_packed_depth_stencil"); common.Printf($" depthStencilAvailable: {glConfig.depthStencilAvailable}\n");

            if (glConfig.anisotropicAvailable) { qglGetFloatv(GL_MAX_TEXTURE_MAX_ANISOTROPY_EXT, out glConfig.maxTextureAnisotropy); common.Printf($"   maxTextureAnisotropy: {glConfig.maxTextureAnisotropy}\n"); }
            else glConfig.maxTextureAnisotropy = 1f;
        }

        // r_mode is normally a small non-negative integer that looks resolutions up in a table, but if it is set to -1,
        // the values from r_customWidth, and r_customHeight will be used instead.
        static readonly (string description, int width, int height)[] r_vidModes = {
	        // Useless modes (UI will not fit)
	        ("Mode  0: 320x240",    320, 240),
            ("Mode  1: 400x300",    400, 300),
            ("Mode  2: 512x384",    512, 384),
	        // Usable modes (UI will be OK)
	        ("Mode  3: 640x480",    640, 480),
            ("Mode  4: 800x600",    800, 600),
            ("Mode  5: 960x640",    960, 640),
            ("Mode  6: 1024x640",  1024, 640),
            ("Mode  7: 1280x720",  1280, 720),
            ("Mode  8: 1024x768",  1024, 768),
            ("Mode  9: 1366x768",  1366, 768),
            ("Mode 10: 1152x864",  1152, 864),
            ("Mode 11: 1440x900",  1440, 900),
            ("Mode 12: 1600x900",  1600, 900),
            ("Mode 13: 1280x1024", 1280, 1024),
            ("Mode 14: 1400x1050", 1400, 1050),
            ("Mode 15: 1680x1050", 1680, 1050),
            ("Mode 16: 1920x1080", 1920, 1080),
            ("Mode 17: 1600x1200", 1600, 1200),
            ("Mode 18: 1920x1200", 1920, 1200),
        };
        // DG: made this an enum so even stupid compilers accept it as array length below
        //enum { r_vidModes.Length = sizeof(r_vidModes) / sizeof(r_vidModes[0]) };

        static bool R_GetModeInfo(out int width, out int height, int mode)
        {
            if (mode < -1 && mode >= r_vidModes.Length) { width = default; height = default; return false; }
            if (mode == -1) { width = r_customWidth.Integer; height = r_customHeight.Integer; return true; }

            var vm = r_vidModes[mode];
            width = vm.width;
            height = vm.height;
            return true;
        }

        // DG: I added all this vidModeInfoPtr stuff, so I can have a second list of vidmodes
        //     that are sorted (by width, height), instead of just r_mode index.
        //     That way I can add modes without breaking r_mode, but still display them
        //     sorted in the menu.
        //struct vidModePtr
        //{
        //    vidmode_t* vidMode;
        //    int modeIndex;
        //};

        //static vidModePtr sortedVidModes[r_vidModes.Length];

        //static int vidModeCmp(void* vm1, void* vm2)
        //{

        //    const vidModePtr* v1 = static_cast <const vidModePtr*> (vm1);
        //    const vidModePtr* v2 = static_cast <const vidModePtr*> (vm2);

        //    // D3Wasm: sort primarily by height, secondarily by width
        //    int wdiff = v1.vidMode.height - v2.vidMode.height;
        //    return (wdiff != 0) ? wdiff : (v1.vidMode.width - v2.vidMode.width);
        //}

        //static void initSortedVidModes()
        //{
        //    if (sortedVidModes[0].vidMode != null)
        //    {
        //        // already initialized
        //        return;
        //    }

        //    for (var i = 0; i < VidModes.Length; ++i)
        //    {
        //        sortedVidModes[i].modeIndex = i;
        //        sortedVidModes[i].vidMode = &r_vidModes[i];
        //    }

        //    qsort(sortedVidModes, r_vidModes.Length, sizeof(vidModePtr), vidModeCmp);
        //}

        // DG: the following two functions are part of a horrible hack in ChoiceWindow.cpp
        //     to overwrite the default resolution list in the system options menu

        // "r_custom*;640x480;800x600;1024x768;..."
        //string R_GetVidModeListString(bool addCustom)
        //{
        //    idStr ret = addCustom ? "r_custom*" : "";

        //    for (int i = 0; i < r_vidModes.Length; ++i)
        //    {
        //        // for some reason, modes 0-2 are not used. maybe too small for GUI?
        //        if (sortedVidModes[i].modeIndex >= 3 && sortedVidModes[i].vidMode != null)
        //        {
        //            idStr modeStr;
        //            sprintf(modeStr, ";%dx%d", sortedVidModes[i].vidMode.width, sortedVidModes[i].vidMode.height);
        //            ret += modeStr;
        //        }
        //    }
        //    return ret;
        //}

        //// r_mode values for resolutions from R_GetVidModeListString(): "-1;3;4;5;..."
        //string R_GetVidModeValsString(bool addCustom)
        //{
        //    idStr ret = addCustom ? "-1" : ""; // for custom resolutions using r_customWidth/r_customHeight
        //    for (int i = 0; i < r_vidModes.Length; ++i)
        //    {
        //        // for some reason, modes 0-2 are not used. maybe too small for GUI?
        //        if (sortedVidModes[i].modeIndex >= 3 && sortedVidModes[i].vidMode != null)
        //        {
        //            ret += ";";
        //            ret += sortedVidModes[i].modeIndex;
        //        }
        //    }
        //    return ret;
        //}
        // DG end


        // This function is responsible for initializing a valid OpenGL subsystem for rendering.  This is done by calling the system specific GLimp_Init,
        // which gives us a working OGL subsystem, then setting all necessary openGL state, including images, vertex programs, and display lists.
        // 
        // Changes to the vertex cache size or smp state require a vid_restart.
        // 
        // If glConfig.isInitialized is false, no rendering can take place, but all renderSystem functions will still operate properly, notably the material
        // and model information functions.
        static bool R_InitOpenGL_gotContext = false;
        public static void R_InitOpenGL()
        {
            //GLint temp;
            int i;

            common.Printf("----- Initializing OpenGL -----\n");
            if (glConfig.isInitialized) common.FatalError("R_InitOpenGL called while active");

            // Wait for BE to finish and take back the GL context
            renderSystem.BackendThreadShutdown();

            // in case we had an error while doing a tiled rendering
            tr.viewportOffset[0] = 0;
            tr.viewportOffset[1] = 0;

            //initSortedVidModes();

            // initialize OS specific portions of the renderSystem
            if (!R_InitOpenGL_gotContext)
            {
                for (i = 0; i < 2; i++)
                {
                    // set the parameters we are trying
                    R_GetModeInfo(out glConfig.vidWidth, out glConfig.vidHeight, r_mode.Integer);

                    GlimpParms parms;
                    parms.width = glConfig.vidWidth;
                    parms.height = glConfig.vidHeight;
                    parms.fullScreen = r_fullscreen.Bool;
                    parms.displayHz = r_displayRefresh.Integer;
                    parms.multiSamples = r_multiSamples.Integer;
                    parms.stereo = false;
                    // it worked
                    if (GLimp_Init(parms)) break;
                    if (i == 1) common.FatalError("Unable to initialize OpenGL");

                    // if we failed, set everything back to "safe mode" and try again
                    r_mode.Integer = 3;
                    r_fullscreen.Integer = 0;
                    r_displayRefresh.Integer = 0;
                    r_multiSamples.Integer = 0;
                }
                R_InitOpenGL_gotContext = true;
            }

            // input and sound systems need to be tied to the new window
            SysW.InitInput();
            soundSystem.InitHW();

            // get our config strings
            glConfig.vendor_string = qglGetString(StringName.Vendor);
            glConfig.renderer_string = qglGetString(StringName.Renderer);
            glConfig.version_string = qglGetString(StringName.Version);
            glConfig.extensions_string = qglGetString(StringName.Extensions);

            // OpenGL driver constants
            qglGetIntegerv(GetPName.MaxTextureSize, out glConfig.maxTextureSize);

            // stubbed or broken drivers may have reported 0...
            if (glConfig.maxTextureSize <= 0) glConfig.maxTextureSize = 256;

            qglGetIntegerv(GetPName.MaxTextureImageUnits, out glConfig.maxTextureUnits);
            if (glConfig.maxTextureUnits > MAX_MULTITEXTURE_UNITS) glConfig.maxTextureUnits = MAX_MULTITEXTURE_UNITS;

            glConfig.isInitialized = true;

            common.Printf($"OpenGL vendor: {glConfig.vendor_string}\n");
            common.Printf($"OpenGL renderer: {glConfig.renderer_string}\n");
            common.Printf($"OpenGL version: {glConfig.version_string}\n");

            // recheck all the extensions (FIXME: this might be dangerous)
            R_CheckPortableExtensions();

            cmdSystem.AddCommand("reloadGLSLprograms", R_ReloadGLSLPrograms_f, CMD_FL.RENDERER, "reloads GLSL programs");
            R_ReloadGLSLPrograms_f(CmdArgs.Empty);

            // allocate the vertex array range or vertex objects
            vertexCache.Init();

            common.Printf("using GLSL renderSystem\n");

            // allocate the frame data, which may be more if smp is enabled
            R_InitFrameData();

            vertexCache.BeginBackEnd(vertexCache.ListNum + 1);
            vertexCache.EndFrame();

            // Reset our gamma
            R_SetColorMappings();

            common.Printf("----- OpenGL Initialization complete-----\n");
        }

        // Reload the material displayed by r_showSurfaceInfo
        static void R_ReloadSurface_f(CmdArgs args)
        {
            Vector3 start, end;

            // start far enough away that we don't hit the player model
            start = tr.primaryView.renderView.vieworg + tr.primaryView.renderView.viewaxis[0] * 16;
            end = start + tr.primaryView.renderView.viewaxis[0] * 1000f;
            if (!tr.primaryWorld.Trace(out var mt, start, end, 0f, false)) return;

            common.Printf($"Reloading {mt.material.Name}\n");

            // reload the decl
            mt.material.base_.Reload();

            // reload any images used by the decl
            mt.material.ReloadImages(false);
        }

        static void R_ListModes_f(CmdArgs args)
        {
            common.Printf("\n");
            for (var i = 0; i < r_vidModes.Length; i++) common.Printf($"{r_vidModes[i].description}\n");
            common.Printf("\n");
        }

        // Display the given image centered on the screen.
        // testimage <number>
        // testimage <filename>
        static void R_TestImage_f(CmdArgs args)
        {
            int imageNum;

            if (tr.testVideo != null) tr.testVideo = null;
            tr.testImage = null;

            if (args.Count != 2) return;

            if (stringX.IsNumeric(args[1]))
            {
                imageNum = intX.Parse(args[1]);
                if (imageNum >= 0 && imageNum < globalImages.images.Count) tr.testImage = globalImages.images[imageNum];
            }
            else tr.testImage = globalImages.ImageFromFile(args[1], Image.TF.DEFAULT, false, Image.TR.REPEAT, Image.TD.DEFAULT);
        }

        // Plays the cinematic file in a testImage
        static void R_TestVideo_f(CmdArgs args)
        {
            if (tr.testVideo != null) tr.testVideo = null;
            tr.testImage = null;

            if (args.Count < 2) return;

            tr.testImage = globalImages.ImageFromFile("_scratch", Image.TF.DEFAULT, false, Image.TR.REPEAT, Image.TD.DEFAULT);
            tr.testVideo = Cinematic.Alloc();
            tr.testVideo.InitFromFile(args[1], true);

            var cin = tr.testVideo.ImageForTime(0);
            if (cin.image == null) { tr.testVideo = null; tr.testImage = null; return; }

            common.Printf($"{cin.imageWidth} x {cin.imageHeight} images\n");

            var len = tr.testVideo.AnimationLength;
            common.Printf($"{len * 0.001f:5.1} seconds of video\n");

            tr.testVideoStartTime = tr.primaryRenderView.time * 0.001f;

            // try to play the matching wav file
            var wavString = args[args.Count == 2 ? 1 : 2];
            wavString = $"{PathX.StripFileExtension(wavString)}.wav";
            session.sw.PlayShaderDirectly(wavString);
        }

        // Prints a list of the materials sorted by surface area
        static void R_ReportSurfaceAreas_f(CmdArgs args)
        {
            int i;

            var count = declManager.GetNumDecls(DECL.MATERIAL);
            var list = new Material[count];
            for (i = 0; i < count; i++) list[i] = (Material)declManager.DeclByIndex(DECL.MATERIAL, i, false);

            Array.Sort(list, (a, b) =>
            {
                var ac = !a.EverReferenced() ? 0f : a.SurfaceArea;
                var bc = !b.EverReferenced() ? 0f : b.SurfaceArea;
                if (ac < bc) return -1;
                if (ac > bc) return 1;
                return string.Compare(a.Name, b.Name, true);
            });

            // skip over ones with 0 area
            for (i = 0; i < count; i++) if (list[i].SurfaceArea > 0f) break;

            // report size in "editor blocks"
            for (; i < count; i++)
            {
                var blocks = (int)(list[i].SurfaceArea / 4096.0f);
                common.Printf($"{blocks,7} {list[i].Name}\n");
            }
        }

        // Checks for images with the same hash value and does a better comparison
        static void R_ReportImageDuplication_f(CmdArgs args)
        {
            int i, j;

            common.Printf("Images with duplicated contents:\n");

            var count = 0;
            for (i = 0; i < globalImages.images.Count; i++)
            {
                var image1 = globalImages.images[i];
                if (
                    // ignore procedural images
                    image1.generatorFunction != null
                    // ignore cube maps
                    || image1.cubeFiles != Image.CF._2D
                    || image1.defaulted) continue;

                byte* data1;
                int w1, h1;
                R_LoadImageProgram(image1.imgName, &data1, &w1, &h1, null);

                for (j = 0; j < i; j++)
                {
                    var image2 = globalImages.images[j];

                    if (
                        image2.generatorFunction != null
                        || image2.cubeFiles != Image.CF._2D
                        || image2.defaulted
                        || image1.imageHash != image2.imageHash
                        || (image2.uploadWidth != image1.uploadWidth || image2.uploadHeight != image1.uploadHeight)
                        // ignore same image-with-different-parms
                        || string.Equals(image1.imgName, image2.imgName, StringComparison.OrdinalIgnoreCase))
                        continue;

                    byte* data2;
                    int w2, h2;
                    R_LoadImageProgram(image2.imgName, &data2, &w2, &h2, null);

                    if (w2 != w1 || h2 != h1) { R_StaticFree(data2); continue; }
                    if (memcmp(data1, data2, w1 * h1 * 4)) { R_StaticFree(data2); continue; }

                    R_StaticFree(data2);

                    common.Printf($"{image1.imgName} == {image2.imgName}\n");
                    session.UpdateScreen(true);
                    count++;
                    break;
                }

                R_StaticFree(data1);
            }
            common.Printf($"{count} / {globalImages.images.Count} collisions\n");
        }

        #region THROUGHPUT BENCHMARKING

        static float R_RenderingFPS(RenderView renderView)
        {
            qglFinish();

            const int SAMPLE_MSEC = 1000;
            int start = SysW.Milliseconds, end, count = 0;
            while (true)
            {
                // render
                renderSystem.BeginFrame(glConfig.vidWidth, glConfig.vidHeight);
                tr.primaryWorld.RenderScene(renderView);
                renderSystem.EndFrame(out _, out _);
                qglFinish();
                count++;
                end = SysW.Milliseconds;
                if (end - start > SAMPLE_MSEC) break;
            }

            var fps = count * 1000.0f / (end - start);
            return fps;
        }

        static void R_Benchmark_f(CmdArgs args)
        {
            float fps, msec;

            if (tr.primaryView == null) { common.Printf("No primaryView for benchmarking\n"); return; }
            var view = tr.primaryRenderView;

            for (var size = 100; size >= 10; size -= 10)
            {
                r_screenFraction.Integer = size;
                fps = R_RenderingFPS(view);
                var kpix = (int)(glConfig.vidWidth * glConfig.vidHeight * (size * 0.01f) * (size * 0.01f) * 0.001f);
                msec = 1000.0f / fps;
                common.Printf($"kpix: {kpix,4}  msec:{msec:5.1} fps:{fps:5.1}f\n");
            }

            r_screenFraction.Integer = 100;
        }

        #endregion

        #region SCREEN SHOTS

        // Allows the rendering of an image larger than the actual window by tiling it into window-sized chunks and rendering each chunk separately
        // If ref isn't specified, the full session UpdateScreen will be done.
        public static void R_ReadTiledPixels(int width, int height, byte* buffer, RenderView ref_ = null)
        {
            // include extra space for OpenGL padding to word boundaries
            var temp = (byte*)R_StaticAlloc((glConfig.vidWidth + 4) * glConfig.vidHeight * 4);

            var oldWidth = glConfig.vidWidth;
            var oldHeight = glConfig.vidHeight;

            tr.tiledViewport[0] = width;
            tr.tiledViewport[1] = height;

            // disable scissor, so we don't need to adjust all those rects
            r_useScissor.Bool = false;

            for (var xo = 0; xo < width; xo += oldWidth)
                for (var yo = 0; yo < height; yo += oldHeight)
                {
                    tr.viewportOffset[0] = -xo;
                    tr.viewportOffset[1] = -yo;

                    if (ref_ != null) { tr.BeginFrame(oldWidth, oldHeight); tr.primaryWorld.RenderScene(ref_); tr.EndFrame(out _, out _); }
                    else session.UpdateScreen(false);

                    var w = oldWidth;
                    if (xo + w > width) w = width - xo;
                    var h = oldHeight;
                    if (yo + h > height) h = height - yo;

                    // Disabled for OES2
                    //qglReadBuffer(GL_FRONT);

                    qglReadPixels(0, 0, w, h, PixelFormat.Rgba, VertexAttribPointerType.UnsignedByte, temp);

                    var row = (w * 4 + 4) & ~4;     // OpenGL pads to dword boundaries

                    for (var y = 0; y < h; y++) Unsafe.CopyBlock(buffer + ((yo + y) * width + xo) * 4, temp + y * row, (uint)(w * 4));
                }

            r_useScissor.Bool = true;

            tr.viewportOffset[0] = 0;
            tr.viewportOffset[1] = 0;
            tr.tiledViewport[0] = 0;
            tr.tiledViewport[1] = 0;

            R_StaticFree(temp);

            glConfig.vidWidth = oldWidth;
            glConfig.vidHeight = oldHeight;
        }

        // Returns a filename with digits appended if we have saved a previous screenshot, don't scan from the beginning, because recording demo avis can involve thousands of shots
        static void R_ScreenshotFilename(int lastNumber, string base_, out string fileName)
        {
            int a, b, c, d, e;

            var fsrestrict = cvarSystem.GetCVarBool("fs_restrict");
            cvarSystem.SetCVarBool("fs_restrict", false);
            fileName = default;

            lastNumber++;
            if (lastNumber > 99999) lastNumber = 99999;
            for (; lastNumber < 99999; lastNumber++)
            {
                var frac = lastNumber;

                a = frac / 10000;
                frac -= a * 10000;
                b = frac / 1000;
                frac -= b * 1000;
                c = frac / 100;
                frac -= c * 100;
                d = frac / 10;
                frac -= d * 10;
                e = frac;

                fileName = $"{base_}{a}{b}{c}{e}{e}.tga";
                if (lastNumber == 99999) break;
                var len = fileSystem.ReadFile(fileName, out _);
                if (len <= 0) break;
                // check again...
            }
            cvarSystem.SetCVarBool("fs_restrict", fsrestrict);
        }

        // screenshot
        // screenshot [filename]
        // screenshot [width] [height]
        // screenshot [width] [height] [samples]
        static int R_ScreenShot_f_lastNumber = 0;
        static void R_ScreenShot_f(CmdArgs args)
        {
            const int MAX_BLENDS = 256; // to keep the accumulation in shorts
            string checkname;

            var width = glConfig.vidWidth;
            var height = glConfig.vidHeight;
            var blends = 0;
            switch (args.Count)
            {
                case 1:
                    width = glConfig.vidWidth;
                    height = glConfig.vidHeight;
                    blends = 1;
                    R_ScreenshotFilename(R_ScreenShot_f_lastNumber, "screenshots/shot", out checkname);
                    break;
                case 2:
                    width = glConfig.vidWidth;
                    height = glConfig.vidHeight;
                    blends = 1;
                    checkname = args[1];
                    break;
                case 3:
                    width = intX.Parse(args[1]);
                    height = intX.Parse(args[2]);
                    blends = 1;
                    R_ScreenshotFilename(R_ScreenShot_f_lastNumber, "screenshots/shot", out checkname);
                    break;
                case 4:
                    width = intX.Parse(args[1]);
                    height = intX.Parse(args[2]);
                    blends = intX.Parse(args[3]);
                    if (blends < 1) blends = 1;
                    if (blends > MAX_BLENDS) blends = MAX_BLENDS;
                    R_ScreenshotFilename(R_ScreenShot_f_lastNumber, "screenshots/shot", out checkname);
                    break;
                default:
                    common.Printf("usage: screenshot\n       screenshot <filename>\n       screenshot <width> <height>\n       screenshot <width> <height> <blends>\n");
                    return;
            }

            // put the console away
            console.Close();

            tr.TakeScreenshot(width, height, checkname, blends, null);

            common.Printf($"Wrote {checkname}\n");
        }

        // envshot <basename>
        // Saves out env/<basename>_ft.tga, etc
        static void R_EnvShot_f(CmdArgs args)
        {
            string[] extensions = { "_px.tga", "_nx.tga", "_py.tga", "_ny.tga", "_pz.tga", "_nz.tga" };

            if (args.Count != 2 && args.Count != 3 && args.Count != 4) { common.Printf("USAGE: envshot <basename> [size] [blends]\n"); return; }
            var baseName = args[1];

            int blends = 1, size;
            if (args.Count == 4) { size = intX.Parse(args[2]); blends = intX.Parse(args[3]); }
            else if (args.Count == 3) { size = intX.Parse(args[2]); blends = 1; }
            else { size = 256; blends = 1; }

            if (tr.primaryView == null) { common.Printf("No primary view.\n"); return; }
            var primary = tr.primaryView;

            var axis = stackalloc Matrix3x3[6];
            Unsafe.InitBlock(axis, 0, (uint)(6 * sizeof(Matrix3x3)));
            axis[0][0].x = 1f;
            axis[0][1].z = 1f;
            axis[0][2].y = 1f;

            axis[1][0].x = -1f;
            axis[1][1].z = -1f;
            axis[1][2].y = 1f;

            axis[2][0].y = 1f;
            axis[2][1].x = -1f;
            axis[2][2].z = -1f;

            axis[3][0].y = -1f;
            axis[3][1].x = -1f;
            axis[3][2].z = 1f;

            axis[4][0].z = 1f;
            axis[4][1].x = -1f;
            axis[4][2].y = 1f;

            axis[5][0].z = -1f;
            axis[5][1].x = 1f;
            axis[5][2].y = 1f;

            RenderView ref_; string fullname = null;
            for (var i = 0; i < 6; i++)
            {
                ref_ = primary.renderView;
                ref_.x = ref_.y = 0;
                ref_.fov_x = ref_.fov_y = 90;
                ref_.width = glConfig.vidWidth;
                ref_.height = glConfig.vidHeight;
                ref_.viewaxis = axis[i];
                fullname = $"env/{baseName}{extensions[i]}";
                tr.TakeScreenshot(size, size, fullname, blends, ref_);
            }

            common.Printf($"Wrote {fullname}, etc\n");
        }

        #endregion

        static Matrix3x3[] cubeAxis = new Matrix3x3[6];

        static void R_SampleCubeMap(in Vector3 dir, int size, byte** buffers, byte* result)
        {
            int axis, x, y;

            var adir = stackalloc float[3];
            adir[0] = (float)Math.Abs(dir[0]);
            adir[1] = (float)Math.Abs(dir[1]);
            adir[2] = (float)Math.Abs(dir[2]);

            if (dir[0] >= adir[1] && dir[0] >= adir[2]) axis = 0;
            else if (-dir[0] >= adir[1] && -dir[0] >= adir[2]) axis = 1;
            else if (dir[1] >= adir[0] && dir[1] >= adir[2]) axis = 2;
            else if (-dir[1] >= adir[0] && -dir[1] >= adir[2]) axis = 3;
            else if (dir[2] >= adir[1] && dir[2] >= adir[2]) axis = 4;
            else axis = 5;

            var fx = dir * cubeAxis[axis][1] / (dir * cubeAxis[axis][0]);
            var fy = dir * cubeAxis[axis][2] / (dir * cubeAxis[axis][0]);

            fx = -fx;
            fy = -fy;
            x = (int)(size * 0.5f * (fx + 1));
            y = (int)(size * 0.5f * (fy + 1));
            if (x < 0) x = 0;
            else if (x >= size) x = size - 1;
            if (y < 0) y = 0;
            else if (y >= size) y = size - 1;

            result[0] = buffers[axis][(y * size + x) * 4 + 0];
            result[1] = buffers[axis][(y * size + x) * 4 + 1];
            result[2] = buffers[axis][(y * size + x) * 4 + 2];
            result[3] = buffers[axis][(y * size + x) * 4 + 3];
        }

        //============================================================================

        static void R_SetColorMappings()
        {
            RB_overbright = (r_brightness.Float * 2) - 1;
            if (RB_overbright < 1) RB_overbright = 1;
            Console.WriteLine($"RB_overbright = {RB_overbright}");
        }

        static void GfxInfo_f(CmdArgs args)
        {
            string[] fsstrings = {
                "windowed",
                "fullscreen"
            };

            common.Printf("\nGL_VENDOR: %s\n", glConfig.vendor_string);
            common.Printf("GL_RENDERER: %s\n", glConfig.renderer_string);
            common.Printf("GL_VERSION: %s\n", glConfig.version_string);
            common.Printf("GL_EXTENSIONS: %s\n", glConfig.extensions_string);
            common.Printf("GL_MAX_TEXTURE_SIZE: %d\n", glConfig.maxTextureSize);
            common.Printf("GL_MAX_TEXTURE_UNITS: %d\n", glConfig.maxTextureUnits);
            common.Printf("\nPIXELFORMAT: RGBA(%d-bits) Z(%d-bit) stencil(%d-bits)\n", glConfig.colorBits, glConfig.depthBits, glConfig.stencilBits);
            common.Printf("MODE: %d, %d x %d %s hz:", r_mode.Integer, glConfig.vidWidth, glConfig.vidHeight, fsstrings[r_fullscreen.Bool ? 1 : 0]);

            common.Printf(glConfig.displayFrequency != 0 ? $"{glConfig.displayFrequency}\n" : "N/A\n");
            common.Printf(r_finish.Bool ? "Forcing glFinish\n" : "glFinish not forced\n");
        }

        static void R_VidRestart_f(CmdArgs args)
        {
            // if OpenGL isn't started, do nothing
            if (!glConfig.isInitialized) return;

            // DG: notify the game DLL about the reloadImages and vid_restart commands
            gameCallbacks.reloadImagesCB?.Invoke(gameCallbacks.reloadImagesUserArg, args);

            var full = true;
            var forceWindow = false;
            for (var i = 1; i < args.Count; i++)
            {
                if (string.Equals(args[i], "partial", StringComparison.OrdinalIgnoreCase)) { full = false; continue; }
                if (string.Equals(args[i], "windowed", StringComparison.OrdinalIgnoreCase)) { forceWindow = true; continue; }
            }

            // this could take a while, so give them the cursor back ASAP
            SysW.GrabMouseCursor(false);

            // dump ambient caches
            renderModelManager.FreeModelVertexCaches();

            // free any current world interaction surfaces and vertex caches
            R_FreeDerivedData();

            // make sure the defered frees are actually freed
            R_ToggleSmpFrame();
            R_ToggleSmpFrame();

            // free the vertex caches so they will be regenerated again
            vertexCache.PurgeAll();

            // sound and input are tied to the window we are about to destroy
            if (full)
            {
                // free all of our texture numbers
                soundSystem.ShutdownHW();
                SysW.ShutdownInput();
                globalImages.PurgeAllImages();
                // free the context and close the window
                GLimp_Shutdown();
                glConfig.isInitialized = false;

                // create the new context and vertex cache
                var latch = cvarSystem.GetCVarBool("r_fullscreen");
                if (forceWindow) cvarSystem.SetCVarBool("r_fullscreen", false);
                R_InitOpenGL();
                cvarSystem.SetCVarBool("r_fullscreen", latch);

                // regenerate all images
                globalImages.ReloadAllImages();
            }
            else
            {
                GlimpParms parms;
                parms.width = glConfig.vidWidth;
                parms.height = glConfig.vidHeight;
                parms.fullScreen = (forceWindow) ? false : r_fullscreen.Bool;
                parms.displayHz = r_displayRefresh.Integer;
                parms.multiSamples = r_multiSamples.Integer;
                parms.stereo = false;
                GLimp_SetScreenParms(parms);
            }

            // make sure the regeneration doesn't use anything no longer valid
            tr.viewCount++;
            tr.viewDef = null;

            // regenerate all necessary interactions
            R_RegenerateWorld_f(CmdArgs.Empty);

            // check for problems
            var err = qglGetError();
            if (err != ErrorCode.NoError) common.Printf($"glGetError() = {err}\n");

            // start sound playing again
            soundSystem.SetMute(false);
        }

        public static void R_InitMaterials()
        {
            tr.defaultMaterial = declManager.FindMaterial("_default", false);
            if (tr.defaultMaterial != null) common.FatalError("_default material not found");
            declManager.FindMaterial("_default", false);

            // needed by R_DeriveLightData
            declManager.FindMaterial("lights/defaultPointLight");
            declManager.FindMaterial("lights/defaultProjectedLight");
        }

        // Keybinding command
        static void R_SizeUp_f(CmdArgs args)
            => r_screenFraction.Integer = r_screenFraction.Integer + 10 > 100 ? 100 : r_screenFraction.Integer + 10;

        // Keybinding command
        static void R_SizeDown_f(CmdArgs args)
            => r_screenFraction.Integer = r_screenFraction.Integer - 10 < 10 ? 10 : r_screenFraction.Integer - 10;

        // this is called from the main thread
        static void R_TouchGui_f(CmdArgs args)
        {
            var gui = args[1];
            if (string.IsNullOrEmpty(gui)) { common.Printf("USAGE: touchGui <guiName>\n"); return; }

            common.Printf($"touchGui {gui}\n", gui);
            session.UpdateScreen();
            uiManager.Touch(gui);
        }

        // update latched cvars here
        public static void R_InitCvars() { }

        public static void R_InitCommands()
        {
            //cmdSystem.AddCommand("MakeMegaTexture", idMegaTexture::MakeMegaTexture_f, CMD_FL.RENDERER | CMD_FL.CHEAT, "processes giant images");
            cmdSystem.AddCommand("sizeUp", R_SizeUp_f, CMD_FL.RENDERER, "makes the rendered view larger");
            cmdSystem.AddCommand("sizeDown", R_SizeDown_f, CMD_FL.RENDERER, "makes the rendered view smaller");
            cmdSystem.AddCommand("reloadGuis", R_ReloadGuis_f, CMD_FL.RENDERER, "reloads guis");
            cmdSystem.AddCommand("listGuis", R_ListGuis_f, CMD_FL.RENDERER, "lists guis");
            cmdSystem.AddCommand("touchGui", R_TouchGui_f, CMD_FL.RENDERER, "touches a gui");
            cmdSystem.AddCommand("screenshot", R_ScreenShot_f, CMD_FL.RENDERER, "takes a screenshot");
            cmdSystem.AddCommand("envshot", R_EnvShot_f, CMD_FL.RENDERER, "takes an environment shot");
            //cmdSystem.AddCommand("makeAmbientMap", R_MakeAmbientMap_f, CMD_FL.RENDERER | CMD_FL.CHEAT, "makes an ambient map");
            cmdSystem.AddCommand("benchmark", R_Benchmark_f, CMD_FL.RENDERER, "benchmark");
            cmdSystem.AddCommand("gfxInfo", GfxInfo_f, CMD_FL.RENDERER, "show graphics info");
            cmdSystem.AddCommand("modulateLights", R_ModulateLights_f, CMD_FL.RENDERER | CMD_FL.CHEAT, "modifies shader parms on all lights");
            cmdSystem.AddCommand("testImage", R_TestImage_f, CMD_FL.RENDERER | CMD_FL.CHEAT, "displays the given image centered on screen", CmdArgs.ArgCompletion_ImageName);
            cmdSystem.AddCommand("testVideo", R_TestVideo_f, CMD_FL.RENDERER | CMD_FL.CHEAT, "displays the given cinematic", CmdArgs.ArgCompletion_VideoName);
            cmdSystem.AddCommand("reportSurfaceAreas", R_ReportSurfaceAreas_f, CMD_FL.RENDERER, "lists all used materials sorted by surface area");
            cmdSystem.AddCommand("reportImageDuplication", R_ReportImageDuplication_f, CMD_FL.RENDERER, "checks all referenced images for duplications");
            cmdSystem.AddCommand("regenerateWorld", R_RegenerateWorld_f, CMD_FL.RENDERER, "regenerates all interactions");
            cmdSystem.AddCommand("showInteractionMemory", R_ShowInteractionMemory_f, CMD_FL.RENDERER, "shows memory used by interactions");
            cmdSystem.AddCommand("showTriSurfMemory", R_ShowTriSurfMemory_f, CMD_FL.RENDERER, "shows memory used by triangle surfaces");
            cmdSystem.AddCommand("vid_restart", R_VidRestart_f, CMD_FL.RENDERER, "restarts renderSystem");
            cmdSystem.AddCommand("listRenderEntityDefs", R_ListRenderEntityDefs_f, CMD_FL.RENDERER, "lists the entity defs");
            cmdSystem.AddCommand("listRenderLightDefs", R_ListRenderLightDefs_f, CMD_FL.RENDERER, "lists the light defs");
            cmdSystem.AddCommand("listModes", R_ListModes_f, CMD_FL.RENDERER, "lists all video modes");
            cmdSystem.AddCommand("reloadSurface", R_ReloadSurface_f, CMD_FL.RENDERER, "reloads the decl and images for selected surface");
        }
    }

    public unsafe partial class RenderSystemLocal : IRenderSystem
    {
        // GUI drawing variables for surface creation
        public int guiRecursionLevel;      // to prevent infinite overruns
        public GuiModel guiModel;
        public GuiModel demoGuiModel;

        // TakeScreenshot
        // Move to tr_imagefiles.c...
        // Will automatically tile render large screen shots if necessary
        // Downsample is the number of steps to mipmap the image before saving it
        // If ref == null, session.updateScreen will be used
        public override void TakeScreenshot(int width, int height, string fileName, int blends, RenderView ref_)
        {
            int i, j, c; byte temp;

            takingScreenshot = true;

            var pix = width * height;

            var buffer = new byte[pix * 4 + 18];
            fixed (byte* bufferB = buffer)
            {
                if (blends <= 1) R_ReadTiledPixels(width, height, bufferB + 18, ref_);
                else
                {
                    var shortBuffer = new ushort[pix * 4];

                    // enable anti-aliasing jitter
                    r_jitter.Bool = true;

                    for (i = 0; i < blends; i++)
                    {
                        R_ReadTiledPixels(width, height, bufferB + 18, ref_);

                        for (j = 0; j < pix * 4; j++) shortBuffer[j] += buffer[18 + j];
                    }

                    // divide back to bytes
                    for (i = 0; i < pix * 4; i++) buffer[18 + i] = (byte)(shortBuffer[i] / blends);

                    r_jitter.Bool = false;
                }
            }

            // fill in the header (this is vertically flipped, which qglReadPixels emits)
            buffer[2] = 2; // uncompressed type
            buffer[12] = (byte)(width & 255);
            buffer[13] = (byte)(width >> 8);
            buffer[14] = (byte)(height & 255);
            buffer[15] = (byte)(height >> 8);
            buffer[16] = 24; // pixel size

            // swap rgb to bgr
            c = 18 + width * height * 4;
            for (i = 18; i < c; i += 4) { temp = buffer[i]; buffer[i] = buffer[i + 2]; buffer[i + 2] = temp; }

            // _D3XP adds viewnote screenie save to cdpath
            fileSystem.WriteFile(fileName, buffer, c, fileName.Contains("viewnote") ? "fs_cdpath" : "fs_savepath");

            takingScreenshot = false;
        }

        public override void Clear()
        {
            registered = false;
            frameCount = 0;
            viewCount = 0;
            staticAllocCount = 0;
            frameShaderTime = 0f;
            viewportOffset[0] = 0;
            viewportOffset[1] = 0;
            tiledViewport[0] = 0;
            tiledViewport[1] = 0;
            ambientLightVector.Zero();
            sortOffset = 0;
            worlds.Clear();
            primaryWorld = null;
            primaryRenderView.memset();
            primaryView = null;
            defaultMaterial = null;
            testImage = null;
            ambientCubeImage = null;
            viewDef = null;
            pc.memset();
            lockSurfacesCmd.memset();
            identitySpace.memset();
            Array.Clear(renderCrops, 0, renderCrops.Length);
            currentRenderCrop = 0;
            guiRecursionLevel = 0;
            guiModel = null;
            demoGuiModel = null;
            takingScreenshot = false;
        }

        public override void Init()
        {
            // clear all our internal state
            viewCount = 1; // so cleared structures never match viewCount we used to memset tr, but now that it is a class, we can't, so there may be other state we need to reset
            hudOpacity = 1f;
            multithreadActive = r_multithread.Bool;
            useSpinLock = false;
            spinLockDelay = 500;

            ambientLightVector[0] = 0.5f;
            ambientLightVector[1] = 0.5f - 0.385f;
            ambientLightVector[2] = 0.8925f;
            ambientLightVector[3] = 1f;

            backEnd.memset();

            R_InitCvars();

            R_InitCommands();

            guiModel = new GuiModel();
            guiModel.Clear();

            demoGuiModel = new GuiModel();
            demoGuiModel.Clear();

            R_InitTriSurfData();

            globalImages.Init();

            Cinematic.InitCinematic();

            R_InitMaterials();

            renderModelManager.Init();

            // set the identity space
            identitySpace.modelMatrix[0 * 4 + 0] = 1f;
            identitySpace.modelMatrix[1 * 4 + 1] = 1f;
            identitySpace.modelMatrix[2 * 4 + 2] = 1f;
        }

        public override void Shutdown()
        {
            common.Printf("RenderSystem::Shutdown()\n");

            renderSystem.BackendThreadWait();

            R_DoneFreeType();

            if (glConfig.isInitialized) globalImages.PurgeAllImages();

            renderModelManager.Shutdown();

            Cinematic.ShutdownCinematic();

            globalImages.Shutdown();

            // free frame memory
            R_ShutdownFrameData();

            // free the vertex cache, which should have nothing allocated now
            vertexCache.Shutdown();

            R_ShutdownTriSurfData();

            guiModel = null;
            demoGuiModel = null;

            Clear();

            ShutdownOpenGL();
        }

        public override void BeginLevelLoad()
        {
            renderModelManager.BeginLevelLoad();
            globalImages.BeginLevelLoad();
        }

        public override void EndLevelLoad()
        {
            renderModelManager.EndLevelLoad();
            globalImages.EndLevelLoad();
        }

        public override void InitOpenGL()
        {
            // if OpenGL isn't started, start it now
            if (!glConfig.isInitialized)
            {
                R_InitOpenGL();

                globalImages.ReloadAllImages();

                var err = qglGetError();
                if (err != ErrorCode.NoError) common.Printf($"glGetError() = {err}\n");
            }

            R_InitFrameBuffer();
        }

        public override void ShutdownOpenGL()
        {
            // free the context and close the window
            R_ShutdownFrameData();
            //GLimp_Shutdown();
            glConfig.isInitialized = false;
        }

        public override bool IsOpenGLRunning
            => glConfig.isInitialized;

        public override bool IsFullScreen
            => glConfig.isFullscreen;

        public override int ScreenWidth
            => glConfig.vidWidth;

        public override int ScreenHeight
            => glConfig.vidHeight;

        public override void SetHudOpacity(float opacity)
            => hudOpacity = opacity;

        public override float FOV
            => throw new NotImplementedException(); // Doom3Quest_GetFOV();

        public override int Refresh
            => throw new NotImplementedException(); // Doom3Quest_GetRefresh();
    }
}