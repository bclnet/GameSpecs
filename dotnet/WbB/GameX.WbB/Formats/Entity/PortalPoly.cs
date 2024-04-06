using System.IO;

namespace GameX.WbB.Formats.Entity
{
    public class PortalPoly
    {
        public readonly short PortalIndex;
        public readonly short PolygonId;

        public PortalPoly(BinaryReader r)
        {
            PortalIndex = r.ReadInt16();
            PolygonId = r.ReadInt16();
        }

        //: Entity.PortalPoly
        public override string ToString() => $"Portal Idx: {PortalIndex}, Polygon ID: {PolygonId}";
    }
}
