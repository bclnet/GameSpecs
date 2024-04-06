using grendgine_collada;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace GameX.Formats.Collada
{
    partial class ColladaFileWriter
    {
        /// <summary>
        /// Adds the Library_Controllers element to the Collada document.
        /// </summary>
        void SetLibraryControllers()
        {
            var compiledBones = File.SkinningInfo.CompiledBones;
            var intVertexs = File.SkinningInfo.IntVertexs;
            var boneMaps = File.SkinningInfo.BoneMaps;
            var ext2IntMaps = File.SkinningInfo.Ext2IntMaps;

            // SOURCES : JOINTS 
            var sources = new List<Grendgine_Collada_Source>();
            var boneNames = new StringBuilder();
            foreach (var compiledBone in compiledBones) boneNames.Append(compiledBone.Name.Replace(' ', '_') + " ");
            sources.Add(new Grendgine_Collada_Source
            {
                ID = "Controller-joints",
                Name_Array = new Grendgine_Collada_Name_Array { ID = "Controller-joints-array", Count = compiledBones.Count, Value_Pre_Parse = boneNames.ToString().TrimEnd() },
                Technique_Common = new Grendgine_Collada_Technique_Common_Source
                {
                    Accessor = new Grendgine_Collada_Accessor { Source = "#Controller-joints-array", Count = (uint)compiledBones.Count, Stride = 1 }
                }
            });

            // SOURCES : BIND POSE
            sources.Add(new Grendgine_Collada_Source
            {
                ID = "Controller-bind_poses",
                Float_Array = new Grendgine_Collada_Float_Array { ID = "Controller-bind_poses-array", Count = compiledBones.Count * 16, Value_As_String = GetBindPoseArray(compiledBones) },
                Technique_Common = new Grendgine_Collada_Technique_Common_Source
                {
                    Accessor = new Grendgine_Collada_Accessor { Source = "#Controller-bind_poses-array", Count = (uint)compiledBones.Count, Stride = 16, Param = new[] { new Grendgine_Collada_Param { Name = "TRANSFORM", Type = "float4x4" } } }
                }
            });

            // SOURCES : WEIGHTS 
            var weights = new StringBuilder();
            var weightsCount = intVertexs == null ? boneMaps.Length : ext2IntMaps.Length;
            // This is a case where there are bones, and only Bone Mapping data from a datastream chunk.  Skin files.
            if (intVertexs == null)
                foreach (var boneMap in boneMaps)
                    for (var j = 0; j < 4; j++) weights.Append(((float)boneMap.Weight[j] / 255).ToString() + " ");
            // Bones and int verts. Will use int verts for weights, but this doesn't seem perfect either.
            else
                foreach (var ext2IntMap in ext2IntMaps)
                    for (var j = 0; j < 4; j++) weights.Append(intVertexs[ext2IntMap].Weights[j] + " ");
            sources.Add(new Grendgine_Collada_Source
            {
                ID = "Controller-weights",
                Float_Array = new Grendgine_Collada_Float_Array { ID = "Controller-weights-array", Count = weightsCount, Value_As_String = CleanNumbers(weights.ToString()).TrimEnd() },
                Technique_Common = new Grendgine_Collada_Technique_Common_Source
                {
                    Accessor = new Grendgine_Collada_Accessor { Source = "#Controller-weights-array", Count = (uint)weightsCount * 4, Stride = 1, Param = new[] { new Grendgine_Collada_Param { Name = "WEIGHT", Type = "float" } } }
                }
            });

            // JOINTS
            var joints = new Grendgine_Collada_Joints
            {
                Input = new[] {
                    new Grendgine_Collada_Input_Unshared { Semantic = Grendgine_Collada_Input_Semantic.JOINT, source = "#Controller-joints" },
                    new Grendgine_Collada_Input_Unshared { Semantic = Grendgine_Collada_Input_Semantic.INV_BIND_MATRIX, source = "#Controller-bind_poses" }
                }
            };

            // VERTEX WEIGHTS
            var vcounts = new StringBuilder();
            foreach (var _ in boneMaps) vcounts.Append("4 ");
            var vertexs = new StringBuilder();
            var idx = 0;
            // Need to map the exterior vertices (geometry) to the int vertices. Or use the Bone Map datastream if it exists (check HasBoneMapDatastream).
            if (ext2IntMaps == null)
                foreach (var boneMap in boneMaps)
                {
                    vertexs.Append($"{boneMap.BoneIndex[0]} {idx + 0}");
                    vertexs.Append($"{boneMap.BoneIndex[1]} {idx + 1}");
                    vertexs.Append($"{boneMap.BoneIndex[2]} {idx + 2}");
                    vertexs.Append($"{boneMap.BoneIndex[3]} {idx + 3}");
                    idx += 4;
                }
            else
                foreach (var ext2IntMap in ext2IntMaps)
                {
                    vertexs.Append($"{intVertexs[ext2IntMap].BoneIDs[0]} {idx + 0}");
                    vertexs.Append($"{intVertexs[ext2IntMap].BoneIDs[1]} {idx + 1}");
                    vertexs.Append($"{intVertexs[ext2IntMap].BoneIDs[2]} {idx + 2}");
                    vertexs.Append($"{intVertexs[ext2IntMap].BoneIDs[3]} {idx + 3}");
                    idx += 4;
                }
            var vertexWeights = new Grendgine_Collada_Vertex_Weights
            {
                Count = boneMaps.Length,
                VCount = new Grendgine_Collada_Int_Array_String { Value_As_String = vcounts.ToString().TrimEnd() },
                V = new Grendgine_Collada_Int_Array_String { Value_As_String = vertexs.ToString().TrimEnd() },
                Input = new[] {
                    new Grendgine_Collada_Input_Shared { Semantic = Grendgine_Collada_Input_Semantic.JOINT, source = "#Controller-joints", Offset = 0 },
                    new Grendgine_Collada_Input_Shared { Semantic = Grendgine_Collada_Input_Semantic.WEIGHT, source = "#Controller-weights", Offset = 1 }
                }
            };

            // CONTROLLER
            var controller = new Grendgine_Collada_Controller
            {
                ID = "Controller",
                // create the extra element for the FCOLLADA profile
                Extra = new[] { new Grendgine_Collada_Extra {
                    Technique = new[] { new Grendgine_Collada_Technique { profile = "FCOLLADA", UserProperties = "SkinController" }}
                }},
                // Create the skin object and assign to the controller
                Skin = new Grendgine_Collada_Skin
                {
                    source = "#" + daeObject.Library_Geometries.Geometry[0].ID,
                    // We will assume the BSM is the identity matrix for now
                    Bind_Shape_Matrix = new Grendgine_Collada_Float_Array_String { Value_As_String = CreateStringFromMatrix44(Matrix4x4.Identity) },
                    // Add the 3 sources for this controller: joints, bind poses, and weights
                    Source = sources.ToArray(),
                    Joints = joints,
                    Vertex_Weights = vertexWeights,
                }
            };

            // Set up the controller library.
            daeObject.Library_Controllers = new Grendgine_Collada_Library_Controllers
            {
                // There can be multiple controllers in the controller library.  But for Cryengine files, there is only one rig.
                // So if a rig exists, make that the controller.  This applies mostly to .chr files, which will have a rig and geometry.
                Controller = new[] { controller }
            };
        }
    }
}