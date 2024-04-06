using GameX.Meta;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static OpenStack.Debug;

namespace GameX.Lucas.Formats
{
    public class Binary_San : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_San(r, (int)f.FileSize));

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        unsafe struct X_Header
        {
            public static (string, int) Struct = (">2I", sizeof(X_Header)); //: BE
            public uint Magic; // 'ANIM'
            public uint ChunkSize;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        unsafe struct X_AHeader
        {
            public static (string, int) Struct = (">2I3H", sizeof(X_AHeader));
            public uint Magic; // 'AHDR'
            public uint Size;
            public ushort Version;
            public ushort NumFrames;
            public ushort Unknown;
            public fixed byte Palette[0x300];
        }

        Range _palDirty = 0..255;
        void SetDirtyColors(int min, int max)
        {
            //if (_palDirty.Start.Value > min)
            //    _palDirty.Start = min;
            //if (_palDirtyMax < max)
            //    _palDirtyMax = max;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        unsafe struct X_Chunk
        {
            public static (string, int) Struct = (">2I", sizeof(X_Chunk));
            public uint Magic; // 'FRME'
            public uint ChunkSize;
            public readonly uint Size => ChunkSize + ((ChunkSize & 1) != 0 ? 1U : 0U);
        }

        //[StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        //unsafe struct X_Chunk_FOBJ
        //{
        //    public static (string, int) Struct = (">2I", sizeof(X_Chunk_FOBJ));
        //    public uint Magic; // 'FOBJ'
        //    public uint ChunkSize;
        //}

        //[StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        //unsafe struct X_Chunk_IACT
        //{
        //    public static (string, int) Struct = (">2I", sizeof(X_Chunk_IACT));
        //    public uint Magic; // 'IACT'
        //    public uint ChunkSize;
        //}

        //[StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        //unsafe struct X_Chunk_PSAD
        //{
        //    public static (string, int) Struct = (">2I", sizeof(X_Chunk_PSAD));
        //    public uint Magic; // 'PSAD'
        //    public uint ChunkSize;
        //}

        public Binary_San(BinaryReader r, int fileSize)
        {
            const uint ANIM_MAGIC = 0x414e494d;
            const uint AHDR_MAGIC = 0x41484452;
            const uint FRME_MAGIC = 0x46524d45;

            //const uint NPAL_MAGIC = 0x4e50414c;
            //const uint ZFOB_MAGIC = 0x5a464f42;

            // read header
            var header = r.ReadS<X_Header>();
            if (header.Magic != ANIM_MAGIC) throw new FormatException("BAD MAGIC");

            // read aheader
            var aheader = r.ReadS<X_AHeader>();
            if (aheader.Magic != AHDR_MAGIC) throw new FormatException("BAD MAGIC");
            var aheaderBody = r.ReadBytes((int)aheader.Size - 6);

            // read frames
            for (var f = 0; f < aheader.NumFrames; f++)
            {
                var chunk = r.ReadS<X_Chunk>();
                if (chunk.Magic != FRME_MAGIC) throw new FormatException("BAD MAGIC");
                var chunkEnd = r.BaseStream.Position + chunk.ChunkSize;
                while (r.BaseStream.Position < chunkEnd)
                {
                    chunk = r.ReadS<X_Chunk>();
                    switch (chunk.Magic)
                    {
                        //case NPAL_MAGIC:
                        //case ZFOB_MAGIC:
                        default:
                            Log($"{Encoding.ASCII.GetString(BitConverter.GetBytes(chunk.Magic).Reverse().ToArray())}");
                            r.Skip(chunk.Size);
                            break;
                    }
                }
            }
        }

        void HandleFrame(BinaryReader r, int frameSize)
        {

        }

        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => new List<MetaInfo> {
            new MetaInfo(null, new MetaContent { Type = "Data", Name = Path.GetFileName(file.Path), Value = null, Tag = Path.GetExtension(file.Path) }),
        };
    }
}
