#define EFX_VERBOSE
using System.Collections.Generic;
using System.NumericsX.OpenAL;
using System.NumericsX.OpenAL.Extensions.Creative.EFX;
using System.NumericsX.OpenAL.Extensions.Creative.EFX.Ranges;
using static System.NumericsX.OpenStack.Gngine.Sound.Lib;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.Sound
{
    class SoundEffect
    {
        public string name;
        public int effect;

        public SoundEffect() => effect = 0;
        public void Dispose()
        {
            if (soundSystemLocal.alIsEffect(effect)) soundSystemLocal.alDeleteEffects(1, ref effect);
        }

        public bool Alloc()
        {
            AL.GetError();

            soundSystemLocal.alGenEffects(1, ref effect);
            var e = AL.GetError();
            if (e != ALError.NoError) { common.Warning($"SoundEffect::alloc: alGenEffects failed: 0x{e}"); return false; }

            soundSystemLocal.alEffecti(effect, EffectInteger.EffectType, (int)EffectType.EaxReverb);
            e = AL.GetError();
            if (e != ALError.NoError) { common.Warning($"SoundEffect::alloc: alEffecti failed: 0x{e}"); return false; }
            return true;
        }
    }

    public class EFXFile
    {
        List<SoundEffect> effects = new();

        public EFXFile() => throw new NotImplementedException();
        public void Dispose() => Clear();

        public bool FindEffect(string name, out int effect)
        {
            for (var i = 0; i < effects.Count; i++) if (effects[i].name == name) { effect = effects[i].effect; return true; }
            effect = default;
            return false;
        }

        public bool LoadFile(string filename, bool OSPath = false)
        {
            var src = new Lexer(LEXFL.NOSTRINGCONCAT);

            src.LoadFile(filename, OSPath);
            if (!src.IsLoaded) return false;
            if (!src.ExpectTokenString("Version")) return false;
            if (src.ParseInt() != 1) { src.Error("EFXFile::LoadFile: Unknown file version"); return false; }

            while (!src.EndOfFile())
            {
                var effect = new SoundEffect();
                if (!effect.Alloc()) { Clear(); return false; }
                if (ReadEffect(src, effect)) effects.Add(effect);
            }

            return true;
        }

        public void Clear() => effects.Clear();

        bool ReadEffect(Lexer src, SoundEffect effect)
        {
            static float mb2gain(float millibels, float min, float max)
                => MathX.ClampFloat(min, max, MathX.Pow(10f, millibels / 2000f));

            void efxi(string paramName, EffectInteger param, int v)
            {
                do
                {
                    EFXprintf($"alEffecti({paramName}, {v})\n");
                    soundSystemLocal.alEffecti(effect.effect, param, v);
                    var err = AL.GetError();
                    if (err != ALError.NoError) common.Warning($"alEffecti({paramName}, {v}) failed: 0x{err}");
                } while (false);
            }

            void efxf(string paramName, EffectFloat param, float v)
            {
                do
                {
                    EFXprintf($"alEffectf({paramName}, {v:.3})\n");
                    soundSystemLocal.alEffectf(effect.effect, param, v);
                    var err = AL.GetError();
                    if (err != ALError.NoError) common.Warning($"alEffectf({paramName}, {v:.3}) failed: 0x{err}");
                } while (false);
            }

            void efxfv(string paramName, EffectVector3 param, float value0, float value1, float value2)
            {
                do
                {
                    var v = new[] { value0, value1, value2 };
                    EFXprintf($"alEffectfv({paramName}, {v[0]:.3}, {v[1]:.3}, {v[2]:.3})\n");
                    soundSystemLocal.alEffectfv(effect.effect, param, v);
                    var err = AL.GetError();
                    if (err != ALError.NoError) common.Warning($"alEffectfv({paramName}, {v[0]:.3}, {v[1]:.3}, {v[2]:.3}) failed: 0x{err}");
                } while (false);
            }

            if (!src.ReadToken(out var token)) return false;
            // reverb effect - other effect (not supported at the moment)
            if (token != "reverb") { src.Error("EFXFile::ReadEffect: Unknown effect definition"); return false; }
            src.ReadTokenOnLine(out token);
            var name = token;
            if (!src.ReadToken(out token)) return false;
            if (token != "{") { src.Error($"EFXFile::ReadEffect: {{ not found, found {token}"); return false; }

            AL.GetError();
            EFXprintf($"Loading EFX effect '{name}' (#{effect.effect})\n");

            do
            {
                if (!src.ReadToken(out token)) { src.Error("EFXFile::ReadEffect: EOF without closing brace"); return false; }
                if (token == "}") { effect.name = name; break; }
                if (token == "environment") src.ParseInt(); // the "environment" token should be ignored (efx has nothing equatable to it)
                else if (token == "environment size") { var size = src.ParseFloat(); efxf(nameof(EffectFloat.EaxReverbDensity), EffectFloat.EaxReverbDensity, size < 2f ? size - 1f : 1f); }
                else if (token == "environment diffusion") efxf(nameof(EffectFloat.EaxReverbDiffusion), EffectFloat.EaxReverbDiffusion, src.ParseFloat());
                else if (token == "room") efxf(nameof(EffectFloat.EaxReverbGain), EffectFloat.EaxReverbGain, mb2gain(src.ParseInt(), EffectRanges.EaxReverbMinGain, EffectRanges.EaxReverbMaxGain));
                else if (token == "room hf") efxf(nameof(EffectFloat.EaxReverbGainHF), EffectFloat.EaxReverbGainHF, mb2gain(src.ParseInt(), EffectRanges.EaxReverbMinGainHF, EffectRanges.EaxReverbMaxGainHF));
                else if (token == "room lf") efxf(nameof(EffectFloat.EaxReverbGainLF), EffectFloat.EaxReverbGainLF, mb2gain(src.ParseInt(), EffectRanges.EaxReverbMinGainLF, EffectRanges.EaxReverbMaxGainLF));
                else if (token == "decay time") efxf(nameof(EffectFloat.EaxReverbDecayTime), EffectFloat.EaxReverbDecayTime, src.ParseFloat());
                else if (token == "decay hf ratio") efxf(nameof(EffectFloat.EaxReverbDecayHFRatio), EffectFloat.EaxReverbDecayHFRatio, src.ParseFloat());
                else if (token == "decay lf ratio") efxf(nameof(EffectFloat.EaxReverbDecayLFRatio), EffectFloat.EaxReverbDecayLFRatio, src.ParseFloat());
                else if (token == "reflections") efxf(nameof(EffectFloat.EaxReverbReflectionsGain), EffectFloat.EaxReverbReflectionsGain, mb2gain(src.ParseInt(), EffectRanges.EaxReverbMinReflectionsGain, EffectRanges.EaxReverbMaxReflectionsGain));
                else if (token == "reflections delay") efxf(nameof(EffectFloat.EaxReverbReflectionsDelay), EffectFloat.EaxReverbReflectionsDelay, src.ParseFloat());
                else if (token == "reflections pan") efxfv(nameof(EffectVector3.EaxReverbReflectionsPan), EffectVector3.EaxReverbReflectionsPan, src.ParseFloat(), src.ParseFloat(), src.ParseFloat());
                else if (token == "reverb") efxf(nameof(EffectFloat.EaxReverbLateReverbGain), EffectFloat.EaxReverbLateReverbGain, mb2gain(src.ParseInt(), EffectRanges.EaxReverbMinLateReverbGain, EffectRanges.EaxReverbMaxLateReverbGain));
                else if (token == "reverb delay") efxf(nameof(EffectFloat.EaxReverbLateReverbDelay), EffectFloat.EaxReverbLateReverbDelay, src.ParseFloat());
                else if (token == "reverb pan") efxfv(nameof(EffectVector3.EaxReverbLateReverbPan), EffectVector3.EaxReverbLateReverbPan, src.ParseFloat(), src.ParseFloat(), src.ParseFloat());
                else if (token == "echo time") efxf(nameof(EffectFloat.EaxReverbEchoTime), EffectFloat.EaxReverbEchoTime, src.ParseFloat());
                else if (token == "echo depth") efxf(nameof(EffectFloat.EaxReverbEchoDepth), EffectFloat.EaxReverbEchoDepth, src.ParseFloat());
                else if (token == "modulation time") efxf(nameof(EffectFloat.EaxReverbModulationTime), EffectFloat.EaxReverbModulationTime, src.ParseFloat());
                else if (token == "modulation depth") efxf(nameof(EffectFloat.EaxReverbModulationDepth), EffectFloat.EaxReverbModulationDepth, src.ParseFloat());
                else if (token == "air absorption hf") efxf(nameof(EffectFloat.EaxReverbAirAbsorptionGainHF), EffectFloat.EaxReverbAirAbsorptionGainHF, mb2gain(src.ParseFloat(), EffectRanges.EaxReverbMinAirAbsorptionGainHF, EffectRanges.EaxReverbMaxAirAbsorptionGainHF));
                else if (token == "hf reference") efxf(nameof(EffectFloat.EaxReverbHFReference), EffectFloat.EaxReverbHFReference, src.ParseFloat());
                else if (token == "lf reference") efxf(nameof(EffectFloat.EaxReverbLFReference), EffectFloat.EaxReverbLFReference, src.ParseFloat());
                else if (token == "room rolloff factor") efxf(nameof(EffectFloat.EaxReverbRoomRolloffFactor), EffectFloat.EaxReverbRoomRolloffFactor, src.ParseFloat());
                else if (token == "flags") { src.ReadTokenOnLine(out token); var flags = token.UnsignedIntValue; efxi(nameof(EffectInteger.EaxReverbDecayHFLimit), EffectInteger.EaxReverbDecayHFLimit, (flags & 0x20) != 0 ? 1 : 0); } // the other SCALE flags have no equivalent in efx
                else { src.ReadTokenOnLine(out _); src.Error("EFXFile::ReadEffect: Invalid parameter in reverb definition"); }
            } while (true);

            return true;
        }

#if EFX_VERBOSE
        public static void EFXprintf(string fmt, params object[] args)
            => common.Printf(fmt, args);
#else
		public static void EFXprintf(string fmt, params object[] args) { }
#endif
    }
}
