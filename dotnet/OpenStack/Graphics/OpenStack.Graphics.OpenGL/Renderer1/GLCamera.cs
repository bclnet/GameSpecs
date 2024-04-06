using OpenStack.Graphics.Renderer1;
using OpenTK.Graphics.OpenGL;

namespace OpenStack.Graphics.OpenGL.Renderer1
{
    public abstract class GLCamera : Camera
    {
        protected override void SetViewport(int x, int y, int width, int height)
            => GL.Viewport(0, 0, width, height);
    }
}
