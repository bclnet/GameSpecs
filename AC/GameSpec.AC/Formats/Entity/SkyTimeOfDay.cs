using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.Entity
{
    public class SkyTimeOfDay : IGetExplorerInfo
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
            WorldFog = r.ReadUInt32(); r.AlignBoundary();

            SkyObjReplace = r.ReadL32Array(x => new SkyObjectReplace(x));
        }

        //: Entity.SkyTimeOfDay
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"Begin: {Begin}"),
                new ExplorerInfoNode($"DirBright: {DirBright}"),
                new ExplorerInfoNode($"DirHeading: {DirHeading}"),
                new ExplorerInfoNode($"DirPitch: {DirPitch}"),
                new ExplorerInfoNode($"DirColor: {DirColor:X8}"),
                new ExplorerInfoNode($"AmbientBrightness: {AmbBright}"),
                new ExplorerInfoNode($"AmbientColor: {AmbColor:X8}"),
                new ExplorerInfoNode($"MinFog: {MinWorldFog}"),
                new ExplorerInfoNode($"MaxFog: {MaxWorldFog}"),
                new ExplorerInfoNode($"FogColor: {WorldFogColor:X8}"),
                new ExplorerInfoNode($"Fog: {WorldFog}"),
                new ExplorerInfoNode("SkyObjectReplace", items: SkyObjReplace.Select(x => {
                    var items = (x as IGetExplorerInfo).GetInfoNodes();
                    var name = items[0].Name.Replace("ObjIdx: ", "");
                    items.RemoveAt(0);
                    return new ExplorerInfoNode(name, items: items);
                })),
            };
            return nodes;
        }
    }
}
