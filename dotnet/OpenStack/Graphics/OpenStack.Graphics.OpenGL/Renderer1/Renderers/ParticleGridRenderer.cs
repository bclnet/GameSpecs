using OpenStack.Graphics.Renderer1;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Numerics;

namespace OpenStack.Graphics.OpenGL.Renderer1.Renderers
{
    public class ParticleGridRenderer : IRenderer
    {
        readonly int _vao;
        readonly Shader _shader;

        readonly int _vertexCount;

        public AABB BoundingBox { get; }

        public ParticleGridRenderer(float cellWidth, int gridWidthInCells, IOpenGLGraphic graphic)
        {
            BoundingBox = new AABB(
                new Vector3(-cellWidth * 0.5f * gridWidthInCells, -cellWidth * 0.5f * gridWidthInCells, 0),
                new Vector3(cellWidth * 0.5f * gridWidthInCells, cellWidth * 0.5f * gridWidthInCells, 0));

            var vertices = GenerateGridVertexBuffer(cellWidth, gridWidthInCells);

            _vertexCount = vertices.Length / 3; // Number of vertices in our buffer
            const int Stride = sizeof(float) * 7;

            _shader = graphic.LoadShader("vrf.grid");

            GL.UseProgram(_shader.Program);

            // Create and bind VAO
            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            var vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0);

            var positionAttributeLocation = GL.GetAttribLocation(_shader.Program, "aVertexPosition");
            GL.EnableVertexAttribArray(positionAttributeLocation);
            GL.VertexAttribPointer(positionAttributeLocation, 3, VertexAttribPointerType.Float, false, Stride, 0);

            var colorAttributeLocation = GL.GetAttribLocation(_shader.Program, "aVertexColor");
            GL.EnableVertexAttribArray(colorAttributeLocation);
            GL.VertexAttribPointer(colorAttributeLocation, 4, VertexAttribPointerType.Float, false, Stride, sizeof(float) * 3);

            GL.BindVertexArray(0); // Unbind VAO
            GL.UseProgram(0);
        }

        public void Update(float frameTime) { } // not required

        public void Render(Camera camera, RenderPass renderPass)
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.UseProgram(_shader.Program);

            var projectionViewMatrix = camera.ViewProjectionMatrix.ToOpenTK();
            GL.UniformMatrix4(_shader.GetUniformLocation("uProjectionViewMatrix"), false, ref projectionViewMatrix);

            GL.BindVertexArray(_vao);
            GL.EnableVertexAttribArray(0);

            GL.DrawArrays(PrimitiveType.Lines, 0, _vertexCount);

            GL.BindVertexArray(0);
            GL.UseProgram(0);
            GL.Disable(EnableCap.Blend);
        }

        static float[] GenerateGridVertexBuffer(float cellWidth, int gridWidthInCells)
        {
            var gridVertices = new List<float>();

            var width = cellWidth * gridWidthInCells;
            var color = new[] { 1.0f, 1.0f, 1.0f, 1.0f };

            for (var i = 0; i <= gridWidthInCells; i++)
            {
                gridVertices.AddRange(new[] { width, i * cellWidth, 0 });
                gridVertices.AddRange(color);
                gridVertices.AddRange(new[] { -width, i * cellWidth, 0 });
                gridVertices.AddRange(color);
            }

            for (var i = 1; i <= gridWidthInCells; i++)
            {
                gridVertices.AddRange(new[] { width, -i * cellWidth, 0 });
                gridVertices.AddRange(color);
                gridVertices.AddRange(new[] { -width, -i * cellWidth, 0 });
                gridVertices.AddRange(color);
            }

            for (var i = 0; i <= gridWidthInCells; i++)
            {
                gridVertices.AddRange(new[] { i * cellWidth, width, 0 });
                gridVertices.AddRange(color);
                gridVertices.AddRange(new[] { i * cellWidth, -width, 0 });
                gridVertices.AddRange(color);
            }

            for (var i = 1; i <= gridWidthInCells; i++)
            {
                gridVertices.AddRange(new[] { -i * cellWidth, width, 0 });
                gridVertices.AddRange(color);
                gridVertices.AddRange(new[] { -i * cellWidth, -width, 0 });
                gridVertices.AddRange(color);
            }

            return gridVertices.ToArray();
        }
    }
}
