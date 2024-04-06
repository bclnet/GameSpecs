using GameX.WbB.Formats.Entity;
using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.WbB.Formats.FileTypes
{
    /// <summary>
    /// These are client_portal.dat files starting with 0x33. 
    /// </summary>
    [PakFileType(PakFileType.PhysicsScript)]
    public class PhysicsScript : FileType, IHaveMetaInfo
    {
        public readonly PhysicsScriptData[] ScriptData;

        public PhysicsScript(BinaryReader r)
        {
            Id = r.ReadUInt32();
            ScriptData = r.ReadL32FArray(x => new PhysicsScriptData(x));
        }

        //: FileTypes.PhysicsScript
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"{nameof(PhysicsScript)}: {Id:X8}", items: new List<MetaInfo> {
                    new MetaInfo("Scripts", items: ScriptData.Select(x => new MetaInfo($"HookType: {x.Hook.HookType}, StartTime: {x.StartTime}", items: (AnimationHook.Factory(x.Hook) as IHaveMetaInfo).GetInfoNodes(tag: tag)))),
                })
            };
            return nodes;
        }
    }
}
