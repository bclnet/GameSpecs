using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity
{
    /// <summary>
    /// I'm not quite sure what a "Stab" is, but this is what the client calls these.
    /// It is an object and a corresponding position. 
    /// Note that since these are referenced by either a landblock or a cellblock, the corresponding Landblock and Cell should come from the parent.
    /// </summary>
    public class Stab : IHaveMetaInfo
    {
        public readonly uint Id;
        public readonly Frame Frame;

        public Stab(BinaryReader r)
        {
            Id = r.ReadUInt32();
            Frame = new Frame(r);
        }

        //: Entity.Stab
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"ID: {Id:X8}", clickable: true),
                new MetaInfo($"Frame: {Frame}"),
            };
            return nodes;
        }

        //: Entity.Stab
        public override string ToString() => $"ID: {Id:X8}, Frame: {Frame}";
    }
}
