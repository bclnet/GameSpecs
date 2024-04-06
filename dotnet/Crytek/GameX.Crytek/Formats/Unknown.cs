using GameX.Crytek.Formats.Core.Chunks;
using GameX.Formats.Unknown;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static OpenStack.Debug;

namespace GameX.Crytek.Formats
{
    partial class CryFile : IUnknownFileModel
    {
        IEnumerable<string> IUnknownFileModel.RootNodes => NodeMap.Values.Where(x => x.ParentNode == null).Select(x => x.Name);
        IEnumerable<IUnknownModel> IUnknownFileModel.Models => throw new NotImplementedException();
        IEnumerable<UnknownMesh> IUnknownFileModel.Meshes
        {
            get
            {
                foreach (var node in NodeMap.Values)
                {
                    if (node.ObjectChunk == null) { Log($"Skipped node with missing Object {node.Name}"); continue; }
                    if (node.ObjectChunk.ChunkType == ChunkType.Helper) { continue; }
                    if (node.ObjectChunk.ChunkType != ChunkType.Mesh) { Log($"Skipped a {node.ObjectChunk.ChunkType} chunk"); continue; }
                    if (!(node.ObjectChunk is ChunkMesh chunk)) { Log($"Invalid ChunkMesh in {node.Name}"); continue; }

                    // Get the Transform here. It's the node chunk Transform.m(41/42/42) divided by 100, added to the parent transform. The transform of a child has to add the transforms of ALL the parents.
                    if (node.ParentNode != null && node.ParentNode.ChunkType != ChunkType.Node) Log($"Rendering {node.Name} to parent {node.ParentNode.Name}");

                    // This is probably wrong.  These may be parents with no geometry, but still have an offset
                    if (chunk.MeshSubsetsData == 0) { Log($"*******Found a Mesh chunk with no Submesh ID (ID: {chunk.ID:X}, Name: {node.Name}).  Skipping..."); continue; }
                    // This is probably wrong.  These may be parents with no geometry, but still have an offset
                    if (chunk.VerticesData == 0 && chunk.VertsUVsData == 0) { Log($"*******Found a Mesh chunk with no Vertex info (ID: {chunk.ID:X}, Name: {node.Name}).  Skipping..."); continue; }

                    // Going to assume that there is only one VerticesData datastream for now. Need to watch for this. Some 801 types have vertices and not VertsUVs.
                    var chunkMap = node._model.ChunkMap;
                    var mtlName = chunkMap.Get(node.MatID, null) as ChunkMtlName;
                    var meshSubsets = chunkMap.Get(chunk.MeshSubsetsData, null) as ChunkMeshSubsets; // Listed as Object ID for the Node
                    var indexs = chunkMap.Get(chunk.IndicesData, null) as ChunkDataStream;
                    var vertexes = chunkMap.Get(chunk.VerticesData, null) as ChunkDataStream;
                    var normals = chunkMap.Get(chunk.NormalsData, null) as ChunkDataStream;
                    var uvs = chunkMap.Get(chunk.UVsData, null) as ChunkDataStream;
                    var vertsUVs = chunkMap.Get(chunk.VertsUVsData, null) as ChunkDataStream;

                    var mesh = new UnknownMesh
                    {
                        Name = node.Name,
                        MaxBound = chunk.MaxBound,
                        MinBound = chunk.MinBound,
                        Subsets = meshSubsets.MeshSubsets.Select(s => new UnknownMesh.Subset
                        {
                            Vertexs = new Range((int)s.FirstVertex, (int)(s.NumVertices + s.FirstVertex)),
                            Indexs = new Range((int)s.FirstVertex, (int)(s.NumIndices + s.FirstVertex)),
                            MatId = (int)s.MatID,
                        }).ToArray(),
                        Vertexs = chunk.VerticesData == 0 ? vertsUVs.Vertices : vertexes?.Vertices,
                        UVs = chunk.VerticesData == 0 ? vertsUVs.UVs : uvs.UVs,
                        Normals = chunk.NormalsData != 0 ? normals.Normals : null,
                        Indexs = indexs.Indices.Cast<int>().ToArray(),
                    };

                    // Let's try this using this node chunk's rotation matrix, and the transform is the sum of all the transforms.
                    // Scales the object by the bounding box.
                    if (chunk.VerticesData == 0)
                    {
                        var scaleX = Math.Abs(chunk.MinBound.X - chunk.MaxBound.X) / 2f; if (scaleX < 1f) scaleX = 1f;
                        var scaleY = Math.Abs(chunk.MinBound.Y - chunk.MaxBound.Y) / 2f; if (scaleY < 1f) scaleY = 1f;
                        var scaleZ = Math.Abs(chunk.MinBound.Z - chunk.MaxBound.Z) / 2f; if (scaleZ < 1f) scaleZ = 1f;
                        var offsetX = (chunk.MinBound.X + chunk.MaxBound.X) / 2f;
                        var offsetY = (chunk.MinBound.Y + chunk.MaxBound.Y) / 2f;
                        var offsetZ = (chunk.MinBound.Z + chunk.MaxBound.Z) / 2f;
                        mesh.Effects = UnknownMesh.Effect.ScaleOffset;
                        mesh.ScaleOffset3 = (new Vector3(scaleX, scaleY, scaleZ), new Vector3(offsetX, offsetY, offsetZ));
                        mesh.ScaleOffset4 = (new Vector4(scaleX, scaleY, scaleZ, 1f), new Vector4(offsetX, offsetY, offsetZ, 0));
                    }

                    yield return mesh;
                }
            }
        }
        IEnumerable<IUnknownMaterial> IUnknownFileModel.Materials => Materials;
        IEnumerable<IUnknownProxy> IUnknownFileModel.Proxies => Chunks.Where(a => a.ChunkType == ChunkType.CompiledPhysicalProxies).Select(x => (IUnknownProxy)x);
        IUnknownSkin IUnknownFileModel.SkinningInfo => throw new NotImplementedException();
        string IUnknownFileObject.Name => Name;
        string IUnknownFileObject.Path => InputFile;
        IEnumerable<IUnknownFileObject.Source> IUnknownFileObject.Sources => Chunks.Where(a => a.ChunkType == ChunkType.SourceInfo).Select(x =>
        {
            var s = (ChunkSourceInfo)x;
            return new IUnknownFileObject.Source { Author = s.Author, SourceFile = s.SourceFile };
        });
    }
}
