using GameSpec.Formats;
using GameSpec.Meta;
using GameSpec.Origin.Games.UO;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameSpec.Origin.Formats.UO
{
    public unsafe class Binary_Effect : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Effect(r));

        #region Records

        //ref: http://wpdev.sourceforge.net/docs/guide/node167.html:
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Effect
        {
            public static (string, int) Struct = ("<64x4x", sizeof(Effect));
            public fixed sbyte Frames[64];
            public byte Unknown;
            public byte FrameCount;
            public byte FrameInterval;
            public byte StartInterval;
        }

        readonly static Effect[][] AnimData = new Effect[0x0800][];

        public static Effect GetResource(int itemID)
        {
            itemID &= Database.ItemIDMask;
            return AnimData[itemID >> 3][itemID & 0x07];
        }

        #endregion

        // file: animdata.mul
        public Binary_Effect(BinaryReader r)
        {
            for (var i = 0; i < AnimData.Length; i++)
            {
                r.Skip(4);
                AnimData[i] = r.ReadSArray<Effect>(Effect.Struct, 8);
            }
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "Effect File" }),
                new MetaInfo("Effect", items: new List<MetaInfo> {
                    //new MetaInfo($"Default: {Default.GumpID}"),
                    //new MetaInfo($"Table: {Table.Count}"),
                })
            };
            return nodes;
        }
    }
}
