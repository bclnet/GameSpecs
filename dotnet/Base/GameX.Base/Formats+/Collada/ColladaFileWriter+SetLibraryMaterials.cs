using grendgine_collada;
using System.Linq;

namespace GameX.Formats.Collada
{
    partial class ColladaFileWriter
    {
        /// <summary>
        /// Adds the Library_Materials element to the Collada document.
        /// </summary>
        void SetLibraryMaterials()
            // Create the list of materials used in this object
            => daeObject.Library_Materials = new Grendgine_Collada_Library_Materials
            {
                // Now create a material for each material in the object
                // The # in front of Material.name is needed to reference the effect in Library_effects.
                // There is just one .mtl file we need to worry about.
                Material = File.Materials.Select(material => material.Name == null
                    ? new Grendgine_Collada_Material
                    {
                        Name = File.Name, // name is blank if it's a material file with no submats. Set to file name. 
                        ID = File.Name, // need material ID here, so the meshes can reference it. Use the chunk ID.
                        Instance_Effect = new Grendgine_Collada_Instance_Effect { URL = $"#{File.Name}-effect" }
                    }
                    : new Grendgine_Collada_Material
                    {
                        Name = material.Name,
                        ID = $"{material.Name}-material", // this is the order the materials appear in the .mtl file. Needed for geometries.
                        Instance_Effect = new Grendgine_Collada_Instance_Effect { URL = $"#{material.Name}-effect" }
                    }).ToArray(),
            };
    }
}