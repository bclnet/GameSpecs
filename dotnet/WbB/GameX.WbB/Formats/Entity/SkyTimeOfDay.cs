using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.WbB.Formats.Entity
{
    public class SkyTimeOfDay : IHaveMetaInfo
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

            SkyObjReplace = r.ReadL32FArray(x => new SkyObjectReplace(x));
        }

        //: Entity.SkyTimeOfDay
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"Begin: {Begin}"),
                new MetaInfo($"DirBright: {DirBright}"),
                new MetaInfo($"DirHeading: {DirHeading}"),
                new MetaInfo($"DirPitch: {DirPitch}"),
                new MetaInfo($"DirColor: {DirColor:X8}"),
                new MetaInfo($"AmbientBrightness: {AmbBright}"),
                new MetaInfo($"AmbientColor: {AmbColor:X8}"),
                new MetaInfo($"MinFog: {MinWorldFog}"),
                new MetaInfo($"MaxFog: {MaxWorldFog}"),
                new MetaInfo($"FogColor: {WorldFogColor:X8}"),
                new MetaInfo($"Fog: {WorldFog}"),
                new MetaInfo("SkyObjectReplace", items: SkyObjReplace.Select(x => {
                    var items = (x as IHaveMetaInfo).GetInfoNodes();
                    var name = items[0].Name.Replace("ObjIdx: ", "");
                    items.RemoveAt(0);
                    return new MetaInfo(name, items: items);
                })),
            };
            return nodes;
        }
    }
}
