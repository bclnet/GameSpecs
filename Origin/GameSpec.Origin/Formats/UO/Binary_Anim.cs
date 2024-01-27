using GameSpec.Formats;
using GameSpec.Meta;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Origin.Formats.UO
{
    public unsafe class Binary_Anim : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Anim(r));

        // file: artLegacyMUL.mul:static/file04000.art
        public Binary_Anim(BinaryReader r)
        {
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "Anim File" }),
                new MetaInfo("Anim", items: new List<MetaInfo> {
                    //new MetaInfo($"Default: {Default.GumpID}"),
                })
            };
            return nodes;
        }
    }
}
