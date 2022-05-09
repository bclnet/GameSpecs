using GameSpec.AC.Formats.Entity;
using GameSpec.Metadata;
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
    public class PhysicsScript : FileType, IGetMetadataInfo
    {
        public readonly PhysicsScriptData[] ScriptData;

        public PhysicsScript(BinaryReader r)
        {
            Id = r.ReadUInt32();
            ScriptData = r.ReadL32Array(x => new PhysicsScriptData(x));
        }

        //: FileTypes.PhysicsScript
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"{nameof(PhysicsScript)}: {Id:X8}", items: new List<MetadataInfo> {
                    new MetadataInfo("Scripts", items: ScriptData.Select(x => new MetadataInfo($"HookType: {x.Hook.HookType}, StartTime: {x.StartTime}", items: (AnimationHook.Factory(x.Hook) as IGetMetadataInfo).GetInfoNodes(tag: tag)))),
                })
            };
            return nodes;
        }
    }
}
