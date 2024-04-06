using System;
using System.IO;

namespace GameX.Crytek.Formats.Core.Chunks
{
    class ChunkIvoSkin_900 : ChunkIvoSkin
    {
        // Node IDs for Ivo models
        // 1: NodeChunk
        // 2: MeshChunk
        // 3: MeshSubsets
        // 4: Indices
        // 5: VertsUVs (contains vertices, UVs and colors)
        // 6: Normals
        // 7: Tangents
        // 8: Bonemap  (assume all #ivo files have armatures)
        // 9: Colors
        bool hasNormalsChunk = false; // If Flags2 of the meshchunk is 5, there is a separate normals chunk

        public override void Read(BinaryReader r)
        {
            var model = _model;
            base.Read(r);
            SkipBytes(r, 4);

            _header.Offset = (uint)r.BaseStream.Position;
            var meshChunk = new ChunkMesh_900
            {
                _model = _model,
                _header = _header,
                ChunkType = ChunkType.Mesh,
                ID = 2,
                MeshSubsetsData = 3
            };
            meshChunk.Read(r);

            model.ChunkMap.Add(meshChunk.ID, meshChunk);
            if (meshChunk.Flags2 == 5) hasNormalsChunk = true;

            SkipBytes(r, 120);  // Unknown data.  All 0x00

            _header.Offset = (uint)r.BaseStream.Position;
            // Create dummy header info here (ChunkType, version, size, offset)
            var subsetsChunk = new ChunkMeshSubsets_900(meshChunk.NumVertSubsets)
            {
                _model = _model,
                _header = _header,
                ChunkType = ChunkType.MeshSubsets,
                ID = 3
            };
            subsetsChunk.Read(r);
            model.ChunkMap.Add(subsetsChunk.ID, subsetsChunk);

            while (r.BaseStream.Position != r.BaseStream.Length)
            {
                var chunkType = (DataStreamType)r.ReadUInt32();
                r.BaseStream.Position = r.BaseStream.Position - 4;
                switch (chunkType)
                {
                    case DataStreamType.IVOINDICES:
                        // Indices datastream
                        _header.Offset = (uint)r.BaseStream.Position;
                        var indicesDatastreamChunk = new ChunkDataStream_900(meshChunk.NumIndices)
                        {
                            _model = _model,
                            _header = _header,
                            DataStreamType = DataStreamType.INDICES,
                            ChunkType = ChunkType.DataStream,
                            ID = 4
                        };
                        indicesDatastreamChunk.Read(r);
                        model.ChunkMap.Add(indicesDatastreamChunk.ID, indicesDatastreamChunk);
                        break;
                    case DataStreamType.IVOVERTSUVS:
                        _header.Offset = (uint)r.BaseStream.Position;
                        var vertsUvsDatastreamChunk = new ChunkDataStream_900(meshChunk.NumVertices)
                        {
                            _model = _model,
                            _header = _header,
                            DataStreamType = DataStreamType.VERTSUVS,
                            ChunkType = ChunkType.DataStream,
                            ID = 5
                        };
                        vertsUvsDatastreamChunk.Read(r);
                        model.ChunkMap.Add(vertsUvsDatastreamChunk.ID, vertsUvsDatastreamChunk);

                        // Create colors chunk
                        var c = new ChunkDataStream_900(meshChunk.NumVertices)
                        {
                            _model = _model,
                            _header = _header,
                            ChunkType = ChunkType.DataStream,
                            BytesPerElement = 4,
                            DataStreamType = DataStreamType.COLORS,
                            Colors = vertsUvsDatastreamChunk.Colors,
                            ID = 9
                        };
                        model.ChunkMap.Add(c.ID, c);
                        break;
                    case DataStreamType.IVONORMALS:
                    case DataStreamType.IVONORMALS2:
                    case DataStreamType.IVONORMALS3:
                        _header.Offset = (uint)r.BaseStream.Position;
                        var normals = new ChunkDataStream_900(meshChunk.NumVertices)
                        {
                            _model = _model,
                            _header = _header,
                            DataStreamType = DataStreamType.NORMALS,
                            ChunkType = ChunkType.DataStream,
                            ID = 6
                        };
                        normals.Read(r);
                        model.ChunkMap.Add(normals.ID, normals);
                        break;
                    case DataStreamType.IVOTANGENTS:
                        _header.Offset = (uint)r.BaseStream.Position;
                        var tangents = new ChunkDataStream_900(meshChunk.NumVertices)
                        {
                            _model = _model,
                            _header = _header,
                            DataStreamType = DataStreamType.TANGENTS,
                            ChunkType = ChunkType.DataStream,
                            ID = 7
                        };
                        tangents.Read(r);
                        model.ChunkMap.Add(tangents.ID, tangents);
                        if (!hasNormalsChunk)
                        {
                            // Create a normals chunk from Tangents data
                            var norms = new ChunkDataStream_900(meshChunk.NumVertices)
                            {
                                _model = _model,
                                _header = _header,
                                ChunkType = ChunkType.DataStream,
                                BytesPerElement = 4,
                                DataStreamType = DataStreamType.NORMALS,
                                Normals = tangents.Normals,
                                ID = 6
                            };
                            model.ChunkMap.Add(norms.ID, norms);
                        }
                        break;
                    case DataStreamType.IVOBONEMAP:
                        _header.Offset = (uint)r.BaseStream.Position;
                        var bonemap = new ChunkDataStream_900(meshChunk.NumVertices)
                        {
                            _model = _model,
                            _header = _header,
                            DataStreamType = DataStreamType.BONEMAP,
                            ChunkType = ChunkType.DataStream,
                            ID = 8
                        };
                        bonemap.Read(r);
                        model.ChunkMap.Add(bonemap.ID, bonemap);
                        break;
                    default: r.BaseStream.Position = r.BaseStream.Position + 4; break;
                }
            }
        }
    }
}
