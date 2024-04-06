namespace System.NumericsX.OpenAL.Extensions.Creative.EFX.Ranges
{
    // Effect parameter ranges and defaults.
    public class EffectRanges
    {
        // Standard reverb effect
        public const float ReverbMinDensity = 0f;
        public const float ReverbMaxDensity = 1f;
        public const float ReverbDefaultDensity = 1f;

        public const float ReverbMinDiffusion = 0f;
        public const float ReverbMaxDiffusion = 1f;
        public const float ReverbDefaultDiffusion = 1f;

        public const float ReverbMinGain = 0f;
        public const float ReverbMaxGain = 1f;
        public const float ReverbDefaultGain = 0.32f;

        public const float ReverbMinGainHF = 0f;
        public const float ReverbMaxGainHF = 1f;
        public const float ReverbDefaultGainHF = 0.89f;

        public const float ReverbMinDecayTime = 0.1f;
        public const float ReverbMaxDecayTime = 20f;
        public const float ReverbDefaultDecayTime = 1.49f;

        public const float ReverbMinDecayHFRatio = 0.1f;
        public const float ReverbMaxDecayHFRatio = 2f;
        public const float ReverbDefaultDecayHFRatio = 0.83f;

        public const float ReverbMinReflectionsGain = 0f;
        public const float ReverbMaxReflectionsGain = 3.16f;
        public const float ReverbDefaultReflectionsGain = 0.05f;

        public const float ReverbMinReflectionsDelay = 0f;
        public const float ReverbMaxReflectionsDelay = 0.3f;
        public const float ReverbDefaultReflectionsDelay = 0.007f;

        public const float ReverbMinLateReverbGain = 0f;
        public const float ReverbMaxLateReverbGain = 10f;
        public const float ReverbDefaultLateReverbGain = 1.26f;

        public const float ReverbMinLateReverbDelay = 0f;
        public const float ReverbMaxLateReverbDelay = 0.1f;
        public const float ReverbDefaultLateReverbDelay = 0.011f;

        public const float ReverbMinAirAbsorptionGainHF = 0.892f;
        public const float ReverbMaxAirAbsorptionGainHF = 1f;
        public const float ReverbDefaultAirAbsorptionGainHF = 0.994f;

        public const float ReverbMinRoomRolloffFactor = 0f;
        public const float ReverbMaxRoomRolloffFactor = 10f;
        public const float ReverbDefaultRoomRolloffFactor = 0f;

        public const int ReverbMinDecayHFLimit = 0; // AL_FALSE
        public const int ReverbMaxDecayHFLimit = 1; // AL_TRUE
        public const int ReverbDefaultDecayHFLimit = 1; // AL_TRUE

        // EAX reverb effect
        public const float EaxReverbMinDensity = 0f;
        public const float EaxReverbMaxDensity = 1f;
        public const float EaxReverbDefaultDensity = 1f;

        public const float EaxReverbMinDiffusion = 0f;
        public const float EaxReverbMaxDiffusion = 1f;
        public const float EaxReverbDefaultDiffusion = 1f;

        public const float EaxReverbMinGain = 0f;
        public const float EaxReverbMaxGain = 1f;
        public const float EaxReverbDefaultGain = 0.32f;

        public const float EaxReverbMinGainHF = 0f;
        public const float EaxReverbMaxGainHF = 1f;
        public const float EaxReverbDefaultGainHF = 0.89f;

        public const float EaxReverbMinGainLF = 0f;
        public const float EaxReverbMaxGainLF = 1f;
        public const float EaxReverbDefaultGainLF = 1f;

        public const float EaxReverbMinDecayTime = 0.1f;
        public const float EaxReverbMaxDecayTime = 20f;
        public const float EaxReverbDefaultDecayTime = 1.49f;

        public const float EaxReverbMinDecayHFRatio = 0.1f;
        public const float EaxReverbMaxDecayHFRatio = 2f;
        public const float EaxReverbDefaultDecayHFRatio = 0.83f;

        public const float EaxReverbMinDecayLFRatio = 0.1f;
        public const float EaxReverbMaxDecayLFRatio = 2f;
        public const float EaxReverbDefaultDecayLFRatio = 1f;

        public const float EaxReverbMinReflectionsGain = 0f;
        public const float EaxReverbMaxReflectionsGain = 3.16f;
        public const float EaxReverbDefaultReflectionsGain = 0.05f;

        public const float EaxReverbMinReflectionsDelay = 0f;
        public const float EaxReverbMaxReflectionsDelay = 0.3f;
        public const float EaxReverbDefaultReflectionsDelay = 0.007f;

        public const float EaxReverbDefaultReflectionsPanXYZ = 0f;

        public const float EaxReverbMinLateReverbGain = 0f;
        public const float EaxReverbMaxLateReverbGain = 10f;
        public const float EaxReverbDefaultLateReverbGain = 1.26f;

        public const float EaxReverbMinLateReverbDelay = 0f;
        public const float EaxReverbMaxLateReverbDelay = 0.1f;
        public const float EaxReverbDefaultLateReverbDelay = 0.011f;

        public const float EaxReverbDefaultLateReverbPanXYZ = 0f;

        public const float EaxReverbMinEchoTime = 0.075f;
        public const float EaxReverbMaxEchoTime = 0.25f;
        public const float EaxReverbDefaultEchoTime = 0.25f;

        public const float EaxReverbMinEchoDepth = 0f;
        public const float EaxReverbMaxEchoDepth = 1f;
        public const float EaxReverbDefaultEchoDepth = 0f;

        public const float EaxReverbMinModulationTime = 0.04f;
        public const float EaxReverbMaxModulationTime = 4f;
        public const float EaxReverbDefaultModulationTime = 0.25f;

        public const float EaxReverbMinModulationDepth = 0f;
        public const float EaxReverbMaxModulationDepth = 1f;
        public const float EaxReverbDefaultModulationDepth = 0f;

        public const float EaxReverbMinAirAbsorptionGainHF = 0.892f;
        public const float EaxReverbMaxAirAbsorptionGainHF = 1f;
        public const float EaxReverbDefaultAirAbsorptionGainHF = 0.994f;

        public const float EaxReverbMinHFReference = 1000f;
        public const float EaxReverbMaxHFReference = 20000f;
        public const float EaxReverbDefaultHFReference = 5000f;

        public const float EaxReverbMinLFReference = 20f;
        public const float EaxReverbMaxLFReference = 1000f;
        public const float EaxReverbDefaultLFReference = 250f;

        public const float EaxReverbMinRoomRolloffFactor = 0f;
        public const float EaxReverbMaxRoomRolloffFactor = 10f;
        public const float EaxReverbDefaultRoomRolloffFactor = 0f;

        public const int EaxReverbMinDecayHFLimit = 0; // AL_FALSE
        public const int EaxReverbMaxDecayHFLimit = 1; // AL_TRUE
        public const int EaxReverbDefaultDecayHFLimit = 1; // AL_TRUE

        // Chorus effect
        public const int ChorusWaveform_Sinusoid = 0;
        public const int ChorusWaveform_Triangle = 1;

        public const int ChorusMinWaveform = 0;
        public const int ChorusMaxWaveform = 1;
        public const int ChorusDefaultWaveform = 1;

        public const int ChorusMinPhase = -180;
        public const int ChorusMaxPhase = 180;
        public const int ChorusDefaultPhase = 90;

        public const float ChorusMinRate = 0f;
        public const float ChorusMaxRate = 10f;
        public const float ChorusDefaultRate = 1.1f;

        public const float ChorusMinDepth = 0f;
        public const float ChorusMaxDepth = 1f;
        public const float ChorusDefaultDepth = 0.1f;

        public const float ChorusMinFeedback = -1f;
        public const float ChorusMaxFeedback = 1f;
        public const float ChorusDefaultFeedback = 0.25f;

        public const float ChorusMinDelay = 0f;
        public const float ChorusMaxDelay = 0.016f;
        public const float ChorusDefaultDelay = 0.016f;

        // Distortion effect
        public const float DistortionMinEdge = 0f;
        public const float DistortionMaxEdge = 1f;
        public const float DistortionDefaultEdge = 0.2f;

        public const float DistortionMinGain = 0.01f;
        public const float DistortionMaxGain = 1f;
        public const float DistortionDefaultGain = 0.05f;

        public const float DistortionMinLowpassCutoff = 80f;
        public const float DistortionMaxLowpassCutoff = 24000f;
        public const float DistortionDefaultLowpassCutoff = 8000f;

        public const float DistortionMinEQCenter = 80f;
        public const float DistortionMaxEQCenter = 24000f;
        public const float DistortionDefaultEQCenter = 3600f;

        public const float DistortionMinEQBandwidth = 80f;
        public const float DistortionMaxEQBandwidth = 24000f;
        public const float DistortionDefaultEQBandwidth = 3600f;

        // Echo effect
        public const float EchoMinDelay = 0f;
        public const float EchoMaxDelay = 0.207f;
        public const float EchoDefaultDelay = 0.1f;

        public const float EchoMinLRDelay = 0f;
        public const float EchoMaxLRDelay = 0.404f;
        public const float EchoDefaultLRDelay = 0.1f;

        public const float EchoMinDamping = 0f;
        public const float EchoMaxDamping = 0.99f;
        public const float EchoDefaultDamping = 0.5f;

        public const float EchoMinFeedback = 0f;
        public const float EchoMaxFeedback = 1f;
        public const float EchoDefaultFeedback = 0.5f;

        public const float EchoMinSpread = -1f;
        public const float EchoMaxSpread = 1f;
        public const float EchoDefaultSpread = -1f;

        // Flanger effect
        public const int FlangerWaveform_Sinusoid = 0;
        public const int FlangerWaveform_Triangle = 1;

        public const int FlangerMinWaveform = 0;
        public const int FlangerMaxWaveform = 1;
        public const int FlangerDefaultWaveform = 1;

        public const int FlangerMinPhase = -180;
        public const int FlangerMaxPhase = 180;
        public const int FlangerDefaultPhase = 0;

        public const float FlangerMinRate = 0f;
        public const float FlangerMaxRate = 10f;
        public const float FlangerDefaultRate = 0.27f;

        public const float FlangerMinDepth = 0f;
        public const float FlangerMaxDepth = 1f;
        public const float FlangerDefaultDepth = 1f;

        public const float FlangerMinFeedback = -1f;
        public const float FlangerMaxFeedback = 1f;
        public const float FlangerDefaultFeedback = -0.5f;

        public const float FlangerMinDelay = 0f;
        public const float FlangerMaxDelay = 0.004f;
        public const float FlangerDefaultDelay = 0.002f;

        // Frequency shifter effect
        public const float FrequencyShifterMinFrequency = 0f;
        public const float FrequencyShifterMaxFrequency = 24000f;
        public const float FrequencyShifterDefaultFrequency = 0f;

        public const int FrequencyShifterMinLeftDirection = 0;
        public const int FrequencyShifterMaxLeftDirection = 2;
        public const int FrequencyShifterDefaultLeftDirection = 0;

        public const int FrequencyShifterDirection_Down = 0;
        public const int FrequencyShifterDirection_Up = 1;
        public const int FrequencyShifterDirection_Of = 2;

        public const int FrequencyShifterMinRight_Direction = 0;
        public const int FrequencyShifterMaxRight_Direction = 2;
        public const int FrequencyShifterDefaultRight_Direction = 0;

        // Vocal morpher effect
        public const int VocalMorpherMinPhonemeA = 0;
        public const int VocalMorpherMaxPhonemeA = 29;
        public const int VocalMorpherDefaultPhonemeA = 0;

        public const int VocalMorpherMinPhonemeACoarseTuning = -24;
        public const int VocalMorpherMaxPhonemeACoarseTuning = 24;
        public const int VocalMorpherDefaultPhonemeACoarseTuning = 0;

        public const int VocalMorpherMinPhonemeB = 0;
        public const int VocalMorpherMaxPhonemeB = 29;
        public const int VocalMorpherDefaultPhonemeB = 10;

        public const int VocalMorpherMinPhonemeBCoarseTuning = -24;
        public const int VocalMorpherMaxPhonemeBCoarseTuning = 24;
        public const int VocalMorpherDefaultPhonemeBCoarseTuning = 0;

        public const int VocalMorpherWaveform_Sinusoid = 0;
        public const int VocalMorpherWaveform_Triangle = 1;
        public const int VocalMorpherWaveform_Sawtooth = 2;

        public const int VocalMorpherMinWaveform = 0;
        public const int VocalMorpherMaxWaveform = 2;
        public const int VocalMorpherDefaultWaveform = 0;

        public const float VocalMorpherMinRate = 0f;
        public const float VocalMorpherMaxRate = 10f;
        public const float VocalMorpherDefaultRate = 1.41f;

        // Pitch shifter effect
        public const int PitchShifterMinCoarseTune = -12;
        public const int PitchShifterMaxCoarseTune = 12;
        public const int PitchShifterDefaultCoarseTune = 12;

        public const int PitchShifterMinFineTune = -50;
        public const int PitchShifterMaxFineTune = 50;
        public const int PitchShifterDefaultFineTune = 0;

        // Ring modulator effect
        public const float RingModulatorMinFrequency = 0f;
        public const float RingModulatorMaxFrequency = 8000f;
        public const float RingModulatorDefaultFrequency = 440f;

        public const float RingModulatorMinHighpassCutoff = 0f;
        public const float RingModulatorMaxHighpassCutoff = 24000f;
        public const float RingModulatorDefaultHighpassCutoff = 800f;

        public const int RingModulator_Sinusoid = 0;
        public const int RingModulator_Sawtooth = 1;
        public const int RingModulator_Square = 2;

        public const int RingModulatorMinWaveform = 0;
        public const int RingModulatorMaxWaveform = 2;
        public const int RingModulatorDefaultWaveform = 0;

        // Autowah effect
        public const float AutowahMinAttackTime = 0.0001f;
        public const float AutowahMaxAttackTime = 1f;
        public const float AutowahDefaultAttackTime = 0.06f;

        public const float AutowahMinReleaseTime = 0.0001f;
        public const float AutowahMaxReleaseTime = 1f;
        public const float AutowahDefaultReleaseTime = 0.06f;

        public const float AutowahMinResonance = 2f;
        public const float AutowahMaxResonance = 1000f;
        public const float AutowahDefaultResonance = 1000f;

        public const float AutowahMinPeakGain = 0.00003f;
        public const float AutowahMaxPeakGain = 31621f;
        public const float AutowahDefaultPeakGain = 11.22f;

        // Compressor effect
        public const int CompressorMinOnOff = 0;
        public const int CompressorMaxOnOff = 1;
        public const int CompressorDefaultOnOff = 1;

        // Equalizer effect
        public const float EqualizerMinLowGain = 0.126f;
        public const float EqualizerMaxLowGain = 7.943f;
        public const float EqualizerDefaultLowGain = 1f;

        public const float EqualizerMinLowCutoff = 50f;
        public const float EqualizerMaxLowCutoff = 800f;
        public const float EqualizerDefaultLowCutoff = 200f;

        public const float EqualizerMinMid1Gain = 0.126f;
        public const float EqualizerMaxMid1Gain = 7.943f;
        public const float EqualizerDefaultMid1Gain = 1f;

        public const float EqualizerMinMid1Center = 200f;
        public const float EqualizerMaxMid1Center = 3000f;
        public const float EqualizerDefaultMid1Center = 500f;

        public const float EqualizerMinMid1Width = 0.01f;
        public const float EqualizerMaxMid1Width = 1f;
        public const float EqualizerDefaultMid1Width = 1f;

        public const float EqualizerMinMid2Gain = 0.126f;
        public const float EqualizerMaxMid2Gain = 7.943f;
        public const float EqualizerDefaultMid2Gain = 1f;

        public const float EqualizerMinMid2Center = 1000f;
        public const float EqualizerMaxMid2Center = 8000f;
        public const float EqualizerDefaultMid2Center = 3000f;

        public const float EqualizerMinMid2Width = 0.01f;
        public const float EqualizerMaxMid2Width = 1f;
        public const float EqualizerDefaultMid2Width = 1f;

        public const float EqualizerMinHighGain = 0.126f;
        public const float EqualizerMaxHighGain = 7.943f;
        public const float EqualizerDefaultHighGain = 1f;

        public const float EqualizerMinHighCutoff = 4000f;
        public const float EqualizerMaxHighCutoff = 16000f;
        public const float EqualizerDefaultHighCutoff = 6000f;
    }
}
