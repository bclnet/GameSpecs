using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.WbB.Formats.Entity
{
    public class SkyTimeOfDay : IGetMetadataInfo
    {
        public readonly float Begin;

        public readonly float DirBright;
        public readonly float DirHeading;
        public readonly float DirPitch;
        public readonly uint DirColor;

        public readonly float AmbBright;
        public readonly uint AmbColor;

        public readonly float MinWorldFog;
        public readonly float MaxWorldFog;
        public readonly uint WorldFogColor;
        public readonly uint WorldFog;

        public readonly SkyObjectReplace[] SkyObjReplace;

        public SkyTimeOfDay(BinaryReader r)
        {
            Begin = r.ReadSingle();

            DirBright = r.ReadSingle();
            DirHeading = r.ReadSingle();
            DirPitch = r.ReadSingle();
            DirColor = r.ReadUInt32();

            AmbBright = r.ReadSingle();
            AmbColor = r.ReadUInt32();

            MinWorldFog = r.ReadSingle();
            MaxWorldFog = r.ReadSingle();
            WorldFogColor = r.ReadUInt32();
            WorldFog = r.ReadUInt32(); r.Align();

            SkyObjReplace = r.ReadL32Array(x => new SkyObjectReplace(x));
        }

        //: Entity.SkyTimeOfDay
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"Begin: {Begin}"),
                new MetadataInfo($"DirBright: {DirBright}"),
                new MetadataInfo($"DirHeading: {DirHeading}"),
                new MetadataInfo($"DirPitch: {DirPitch}"),
                new MetadataInfo($"DirColor: {DirColor:X8}"),
                new MetadataInfo($"AmbientBrightness: {AmbBright}"),
                new MetadataInfo($"AmbientColor: {AmbColor:X8}"),
                new MetadataInfo($"MinFog: {MinWorldFog}"),
                new MetadataInfo($"MaxFog: {MaxWorldFog}"),
                new MetadataInfo($"FogColor: {WorldFogColor:X8}"),
                new MetadataInfo($"Fog: {WorldFog}"),
                new MetadataInfo("SkyObjectReplace", items: SkyObjReplace.Select(x => {
                    var items = (x as IGetMetadataInfo).GetInfoNodes();
                    var name = items[0].Name.Replace("ObjIdx: ", "");
                    items.RemoveAt(0);
                    return new MetadataInfo(name, items: items);
                })),
            };
            return nodes;
        }
    }
}
