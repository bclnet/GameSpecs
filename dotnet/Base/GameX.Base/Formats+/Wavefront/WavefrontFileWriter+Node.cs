//using GameX.Formats.Generic;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using static OpenStack.Debug;

//namespace GameX.Formats.Wavefront
//{
//    partial class WavefrontObjectWriter
//    {
//        void WriteObjNode(StreamWriter w, IGenericNode node) // Pass a node to this to have it write to the Stream
//        {
//            // TODO: Transform Root Nodes here?
//            file.WriteLine("o {0}", node.Name);

//            // Get the Transform here. It's the node chunk Transform.m(41/42/42) divided by 100, added to the parent transform.
//            // The transform of a child has to add the transforms of ALL the parents.
//            if (!(node.Object is IChunkMesh tmpMesh)) return;

//            if (tmpMesh.MeshSubsets == 0) // This is probably wrong. These may be parents with no geometry, but still have an offset
//            {
//                Log($"*******Found a Mesh chunk with no Submesh ID (ID: {tmpMesh.Id:X}, Name: {node.Name}).  Skipping...");
//                // tmpMesh.WriteChunk();
//                // Log($"Node Chunk: {chunkNode.Name}");
//                // transform = cgfData.GetTransform(chunkNode, transform);
//                return;
//            }
//            if (tmpMesh.VerticesData == 0 && tmpMesh.VertsUVsData == 0) // This is probably wrong. These may be parents with no geometry, but still have an offset
//            {
//                Log($"*******Found a Mesh chunk with no Vertex info (ID: {tmpMesh.Id:X}, Name: {node.Name}).  Skipping...");
//                //tmpMesh.WriteChunk();
//                //Log($"Node Chunk: {chunkNode.Name}");
//                //transform = cgfData.GetTransform(chunkNode, transform);
//                return;
//            }

//            // Going to assume that there is only one VerticesData datastream for now.  Need to watch for this.   
//            // Some 801 types have vertices and not VertsUVs.
//            var chunkMap = node._model.ChunkMap;
//            var tmpMtlName = chunkMap.GetValue(node.MatID, null) as ChunkMtlName;
//            var tmpMeshSubsets = chunkMap.GetValue(tmpMesh.MeshSubsets, null) as ChunkMeshSubsets; // Listed as Object ID for the Node
//            var tmpIndices = chunkMap.GetValue(tmpMesh.IndicesData, null) as ChunkDataStream;
//            var tmpVertices = chunkMap.GetValue(tmpMesh.VerticesData, null) as ChunkDataStream;
//            var tmpNormals = chunkMap.GetValue(tmpMesh.NormalsData, null) as ChunkDataStream;
//            var tmpUVs = chunkMap.GetValue(tmpMesh.UVsData, null) as ChunkDataStream;
//            var tmpVertsUVs = chunkMap.GetValue(tmpMesh.VertsUVsData, null) as ChunkDataStream;

//            // We only use 3 things in obj files:  vertices, normals and UVs.  No need to process the Tangents.

//            var numChildren = node.__NumChildren;           // use in a for loop to print the mesh for each child

//            var tempVertexPosition = CurrentVertexPosition;
//            var tempIndicesPosition = CurrentIndicesPosition;

//            foreach (var meshSubset in tmpMeshSubsets.MeshSubsets)
//            {
//                // Write vertices data for each MeshSubSet (v)
//                w.WriteLine("g {0}", GroupOverride ?? node.Name);

//                // WRITE VERTICES OUT (V, VT)
//                if (tmpMesh.VerticesData == 0)
//                {
//                    // Probably using VertsUVs (3.7+).  Write those vertices out. Do UVs at same time.
//                    for (var j = meshSubset.FirstVertex; j < meshSubset.NumVertices + meshSubset.FirstVertex; j++)
//                    {
//                        // Let's try this using this node chunk's rotation matrix, and the transform is the sum of all the transforms.
//                        // Get the transform.
//                        // Dymek's code.  Scales the object by the bounding box.
//                        var multiplerX = Math.Abs(tmpMesh.MinBound.x - tmpMesh.MaxBound.x) / 2f;
//                        var multiplerY = Math.Abs(tmpMesh.MinBound.y - tmpMesh.MaxBound.y) / 2f;
//                        var multiplerZ = Math.Abs(tmpMesh.MinBound.z - tmpMesh.MaxBound.z) / 2f;
//                        if (multiplerX < 1) multiplerX = 1;
//                        if (multiplerY < 1) multiplerY = 1;
//                        if (multiplerZ < 1) multiplerZ = 1;
//                        tmpVertsUVs.Vertices[j].x = tmpVertsUVs.Vertices[j].x * multiplerX + (tmpMesh.MaxBound.x + tmpMesh.MinBound.x) / 2f;
//                        tmpVertsUVs.Vertices[j].y = tmpVertsUVs.Vertices[j].y * multiplerY + (tmpMesh.MaxBound.y + tmpMesh.MinBound.y) / 2f;
//                        tmpVertsUVs.Vertices[j].z = tmpVertsUVs.Vertices[j].z * multiplerZ + (tmpMesh.MaxBound.z + tmpMesh.MinBound.z) / 2f;
//                        var vertex = node.GetTransform(tmpVertsUVs.Vertices[j]);
//                        w.WriteLine("v {0:F7} {1:F7} {2:F7}", safe(vertex.x), safe(vertex.y), safe(vertex.z));
//                    }
//                    w.WriteLine();
//                    for (var j = meshSubset.FirstVertex; j < meshSubset.NumVertices + meshSubset.FirstVertex; j++) w.WriteLine("vt {0:F7} {1:F7} 0", safe(tmpVertsUVs.UVs[j].U), safe(1 - tmpVertsUVs.UVs[j].V));
//                }
//                else
//                {
//                    for (var j = meshSubset.FirstVertex; j < meshSubset.NumVertices + meshSubset.FirstVertex; j++)
//                        if (tmpVertices != null)
//                        {
//                            // Rotate/translate the vertex
//                            var vertex = node.GetTransform(tmpVertices.Vertices[j]);
//                            w.WriteLine("v {0:F7} {1:F7} {2:F7}", safe(vertex.x), safe(vertex.y), safe(vertex.z));
//                        }
//                        else Log($"Error rendering vertices for {node.Name:X}");
//                    w.WriteLine();
//                    for (var j = meshSubset.FirstVertex; j < meshSubset.NumVertices + meshSubset.FirstVertex; j++) w.WriteLine("vt {0:F7} {1:F7} 0", safe(tmpUVs.UVs[j].U), safe(1 - tmpUVs.UVs[j].V));
//                }

//                w.WriteLine();

//                // WRITE NORMALS BLOCK (VN)
//                if (tmpMesh.NormalsData != 0) for (var j = meshSubset.FirstVertex; j < meshSubset.NumVertices + meshSubset.FirstVertex; j++) w.WriteLine("vn {0:F7} {1:F7} {2:F7}", tmpNormals.Normals[j].x, tmpNormals.Normals[j].y, tmpNormals.Normals[j].z);

//                // WRITE GROUP (G)
//                // w.WriteLine("g {0}", this.GroupOverride ?? chunkNode.Name);

//                if (Smooth) w.WriteLine("s {0}", FaceIndex++);

//                // WRITE MATERIAL BLOCK (USEMTL)
//                if (File.Materials.Length > meshSubset.MatID) w.WriteLine("usemtl {0}", File.Materials[meshSubset.MatID].Name);
//                else
//                {
//                    if (File.Materials.Length > 0) Log($"Missing Material {meshSubset.MatID}");
//                    // The material file doesn't have any elements with the Name of the material.  Use the object name.
//                    w.WriteLine("usemtl {0}_{1}", File.Root.Name, meshSubset.MatID);
//                }

//                // Now write out the faces info based on the MtlName
//                for (var j = meshSubset.FirstIndex; j < meshSubset.NumIndices + meshSubset.FirstIndex; j++)
//                {
//                    w.WriteLine("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}",    // Vertices, UVs, Normals
//                        tmpIndices.Indices[j] + 1 + CurrentVertexPosition,
//                        tmpIndices.Indices[j + 1] + 1 + CurrentVertexPosition,
//                        tmpIndices.Indices[j + 2] + 1 + CurrentVertexPosition);
//                    j += 2;
//                }

//                tempVertexPosition += meshSubset.NumVertices;  // add the number of vertices so future objects can start at the right place
//                tempIndicesPosition += meshSubset.NumIndices;  // Not really used...
//            }

//            // Extend the current vertex, uv and normal positions by the length of those arrays.
//            CurrentVertexPosition = tempVertexPosition;
//            CurrentIndicesPosition = tempIndicesPosition;
//        }
//    }
//}