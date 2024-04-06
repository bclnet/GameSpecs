using System.Collections.Generic;
using System.Diagnostics;
using System.NumericsX.OpenAL;
using System.NumericsX.OpenAL.Extensions.Creative.EFX;
using System.NumericsX.OpenStack.Gngine.Framework;
using System.NumericsX.OpenStack.Gngine.Render;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.Gngine.Sound.Lib;
using static System.NumericsX.OpenStack.OpenStack;
using static System.NumericsX.Platform;

namespace System.NumericsX.OpenStack.Gngine.Sound
{
    // demo sound commands
    public enum SCMD : int
    {
        STATE,             // followed by a load game state
        PLACE_LISTENER,
        ALLOC_EMITTER,
        FREE,
        UPDATE,
        START,
        MODIFY,
        STOP,
        FADE
    }

    public class SoundStats
    {
        public int rinuse;
        public int runs = 1;
        public int timeinprocess;
        public int missedWindow;
        public int missedUpdateWindow;
        public int activeSounds;
    }

    public class SoundPortalTrace
    {
        public int portalArea;
        public SoundPortalTrace prevStack;
    }

    public class SoundWorldLocal : ISoundWorld
    {
        // call at each map start
        public virtual void ClearAllSoundEmitters()
        {
            int i;

            ISystem.EnterCriticalSection();

            AVIClose();

            for (i = 0; i < emitters.Count; i++)
            {
                var sound = emitters[i];
                sound.Clear();
            }
            localSound = null;

            ISystem.LeaveCriticalSection();
        }

        // this is called from the main thread
        public virtual void StopAllSounds()
        {
            for (var i = 0; i < emitters.Count; i++) emitters[i].StopSound(ISoundSystem.SCHANNEL_ANY);
        }

        // get a new emitter that can play sounds in this world
        // this is called from the main thread
        public virtual ISoundEmitter AllocSoundEmitter()
        {
            var emitter = AllocLocalSoundEmitter();

            if (SoundSystemLocal.s_showStartSound.Integer != 0) common.Printf($"AllocSoundEmitter = {emitter.index}\n");
            if (writeDemo != null)
            {
                writeDemo.WriteInt((int)VFileDemo.DS.SOUND);
                writeDemo.WriteInt((int)SCMD.ALLOC_EMITTER);
                writeDemo.WriteInt(emitter.index);
            }

            return emitter;
        }

        // for load games
        public virtual ISoundEmitter EmitterForIndex(int index)
        {
            if (index == 0) return null;
            if (index >= emitters.Count) common.Error($"SoundWorldLocal::EmitterForIndex: {index} > {emitters.Count}");
            return emitters[index];
        }

        // query data from all emitters in the world
        // this is called from the main thread
        public virtual float CurrentShakeAmplitudeForPosition(int time, Vector3? listererPosition)
        {
            var amp = 0f; int localTime;

            if (SoundSystemLocal.s_constantAmplitude.Float >= 0f) return 0f;

            localTime = soundSystemLocal.Current44kHzTime;

            for (var i = 1; i < emitters.Count; i++)
            {
                var sound = emitters[i];
                if (!sound.hasShakes) continue;
                amp += FindAmplitude(sound, localTime, listererPosition, ISoundSystem.SCHANNEL_ANY, true);
            }
            return amp;
        }

        // where is the camera/microphone listenerId allows listener-private sounds to be added
        // this is called by the main thread
        public virtual void PlaceListener(Vector3 origin, Matrix3x3 axis, int listenerId, int gameTime, string areaName)
        {
            int current44kHzTime;

            if (!soundSystemLocal.isInitialized) return;

            if (pause44kHz >= 0) return;

            if (writeDemo != null)
            {
                writeDemo.WriteInt((int)VFileDemo.DS.SOUND);
                writeDemo.WriteInt((int)SCMD.PLACE_LISTENER);
                writeDemo.WriteVec3(origin);
                writeDemo.WriteMat3(axis);
                writeDemo.WriteInt(listenerId);
                writeDemo.WriteInt(gameTime);
            }

            current44kHzTime = soundSystemLocal.Current44kHzTime;

            // we usually expect gameTime to be increasing by 16 or 32 msec, but when a cinematic is fast-forward skipped through, it can jump by a significant
            // amount, while the hardware 44kHz position will not have changed accordingly, which would make sounds (like long character speaches) continue from the
            // old time.  Fix this by killing all non-looping sounds
            if (gameTime > gameMsec + 500) OffsetSoundTime((int)(-(gameTime - gameMsec) * 0.001f * 44100f));

            gameMsec = gameTime;
            game44kHz = fpa[0] != null
                ? MathX.FtoiFast(gameMsec * ((1000f / 60f) / 16f) * 0.001f * 44100f) // exactly 30 fps so the wave file can be used for exact video frames
                : MathX.FtoiFast(gameMsec * 0.001f * 44100f); // the normal 16 msec / frame

            listenerPrivateId = listenerId;

            listenerQU = origin;                            // Doom units
            listenerPos = origin * ISoundSystem.DOOM_TO_METERS;          // meters
            listenerAxis = axis;
            listenerAreaName = areaName.ToLowerInvariant();

            listenerArea = rw != null ? rw.PointInArea(listenerQU) : 0; // where are we?
            if (listenerArea < 0) return;

            ForegroundUpdate(current44kHzTime);
        }

        // fade all sounds in the world with a given shader soundClass to is in Db (sigh), over is in seconds
        public virtual void FadeSoundClasses(int soundClass, float to, float over)
        {
            if (soundClass < 0 || soundClass >= ISoundSystem.SOUND_MAX_CLASSES) common.Error($"SoundWorldLocal::FadeSoundClasses: bad soundClass {soundClass}");

            var fade = soundClassFade[soundClass];

            var length44kHz = soundSystemLocal.MillisecondsToSamples((int)(over * 1000));

            // if it is already fading to this volume at this rate, don't change it
            if (fade.fadeEndVolume == to && fade.fadeEnd44kHz - fade.fadeStart44kHz == length44kHz) return;

            var start44kHz = fpa[0] != null
                ? lastAVI44kHz + Simd.MIXBUFFER_SAMPLES // if we are recording an AVI demo, don't use hardware time
                : soundSystemLocal.Current44kHzTime + Simd.MIXBUFFER_SAMPLES;

            // fade it
            fade.fadeStartVolume = fade.FadeDbAt44kHz(start44kHz);
            fade.fadeStart44kHz = start44kHz;
            fade.fadeEnd44kHz = start44kHz + length44kHz;
            fade.fadeEndVolume = to;
        }

        // dumps the current state and begins archiving commands
        // this is called from the main thread
        public virtual void StartWritingDemo(VFileDemo demo)
        {
            writeDemo = demo;

            writeDemo.WriteInt((int)VFileDemo.DS.SOUND);
            writeDemo.WriteInt((int)SCMD.STATE);

            // use the normal save game code to archive all the emitters
            WriteToSaveGame(writeDemo);
        }

        // this is called from the main thread
        public virtual void StopWritingDemo()
            => writeDemo = null;

        // read a sound command from a demo file
        // this is called from the main thread
        public virtual void ProcessDemoCommand(VFileDemo readDemo)
        {
            int index; SoundEmitterLocal def;

            if (readDemo == null) return;

            if (readDemo.ReadInt(out int dc) == 0) return;

            switch ((SCMD)dc)
            {
                case SCMD.STATE:
                    // we need to protect this from the async thread
                    // other instances of calling idSoundWorldLocal::ReadFromSaveGame do this while the sound code is muted
                    // setting muted and going right in may not be good enough here, as we async thread may already be in an async tick (in which case we could still race to it)
                    ISystem.EnterCriticalSection();
                    ReadFromSaveGame(readDemo);
                    ISystem.LeaveCriticalSection();
                    UnPause();
                    break;
                case SCMD.PLACE_LISTENER:
                    {
                        readDemo.ReadVec3(out var origin);
                        readDemo.ReadMat3(out var axis);
                        readDemo.ReadInt(out var listenerId);
                        readDemo.ReadInt(out var gameTime);
                        PlaceListener(origin, axis, listenerId, gameTime, "");
                    };
                    break;
                case SCMD.ALLOC_EMITTER:
                    readDemo.ReadInt(out index);
                    if (index < 1 || index > emitters.Count) common.Error("SoundWorldLocal::ProcessDemoCommand: bad emitter number");
                    if (index == emitters.Count)
                    {
                        // append a brand new one
                        def = new SoundEmitterLocal();
                        emitters.Add(def);
                    }
                    def = emitters[index];
                    def.Clear();
                    def.index = index;
                    def.removeStatus = REMOVE_STATUS.ALIVE;
                    def.soundWorld = this;
                    break;
                case SCMD.FREE:
                    {
                        readDemo.ReadInt(out index);
                        readDemo.ReadInt(out var immediate);
                        EmitterForIndex(index).Free(immediate != 0);
                    }
                    break;
                case SCMD.UPDATE:
                    {
                        SoundShaderParms parms = new();
                        readDemo.ReadInt(out index);
                        readDemo.ReadVec3(out var origin);
                        readDemo.ReadInt(out var listenerId);
                        readDemo.ReadFloat(out parms.minDistance);
                        readDemo.ReadFloat(out parms.maxDistance);
                        readDemo.ReadFloat(out parms.volume);
                        readDemo.ReadFloat(out parms.shakes);
                        readDemo.ReadInt(out parms.soundShaderFlags);
                        readDemo.ReadInt(out parms.soundClass);
                        EmitterForIndex(index).UpdateEmitter(origin, listenerId, parms);
                    }
                    break;
                case SCMD.START:
                    {
                        readDemo.ReadInt(out index);
                        var shader = declManager.FindSound(readDemo.ReadHashString());
                        readDemo.ReadInt(out var channel);
                        readDemo.ReadFloat(out var diversity);
                        readDemo.ReadInt(out var shaderFlags);
                        EmitterForIndex(index).StartSound(shader, channel, diversity, shaderFlags);
                    }
                    break;
                case SCMD.MODIFY:
                    {
                        SoundShaderParms parms = new();
                        readDemo.ReadInt(out index);
                        readDemo.ReadInt(out var channel);
                        readDemo.ReadFloat(out parms.minDistance);
                        readDemo.ReadFloat(out parms.maxDistance);
                        readDemo.ReadFloat(out parms.volume);
                        readDemo.ReadFloat(out parms.shakes);
                        readDemo.ReadInt(out parms.soundShaderFlags);
                        readDemo.ReadInt(out parms.soundClass);
                        EmitterForIndex(index).ModifySound(channel, parms);
                    }
                    break;
                case SCMD.STOP:
                    {
                        readDemo.ReadInt(out index);
                        readDemo.ReadInt(out var channel);
                        EmitterForIndex(index).StopSound(channel);
                    }
                    break;
                case SCMD.FADE:
                    {
                        readDemo.ReadInt(out index);
                        readDemo.ReadInt(out var channel);
                        readDemo.ReadFloat(out var to);
                        readDemo.ReadFloat(out var over);
                        EmitterForIndex(index).FadeSound(channel, to, over);
                    }
                    break;
            }
        }

        // background music
        // start a music track
        // this is called from the main thread
        static RandomX PlayShaderDirectly_rnd = new();
        public virtual void PlayShaderDirectly(string name, int channel = -1)
        {
            if (localSound != null && channel == -1) localSound.StopSound(ISoundSystem.SCHANNEL_ANY);
            else if (localSound != null) localSound.StopSound(channel);

            if (string.IsNullOrEmpty(name)) return;

            var shader = declManager.FindSound(name);
            if (shader == null) return;

            if (localSound == null) localSound = AllocLocalSoundEmitter();

            var diversity = PlayShaderDirectly_rnd.RandomFloat();

            localSound.StartSound(shader, channel == -1 ? ISoundSystem.SCHANNEL_ONE : channel, diversity, ISoundSystem.SSF_GLOBAL);

            // in case we are at the console without a game doing updates, force an update
            ForegroundUpdate(soundSystemLocal.Current44kHzTime);
        }

        // pause and unpause the sound world
        public virtual void Pause()
        {
            if (pause44kHz >= 0) { common.Warning("SoundWorldLocal::Pause: already paused"); return; }

            pause44kHz = soundSystemLocal.Current44kHzTime;
        }

        public virtual void UnPause()
        {
            if (pause44kHz < 0) { common.Warning("SoundWorldLocal::UnPause: not paused"); return; }

            var offset44kHz = soundSystemLocal.Current44kHzTime - pause44kHz;
            OffsetSoundTime(offset44kHz);

            pause44kHz = -1;
        }

        public virtual bool IsPaused
            => pause44kHz >= 0;

        // avidump
        // this is called by the main thread
        public virtual void AVIOpen(string path, string name)
        {
            aviDemoPath = path;
            aviDemoName = name;

            lastAVI44kHz = game44kHz - game44kHz % Simd.MIXBUFFER_SAMPLES;

            if (SoundSystemLocal.s_numberOfSpeakers.Integer == 6)
            {
                fpa[0] = fileSystem.OpenFileWrite($"{aviDemoPath}channel_51_left.raw");
                fpa[1] = fileSystem.OpenFileWrite($"{aviDemoPath}channel_51_right.raw");
                fpa[2] = fileSystem.OpenFileWrite($"{aviDemoPath}channel_51_center.raw");
                fpa[3] = fileSystem.OpenFileWrite($"{aviDemoPath}channel_51_lfe.raw");
                fpa[4] = fileSystem.OpenFileWrite($"{aviDemoPath}channel_51_backleft.raw");
                fpa[5] = fileSystem.OpenFileWrite($"{aviDemoPath}channel_51_backright.raw");
            }
            else
            {
                fpa[0] = fileSystem.OpenFileWrite($"{aviDemoPath}channel_left.raw");
                fpa[1] = fileSystem.OpenFileWrite($"{aviDemoPath}channel_right.raw");
            }

            soundSystemLocal.SetMute(true);
        }

        public unsafe virtual void AVIClose()
        {
            int i;

            if (fpa[0] == null)
                return;

            // make sure the final block is written
            game44kHz += Simd.MIXBUFFER_SAMPLES;
            AVIUpdate();
            game44kHz -= Simd.MIXBUFFER_SAMPLES;

            for (i = 0; i < 6; i++)
                if (fpa[i] != null)
                {
                    fileSystem.CloseFile(fpa[i]);
                    fpa[i] = null;
                }
            if (SoundSystemLocal.s_numberOfSpeakers.Integer == 2)
            {
                // convert it to a wave file
                VFile rL, lL, wO; string name;

                name = $"{aviDemoPath}{aviDemoName}.wav";
                wO = fileSystem.OpenFileWrite(name);
                if (wO == null) common.Error($"Couldn't write {name}");

                name = $"{aviDemoPath}channel_right.raw";
                rL = fileSystem.OpenFileRead(name);
                if (rL == null) common.Error($"Couldn't open {name}");

                name = $"{aviDemoPath }channel_left.raw";
                lL = fileSystem.OpenFileRead(name);
                if (lL == null) common.Error($"Couldn't open {name}");

                var numSamples = rL.Length / 2;
                Mminfo info;
                PcmWaveFormat format;

                info.ckid = SoundSystemLocal.fourcc_riff;
                info.fccType = SoundSystemLocal.mmioFOURCC('W', 'A', 'V', 'E');
                info.cksize = (uint)((rL.Length * 2) - 8 + 4 + 16 + 8 + 8);
                info.dwDataOffset = 12;

                wO.Write((byte*)&info, 12);

                info.ckid = SoundSystemLocal.mmioFOURCC('f', 'm', 't', ' ');
                info.cksize = 16;

                wO.Write((byte*)&info, 8);

                format.wBitsPerSample = 16;
                format.wf.nAvgBytesPerSec = 44100 * 4;      // sample rate * block align
                format.wf.nChannels = 2;
                format.wf.nSamplesPerSec = 44100;
                format.wf.wFormatTag = WAVE_FORMAT_TAG.PCM;
                format.wf.nBlockAlign = 4;                  // channels * bits/sample / 8

                wO.Write((byte*)&format, 16);

                info.ckid = SoundSystemLocal.mmioFOURCC('d', 'a', 't', 'a');
                info.cksize = (uint)(rL.Length * 2);

                wO.Write((byte*)&info, 8);

                for (i = 0; i < numSamples; i++)
                {
                    lL.Read(out short s0); rL.Read(out short s1);
                    wO.Write(s0); wO.Write(s1);
                }

                fileSystem.CloseFile(wO);
                fileSystem.CloseFile(lL);
                fileSystem.CloseFile(rL);

                fileSystem.RemoveFile($"{aviDemoPath}channel_right.raw");
                fileSystem.RemoveFile($"{aviDemoPath}channel_left.raw");
            }

            soundSystemLocal.SetMute(false);
        }

        // SaveGame Support
        public virtual void WriteToSaveGame(VFile savefile)
        {
            int i, j, num, currentSoundTime;

            // the game soundworld is always paused at this point, save that time down
            currentSoundTime = pause44kHz > 0 ? pause44kHz : soundSystemLocal.Current44kHzTime;

            // write listener data
            savefile.WriteVec3(listenerQU);
            savefile.WriteMat3(listenerAxis);
            savefile.WriteInt(listenerPrivateId);
            savefile.WriteInt(gameMsec);
            savefile.WriteInt(game44kHz);
            savefile.WriteInt(currentSoundTime);

            num = emitters.Count;
            savefile.WriteInt(num);

            for (i = 1; i < emitters.Count; i++)
            {
                var def = emitters[i];

                if (def.removeStatus != REMOVE_STATUS.ALIVE) { var skip = -1; savefile.Write(skip); continue; }

                savefile.WriteInt(i);

                // Write the emitter data
                savefile.WriteVec3(def.origin);
                savefile.WriteInt(def.listenerId);
                WriteToSaveGameSoundShaderParams(savefile, def.parms);
                savefile.WriteFloat(def.amplitude);
                savefile.WriteInt(def.ampTime);
                for (var k = 0; k < SoundSystemLocal.SOUND_MAX_CHANNELS; k++) WriteToSaveGameSoundChannel(savefile, def.channels[k]);
                savefile.WriteFloat(def.distance);
                savefile.WriteBool(def.hasShakes);
                savefile.WriteInt(def.lastValidPortalArea);
                savefile.WriteFloat(def.maxDistance);
                savefile.WriteBool(def.playing);
                savefile.WriteFloat(def.realDistance);
                savefile.WriteInt((int)def.removeStatus);
                savefile.WriteVec3(def.spatializedOrigin);

                // write the channel data
                for (j = 0; j < SoundSystemLocal.SOUND_MAX_CHANNELS; j++)
                {
                    var chan = def.channels[j];

                    // Write out any sound commands for this def
                    if (chan.triggerState && chan.soundShader != null && chan.leadinSample != null)
                    {
                        savefile.WriteInt(j);

                        // write the pointers out separately
                        savefile.WriteString(chan.soundShader.Name);
                        savefile.WriteString(chan.leadinSample.name);
                    }
                }

                // End active channels with -1
                var end = -1;
                savefile.WriteInt(end);
            }

            // new in Doom3 v1.2
            savefile.Write(slowmoActive);
            savefile.Write(slowmoSpeed);
            savefile.Write(enviroSuitActive);
        }

        public virtual void ReadFromSaveGame(VFile savefile)
        {
            int i, num, handle, channel;
            int currentSoundTime, soundTimeOffset;
            SoundEmitterLocal def;
            string soundShader;

            ClearAllSoundEmitters();

            savefile.ReadVec3(out var origin);
            savefile.ReadMat3(out var axis);
            savefile.ReadInt(out var listenerId);
            savefile.ReadInt(out var gameTime);
            savefile.ReadInt(out var game44kHz);
            savefile.ReadInt(out var savedSoundTime);

            // we will adjust the sound starting times from those saved with the demo
            currentSoundTime = soundSystemLocal.Current44kHzTime;
            soundTimeOffset = currentSoundTime - savedSoundTime;

            // at the end of the level load we unpause the sound world and adjust the sound starting times once more
            pause44kHz = currentSoundTime;

            // place listener
            PlaceListener(origin, axis, listenerId, gameTime, "Undefined");

            // make sure there are enough slots to read the saveGame in.  We don't shrink the list if there are extras.
            savefile.ReadInt(out num);

            while (emitters.Count < num)
            {
                def = new SoundEmitterLocal();
                def.index = emitters.Add_(def);
                def.soundWorld = this;
            }

            // read in the state
            for (i = 1; i < num; i++)
            {
                savefile.ReadInt(out handle);
                if (handle < 0) continue;
                if (handle != i) common.Error("SoundWorldLocal::ReadFromSaveGame: index mismatch");
                def = emitters[i];

                def.removeStatus = REMOVE_STATUS.ALIVE;
                def.playing = true;        // may be reset by the first UpdateListener

                savefile.ReadVec3(out def.origin);
                savefile.ReadInt(out def.listenerId);
                ReadFromSaveGameSoundShaderParams(savefile, def.parms);
                savefile.ReadFloat(out def.amplitude);
                savefile.ReadInt(out def.ampTime);
                for (var k = 0; k < SoundSystemLocal.SOUND_MAX_CHANNELS; k++) ReadFromSaveGameSoundChannel(savefile, def.channels[k]);
                savefile.ReadFloat(out def.distance);
                savefile.ReadBool(out def.hasShakes);
                savefile.ReadInt(out def.lastValidPortalArea);
                savefile.ReadFloat(out def.maxDistance);
                savefile.ReadBool(out def.playing);
                savefile.ReadFloat(out def.realDistance);
                savefile.ReadInt(out int removeStatus); def.removeStatus = (REMOVE_STATUS)removeStatus;
                savefile.ReadVec3(out def.spatializedOrigin);

                // read the individual channels
                savefile.ReadInt(out channel);

                while (channel >= 0)
                {
                    if (channel > SoundSystemLocal.SOUND_MAX_CHANNELS) common.Error("SoundWorldLocal::ReadFromSaveGame: channel > SOUND_MAX_CHANNELS");

                    var chan = def.channels[channel];

                    // The pointer in the save file is not valid, so we grab a new one
                    if (chan.decoder == null) chan.decoder = ISampleDecoder.Alloc();

                    savefile.ReadString(out soundShader);
                    chan.soundShader = (SoundShader)declManager.FindSound(soundShader);

                    savefile.ReadString(out soundShader);
                    // load savegames with s_noSound 1
                    chan.leadinSample = soundSystemLocal.soundCache != null ? soundSystemLocal.soundCache.FindSound(soundShader, false) : null;

                    // adjust the hardware start time
                    chan.trigger44kHzTime += soundTimeOffset;

                    // make sure we start up the hardware voice if needed
                    chan.triggered = chan.triggerState;
                    chan.openalStreamingOffset = currentSoundTime - chan.trigger44kHzTime;

                    // adjust the hardware fade time
                    if (chan.channelFade.fadeStart44kHz != 0) { chan.channelFade.fadeStart44kHz += soundTimeOffset; chan.channelFade.fadeEnd44kHz += soundTimeOffset; }

                    // next command
                    savefile.ReadInt(out channel);
                }
            }

            if (session.SaveGameVersion >= 17)
            {
                savefile.Read(out slowmoActive);
                savefile.Read(out slowmoSpeed);
                savefile.Read(out enviroSuitActive);
            }
            else
            {
                slowmoActive = false;
                slowmoSpeed = 0;
                enviroSuitActive = false;
            }
        }

        public virtual void ReadFromSaveGameSoundChannel(VFile saveGame, SoundChannel ch)
        {
            saveGame.ReadBool(out ch.triggerState);
            saveGame.ReadChar(out _);
            saveGame.ReadChar(out _);
            saveGame.ReadChar(out _);
            saveGame.ReadInt(out ch.trigger44kHzTime);
            saveGame.ReadInt(out ch.triggerGame44kHzTime);
            ReadFromSaveGameSoundShaderParams(saveGame, ch.parms);
            saveGame.ReadInt(out _);
            ch.leadinSample = null;
            saveGame.ReadInt(out ch.triggerChannel);
            saveGame.ReadInt(out _);
            ch.soundShader = null;
            saveGame.ReadInt(out _);
            ch.decoder = null;
            saveGame.ReadFloat(out ch.diversity);
            saveGame.ReadFloat(out ch.lastVolume);
            for (var m = 0; m < 6; m++) saveGame.ReadFloat(out ch.lastV[m]);
            saveGame.ReadInt(out ch.channelFade.fadeStart44kHz);
            saveGame.ReadInt(out ch.channelFade.fadeEnd44kHz);
            saveGame.ReadFloat(out ch.channelFade.fadeStartVolume);
            saveGame.ReadFloat(out ch.channelFade.fadeEndVolume);
        }

        public virtual void ReadFromSaveGameSoundShaderParams(VFile saveGame, SoundShaderParms parms)
        {
            saveGame.ReadFloat(out parms.minDistance);
            saveGame.ReadFloat(out parms.maxDistance);
            saveGame.ReadFloat(out parms.volume);
            saveGame.ReadFloat(out parms.shakes);
            saveGame.ReadInt(out parms.soundShaderFlags);
            saveGame.ReadInt(out parms.soundClass);
        }

        public virtual void WriteToSaveGameSoundChannel(VFile saveGame, SoundChannel ch)
        {
            saveGame.WriteBool(ch.triggerState);
            saveGame.WriteUnsignedChar(0);
            saveGame.WriteUnsignedChar(0);
            saveGame.WriteUnsignedChar(0);
            saveGame.WriteInt(ch.trigger44kHzTime);
            saveGame.WriteInt(ch.triggerGame44kHzTime);
            WriteToSaveGameSoundShaderParams(saveGame, ch.parms);
            saveGame.WriteInt(0 /* ch.leadinSample */ );
            saveGame.WriteInt(ch.triggerChannel);
            saveGame.WriteInt(0 /* ch.soundShader */ );
            saveGame.WriteInt(0 /* ch.decoder */ );
            saveGame.WriteFloat(ch.diversity);
            saveGame.WriteFloat(ch.lastVolume);
            for (var m = 0; m < 6; m++) saveGame.WriteFloat(ch.lastV[m]);
            saveGame.WriteInt(ch.channelFade.fadeStart44kHz);
            saveGame.WriteInt(ch.channelFade.fadeEnd44kHz);
            saveGame.WriteFloat(ch.channelFade.fadeStartVolume);
            saveGame.WriteFloat(ch.channelFade.fadeEndVolume);
        }

        public virtual void WriteToSaveGameSoundShaderParams(VFile saveGame, SoundShaderParms parms)
        {
            saveGame.WriteFloat(parms.minDistance);
            saveGame.WriteFloat(parms.maxDistance);
            saveGame.WriteFloat(parms.volume);
            saveGame.WriteFloat(parms.shakes);
            saveGame.WriteInt(parms.soundShaderFlags);
            saveGame.WriteInt(parms.soundClass);
        }

        public virtual void SetSlowmo(bool active)
            => slowmoActive = active;

        public virtual void SetSlowmoSpeed(float speed)
            => slowmoSpeed = speed;

        public virtual void SetEnviroSuit(bool active)
            => enviroSuitActive = active;

        //=======================================

        public IRenderWorld rw;              // for portals and debug drawing
        public VFileDemo writeDemo;          // if not NULL, archive commands here

        public Matrix3x3 listenerAxis;
        public Vector3 listenerPos;     // position in meters
        public int listenerPrivateId;
        public Vector3 listenerQU;          // position in "quake units"
        public int listenerArea;
        public string listenerAreaName;
        public int listenerEffect;
        public int listenerSlot;
        public int listenerFilter;

        public int gameMsec;
        public int game44kHz;
        public int pause44kHz;
        public int lastAVI44kHz;       // determine when we need to mix and write another block

        public List<SoundEmitterLocal> emitters = new();

        public SoundFade[] soundClassFade = new SoundFade[ISoundSystem.SOUND_MAX_CLASSES];  // for global sound fading

        // avi stuff
        public VFile[] fpa = new VFile[6];
        public string aviDemoPath;
        public string aviDemoName;

        public SoundEmitterLocal localSound;        // just for playShaderDirectly()

        public bool slowmoActive;
        public float slowmoSpeed;
        public bool enviroSuitActive;

        public SoundWorldLocal() { }

        public void Dispose()
            => Shutdown();

        // this is called from the main thread
        public void Shutdown()
        {
            int i;

            if (soundSystemLocal.currentSoundWorld == this)
                soundSystemLocal.currentSoundWorld = null;

            AVIClose();

            if (SoundSystemLocal.useEFXReverb)
            {
                if (soundSystemLocal.alIsAuxiliaryEffectSlot(listenerSlot))
                {
                    soundSystemLocal.alAuxiliaryEffectSloti(listenerSlot, EffectSlotInteger.Effect, 0);
                    soundSystemLocal.alDeleteAuxiliaryEffectSlots(1, ref listenerSlot);
                    listenerSlot = 0;
                }

                if (soundSystemLocal.alIsFilter(listenerFilter))
                {
                    soundSystemLocal.alDeleteFilters(1, ref listenerFilter);
                    listenerFilter = 0;
                }
            }

            for (i = 0; i < emitters.Count; i++) if (emitters[i] != null) emitters[i] = null;
            localSound = null;
        }

        public void Init(IRenderWorld rw)
        {
            this.rw = rw;
            writeDemo = null;

            listenerAxis.Identity();
            listenerPos.Zero();
            listenerPrivateId = 0;
            listenerQU.Zero();
            listenerArea = 0;
            listenerAreaName = "Undefined";

            if (SoundSystemLocal.useEFXReverb)
            {
                if (!soundSystemLocal.alIsAuxiliaryEffectSlot(listenerSlot))
                {
                    AL.GetError();

                    soundSystemLocal.alGenAuxiliaryEffectSlots(1, ref listenerSlot);
                    var e = AL.GetError();
                    if (e != ALError.NoError) { common.Warning($"SoundWorldLocal::Init: alGenAuxiliaryEffectSlots failed: 0x{e}"); listenerSlot = 0; }
                }

                if (!soundSystemLocal.alIsFilter(listenerFilter))
                {
                    AL.GetError();

                    soundSystemLocal.alGenFilters(1, ref listenerFilter);
                    var e = AL.GetError();
                    if (e != ALError.NoError) { common.Warning($"SoundWorldLocal::Init: alGenFilters failed: 0x{e}"); listenerFilter = 0; }
                    else
                    {
                        soundSystemLocal.alFilteri(listenerFilter, FilterInteger.FilterType, (int)FilterType.Lowpass);
                        // original EAX occusion value was -1150
                        // default OCCLUSIONLFRATIO is 0.25

                        // pow(10.0, (-1150*0.25)/2000.0)
                        soundSystemLocal.alFilterf(listenerFilter, FilterFloat.LowpassGain, 0.718208f);
                        // pow(10.0, -1150/2000.0)
                        soundSystemLocal.alFilterf(listenerFilter, FilterFloat.LowpassGainHF, 0.266073f);
                    }
                }
            }

            gameMsec = 0;
            game44kHz = 0;
            pause44kHz = -1;
            lastAVI44kHz = 0;

            for (var i = 0; i < ISoundSystem.SOUND_MAX_CLASSES; i++) soundClassFade[i].Clear();

            // fill in the 0 index spot
            var placeHolder = new SoundEmitterLocal();
            emitters.Add(placeHolder);

            fpa[0] = fpa[1] = fpa[2] = fpa[3] = fpa[4] = fpa[5] = null;

            aviDemoPath = "";
            aviDemoName = "";

            localSound = null;

            slowmoActive = false;
            slowmoSpeed = 0;
            enviroSuitActive = false;
        }

        // update
        public void ForegroundUpdate(int current44kHzTime)
        {
            int j, k; SoundEmitterLocal def;

            if (!soundSystemLocal.isInitialized) return;

            ISystem.EnterCriticalSection();

            // if we are recording an AVI demo, don't use hardware time
            if (fpa[0] != null) current44kHzTime = lastAVI44kHz;

            // check to see if each sound is visible or not speed up by checking maxdistance to origin
            // although the sound may still need to play if it has just become occluded so it can ramp down to 0
            for (j = 1; j < emitters.Count; j++)
            {
                def = emitters[j];

                if (def.removeStatus >= REMOVE_STATUS.SAMPLEFINISHED) continue;

                // see if our last channel just finished
                def.CheckForCompletion(current44kHzTime);

                if (!def.playing) continue;

                // update virtual origin / distance, etc
                def.Spatialize(listenerPos, listenerArea, rw);

                // per-sound debug options
                if (SoundSystemLocal.s_drawSounds.Integer != 0 && rw != null)
                {
                    if (def.distance < def.maxDistance || SoundSystemLocal.s_drawSounds.Integer > 1)
                    {
                        var ref_ = new Bounds();
                        ref_.Clear();
                        ref_.AddPoint(new Vector3(-10, -10, -10));
                        ref_.AddPoint(new Vector3(10, 10, 10));
                        var vis = 1f - (def.distance / def.maxDistance);

                        // draw a box
                        rw.DebugBounds(new Vector4(vis, 0.25f, vis, vis), ref_, def.origin);

                        // draw an arrow to the audible position, possible a portal center
                        if (def.origin != def.spatializedOrigin)
                            rw.DebugArrow(colorRed, def.origin, def.spatializedOrigin, 4);

                        // draw the index
                        var textPos = def.origin;
                        textPos.z -= 8;
                        rw.DrawText(def.index.ToString(), textPos, 0.1f, new Vector4(1, 0, 0, 1), listenerAxis);
                        textPos.z += 8;

                        // run through all the channels
                        for (k = 0; k < SoundSystemLocal.SOUND_MAX_CHANNELS; k++)
                        {
                            var chan = def.channels[k];

                            // see if we have a sound triggered on this channel
                            if (!chan.triggerState) continue;

                            var min = chan.parms.minDistance;
                            var max = chan.parms.maxDistance;
                            var defaulted = chan.leadinSample.defaultSound ? "(DEFAULTED)" : "";
                            var text = $"{chan.soundShader.Name} ({(int)def.distance}/{(int)def.realDistance} {(int)min}/{(int)max}){defaulted}";
                            rw.DrawText(text, textPos, 0.1f, new Vector4(1, 0, 0, 1), listenerAxis);
                            textPos[2] += 8;
                        }
                    }
                }
            }

            ISystem.LeaveCriticalSection();

            // the sound meter
            if (SoundSystemLocal.s_showLevelMeter.Integer != 0)
            {
                var gui = declManager.FindMaterial("guis/assets/soundmeter/audiobg", false);
                if (gui != null)
                {
                    var foo = gui.GetStage(0);
                    if (foo.texture.cinematic == null) foo.texture.cinematic = new SndWindow();
                }
            }

            // optionally dump out the generated sound
            if (fpa[0] != null) AVIUpdate();
        }

        public void OffsetSoundTime(int offset44kHz)
        {
            int i, j;

            for (i = 0; i < emitters.Count; i++)
            {
                if (emitters[i] == null) continue;
                for (j = 0; j < SoundSystemLocal.SOUND_MAX_CHANNELS; j++)
                {
                    var chan = emitters[i].channels[j];

                    if (!chan.triggerState) continue;

                    chan.trigger44kHzTime += offset44kHz;
                }
            }
        }

        public SoundEmitterLocal AllocLocalSoundEmitter()
        {
            int i, index; SoundEmitterLocal def = null;

            index = -1;

            // never use the 0 index spot
            for (i = 1; i < emitters.Count; i++)
            {
                def = emitters[i];

                // check for a completed and freed spot
                if (def.removeStatus >= REMOVE_STATUS.SAMPLEFINISHED)
                {
                    index = i;
                    if (SoundSystemLocal.s_showStartSound.Integer != 0) common.Printf($"sound: recycling sound def {i}\n");
                    break;
                }
            }

            if (index == -1)
            {
                // append a brand new one
                def = new SoundEmitterLocal();

                // we need to protect this from the async thread
                ISystem.EnterCriticalSection();
                index = emitters.Add_(def);
                ISystem.LeaveCriticalSection();

                if (SoundSystemLocal.s_showStartSound.Integer != 0) common.Printf($"sound: appended new sound def {index}\n");
            }

            def.Clear();
            def.index = index;
            def.removeStatus = REMOVE_STATUS.ALIVE;
            def.soundWorld = this;

            return def;
        }

        // Determine the volumes from each speaker for a given sound emitter
        static Vector3[] CalcEars_speakerVector = {
            new Vector3( 0.707f,  0.707f, 0f),	// front left
			new Vector3( 0.707f, -0.707f, 0f),	// front right
			new Vector3( 0.707f,      0f, 0f),	// front center
			new Vector3(     0f,      0f, 0f),	// sub
			new Vector3(-0.707f,  0.707f, 0f),	// rear left
			new Vector3(-0.707f, -0.707f, 0f)	// rear right
		};
        public void CalcEars(int numSpeakers, Vector3 realOrigin, Vector3 listenerPos, Matrix3x3 listenerAxis, float[] ears, float spatialize)
        {
            Vector3 svec = realOrigin - listenerPos;
            Vector3 ovec;

            ovec.x = svec * listenerAxis[0];
            ovec.y = svec * listenerAxis[1];
            ovec.z = svec * listenerAxis[2];

            ovec.Normalize();

            if (numSpeakers == 6)
            {
                for (var i = 0; i < 6; i++)
                {
                    if (i == 3) { ears[i] = SoundSystemLocal.s_subFraction.Float; continue; } // subwoofer
                    var dot = ovec * CalcEars_speakerVector[i];
                    ears[i] = (SoundSystemLocal.s_dotbias6.Float + dot) / (1f + SoundSystemLocal.s_dotbias6.Float);
                    if (ears[i] < SoundSystemLocal.s_minVolume6.Float) ears[i] = SoundSystemLocal.s_minVolume6.Float;
                }
            }
            else
            {
                var dot = ovec.y;
                var dotBias = SoundSystemLocal.s_dotbias2.Float;

                // when we are inside the minDistance, start reducing the amount of spatialization so NPC voices right in front of us aren't quieter that off to the side
                dotBias += (SoundSystemLocal.s_spatializationDecay.Float - dotBias) * (1f - spatialize);

                ears[0] = (SoundSystemLocal.s_dotbias2.Float + dot) / (1f + dotBias);
                ears[1] = (SoundSystemLocal.s_dotbias2.Float - dot) / (1f + dotBias);
                if (ears[0] < SoundSystemLocal.s_minVolume2.Float) ears[0] = SoundSystemLocal.s_minVolume2.Float;
                if (ears[1] < SoundSystemLocal.s_minVolume2.Float) ears[1] = SoundSystemLocal.s_minVolume2.Float;
                ears[2] = ears[3] = ears[4] = ears[5] = 0f;
            }
        }

        // Adds the contribution of a single sound channel to finalMixBuffer this is called from the async thread
        // Mixes MIXBUFFER_SAMPLES samples starting at current44kHz sample time into finalMixBuffer
        public unsafe void AddChannelContribution(SoundEmitterLocal sound, SoundChannel chan, int current44kHz, int numSpeakers, float* finalMixBuffer)
        {
            int j; float volume;

            // get the sound definition and parameters from the entity
            var parms = chan.parms;

            // assume we have a sound triggered on this channel
            Debug.Assert(chan.triggerState);

            // fetch the actual wave file and see if it's valid
            var sample = chan.leadinSample;
            if (sample == null) return;

            // if you don't want to hear all the beeps from missing sounds
            if (sample.defaultSound && !SoundSystemLocal.s_playDefaultSound.Bool) return;

            // get the actual shader
            var shader = chan.soundShader;

            // this might happen if the foreground thread just deleted the sound emitter
            if (shader == null) return;

            var maxd = parms.maxDistance;
            var mind = parms.minDistance;

            var mask = shader.speakerMask;
            var omni = (parms.soundShaderFlags & ISoundSystem.SSF_OMNIDIRECTIONAL) != 0;
            var looping = (parms.soundShaderFlags & ISoundSystem.SSF_LOOPING) != 0;
            var global = (parms.soundShaderFlags & ISoundSystem.SSF_GLOBAL) != 0;
            var noOcclusion = (parms.soundShaderFlags & ISoundSystem.SSF_NO_OCCLUSION) != 0 || !SoundSystemLocal.s_useOcclusion.Bool;

            // speed goes from 1 to 0.2
            if (SoundSystemLocal.s_slowAttenuate.Bool && slowmoActive && !chan.disallowSlow) maxd *= slowmoSpeed;

            // stereo samples are always omni
            if (sample.objectInfo.nChannels == 2) omni = true;

            // if the sound is playing from the current listener, it will not be spatialized at all
            if (sound.listenerId == listenerPrivateId) global = true;

            // see if it's in range

            // convert volumes from decibels to float scale

            // leadin volume scale for shattering lights this isn't exactly correct, because the modified volume will get applied to
            // some initial chunk of the loop as well, because the volume is scaled for the entire mix buffer
            volume = shader.leadinVolume != 0 && current44kHz - chan.trigger44kHzTime < sample.LengthIn44kHzSamples
                ? soundSystemLocal.dB2Scale(shader.leadinVolume)
                : soundSystemLocal.dB2Scale(parms.volume);

            // global volume scale
            volume *= soundSystemLocal.dB2Scale(SoundSystemLocal.s_volume.Float);

            // DG: scaling the volume of *everything* down a bit to prevent some sounds. (like shotgun shot) being "drowned" when lots of other loud sounds.
            // (like shotgun impacts on metal) are played at the same time. I guess this happens because the loud sounds mixed together are too loud so OpenAL just makes *everything* quiter or sth like that.
            // See also https://github.com/dhewm/dhewm3/issues/179
            volume *= 0.333f; // (0.333 worked fine, 0.5 didn't)

            // volume fading
            var fadeDb = chan.channelFade.FadeDbAt44kHz(current44kHz);
            volume *= soundSystemLocal.dB2Scale(fadeDb);

            fadeDb = soundClassFade[parms.soundClass].FadeDbAt44kHz(current44kHz);
            volume *= soundSystemLocal.dB2Scale(fadeDb);

            // if it's a global sound then it's not affected by distance or occlusion
            var spatialize = 1f;
            Vector3 spatializedOriginInMeters;
            if (!global)
            {
                float dlen;

                if (noOcclusion)
                {
                    // use the real origin and distance
                    spatializedOriginInMeters = sound.origin * ISoundSystem.DOOM_TO_METERS;
                    dlen = sound.realDistance;
                }
                else
                {
                    // use the possibly portal-occluded origin and distance
                    spatializedOriginInMeters = sound.spatializedOrigin * ISoundSystem.DOOM_TO_METERS;
                    dlen = sound.distance;
                }

                // reduce volume based on distance
                if (dlen >= maxd) volume = 0f;
                else if (dlen > mind)
                {
                    var frac = MathX.ClampFloat(0f, 1f, 1f - ((dlen - mind) / (maxd - mind)));
                    if (SoundSystemLocal.s_quadraticFalloff.Bool) frac *= frac;
                    volume *= frac;
                }
                // we tweak the spatialization bias when you are inside the minDistance
                else if (mind > 0f) spatialize = dlen / mind;
            }
            else spatializedOriginInMeters = Vector3.origin;

            // if it is a private sound, set the volume to zero unless we match the listenerId
            if ((parms.soundShaderFlags & ISoundSystem.SSF_PRIVATE_SOUND) != 0 && sound.listenerId != listenerPrivateId) volume = 0;
            if ((parms.soundShaderFlags & ISoundSystem.SSF_ANTI_PRIVATE_SOUND) != 0 && sound.listenerId == listenerPrivateId) volume = 0;

            // do we have anything to add?
            if (volume < SoundSystemLocal.SND_EPSILON && chan.lastVolume < SoundSystemLocal.SND_EPSILON) return;
            chan.lastVolume = volume;

            // fetch the sound from the cache as 44kHz, 16 bit samples
            var offset = current44kHz - chan.trigger44kHzTime;
            var inputSamples = new float[Simd.MIXBUFFER_SAMPLES * 2 + 16];
            fixed (float* inputSamplesF = inputSamples)
            {
                var alignedInputSamples = (float*)((int)(((byte*)inputSamplesF) + 15) & ~15);
                // allocate and initialize hardware source
                if (sound.removeStatus < REMOVE_STATUS.SAMPLEFINISHED)
                {
                    if (!AL.IsSource(chan.openalSource)) chan.openalSource = soundSystemLocal.AllocOpenALSource(chan, !chan.leadinSample.hardwareBuffer || !chan.soundShader.entries[0].hardwareBuffer || looping, chan.leadinSample.objectInfo.nChannels == 2);

                    if (AL.IsSource(chan.openalSource))
                    {
                        // stop source if needed..
                        if (chan.triggered) AL.SourceStop(chan.openalSource);

                        // update source parameters
                        if (global || omni)
                        {
                            AL.Source(chan.openalSource, ALSourceb.SourceRelative, true);
                            AL.Source(chan.openalSource, ALSource3f.Position, 0f, 0f, 0f);
                            AL.Source(chan.openalSource, ALSourcef.Gain, volume < 1f ? volume : 1f);
                        }
                        else
                        {
                            AL.Source(chan.openalSource, ALSourceb.SourceRelative, false);
                            AL.Source(chan.openalSource, ALSource3f.Position, -spatializedOriginInMeters.y, spatializedOriginInMeters.z, -spatializedOriginInMeters.x);
                            AL.Source(chan.openalSource, ALSourcef.Gain, volume < 1f ? volume : 1f);
                        }
                        // DG: looping sounds with a leadin can't just use a HW buffer and openal's AL_LOOPING because we need to switch from leadin to the looped sound.. see https://github.com/dhewm/dhewm3/issues/291
                        var haveLeadin = chan.soundShader.numLeadins > 0;
                        AL.Source(chan.openalSource, ALSourceb.Looping, looping && chan.soundShader.entries[0].hardwareBuffer && !haveLeadin);
#if true
                        AL.Source(chan.openalSource, ALSourcef.ReferenceDistance, mind);
                        AL.Source(chan.openalSource, ALSourcef.MaxDistance, maxd);
#endif
                        AL.Source(chan.openalSource, ALSourcef.Pitch, slowmoActive && !chan.disallowSlow ? slowmoSpeed : 1f);

                        if (SoundSystemLocal.useEFXReverb)
                            if (enviroSuitActive)
                            {
                                AL.Source(chan.openalSource, ALSourcei.EfxDirectFilter, listenerFilter);
                                AL.Source(chan.openalSource, ALSource3i.EfxAuxiliarySendFilter, listenerSlot, 0, listenerFilter);
                            }
                            else AL.Source(chan.openalSource, ALSource3i.EfxAuxiliarySendFilter, listenerSlot, 0, 0);

                        if ((!looping && chan.leadinSample.hardwareBuffer) || (looping && !haveLeadin && chan.soundShader.entries[0].hardwareBuffer))
                        {
                            // handle uncompressed (non streaming) single shot and looping sounds
                            // DG: ... that have no leadin (with leadin we still need to switch to another sound, just use streaming code for that) - see https://github.com/dhewm/dhewm3/issues/291
                            if (chan.triggered)
                                AL.Source(chan.openalSource, ALSourcei.Buffer, looping ? chan.soundShader.entries[0].openalBuffer : chan.leadinSample.openalBuffer);
                        }
                        else
                        {
                            int finishedbuffers;
                            var buffers = new int[3];

                            // handle streaming sounds (decode on the fly) both single shot AND looping
                            if (chan.triggered)
                            {
                                AL.Source(chan.openalSource, ALSourcei.Buffer, 0);
                                AL.DeleteBuffers(3, ref chan.lastopenalStreamingBuffer[0]);
                                chan.lastopenalStreamingBuffer[0] = chan.openalStreamingBuffer[0];
                                chan.lastopenalStreamingBuffer[1] = chan.openalStreamingBuffer[1];
                                chan.lastopenalStreamingBuffer[2] = chan.openalStreamingBuffer[2];
                                AL.GenBuffers(3, ref chan.openalStreamingBuffer[0]);
                                buffers[0] = chan.openalStreamingBuffer[0];
                                buffers[1] = chan.openalStreamingBuffer[1];
                                buffers[2] = chan.openalStreamingBuffer[2];
                                finishedbuffers = 3;
                            }
                            else
                            {
                                AL.GetSource(chan.openalSource, ALGetSourcei.BuffersProcessed, out finishedbuffers);
                                AL.SourceUnqueueBuffers(chan.openalSource, finishedbuffers, ref buffers[0]);
                                if (finishedbuffers == 3) chan.triggered = true;
                            }

                            for (j = 0; j < finishedbuffers; j++)
                            {
                                chan.GatherChannelSamples(chan.openalStreamingOffset * sample.objectInfo.nChannels, Simd.MIXBUFFER_SAMPLES * sample.objectInfo.nChannels, alignedInputSamples);
                                var alignedInputSamplesS = (short*)alignedInputSamples;
                                for (var i = 0; i < (Simd.MIXBUFFER_SAMPLES * sample.objectInfo.nChannels); i++)
                                {
                                    if (alignedInputSamples[i] < -32768f) alignedInputSamplesS[i] = -32768;
                                    else if (alignedInputSamples[i] > 32767f) alignedInputSamplesS[i] = 32767;
                                    else alignedInputSamplesS[i] = (short)MathX.FtoiFast(alignedInputSamples[i]);
                                }
                                AL.BufferData(buffers[j], chan.leadinSample.objectInfo.nChannels == 1 ? ALFormat.Mono16 : ALFormat.Stereo16, alignedInputSamples, Simd.MIXBUFFER_SAMPLES * sample.objectInfo.nChannels * sizeof(short), 44100);
                                chan.openalStreamingOffset += Simd.MIXBUFFER_SAMPLES;
                            }

                            if (finishedbuffers != 0) AL.SourceQueueBuffers(chan.openalSource, finishedbuffers, ref buffers[0]);
                        }

                        // (re)start if needed..
                        if (chan.triggered) { AL.SourcePlay(chan.openalSource); chan.triggered = false; }
                    }
                }
#if true
                // DG: I /think/ this was only relevant for the old sound backends?
                // FIXME: completely remove else branch, but for testing leave it in under com_asyncSound 2
                //        (which also does the old 92-100ms updates)
                else if (C.com_asyncSound.Integer == 2)
                {
                    if (slowmoActive && !chan.disallowSlow)
                    {
                        var slow = sound.GetSlowChannel(chan);

                        slow.AttachSoundChannel(chan);

                        // need to add a stereo path, but very few samples go through this
                        if (sample.objectInfo.nChannels == 2) UnsafeX.InitBlock(alignedInputSamples, 0, sizeof(float) * Simd.MIXBUFFER_SAMPLES * 2);
                        else slow.GatherChannelSamples(offset, Simd.MIXBUFFER_SAMPLES, alignedInputSamples);

                        sound.SetSlowChannel(chan, slow);
                    }
                    else
                    {
                        sound.ResetSlowChannel(chan);

                        // if we are getting a stereo sample adjust accordingly
                        if (sample.objectInfo.nChannels == 2) chan.GatherChannelSamples(offset * 2, Simd.MIXBUFFER_SAMPLES * 2, alignedInputSamples); // we should probably check to make sure any looping is also to a stereo sample...
                        else chan.GatherChannelSamples(offset, Simd.MIXBUFFER_SAMPLES, alignedInputSamples);
                    }

                    // work out the left / right ear values
                    var ears = new float[6];
                    if (global || omni)
                    {
                        // same for all speakers
                        for (var i = 0; i < 6; i++) ears[i] = SoundSystemLocal.s_globalFraction.Float * volume;
                        ears[3] = SoundSystemLocal.s_subFraction.Float * volume;       // subwoofer

                    }
                    else
                    {
                        CalcEars(numSpeakers, spatializedOriginInMeters, listenerPos, listenerAxis, ears, spatialize);
                        for (var i = 0; i < 6; i++) ears[i] *= volume;
                    }

                    // if the mask is 0, it really means do every channel
                    if (mask == 0) mask = 255;
                    // cleared mask bits set the mix volume to zero
                    for (var i = 0; i < 6; i++) if ((mask & (1 << i)) == 0) ears[i] = 0;

                    // if sounds are generally normalized, using a mixing volume over 1.0 will almost always cause clipping noise.  If samples aren't normalized, there
                    // is a good call to allow overvolumes
                    if (SoundSystemLocal.s_clipVolumes.Bool && (parms.soundShaderFlags & ISoundSystem.SSF_UNCLAMPED) == 0)
                        for (var i = 0; i < 6; i++) if (ears[i] > 1f) ears[i] = 1f;

                    // if this is the very first mixing block, set the lastV to the current volume
                    if (current44kHz == chan.trigger44kHzTime) for (j = 0; j < 6; j++) chan.lastV[j] = ears[j];

                    if (numSpeakers == 6)
                    {
                        if (sample.objectInfo.nChannels == 1) Simd.MixSoundSixSpeakerMono(finalMixBuffer, alignedInputSamples, Simd.MIXBUFFER_SAMPLES, chan.lastV, ears);
                        else Simd.MixSoundSixSpeakerStereo(finalMixBuffer, alignedInputSamples, Simd.MIXBUFFER_SAMPLES, chan.lastV, ears);
                    }
                    else
                    {
                        if (sample.objectInfo.nChannels == 1) Simd.MixSoundTwoSpeakerMono(finalMixBuffer, alignedInputSamples, Simd.MIXBUFFER_SAMPLES, chan.lastV, ears);
                        else Simd.MixSoundTwoSpeakerStereo(finalMixBuffer, alignedInputSamples, Simd.MIXBUFFER_SAMPLES, chan.lastV, ears);
                    }

                    for (j = 0; j < 6; j++) chan.lastV[j] = ears[j];
                }
#endif
            }

            soundSystemLocal.soundStats.activeSounds++;
        }

        // Sum all sound contributions into finalMixBuffer, an unclamped float buffer holding all output channels.  MIXBUFFER_SAMPLES samples will be created, with each sample consisting
        // of 2 or 6 floats depending on numSpeakers.
        // 
        // this is normally called from the sound thread, but also from the main thread for AVIdemo writing
        public unsafe void MixLoop(int current44kHz, int numSpeakers, float* finalMixBuffer)
        {
            int i, j; SoundEmitterLocal sound;

            // if noclip flying outside the world, leave silence
            if (listenerArea == -1) { AL.Listener(ALListenerf.Gain, 0f); return; }

            // update the listener position and orientation
            var listenerPosition0 = -listenerPos.y;
            var listenerPosition1 = listenerPos.z;
            var listenerPosition2 = -listenerPos.x;

            var listenerOrientation = new[]{
                -listenerAxis[0].y,
                listenerAxis[0].z,
                -listenerAxis[0].x,
                -listenerAxis[2].y,
                listenerAxis[2].z,
                -listenerAxis[2].x
            };

            AL.Listener(ALListenerf.Gain, 1f);
            AL.Listener(ALListener3f.Position, listenerPosition0, listenerPosition1, listenerPosition2);
            AL.Listener(ALListenerfv.Orientation, listenerOrientation);

            if (SoundSystemLocal.useEFXReverb && soundSystemLocal.efxloaded)
            {
                var s = listenerArea.ToString();

                var found = soundSystemLocal.EFXDatabase.FindEffect(s, out var effect);
                if (!found)
                {
                    s = listenerAreaName;
                    found = soundSystemLocal.EFXDatabase.FindEffect(s, out effect);
                }
                if (!found)
                {
                    s = "default";
                    found = soundSystemLocal.EFXDatabase.FindEffect(s, out effect);
                }

                // only update if change in settings
                if (found && listenerEffect != effect)
                {
                    EFXFile.EFXprintf($"Switching to EFX '{s}' (#{effect:u})\n");
                    listenerEffect = effect;
                    soundSystemLocal.alAuxiliaryEffectSloti(listenerSlot, EffectSlotInteger.Effect, effect);
                }
            }

            // debugging option to mute all but a single soundEmitter
            if (SoundSystemLocal.s_singleEmitter.Integer > 0 && SoundSystemLocal.s_singleEmitter.Integer < emitters.Count)
            {
                sound = emitters[SoundSystemLocal.s_singleEmitter.Integer];

                if (sound != null && sound.playing)
                    // run through all the channels
                    for (j = 0; j < SoundSystemLocal.SOUND_MAX_CHANNELS; j++)
                    {
                        var chan = sound.channels[j];

                        // see if we have a sound triggered on this channel
                        if (!chan.triggerState) { chan.ALStop(); continue; }

                        AddChannelContribution(sound, chan, current44kHz, numSpeakers, finalMixBuffer);
                    }
                return;
            }

            for (i = 1; i < emitters.Count; i++)
            {
                sound = emitters[i];
                if (sound == null) continue;
                // if no channels are active, do nothing
                if (!sound.playing) continue;
                // run through all the channels
                for (j = 0; j < SoundSystemLocal.SOUND_MAX_CHANNELS; j++)
                {
                    var chan = sound.channels[j];

                    // see if we have a sound triggered on this channel
                    if (!chan.triggerState) { chan.ALStop(); continue; }

                    AddChannelContribution(sound, chan, current44kHz, numSpeakers, finalMixBuffer);
                }
            }

            // TODO port to OpenAL
            if (false && enviroSuitActive) soundSystemLocal.DoEnviroSuit(finalMixBuffer, Simd.MIXBUFFER_SAMPLES, numSpeakers);
        }

        // this is called by the main thread writes one block of sound samples if enough time has passed
        // This can be used to write wave files even if no sound hardware exists
        public unsafe void AVIUpdate()
        {
            if (game44kHz - lastAVI44kHz < Simd.MIXBUFFER_SAMPLES) return;

            var numSpeakers = SoundSystemLocal.s_numberOfSpeakers.Integer;

            var mix = stackalloc float[Simd.MIXBUFFER_SAMPLES * 6 + 16]; mix = (float*)_alloca16(mix);

            Simd.Memset(mix, 0, Simd.MIXBUFFER_SAMPLES * sizeof(float) * numSpeakers);

            MixLoop(lastAVI44kHz, numSpeakers, mix);

            var outD = stackalloc short[Simd.MIXBUFFER_SAMPLES];
            for (var i = 0; i < numSpeakers; i++)
            {
                for (var j = 0; j < Simd.MIXBUFFER_SAMPLES; j++)
                {
                    var s = mix[j * numSpeakers + i];
                    outD[j] = s < -32768f ? (short)-32768
                        : s > 32767f ? (short)32767
                        : (short)MathX.FtoiFast(s);
                }
                // write to file
                fpa[i].Write((byte*)outD, Simd.MIXBUFFER_SAMPLES * sizeof(short));
            }

            lastAVI44kHz += Simd.MIXBUFFER_SAMPLES;

            return;
        }

        const int MAX_PORTAL_TRACE_DEPTH = 10;
        // Find out of the sound is completely occluded by a closed door portal, or the virtual sound origin position at the portal closest to the listener.
        // this is called by the main thread
        // 
        // dist is the distance from the orignial sound origin to the current portal that enters soundArea def.distance is the distance we are trying to reduce.
        // 
        // If there is no path through open portals from the sound to the listener, def.distance will remain set at maxDistance
        public void ResolveOrigin(int stackDepth, SoundPortalTrace prevStack, int soundArea, float dist, Vector3 soundOrigin, SoundEmitterLocal def)
        {
            // we can't possibly hear the sound through this chain of portals
            if (dist >= def.distance) return;

            if (soundArea == listenerArea)
            {
                var fullDist = dist + (soundOrigin - listenerQU).LengthFast;
                if (fullDist < def.distance) { def.distance = fullDist; def.spatializedOrigin = soundOrigin; }
                return;
            }

            // don't spend too much time doing these calculations in big maps
            if (stackDepth == MAX_PORTAL_TRACE_DEPTH) return;

            SoundPortalTrace newStack = new();
            newStack.portalArea = soundArea;
            newStack.prevStack = prevStack;

            var numPortals = rw.NumPortalsInArea(soundArea);
            for (var p = 0; p < numPortals; p++)
            {
                var re = rw.GetPortal(soundArea, p);
                var occlusionDistance = 0f;

                // air blocking windows will block sound like closed doors
                if ((re.blockingBits & (int)(PortalConnection.PS_BLOCK_VIEW | PortalConnection.PS_BLOCK_AIR)) != 0)
                    // we could just completely cut sound off, but reducing the volume works better
                    // continue;
                    occlusionDistance = SoundSystemLocal.s_doorDistanceAdd.Float;

                // what area are we about to go look at
                var otherArea = re.areas[0];
                if (re.areas[0] == soundArea) otherArea = re.areas[1];

                // if this area is already in our portal chain, don't bother looking into it
                SoundPortalTrace prev;
                for (prev = prevStack; prev != null; prev = prev.prevStack) if (prev.portalArea == otherArea) break;
                if (prev != null) continue;

                // pick a point on the portal to serve as our virtual sound origin
#if true
                Vector3 source;
                re.w.GetPlane(out var pl);

                Vector3 dir = listenerQU - soundOrigin;
                if (!pl.RayIntersection(soundOrigin, dir, out var scale)) source = re.w.Center;
                else
                {
                    source = soundOrigin + scale * dir;

                    // if this point isn't inside the portal edges, slide it in
                    for (var i = 0; i < re.w.NumPoints; i++)
                    {
                        var j = (i + 1) % re.w.NumPoints;
                        var edgeDir = re.w[j].ToVec3() - re.w[i].ToVec3();
                        Vector3 edgeNormal = new();

                        edgeNormal.Cross(pl.Normal, edgeDir);

                        var fromVert = source - re.w[j].ToVec3();

                        var d = edgeNormal * fromVert;
                        if (d > 0)
                        {
                            // move it in
                            var div = edgeNormal.Normalize();
                            d /= div;

                            source -= d * edgeNormal;
                        }
                    }
                }
#else
            // clip the ray from the listener to the center of the portal by all the portal edge planes, then project that point (or the original if not clipped)
            // onto the portal plane to get the spatialized origin

            var start = listenerQU;
            var mid = re.w.Center;
            var wasClipped = false;

            for (var i = 0; i < re.w.NumPoints; i++)
            {
                var j = (i + 1) % re.w.NumPoints;
                var v1 = re.w[j].ToVec3() - soundOrigin;
                var v2 = re.w[i].ToVec3() - soundOrigin;

                v1.Normalize();
                v2.Normalize();

                Vector3 edgeNormal = new();

                edgeNormal.Cross(v1, v2);

                var fromVert = start - soundOrigin;
                var d1 = edgeNormal * fromVert;

                if (d1 > 0f)
                {
                    fromVert = mid - re.w[j].ToVec3();
                    var d2 = edgeNormal * fromVert;

                    // move it in
                    var f = d1 / (d1 - d2);

                    var clipped = start * (1f - f) + mid * f;
                    start = clipped;
                    wasClipped = true;
                }
            }

            Vector3 source;
            if (wasClipped)
            {
                // now project it onto the portal plane
                re.w.GetPlane(out var pl);

                var f1 = pl.Distance(start);
                var f2 = pl.Distance(soundOrigin);

                var f = f1 / (f1 - f2);
                source = start * (1f - f) + soundOrigin * f;
            }
            else source = soundOrigin;
#endif

                var tlen = source - soundOrigin;
                var tlenLength = tlen.LengthFast;

                ResolveOrigin(stackDepth + 1, newStack, otherArea, dist + tlenLength + occlusionDistance, source, def);
            }
        }

        // this is called from the main thread
        // if listenerPosition is NULL, this is being used for shader parameters, like flashing lights and glows based on sound level.Otherwise, it is being used for
        // the screen-shake on a player. This doesn't do the portal-occlusion currently, because it would have to reset all the defs which would be problematic in multiplayer
        public unsafe float FindAmplitude(SoundEmitterLocal sound, int localTime, Vector3? listenerPosition, int channel, bool shakesOnly)
        {
            const int AMPLITUDE_SAMPLES = Simd.MIXBUFFER_SAMPLES / 8;
            int i, j;
            SoundShaderParms parms;
            float volume;
            int activeChannelCount;
            var sourceBuffer = stackalloc float[AMPLITUDE_SAMPLES];
            var sumBuffer = stackalloc float[AMPLITUDE_SAMPLES];
            // work out the distance from the listener to the emitter
            float dlen;

            if (!sound.playing) return 0;

            if (listenerPosition != null)
            {
                // this doesn't do the portal spatialization
                var dist = sound.origin - listenerPosition.Value;
                dlen = dist.Length;
                dlen *= ISoundSystem.DOOM_TO_METERS;
            }
            else dlen = 1;

            activeChannelCount = 0;

            for (i = 0; i < SoundSystemLocal.SOUND_MAX_CHANNELS; i++)
            {
                var chan = sound.channels[i];

                if (!chan.triggerState) continue;

                if (channel != ISoundSystem.SCHANNEL_ANY && chan.triggerChannel != channel) continue;

                parms = chan.parms;

                var localTriggerTimes = chan.trigger44kHzTime;

                var looping = (parms.soundShaderFlags & ISoundSystem.SSF_LOOPING) != 0;

                // check for screen shakes
                var shakes = parms.shakes;
                if (shakesOnly && shakes <= 0f) continue;

                // calculate volume
                if (listenerPosition == null) volume = 1f; // just look at the raw wav data for light shader evaluation
                else
                {
                    volume = parms.volume;
                    volume = soundSystemLocal.dB2Scale(volume);
                    if (shakesOnly) volume *= shakes;

                    if (listenerPosition != null && (parms.soundShaderFlags & ISoundSystem.SSF_GLOBAL) == 0)
                    {
                        // check for overrides
                        var maxd = parms.maxDistance;
                        var mind = parms.minDistance;

                        if (dlen >= maxd) volume = 0f;
                        else if (dlen > mind)
                        {
                            var frac = MathX.ClampFloat(0, 1, 1f - ((dlen - mind) / (maxd - mind)));
                            if (SoundSystemLocal.s_quadraticFalloff.Bool) frac *= frac;
                            volume *= frac;
                        }
                    }
                }

                if (volume <= 0)
                    continue;

                // fetch the sound from the cache this doesn't handle stereo samples correctly...
                if (listenerPosition == null && (chan.parms.soundShaderFlags & ISoundSystem.SSF_NO_FLICKER) != 0)
                {
                    // the NO_FLICKER option is to allow a light to still play a sound, but not have it effect the intensity
                    for (j = 0; j < (AMPLITUDE_SAMPLES); j++) sourceBuffer[j] = (j & 1) != 0 ? 32767f : -32767f;
                }
                else
                {
                    var offset = localTime - localTriggerTimes;   // offset in samples
                    var size = looping ? chan.soundShader.entries[0].LengthIn44kHzSamples : chan.leadinSample.LengthIn44kHzSamples;
                    var amplitudeData = looping ? chan.soundShader.entries[0].amplitudeData : chan.leadinSample.amplitudeData;
                    if (amplitudeData != null)
                    {
                        // when the amplitudeData is present use that fill a dummy sourceBuffer this is to allow for amplitude based effect on hardware audio solutions
                        if (looping) offset %= size;
                        if (offset < size)
                            fixed (byte* amplitudeDataB = amplitudeData)
                            {
                                var amplitudeDataS = (short*)amplitudeDataB;
                                for (j = 0; j < AMPLITUDE_SAMPLES; j++) sourceBuffer[j] = (j & 1) != 0 ? amplitudeDataS[offset / 512 * 2] : amplitudeDataS[offset / 512 * 2 + 1];
                            }
                    }
                    // get actual sample data
                    else chan.GatherChannelSamples(offset, AMPLITUDE_SAMPLES, sourceBuffer);
                }
                activeChannelCount++;
                // store to the buffer
                if (activeChannelCount == 1) for (j = 0; j < AMPLITUDE_SAMPLES; j++) sumBuffer[j] = volume * sourceBuffer[j];
                // add to the buffer
                else for (j = 0; j < AMPLITUDE_SAMPLES; j++) sumBuffer[j] += volume * sourceBuffer[j];
            }

            if (activeChannelCount == 0) return 0f;

            var high = -32767f;
            var low = 32767f;

            // use a 20th of a second
            for (i = 0; i < AMPLITUDE_SAMPLES; i++)
            {
                var fabval = sumBuffer[i];
                if (high < fabval) high = fabval;
                if (low > fabval) low = fabval;
            }

            var sout = (float)Math.Atan((high - low) / 32767f) / MathX.DEG2RAD(45);

            return sout;
        }
    }
}

