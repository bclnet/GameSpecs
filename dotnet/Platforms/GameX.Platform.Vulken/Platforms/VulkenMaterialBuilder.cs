using OpenStack.Graphics;
using OpenStack.Graphics.OpenGL.Renderer1;
using System;
using System.Numerics;

namespace GameX.Platforms
{
    public class VulkenMaterialBuilder : MaterialBuilderBase<GLRenderMaterial, int>
    {
        public VulkenMaterialBuilder(TextureManager<int> textureManager) : base(textureManager) { }

        GLRenderMaterial _defaultMaterial;
        public override GLRenderMaterial DefaultMaterial => _defaultMaterial != null ? _defaultMaterial : _defaultMaterial = BuildAutoMaterial(-1);

        GLRenderMaterial BuildAutoMaterial(int type)
        {
            var m = new GLRenderMaterial(null);
            m.Textures["g_tColor"] = TextureManager.DefaultTexture;
            m.Material.ShaderName = "vrf.error";
            return m;
        }

        public override GLRenderMaterial BuildMaterial(object key)
        {
            switch (key)
            {
                case IMaterial s:
                    var m = new GLRenderMaterial(s);
                    switch (s)
                    {
                        case IFixedMaterial _: return m;
                        case IParamMaterial p:
                            foreach (var tex in p.TextureParams) m.Textures[tex.Key] = TextureManager.LoadTexture(tex.Value, out _);
                            if (p.IntParams.ContainsKey("F_SOLID_COLOR") && p.IntParams["F_SOLID_COLOR"] == 1)
                            {
                                var a = p.VectorParams["g_vColorTint"];
                                m.Textures["g_tColor"] = TextureManager.BuildSolidTexture(1, 1, a.X, a.Y, a.Z, a.W);
                            }
                            if (!m.Textures.ContainsKey("g_tColor")) m.Textures["g_tColor"] = TextureManager.DefaultTexture;
                            // Since our shaders only use g_tColor, we have to find at least one texture to use here
                            if (m.Textures["g_tColor"] == TextureManager.DefaultTexture)
                                foreach (var name in new[] { "g_tColor2", "g_tColor1", "g_tColorA", "g_tColorB", "g_tColorC" })
                                    if (m.Textures.ContainsKey(name))
                                    {
                                        m.Textures["g_tColor"] = m.Textures[name];
                                        break;
                                    }

                            // Set default values for scale and positions
                            if (!p.VectorParams.ContainsKey("g_vTexCoordScale")) p.VectorParams["g_vTexCoordScale"] = Vector4.One;
                            if (!p.VectorParams.ContainsKey("g_vTexCoordOffset")) p.VectorParams["g_vTexCoordOffset"] = Vector4.Zero;
                            if (!p.VectorParams.ContainsKey("g_vColorTint")) p.VectorParams["g_vColorTint"] = Vector4.One;
                            return m;
                        default: throw new ArgumentOutOfRangeException(nameof(s));
                    }
                default: throw new ArgumentOutOfRangeException(nameof(key));
            }
        }

    }
}