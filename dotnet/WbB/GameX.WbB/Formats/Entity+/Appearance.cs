using System.IO;

namespace GameX.WbB.Formats.Entity
{
    public class Appearance
    {
        public readonly uint Eyes;
        public readonly uint Nose;
        public readonly uint Mouth;
        public readonly uint HairColor;
        public readonly uint EyeColor;
        public readonly uint HairStyle;
        public readonly uint HeadgearStyle;
        public readonly uint HeadgearColor;
        public readonly uint ShirtStyle;
        public readonly uint ShirtColor;
        public readonly uint PantsStyle;
        public readonly uint PantsColor;
        public readonly uint FootwearStyle;
        public readonly uint FootwearColor;
        public readonly double SkinHue;
        public readonly double HairHue;
        public readonly double HeadgearHue;
        public readonly double ShirtHue;
        public readonly double PantsHue;
        public readonly double FootwearHue;

        public Appearance() { }
        public Appearance(BinaryReader r)
        {
            Eyes = r.ReadUInt32();
            Nose = r.ReadUInt32();
            Mouth = r.ReadUInt32();
            HairColor = r.ReadUInt32();
            EyeColor = r.ReadUInt32();
            HairStyle = r.ReadUInt32();
            HeadgearStyle = r.ReadUInt32();
            HeadgearColor = r.ReadUInt32();
            ShirtStyle = r.ReadUInt32();
            ShirtColor = r.ReadUInt32();
            PantsStyle = r.ReadUInt32();
            PantsColor = r.ReadUInt32();
            FootwearStyle = r.ReadUInt32();
            FootwearColor = r.ReadUInt32();
            SkinHue = r.ReadDouble();
            HairHue = r.ReadDouble();
            HeadgearHue = r.ReadDouble();
            ShirtHue = r.ReadDouble();
            PantsHue = r.ReadDouble();
            FootwearHue = r.ReadDouble();
        }
    }
}
