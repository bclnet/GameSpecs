using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity
{
    // TODO: refactor to merge with existing TextureMapOverride object
    public class TextureMapChange : IGetMetadataInfo
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
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"PartIdx: {PartIndex}"),
                new MetadataInfo($"Old Texture: {OldTexture:X8}", clickable: true),
                new MetadataInfo($"New Texture: {NewTexture:X8}", clickable: true),
            };
            return nodes;
        }

        //: Entity.TextureMapChange
        public override string ToString() => $"PartIdx: {PartIndex}, Old Tex: {OldTexture:X8}, New Tex: {NewTexture:X8}";
    }
}
