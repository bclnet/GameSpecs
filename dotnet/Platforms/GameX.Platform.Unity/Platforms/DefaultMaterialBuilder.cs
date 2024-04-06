using OpenStack.Graphics;
using System;
using UnityEngine;
using ur = UnityEngine.Rendering;

namespace GameX.Platforms
{
    /// <summary>
    /// A material that uses the default shader created for TESUnity.
    /// </summary>
    public class DefaultMaterialBuilder : MaterialBuilderBase<Material, Texture2D>
    {
        static readonly Material _defaultMaterial = BuildMaterial();

        public DefaultMaterialBuilder(TextureManager<Texture2D> textureManager) : base(textureManager) { }

        public override Material DefaultMaterial => _defaultMaterial;

        public override Material BuildMaterial(object key)
        {
            switch (key)
            {
                case null: return BuildMaterial();
                case IFixedMaterial p:
                    Material material;
                    if (p.AlphaBlended) material = BuildMaterialBlended((ur.BlendMode)p.SrcBlendMode, (ur.BlendMode)p.DstBlendMode);
                    else if (p.AlphaTest) material = BuildMaterialTested(p.AlphaCutoff);
                    else material = BuildMaterial();
                    if (p.MainFilePath != null && material.HasProperty("_MainTex")) material.SetTexture("_MainTex", TextureManager.LoadTexture(p.MainFilePath, out var _));
                    if (p.DetailFilePath != null && material.HasProperty("_DetailTex")) material.SetTexture("_DetailTex", TextureManager.LoadTexture(p.DetailFilePath, out var _));
                    if (p.DarkFilePath != null && material.HasProperty("_DarkTex")) material.SetTexture("_DarkTex", TextureManager.LoadTexture(p.DarkFilePath, out var _));
                    if (p.GlossFilePath != null && material.HasProperty("_GlossTex")) material.SetTexture("_GlossTex", TextureManager.LoadTexture(p.GlossFilePath, out var _));
                    if (p.GlowFilePath != null && material.HasProperty("_Glowtex")) material.SetTexture("_Glowtex", TextureManager.LoadTexture(p.GlowFilePath, out var _));
                    if (p.BumpFilePath != null && material.HasProperty("_BumpTex")) material.SetTexture("_BumpTex", TextureManager.LoadTexture(p.BumpFilePath, out var _));
                    if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0f);
                    if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", 0f);
                    return material;
                case MaterialTerrain _: return BuildMaterialTerrain();
                default: throw new ArgumentOutOfRangeException(nameof(key));
            }
        }

        static Material BuildMaterial() => new Material(Shader.Find("TES Unity/Standard"));

        static Material BuildMaterialTerrain() => new Material(Shader.Find("Nature/Terrain/Diffuse"));

        static Material BuildMaterialBlended(ur.BlendMode sourceBlendMode, ur.BlendMode destinationBlendMode)
        {
            var material = new Material(Shader.Find("TES Unity/Alpha Blended"));
            material.SetInt("_SrcBlend", (int)sourceBlendMode);
            material.SetInt("_DstBlend", (int)destinationBlendMode);
            return material;
        }

        static Material BuildMaterialTested(float cutoff = 0.5f)
        {
            var material = new Material(Shader.Find("TES Unity/Alpha Tested"));
            material.SetFloat("_Cutoff", cutoff);
            return material;
        }
    }
}