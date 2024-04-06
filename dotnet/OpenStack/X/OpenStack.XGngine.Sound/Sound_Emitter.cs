using System.Linq;
using System.NumericsX.OpenAL;
using System.NumericsX.OpenStack.Gngine.Render;
using System.NumericsX.OpenStack.System;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.Gngine.Sound.Lib;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.Sound
{
    public enum REMOVE_STATUS // removeStatus_t;
    {
        INVALID = -1,
        ALIVE = 0,
        WAITSAMPLEFINISHED = 1,
        SAMPLEFINISHED = 2
    }

    public class SoundFade
    {
        public int fadeStart44kHz;
        public int fadeEnd44kHz;
        public float fadeStartVolume;      // in dB
        public float fadeEndVolume;            // in dB

        public void Clear()
        {
            fadeStart44kHz = 0;
            fadeEnd44kHz = 0;
            fadeStartVolume = 0;
            fadeEndVolume = 0;
        }

        public float FadeDbAt44kHz(int current44kHz)
        {
            float fadeDb;

            if (current44kHz >= fadeEnd44kHz) fadeDb = fadeEndVolume;
            else if (current44kHz > fadeStart44kHz)
            {
                float fraction = fadeEnd44kHz - fadeStart44kHz;
                float over = current44kHz - fadeStart44kHz;
                fadeDb = fadeStartVolume + (fadeEndVolume - fadeStartVolume) * over / fraction;
            }
            else fadeDb = fadeStartVolume;
            return fadeDb;
        }
    }

    public class FracTime
    {
        public int time;
        public float frac;

        public void Set(int val) { time = val; frac = 0; }
        public void Increment(float val) { frac += val; while (frac >= 1f) { time++; frac--; } }
    }

    public enum PLAYBACK
    {
        RESET,
        ADVANCING
    }

    public class SlowChannel
    {
        public int Id;

        bool active;
        SoundChannel chan;

        int playbackState;
        int triggerOffset;

        FracTime newPosition;
        int newSampleOffset;

        FracTime curPosition;
        int curSampleOffset;

        SoundFX_LowpassFast lowpass;

        // functions
        unsafe void GenerateSlowChannel(ref FracTime playPos, int sampleCount44k, float* finalBuffer)
        {
            int i, zeroedPos, count = 0;

            var sw = (SoundWorldLocal)soundSystemLocal.PlayingSoundWorld;

            var in_ = stackalloc float[Simd.MIXBUFFER_SAMPLES + 3]; var src = in_ + 2;
            var out_ = stackalloc float[Simd.MIXBUFFER_SAMPLES + 3]; var spline = out_ + 2;

            var slowmoSpeed = sw != null ? sw.slowmoSpeed : 1f;
            var neededSamples = (int)(sampleCount44k * slowmoSpeed + 4);

            // get the channel's samples
            chan.GatherChannelSamples(playPos.time * 2, neededSamples, src);
            for (i = 0; i < neededSamples >> 1; i++) spline[i] = src[i * 2];

            // interpolate channel
            zeroedPos = playPos.time;
            playPos.time = 0;

            for (i = 0; i < sampleCount44k >> 1; i++, count += 2)
            {
                var val = spline[playPos.time];
                src[i] = val;
                playPos.Increment(slowmoSpeed);
            }

            // lowpass filter
            var in_p = in_ + 2; var out_p = out_ + 2;
            var numSamples = sampleCount44k >> 1;

            lowpass.GetContinuitySamples(out in_p[-1], out in_p[-2], out out_p[-1], out out_p[-2]);
            lowpass.SetParms(slowmoSpeed * 15000, 1.2f);

            for (i = 0, count = 0; i < numSamples; i++, count += 2)
            {
                lowpass.ProcessSample(in_p + i, out_p + i);
                finalBuffer[count] = finalBuffer[count + 1] = out_[i];
            }

            lowpass.SetContinuitySamples(in_p[numSamples - 2], in_p[numSamples - 3], out_p[numSamples - 2], out_p[numSamples - 3]);

            playPos.time += zeroedPos;
        }

        float SlowmoSpeed
        {
            get
            {
                var sw = (SoundWorldLocal)soundSystemLocal.PlayingSoundWorld;
                return sw != null ? sw.slowmoSpeed : 0;
            }
        }

        public void AttachSoundChannel(SoundChannel chan)
            => this.chan = chan;

        public void Reset()
        {
            memset();
            //this.chan = chan;

            curPosition.Set(0);
            newPosition.Set(0);

            curSampleOffset = -10000;
            newSampleOffset = -10000;

            triggerOffset = 0;
        }

        private void memset()
        {
            throw new NotImplementedException();
        }

        public unsafe void GatherChannelSamples(int sampleOffset44k, int sampleCount44k, float* dest)
        {
            PLAYBACK state = 0;

            // setup chan
            active = true;
            newSampleOffset = sampleOffset44k >> 1;

            // set state
            if (newSampleOffset < curSampleOffset) state = PLAYBACK.RESET;
            else if (newSampleOffset > curSampleOffset) state = PLAYBACK.ADVANCING;

            if (state == PLAYBACK.RESET) curPosition.Set(newSampleOffset);

            // set current vars
            curSampleOffset = newSampleOffset;
            newPosition = curPosition;

            // do the slow processing
            GenerateSlowChannel(ref newPosition, sampleCount44k, dest);

            // finish off
            if (state == PLAYBACK.ADVANCING) curPosition = newPosition;
        }

        public bool IsActive
            => active;
        public FracTime CurrentPosition
            => curPosition;
    }

    public class SoundChannel : IDisposable
    {
        public int Id;
        public bool triggerState;
        public int trigger44kHzTime;       // hardware time sample the channel started
        public int triggerGame44kHzTime;   // game time sample time the channel started
        public SoundShaderParms parms;                   // combines the shader parms and the per-channel overrides
        public SoundSample leadinSample;            // if not looped, this is the only sample
        public int triggerChannel;
        public SoundShader soundShader;
        public ISampleDecoder decoder;
        public float diversity;
        public float lastVolume;               // last calculated volume based on distance
        public float[] lastV = new float[6];             // last calculated volume for each speaker, so we can smoothly fade
        public SoundFade channelFade;
        public bool triggered;
        public int openalSource;
        public int openalStreamingOffset;
        public int[] openalStreamingBuffer = new int[3];
        public int[] lastopenalStreamingBuffer = new int[3];
        public bool stopped;

        public bool disallowSlow;

        public SoundChannel()
        {
            decoder = null;
            Clear();
        }
        public void Dispose()
            => Clear();

        public void Clear()
        {
            Stop();
            soundShader = null;
            lastVolume = 0f;
            triggerChannel = ISoundSystem.SCHANNEL_ANY;
            channelFade.Clear();
            diversity = 0f;
            leadinSample = null;
            trigger44kHzTime = 0;
            stopped = false;
            for (var j = 0; j < 6; j++) lastV[j] = 0f;
            parms.memset();

            triggered = false;
            openalSource = 0;
            openalStreamingOffset = 0;
            openalStreamingBuffer[0] = openalStreamingBuffer[1] = openalStreamingBuffer[2] = 0;
            lastopenalStreamingBuffer[0] = lastopenalStreamingBuffer[1] = lastopenalStreamingBuffer[2] = 0;
        }

        public void Start()
        {
            triggerState = true;
            if (decoder == null) decoder = ISampleDecoder.Alloc();
        }

        public void Stop()
        {
            triggerState = false;
            stopped = true;
            if (decoder != null) { ISampleDecoder.Free(decoder); decoder = null; }
        }

        // free OpenAL resources if any
        public void ALStop()
        {
            if (AL.IsSource(openalSource))
            {
                AL.SourceStop(openalSource);
                AL.Source(openalSource, ALSourcei.Buffer, 0);
                soundSystemLocal.FreeOpenALSource(openalSource);
            }
            if (openalStreamingBuffer[0] != 0 && openalStreamingBuffer[1] != 0 && openalStreamingBuffer[2] != 0)
            {
                AL.GetError();
                AL.DeleteBuffers(3, ref openalStreamingBuffer[0]);
                if (AL.GetError() == ALError.NoError) openalStreamingBuffer[0] = openalStreamingBuffer[1] = openalStreamingBuffer[2] = 0;
            }
            if (lastopenalStreamingBuffer[0] != 0 && lastopenalStreamingBuffer[1] != 0 && lastopenalStreamingBuffer[2] != 0)
            {
                AL.GetError();
                AL.DeleteBuffers(3, ref lastopenalStreamingBuffer[0]);
                if (AL.GetError() == ALError.NoError) lastopenalStreamingBuffer[0] = lastopenalStreamingBuffer[1] = lastopenalStreamingBuffer[2] = 0;
            }
        }

        // Will always return 44kHz samples for the given range, even if it deeply looped or out of the range of the unlooped samples.  Handles looping between multiple different
        // samples and leadins
        public unsafe void GatherChannelSamples(int sampleOffset44k, int sampleCount44k, float* dest)
        {
            int len;

            //Sys_DebugPrintf( "msec:%i sample:%i : %i : %i\n", Sys_Milliseconds(), soundSystemLocal.GetCurrent44kHzTime(), sampleOffset44k, sampleCount44k );	//!@#
            var destF = dest;
            {
                var dest_p = destF;
                // negative offset times will just zero fill
                if (sampleOffset44k < 0)
                {
                    len = -sampleOffset44k;
                    if (len > sampleCount44k) len = sampleCount44k;
                    UnsafeX.InitBlock(dest_p, 0, len * sizeof(float));
                    dest_p += len;
                    sampleCount44k -= len;
                    sampleOffset44k += len;
                }

                // grab part of the leadin sample
                var leadin = leadinSample;
                if (leadin == null || sampleOffset44k < 0 || sampleCount44k <= 0) { UnsafeX.InitBlock(dest_p, 0, sampleCount44k * sizeof(float)); return; }

                if (sampleOffset44k < leadin.LengthIn44kHzSamples)
                {
                    len = leadin.LengthIn44kHzSamples - sampleOffset44k;
                    if (len > sampleCount44k) len = sampleCount44k;

                    // decode the sample
                    decoder.Decode(leadin, sampleOffset44k, len, dest_p);

                    dest_p += len;
                    sampleCount44k -= len;
                    sampleOffset44k += len;
                }

                // if not looping, zero fill any remaining spots
                if (soundShader == null || (parms.soundShaderFlags & ISoundSystem.SSF_LOOPING) == 0) { UnsafeX.InitBlock(dest_p, 0, sampleCount44k * sizeof(float)); return; }

                // fill the remainder with looped samples
                var loop = soundShader.entries[0];
                if (loop != null) { UnsafeX.InitBlock(dest_p, 0, sampleCount44k * sizeof(float)); return; }

                sampleOffset44k -= leadin.LengthIn44kHzSamples;
                while (sampleCount44k > 0)
                {
                    var totalLen = loop.LengthIn44kHzSamples;
                    sampleOffset44k %= totalLen;

                    len = totalLen - sampleOffset44k;
                    if (len > sampleCount44k) len = sampleCount44k;

                    // decode the sample
                    decoder.Decode(loop, sampleOffset44k, len, dest_p);

                    dest_p += len;
                    sampleCount44k -= len;
                    sampleOffset44k += len;
                }
            }
        }
    }

    public class SoundEmitterLocal : IDisposable, ISoundEmitter
    {
        public SoundEmitterLocal()
        {
            soundWorld = null;
            Clear();
        }
        public void Dispose()
            => Clear();

        //----------------------------------------------

        // the "time" parameters should be game time in msec, which is used to make queries return deterministic values regardless of async buffer scheduling
        // a non-immediate free will let all currently playing sounds complete
        // They are never truly freed, just marked so they can be reused by the soundWorld
        public virtual void Free(bool immediate)
        {
            if (removeStatus != REMOVE_STATUS.ALIVE) return;

            if (SoundSystemLocal.s_showStartSound.Integer != 0) common.Printf($"FreeSound ({index},{(immediate ? 1 : 0)})\n");
            if (soundWorld != null && soundWorld.writeDemo != null)
            {
                soundWorld.writeDemo.WriteInt((int)VFileDemo.DS.SOUND);
                soundWorld.writeDemo.WriteInt((int)SCMD.FREE);
                soundWorld.writeDemo.WriteInt(index);
                soundWorld.writeDemo.WriteInt(immediate ? 1 : 0);
            }
            if (!immediate) removeStatus = REMOVE_STATUS.WAITSAMPLEFINISHED;
            else Clear();
        }

        // the parms specified will be the default overrides for all sounds started on this emitter. null is acceptable for parms
        public virtual void UpdateEmitter(Vector3 origin, int listenerId, SoundShaderParms parms)
        {
            if (parms == null) common.Error("SoundEmitterLocal::UpdateEmitter: null parms");
            if (soundWorld != null && soundWorld.writeDemo != null)
            {
                soundWorld.writeDemo.WriteInt((int)VFileDemo.DS.SOUND);
                soundWorld.writeDemo.WriteInt((int)SCMD.UPDATE);
                soundWorld.writeDemo.WriteInt(index);
                soundWorld.writeDemo.WriteVec3(origin);
                soundWorld.writeDemo.WriteInt(listenerId);
                soundWorld.writeDemo.WriteFloat(parms.minDistance);
                soundWorld.writeDemo.WriteFloat(parms.maxDistance);
                soundWorld.writeDemo.WriteFloat(parms.volume);
                soundWorld.writeDemo.WriteFloat(parms.shakes);
                soundWorld.writeDemo.WriteInt(parms.soundShaderFlags);
                soundWorld.writeDemo.WriteInt(parms.soundClass);
            }

            this.origin = origin;
            this.listenerId = listenerId;
            this.parms = parms;
            // FIXME: change values on all channels?
        }

        // returns the length of the started sound in msec
        public virtual int StartSound(ISoundShader shader, int channel, float diversity = 0, int shaderFlags = 0, bool allowSlow = true /* D3XP */ )
        {
            int i;

            if (shader == null) return 0;
            var shader2 = (SoundShader)shader;

            if (SoundSystemLocal.s_showStartSound.Integer != 0) common.Printf($"StartSound {soundWorld.gameMsec}ms ({index},{channel},{shader2.Name}) = ");

            if (soundWorld != null && soundWorld.writeDemo != null)
            {
                soundWorld.writeDemo.WriteInt((int)VFileDemo.DS.SOUND);
                soundWorld.writeDemo.WriteInt((int)SCMD.START);
                soundWorld.writeDemo.WriteInt(index);
                soundWorld.writeDemo.WriteHashString(shader2.Name);
                soundWorld.writeDemo.WriteInt(channel);
                soundWorld.writeDemo.WriteFloat(diversity);
                soundWorld.writeDemo.WriteInt(shaderFlags);
            }

            // build the channel parameters by taking the shader parms and optionally overriding
            SoundShaderParms chanParms;

            chanParms = shader2.parms;
            OverrideParms(chanParms, this.parms, out chanParms);
            chanParms.soundShaderFlags |= shaderFlags;

            if (chanParms.shakes > 0f) shader2.CheckShakesAndOgg();

            // this is the sample time it will be first mixed
            var start44kHz = soundWorld.fpa[0] != null
                ? soundWorld.lastAVI44kHz + Simd.MIXBUFFER_SAMPLES // if we are recording an AVI demo, don't use hardware time
                : soundSystemLocal.Current44kHzTime + Simd.MIXBUFFER_SAMPLES;

            // pick which sound to play from the shader
            if (shader2.numEntries == 0)
            {
                if (SoundSystemLocal.s_showStartSound.Integer != 0) common.Printf("no samples in sound shader\n");
                return 0; // no sounds
            }

            // pick a sound from the list based on the passed diversity
            var choice = (int)(diversity * shader2.numEntries);
            if (choice < 0 || choice >= shader2.numEntries) choice = 0;

            // bump the choice if the exact sound was just played and we are NO_DUPS
            if ((chanParms.soundShaderFlags & ISoundSystem.SSF_NO_DUPS) != 0)
            {
                var sample = shader2.leadins[choice] ?? shader2.entries[choice];
                for (i = 0; i < SoundSystemLocal.SOUND_MAX_CHANNELS; i++)
                {
                    var chan2 = channels[i];
                    if (chan2.leadinSample == sample) { choice = (choice + 1) % shader2.numEntries; break; }
                }
            }

            // PLAY_ONCE sounds will never be restarted while they are running
            if ((chanParms.soundShaderFlags & ISoundSystem.SSF_PLAY_ONCE) != 0)
                for (i = 0; i < SoundSystemLocal.SOUND_MAX_CHANNELS; i++)
                {
                    var chan2 = channels[i];
                    if (chan2.triggerState && chan2.soundShader == shader2) { if (SoundSystemLocal.s_showStartSound.Integer != 0) common.Printf("PLAY_ONCE not restarting\n"); return 0; }
                }

            // never play the same sound twice with the same starting time, even if they are on different channels
            for (i = 0; i < SoundSystemLocal.SOUND_MAX_CHANNELS; i++)
            {
                var chan2 = channels[i];
                if (chan2.triggerState && chan2.soundShader == shader2 && chan2.trigger44kHzTime == start44kHz) { if (SoundSystemLocal.s_showStartSound.Integer != 0) common.Printf("already started this frame\n"); return 0; }
            }

            ISystem.EnterCriticalSection();

            // kill any sound that is currently playing on this channel
            if (channel != ISoundSystem.SCHANNEL_ANY)
                for (i = 0; i < SoundSystemLocal.SOUND_MAX_CHANNELS; i++)
                {
                    var chan2 = channels[i];
                    if (chan2.triggerState && chan2.soundShader != null && chan2.triggerChannel == channel)
                    {
                        if (SoundSystemLocal.s_showStartSound.Integer != 0) common.Printf($"(override {chan2.soundShader.base_.Name})");
                        chan2.Stop();
                        // if this was an onDemand sound, purge the sample now
                        if (chan2.leadinSample.onDemand) { chan2.ALStop(); chan2.leadinSample.PurgeSoundSample(); }
                        break;
                    }
                }

            // find a free channel to play the sound on
            SoundChannel chan;
            for (i = 0; i < SoundSystemLocal.SOUND_MAX_CHANNELS; i++)
            {
                chan = channels[i];
                if (!chan.triggerState) break;
            }

            if (i == SoundSystemLocal.SOUND_MAX_CHANNELS)
            {
                // we couldn't find a channel for it
                ISystem.LeaveCriticalSection();
                if (SoundSystemLocal.s_showStartSound.Integer != 0) common.Printf("no channels available\n");
                return 0;
            }

            chan = channels[i];
            chan.leadinSample = shader2.leadins[choice] != null ? shader2.leadins[choice] : shader2.entries[choice];

            // if the sample is onDemand (voice mails, etc), load it now
            if (chan.leadinSample.purged)
            {
                var start = SysW.Milliseconds;
                chan.leadinSample.Load();
                var end = SysW.Milliseconds;
                session.TimeHitch(end - start);
                // recalculate start44kHz, because loading may have taken a fair amount of time
                if (soundWorld.fpa[0] == null) start44kHz = soundSystemLocal.Current44kHzTime + Simd.MIXBUFFER_SAMPLES;
            }

            if (SoundSystemLocal.s_showStartSound.Integer != 0) common.Printf($"'{chan.leadinSample.name}'\n");

            chan.disallowSlow = SoundSystemLocal.s_skipHelltimeFX.Bool || !allowSlow;

            ResetSlowChannel(chan);

            // the sound will start mixing in the next async mix block
            chan.triggered = true;
            chan.openalStreamingOffset = 0;
            chan.trigger44kHzTime = start44kHz;
            chan.parms = chanParms;
            chan.triggerGame44kHzTime = soundWorld.game44kHz;
            chan.soundShader = shader2;
            chan.triggerChannel = channel;
            chan.stopped = false;
            chan.Start();

            // we need to start updating the def and mixing it in
            playing = true;

            // spatialize it immediately, so it will start the next mix block
            // even if that happens before the next PlaceOrigin()
            Spatialize(soundWorld.listenerPos, soundWorld.listenerArea, soundWorld.rw);

            // return length of sound in milliseconds
            var length = chan.leadinSample.LengthIn44kHzSamples;

            if (chan.leadinSample.objectInfo.nChannels == 2) length /= 2;    // stereo samples

            // adjust the start time based on diversity for looping sounds, so they don't all start at the same point
            if ((chan.parms.soundShaderFlags & ISoundSystem.SSF_LOOPING) != 0 && chan.leadinSample.LengthIn44kHzSamples == 0)
            {
                chan.trigger44kHzTime -= (int)(diversity * length);
                chan.trigger44kHzTime &= ~7;        // so we don't have to worry about the 22kHz and 11kHz expansions
                                                    // starting in fractional samples
                chan.triggerGame44kHzTime -= (int)(diversity * length);
                chan.triggerGame44kHzTime &= ~7;
            }

            length *= (int)(1000 / (float)SoundSystemLocal.PRIMARYFREQ);

            ISystem.LeaveCriticalSection();
            return length;
        }

        // can pass SCHANNEL_ANY
        public virtual void ModifySound(int channel, SoundShaderParms parms)
        {
            if (parms == null) common.Error("SoundEmitterLocal::ModifySound: null parms");
            if (SoundSystemLocal.s_showStartSound.Integer != 0) common.Printf($"ModifySound({index},{channel})\n");
            if (soundWorld != null && soundWorld.writeDemo != null)
            {
                soundWorld.writeDemo.WriteInt((int)VFileDemo.DS.SOUND);
                soundWorld.writeDemo.WriteInt((int)SCMD.MODIFY);
                soundWorld.writeDemo.WriteInt(index);
                soundWorld.writeDemo.WriteInt(channel);
                soundWorld.writeDemo.WriteFloat(parms.minDistance);
                soundWorld.writeDemo.WriteFloat(parms.maxDistance);
                soundWorld.writeDemo.WriteFloat(parms.volume);
                soundWorld.writeDemo.WriteFloat(parms.shakes);
                soundWorld.writeDemo.WriteInt(parms.soundShaderFlags);
                soundWorld.writeDemo.WriteInt(parms.soundClass);
            }

            for (var i = 0; i < SoundSystemLocal.SOUND_MAX_CHANNELS; i++)
            {
                var chan = channels[i];

                if (!chan.triggerState) continue;
                if (channel != ISoundSystem.SCHANNEL_ANY && chan.triggerChannel != channel) continue;

                OverrideParms(chan.parms, parms, out chan.parms);

                if (chan.parms.shakes > 0f && chan.soundShader != null) chan.soundShader.CheckShakesAndOgg();
            }
        }

        // can pass SCHANNEL_ANY
        public virtual void StopSound(int channel)
        {
            int i;

            if (SoundSystemLocal.s_showStartSound.Integer != 0) common.Printf($"StopSound({index},{channel})\n");

            if (soundWorld != null && soundWorld.writeDemo != null)
            {
                soundWorld.writeDemo.WriteInt((int)VFileDemo.DS.SOUND);
                soundWorld.writeDemo.WriteInt((int)SCMD.STOP);
                soundWorld.writeDemo.WriteInt(index);
                soundWorld.writeDemo.WriteInt(channel);
            }

            ISystem.EnterCriticalSection();
            for (i = 0; i < SoundSystemLocal.SOUND_MAX_CHANNELS; i++)
            {
                var chan = channels[i];

                if (!chan.triggerState) continue;
                if (channel != ISoundSystem.SCHANNEL_ANY && chan.triggerChannel != channel) continue;

                // stop it
                chan.Stop();

                // free hardware resources
                chan.ALStop();

                // if this was an onDemand sound, purge the sample now
                if (chan.leadinSample.onDemand) chan.leadinSample.PurgeSoundSample();

                chan.leadinSample = null;
                chan.soundShader = null;
            }
            ISystem.LeaveCriticalSection();
        }

        // to is in Db (sigh), over is in seconds
        public virtual void FadeSound(int channel, float to, float over)
        {
            if (SoundSystemLocal.s_showStartSound.Integer != 0) common.Printf($"FadeSound({index},{channel},{to},{over})\n");
            if (soundWorld == null) return;
            if (soundWorld.writeDemo != null)
            {
                soundWorld.writeDemo.WriteInt((int)VFileDemo.DS.SOUND);
                soundWorld.writeDemo.WriteInt((int)SCMD.FADE);
                soundWorld.writeDemo.WriteInt(index);
                soundWorld.writeDemo.WriteInt(channel);
                soundWorld.writeDemo.WriteFloat(to);
                soundWorld.writeDemo.WriteFloat(over);
            }

            var start44kHz = soundWorld.fpa[0] != null
                // if we are recording an AVI demo, don't use hardware time
                ? soundWorld.lastAVI44kHz + Simd.MIXBUFFER_SAMPLES
                : soundSystemLocal.Current44kHzTime + Simd.MIXBUFFER_SAMPLES;

            var length44kHz = soundSystemLocal.MillisecondsToSamples((int)(over * 1000));

            for (var i = 0; i < SoundSystemLocal.SOUND_MAX_CHANNELS; i++)
            {
                var chan = channels[i];
                if (!chan.triggerState) continue;
                if (channel != ISoundSystem.SCHANNEL_ANY && chan.triggerChannel != channel) continue;
                // if it is already fading to this volume at this rate, don't change it
                if (chan.channelFade.fadeEndVolume == to && chan.channelFade.fadeEnd44kHz - chan.channelFade.fadeStart44kHz == length44kHz) continue;

                // fade it
                chan.channelFade.fadeStartVolume = chan.channelFade.FadeDbAt44kHz(start44kHz);
                chan.channelFade.fadeStart44kHz = start44kHz;
                chan.channelFade.fadeEnd44kHz = start44kHz + length44kHz;
                chan.channelFade.fadeEndVolume = to;
            }
        }

        public virtual bool CurrentlyPlaying
            => playing;

        // can pass SCHANNEL_ANY
        // this is called from the main thread by the material shader system to allow lights and surface flares to vary with the sound amplitude
        public virtual float CurrentAmplitude
        {
            get
            {
                if (SoundSystemLocal.s_constantAmplitude.Float >= 0f) return SoundSystemLocal.s_constantAmplitude.Float;
                if (removeStatus > REMOVE_STATUS.WAITSAMPLEFINISHED) return 0f;
                var localTime = soundSystemLocal.Current44kHzTime;
                // see if we can use our cached value
                if (ampTime == localTime) return amplitude;
                // calculate a new value
                ampTime = localTime;
                amplitude = soundWorld.FindAmplitude(this, localTime, null, ISoundSystem.SCHANNEL_ANY, false);
                return amplitude;
            }
        }

        // used for save games
        public virtual int Index
            => index;

        //----------------------------------------------

        public void Clear()
        {
            for (var i = 0; i < SoundSystemLocal.SOUND_MAX_CHANNELS; i++) { channels[i].ALStop(); channels[i].Clear(); }

            removeStatus = REMOVE_STATUS.SAMPLEFINISHED;
            distance = 0f;

            lastValidPortalArea = -1;

            playing = false;
            hasShakes = false;
            ampTime = 0;                                // last time someone queried
            amplitude = 0;
            maxDistance = 10f;                        // meters
            spatializedOrigin.Zero();

            parms.memset();
        }

        public void OverrideParms(SoundShaderParms base_, SoundShaderParms over, out SoundShaderParms o)
        {
            if (over == null) { o = base_; return; }
            o = new SoundShaderParms
            {
                minDistance = over.minDistance != 0 ? over.minDistance : base_.minDistance,
                maxDistance = over.maxDistance != 0 ? over.maxDistance : base_.maxDistance,
                shakes = over.shakes != 0 ? over.shakes : base_.shakes,
                volume = over.volume != 0 ? over.volume : base_.volume,
                soundClass = over.soundClass != 0 ? over.soundClass : base_.soundClass,
                soundShaderFlags = base_.soundShaderFlags | over.soundShaderFlags
            };
        }

        // Checks to see if all the channels have completed, clearing the playing flag if necessary. Sets the playing and shakes bools.
        public void CheckForCompletion(int current44kHzTime)
        {
            bool hasActive;
            int i;

            hasActive = false;
            hasShakes = false;

            if (playing)
            {
                for (i = 0; i < SoundSystemLocal.SOUND_MAX_CHANNELS; i++)
                {
                    var chan = channels[i];

                    if (!chan.triggerState) continue;
                    var shader = chan.soundShader;
                    if (shader == null) continue;

                    // see if this channel has completed
                    if ((chan.parms.soundShaderFlags & ISoundSystem.SSF_LOOPING) == 0)
                    {
                        var state = (int)ALSourceState.Playing;
                        if (AL.IsSource(chan.openalSource)) AL.GetSource(chan.openalSource, ALGetSourcei.SourceState, out state);
                        var slow = GetSlowChannel(chan);
                        if (soundWorld.slowmoActive && slow.IsActive)
                        {
                            if (slow.CurrentPosition.time >= chan.leadinSample.LengthIn44kHzSamples / 2)
                            {
                                chan.Stop();
                                // if this was an onDemand sound, purge the sample now
                                if (chan.leadinSample.onDemand) chan.leadinSample.PurgeSoundSample();
                            }
                        }
                        else if (chan.trigger44kHzTime + chan.leadinSample.LengthIn44kHzSamples < current44kHzTime || chan.stopped)
                        {
                            chan.Stop();

                            // free hardware resources
                            chan.ALStop();

                            // if this was an onDemand sound, purge the sample now
                            if (chan.leadinSample.onDemand) chan.leadinSample.PurgeSoundSample();
                        }
                    }

                    // free decoder memory if no sound was decoded for a while
                    if (chan.decoder != null && chan.decoder.LastDecodeTime < current44kHzTime - SoundSystemLocal.SOUND_DECODER_FREE_DELAY) chan.decoder.ClearDecoder();

                    hasActive = true;
                    if (chan.parms.shakes > 0f) hasShakes = true;
                }
            }

            // mark the entire sound emitter as non-playing if there aren't any active channels
            if (!hasActive)
            {
                playing = false;
                // this can now be reused by the next request for a new soundEmitter
                if (removeStatus == REMOVE_STATUS.WAITSAMPLEFINISHED) removeStatus = REMOVE_STATUS.SAMPLEFINISHED;
            }
        }

        // Called once each sound frame by the main thread from SoundWorldLocal::PlaceOrigin
        public void Spatialize(Vector3 listenerPos, int listenerArea, IRenderWorld rw)
        {
            // work out the maximum distance of all the playing channels
            maxDistance = 0;

            for (var i = 0; i < SoundSystemLocal.SOUND_MAX_CHANNELS; i++)
            {
                var chan = channels[i];
                if (!chan.triggerState) continue;
                if (chan.parms.maxDistance > maxDistance) maxDistance = chan.parms.maxDistance;
            }

            // work out where the sound comes from
            var realOrigin = origin * ISoundSystem.DOOM_TO_METERS;
            var len = listenerPos - realOrigin;
            realDistance = len.LengthFast;

            // no way to possibly hear it
            if (realDistance >= maxDistance) { distance = realDistance; return; }

            // work out virtual origin and distance, which may be from a portal instead of the actual origin
            distance = maxDistance * ISoundSystem.METERS_TO_DOOM;
            // listener is outside the world
            if (listenerArea == -1) return;
            if (rw != null)
            {
                // we have a valid renderWorld
                var soundInArea = rw.PointInArea(origin);
                if (soundInArea == -1)
                {
                    // sound is outside the world
                    if (lastValidPortalArea == -1) { distance = realDistance; spatializedOrigin = origin; return; }
                    soundInArea = lastValidPortalArea; // sound is in our area
                }
                lastValidPortalArea = soundInArea;
                if (soundInArea == listenerArea) { distance = realDistance; spatializedOrigin = origin; return; } // sound is in our area

                soundWorld.ResolveOrigin(0, null, soundInArea, 0f, origin, this);
                distance /= ISoundSystem.METERS_TO_DOOM;
            }
            // no portals available
            else { distance = realDistance; spatializedOrigin = origin; } // sound is in our area
        }

        public SoundWorldLocal soundWorld;              // the world that holds this emitter

        public int index;                               // in world emitter list
        public REMOVE_STATUS removeStatus;

        public Vector3 origin;
        public int listenerId;
        public SoundShaderParms parms;                       // default overrides for all channels

        // the following are calculated in UpdateEmitter, and don't need to be archived
        public float maxDistance;              // greatest of all playing channel distances
        public int lastValidPortalArea;        // so an emitter that slides out of the world continues playing
        public bool playing;                   // if false, no channel is active
        public bool hasShakes;
        public Vector3 spatializedOrigin;      // the virtual sound origin, either the real sound origin, or a point through a portal chain
        public float realDistance;             // in meters
        public float distance;                 // in meters, this may be the straight-line distance, or it may go through a chain of portals.  If there is not an open-portal path, distance will be > maxDistance

        // a single soundEmitter can have many channels playing from the same point
        public SoundChannel[] channels = Enumerable.Range(0, SoundSystemLocal.SOUND_MAX_CHANNELS).Select(id => new SoundChannel { Id = id }).ToArray();
        public SlowChannel[] slowChannels = Enumerable.Range(0, SoundSystemLocal.SOUND_MAX_CHANNELS).Select(id => new SlowChannel { Id = id }).ToArray();

        public SlowChannel GetSlowChannel(SoundChannel chan)
            => slowChannels[chan.Id];

        public void SetSlowChannel(SoundChannel chan, SlowChannel slow)
            => slowChannels[chan.Id] = slow;

        public void ResetSlowChannel(SoundChannel chan)
            => slowChannels[chan.Id].Reset();

        // this is just used for feedback to the game or rendering system: flashing lights and screen shakes.  Because the material expression
        // evaluation doesn't do common subexpression removal, we cache the last generated value
        public int ampTime;
        public float amplitude;

        #region PermuteList

        //Fills in elements[0] .. elements[numElements-1] with a permutation of 0 .. numElements-1 based on the permute parameter
        //numElements == 3
        //maxPermute = 6
        //permute 0 = 012
        //permute 1 = 021
        //permute 2 = 102
        //permute 3 = 120
        //permute 4 = 201
        //permute 5 = 210
        static void PermuteList_r(int[] list, int offset, int listLength, int permute, int maxPermute)
        {
            if (listLength < 2) return;
            permute %= maxPermute;
            var swap = permute * listLength / maxPermute;
            var old = list[offset + swap]; list[offset + swap] = list[offset]; list[offset] = old;

            maxPermute /= listLength;
            PermuteList_r(list, offset + 1, listLength - 1, permute, maxPermute);
        }

        static int Factorial(int val)
        {
            var fact = val;
            while (val > 1) { val--; fact *= val; }
            return fact;
        }

        static void GeneratePermutedList(int[] list, int listLength, int permute)
        {
            for (var i = 0; i < listLength; i++) list[i] = i;

            // we can't calculate > 12 factorial, so we can't easily build a permuted list
            if (listLength > 12) return;

            // calculate listLength factorial
            var maxPermute = Factorial(listLength);

            // recursively permute
            PermuteList_r(list, 0, listLength, permute, maxPermute);
        }

        static void TestPermutations()
        {
            var list = new int[ISoundSystem.SOUND_MAX_LIST_WAVS];
            for (var len = 1; len < 5; len++)
            {
                common.Printf($"list length: {len}\n");

                var max = Factorial(len);
                for (var j = 0; j < max * 2; j++)
                {
                    GeneratePermutedList(list, len, j);
                    common.Printf($"{j:4} : ");
                    for (var k = 0; k < len; k++) common.Printf(list[k].ToString());
                    common.Printf("\n");
                }
            }
        }

        #endregion
    }
}