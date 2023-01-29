using GameSpec.Formats;
using GameSpec.Metadata;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Arkane.Formats
{
    public class BinaryTea : IGetMetadataInfo
    {
        public static Task<object> Factory(BinaryReader r, FileMetadata f, PakFile s) => Task.FromResult((object)new BinaryTea(r));

        public BinaryTea() { }
        public BinaryTea(BinaryReader r) => Read(r);

        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo("BinaryTEA", items: new List<MetadataInfo> {
                    //new MetadataInfo($"Type: {Type}"),
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
