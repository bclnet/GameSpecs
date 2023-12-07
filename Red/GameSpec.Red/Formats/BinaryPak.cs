using GameSpec.Metadata;
using GameSpec.Formats;
using System;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.Red.Formats
{
    public class BinaryPak : IGetMetadataInfo
    {
        public BinaryPak() { }
        public BinaryPak(BinaryReader r) => Read(r);

        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo("BinaryPak", items: new List<MetadataInfo> {
                    //new MetadataInfo($"Type: {Type}"),
                })
            };
            return nodes;
        }

        public unsafe void Read(BinaryReader r)
            => throw new NotImplementedException();
    }
}
