using System.IO;

namespace GameX.Bethesda.Formats.Records
{
    public class EFSHRecord : Record
    {
        public class DATAField
        {
            public byte Flags;
            public uint MembraneShader_SourceBlendMode;
            public uint MembraneShader_BlendOperation;
            public uint MembraneShader_ZTestFunction;
            public ColorRef4 FillTextureEffect_Color;
            public float FillTextureEffect_AlphaFadeInTime;
            public float FillTextureEffect_FullAlphaTime;
            public float FillTextureEffect_AlphaFadeOutTime;
            public float FillTextureEffect_PresistentAlphaRatio;
            public float FillTextureEffect_AlphaPulseAmplitude;
            public float FillTextureEffect_AlphaPulseFrequency;
            public float FillTextureEffect_TextureAnimationSpeed_U;
            public float FillTextureEffect_TextureAnimationSpeed_V;
            public float EdgeEffect_FallOff;
            public ColorRef4 EdgeEffect_Color;
            public float EdgeEffect_AlphaFadeInTime;
            public float EdgeEffect_FullAlphaTime;
            public float EdgeEffect_AlphaFadeOutTime;
            public float EdgeEffect_PresistentAlphaRatio;
            public float EdgeEffect_AlphaPulseAmplitude;
            public float EdgeEffect_AlphaPulseFrequency;
            public float FillTextureEffect_FullAlphaRatio;
            public float EdgeEffect_FullAlphaRatio;
            public uint MembraneShader_DestBlendMode;
            public uint ParticleShader_SourceBlendMode;
            public uint ParticleShader_BlendOperation;
            public uint ParticleShader_ZTestFunction;
            public uint ParticleShader_DestBlendMode;
            public float ParticleShader_ParticleBirthRampUpTime;
            public float ParticleShader_FullParticleBirthTime;
            public float ParticleShader_ParticleBirthRampDownTime;
            public float ParticleShader_FullParticleBirthRatio;
            public float ParticleShader_PersistantParticleBirthRatio;
            public float ParticleShader_ParticleLifetime;
            public float ParticleShader_ParticleLifetime_Delta;
            public float ParticleShader_InitialSpeedAlongNormal;
            public float ParticleShader_AccelerationAlongNormal;
            public float ParticleShader_InitialVelocity1;
            public float ParticleShader_InitialVelocity2;
            public float ParticleShader_InitialVelocity3;
            public float ParticleShader_Acceleration1;
            public float ParticleShader_Acceleration2;
            public float ParticleShader_Acceleration3;
            public float ParticleShader_ScaleKey1;
            public float ParticleShader_ScaleKey2;
            public float ParticleShader_ScaleKey1Time;
            public float ParticleShader_ScaleKey2Time;
            public ColorRef4 ColorKey1_Color;
            public ColorRef4 ColorKey2_Color;
            public ColorRef4 ColorKey3_Color;
            public float ColorKey1_ColorAlpha;
            public float ColorKey2_ColorAlpha;
            public float ColorKey3_ColorAlpha;
            public float ColorKey1_ColorKeyTime;
            public float ColorKey2_ColorKeyTime;
            public float ColorKey3_ColorKeyTime;

            public DATAField(BinaryReader r, int dataSize)
            {
                if (dataSize != 224 && dataSize != 96)
                    Flags = 0;
                Flags = r.ReadByte();
                r.Skip(3); // Unused
                MembraneShader_SourceBlendMode = r.ReadUInt32();
                MembraneShader_BlendOperation = r.ReadUInt32();
                MembraneShader_ZTestFunction = r.ReadUInt32();
                FillTextureEffect_Color = r.ReadS2<ColorRef4>(dataSize);
                FillTextureEffect_AlphaFadeInTime = r.ReadSingle();
                FillTextureEffect_FullAlphaTime = r.ReadSingle();
                FillTextureEffect_AlphaFadeOutTime = r.ReadSingle();
                FillTextureEffect_PresistentAlphaRatio = r.ReadSingle();
                FillTextureEffect_AlphaPulseAmplitude = r.ReadSingle();
                FillTextureEffect_AlphaPulseFrequency = r.ReadSingle();
                FillTextureEffect_TextureAnimationSpeed_U = r.ReadSingle();
                FillTextureEffect_TextureAnimationSpeed_V = r.ReadSingle();
                EdgeEffect_FallOff = r.ReadSingle();
                EdgeEffect_Color = r.ReadS2<ColorRef4>(dataSize);
                EdgeEffect_AlphaFadeInTime = r.ReadSingle();
                EdgeEffect_FullAlphaTime = r.ReadSingle();
                EdgeEffect_AlphaFadeOutTime = r.ReadSingle();
                EdgeEffect_PresistentAlphaRatio = r.ReadSingle();
                EdgeEffect_AlphaPulseAmplitude = r.ReadSingle();
                EdgeEffect_AlphaPulseFrequency = r.ReadSingle();
                FillTextureEffect_FullAlphaRatio = r.ReadSingle();
                EdgeEffect_FullAlphaRatio = r.ReadSingle();
                MembraneShader_DestBlendMode = r.ReadUInt32();
                if (dataSize == 96)
                    return;
                ParticleShader_SourceBlendMode = r.ReadUInt32();
                ParticleShader_BlendOperation = r.ReadUInt32();
                ParticleShader_ZTestFunction = r.ReadUInt32();
                ParticleShader_DestBlendMode = r.ReadUInt32();
                ParticleShader_ParticleBirthRampUpTime = r.ReadSingle();
                ParticleShader_FullParticleBirthTime = r.ReadSingle();
                ParticleShader_ParticleBirthRampDownTime = r.ReadSingle();
                ParticleShader_FullParticleBirthRatio = r.ReadSingle();
                ParticleShader_PersistantParticleBirthRatio = r.ReadSingle();
                ParticleShader_ParticleLifetime = r.ReadSingle();
                ParticleShader_ParticleLifetime_Delta = r.ReadSingle();
                ParticleShader_InitialSpeedAlongNormal = r.ReadSingle();
                ParticleShader_AccelerationAlongNormal = r.ReadSingle();
                ParticleShader_InitialVelocity1 = r.ReadSingle();
                ParticleShader_InitialVelocity2 = r.ReadSingle();
                ParticleShader_InitialVelocity3 = r.ReadSingle();
                ParticleShader_Acceleration1 = r.ReadSingle();
                ParticleShader_Acceleration2 = r.ReadSingle();
                ParticleShader_Acceleration3 = r.ReadSingle();
                ParticleShader_ScaleKey1 = r.ReadSingle();
                ParticleShader_ScaleKey2 = r.ReadSingle();
                ParticleShader_ScaleKey1Time = r.ReadSingle();
                ParticleShader_ScaleKey2Time = r.ReadSingle();
                ColorKey1_Color = r.ReadS2<ColorRef4>(dataSize);
                ColorKey2_Color = r.ReadS2<ColorRef4>(dataSize);
                ColorKey3_Color = r.ReadS2<ColorRef4>(dataSize);
                ColorKey1_ColorAlpha = r.ReadSingle();
                ColorKey2_ColorAlpha = r.ReadSingle();
                ColorKey3_ColorAlpha = r.ReadSingle();
                ColorKey1_ColorKeyTime = r.ReadSingle();
                ColorKey2_ColorKeyTime = r.ReadSingle();
                ColorKey3_ColorKeyTime = r.ReadSingle();
            }
        }

        public override string ToString() => $"EFSH: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID
        public FILEField ICON; // Fill Texture
        public FILEField ICO2; // Particle Shader Texture
        public DATAField DATA; // Data

        public override bool CreateField(BinaryReader r, BethesdaFormat format, string type, int dataSize)
        {
            switch (type)
            {
                case "EDID": EDID = r.ReadSTRV(dataSize); return true;
                case "ICON": ICON = r.ReadFILE(dataSize); return true;
                case "ICO2": ICO2 = r.ReadFILE(dataSize); return true;
                case "DATA": DATA = new DATAField(r, dataSize); return true;
                default: return false;
            }
        }
    }
}