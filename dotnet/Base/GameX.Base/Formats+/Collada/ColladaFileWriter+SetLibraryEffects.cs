using GameX.Formats.Unknown;
using grendgine_collada;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Xml;

namespace GameX.Formats.Collada
{
    partial class ColladaFileWriter
    {
        /// <summary>
        /// Adds the Library_Effects element to the Collada document.
        /// </summary>
        void SetLibraryEffects()
            // The Effects library. This is actual material stuff
            => daeObject.Library_Effects = new Grendgine_Collada_Library_Effects
            {
                Effect = File.Materials.Select(material =>
                {
                    #region MATERIALS : SAMPLER AND SURFACE

                    // Check to see if the texture exists, and if so make a sampler and surface.
                    var newParams = new List<Grendgine_Collada_New_Param>();
                    foreach (var texture in material.Textures)
                    {
                        // Add the Surface node
                        Grendgine_Collada_New_Param texSurface;
                        newParams.Add(texSurface = new Grendgine_Collada_New_Param
                        {
                            sID = $"{CleanMtlFileName(texture.Path)}-surface",
                            Surface = new Grendgine_Collada_Surface
                            {
                                Type = "2D",
                                //Init_From = new Grendgine_Collada_Init_From { Uri = CleanName(texture.File) },
                                Init_From = new Grendgine_Collada_Init_From { Uri = $"{material.Name}_{texture.Maps}" },
                            },
                        });
                        // Add the Sampler node
                        newParams.Add(new Grendgine_Collada_New_Param
                        {
                            sID = $"{CleanMtlFileName(texture.Path)}-sampler",
                            Sampler2D = new Grendgine_Collada_Sampler2D { Source = texSurface.sID }
                        });
                    }

                    #endregion

                    #region TECHNIQUE

                    // Make the techniques for the profile
                    var phong = new Grendgine_Collada_Phong
                    {
                        Diffuse = new Grendgine_Collada_FX_Common_Color_Or_Texture_Type(),
                        Specular = new Grendgine_Collada_FX_Common_Color_Or_Texture_Type(),
                        Emission = new Grendgine_Collada_FX_Common_Color_Or_Texture_Type
                        {
                            Color = new Grendgine_Collada_Color { sID = "emission", Value_As_String = $"{material.Emissive}".Replace(",", " ") }
                        },
                        Shininess = new Grendgine_Collada_FX_Common_Float_Or_Param_Type
                        {
                            Float = new Grendgine_Collada_SID_Float { sID = "shininess", Value = material.Shininess }
                        },
                        Index_Of_Refraction = new Grendgine_Collada_FX_Common_Float_Or_Param_Type
                        {
                            Float = new Grendgine_Collada_SID_Float()
                        },
                        Transparent = new Grendgine_Collada_FX_Common_Color_Or_Texture_Type
                        {
                            Color = new Grendgine_Collada_Color { Value_As_String = $"{1 - material.Opacity}" }, // Subtract from 1 for proper value.
                            Opaque = new Grendgine_Collada_FX_Opaque_Channel()
                        }
                    };
                    var technique = new Grendgine_Collada_Effect_Technique_COMMON { Phong = phong, sID = "common" };

                    // Add all the emissive, etc features to the phong
                    // Need to check if a texture exists. If so, refer to the sampler. Should be a <Texture Map="Diffuse" line if there is a map.
                    bool hasDiffuse = false, hasSpecular = false;
                    foreach (var texture in material.Textures)
                    {
                        //Console.WriteLine("Processing material texture {0}", CleanName(texture.File));
                        if ((texture.Maps & IUnknownTexture.Map.Diffuse) != 0)
                        {
                            hasDiffuse = true;
                            // Texcoord is the ID of the UV source in geometries.  Not needed.
                            phong.Diffuse.Texture = new Grendgine_Collada_Texture { Texture = $"{CleanMtlFileName(texture.Path)}-sampler", TexCoord = string.Empty };
                        }
                        if ((texture.Maps & IUnknownTexture.Map.Specular) != 0)
                        {
                            hasSpecular = true;
                            phong.Specular.Texture = new Grendgine_Collada_Texture { Texture = $"{CleanMtlFileName(texture.Path)}-sampler", TexCoord = string.Empty };
                        }
                        if ((texture.Maps & IUnknownTexture.Map.Bumpmap) != 0)
                            technique.Extra = new[] { new Grendgine_Collada_Extra
                            {
                                Technique = new[] { new Grendgine_Collada_Technique
                                {
                                    profile = "FCOLLADA",
                                    Data = new XmlElement[] { new Grendgine_Collada_BumpMap
                                    {
                                        Textures = new[] { new Grendgine_Collada_Texture { Texture = $"{CleanMtlFileName(texture.Path)}-sampler" } }
                                    }}
                                }}
                            }};
                    }
                    if (!hasDiffuse) phong.Diffuse.Color = new Grendgine_Collada_Color { sID = "diffuse", Value_As_String = $"{material.Diffuse}".Replace(",", " ") };
                    if (!hasSpecular) phong.Specular.Color = new Grendgine_Collada_Color { sID = "specular", Value_As_String = $"{material.Diffuse ?? Vector3.One}".Replace(",", " ") };

                    #endregion

                    // libraryEffects contains a number of effects objects.  One effects object for each material.
                    return new Grendgine_Collada_Effect
                    {
                        ID = material.Name + "-effect",
                        Name = material.Name,
                        Profile_COMMON = new[] { new Grendgine_Collada_Profile_COMMON { Technique = technique, New_Param = newParams.ToArray() } },
                    };
                }).ToArray()
            };
    }
}