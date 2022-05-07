using GameSpec.AC.Formats.Entity;
using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.FileTypes
{
    /// <summary>
    /// These are client_portal.dat files starting with 0x33. 
    /// </summary>
    [PakFileType(PakFileType.PhysicsScript)]
    public class PhysicsScript : FileType, IGetExplorerInfo
    {
        public readonly PhysicsScriptData[] ScriptData;

        public PhysicsScript(BinaryReader r)
        {
            Id = r.ReadUInt32();
            ScriptData = r.ReadL32Array(x => new PhysicsScriptData(x));
        }

        //: FileTypes.PhysicsScript
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"{nameof(PhysicsScript)}: {Id:X8}", items: new List<ExplorerInfoNode> {
                    new ExplorerInfoNode("Scripts", items: ScriptData.Select(x => new ExplorerInfoNode($"HookType: {x.Hook.HookType}, StartTime: {x.StartTime}", items: (AnimationHook.Factory(x.Hook) as IGetExplorerInfo).GetInfoNodes(tag: tag)))),
                })
            };
            return nodes;
        }
    }
}
