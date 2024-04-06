using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity
{
    public class ScriptAndModData : IHaveMetaInfo
    {
        public readonly float Mod;
        public readonly uint ScriptId;

        public ScriptAndModData(BinaryReader r)
        {
            Mod = r.ReadSingle();
            ScriptId = r.ReadUInt32();
        }

        //: Entity.ScriptMod
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"{Mod}"),
                new MetaInfo($"{ScriptId:X8}", clickable: true),
            };
            return nodes;
        }

        //: Entity.ScriptMod
        public override string ToString() => $"Mod: {Mod}, Script: {ScriptId:X8}";
    }
}
