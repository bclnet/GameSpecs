namespace GameX.Formats.Collada
{
    partial class ColladaFileWriter
    {

#if false
        Grendgine_Collada_Node CreateNode(ChunkNode nodeChunk)
        {
            // This will be used recursively to create a node object and return it to WriteLibrary_VisualScenes
            Grendgine_Collada_Node tmpNode;
            // Check to see if there is a second model file, and if the mesh chunk is actually there.
            if (CryFile.Models.Count > 1)
            {
                // Star Citizen pair.  Get the Node and Mesh chunks from the geometry file, unless it's a Stream node.
                var nodeName = nodeChunk.Name;
                var nodeID = nodeChunk.ID;
                // make sure there is a geometry node in the geometry file
                if (CryFile.Models[1].NodeMap.ContainsKey(nodeID))
                {
                    var geometryNode = CryFile.Models[1].NodeMap.Values.Where(a => a.Name == nodeChunk.Name).First();
                    var geometryMesh = (ChunkMesh)CryFile.Models[1].ChunkMap[geometryNode.ObjectNodeID];
                    tmpNode = CreateGeometryNode(geometryNode, geometryMesh);
                }
                else tmpNode = CreateSimpleNode(nodeChunk);
            }
            else
            {
                // Regular Cryengine file.
                if (nodeChunk._model.ChunkMap[nodeChunk.ObjectNodeID].ChunkType == ChunkTypeEnum.Mesh)
                {
                    var tmpMeshChunk = (ChunkMesh)nodeChunk._model.ChunkMap[nodeChunk.ObjectNodeID];
                    // Can have a node with a mesh and meshsubset, but no vertices.  Write as simple node.
                    tmpNode = tmpMeshChunk.MeshSubsets == 0 || tmpMeshChunk.NumVertices == 0
                        ? CreateSimpleNode(nodeChunk)
                        : nodeChunk._model.ChunkMap[tmpMeshChunk.MeshSubsets].ID != 0
                            ? CreateGeometryNode(nodeChunk, (ChunkMesh)nodeChunk._model.ChunkMap[nodeChunk.ObjectNodeID])
                            : CreateSimpleNode(nodeChunk);
                }
                else tmpNode = CreateSimpleNode(nodeChunk);
            }
            // Add childnodes
            tmpNode.node = CreateChildNodes(nodeChunk);
            return tmpNode;
        }

        /// <summary>
        /// This will be used to make the Collada node element for Node chunks that point to Helper Chunks and MeshPhysics
        /// </summary>
        /// <param name="nodeChunk">The node chunk for this Collada Node.</param>
        /// <returns>Grendgine_Collada_Node for the node chunk</returns>
        Grendgine_Collada_Node CreateSimpleNode(ChunkNode nodeChunk)
        {
            var matrixString = new StringBuilder();
            CalculateTransform(nodeChunk);
            matrixString.AppendFormat("{0:F6} {1:F6} {2:F6} {3:F6} {4:F6} {5:F6} {6:F6} {7:F6} {8:F6} {9:F6} {10:F6} {11:F6} {12:F6} {13:F6} {14:F6} {15:F6}",
                nodeChunk.LocalTransform.m00, nodeChunk.LocalTransform.m01, nodeChunk.LocalTransform.m02, nodeChunk.LocalTransform.m03,
                nodeChunk.LocalTransform.m10, nodeChunk.LocalTransform.m11, nodeChunk.LocalTransform.m12, nodeChunk.LocalTransform.m13,
                nodeChunk.LocalTransform.m20, nodeChunk.LocalTransform.m21, nodeChunk.LocalTransform.m22, nodeChunk.LocalTransform.m23,
                nodeChunk.LocalTransform.m30, nodeChunk.LocalTransform.m31, nodeChunk.LocalTransform.m32, nodeChunk.LocalTransform.m33);
            // This will be used to make the Collada node element for Node chunks that point to Helper Chunks and MeshPhysics
            return new Grendgine_Collada_Node
            {
                Type = Grendgine_Collada_Node_Type.NODE,
                Name = nodeChunk.Name,
                ID = nodeChunk.Name,
                // we can have multiple matrices, but only need one since there is only one per Node chunk anyway
                Matrix = new[] { new Grendgine_Collada_Matrix { Value_As_String = matrixString.ToString(), sID = "transform" } },
                // Add childnodes
                node = CreateChildNodes(nodeChunk),
            };
        }

        /// <summary>
        /// Used by CreateNode and CreateSimpleNodes to create all the child nodes for the given node.
        /// </summary>
        /// <param name="nodeChunk">Node with children to add.</param>
        /// <returns>A node with all the children added.</returns>
        Grendgine_Collada_Node[] CreateChildNodes(ChunkNode nodeChunk)
        {
            if (nodeChunk.__NumChildren != 0)
            {
                var childNodes = new List<Grendgine_Collada_Node>();
                foreach (var childNodeChunk in nodeChunk.AllChildNodes.ToList())
                    childNodes.Add(CreateNode(childNodeChunk));
                return childNodes.ToArray();
            }
            return null;
        }

        Grendgine_Collada_Node CreateJointNode(CompiledBone bone)
        {
            // Populate the matrix.  This is based on the BONETOWORLD data in this bone.
            var matrixValues = new StringBuilder();
            matrixValues.AppendFormat("{0:F6} {1:F6} {2:F6} {3:F6} {4:F6} {5:F6} {6:F6} {7:F6} {8:F6} {9:F6} {10:F6} {11:F6} 0 0 0 1",
                bone.LocalTransform.m00,
                bone.LocalTransform.m01,
                bone.LocalTransform.m02,
                bone.LocalTransform.m03,
                bone.LocalTransform.m10,
                bone.LocalTransform.m11,
                bone.LocalTransform.m12,
                bone.LocalTransform.m13,
                bone.LocalTransform.m20,
                bone.LocalTransform.m21,
                bone.LocalTransform.m22,
                bone.LocalTransform.m23);
            CleanNumbers(matrixValues);

            // This will be used recursively to create a node object and return it to WriteLibrary_VisualScenes
            // If this is the root bone, set the node id to Armature.  Otherwise set to armature_<bonename>
            var tmpNode = new Grendgine_Collada_Node
            {
                ID = bone.parentID != 0 ? "Armature_" + bone.boneName.Replace(' ', '_') : "Armature",
                Name = bone.boneName.Replace(' ', '_'),
                sID = bone.boneName.Replace(' ', '_'),
                Type = Grendgine_Collada_Node_Type.JOINT,
                // we can have multiple matrices, but only need one since there is only one per Node chunk anyway
                Matrix = new[] { new Grendgine_Collada_Matrix { Value_As_String = matrixValues.ToString() } }
            };

            // Recursively call this for each of the child bones to this bone.
            if (bone.numChildren > 0)
            {
                var idx = 0;
                var childNodes = new Grendgine_Collada_Node[bone.numChildren];
                foreach (var childBone in CryFile.Bones.GetAllChildBones(bone))
                    childNodes[idx++] = CreateJointNode(childBone);
                tmpNode.node = childNodes;
            }
            return tmpNode;
        }

        Grendgine_Collada_Node CreateGeometryNode(ChunkNode nodeChunk, ChunkMesh tmpMeshChunk)
        {
            var matrixString = new StringBuilder();
            // matrixString might have to be an identity matrix, since GetTransform is applying the transform to all the vertices.
            // Use same principle as CreateJointNode.  The Transform matrix (Matrix44) is the world transform matrix.
            CalculateTransform(nodeChunk);
            matrixString.AppendFormat("{0:F6} {1:F6} {2:F6} {3:F6} {4:F6} {5:F6} {6:F6} {7:F6} {8:F6} {9:F6} {10:F6} {11:F6} {12:F6} {13:F6} {14:F6} {15:F6}",
                nodeChunk.LocalTransform.m00, nodeChunk.LocalTransform.m01, nodeChunk.LocalTransform.m02, nodeChunk.LocalTransform.m03,
                nodeChunk.LocalTransform.m10, nodeChunk.LocalTransform.m11, nodeChunk.LocalTransform.m12, nodeChunk.LocalTransform.m13,
                nodeChunk.LocalTransform.m20, nodeChunk.LocalTransform.m21, nodeChunk.LocalTransform.m22, nodeChunk.LocalTransform.m23,
                nodeChunk.LocalTransform.m30, nodeChunk.LocalTransform.m31, nodeChunk.LocalTransform.m32, nodeChunk.LocalTransform.m33);

            // This gets complicated.  We need to make one instance_material for each material used in this node chunk.  The mat IDs used in this
            // node chunk are stored in meshsubsets, so for each subset we need to grab the mat, get the target (id), and make an instance_material for it.
            var instanceMaterials = new List<Grendgine_Collada_Instance_Material_Geometry>();
            var tmpMeshSubsets = (ChunkMeshSubsets)nodeChunk._model.ChunkMap[tmpMeshChunk.MeshSubsets];  // Listed as Object ID for the Node
            for (var i = 0; i < tmpMeshSubsets.NumMeshSubset; i++)
                // For each mesh subset, we want to create an instance material and add it to instanceMaterials list.
                if (CryFile.Materials.Count() > 0)
                    instanceMaterials.Add(new Grendgine_Collada_Instance_Material_Geometry
                    {
                        Target = "#" + CryFile.Materials[tmpMeshSubsets.MeshSubsets[i].MatID].Name + "-material",
                        Symbol = CryFile.Materials[tmpMeshSubsets.MeshSubsets[i].MatID].Name + "-material"
                    });

            // we can have multiple matrices, but only need one since there is only one per Node chunk anyway
            return new Grendgine_Collada_Node
            {
                Type = Grendgine_Collada_Node_Type.NODE,
                Name = nodeChunk.Name,
                ID = nodeChunk.Name,
                Matrix = new[] { new Grendgine_Collada_Matrix { Value_As_String = matrixString.ToString(), sID = "transform" } },
                // Each node will have one instance geometry, although it could be a list.
                Instance_Geometry = new[] { new Grendgine_Collada_Instance_Geometry {
                    Name = nodeChunk.Name,
                    URL = "#" + nodeChunk.Name + "-mesh",  // this is the ID of the geometry.
                    Bind_Material = new[]{new Grendgine_Collada_Bind_Material {
                        Technique_Common = new Grendgine_Collada_Technique_Common_Bind_Material {
                            Instance_Material = instanceMaterials.ToArray()
                        }
                    }}
                }}
            };
        }

        /// <summary>
        /// Creates the Collada Source element for a given datastream).
        /// </summary>
        /// <param name="vertices">The vertices of the source datastream.  This can be position, normals, colors, tangents, etc.</param>
        /// <param name="nodeChunk">The Node chunk of the datastream.  Need this for names, mesh, and submesh info.</param>
        /// <returns>Grendgine_Collada_Source object with the source data.</returns>
        Grendgine_Collada_Source GetMeshSource(ChunkDataStream vertices, ChunkNode nodeChunk) => new Grendgine_Collada_Source
        {
            ID = nodeChunk.Name + "-mesh-pos",
            Name = nodeChunk.Name + "-pos"
        };

#endif

    }
}