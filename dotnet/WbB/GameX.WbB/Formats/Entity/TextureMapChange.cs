using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity
{
    // TODO: refactor to merge with existing TextureMapOverride object
    public class TextureMapChange : IHaveMetaInfo
    {
        public readonly byte PartIndex;
        public readonly uint OldTexture;
        public readonly uint NewTexture;

        public TextureMapChange(BinaryReader r)
        {
            PartIndex = r.ReadByte();
            OldTexture = r.ReadAsDataIDOfKnownType(0x05000000);
            NewTexture = r.ReadAsDataIDOfKnownType(0x05000000);
        }

        //: Entity.TextureMapChange
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"PartIdx: {PartIndex}"),
                new MetaInfo($"Old Texture: {OldTexture:X8}", clickable: true),
                new MetaInfo($"New Texture: {NewTexture:X8}", clickable: true),
            };
            return nodes;
        }

        //: Entity.TextureMapChange
        public override string ToString() => $"PartIdx: {PartIndex}, Old Tex: {OldTexture:X8}, New Tex: {NewTexture:X8}";
    }
}
