using GameSpec.Formats;
using GameSpec.Metadata;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Arkane.Formats.Danae
{
    public class BinaryTea : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new BinaryTea(r));

        public BinaryTea() { }
        public BinaryTea(BinaryReader r) => Read(r);

        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo("BinaryTEA", items: new List<MetaInfo> {
                    //new MetaInfo($"Type: {Type}"),
                })
            };
            return nodes;
        }

        // https://github.com/OpenSourcedGames/Arx-Fatalis/blob/master/Sources/EERIE/EERIEAnim.cpp#L355
        public unsafe void Read(BinaryReader r)
        {
        }
    }
}
