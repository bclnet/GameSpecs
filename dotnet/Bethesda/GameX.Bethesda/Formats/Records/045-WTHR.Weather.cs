using System.Collections.Generic;
using System.IO;

namespace GameX.Bethesda.Formats.Records
{
    public class WTHRRecord : Record, IHaveEDID, IHaveMODL
    {
        public struct FNAMField
        {
            public float DayNear;
            public float DayFar;
            public float NightNear;
            public float NightFar;

            public FNAMField(BinaryReader r, int dataSize)
            {
                DayNear = r.ReadSingle();
                DayFar = r.ReadSingle();
                NightNear = r.ReadSingle();
                NightFar = r.ReadSingle();
            }
        }

        public struct HNAMField
        {
            public float EyeAdaptSpeed;
            public float BlurRadius;
            public float BlurPasses;
            public float EmissiveMult;
            public float TargetLUM;
            public float UpperLUMClamp;
            public float BrightScale;
            public float BrightClamp;
            public float LUMRampNoTex;
            public float LUMRampMin;
            public float LUMRampMax;
            public float SunlightDimmer;
            public float GrassDimmer;
            public float TreeDimmer;

            public HNAMField(BinaryReader r, int dataSize)
            {
                EyeAdaptSpeed = r.ReadSingle();
                BlurRadius = r.ReadSingle();
                BlurPasses = r.ReadSingle();
                EmissiveMult = r.ReadSingle();
                TargetLUM = r.ReadSingle();
                UpperLUMClamp = r.ReadSingle();
                BrightScale = r.ReadSingle();
                BrightClamp = r.ReadSingle();
                LUMRampNoTex = r.ReadSingle();
                LUMRampMin = r.ReadSingle();
                LUMRampMax = r.ReadSingle();
                SunlightDimmer = r.ReadSingle();
                GrassDimmer = r.ReadSingle();
                TreeDimmer = r.ReadSingle();
            }
        }

        public struct DATAField
        {
            public byte WindSpeed;
            public byte CloudSpeed_Lower;
            public byte CloudSpeed_Upper;
            public byte TransDelta;
            public byte SunGlare;
            public byte SunDamage;
            public byte Precipitation_BeginFadeIn;
            public byte Precipitation_EndFadeOut;
            public byte ThunderLightning_BeginFadeIn;
            public byte ThunderLightning_EndFadeOut;
            public byte ThunderLightning_Frequency;
            public byte WeatherClassification;
            public ColorRef4 LightningColor;

            public DATAField(BinaryReader r, int dataSize)
            {
                WindSpeed = r.ReadByte();
                CloudSpeed_Lower = r.ReadByte();
                CloudSpeed_Upper = r.ReadByte();
                TransDelta = r.ReadByte();
                SunGlare = r.ReadByte();
                SunDamage = r.ReadByte();
                Precipitation_BeginFadeIn = r.ReadByte();
                Precipitation_EndFadeOut = r.ReadByte();
                ThunderLightning_BeginFadeIn = r.ReadByte();
                ThunderLightning_EndFadeOut = r.ReadByte();
                ThunderLightning_Frequency = r.ReadByte();
                WeatherClassification = r.ReadByte();
                LightningColor = new ColorRef4 { Red = r.ReadByte(), Green = r.ReadByte(), Blue = r.ReadByte(), Null = 255 };
            }
        }

        public struct SNAMField
        {
            public FormId<SOUNRecord> Sound; // Sound FormId
            public uint Type; // Sound Type - 0=Default, 1=Precipitation, 2=Wind, 3=Thunder

            public SNAMField(BinaryReader r, int dataSize)
            {
                Sound = new FormId<SOUNRecord>(r.ReadUInt32());
                Type = r.ReadUInt32();
            }
        }

        public override string ToString() => $"WTHR: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID
        public MODLGroup MODL { get; set; } // Model
        public FILEField CNAM; // Lower Cloud Layer
        public FILEField DNAM; // Upper Cloud Layer
        public BYTVField NAM0; // Colors by Types/Times
        public FNAMField FNAM; // Fog Distance
        public HNAMField HNAM; // HDR Data
        public DATAField DATA; // Weather Data
        public List<SNAMField> SNAMs = new List<SNAMField>(); // Sounds

        public override bool CreateField(BinaryReader r, BethesdaFormat format, string type, int dataSize)
        {
            switch (type)
            {
                case "EDID": EDID = r.ReadSTRV(dataSize); return true;
                case "MODL": MODL = new MODLGroup(r, dataSize); return true;
                case "MODB": MODL.MODBField(r, dataSize); return true;
                case "CNAM": CNAM = r.ReadFILE(dataSize); return true;
                case "DNAM": DNAM = r.ReadFILE(dataSize); return true;
                case "NAM0": NAM0 = r.ReadBYTV(dataSize); return true;
                case "FNAM": FNAM = new FNAMField(r, dataSize); return true;
                case "HNAM": HNAM = new HNAMField(r, dataSize); return true;
                case "DATA": DATA = new DATAField(r, dataSize); return true;
                case "SNAM": SNAMs.Add(new SNAMField(r, dataSize)); return true;
                default: return false;
            }
        }
    }
}