using GameX.Formats;
using GameX.Meta;
using OpenStack.Graphics.DirectX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameX.Cryptic.Formats
{
    public unsafe class Binary_Bin : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Bin(r));

        // Headers
        #region Headers

        const ulong MAGIC = 0x5363697470797243;
        const string PARSE_N = "ParseN";
        const string PARSE_M = "ParseM";

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Header_M
        {
            public ulong Magic;                     // CrypticS
            public ushort ParseHash;                // 
            public ushort Flags;                    // 
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Header_N
        {
            public ulong Magic;                     // CrypticS
            public uint ParseHash;                  // 
            public uint Flags;                      // 
        }

        #endregion

        public Binary_Bin(BinaryReader r)
        {
            var header = r.ReadT<Header_M>(sizeof(Header_M));
            if (header.Magic != MAGIC) throw new FormatException("BAD MAGIC");
            var type = r.ReadL16String(maxLength: 4096); r.Align();
            if (type != PARSE_M) throw new FormatException("BAD TYPE");

            // file section
            var filesTag = r.ReadL16String(20); r.Align();
            if (filesTag != "Files1") throw new FormatException("BAD Tag");
            var fileSectionEnd = r.ReadUInt32() + r.Tell();
            var files = r.ReadL32FArray(x =>
            {
                var name = x.ReadL16String(maxLength: 260); x.Align();
                var timestamp = x.ReadUInt32();
                return (name, timestamp);
            });
            if (r.Tell() != fileSectionEnd) throw new FormatException("did not read blob file entry correctly");

            // extra section
            var extraTag = r.ReadL16String(20); r.Align();
            if (extraTag != "Files1") throw new FormatException("BAD Tag");
            var extraSectionEnd = r.ReadUInt32() + r.Tell();
            var extras = r.ReadL32FArray(x =>
            {
                return (string)null;
            });
            if (r.Tell() != extraSectionEnd) throw new FormatException("did not read blob file entry correctly");

            // dependency section
            var dependencyTag = r.ReadL16String(20); r.Align();
            if (dependencyTag != "Depen1") throw new FormatException("BAD Tag");
            var dependencySectionEnd = r.ReadUInt32() + r.Tell();
            var dependencys = r.ReadL32FArray(x =>
            {
                var type = x.ReadUInt32();
                var name = x.ReadL16String(maxLength: 260); x.Align();
                var hash = x.ReadUInt32();
                return (type, name, hash);
            });
            if (r.Tell() != dependencySectionEnd) throw new FormatException("did not read blob file entry correctly");
        }

        // IHaveMetaInfo
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
