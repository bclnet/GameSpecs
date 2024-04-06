using GameX.WbB.Formats.Props;
using GameX.Meta;
using GameX.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.WbB.Formats.Entity
{
    public class SexCG : IHaveMetaInfo
    {
        public string Name;
        public uint Scale;
        public uint SetupID;
        public uint SoundTable;
        public uint IconImage;
        public uint BasePalette;
        public uint SkinPalSet;
        public uint PhysicsTable;
        public uint MotionTable;
        public uint CombatTable;
        public ObjDesc BaseObjDesc;
        public uint[] HairColorList;
        public HairStyleCG[] HairStyleList;
        public uint[] EyeColorList;
        public EyeStripCG[] EyeStripList;
        public FaceStripCG[] NoseStripList;
        public FaceStripCG[] MouthStripList;
        public GearCG[] HeadgearList;
        public GearCG[] ShirtList;
        public GearCG[] PantsList;
        public GearCG[] FootwearList;
        public uint[] ClothingColorsList;

        public SexCG(BinaryReader r)
        {
            Name = r.ReadString();
            Scale = r.ReadUInt32();
            SetupID = r.ReadUInt32();
            SoundTable = r.ReadUInt32();
            IconImage = r.ReadUInt32();
            BasePalette = r.ReadUInt32();
            SkinPalSet = r.ReadUInt32();
            PhysicsTable = r.ReadUInt32();
            MotionTable = r.ReadUInt32();
            CombatTable = r.ReadUInt32();
            BaseObjDesc = new ObjDesc(r);
            HairColorList = r.ReadC32TArray<uint>(sizeof(uint));
            HairStyleList = r.ReadC32FArray(x => new HairStyleCG(x));
            EyeColorList = r.ReadC32TArray<uint>(sizeof(uint));
            EyeStripList = r.ReadC32FArray(x => new EyeStripCG(x));
            NoseStripList = r.ReadC32FArray(x => new FaceStripCG(x));
            MouthStripList = r.ReadC32FArray(x => new FaceStripCG(x));
            HeadgearList = r.ReadC32FArray(x => new GearCG(x));
            ShirtList = r.ReadC32FArray(x => new GearCG(x));
            PantsList = r.ReadC32FArray(x => new GearCG(x));
            FootwearList = r.ReadC32FArray(x => new GearCG(x));
            ClothingColorsList = r.ReadC32TArray<uint>(sizeof(uint));
        }

        // Eyes
        public uint GetEyeTexture(uint eyesStrip, bool isBald) => (isBald ? EyeStripList[Convert.ToInt32(eyesStrip)].ObjDescBald : EyeStripList[Convert.ToInt32(eyesStrip)].ObjDesc).TextureChanges[0].NewTexture;
        public uint GetDefaultEyeTexture(uint eyesStrip, bool isBald) => (isBald ? EyeStripList[Convert.ToInt32(eyesStrip)].ObjDescBald : EyeStripList[Convert.ToInt32(eyesStrip)].ObjDesc).TextureChanges[0].OldTexture;

        // Nose
        public uint GetNoseTexture(uint noseStrip) => NoseStripList[Convert.ToInt32(noseStrip)].ObjDesc.TextureChanges[0].NewTexture;
        public uint GetDefaultNoseTexture(uint noseStrip) => NoseStripList[Convert.ToInt32(noseStrip)].ObjDesc.TextureChanges[0].OldTexture;

        // Mouth
        public uint GetMouthTexture(uint mouthStrip) => MouthStripList[Convert.ToInt32(mouthStrip)].ObjDesc.TextureChanges[0].NewTexture;
        public uint GetDefaultMouthTexture(uint mouthStrip) => MouthStripList[Convert.ToInt32(mouthStrip)].ObjDesc.TextureChanges[0].OldTexture;

        // Hair (Head)
        public uint? GetHeadObject(uint hairStyle)
        {
            var hairstyle = HairStyleList[Convert.ToInt32(hairStyle)];
            // Gear Knights, both Olthoi types have multiple anim part changes.
            return hairstyle.ObjDesc.AnimPartChanges.Count == 1 ? hairstyle.ObjDesc.AnimPartChanges[0].PartID : (uint?)null;
        }
        public uint? GetHairTexture(uint hairStyle)
        {
            var hairstyle = HairStyleList[Convert.ToInt32(hairStyle)];
            // OlthoiAcid has no TextureChanges
            return hairstyle.ObjDesc.TextureChanges.Count > 0 ? (uint?)hairstyle.ObjDesc.TextureChanges[0].NewTexture : null;
        }
        public uint? GetDefaultHairTexture(uint hairStyle)
        {
            var hairstyle = HairStyleList[Convert.ToInt32(hairStyle)];
            // OlthoiAcid has no TextureChanges
            return hairstyle.ObjDesc.TextureChanges.Count > 0 ? (uint?)hairstyle.ObjDesc.TextureChanges[0].OldTexture : null;
        }

        // Headgear
        public uint GetHeadgearWeenie(uint headgearStyle) => HeadgearList[Convert.ToInt32(headgearStyle)].WeenieDefault;
        public uint GetHeadgearClothingTable(uint headgearStyle) => HeadgearList[Convert.ToInt32(headgearStyle)].ClothingTable;

        // Shirt
        public uint GetShirtWeenie(uint shirtStyle) => ShirtList[Convert.ToInt32(shirtStyle)].WeenieDefault;
        public uint GetShirtClothingTable(uint shirtStyle) => ShirtList[Convert.ToInt32(shirtStyle)].ClothingTable;

        // Pants
        public uint GetPantsWeenie(uint pantsStyle) => PantsList[Convert.ToInt32(pantsStyle)].WeenieDefault;
        public uint GetPantsClothingTable(uint pantsStyle) => PantsList[Convert.ToInt32(pantsStyle)].ClothingTable;

        // Footwear
        public uint GetFootwearWeenie(uint footwearStyle) => FootwearList[Convert.ToInt32(footwearStyle)].WeenieDefault;
        public uint GetFootwearClothingTable(uint footwearStyle) => FootwearList[Convert.ToInt32(footwearStyle)].ClothingTable;

        //: Entity.SexCG
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"Name: {Name}"),
                new MetaInfo($"Scale: {Scale}%"),
                new MetaInfo($"Setup: {SetupID:X8}", clickable: true),
                new MetaInfo($"Sound Table: {SoundTable:X8}", clickable: true),
                new MetaInfo($"Icon: {IconImage:X8}", clickable: true),
                new MetaInfo($"Base Palette: {BasePalette:X8}", clickable: true),
                new MetaInfo($"Skin Palette Set: {SkinPalSet:X8}", clickable: true),
                new MetaInfo($"Physics Table: {PhysicsTable:X8}", clickable: true),
                new MetaInfo($"Motion Table: {MotionTable:X8}", clickable: true),
                new MetaInfo($"Combat Table: {CombatTable:X8}", clickable: true),
                new MetaInfo("ObjDesc", items: (BaseObjDesc as IHaveMetaInfo).GetInfoNodes()),
                new MetaInfo("Hair Colors", items: HairColorList.Select(x => new MetaInfo($"{x:X8}", clickable: true))),
                new MetaInfo("Hair Styles", items: HairStyleList.Select((x, i) => new MetaInfo($"{i}", items: (x as IHaveMetaInfo).GetInfoNodes()))),
                new MetaInfo("Eye Colors", items: EyeColorList.Select(x => new MetaInfo($"{x:X8}", clickable: true))),
                new MetaInfo("Eye Strips", items: EyeStripList.Select((x, i) => new MetaInfo($"{i}", items: (x as IHaveMetaInfo).GetInfoNodes()))),
                new MetaInfo("Nose Strips", items: NoseStripList.Select((x, i) => new MetaInfo($"{i}", items: (x as IHaveMetaInfo).GetInfoNodes()))),
                new MetaInfo("Mouth Strips", items: MouthStripList.Select((x, i) => new MetaInfo($"{i}", items: (x as IHaveMetaInfo).GetInfoNodes()))),
                new MetaInfo("Headgear", items: HeadgearList.Select((x, i) => new MetaInfo($"{i}", items: (x as IHaveMetaInfo).GetInfoNodes()))),
                new MetaInfo("Shirt", items: ShirtList.Select((x, i) => new MetaInfo($"{i}", items: (x as IHaveMetaInfo).GetInfoNodes()))),
                new MetaInfo("Pants", items: PantsList.Select((x, i) => new MetaInfo($"{i}", items: (x as IHaveMetaInfo).GetInfoNodes()))),
                new MetaInfo("Footwear", items: FootwearList.Select((x, i) => new MetaInfo($"{i}", items: (x as IHaveMetaInfo).GetInfoNodes()))),
                new MetaInfo("Clothing Colors", items: ClothingColorsList.OrderBy(i => i).Select(x => new MetaInfo($"{x} - {(PaletteTemplate)x}"))),
            };
            return nodes;
        }
    }
}
