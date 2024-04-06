using GameX.Formats;
using GameX.Meta;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Cryptic.Formats
{
    public class Binary_Tex : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Tex(r, (int)f.FileSize));

        public string Data;

        public Binary_Tex(BinaryReader r, int fileSize)
        {
            Data = r.ReadEncoding(fileSize);
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
