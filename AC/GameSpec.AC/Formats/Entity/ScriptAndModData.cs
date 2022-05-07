using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity
{
    public class ScriptAndModData : IGetExplorerInfo
    {
        public readonly float Mod;
        public readonly uint ScriptId;

        public ScriptAndModData(BinaryReader r)
        {
            Mod = r.ReadSingle();
            ScriptId = r.ReadUInt32();
        }

        //: Entity.ScriptMod
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"{Mod}"),
                new ExplorerInfoNode($"{ScriptId:X8}", clickable: true),
            };
            return nodes;
        }

        //: Entity.ScriptMod
        public override string ToString() => $"Mod: {Mod}, Script: {ScriptId:X8}";
    }
}
