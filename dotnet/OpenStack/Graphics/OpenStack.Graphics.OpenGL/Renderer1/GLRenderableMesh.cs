using OpenStack.Graphics.Renderer1;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace OpenStack.Graphics.OpenGL.Renderer1
{
    //was:Render/RenderableMesh
    public class GLRenderableMesh : RenderableMesh
    {
        IOpenGLGraphic Graphic;

        public GLRenderableMesh(IOpenGLGraphic graphic, IMesh mesh, int meshIndex, IDictionary<string, string> skinMaterials = null, IModel model = null) : base(t => ((GLRenderableMesh)t).Graphic = graphic, mesh, meshIndex, skinMaterials, model) { }

        public override void SetRenderMode(string renderMode)
        {
            foreach (var call in DrawCallsOpaque.Union(DrawCallsBlended))
            {
                // Recycle old shader parameters that are not render modes since we are scrapping those anyway
                var parameters = call.Shader.Parameters
                    .Where(kvp => !kvp.Key.StartsWith("renderMode"))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                if (renderMode != null && call.Shader.RenderModes.Contains(renderMode))
                    parameters.Add($"renderMode_{renderMode}", true);

                call.Shader = Graphic.LoadShader(call.Shader.Name, parameters);
                call.VertexArrayObject = Graphic.MeshBufferCache.GetVertexArrayObject(Mesh.VBIB, call.Shader, call.VertexBuffer.Id, call.IndexBuffer.Id, call.BaseVertex);
            }
        }

        protected override void ConfigureDrawCalls(IDictionary<string, string> skinMaterials, bool firstSetup)
        {
            var data = Mesh.Data;
            if (firstSetup) Graphic.MeshBufferCache.GetVertexIndexBuffers(VBIB);  // This call has side effects because it uploads to gpu

            // Prepare drawcalls
            var i = 0;
            foreach (var sceneObject in data.GetArray("m_sceneObjects"))
                foreach (var objectDrawCall in sceneObject.GetArray("m_drawCalls"))
                {
                    var materialName = objectDrawCall.Get<string>("m_material") ?? objectDrawCall.Get<string>("m_pMaterial");
                    if (skinMaterials != null && skinMaterials.ContainsKey(materialName)) materialName = skinMaterials[materialName];

                    var material = Graphic.MaterialManager.LoadMaterial($"{materialName}_c", out var _);
                    var isOverlay = material.Material is IParamMaterial z && z.IntParams.ContainsKey("F_OVERLAY");

                    // Ignore overlays for now
                    if (isOverlay) continue;

                    var shaderArgs = new Dictionary<string, bool>();
                    if (DrawCall.IsCompressedNormalTangent(objectDrawCall)) shaderArgs.Add("fulltangent", false);

                    if (firstSetup)
                    {
                        // TODO: Don't pass around so much shit
                        var drawCall = CreateDrawCall(objectDrawCall, shaderArgs, material);

                        DrawCallsAll.Add(drawCall);

                        if (drawCall.Material.IsBlended) DrawCallsBlended.Add(drawCall);
                        else DrawCallsOpaque.Add(drawCall);

                        continue;
                    }

                    SetupDrawCallMaterial(DrawCallsAll[i++], shaderArgs, material);
                }
        }

        DrawCall CreateDrawCall(IDictionary<string, object> objectDrawCall, IDictionary<string, bool> shaderArgs, GLRenderMaterial material)
        {
            var drawCall = new DrawCall();
            var primitiveType = objectDrawCall.Get<object>("m_nPrimitiveType");
            if (primitiveType is byte primitiveTypeByte)
            {
                if ((RenderPrimitiveType)primitiveTypeByte == RenderPrimitiveType.RENDER_PRIM_TRIANGLES) drawCall.PrimitiveType = (int)PrimitiveType.Triangles;
            }
            else if (primitiveType is string primitiveTypeString)
            {
                if (primitiveTypeString == "RENDER_PRIM_TRIANGLES") drawCall.PrimitiveType = (int)PrimitiveType.Triangles;
            }
            if (drawCall.PrimitiveType != (int)PrimitiveType.Triangles) throw new NotImplementedException($"Unknown PrimitiveType in drawCall! {primitiveType})");

            SetupDrawCallMaterial(drawCall, shaderArgs, material);

            var indexBufferObject = objectDrawCall.GetSub("m_indexBuffer");
            drawCall.IndexBuffer = (indexBufferObject.GetUInt32("m_hBuffer"), indexBufferObject.GetUInt32("m_nBindOffsetBytes"));

            var vertexElementSize = VBIB.VertexBuffers[(int)drawCall.VertexBuffer.Id].ElementSizeInBytes;
            drawCall.BaseVertex = objectDrawCall.GetUInt32("m_nBaseVertex") * vertexElementSize;
            //drawCall.VertexCount = objectDrawCall.GetUInt32("m_nVertexCount");

            var indexElementSize = VBIB.IndexBuffers[(int)drawCall.IndexBuffer.Id].ElementSizeInBytes;
            drawCall.StartIndex = objectDrawCall.GetUInt32("m_nStartIndex") * indexElementSize;
            drawCall.IndexCount = objectDrawCall.GetInt32("m_nIndexCount");

            if (objectDrawCall.ContainsKey("m_vTintColor")) drawCall.TintColor = objectDrawCall.GetVector3("m_vTintColor");

            if (indexElementSize == 2) drawCall.IndexType = (int)DrawElementsType.UnsignedShort; // shopkeeper_vr
            else if (indexElementSize == 4) drawCall.IndexType = (int)DrawElementsType.UnsignedInt; // glados
            else throw new ArgumentOutOfRangeException(nameof(indexElementSize), $"Unsupported index type {indexElementSize}");

            var vertexBuffer = objectDrawCall.GetArray("m_vertexBuffers")[0];
            drawCall.VertexBuffer = (vertexBuffer.GetUInt32("m_hBuffer"), vertexBuffer.GetUInt32("m_nBindOffsetBytes"));
            drawCall.VertexArrayObject = Graphic.MeshBufferCache.GetVertexArrayObject(VBIB, drawCall.Shader, drawCall.VertexBuffer.Id, drawCall.IndexBuffer.Id, drawCall.BaseVertex);
            return drawCall;
        }

        void SetupDrawCallMaterial(DrawCall drawCall, IDictionary<string, bool> shaderArgs, RenderMaterial material)
        {
            drawCall.Material = material;

            // Add shader parameters from material to the shader parameters from the draw call
            var combinedShaderArgs = shaderArgs
                .Concat(material.Material.GetShaderArgs())
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            // Load shader
            drawCall.Shader = Graphic.LoadShader(drawCall.Material.Material.ShaderName, combinedShaderArgs);

            // Bind and validate shader
            GL.UseProgram(drawCall.Shader.Program);

            if (!drawCall.Material.Textures.ContainsKey("g_tTintMask")) drawCall.Material.Textures.Add("g_tTintMask", Graphic.TextureManager.BuildSolidTexture(1, 1, 1f, 1f, 1f, 1f));
            if (!drawCall.Material.Textures.ContainsKey("g_tNormal")) drawCall.Material.Textures.Add("g_tNormal", Graphic.TextureManager.BuildSolidTexture(1, 1, 0.5f, 1f, 0.5f, 1f));
        }
    }
}
