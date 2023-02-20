using OpenStack.Graphics;
using OpenStack.Graphics.OpenGL;
using System;
using System.Numerics;

namespace GameSpec.Graphics
{
    public class OpenGLMaterialBuilder : AbstractMaterialBuilder<Material, int>
    {
        public OpenGLMaterialBuilder(TextureManager<int> textureManager) : base(textureManager) { }

        Material _defaultMaterial;
        public override Material DefaultMaterial => _defaultMaterial != null ? _defaultMaterial : _defaultMaterial = BuildAutoMaterial(-1);

        Material BuildAutoMaterial(int type)
        {
            var m = new Material(null);
            m.Textures["g_tColor"] = _textureManager.DefaultTexture;
            m.Info.ShaderName = "vrf.error";
            return m;
        }

        public override Material BuildMaterial(object key)
        {
            switch (key)
            {
                case IMaterialInfo s:
                    var m = new Material(s);
                    switch (s)
                    {
                        case IFixedMaterialInfo _: return m;
                        case IParamMaterialInfo p:
                            foreach (var tex in p.TextureParams) m.Textures[tex.Key] = _textureManager.LoadTexture(tex.Value, out _);
                            if (p.IntParams.ContainsKey("F_SOLID_COLOR") && p.IntParams["F_SOLID_COLOR"] == 1)
                            {
                                var a = p.VectorParams["g_vColorTint"];
                                m.Textures["g_tColor"] = _textureManager.BuildSolidTexture(1, 1, a.X, a.Y, a.Z, a.W);
                            }
                            if (!m.Textures.ContainsKey("g_tColor")) m.Textures["g_tColor"] = _textureManager.DefaultTexture;
                            // Since our shaders only use g_tColor, we have to find at least one texture to use here
                            if (m.Textures["g_tColor"] == _textureManager.DefaultTexture)
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