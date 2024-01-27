using GameSpec.Formats;
using GameSpec.Meta;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Origin.Formats.UO
{
    public unsafe class Binary_Light : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Light(r));

        // file: artLegacyMUL.uop:land/file00000.land
        public Binary_Light(BinaryReader r)
        {
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "Light File" }),
                new MetaInfo("Light", items: new List<MetaInfo> {
                    //new MetaInfo($"Default: {Default.GumpID}"),
                })
            };
            return nodes;
        }
    }
}
