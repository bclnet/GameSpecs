using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.WbB.Formats.Entity
{
    public class ObjDesc : IHaveMetaInfo
    {
        public readonly uint PaletteID;
        public readonly List<SubPalette> SubPalettes;
        public readonly List<TextureMapChange> TextureChanges;
        public readonly List<AnimationPartChange> AnimPartChanges;

        //: Entity+ObjDesc
        public ObjDesc()
        {
            SubPalettes = new List<SubPalette>();
            TextureChanges = new List<TextureMapChange>();
            AnimPartChanges = new List<AnimationPartChange>();
        }
        public ObjDesc(BinaryReader r)
        {
            r.Align();
            r.ReadByte(); // ObjDesc always starts with 11.
            var numPalettes = r.ReadByte();
            var numTextureMapChanges = r.ReadByte();
            var numAnimPartChanges = r.ReadByte();
            if (numPalettes > 0) PaletteID = r.ReadAsDataIDOfKnownType(0x04000000);
            SubPalettes = r.ReadFArray(x => new SubPalette(x), numPalettes).ToList();
            TextureChanges = r.ReadFArray(x => new TextureMapChange(x), numTextureMapChanges).ToList();
            AnimPartChanges = r.ReadFArray(x => new AnimationPartChange(x), numAnimPartChanges).ToList(); r.Align();
        }

        //: Entity.ObjDesc
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                PaletteID != 0 ? new MetaInfo($"Palette ID: {PaletteID:X8}", clickable: true) : null,
                SubPalettes.Count > 0 ? new MetaInfo("SubPalettes", items: SubPalettes.Select(x => {
                    var items = (x as IHaveMetaInfo).GetInfoNodes();
                    var name = items[0].Name;
                    items.RemoveAt(0);
                    return new MetaInfo(name, items: items);
                })) : null,
                TextureChanges.Count > 0 ? new MetaInfo("Texture Changes", items: TextureChanges.Select(x => new MetaInfo($"{x}", clickable: true))) : null,
                AnimPartChanges.Count > 0 ? new MetaInfo("AnimPart Changes", items: AnimPartChanges.Select(x => new MetaInfo($"{x}", clickable: true))) : null,
            };
            return nodes;
        }

        /// <summary>
        /// Helper function to ensure we don't add redundant parts to the list
        /// </summary>
        //: Entity+ObjDesc
        public void AddTextureChange(TextureMapChange tm)
        {
            var e = TextureChanges.FirstOrDefault(c => c.PartIndex == tm.PartIndex && c.OldTexture == tm.OldTexture && c.NewTexture == tm.NewTexture);
            if (e == null) TextureChanges.Add(tm);
        }

        /// <summary>
        /// Helper function to ensure we only have one AnimationPartChange.PartId in the list
        /// </summary>
        //: Entity+ObjDesc
        public void AddAnimPartChange(AnimationPartChange ap)
        {
            var p = AnimPartChanges.FirstOrDefault(c => c.PartIndex == ap.PartIndex && c.PartID == ap.PartID);
            if (p != null) AnimPartChanges.Remove(p);
            AnimPartChanges.Add(ap);
        }
    }
}
