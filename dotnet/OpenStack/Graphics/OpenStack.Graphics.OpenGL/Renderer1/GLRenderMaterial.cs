using OpenStack.Graphics.Renderer1;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OpenStack.Graphics.OpenGL.Renderer1
{
    //was:Render/RenderMaterial
    public class GLRenderMaterial : RenderMaterial
    {
        public GLRenderMaterial(IMaterial material) : base(material) { }

        public override void Render(Shader shader)
        {
            // Start at 1, texture unit 0 is reserved for the animation texture
            var textureUnit = 1;
            int uniformLocation;
            foreach (var texture in Textures)
            {
                uniformLocation = shader.GetUniformLocation(texture.Key);
                if (uniformLocation > -1)
                {
                    GL.ActiveTexture(TextureUnit.Texture0 + textureUnit);
                    GL.BindTexture(TextureTarget.Texture2D, texture.Value);
                    GL.Uniform1(uniformLocation, textureUnit);
                    textureUnit++;
                }
            }

            switch (Material)
            {
                case IParamMaterial p:
                    foreach (var param in p.FloatParams)
                    {
                        uniformLocation = shader.GetUniformLocation(param.Key);
                        if (uniformLocation > -1)
                            GL.Uniform1(uniformLocation, param.Value);
                    }

                    foreach (var param in p.VectorParams)
                    {
                        uniformLocation = shader.GetUniformLocation(param.Key);
                        if (uniformLocation > -1)
                            GL.Uniform4(uniformLocation, new Vector4(param.Value.X, param.Value.Y, param.Value.Z, param.Value.W));
                    }
                    break;
            }

            var alphaReference = shader.GetUniformLocation("g_flAlphaTestReference");
            if (alphaReference > -1) GL.Uniform1(alphaReference, AlphaTestReference);

            if (IsBlended)
            {
                GL.DepthMask(false);
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, IsAdditiveBlend ? BlendingFactor.One : BlendingFactor.OneMinusSrcAlpha);
            }

            if (IsRenderBackfaces) GL.Disable(EnableCap.CullFace);
        }

        public override void PostRender()
        {
            if (IsBlended) { GL.DepthMask(true); GL.Disable(EnableCap.Blend); }
            if (IsRenderBackfaces) GL.Enable(EnableCap.CullFace);
        }
    }
}
