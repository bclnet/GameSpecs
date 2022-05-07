using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace GameSpec.AC.Formats.Entity
{
    public class LightInfo : IGetExplorerInfo
    {
        public readonly Frame ViewerSpaceLocation;
        public readonly uint Color; // _RGB Color. Red is bytes 3-4, Green is bytes 5-6, Blue is bytes 7-8. Bytes 1-2 are always FF (?)
        public readonly float Intensity;
        public readonly float Falloff;
        public readonly float ConeAngle;

        public LightInfo(BinaryReader r)
        {
            ViewerSpaceLocation = new Frame(r);
            Color = r.ReadUInt32();
            Intensity = r.ReadSingle();
            Falloff = r.ReadSingle();
            ConeAngle = r.ReadSingle();
        }

        //: Entity.LightInfo
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"Viewer space location: {ViewerSpaceLocation}"),
                new ExplorerInfoNode($"Color: {ColorX.ToRGBA(Color)}"),
                new ExplorerInfoNode($"Intensity: {Intensity}"),
                new ExplorerInfoNode($"Falloff: {Falloff}"),
                new ExplorerInfoNode($"ConeAngle: {ConeAngle}"),
            };
            return nodes;
        }

        //: Entity.LightInfo
        public override string ToString() => $"Viewer Space Location: {ViewerSpaceLocation}, Color: {ColorX.ToRGBA(Color)}, Intensity: {Intensity}, Falloff: {Falloff}, Cone Angle: {ConeAngle}";
    }
}
