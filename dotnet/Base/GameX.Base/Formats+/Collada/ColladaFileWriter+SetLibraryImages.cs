using grendgine_collada;
using System;
using System.Linq;

namespace GameX.Formats.Collada
{
    partial class ColladaFileWriter
    {
        /// <summary>
        /// Adds the Library_Images element to the Collada document.
        /// </summary>
        void SetLibraryImages()
            // images is the array of image (Gredgine_Collada_Image) objects
            => daeObject.Library_Images = new Grendgine_Collada_Library_Images
            {
                // each mat will have a number of texture files.  Need to create an <image> for each of them.
                Image = File.Materials.SelectMany(material => material.Textures, (material, texture) =>
                {
                    // Build the URI path to the file as a .dds, clean up the slashes.
                    var path = texture.Path.Contains("/") || texture.Path.Contains("\\")
                        // if Datadir is empty, need a clean name and can only search in the current directory.
                        ? DataDir == null ? $"{CleanMtlFileName(texture.Path)}.dds"
                        // if Datadir is provided, then look there.
                        : $"/{DataDir.FullName.Replace(" ", "%20")}/{texture.Path}"
                        // else path
                        : texture.Path;
                    path = !TiffTextures
                        ? path.Replace(".tif", ".dds", StringComparison.OrdinalIgnoreCase)
                        : path.Replace(".dds", ".tif", StringComparison.OrdinalIgnoreCase);

                    // For each texture in the material, we make a new <image> object and add it to the list. 
                    return new Grendgine_Collada_Image
                    {
                        ID = $"{material.Name}_{texture.Maps}",
                        Name = $"{material.Name}_{texture.Maps}",
                        // if 1.4.1, use URI, else use Ref
                        Init_From = new Grendgine_Collada_Init_From
                        {
                            Uri = daeObject.Collada_Version == "1.4.1" ? path : null,
                            Ref = daeObject.Collada_Version != "1.4.1" ? path : null,
                        }
                    };
                }).ToArray()
            };
    }
}