using GameX.Formats;
using GameX.Meta;
using System;
using System.Collections.Generic;
using System.IO;

namespace GameX.IW.Formats
{
    public class Binary_Abc : IHaveMetaInfo
    {
        public Binary_Abc() { }
        public Binary_Abc(BinaryReader r) => Read(r);

        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo("BinaryPak", items: new List<MetaInfo> {
                    //new MetaInfo($"Type: {Type}"),
                })
            };
            return nodes;
        }

        public unsafe void Read(BinaryReader r)
            => throw new NotImplementedException();
    }
}
