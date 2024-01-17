using GameSpec.Formats;
using GameSpec.Metadata;
using OpenStack.Graphics.DirectX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameSpec.Cryptic.Formats
{
    public class Binary_Bin : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Bin(r));

        // Headers
        #region Headers

        const ulong MAGIC = 0x5363697470797243;
        const string PARSEN = "ParseN";

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Header
        {
            public ulong Magic;                     // CrypticS
            public ushort ParseHash;                // 
            public ushort Flags;                    // 
        }

        #endregion

        public Binary_Bin(BinaryReader r) => Read(r);

        public unsafe void Read(BinaryReader r)
        {
            var header = r.ReadT<Header>(sizeof(Header));
            if (header.Magic != MAGIC) throw new FormatException("BAD MAGIC");
            var type = r.ReadL16String(maxLength: 4096); r.Align();
            if (type != PARSEN) throw new FormatException("BAD TYPE");
            var filesTag = r.ReadL16String(20); r.Align();
            if (filesTag != "Files1") throw new FormatException("BAD Tag");
            var fileInfoSize = r.ReadUInt32();
        }

        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo("BinaryBin", items: new List<MetaInfo> {
                    //new MetaInfo($"Type: {Type}"),
                })
            };
            return nodes;
        }
    }
}
