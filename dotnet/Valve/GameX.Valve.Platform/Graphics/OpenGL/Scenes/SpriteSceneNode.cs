using GameX.Valve.Formats;
using OpenStack;
using OpenStack.Graphics;
using OpenStack.Graphics.OpenGL.Renderer1;
using OpenStack.Graphics.Renderer1;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace GameX.Valve.Graphics.OpenGL.Scenes
{
    //was:Renderer/SpriteSceneNode
    public class SpriteSceneNode : SceneNode
    {
        readonly int quadVao;

        readonly GLRenderMaterial material;
        readonly Shader shader;
        readonly Vector3 position;
        readonly float size;

        public SpriteSceneNode(Scene scene, Binary_Pak resource, Vector3 position) : base(scene)
        {
            var graphic = scene.Graphic as IOpenGLGraphic;
            material = graphic.MaterialManager.LoadMaterial(resource, out var _);
            shader = graphic.LoadShader(material.Material.ShaderName, material.Material.GetShaderArgs());

            if (quadVao == 0) quadVao = SetupQuadBuffer();

            var paramMaterial = material.Material as IParamMaterial;
            size = paramMaterial.FloatParams.GetValueOrDefault("g_flUniformPointSize", 16);

            this.position = position;
            var size3 = new Vector3(size);
            LocalBoundingBox = new AABB(position - size3, position + size3);
        }

         int SetupQuadBuffer()
        {
            GL.UseProgram(shader.Program);

            var vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            var vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

            var vertices = new[]
            {
                // position          ; texcoord
                -1.0f, -1.0f, 0.0f,  0.0f, 1.0f,
                -1.0f, 1.0f, 0.0f,   0.0f, 0.0f,
                1.0f, -1.0f, 0.0f,   1.0f, 1.0f,
                1.0f, 1.0f, 0.0f,    1.0f, 0.0f,
            };

            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);

            var attributes = new List<(string Name, int Size)>
            {
                ("vPOSITION", 3),
                ("vTEXCOORD", 2),
            };
            var stride = sizeof(float) * attributes.Sum(x => x.Size);
            var offset = 0;

            foreach (var (Name, Size) in attributes)
            {
                var attributeLocation = GL.GetAttribLocation(shader.Program, Name);
                GL.EnableVertexAttribArray(attributeLocation);
                GL.VertexAttribPointer(attributeLocation, Size, VertexAttribPointerType.Float, false, stride, offset);
                offset += sizeof(float) * Size;
            }

            GL.BindVertexArray(0);

            return vao;
        }

        public override void Render(Scene.RenderContext context)
        {
            GL.UseProgram(shader.Program);
            GL.BindVertexArray(quadVao);

            var viewProjectionMatrix = context.Camera.ViewProjectionMatrix.ToOpenTK();
            var cameraPosition = context.Camera.Location;

            // Create billboarding rotation (always facing camera)
            Matrix4x4.Decompose(context.Camera.CameraViewMatrix, out _, out var modelViewRotation, out _);
            modelViewRotation = Quaternion.Inverse(modelViewRotation);
            var billboardMatrix = Matrix4x4.CreateFromQuaternion(modelViewRotation);

            var scaleMatrix = Matrix4x4.CreateScale(size);
            var translationMatrix = Matrix4x4.CreateTranslation(position.X, position.Y, position.Z);

            var test = billboardMatrix * scaleMatrix * translationMatrix;
            var test2 = test.ToOpenTK();

            GL.UniformMatrix4(shader.GetUniformLocation("uProjectionViewMatrix"), false, ref viewProjectionMatrix);

            var transformTk = Transform;
            GL.UniformMatrix4(shader.GetUniformLocation("transform"), false, ref test2);

            material.Render(shader);

            GL.Disable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);

            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

            material.PostRender();

            GL.Enable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);

            GL.BindVertexArray(0);
            GL.UseProgram(0);
        }

        public override void Update(Scene.UpdateContext context) { }
    }
}
