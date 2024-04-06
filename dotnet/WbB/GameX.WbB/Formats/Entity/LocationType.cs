using System.IO;

namespace GameX.WbB.Formats.Entity
{
    public class LocationType
    {
        public readonly int PartId;
        public readonly Frame Frame;

        public LocationType(BinaryReader r)
        {
            PartId = r.ReadInt32();
            Frame = new Frame(r);
        }

        //: Entity.LocationType
        public override string ToString() => $"Part ID: {PartId}, Frame: {Frame}";
    }
}
