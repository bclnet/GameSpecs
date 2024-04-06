using System.Runtime.InteropServices;
using WaveEngine.Bindings.OpenGLES;
using static System.NumericsX.OpenStack.Gngine.Render.QGL;
using static System.NumericsX.OpenStack.OpenStack;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static WaveEngine.Bindings.OpenGLES.GL;

namespace System.NumericsX.OpenStack.Gngine.Render
{
    public unsafe class FrameBuffer
    {
        const int FRAMEBUFFER_POOL_SIZE = 5;

        static uint* _framebuffer = (uint*)Marshal.AllocHGlobal(FRAMEBUFFER_POOL_SIZE * sizeof(uint));
        static uint* _depthbuffer = (uint*)Marshal.AllocHGlobal(FRAMEBUFFER_POOL_SIZE * sizeof(uint));

        static int _framebuffer_width, _framebuffer_height;
        static uint* _framebuffer_texture = (uint*)Marshal.AllocHGlobal(FRAMEBUFFER_POOL_SIZE * sizeof(uint));

        static int drawFboId = 0;
        static int currentFramebufferIndex = 0;

        static void R_InitFrameBuffer()
        {
            _framebuffer_width = glConfig.vidWidth;
            _framebuffer_height = glConfig.vidHeight;

            for (var i = 0; i < FRAMEBUFFER_POOL_SIZE; ++i)
            {
                // Create texture
                glGenTextures(1, &_framebuffer_texture[i]);
                glBindTexture(TextureTarget.Texture2d, _framebuffer_texture[i]);

                glTexImage2D(TextureTarget.Texture2d, 0, (int)InternalFormat.Rgba, _framebuffer_width, _framebuffer_height, 0, PixelFormat.Rgba, (PixelType)VertexAttribPointerType.UnsignedByte, null);
                glTexParameteri(TextureTarget.Texture2d, (TextureParameterName)GetTextureParameter.TextureMinFilter, (int)BlitFramebufferFilter.Linear);
                glTexParameteri(TextureTarget.Texture2d, (TextureParameterName)GetTextureParameter.TextureMagFilter, (int)BlitFramebufferFilter.Linear);

                // Create framebuffer
                glGenFramebuffers(1, &_framebuffer[i]);

                // Create renderbuffer
                glGenRenderbuffers(1, &_depthbuffer[i]);
                glBindRenderbuffer(RenderbufferTarget.Renderbuffer, _depthbuffer[i]);
                glRenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.Depth24Stencil8Oes, _framebuffer_width, _framebuffer_height);
            }
        }

        static void R_FrameBufferStart()
        {
            if (currentFramebufferIndex == 0) fixed (int* drawFboIdI = &drawFboId) glGetIntegerv(GetPName.DrawFramebufferBinding, drawFboIdI);

            // Render to framebuffer
            glBindFramebuffer(FramebufferTarget.Framebuffer, _framebuffer[currentFramebufferIndex]);
            glBindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
            glFramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2d, _framebuffer_texture[currentFramebufferIndex], 0);

            // Attach combined depth+stencil
            glFramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, _depthbuffer[currentFramebufferIndex]);
            glFramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, _depthbuffer[currentFramebufferIndex]);

            var result = glCheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (result != FramebufferStatus.FramebufferComplete) common.Error($"Error binding Framebuffer: {result}\n");

            glClearColor(0f, 0f, 0f, 1f);
            qglClear((int)AttribMask.ColorBufferBit);

            // Increment index in case this gets called again
            currentFramebufferIndex++;
        }

        static void R_FrameBufferEnd()
        {
            currentFramebufferIndex--;
            glBindFramebuffer(FramebufferTarget.Framebuffer, currentFramebufferIndex == 0 ? (uint)drawFboId : _framebuffer[currentFramebufferIndex - 1]);
        }
    }
}