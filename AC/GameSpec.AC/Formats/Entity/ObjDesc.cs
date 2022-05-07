using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.Entity
{
    public class ObjDesc : IGetExplorerInfo
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
            r.AlignBoundary();
            r.ReadByte(); // ObjDesc always starts with 11.
            var numPalettes = r.ReadByte();
            var numTextureMapChanges = r.ReadByte();
            var numAnimPartChanges = r.ReadByte();
            if (numPalettes > 0) PaletteID = r.ReadAsDataIDOfKnownType(0x04000000);
            SubPalettes = r.ReadTArray(x => new SubPalette(x), numPalettes).ToList();
            TextureChanges = r.ReadTArray(x => new TextureMapChange(x), numTextureMapChanges).ToList();
            AnimPartChanges = r.ReadTArray(x => new AnimationPartChange(x), numAnimPartChanges).ToList(); r.AlignBoundary();
        }

        //: Entity.ObjDesc
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                PaletteID != 0 ? new ExplorerInfoNode($"Palette ID: {PaletteID:X8}", clickable: true) : null,
                SubPalettes.Count > 0 ? new ExplorerInfoNode("SubPalettes", items: SubPalettes.Select(x => {
                    var items = (x as IGetExplorerInfo).GetInfoNodes();
                    var name = items[0].Name;
                    items.RemoveAt(0);
                    return new ExplorerInfoNode(name, items: items);
                })) : null,
                TextureChanges.Count > 0 ? new ExplorerInfoNode("Texture Changes", items: TextureChanges.Select(x => new ExplorerInfoNode($"{x}", clickable: true))) : null,
                AnimPartChanges.Count > 0 ? new ExplorerInfoNode("AnimPart Changes", items: AnimPartChanges.Select(x => new ExplorerInfoNode($"{x}", clickable: true))) : null,
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
