namespace System.NumericsX.OpenAL.Extensions.Creative.EFX.Ranges
{
    // Source parameter value ranges and defaults.
    public class SourceRanges
    {
        public const float MinAirAbsorptionFactor = 0f;
        public const float MaxAirAbsorptionFactor = 10f;
        public const float DefaultAirAbsorptionFactor = 0f;

        public const float MinRoomRolloffFactor = 0f;
        public const float MaxRoomRolloffFactor = 10f;
        public const float DefaultRoomRolloffFactor = 0f;

        public const float MinConeOuterGainHF = 0f;
        public const float MaxConeOuterGainHF = 1f;
        public const float DefaultConeOuterGainHF = 1f;

        public const int MinDirectFilterGainHFAuto = 0; // AL_FALSE;
        public const int MaxDirectFilterGainHFAuto = 1; // AL_TRUE;
        public const int DefaultDirectFilterGainHFAuto = 1; // AL_TRUE;

        public const int MinAuxiliarySendFilterGainAuto = 0; // AL_FALSE;
        public const int MaxAuxiliarySendFilterGainAuto = 1; // AL_TRUE;
        public const int DefaultAuxiliarySendFilterGainAuto = 1; // AL_TRUE;

        public const int MinAuxiliarySendFilterGainHFAuto = 0; // AL_FALSE;
        public const int MaxAuxiliarySendFilterGainHFAuto = 1; // AL_TRUE;
        public const int DefaultAuxiliarySendFilterGainHFAuto = 1; // AL_TRUE;
    }
}
