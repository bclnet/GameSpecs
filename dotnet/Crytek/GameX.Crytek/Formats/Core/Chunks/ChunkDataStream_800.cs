using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using static OpenStack.Debug;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public class ChunkDataStream_800 : ChunkDataStream
    {
        // This includes changes for 2. (byte4/1/2hex, and 20 byte per element vertices).
        short starCitizenFlag = 0;

        public override void Read(BinaryReader r)
        {
            base.Read(r);

            Flags2 = r.ReadUInt32(); // another filler
            DataStreamType = (DataStreamType)r.ReadUInt32();
            NumElements = (int)r.ReadUInt32(); // number of elements in this chunk
            if (_model.FileVersion == FileVersion.CryTek_3_5 || _model.FileVersion == FileVersion.CryTek_3_4) BytesPerElement = (int)r.ReadUInt32(); // bytes per element
            else if (_model.FileVersion == FileVersion.CryTek_3_6) { BytesPerElement = r.ReadInt16(); r.ReadInt16(); } // Star Citizen 2.0 is using an int16 here now. Second value is unknown. Doesn't look like padding though.
            SkipBytes(r, 8);

            // Now do loops to read for each of the different Data Stream Types. If vertices, need to populate Vector3s for example.
            switch (DataStreamType)
            {
                case DataStreamType.VERTICES: // Ref is 0x00000000
                    switch (BytesPerElement)
                    {
                        case 12: Vertices = r.ReadTArray<Vector3>(MathX.SizeOfVector3, NumElements); break;
                        // Prey files, and old Star Citizen files. 2 byte floats.
                        case 8: Vertices = new Vector3[NumElements]; for (var i = 0; i < NumElements; i++) { Vertices[i] = r.ReadHalfVector3(); r.ReadUInt16(); } break;
                        case 16: Vertices = new Vector3[NumElements]; for (var i = 0; i < NumElements; i++) { Vertices[i] = r.ReadVector3(); SkipBytes(r, 4); } break;
                    }
                    break;
                case DataStreamType.INDICES:  // Ref is
                    if (BytesPerElement == 2) { Indices = new uint[NumElements]; for (var i = 0; i < NumElements; i++) Indices[i] = r.ReadUInt16(); }
                    else if (BytesPerElement == 4) Indices = r.ReadTArray<uint>(sizeof(uint), NumElements);
                    break;
                case DataStreamType.NORMALS: Normals = r.ReadTArray<Vector3>(MathX.SizeOfVector3, NumElements); break;
                case DataStreamType.UVS: UVs = r.ReadTArray<Vector2>(MathX.SizeOfVector2, NumElements); break;
                case DataStreamType.TANGENTS:
                    Tangents = new Tangent[NumElements, 2];
                    Normals = new Vector3[NumElements];
                    for (var i = 0; i < NumElements; i++)
                        switch (BytesPerElement)
                        {
                            // These have to be divided by 32767 to be used properly (value between 0 and 1)
                            case 0x10:
                                // Tangent
                                Tangents[i, 0].X = r.ReadInt16();
                                Tangents[i, 0].Y = r.ReadInt16();
                                Tangents[i, 0].Z = r.ReadInt16();
                                Tangents[i, 0].W = r.ReadInt16();

                                // Binormal
                                Tangents[i, 1].X = r.ReadInt16();
                                Tangents[i, 1].Y = r.ReadInt16();
                                Tangents[i, 1].Z = r.ReadInt16();
                                Tangents[i, 1].W = r.ReadInt16();
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

                                // Calculate the normal based on the cross product of the tangents.
                                Normals[i].X = (Tangents[i, 0].Y * Tangents[i, 1].Z - Tangents[i, 0].Z * Tangents[i, 1].Y);
                                Normals[i].Y = 0 - (Tangents[i, 0].X * Tangents[i, 1].Z - Tangents[i, 0].Z * Tangents[i, 1].X);
                                Normals[i].Z = (Tangents[i, 0].X * Tangents[i, 1].Y - Tangents[i, 0].Y * Tangents[i, 1].X);
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
                case DataStreamType.VERTSUVS:  // 3 half floats for verts, 3 half floats for normals, 2 half floats for UVs
                    Vertices = new Vector3[NumElements];
                    Normals = new Vector3[NumElements];
                    Colors = new IRGBA[NumElements];
                    UVs = new Vector2[NumElements];
                    switch (BytesPerElement)
                    {
                        // Used in 2.6 skin files. 3 floats for vertex position, 4 bytes for normals, 2 halfs for UVs.  Normals are calculated from Tangents
                        case 20:
                            for (var i = 0; i < NumElements; i++)
                            {
                                Vertices[i] = r.ReadVector3(); // For some reason, skins are an extra 1 meter in the z direction.
                                // Normals are stored in a signed byte, prob div by 127.
                                Normals[i].X = r.ReadSByte() / 127f;
                                Normals[i].Y = r.ReadSByte() / 127f;
                                Normals[i].Z = r.ReadSByte() / 127f;
                                r.ReadSByte(); // Should be FF.
                                UVs[i].X = (float)r.ReadHalf();
                                UVs[i].Y = (float)r.ReadHalf();
                            }
                            break;
                        // 3 half floats for verts, 3 colors, 2 half floats for UVs
                        case 16 when starCitizenFlag == 257:
                            for (var i = 0; i < NumElements; i++)
                            {
                                Vertices[i] = r.ReadHalf16Vector3();
                                SkipBytes(r, 2);

                                Colors[i] = new IRGBA(
                                    b: r.ReadByte(),
                                    g: r.ReadByte(),
                                    r: r.ReadByte(),
                                    a: r.ReadByte());

                                // Inelegant hack for Blender, as it's Collada importer doesn't support Alpha channels, and some materials need the alpha channel more than the green channel.
                                // This is complicated, as some materials need the green channel more.
                                byte a = Colors[i].a, g = Colors[i].g; Colors[i].a = g; Colors[i].g = a;

                                // UVs ABSOLUTELY should use the Half structures.
                                UVs[i].X = (float)r.ReadHalf();
                                UVs[i].Y = (float)r.ReadHalf();
                            }
                            break;
                        case 16 when starCitizenFlag != 257:
                            Normals = new Vector3[NumElements];
                            // Legacy version using Halfs (Also Hunt models)
                            for (var i = 0; i < NumElements; i++)
                            {
                                Vertices[i] = r.ReadHalfVector3();
                                Normals[i] = r.ReadHalfVector3();
                                UVs[i] = r.ReadHalfVector2();
                            }
                            break;
                        default:
                            Log("Unknown VertUV structure");
                            SkipBytes(r, NumElements * BytesPerElement);
                            break;
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
                                for (var j = 0; j < 4; j++) map.BoneIndex[j] = r.ReadUInt16();  // read the 4 bone indexes first
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