//using grendgine_collada;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using static OpenStack.Debug;

//namespace GameX.Formats.Collada
//{
//    /// <summary>
//    /// Adds the Library_Controllers element to the Collada document.
//    /// </summary>
//    partial class ColladaObjectWriter
//    {
//        /// <summary>
//        /// Adds the Library_Geometries element to the Collada document. These won't be instantiated except through the visual scene or controllers.
//        /// </summary>
//        void SetLibraryGeometries()
//        {
//            // Make a list for all the geometries objects we will need. Will convert to array at end.  Define the array here as well
//            // Unfortunately we have to define a Geometry for EACH meshsubset in the meshsubsets, since the mesh can contain multiple materials
//            var geometrys = new List<Grendgine_Collada_Geometry>();

//            // For each of the nodes, we need to write the geometry.
//            // Use a foreach statement to get all the node chunks.  This will get us the meshes, which will contain the vertex, UV and normal info.
//            foreach (ChunkNode nodeChunk in CryFile.Chunks.Where(a => a.ChunkType == ChunkTypeEnum.Node))
//            {
//                // Create a geometry object.  Use the chunk ID for the geometry ID
//                // Will have to be careful with this, since with .cga/.cgam pairs will need to match by Name.
//                // Now make the mesh object.  This will have 3 sources, 1 vertices, and 1 or more polylist (with material ID)
//                // If the Object ID of Node chunk points to a Helper or a Controller though, place an empty.
//                // Will have to figure out transforms here too.
//                // need to make a list of the sources and triangles to add to tmpGeo.Mesh
//                var sourceList = new List<Grendgine_Collada_Source>();
//                //var triList = new List<Grendgine_Collada_Triangles>();  // Use PolyList over trilist
//                var polylistList = new List<Grendgine_Collada_Polylist>();
//                ChunkDataStream tmpNormals = null;
//                ChunkDataStream tmpUVs = null;
//                ChunkDataStream tmpVertices = null;
//                ChunkDataStream tmpVertsUVs = null;
//                ChunkDataStream tmpIndices = null;
//                ChunkDataStream tmpColors = null;
//                ChunkDataStream tmpTangents = null;
//                //var geometryInfo = nodeChunk.ObjectChunk.

//                // Don't render shields
//                if (SkipShieldNodes && nodeChunk.Name.StartsWith("$shield"))
//                {
//                    Log($"Skipped shields node {nodeChunk.Name}");
//                    continue;
//                }
//                // Don't render proxies
//                if (SkipStreamNodes && nodeChunk.Name.StartsWith("stream"))
//                {
//                    Log($"Skipped stream node {nodeChunk.Name}");
//                    continue;
//                }
//                if (nodeChunk.ObjectChunk == null)
//                {
//                    Log($"Skipped node with missing Object {nodeChunk.Name}");
//                    continue;
//                }
//                if (nodeChunk._model.ChunkMap[nodeChunk.ObjectNodeID].ChunkType == ChunkTypeEnum.Mesh)
//                {
//                    // Get the mesh chunk and submesh chunk for this node.
//                    var tmpMeshChunk = (ChunkMesh)nodeChunk._model.ChunkMap[nodeChunk.ObjectNodeID];
//                    // Check to see if the Mesh points to a PhysicsData mesh.  Don't want to write these.
//                    if (tmpMeshChunk.MeshPhysicsData != 0)
//                    {
//                        // TODO:  Implement this chunk
//                    }
//                    // For the SC files, you can have Mesh chunks with no Mesh Subset.  Need to skip these.  They are in the .cga file and contain no geometry.  Just stub info.
//                    if (tmpMeshChunk.MeshSubsets != 0)
//                    {
//                        //Console.WriteLine($"tmpMeshChunk ID is {nodeChunk.ObjectNodeID:X}");
//                        //tmpMeshChunk.WriteChunk();
//                        //Console.WriteLine($"tmpmeshsubset ID is {tmpMeshChunk.MeshSubsets:X}");
//                        var tmpMeshSubsets = (ChunkMeshSubsets)nodeChunk._model.ChunkMap[tmpMeshChunk.MeshSubsets];  // Listed as Object ID for the Node
//                        if (tmpMeshChunk.MeshSubsets != 0) tmpMeshSubsets = (ChunkMeshSubsets)nodeChunk._model.ChunkMap[tmpMeshChunk.MeshSubsets];  // Listed as Object ID for the Node
//                        // Get pointers to the vertices data
//                        if (tmpMeshChunk.VerticesData != 0) tmpVertices = (ChunkDataStream)nodeChunk._model.ChunkMap[tmpMeshChunk.VerticesData];
//                        if (tmpMeshChunk.NormalsData != 0) tmpNormals = (ChunkDataStream)nodeChunk._model.ChunkMap[tmpMeshChunk.NormalsData];
//                        if (tmpMeshChunk.UVsData != 0) tmpUVs = (ChunkDataStream)nodeChunk._model.ChunkMap[tmpMeshChunk.UVsData];
//                        // Star Citizen file.  That means VerticesData and UVsData will probably be empty.  Need to handle both cases.
//                        if (tmpMeshChunk.VertsUVsData != 0) tmpVertsUVs = (ChunkDataStream)nodeChunk._model.ChunkMap[tmpMeshChunk.VertsUVsData];
//                        if (tmpMeshChunk.IndicesData != 0) tmpIndices = (ChunkDataStream)nodeChunk._model.ChunkMap[tmpMeshChunk.IndicesData];
//                        // Ignore Tangent and Color data for now.
//                        if (tmpMeshChunk.ColorsData != 0) tmpColors = (ChunkDataStream)nodeChunk._model.ChunkMap[tmpMeshChunk.ColorsData];
//                        if (tmpMeshChunk.TangentsData != 0) tmpTangents = (ChunkDataStream)nodeChunk._model.ChunkMap[tmpMeshChunk.TangentsData];
//                        if (tmpVertices == null && tmpVertsUVs == null)
//                            // There is no vertex data for this node.  Skip.
//                            continue;

//                        // tmpGeo is a Geometry object for each meshsubset.  Name will be "Nodechunk name_matID".  Hopefully there is only one matID used per submesh
//                        var tmpGeo = new Grendgine_Collada_Geometry { Name = nodeChunk.Name, ID = nodeChunk.Name + "-mesh" };
//                        var tmpMesh = new Grendgine_Collada_Mesh();
//                        tmpGeo.Mesh = tmpMesh;

//                        var source = new Grendgine_Collada_Source[3]; // 3 possible source types.
//                        // need a collada_source for position, normal, UV, tangents and color, what the source is (verts), and the tri index
//                        var posSource = source[0] = new Grendgine_Collada_Source { ID = nodeChunk.Name + "-mesh-pos", Name = nodeChunk.Name + "-pos" };
//                        var normSource = source[1] = new Grendgine_Collada_Source { ID = nodeChunk.Name + "-mesh-norm", Name = nodeChunk.Name + "-norm" };
//                        var uvSource = source[2] = new Grendgine_Collada_Source { Name = nodeChunk.Name + "-UV", ID = nodeChunk.Name + "-mesh-UV" };
//                        //var tangentSource = source[3] = new Grendgine_Collada_Source { Name = nodeChunk.Name + "-tangent", ID = nodeChunk.Name + "-mesh-tangent" };
//                        //
//                        var posInput = new Grendgine_Collada_Input_Unshared { Semantic = Grendgine_Collada_Input_Semantic.POSITION, source = "#" + posSource.ID };
//                        var normInput = new Grendgine_Collada_Input_Unshared { Semantic = Grendgine_Collada_Input_Semantic.NORMAL, source = "#" + normSource.ID };
//                        var uvInput = new Grendgine_Collada_Input_Unshared { Semantic = Grendgine_Collada_Input_Semantic.TEXCOORD, source = "#" + uvSource.ID };    // might need to replace TEXCOORD with UV
//                        var colorInput = new Grendgine_Collada_Input_Unshared { Semantic = Grendgine_Collada_Input_Semantic.COLOR };
//                        //var tangentInput = new Grendgine_Collada_Input_Unshared { Semantic = Grendgine_Collada_Input_Semantic.TANGENT, source = "#" + tangentSource.ID } ;

//                        // Create vertices node.  For polylist will just have VERTEX.
//                        var vertices = tmpGeo.Mesh.Vertices = new Grendgine_Collada_Vertices { ID = nodeChunk.Name + "-vertices", Input = new[] { posInput, null, null } };

//                        // Create a float_array object to store all the data
//                        Grendgine_Collada_Float_Array floatArrayVerts, floatArrayNormals, floatArrayUVs; //floatArrayColors, floatArrayTangents;
//                        // Strings for vertices
//                        StringBuilder vertString = new StringBuilder(), normString = new StringBuilder(), uvString = new StringBuilder(); //tangentString = new StringBuilder()
//                        if (tmpVertices != null)  // Will be null if it's using VertsUVs.
//                        {
//                            floatArrayVerts = new Grendgine_Collada_Float_Array
//                            {
//                                ID = posSource.ID + "-array",
//                                Digits = 6,
//                                Magnitude = 38,
//                                Count = (int)tmpVertices.NumElements * 3
//                            };
//                            floatArrayUVs = new Grendgine_Collada_Float_Array
//                            {
//                                ID = uvSource.ID + "-array",
//                                Digits = 6,
//                                Magnitude = 38,
//                                Count = (int)tmpUVs.NumElements * 2
//                            };
//                            floatArrayNormals = new Grendgine_Collada_Float_Array
//                            {
//                                ID = normSource.ID + "-array",
//                                Digits = 6,
//                                Magnitude = 38
//                            };
//                            if (tmpNormals != null)
//                                floatArrayNormals.Count = (int)tmpNormals.NumElements * 3;
//                            // Create Vertices and normals string
//                            for (var j = 0U; j < tmpMeshChunk.NumVertices; j++)
//                            {
//                                var vertex = tmpVertices.Vertices[j];
//                                vertString.AppendFormat("{0:F6} {1:F6} {2:F6} ", vertex.x, vertex.y, vertex.z);
//                                var normal = tmpNormals?.Normals[j] ?? new Vector3(0.0f, 0.0f, 0.0f);
//                                normString.AppendFormat("{0:F6} {1:F6} {2:F6} ", safe(normal.x), safe(normal.y), safe(normal.z));
//                            }
//                            // Create UV string
//                            for (var j = 0U; j < tmpUVs.NumElements; j++)
//                                uvString.AppendFormat("{0:F6} {1:F6} ", safe(tmpUVs.UVs[j].U), 1 - safe(tmpUVs.UVs[j].V));
//                        }
//                        // VertsUV structure.  Pull out verts and UVs from tmpVertsUVs.
//                        else
//                        {
//                            floatArrayVerts = new Grendgine_Collada_Float_Array
//                            {
//                                ID = posSource.ID + "-array",
//                                Digits = 6,
//                                Magnitude = 38,
//                                Count = (int)tmpVertsUVs.NumElements * 3
//                            };
//                            floatArrayUVs = new Grendgine_Collada_Float_Array
//                            {
//                                ID = uvSource.ID + "-array",
//                                Digits = 6,
//                                Magnitude = 38,
//                                Count = (int)tmpVertsUVs.NumElements * 2
//                            };
//                            floatArrayNormals = new Grendgine_Collada_Float_Array
//                            {
//                                ID = normSource.ID + "-array",
//                                Digits = 6,
//                                Magnitude = 38,
//                                Count = (int)tmpVertsUVs.NumElements * 3
//                            };
//                            // Create Vertices and normals string
//                            for (var j = 0U; j < tmpMeshChunk.NumVertices; j++)
//                            {
//                                // Rotate/translate the vertex
//                                // Dymek's code to rescale by bounding box.  Only apply to geometry (cga or cgf), and not skin or chr objects.
//                                if (!CryFile.InputFile.EndsWith("skin") && !CryFile.InputFile.EndsWith("chr"))
//                                {
//                                    var multiplerX = Math.Abs(tmpMeshChunk.MinBound.x - tmpMeshChunk.MaxBound.x) / 2f;
//                                    var multiplerY = Math.Abs(tmpMeshChunk.MinBound.y - tmpMeshChunk.MaxBound.y) / 2f;
//                                    var multiplerZ = Math.Abs(tmpMeshChunk.MinBound.z - tmpMeshChunk.MaxBound.z) / 2f;
//                                    if (multiplerX < 1) multiplerX = 1;
//                                    if (multiplerY < 1) multiplerY = 1;
//                                    if (multiplerZ < 1) multiplerZ = 1;
//                                    tmpVertsUVs.Vertices[j].x = tmpVertsUVs.Vertices[j].x * multiplerX + (tmpMeshChunk.MaxBound.x + tmpMeshChunk.MinBound.x) / 2;
//                                    tmpVertsUVs.Vertices[j].y = tmpVertsUVs.Vertices[j].y * multiplerY + (tmpMeshChunk.MaxBound.y + tmpMeshChunk.MinBound.y) / 2;
//                                    tmpVertsUVs.Vertices[j].z = tmpVertsUVs.Vertices[j].z * multiplerZ + (tmpMeshChunk.MaxBound.z + tmpMeshChunk.MinBound.z) / 2;
//                                }

//                                var vertex = tmpVertsUVs.Vertices[j];
//                                vertString.AppendFormat("{0:F6} {1:F6} {2:F6} ", vertex.x, vertex.y, vertex.z);
//                                var normal = new Vector3();
//                                // Normals depend on the data size.  16 byte structures have the normals in the Tangents.  20 byte structures are in the VertsUV.
//                                if (tmpVertsUVs.BytesPerElement == 20)
//                                    normal = tmpVertsUVs.Normals[j];
//                                else
//                                {
//                                    //normal = tmpTangents.Normals[j];
//                                    //normal.x = normal.x / 32767.0;
//                                    //normal.y = normal.y / 32767.0;
//                                    //normal.z = normal.z / 32767.0;                                    
//                                    normal = tmpVertsUVs.Normals[j];
//                                }
//                                normString.AppendFormat("{0:F6} {1:F6} {2:F6} ", safe(normal.x), safe(normal.y), safe(normal.z));
//                            }
//                            // Create UV string
//                            for (var j = 0U; j < tmpVertsUVs.NumElements; j++)
//                                uvString.AppendFormat("{0:F6} {1:F6} ", safe(tmpVertsUVs.UVs[j].U), safe(1 - tmpVertsUVs.UVs[j].V));
//                        }
//                        CleanNumbers(vertString);
//                        CleanNumbers(normString);
//                        CleanNumbers(uvString);

//                        //floatArrayNormals = new Grendgine_Collada_Float_Array
//                        //{
//                        //    ID = tangentSource.ID + "-array",
//                        //    Digits = 6,
//                        //    Magnitude = 38,
//                        //    Count = (int)tmpTangents.NumElements * 2,
//                        //};
//                        //var tangentString = new StringBuilder();
//                        // Create Tangent string
//                        //for (var j = 0U; j < tmpTangents.NumElements; j++)
//                        //    tangentString.AppendFormat("{0:F6} {1:F6} {2:F6} {3:F6} {4:F6} {5:F6} {6:F6} {7:F6} ", 
//                        //        tmpTangents.Tangents[j, 0].w / 32767, tmpTangents.Tangents[j, 0].x / 32767, tmpTangents.Tangents[j, 0].y / 32767, tmpTangents.Tangents[j, 0].z / 32767,
//                        //        tmpTangents.Tangents[j, 1].w / 32767, tmpTangents.Tangents[j, 1].x / 32767, tmpTangents.Tangents[j, 1].y / 32767, tmpTangents.Tangents[j, 1].z / 32767);
//                        //CleanNumbers(tangentString);

//                        #region Create the polylist node.

//                        var polylists = tmpGeo.Mesh.Polylist = new Grendgine_Collada_Polylist[tmpMeshSubsets.NumMeshSubset];
//                        StringBuilder b0 = new StringBuilder(), b1 = new StringBuilder();
//                        for (var j = 0U; j < tmpMeshSubsets.NumMeshSubset; j++) // Need to make a new Polylist entry for each submesh.
//                        {
//                            b0.Length = 0; b1.Length = 0;
//                            // Create the vcount list.  All triangles, so the subset number of indices.
//                            for (var k = tmpMeshSubsets.MeshSubsets[j].FirstIndex; k < (tmpMeshSubsets.MeshSubsets[j].FirstIndex + tmpMeshSubsets.MeshSubsets[j].NumIndices); k++)
//                            {
//                                b0.AppendFormat("3 ");
//                                k += 2;
//                            }
//                            // Create the P node for the Polylist.
//                            for (var k = tmpMeshSubsets.MeshSubsets[j].FirstIndex; k < (tmpMeshSubsets.MeshSubsets[j].FirstIndex + tmpMeshSubsets.MeshSubsets[j].NumIndices); k++)
//                            {
//                                b1.AppendFormat("{0} {0} {0} {1} {1} {1} {2} {2} {2} ", tmpIndices.Indices[k], tmpIndices.Indices[k + 1], tmpIndices.Indices[k + 2]);
//                                k += 2;
//                            }
//                            polylists[j] = new Grendgine_Collada_Polylist
//                            {
//                                Count = (int)tmpMeshSubsets.MeshSubsets[j].NumIndices / 3,
//                                Input = new[] { // Create the 4 inputs.  vertex, normal, texcoord, tangent
//                                    new Grendgine_Collada_Input_Shared { Semantic = Grendgine_Collada_Input_Semantic.VERTEX, Offset = 0, source = "#" + vertices.ID },
//                                    new Grendgine_Collada_Input_Shared { Semantic = Grendgine_Collada_Input_Semantic.NORMAL, Offset = 1, source = "#" + normSource.ID },
//                                    new Grendgine_Collada_Input_Shared { Semantic = Grendgine_Collada_Input_Semantic.TEXCOORD, Offset = 2, source = "#" + uvSource.ID }
//                                    //new Grendgine_Collada_Input_Shared { Semantic = Grendgine_Collada_Input_Semantic.TANGENT, Offset = 3, source = "#" + tangentSource.ID }
//                                },
//                                VCount = new Grendgine_Collada_Int_Array_String { Value_As_String = b0.ToString().TrimEnd() },
//                                P = new Grendgine_Collada_Int_Array_String { Value_As_String = b1.ToString().TrimEnd() },
//                                Material = CryFile.Materials.Count() != 0 ? CryFile.Materials[tmpMeshSubsets.MeshSubsets[j].MatID].Name + "-material" : null
//                            };
//                        }

//                        #endregion

//                        #region Create the source float_array nodes.  Vertex, normal, UV.  May need color as well.

//                        floatArrayVerts.Value_As_String = vertString.ToString().TrimEnd();
//                        floatArrayNormals.Value_As_String = normString.ToString().TrimEnd();
//                        floatArrayUVs.Value_As_String = uvString.ToString().TrimEnd();
//                        //floatArrayColors.Value_As_String = colorString.ToString();
//                        //floatArrayTangents.Value_As_String = tangentString.ToString().TrimEnd();
//                        //
//                        source[0].Float_Array = floatArrayVerts;
//                        source[1].Float_Array = floatArrayNormals;
//                        source[2].Float_Array = floatArrayUVs;
//                        //source[3].Float_Array = floatArrayColors;
//                        //source[3].Float_Array = floatArrayTangents;
//                        tmpGeo.Mesh.Source = source;

//                        // create the technique_common for each of these
//                        posSource.Technique_Common = new Grendgine_Collada_Technique_Common_Source
//                        {
//                            Accessor = new Grendgine_Collada_Accessor
//                            {
//                                Source = "#" + floatArrayVerts.ID,
//                                Stride = 3,
//                                Count = (uint)tmpMeshChunk.NumVertices,
//                                Param = new[] {
//                                    new Grendgine_Collada_Param { Name = "X", Type = "float" },
//                                    new Grendgine_Collada_Param { Name = "Y", Type = "float" },
//                                    new Grendgine_Collada_Param { Name = "Z", Type = "float" }
//                                }
//                            }
//                        };
//                        normSource.Technique_Common = new Grendgine_Collada_Technique_Common_Source
//                        {
//                            Accessor = new Grendgine_Collada_Accessor
//                            {
//                                Source = "#" + floatArrayNormals.ID,
//                                Stride = 3,
//                                Count = (uint)tmpMeshChunk.NumVertices,
//                                Param = new[] {
//                                    new Grendgine_Collada_Param { Name = "X", Type = "float" },
//                                    new Grendgine_Collada_Param { Name = "Y", Type = "float" },
//                                    new Grendgine_Collada_Param { Name = "Z", Type = "float" }
//                                }
//                            }
//                        };
//                        uvSource.Technique_Common = new Grendgine_Collada_Technique_Common_Source
//                        {
//                            Accessor = new Grendgine_Collada_Accessor
//                            {
//                                Source = "#" + floatArrayUVs.ID,
//                                Stride = 2,
//                                Count = tmpVertices != null ? tmpUVs.NumElements : tmpVertsUVs.NumElements,
//                                Param = new[] {
//                                    new Grendgine_Collada_Param { Name = "S", Type = "float" },
//                                    new Grendgine_Collada_Param { Name = "T", Type = "float" }
//                                }
//                            }
//                        };
//                        //tangentSource.Technique_Common = new Grendgine_Collada_Technique_Common_Source
//                        //{
//                        //    Accessor = new Grendgine_Collada_Accessor
//                        //    {
//                        //        Source = "#" + floatArrayTangents.ID,
//                        //        Stride = 8,
//                        //        Count = tmpTangents.NumElements
//                        //    }
//                        //};
//                        //if (tmpColors != null)
//                        //    colorSource.Technique_Common = new Grendgine_Collada_Technique_Common_Source
//                        //    {
//                        //        Accessor = new Grendgine_Collada_Accessor
//                        //        {
//                        //            Source = "#" + floatArrayColors.ID,
//                        //            Stride = 3,
//                        //            Count = tmpColors.NumElements,
//                        //            Param = new[] {
//                        //              new Grendgine_Collada_Param { Name = "R", Type = "float"},
//                        //              new Grendgine_Collada_Param { Name = "G", Type = "float"},
//                        //              new Grendgine_Collada_Param { Name = "B", Type = "float"}},
//                        //        }
//                        //    };
//                        geometrys.Add(tmpGeo);

//                        #endregion
//                    }
//                }
//                // There is no geometry for a helper or controller node.  Can skip the rest.
//            }

//            daeObject.Library_Geometries = new Grendgine_Collada_Library_Geometries
//            {
//                Geometry = geometrys.ToArray()
//            };
//        }
//    }
//}