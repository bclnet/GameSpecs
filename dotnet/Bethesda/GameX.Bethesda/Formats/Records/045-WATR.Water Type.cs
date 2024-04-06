using System.IO;

namespace GameX.Bethesda.Formats.Records
{
    public class WATRRecord : Record
    {
        public class DATAField
        {
            public float WindVelocity;
            public float WindDirection;
            public float WaveAmplitude;
            public float WaveFrequency;
            public float SunPower;
            public float ReflectivityAmount;
            public float FresnelAmount;
            public float ScrollXSpeed;
            public float ScrollYSpeed;
            public float FogDistance_NearPlane;
            public float FogDistance_FarPlane;
            public ColorRef4 ShallowColor;
            public ColorRef4 DeepColor;
            public ColorRef4 ReflectionColor;
            public byte TextureBlend;
            public float RainSimulator_Force;
            public float RainSimulator_Velocity;
            public float RainSimulator_Falloff;
            public float RainSimulator_Dampner;
            public float RainSimulator_StartingSize;
            public float DisplacementSimulator_Force;
            public float DisplacementSimulator_Velocity;
            public float DisplacementSimulator_Falloff;
            public float DisplacementSimulator_Dampner;
            public float DisplacementSimulator_StartingSize;
            public ushort Damage;

            public DATAField(BinaryReader r, int dataSize)
            {
                if (dataSize != 102 && dataSize != 86 && dataSize != 62 && dataSize != 42 && dataSize != 2)
                    WindVelocity = 1;
                if (dataSize == 2)
                {
                    Damage = r.ReadUInt16();
                    return;
                }
                WindVelocity = r.ReadSingle();
                WindDirection = r.ReadSingle();
                WaveAmplitude = r.ReadSingle();
                WaveFrequency = r.ReadSingle();
                SunPower = r.ReadSingle();
                ReflectivityAmount = r.ReadSingle();
                FresnelAmount = r.ReadSingle();
                ScrollXSpeed = r.ReadSingle();
                ScrollYSpeed = r.ReadSingle();
                FogDistance_NearPlane = r.ReadSingle();
                if (dataSize == 42)
                {
                    Damage = r.ReadUInt16();
                    return;
                }
                FogDistance_FarPlane = r.ReadSingle();
                ShallowColor = r.ReadS2<ColorRef4>(dataSize);
                DeepColor = r.ReadS2<ColorRef4>(dataSize);
                ReflectionColor = r.ReadS2<ColorRef4>(dataSize);
                TextureBlend = r.ReadByte();
                r.Skip(3); // Unused
                if (dataSize == 62)
                {
                    Damage = r.ReadUInt16();
                    return;
                }
                RainSimulator_Force = r.ReadSingle();
                RainSimulator_Velocity = r.ReadSingle();
                RainSimulator_Falloff = r.ReadSingle();
                RainSimulator_Dampner = r.ReadSingle();
                RainSimulator_StartingSize = r.ReadSingle();
                DisplacementSimulator_Force = r.ReadSingle();
                if (dataSize == 86)
                {
                    //DisplacementSimulator_Velocity = DisplacementSimulator_Falloff = DisplacementSimulator_Dampner = DisplacementSimulator_StartingSize = 0F;
                    Damage = r.ReadUInt16();
                    return;
                }
                DisplacementSimulator_Velocity = r.ReadSingle();
                DisplacementSimulator_Falloff = r.ReadSingle();
                DisplacementSimulator_Dampner = r.ReadSingle();
                DisplacementSimulator_StartingSize = r.ReadSingle();
                Damage = r.ReadUInt16();
            }
        }

        public struct GNAMField
        {
            public FormId<WATRRecord> Daytime;
            public FormId<WATRRecord> Nighttime;
            public FormId<WATRRecord> Underwater;

            public GNAMField(BinaryReader r, int dataSize)
            {
                Daytime = new FormId<WATRRecord>(r.ReadUInt32());
                Nighttime = new FormId<WATRRecord>(r.ReadUInt32());
                Underwater = new FormId<WATRRecord>(r.ReadUInt32());
            }
        }

        public override string ToString() => $"WATR: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID
        public STRVField TNAM; // Texture
        public BYTEField ANAM; // Opacity
        public BYTEField FNAM; // Flags
        public STRVField MNAM; // Material ID
        public FMIDField<SOUNRecord> SNAM; // Sound
        public DATAField DATA; // DATA
        public GNAMField GNAM; // GNAM

        public override bool CreateField(BinaryReader r, BethesdaFormat format, string type, int dataSize)
        {
            switch (type)
            {
                case "EDID": EDID = r.ReadSTRV(dataSize); return true;
                case "TNAM": TNAM = r.ReadSTRV(dataSize); return true;
                case "ANAM": ANAM = r.ReadS2<BYTEField>(dataSize); return true;
                case "FNAM": FNAM = r.ReadS2<BYTEField>(dataSize); return true;
                case "MNAM": MNAM = r.ReadSTRV(dataSize); return true;
                case "SNAM": SNAM = new FMIDField<SOUNRecord>(r, dataSize); return true;
                case "DATA": DATA = new DATAField(r, dataSize); return true;
                case "GNAM": GNAM = new GNAMField(r, dataSize); return true;
                default: return false;
            }
        }
    }
}