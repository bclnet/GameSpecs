using GameX.Formats;
using GameX.Meta;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Arkane.Formats.Danae
{
    public unsafe class Binary_Tea : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Tea(r));

        // https://github.com/OpenSourcedGames/Arx-Fatalis/blob/master/Sources/EERIE/EERIEAnim.cpp#L355
        public Binary_Tea(BinaryReader r)
        {
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo("BinaryTEA", items: new List<MetaInfo> {
                    //new MetaInfo($"Type: {Type}"),
                })
            };
            return nodes;
        }
    }
}
