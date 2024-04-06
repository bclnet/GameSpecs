using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using static OpenStack.Debug;

namespace GameX.Crytek.Formats.Core.Chunks
{
    // Reversed endian class of x0800 for console games
    public class ChunkDataStream_80000800 : ChunkDataStream
    {
        public override void Read(BinaryReader r)
        {
            base.Read(r);

            Flags2 = MathX.SwapEndian(r.ReadUInt32()); // another filler
            DataStreamType = (DataStreamType)MathX.SwapEndian(r.ReadUInt32());
            NumElements = (int)MathX.SwapEndian(r.ReadUInt32()); // number of elements in this chunk
            BytesPerElement = (int)MathX.SwapEndian(r.ReadUInt32()); // bytes per element
            SkipBytes(r, 8);

            // Now do loops to read for each of the different Data Stream Types. If vertices, need to populate Vector3s for example.
            switch (DataStreamType)
            {
                case DataStreamType.VERTICES: // Ref is 0x00000000
                    Vertices = new Vector3[NumElements];
                    switch (BytesPerElement)
                    {
                        case 12:
                            for (int i = 0; i < NumElements; i++)
                            {
                                Vertices[i].X = MathX.SwapEndian(r.ReadSingle());
                                Vertices[i].Y = MathX.SwapEndian(r.ReadSingle());
                                Vertices[i].Z = MathX.SwapEndian(r.ReadSingle());
                            }
                            break;
                    }
                    break;
                case DataStreamType.INDICES:  // Ref is
                    Indices = new uint[NumElements];
                    if (BytesPerElement == 2) for (var i = 0; i < NumElements; i++) Indices[i] = MathX.SwapEndian(r.ReadUInt16());
                    else if (BytesPerElement == 4) for (var i = 0; i < NumElements; i++) Indices[i] = MathX.SwapEndian(r.ReadUInt32());
                    break;
                case DataStreamType.NORMALS:
                    Normals = new Vector3[NumElements];
                    for (var i = 0; i < NumElements; i++)
                    {
                        Normals[i].X = MathX.SwapEndian(r.ReadSingle());
                        Normals[i].Y = MathX.SwapEndian(r.ReadSingle());
                        Normals[i].Z = MathX.SwapEndian(r.ReadSingle());
                    }
                    break;
                case DataStreamType.UVS:
                    UVs = new Vector2[NumElements];
                    for (var i = 0; i < NumElements; i++)
                    {
                        Normals[i].X = MathX.SwapEndian(r.ReadSingle());
                        Normals[i].Y = MathX.SwapEndian(r.ReadSingle());
                    }
                    break;
                case DataStreamType.TANGENTS:
                    Tangents = new Tangent[NumElements, 2];
                    Normals = new Vector3[NumElements];
                    for (var i = 0; i < NumElements; i++)
                        switch (BytesPerElement)
                        {
                            // These have to be divided by 32767 to be used properly (value between 0 and 1)
                            case 0x10:
                                // Tangent
                                Tangents[i, 0].X = MathX.SwapEndian(r.ReadInt16());
                                Tangents[i, 0].Y = MathX.SwapEndian(r.ReadInt16());
                                Tangents[i, 0].Z = MathX.SwapEndian(r.ReadInt16());
                                Tangents[i, 0].W = MathX.SwapEndian(r.ReadInt16());

                                // Binormal
                                Tangents[i, 1].X = MathX.SwapEndian(r.ReadInt16());
                                Tangents[i, 1].Y = MathX.SwapEndian(r.ReadInt16());
                                Tangents[i, 1].Z = MathX.SwapEndian(r.ReadInt16());
                                Tangents[i, 1].W = MathX.SwapEndian(r.ReadInt16());
                                break;
                            // These have to be divided by 127 to be used properly (value between 0 and 1)
                            case 0x08:
                                // Tangent
                                Tangents[i, 0].W = r.ReadSByte() / 127f;
                                Tangents[i, 0].X = r.ReadSByte() / 127f;
                                Tangents[i, 0].Y = r.ReadSByte() / 127f;
                                Tangents[i, 0].Z = r.ReadSByte() / 127f;

                                // Binormal
                                Tangents[i, 1].W = r.ReadSByte() / 127f;
                                Tangents[i, 1].X = r.ReadSByte() / 127f;
                                Tangents[i, 1].Y = r.ReadSByte() / 127f;
                                Tangents[i, 1].Z = r.ReadSByte() / 127f;
                                break;
                            default: throw new Exception("Need to add new Tangent Size");
                        }
                    break;
                case DataStreamType.COLORS:
                    switch (BytesPerElement)
                    {
                        case 3:
                            Colors = new IRGBA[NumElements];
                            for (var i = 0; i < NumElements; i++)
                                Colors[i] = new IRGBA(
                                    r: r.ReadByte(),
                                    g: r.ReadByte(),
                                    b: r.ReadByte(),
                                    a: 255);
                            break;
                        case 4: Colors = r.ReadTArray<IRGBA>(IRGBA.SizeOf, NumElements); break;
                        default: Log("Unknown Color Depth"); SkipBytes(r, NumElements * BytesPerElement); break;
                    }
                    break;
                case DataStreamType.BONEMAP:
                    var skin = GetSkinningInfo();
                    skin.HasBoneMapDatastream = true;
                    skin.BoneMapping = new List<MeshBoneMapping>();
                    // Bones should have 4 bone IDs (index) and 4 weights.
                    for (var i = 0; i < NumElements; i++)
                    {
                        var map = new MeshBoneMapping();
                        switch (BytesPerElement)
                        {
                            case 8:
                                map.BoneIndex = new int[4];
                                map.Weight = new int[4];
                                for (var j = 0; j < 4; j++) map.BoneIndex[j] = r.ReadByte();    // read the 4 bone indexes first
                                for (var j = 0; j < 4; j++) map.Weight[j] = r.ReadByte();       // read the weights. 
                                skin.BoneMapping.Add(map);
                                break;
                            case 12:
                                map.BoneIndex = new int[4];
                                map.Weight = new int[4];
                                for (var j = 0; j < 4; j++) map.BoneIndex[j] = MathX.SwapEndian(r.ReadUInt16());  // read the 4 bone indexes first
                                for (var j = 0; j < 4; j++) map.Weight[j] = r.ReadByte();       // read the weights.
                                skin.BoneMapping.Add(map);
                                break;
                            default: Log("Unknown BoneMapping structure"); break;
                        }
                    }
                    break;
                case DataStreamType.QTANGENTS:
                    Tangents = new Tangent[NumElements, 2];
                    Normals = new Vector3[NumElements];
                    for (var i = 0; i < NumElements; i++)
                    {
                        Tangents[i, 0].W = r.ReadSByte() / 127f;
                        Tangents[i, 0].X = r.ReadSByte() / 127f;
                        Tangents[i, 0].Y = r.ReadSByte() / 127f;
                        Tangents[i, 0].Z = r.ReadSByte() / 127f;
                        // Binormal
                        Tangents[i, 1].W = r.ReadSByte() / 127f;
                        Tangents[i, 1].X = r.ReadSByte() / 127f;
                        Tangents[i, 1].Y = r.ReadSByte() / 127f;
                        Tangents[i, 1].Z = r.ReadSByte() / 127f;
                        // Calculate the normal based on the cross product of the tangents.
                        Normals[i].X = (Tangents[i, 0].Y * Tangents[i, 1].Z - Tangents[i, 0].Z * Tangents[i, 1].Y);
                        Normals[i].Y = 0 - (Tangents[i, 0].X * Tangents[i, 1].Z - Tangents[i, 0].Z * Tangents[i, 1].X);
                        Normals[i].Z = (Tangents[i, 0].X * Tangents[i, 1].Y - Tangents[i, 0].Y * Tangents[i, 1].X);
                    }
                    break;
                default: Log("***** Unknown DataStream Type *****"); break;
            }
        }
    }
}