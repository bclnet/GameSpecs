using GameX.Formats;
using GameX.Meta;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameX.Cryptic.Formats
{
    public unsafe class Binary_MSet : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_MSet(r));

        // MSet
        #region MSet

        public class MSetFile
        {
            public MSetFileHeader Header;
            public MSetSourceFileSet SourceFileSet;
            public MSetModel[] Models;
        }

        public class MSetFileHeader
        {
            public int HeaderSize;
            public int FileVersion;
            public int DataCrc;
            public MSetModelDefinition[] ModelDefinitions;
        }

        public class MSetModel
        {
            public int DataSize; // The size of this structure plus all persistent packed data
            public int VertexCount;
            public int FaceCount;
            public int TextureCount; // number of tex_idxs (sum of all tex_idx->counts == tri_count)
            public float AverageTexelDensity;
            public float StdDevTexelDensity;
            public int ProcessTimeFlags;
            public int Unknown1C;
            public MSetModelDataOffset[] ModelDataOffsets;
            public string _ModelName; // ignore
        }

        public class MSetModelDataOffset
        {
            public int CompressedSize;
            public int DecompressedSize;
            public int Offset;
            public bool IsEncoded;
            public byte[] Data;

            internal MSetModelDataOffset ReadData(BinaryReader r, int masterOffset)
            {
                if (Offset <= 0) return this;
                var len = CompressedSize;
                if (len == 0 && DecompressedSize > 0) len = DecompressedSize;
                r.Seek(Offset + masterOffset);
                Data = r.ReadBytes(len);
                return this;
            }
        }

        public class MSetModelDefinition
        {
            public string ModelName;
            public MSetModelOffset[] ModelOffsets;
        }

        public struct MSetModelOffset
        {
            public static (string, int) Struct = (">2i", sizeof(MSetModelOffset));
            public int Offset;
            public int Length;
        }

        public class MSetSourceFilePath
        {
            public string Path;
            public int Timestamp;
        }

        public class MSetSourceFileSet
        {
            public string FileSetName;
            public int SetLength;
            public MSetSourceFilePath[] Files;
        }

        #endregion

        // Headers
        #region Headers

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Header
        {
            public static (string, int) Struct = (">4x2i", sizeof(Header));
            public int HeaderSize;
            public int FileVersion;
            public int DataCrc;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ModelOffset
        {
            public static (string, int) Struct = (">4i2f2i", sizeof(ModelOffset));
            public int DataSize;
            public int VertexCount;
            public int FaceCount;
            public int TextureCount;
            public float AverageTexelDensity;
            public float StdDevTexelDensity;
            public int ProcessTimeFlags;
            public int Unknown1C;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ModelDataOffset
        {
            public static (string, int) Struct = (">3i", sizeof(ModelDataOffset));
            public int CompressedSize;
            public int DecompressedSize;
            public int Offset;
        }

        #endregion

        MSetFile Mset;

        public Binary_MSet(BinaryReader r)
        {
            string p;
            var header = r.ReadS<Header>();
            var mh = new MSetFileHeader
            {
                HeaderSize = header.HeaderSize,
                FileVersion = header.FileVersion,
                DataCrc = header.DataCrc,
                ModelDefinitions = r.ReadL16FArray(r1 => new MSetModelDefinition
                {
                    ModelName = r1.ReadL16AString(endian: true),
                    ModelOffsets = r1.ReadL16SArray<MSetModelOffset>(endian: true),
                }, endian: true)
            };
            r.Skip(3 * sizeof(int));
            static int PaddingSize(int len) => (4 - ((len + 6) & 3)) & 3;
            var msf = new MSetSourceFileSet
            {
                FileSetName = r.ReadL16AString(),
                SetLength = r.ReadInt32(),
                Files = r.ReadL32FArray(r1 => new MSetSourceFilePath
                {
                    Path = p = r1.ReadL16AString(),
                    Timestamp = r1.Skip(PaddingSize(p.Length)).ReadInt32(),
                }),
            };
            var mdls = mh.ModelDefinitions.SelectMany(x => x.ModelOffsets, (model, offset) =>
            {
                if (offset.Offset <= 0) return null;
                r.Seek(offset.Offset);
                var m = r.ReadS<ModelOffset>();
                return new MSetModel
                {
                    DataSize = m.DataSize,
                    VertexCount = m.VertexCount,
                    FaceCount = m.FaceCount,
                    TextureCount = m.TextureCount,
                    AverageTexelDensity = m.AverageTexelDensity,
                    StdDevTexelDensity = m.StdDevTexelDensity,
                    ProcessTimeFlags = m.ProcessTimeFlags,
                    Unknown1C = m.Unknown1C,
                    ModelDataOffsets = r.ReadSArray<ModelDataOffset>(12).Select(x => new MSetModelDataOffset
                    {
                        CompressedSize = x.CompressedSize,
                        DecompressedSize = Math.Abs(x.DecompressedSize),
                        Offset = x.Offset,
                        IsEncoded = x.DecompressedSize > 0
                    }.ReadData(r, offset.Offset)).ToArray(),
                    _ModelName = model.ModelName,
                };
            }).Where(x => x != null).ToArray();
            Mset = new MSetFile
            {
                Header = mh,
                SourceFileSet = msf,
                Models = mdls,
            };
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo("Model", items: new List<MetaInfo> {
                    //new MetaInfo($"Type: {Type}"),
                })
            };
            return nodes;
        }
    }
}
