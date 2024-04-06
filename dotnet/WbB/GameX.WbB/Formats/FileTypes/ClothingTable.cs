using GameX.WbB.Formats.Entity;
using GameX.WbB.Formats.Props;
using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.WbB.Formats.FileTypes
{
    /// <summary>
    /// These are client_portal.dat files starting with 0x10. 
    /// It contains information on an items model, texture changes, available palette(s) and icons for that item.
    /// </summary>
    /// <remarks>
    /// Thanks to Steven Nygard and his work on the Mac program ACDataTools that were used to help debug & verify some of this data.
    /// </remarks>
    [PakFileType(PakFileType.Clothing)]
    public class ClothingTable : FileType, IHaveMetaInfo
    {
        /// <summary>
        /// Key is the setup model id
        /// </summary>
        public readonly IDictionary<uint, ClothingBaseEffect> ClothingBaseEffects;
        /// <summary>
        /// Key is PaletteTemplate
        /// </summary>
        public readonly IDictionary<uint, CloSubPalEffect> ClothingSubPalEffects;

        public ClothingTable(BinaryReader r)
        {
            Id = r.ReadUInt32();
            ClothingBaseEffects = r.Skip(2).ReadL16TMany<uint, ClothingBaseEffect>(sizeof(uint), x => new ClothingBaseEffect(x));
            ClothingSubPalEffects = r.Skip(2).ReadL16TMany<uint, CloSubPalEffect>(sizeof(uint), x => new CloSubPalEffect(x));
        }

        //: FileTypes.ClothingTable
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"{nameof(ClothingTable)}: {Id:X8}", items: new List<MetaInfo> {
                    new MetaInfo("Base Effects", items: ClothingBaseEffects.Select(x => new MetaInfo($"{x.Key:X8}", items: (x.Value as IHaveMetaInfo).GetInfoNodes(), clickable: true))),
                    new MetaInfo("SubPalette Effects", items: ClothingSubPalEffects.OrderBy(i => i.Key).Select(x => new MetaInfo($"{x.Key} - {(PaletteTemplate)x.Key}", items: (x.Value as IHaveMetaInfo).GetInfoNodes()))),
                })
            };
            return nodes;
        }

        public uint GetIcon(uint palEffectIdx) => ClothingSubPalEffects.TryGetValue(palEffectIdx, out CloSubPalEffect result) ? result.Icon : 0;

        /// <summary>
        /// Calculates the ClothingPriority of an item based on the actual coverage. So while an Over-Robe may just be "Chest", we want to know it covers everything but head & arms.
        /// </summary>
        /// <param name="setupId">Defaults to HUMAN_MALE if not set, which is good enough</param>
        /// <returns></returns>
        public CoverageMask? GetVisualPriority(uint setupId = 0x02000001)
        {
            if (!ClothingBaseEffects.TryGetValue(setupId, out var clothingBaseEffect)) return null;
            CoverageMask visualPriority = 0;
            foreach (var t in clothingBaseEffect.CloObjectEffects)
                switch (t.Index)
                {
                    case 0: // HUMAN_ABDOMEN;
                        visualPriority |= CoverageMask.OuterwearAbdomen; break;
                    case 1: // HUMAN_LEFT_UPPER_LEG;
                    case 5: // HUMAN_RIGHT_UPPER_LEG;
                        visualPriority |= CoverageMask.OuterwearUpperLegs; break;
                    case 2: // HUMAN_LEFT_LOWER_LEG;
                    case 6: // HUMAN_RIGHT_LOWER_LEG;
                        visualPriority |= CoverageMask.OuterwearLowerLegs; break;
                    case 3: // HUMAN_LEFT_FOOT;
                    case 4: // HUMAN_LEFT_TOE;
                    case 7: // HUMAN_RIGHT_FOOT;
                    case 8: // HUMAN_RIGHT_TOE;
                        visualPriority |= CoverageMask.Feet; break;
                    case 9: // HUMAN_CHEST;
                        visualPriority |= CoverageMask.OuterwearChest; break;
                    case 10: // HUMAN_LEFT_UPPER_ARM;
                    case 13: // HUMAN_RIGHT_UPPER_ARM;
                        visualPriority |= CoverageMask.OuterwearUpperArms; break;
                    case 11: // HUMAN_LEFT_LOWER_ARM;
                    case 14: // HUMAN_RIGHT_LOWER_ARM;
                        visualPriority |= CoverageMask.OuterwearLowerArms; break;
                    case 12: // HUMAN_LEFT_HAND;
                    case 15: // HUMAN_RIGHT_HAND;
                        visualPriority |= CoverageMask.Hands; break;
                    case 16: // HUMAN_HEAD;
                        visualPriority |= CoverageMask.Head; break;
                    default: break; // Lots of things we don't care about
                }
            return visualPriority;
        }
    }
}
