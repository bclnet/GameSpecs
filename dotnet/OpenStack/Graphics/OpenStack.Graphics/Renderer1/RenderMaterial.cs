using OpenStack.Graphics.Renderer1;
using System;
using System.Collections.Generic;

namespace OpenStack.Graphics.OpenGL.Renderer1
{
    //was:Render/RenderMaterial
    public abstract class RenderMaterial
    {
        public IMaterial Material { get; private set; }
        public Dictionary<string, int> Textures { get; } = new Dictionary<string, int>();
        public bool IsBlended { get; }
        public bool IsToolsMaterial { get; }

        protected float AlphaTestReference;
        protected bool IsAdditiveBlend;
        protected bool IsRenderBackfaces;

        public RenderMaterial(IMaterial material)
        {
            Material = material;
            switch (material)
            {
                case IFixedMaterial p:
                    break;
                case IParamMaterial p:
                    // # FIX: Valve specific
                    if (p.IntParams.ContainsKey("F_ALPHA_TEST") && p.IntParams["F_ALPHA_TEST"] == 1 && p.FloatParams.ContainsKey("g_flAlphaTestReference")) AlphaTestReference = p.FloatParams["g_flAlphaTestReference"];
                    IsToolsMaterial = p.IntAttributes.ContainsKey("tools.toolsmaterial");
                    IsBlended = (p.IntParams.ContainsKey("F_TRANSLUCENT") && p.IntParams["F_TRANSLUCENT"] == 1) || p.IntAttributes.ContainsKey("mapbuilder.water") || material.ShaderName == "vr_glass.vfx" || material.ShaderName == "tools_sprite.vfx";
                    IsAdditiveBlend = p.IntParams.ContainsKey("F_ADDITIVE_BLEND") && p.IntParams["F_ADDITIVE_BLEND"] == 1;
                    IsRenderBackfaces = p.IntParams.ContainsKey("F_RENDER_BACKFACES") && p.IntParams["F_RENDER_BACKFACES"] == 1;
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(material), $"{material}");
            }
        }

        public abstract void Render(Shader shader);

        public abstract void PostRender();
    }
}
