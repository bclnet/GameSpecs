using GameSpec.Explorer;
using GameSpec.Formats;
using System;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.Red.Formats
{
    public class BinaryPak : IGetExplorerInfo
    {
        public BinaryPak() { }
        public BinaryPak(BinaryReader r) => Read(r);

        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode("BinaryPak", items: new List<ExplorerInfoNode> {
                    //new ExplorerInfoNode($"Type: {Type}"),
                })
            };
            return nodes;
        }

        public unsafe void Read(BinaryReader r)
            => throw new NotImplementedException();
    }
}
