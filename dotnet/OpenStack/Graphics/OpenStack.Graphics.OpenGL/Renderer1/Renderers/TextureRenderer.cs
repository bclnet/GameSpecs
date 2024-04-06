using OpenStack.Graphics.Renderer1;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Linq;

namespace OpenStack.Graphics.OpenGL.Renderer1.Renderers
{
    public class TextureRenderer : IRenderer
    {
        readonly IOpenGLGraphic Graphic;
        readonly int Texture;
        readonly Shader Shader;
        readonly int QuadVao;
        public bool Background;

        public AABB BoundingBox => new AABB(-1, -1, -1, 1, 1, 1);

        public TextureRenderer(IOpenGLGraphic graphic, int texture, bool background = false)
        {
            Graphic = graphic;
            Texture = texture;
            Shader = Graphic.ShaderManager.LoadPlaneShader("plane");
            QuadVao = SetupQuadBuffer();
            Background = background;
        }

        int SetupQuadBuffer()
        {
            GL.UseProgram(Shader.Program);

            // Create and bind VAO
            var vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            var vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

            var vertices = new[]
            {
                // position     :normal        :texcoord  :tangent
                -1f, -1f, +0f,  +0f, +0f, 1f,  +0f, +1f,  +1f, +0f, +0f,
                -1f, +1f, +0f,  +0f, +0f, 1f,  +0f, +0f,  +1f, +0f, +0f,
                +1f, -1f, +0f,  +0f, +0f, 1f,  +1f, +1f,  +1f, +0f, +0f,
                +1f, +1f, +0f,  +0f, +0f, 1f,  +1f, +0f,  +1f, +0f, +0f,
            };

            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0);

            var attributes = new List<(string Name, int Size)>
            {
                ("vPOSITION", 3),
                ("vNORMAL", 3),
                ("vTEXCOORD", 2),
                ("vTANGENT", 3)
            };
            var stride = sizeof(float) * attributes.Sum(x => x.Size);
            var offset = 0;
            foreach (var (Name, Size) in attributes)
            {
                var attributeLocation = GL.GetAttribLocation(Shader.Program, Name);
                if (attributeLocation > -1)
                {
                    GL.EnableVertexAttribArray(attributeLocation);
                    GL.VertexAttribPointer(attributeLocation, Size, VertexAttribPointerType.Float, false, stride, offset);
                }
                offset += sizeof(float) * Size;
            }

            GL.BindVertexArray(0); // Unbind VAO

            return vao;
        }

        public void Render(Camera camera, RenderPass renderPass)
        {
            if (Background)
            {
                GL.ClearColor(OpenTK.Color.White);
                GL.Clear(ClearBufferMask.ColorBufferBit);
            }

            GL.UseProgram(Shader.Program);
            GL.BindVertexArray(QuadVao);
            GL.EnableVertexAttribArray(0);

            if (Texture > -1)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, Texture);
            }

            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

            GL.BindVertexArray(0);
            GL.UseProgram(0);
        }

        public void Update(float frameTime) { }
    }
}
