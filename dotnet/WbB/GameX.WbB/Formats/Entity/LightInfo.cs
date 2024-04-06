using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace GameX.WbB.Formats.Entity
{
    public class LightInfo : IHaveMetaInfo
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
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"Viewer space location: {ViewerSpaceLocation}"),
                new MetaInfo($"Color: {ColorX.ToRGBA(Color)}"),
                new MetaInfo($"Intensity: {Intensity}"),
                new MetaInfo($"Falloff: {Falloff}"),
                new MetaInfo($"ConeAngle: {ConeAngle}"),
            };
            return nodes;
        }

        //: Entity.LightInfo
        public override string ToString() => $"Viewer Space Location: {ViewerSpaceLocation}, Color: {ColorX.ToRGBA(Color)}, Intensity: {Intensity}, Falloff: {Falloff}, Cone Angle: {ConeAngle}";
    }
}
