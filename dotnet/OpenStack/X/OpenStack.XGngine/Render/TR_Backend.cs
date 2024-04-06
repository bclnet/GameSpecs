using System.NumericsX.OpenStack.System;
using WaveEngine.Bindings.OpenGLES;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.Gngine.Render.QGL;
using static System.NumericsX.OpenStack.Gngine.Render.R;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.Render
{
    partial class R
    {
        const int NUM_FRAME_DATA = 2;
        static FrameData[] smpFrameData = new FrameData[NUM_FRAME_DATA];
        static volatile uint smpFrame;

        // This should initialize all GL state that any part of the entire program may touch, including the editor.
        public static void RB_SetDefaultGLState()
        {
            // Clear value for the Depth buffer
            qglClearDepthf(1f);

            // make sure our GL state vector is set correctly
            backEnd.glState = default;
            backEnd.glState.forceGlState = true;

            // All color channels are used
            qglColorMask(true, true, true, true);

            qglEnable(EnableCap.DepthTest);
            qglEnable(EnableCap.Blend);
            qglEnable(EnableCap.ScissorTest);
            qglEnable(EnableCap.CullFace);
            qglDisable(EnableCap.StencilTest);

            qglDepthMask(true);
            qglDepthFunc(DepthFunction.Always);

            qglCullFace(CullFaceMode.FrontAndBack);

            if (r_useScissor.Bool) qglScissor(0, 0, glConfig.vidWidth, glConfig.vidHeight);

            backEnd.glState.currentTexture = -1;  // Force texture unit to be reset
            for (var i = glConfig.maxTextureUnits - 1; i >= 0; i--)
            {
                GL_SelectTexture(i);
                globalImages.BindNull();
            }
            // Last active texture is Tex0
        }

        public static void GL_CheckErrors()
        {
            if (r_ignoreGLErrors.Bool) return;

            // check for up to 10 errors pending
            ErrorCode err; string s;
            for (var i = 0; i < 10; i++)
            {
                err = qglGetError();
                if (err == ErrorCode.NoError) return;
                s = err switch
                {
                    ErrorCode.InvalidEnum => "GL_INVALID_ENUM",
                    ErrorCode.InvalidValue => "GL_INVALID_VALUE",
                    ErrorCode.InvalidOperation => "GL_INVALID_OPERATION",
                    ErrorCode.OutOfMemory => "GL_OUT_OF_MEMORY",
                    _ => err.ToString(),
                };
                if (!r_ignoreGLErrors.Bool) common.Printf($"GL_CheckErrors: {s}\n");
            }
        }

        public static void GL_SelectTexture(int unit)
        {
            if (backEnd.glState.currentTexture != unit)
            {
                qglActiveTexture((TextureUnit)((int)TextureUnit.Texture0 + unit));
                backEnd.glState.currentTexture = unit;
            }
        }

        // This handles the flipping needed when the view being rendered is a mirored view.
        public static void GL_Cull(CT cullType)
        {
            if (backEnd.glState.faceCulling == cullType) return;

            if (cullType == CT.TWO_SIDED) qglDisable(EnableCap.CullFace);
            else
            {
                if (backEnd.glState.faceCulling == CT.TWO_SIDED) qglEnable(EnableCap.CullFace);
                qglCullFace(cullType == CT.BACK_SIDED
                    ? backEnd.viewDef.isMirror ? CullFaceMode.Front : CullFaceMode.Back
                    : backEnd.viewDef.isMirror ? CullFaceMode.Back : CullFaceMode.Front);
            }

            backEnd.glState.faceCulling = cullType;
        }

        // Clears the state delta bits, so the next GL_State will set every item
        public static void GL_ClearStateDelta()
            => backEnd.glState.forceGlState = true;

        // This routine is responsible for setting the most commonly changed state
        public static void GL_State(int stateBits)
        {
            int diff;
            if (!r_useStateCaching.Bool || backEnd.glState.forceGlState)
            {
                // make sure everything is set all the time, so we can see if our delta checking is screwing up
                diff = -1;
                backEnd.glState.forceGlState = false;
            }
            else
            {
                diff = stateBits ^ backEnd.glState.glStateBits;
                if (diff == 0) return;
            }

            // check depthFunc bits
            if ((diff & (GLS_DEPTHFUNC_EQUAL | GLS_DEPTHFUNC_LESS | GLS_DEPTHFUNC_ALWAYS)) != 0)
                qglDepthFunc((stateBits & GLS_DEPTHFUNC_EQUAL) != 0 ? DepthFunction.Equal
                    : (stateBits & GLS_DEPTHFUNC_ALWAYS) != 0 ? DepthFunction.Always
                    : DepthFunction.Lequal);

            // check blend bits
            if ((diff & (GLS_SRCBLEND_BITS | GLS_DSTBLEND_BITS)) != 0)
            {
                BlendingFactor srcFactor;
                switch (stateBits & GLS_SRCBLEND_BITS)
                {
                    case GLS_SRCBLEND_ZERO: srcFactor = 0; break;
                    case GLS_SRCBLEND_ONE: srcFactor = BlendingFactor.One; break;
                    case GLS_SRCBLEND_DST_COLOR: srcFactor = BlendingFactor.DstColor; break;
                    case GLS_SRCBLEND_ONE_MINUS_DST_COLOR: srcFactor = BlendingFactor.OneMinusDstColor; break;
                    case GLS_SRCBLEND_SRC_ALPHA: srcFactor = BlendingFactor.SrcAlpha; break;
                    case GLS_SRCBLEND_ONE_MINUS_SRC_ALPHA: srcFactor = BlendingFactor.OneMinusSrcAlpha; break;
                    case GLS_SRCBLEND_DST_ALPHA: srcFactor = BlendingFactor.DstAlpha; break;
                    case GLS_SRCBLEND_ONE_MINUS_DST_ALPHA: srcFactor = BlendingFactor.OneMinusDstAlpha; break;
                    case GLS_SRCBLEND_ALPHA_SATURATE: srcFactor = BlendingFactor.SrcAlphaSaturate; break;
                    default: srcFactor = BlendingFactor.One; common.Error("GL_State: invalid src blend state bits\n"); break;
                }
                BlendingFactor dstFactor;
                switch (stateBits & GLS_DSTBLEND_BITS)
                {
                    case GLS_DSTBLEND_ZERO: dstFactor = 0; break;
                    case GLS_DSTBLEND_ONE: dstFactor = BlendingFactor.One; break;
                    case GLS_DSTBLEND_SRC_COLOR: dstFactor = BlendingFactor.SrcColor; break;
                    case GLS_DSTBLEND_ONE_MINUS_SRC_COLOR: dstFactor = BlendingFactor.OneMinusSrcColor; break;
                    case GLS_DSTBLEND_SRC_ALPHA: dstFactor = BlendingFactor.SrcAlpha; break;
                    case GLS_DSTBLEND_ONE_MINUS_SRC_ALPHA: dstFactor = BlendingFactor.OneMinusSrcAlpha; break;
                    case GLS_DSTBLEND_DST_ALPHA: dstFactor = BlendingFactor.DstAlpha; break;
                    case GLS_DSTBLEND_ONE_MINUS_DST_ALPHA: dstFactor = BlendingFactor.OneMinusConstantAlpha; break;
                    default: dstFactor = BlendingFactor.One; common.Error("GL_State: invalid dst blend state bits\n"); break;
                }
                qglBlendFunc(srcFactor, dstFactor);
            }

            // check depthmask
            if ((diff & GLS_DEPTHMASK) != 0)
                qglDepthMask((stateBits & GLS_DEPTHMASK) == 0);

            // check colormask
            if ((diff & (GLS_REDMASK | GLS_GREENMASK | GLS_BLUEMASK | GLS_ALPHAMASK)) != 0)
                qglColorMask((stateBits & GLS_REDMASK) == 0,
                    (stateBits & GLS_GREENMASK) == 0,
                    (stateBits & GLS_BLUEMASK) == 0,
                    (stateBits & GLS_ALPHAMASK) == 0);

            backEnd.glState.glStateBits = stateBits;
        }

        #region RENDER BACK END THREAD FUNCTIONS

        static void RB_SetBuffer(SetBufferCommand cmd)
        {
            // see which draw buffer we want to render the frame to
            backEnd.frameCount = cmd.frameCount;

            // Disabled for OES2
            //qglDrawBuffer(cmd.buffer);

            GLimp_SetupFrame(cmd.buffer);

            // clear screen for debugging automatically enable this with several other debug tools that might leave unrendered portions of the screen
            if (R.r_clear.Float != 0f || R.r_clear.String.Length != 1 || r_lockSurfaces.Bool || r_singleArea.Bool)
            {
                if (TextScanFormatted.Scan(R.r_clear.String, "%f %f %f", out float c0, out float c1, out float c2) == 3) qglClearColor(c0, c1, c2, 1f);
                else if (R.r_clear.Integer == 2) qglClearColor(0f, 0f, 0f, 1f);
                else qglClearColor(0.4f, 0f, 0.25f, 1f);
                qglClear((uint)AttribMask.ColorBufferBit);
            }
        }

        static void RB_SwapBuffers(EmptyCommand data)
        {
#if WEBGL
            // GAB Note Dec 2018: Clear the Alpha channel, so that final render will not blend with the HTML5 background (canvas with premultiplied alpha)
            qglColorMask(false, false, false, true);
            qglClear(GL_COLOR_BUFFER_BIT);
#endif

            // force a gl sync if requested
            if (R.r_finish.Bool) qglFinish();

            // don't flip if drawing to front buffer
            GLimp_SwapBuffers();
        }

        // Copy part of the current framebuffer to an image
        static void RB_CopyRender(CopyRenderCommand cmd)
        {
            if (r_skipCopyTexture.Bool) return;

            cmd.image?.CopyFramebuffer(cmd.x, cmd.y, cmd.imageWidth, cmd.imageHeight, false);
        }

        // This function will be called syncronously if running without smp extensions, or asyncronously by another thread.
        static int RB_ExecuteBackEndCommands_backEndStartTime, RB_ExecuteBackEndCommands_backEndFinishTime;
        public static void RB_ExecuteBackEndCommands(EmptyCommand cmds)
        {
            // r_debugRenderToTexture
            int c_draw3d = 0, c_draw2d = 0, c_setBuffers = 0, c_swapBuffers = 0, c_copyRenders = 0;

            if (cmds.commandId == RC.NOP && cmds.next == null) return;

            var cmd = cmds;
            RB_ExecuteBackEndCommands_backEndStartTime = SysW.Milliseconds;

            // needed for editor rendering
            RB_SetDefaultGLState();

            for (; cmd != null; cmd = cmd.next)
                switch (cmd.commandId)
                {
                    case RC.NOP: break;
                    case RC.DRAW_VIEW: RB_DrawView((DrawSurfsCommand)cmd); if (((DrawSurfsCommand)cmd).viewDef.viewEntitys != null) c_draw3d++; else c_draw2d++; break;
                    case RC.SET_BUFFER: RB_SetBuffer((SetBufferCommand)cmd); c_setBuffers++; break;
                    case RC.SWAP_BUFFERS: RB_SwapBuffers(cmd); c_swapBuffers++; break;
                    case RC.COPY_RENDER: RB_CopyRender((CopyRenderCommand)cmd); c_copyRenders++; break;
                    case RC.DIRECT_BUFFER_START: R_FrameBufferStart(); break;
                    case RC.DIRECT_BUFFER_END: R_FrameBufferEnd(); break;
                    default: common.Error("RB_ExecuteBackEndCommands: bad commandId"); break;
                }

            // stop rendering on this thread
            RB_ExecuteBackEndCommands_backEndFinishTime = SysW.Milliseconds;
            backEnd.pc.msec = RB_ExecuteBackEndCommands_backEndFinishTime - RB_ExecuteBackEndCommands_backEndStartTime;

            if (r_debugRenderToTexture.Integer == 1)
            {
                common.Printf($"3d: {c_draw3d}, 2d: {c_draw2d}, SetBuf: {c_setBuffers}, SwpBuf: {c_swapBuffers}, CpyRenders: {c_copyRenders}, CpyFrameBuf: {backEnd.c_copyFrameBuffer}\n");
                backEnd.c_copyFrameBuffer = 0;
            }
        }

        #endregion
    }
}