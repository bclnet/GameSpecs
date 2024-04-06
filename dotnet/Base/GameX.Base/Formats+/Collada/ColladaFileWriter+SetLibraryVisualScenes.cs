using grendgine_collada;
using System.Collections.Generic;
using System.Linq;

namespace GameX.Formats.Collada
{
    partial class ColladaFileWriter
    {
        /// <summary>
        /// Adds the Library_Visual_Scene element to the Collada document.
        /// Provides a library in which to place visual_scene elements. 
        /// </summary>
        public void SetLibraryVisualScenes()
        {
            // There can be multiple visual scenes.  Will just have one (World) for now.  All node chunks go under Nodes for that visual scene
            var nodes = new List<Grendgine_Collada_Node>();

            //// Check to see if there is a CompiledBones chunk.  If so, add a Node.
            //if (File.Chunks.Any(a => a.ChunkType == ChunkTypeEnum.CompiledBones || a.ChunkType == ChunkTypeEnum.CompiledBonesSC))
            //    nodes.Add(CreateJointNode(File.Bones.RootBone));

            //// Geometry visual Scene.
            //if (File.Models.Count() > 1) // Star Citizen model with .cga/.cgam pair.
            //{
            //    // First model file (.cga or .cgf) will contain the main Root Node, along with all non geometry Node chunks (placeholders).
            //    // Second one will have all the datastreams, but needs to be tied to the RootNode of the first model.
            //    // THERE CAN BE MULTIPLE ROOT NODES IN EACH FILE!  Check to see if the parentnodeid ~0 and be sure to add a node for it.
            //    var positionNodes = new List<Grendgine_Collada_Node>();        // For SC files, these are the nodes in the .cga/.cgf files.
            //    foreach (var root in File.Models[0].NodeMap.Values.Where(a => a.ParentNodeID == ~0))
            //        positionNodes.Add(CreateNode(root));
            //    nodes.AddRange(positionNodes.ToArray());
            //}
            //else nodes.Add(CreateNode(File.RootNode));

            // Set up the library
            daeObject.Library_Visual_Scene = new Grendgine_Collada_Library_Visual_Scenes
            {
                Visual_Scene = new[] { new Grendgine_Collada_Visual_Scene { Node = nodes.ToArray(), ID = "Scene" } }
            };
        }
    }
}