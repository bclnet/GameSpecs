using grendgine_collada;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace GameX.Formats.Collada
{
    partial class ColladaFileWriter
    {
        /// <summary>
        /// Adds the Library_Visual_Scene element to the Collada document.
        /// Provides a library in which to place visual_scene elements for chr files (rigs + geometry). 
        /// </summary>
        void SetLibraryVisualScenesWithSkeleton()
        {
            // There can be multiple visual scenes.  Will just have one (World) for now.  All node chunks go under Nodes for that visual scene
            var nodes = new List<Grendgine_Collada_Node>();

            //// Check to see if there is a CompiledBones chunk.  If so, add a Node.  
            //if (File.Chunks.Any(a => a.ChunkType == ChunkTypeEnum.CompiledBones || a.ChunkType == ChunkTypeEnum.CompiledBonesSC))
            //    nodes.Add(CreateJointNode(File.Bones.RootBone));

            // Geometry visual Scene.
            var firstModel = File.Models.First();
            nodes.Add(new Grendgine_Collada_Node
            {
                ID = firstModel.Path,
                Name = firstModel.Path,
                Type = Grendgine_Collada_Node_Type.NODE,
                Matrix = new[] { new Grendgine_Collada_Matrix { Value_As_String = CreateStringFromMatrix44(Matrix4x4.Identity) } },
                Instance_Controller = new[] { new Grendgine_Collada_Instance_Controller {
                    URL = "#Controller",
                    Skeleton = new[] { new Grendgine_Collada_Skeleton { Value = "#Armature" } },
                    Bind_Material = new[] { new Grendgine_Collada_Bind_Material {
                        // This gets complicated. We need to make one instance_material for each material used in this node chunk.
                        // The mat IDs used in this node chunk are stored in meshsubsets, so for each subset we need to grab the mat, get the target (id), and make an instance_material for it.
                        Technique_Common = new Grendgine_Collada_Technique_Common_Bind_Material {
                            // For each mesh subset, we want to create an instance material and add it to instanceMaterials list.
                            Instance_Material =File.Materials.Select(material => new Grendgine_Collada_Instance_Material_Geometry
                            {
                                Target = $"#{material.Name}-material",
                                Symbol = $"{material.Name}-material"
                            }).ToArray()
                        }
                    }},
                }}
            });

            // Set up the library
            daeObject.Library_Visual_Scene = new Grendgine_Collada_Library_Visual_Scenes
            {
                Visual_Scene = new[] { new Grendgine_Collada_Visual_Scene { Node = nodes.ToArray(), ID = "Scene" } }
            };
        }
    }
}