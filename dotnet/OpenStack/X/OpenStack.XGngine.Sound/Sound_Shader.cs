using System.IO;
using System.NumericsX.OpenStack.Gngine.Framework;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.Gngine.Sound.Lib;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.Sound
{
    // it is somewhat tempting to make this a virtual class to hide the private details here, but that doesn't fit easily with the decl manager at the moment.
    public class SoundShader : Decl, ISoundShader
    {
        // options from sound shader text
        internal SoundShaderParms parms;                       // can be overriden on a per-channel basis

        bool onDemand;                  // only load when played, and free when finished
        internal int speakerMask;
        SoundShader altSound;
        string desc;                     // description
        bool errorDuringParse;
        internal float leadinVolume;             // allows light breaking leadin sounds to be much louder than the broken loop

        internal SoundSample[] leadins = new SoundSample[ISoundSystem.SOUND_MAX_LIST_WAVS];
        internal int numLeadins;
        internal SoundSample[] entries = new SoundSample[ISoundSystem.SOUND_MAX_LIST_WAVS];
        internal int numEntries;

        public SoundShader()
            => Init();

        public override int Size => 0;

        public override bool SetDefaultText()
        {
            var wavname = Name;
            if (Path.GetExtension(wavname).Length == 0) wavname = $"{wavname}.wav"; // if the name has .ogg in it, that will stay
            // if there exists a wav file with the same name //if (fileSystem.ReadFile(wavname, null) == -1) return false;
            Text = $"sound {Name} // IMPLICITLY GENERATED\n{{\n{wavname}\n}}\n";
            return true;
        }

        public override string DefaultDefinition =>
@"{
    _default.wav
}";

        public override bool Parse(string text)
        {
            Lexer src = new();
            src.LoadMemory(text, FileName, LineNum);
            src.Flags = DeclBase.DECL_LEXER_FLAGS;
            src.SkipUntilString("{");
            // deeper functions can set this, which will cause MakeDefault() to be called at the end
            errorDuringParse = false;
            if (!ParseShader(src) || errorDuringParse) { MakeDefault(); return false; }
            return true;
        }

        public override void FreeData()
        {
            numEntries = 0;
            numLeadins = 0;
        }

        public override void List()
        {
            common.Printf($"{Index:4}: {Name}\n");
            if (!string.Equals(Description, "<no description>", StringComparison.OrdinalIgnoreCase)) common.Printf($"      description: {Description}\n");
            for (var k = 0; k < numLeadins; k++)
            {
                var objectp = leadins[k];
                if (objectp != null) common.Printf($"      {soundSystemLocal.SamplesToMilliseconds(objectp.LengthIn44kHzSamples):5}ms {objectp.objectMemSize / 1024:4}Kb {objectp.name} (LEADIN)\n");
            }
            for (var k = 0; k < numEntries; k++)
            {
                var objectp = entries[k];
                if (objectp != null) common.Printf($"      {soundSystemLocal.SamplesToMilliseconds(objectp.LengthIn44kHzSamples):5}ms {objectp.objectMemSize / 1024:4}Kb {objectp.name}\n");
            }
        }

        public virtual string Description => desc;
        // so the editor can draw correct default sound spheres this is currently defined as meters, which sucks, IMHO.
        public virtual float MinDistance => parms.minDistance;       // FIXME: replace this with a GetSoundShaderParms()
        public virtual float MaxDistance => parms.maxDistance;
        // returns null if an AltSound isn't defined in the shader.
        // we use this for pairing a specific broken light sound with a normal light sound
        public virtual SoundShader AltSound => altSound;

        public virtual bool HasDefaultSound
        {
            get
            {
                for (var i = 0; i < numEntries; i++) if (entries[i] != null && entries[i].defaultSound) return true;
                return false;
            }
        }

        public virtual SoundShaderParms Parms => parms;
        public virtual int NumSounds => numLeadins + numEntries;
        public virtual string GetSound(int index)
        {
            if (index >= 0)
            {
                if (index < numLeadins) return leadins[index].name;
                index -= numLeadins;
                if (index < numEntries) return entries[index].name;
            }
            return "";
        }

        public virtual bool CheckShakesAndOgg()
        {
            int i; var ret = false;

            for (i = 0; i < numLeadins; i++) if (leadins[i].objectInfo.wFormatTag == WAVE_FORMAT_TAG.OGG) { common.Warning($"sound shader '{Name}' has shakes and uses OGG file '{leadins[i].name}'"); ret = true; }
            for (i = 0; i < numEntries; i++) if (entries[i].objectInfo.wFormatTag == WAVE_FORMAT_TAG.OGG) { common.Warning($"sound shader '{Name}' has shakes and uses OGG file '{entries[i].name}'"); ret = true; }
            return ret;
        }

        void Init()
        {
            desc = "<no description>";
            errorDuringParse = false;
            onDemand = false;
            numEntries = 0;
            numLeadins = 0;
            leadinVolume = 0;
            altSound = null;
        }

        bool ParseShader(Lexer src)
        {
            int i;

            parms.minDistance = 1;
            parms.maxDistance = 10;
            parms.volume = 1;
            parms.shakes = 0;
            parms.soundShaderFlags = 0;
            parms.soundClass = 0;

            speakerMask = 0;
            altSound = null;

            for (i = 0; i < ISoundSystem.SOUND_MAX_LIST_WAVS; i++) { leadins[i] = null; entries[i] = null; }
            numEntries = 0;
            numLeadins = 0;

            var maxSamples = SoundSystemLocal.s_maxSoundsPerShader.Integer;
            if (C.com_makingBuild.Bool || maxSamples <= 0 || maxSamples > ISoundSystem.SOUND_MAX_LIST_WAVS) maxSamples = ISoundSystem.SOUND_MAX_LIST_WAVS;

            string tokenS;
            while (true)
            {
                if (!src.ExpectAnyToken(out var token)) return false;
                // end of definition
                else if (token == "}") break;
                // minimum number of sounds
                else if (string.Equals(token, "minSamples", StringComparison.OrdinalIgnoreCase)) maxSamples = MathX.ClampInt(src.ParseInt(), ISoundSystem.SOUND_MAX_LIST_WAVS, maxSamples);
                // description
                else if (string.Equals(token, "description", StringComparison.OrdinalIgnoreCase)) { src.ReadTokenOnLine(out token); desc = token; }
                // mindistance
                else if (string.Equals(token, "mindistance", StringComparison.OrdinalIgnoreCase)) parms.minDistance = src.ParseFloat();
                // maxdistance
                else if (string.Equals(token, "maxdistance", StringComparison.OrdinalIgnoreCase)) parms.maxDistance = src.ParseFloat();
                // shakes screen
                else if (string.Equals(token, "shakes", StringComparison.OrdinalIgnoreCase))
                {
                    src.ExpectAnyToken(out token);
                    if (token.type == TT.NUMBER) parms.shakes = token.FloatValue;
                    else { src.UnreadToken(token); parms.shakes = 1f; }
                }
                // reverb
                else if (string.Equals(token, "reverb"))
                {
                    src.ParseFloat();
                    if (!src.ExpectTokenString(",")) { src.FreeSource(); return false; }
                    src.ParseFloat();
                    // no longer supported
                }
                // volume
                else if (string.Equals(token, "volume", StringComparison.OrdinalIgnoreCase)) parms.volume = src.ParseFloat();
                // leadinVolume is used to allow light breaking leadin sounds to be much louder than the broken loop
                else if (string.Equals(token, "leadinVolume", StringComparison.OrdinalIgnoreCase)) leadinVolume = src.ParseFloat();
                // speaker mask
                else if (string.Equals(token, "mask_center", StringComparison.OrdinalIgnoreCase)) speakerMask |= 1 << (int)SPEAKER.CENTER;
                // speaker mask
                else if (string.Equals(token, "mask_left", StringComparison.OrdinalIgnoreCase)) speakerMask |= 1 << (int)SPEAKER.LEFT;
                // speaker mask
                else if (string.Equals(token, "mask_right", StringComparison.OrdinalIgnoreCase)) speakerMask |= 1 << (int)SPEAKER.RIGHT;
                // speaker mask
                else if (string.Equals(token, "mask_backright", StringComparison.OrdinalIgnoreCase)) speakerMask |= 1 << (int)SPEAKER.BACKRIGHT;
                // speaker mask
                else if (string.Equals(token, "mask_backleft", StringComparison.OrdinalIgnoreCase)) speakerMask |= 1 << (int)SPEAKER.BACKLEFT;
                // speaker mask
                else if (string.Equals(token, "mask_lfe", StringComparison.OrdinalIgnoreCase)) speakerMask |= 1 << (int)SPEAKER.LFE;
                // soundClass
                else if (string.Equals(token, "soundClass", StringComparison.OrdinalIgnoreCase))
                {
                    parms.soundClass = src.ParseInt();
                    if (parms.soundClass < 0 || parms.soundClass >= ISoundSystem.SOUND_MAX_CLASSES) { src.Warning("SoundClass out of range"); return false; }
                }
                // altSound
                else if (string.Equals(token, "altSound", StringComparison.OrdinalIgnoreCase))
                {
                    if (!src.ExpectAnyToken(out token)) return false;
                    altSound = (SoundShader)declManager.FindSound(token);
                }
                // ordered
                else if (string.Equals(token, "ordered", StringComparison.OrdinalIgnoreCase)) { } // no longer supported
                // no_dups
                else if (string.Equals(token, "no_dups", StringComparison.OrdinalIgnoreCase))
                    parms.soundShaderFlags |= ISoundSystem.SSF_NO_DUPS;
                // no_flicker
                else if (string.Equals(token, "no_flicker", StringComparison.OrdinalIgnoreCase))
                    parms.soundShaderFlags |= ISoundSystem.SSF_NO_FLICKER;
                // plain
                else if (string.Equals(token, "plain", StringComparison.OrdinalIgnoreCase)) { } // no longer supported
                // looping
                else if (string.Equals(token, "looping", StringComparison.OrdinalIgnoreCase)) parms.soundShaderFlags |= ISoundSystem.SSF_LOOPING;
                // no occlusion
                else if (string.Equals(token, "no_occlusion", StringComparison.OrdinalIgnoreCase)) parms.soundShaderFlags |= ISoundSystem.SSF_NO_OCCLUSION;
                // private
                else if (string.Equals(token, "private", StringComparison.OrdinalIgnoreCase)) parms.soundShaderFlags |= ISoundSystem.SSF_PRIVATE_SOUND;
                // antiPrivate
                else if (string.Equals(token, "antiPrivate", StringComparison.OrdinalIgnoreCase)) parms.soundShaderFlags |= ISoundSystem.SSF_ANTI_PRIVATE_SOUND;
                // once
                else if (string.Equals(token, "playonce", StringComparison.OrdinalIgnoreCase)) parms.soundShaderFlags |= ISoundSystem.SSF_PLAY_ONCE;
                // global
                else if (string.Equals(token, "global", StringComparison.OrdinalIgnoreCase)) parms.soundShaderFlags |= ISoundSystem.SSF_GLOBAL;
                // unclamped
                else if (string.Equals(token, "unclamped", StringComparison.OrdinalIgnoreCase)) parms.soundShaderFlags |= ISoundSystem.SSF_UNCLAMPED;
                // omnidirectional
                else if (string.Equals(token, "omnidirectional", StringComparison.OrdinalIgnoreCase)) parms.soundShaderFlags |= ISoundSystem.SSF_OMNIDIRECTIONAL;
                // onDemand can't be a parms, because we must track all references and overrides would confuse it
                else if (string.Equals(token, "onDemand", StringComparison.OrdinalIgnoreCase)) { /*onDemand = true;*/ } // no longer loading sounds on demand
                // the wave files
                else if (string.Equals(token, "leadin", StringComparison.OrdinalIgnoreCase))
                {
                    // add to the leadin list
                    if (!src.ReadToken(out token)) { src.Warning("Expected sound after leadin"); return false; }
                    if (soundSystemLocal.soundCache != null && numLeadins < maxSamples)
                    {
                        leadins[numLeadins] = soundSystemLocal.soundCache.FindSound(token, onDemand);
                        numLeadins++;
                    }
                }
                else if ((tokenS = token).Length != 0 && (tokenS.IndexOf(".wav", StringComparison.OrdinalIgnoreCase) != -1 || tokenS.IndexOf(".ogg", StringComparison.OrdinalIgnoreCase) != -1))
                {
                    // add to the wav list
                    if (soundSystemLocal.soundCache != null && numEntries < maxSamples)
                    {
                        var tokenS2 = PathX.BackSlashesToSlashes(tokenS);
                        var lang = cvarSystem.GetCVarString("sys_lang");
                        if (!string.Equals(lang, "english", StringComparison.OrdinalIgnoreCase) && tokenS2.IndexOf("sound/vo/", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            var work = tokenS2.ToLowerInvariant();
                            work = work.StripLeading("sound/vo/");
                            work = $"sound/vo/{lang}/{work}";
                            if (fileSystem.ReadFile(work) > 0) token = work;
                            else
                            {
                                // also try to find it with the .ogg extension
                                work = PathX.SetFileExtension(work, ".ogg");
                                if (fileSystem.ReadFile(work) > 0)
                                    token = work;
                            }
                        }
                        entries[numEntries] = soundSystemLocal.soundCache.FindSound(token, onDemand);
                        numEntries++;
                    }
                }
                else { src.Warning($"unknown token '{token}'"); return false; }
            }

            if (parms.shakes > 0f) CheckShakesAndOgg();
            return true;
        }
    }
}