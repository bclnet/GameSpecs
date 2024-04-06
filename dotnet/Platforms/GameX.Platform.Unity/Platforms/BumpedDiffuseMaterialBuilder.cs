using OpenStack.Graphics;
using System;
using UnityEngine;
using ur = UnityEngine.Rendering;

namespace GameX.Platforms
{
    /// <summary>
    /// A material that uses the legacy Bumped Diffuse Shader.
    /// </summary>
    public class BumpedDiffuseMaterialBuilder : MaterialBuilderBase<Material, Texture2D>
    {
        static readonly Material _defaultMaterial = BuildMaterial();

        public BumpedDiffuseMaterialBuilder(ITextureManager<Texture2D> textureManager) : base(textureManager) { }

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
                    if (p.MainFilePath != null)
                    {
                        material.mainTexture = TextureManager.LoadTexture(p.MainFilePath, out var _);
                        if (NormalGeneratorIntensity != null) material.SetTexture("_BumpMap", TextureManager.BuildNormalMap((Texture2D)material.mainTexture, NormalGeneratorIntensity.Value));
                    }
                    if (p.BumpFilePath != null) material.SetTexture("_BumpMap", TextureManager.LoadTexture(p.BumpFilePath, out var _));
                    return material;
                case MaterialTerrain _: return BuildMaterialTerrain();
                default: throw new ArgumentOutOfRangeException(nameof(key));
            }
        }

        static Material BuildMaterial() => new Material(Shader.Find("Legacy Shaders/Bumped Diffuse"));

        static Material BuildMaterialTerrain() => new Material(Shader.Find("Nature/Terrain/Diffuse"));

        static Material BuildMaterialBlended(ur.BlendMode srcBlendMode, ur.BlendMode dstBlendMode)
        {
            var material = new Material(Shader.Find("Legacy Shaders/Transparent/Cutout/Bumped Diffuse"));
            material.SetInt("_SrcBlend", (int)srcBlendMode);
            material.SetInt("_DstBlend", (int)dstBlendMode);
            return material;
        }

        static Material BuildMaterialTested(float cutoff = 0.5f)
        {
            var material = new Material(Shader.Find("Legacy Shaders/Transparent/Cutout/Bumped Diffuse"));
            material.SetFloat("_AlphaCutoff", cutoff);
            return material;
        }
    }
}