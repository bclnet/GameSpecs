using OpenStack.Graphics;
using System;
using UnityEngine;
using ur = UnityEngine.Rendering;

namespace GameX.Platforms
{
    /// <summary>
    /// A material that uses the new Standard Shader.
    /// </summary>
    public class StandardMaterialBuilder : MaterialBuilderBase<Material, Texture2D>
    {
        Material _defaultMaterial;
        Material _standardMaterial;
        Material _standardCutoutMaterial;

        public StandardMaterialBuilder(TextureManager<Texture2D> textureManager) : base(textureManager)
        {
            _standardMaterial = new Material(Shader.Find("Standard"));
            _standardCutoutMaterial = UnityEngine.Resources.Load<Material>("Materials/StandardCutout");
            _defaultMaterial = BuildMaterial();
        }

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
                        if (NormalGeneratorIntensity != null)
                        {
                            material.EnableKeyword("_NORMALMAP");
                            material.SetTexture("_BumpMap", TextureManager.BuildNormalMap((Texture2D)material.mainTexture, NormalGeneratorIntensity.Value));
                        }
                    }
                    else material.DisableKeyword("_NORMALMAP");
                    if (p.BumpFilePath != null)
                    {
                        material.EnableKeyword("_NORMALMAP");
                        material.SetTexture("_NORMALMAP", TextureManager.LoadTexture(p.BumpFilePath, out var _));
                    }
                    return material;
                case MaterialTerrain _: return BuildMaterialTerrain();
                default: throw new ArgumentOutOfRangeException(nameof(key));
            }
        }

        Material BuildMaterial()
        {
            var material = new Material(Shader.Find("Standard"));
            material.CopyPropertiesFromMaterial(_standardMaterial);
            return material;
        }

        static Material BuildMaterialTerrain() => new Material(Shader.Find("Nature/Terrain/Diffuse"));

        Material BuildMaterialBlended(ur.BlendMode srcBlendMode, ur.BlendMode dstBlendMode)
        {
            var material = BuildMaterialTested();
            //material.SetInt("_SrcBlend", (int)srcBlendMode);
            //material.SetInt("_DstBlend", (int)dstBlendMode);
            return material;
        }

        Material BuildMaterialTested(float cutoff = 0.5f)
        {
            var material = new Material(Shader.Find("Standard"));
            material.CopyPropertiesFromMaterial(_standardCutoutMaterial);
            material.SetFloat("_Cutout", cutoff);
            return material;
        }
    }
}