using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.WbB.Formats.Entity
{
    public class ScriptAndModData : IGetMetadataInfo
    {
        public readonly float Mod;
        public readonly uint ScriptId;

        public ScriptAndModData(BinaryReader r)
        {
            Mod = r.ReadSingle();
            ScriptId = r.ReadUInt32();
        }

        //: Entity.ScriptMod
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"{Mod}"),
                new MetadataInfo($"{ScriptId:X8}", clickable: true),
            };
            return nodes;
        }

        //: Entity.ScriptMod
        public override string ToString() => $"Mod: {Mod}, Script: {ScriptId:X8}";
    }
}
