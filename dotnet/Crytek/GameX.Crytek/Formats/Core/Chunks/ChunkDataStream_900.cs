using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using static OpenStack.Debug;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public class ChunkDataStream_900 : ChunkDataStream
    {
        public ChunkDataStream_900(int numElements)
            => NumElements = numElements;

        public override void Read(BinaryReader r)
        {
            base.Read(r);

            DataStreamType = (DataStreamType)r.ReadUInt32();
            SkipBytes(r, 4);
            BytesPerElement = (int)r.ReadUInt32();

            switch (DataStreamType)
            {
                case DataStreamType.IVOINDICES:
                    if (BytesPerElement == 2)
                    {
                        Indices = new uint[NumElements]; for (var i = 0; i < NumElements; i++) Indices[i] = r.ReadUInt16();
                        if (NumElements % 2 == 1) SkipBytes(r, 2);
                        else
                        {
                            var peek = Convert.ToChar(r.ReadByte()); // Sometimes the next Ivo chunk has a 4 byte filler, sometimes it doesn't.
                            r.BaseStream.Position -= 1;
                            if (peek == 0) SkipBytes(r, 4);
                        }
                    }
                    else if (BytesPerElement == 4) Indices = r.ReadTArray<uint>(sizeof(uint), NumElements);
                    break;
                case DataStreamType.IVOVERTSUVS:
                    Vertices = new Vector3[NumElements];
                    Normals = new Vector3[NumElements];
                    Colors = new IRGBA[NumElements];
                    UVs = new Vector2[NumElements];
                    switch (BytesPerElement)
                    {
                        case 20:
                            for (var i = 0; i < NumElements; i++)
                            {
                                Vertices[i] = r.ReadVector3(); // For some reason, skins are an extra 1 meter in the z direction.
                                Colors[i] = new IRGBA(
                                    b: r.ReadByte(),
                                    g: r.ReadByte(),
                                    r: r.ReadByte(),
                                    a: r.ReadByte());

                                // Inelegant hack for Blender, as it's Collada importer doesn't support Alpha channels, and some materials need the alpha channel more than the green channel.
                                // This is complicated, as some materials need the green channel more.
                                byte a = Colors[i].a, g = Colors[i].g; Colors[i].a = g; Colors[i].g = a;

                                UVs[i].X = (float)r.ReadHalf();
                                UVs[i].Y = (float)r.ReadHalf();
                            }
                            if (NumElements % 2 == 1) SkipBytes(r, 4);
                            break;
                    }
                    break;
                case DataStreamType.IVONORMALS:
                case DataStreamType.IVONORMALS2:
                    switch (BytesPerElement)
                    {
                        case 4:
                            Normals = new Vector3[NumElements];
                            for (var i = 0; i < NumElements; i++)
                            {
                                var x = r.ReadSByte() / 128f;
                                var y = r.ReadSByte() / 128f;
                                var z = r.ReadSByte() / 128f;
                                var w = r.ReadSByte() / 128f;
                                Normals[i].X = 2.0f * (x * z + y * w);
                                Normals[i].Y = 2.0f * (y * z - x * w);
                                Normals[i].Z = (2.0f * (z * z + w * w)) - 1.0f;
                            }
                            if (NumElements % 2 == 1) SkipBytes(r, 4);
                            break;
                        default: Log("Unknown Normals Format"); SkipBytes(r, NumElements * BytesPerElement); break;
                    }
                    break;
                case DataStreamType.IVONORMALS3: break;
                case DataStreamType.IVOTANGENTS:
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
                case DataStreamType.IVOBONEMAP:
                    var skin = GetSkinningInfo();
                    skin.HasBoneMapDatastream = true;
                    skin.BoneMapping = new List<MeshBoneMapping>();
                    switch (BytesPerElement)
                    {
                        case 12:
                            for (var i = 0; i < NumElements; i++)
                            {
                                var map = new MeshBoneMapping();
                                for (var j = 0; j < 4; j++) map.BoneIndex[j] = r.ReadUInt16();  // read the 4 bone indexes first
                                for (var j = 0; j < 4; j++) map.Weight[j] = r.ReadByte();       // read the weights. 
                                skin.BoneMapping.Add(map);
                            }
                            if (NumElements % 2 == 1) SkipBytes(r, 4);
                            break;
                        default: Log("Unknown BoneMapping structure"); break;
                    }
                    break;
                case DataStreamType.IVOUNKNOWN2: break;
                //default: Log("***** Unknown DataStream Type *****"); break;
            }
        }
    }
}