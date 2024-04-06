using System.Collections.Generic;
using System.Diagnostics;
using System.NumericsX.OpenAL;
using System.NumericsX.OpenAL.Extensions.Creative.EFX;
using System.NumericsX.OpenAL.Extensions.SOFT.HRTF;
using System.NumericsX.OpenStack.Gngine.Framework;
using System.NumericsX.OpenStack.Gngine.Render;
using System.NumericsX.OpenStack.System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.Gngine.Sound.Lib;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.Sound
{
    public unsafe abstract class SoundFX
    {
        protected bool initialized;

        protected int channel;
        protected int maxlen;

        protected float[] buffer;
        protected float[] continuitySamples = new float[4];

        protected float param;

        public SoundFX() { channel = 0; buffer = null; initialized = false; maxlen = 0; Array.Clear(continuitySamples, 0, 4); }

        public virtual void Initialize() { }

        public abstract void ProcessSample(float* i, float* o);

        public int Channel
        {
            get => channel;
            set => channel = value;
        }

        public void SetContinuitySamples(float in1, float in2, float out1, float out2) { continuitySamples[0] = in1; continuitySamples[1] = in2; continuitySamples[2] = out1; continuitySamples[3] = out2; }      // FIXME?
        public void GetContinuitySamples(out float in1, out float in2, out float out1, out float out2) { in1 = continuitySamples[0]; in2 = continuitySamples[1]; out1 = continuitySamples[2]; out2 = continuitySamples[3]; }

        public float Parameter
        {
            set => param = value;
        }
    }

    public struct OpenalSource
    {
        public int handle;
        public int startTime;
        public SoundChannel chan;
        public bool inUse;
        public bool looping;
        public bool stereo;
    }

    public unsafe class SoundSystemLocal : ISoundSystem
    {
        public static int mmioFOURCC(char ch0, char ch1, char ch2, char ch3) => ch0 | ch1 << 8 | ch2 << 16 | ch3 << 24;
        public const int fourcc_riff = 'R' | 'I' << 8 | 'F' << 16 | 'F' << 24;

        public const int SOUND_MAX_CHANNELS = 8;
        public const int SOUND_DECODER_FREE_DELAY = 1000 * Simd.MIXBUFFER_SAMPLES / IUsercmd.USERCMD_MSEC;       // four seconds
        public const int PRIMARYFREQ = 44100;          // samples per second
        public const float SND_EPSILON = 1f / 32768f;  // if volume is below this, it will always multiply to zero
        public const int ROOM_SLICES_IN_BUFFER = 10;

        public SoundSystemLocal()
            => isInitialized = false;

        // all non-hardware initialization. initialize the sound system
        public virtual void Init()
        {
            common.Printf("----- Initializing OpenAL -----\n");

            isInitialized = false;
            muted = false;
            shutdown = false;

            currentSoundWorld = null;
            soundCache = null;

            olddwCurrentWritePos = 0;
            buffers = 0;
            CurrentSoundTime = 0;

            nextWriteBlock = 0xffffffff;

            Array.Clear(meterTops, 0, meterTops.Length);
            Array.Clear(meterTopsTime, 0, meterTopsTime.Length);

            for (var i = -600; i < 600; i++) { var pt = i * 0.1f; volumesDB[i + 600] = (float)Math.Pow(2f, pt * (1f / 6f)); }

            // make a 16 byte aligned finalMixBuffer
            finalMixBuffer = realAccum; // (float*)((((intptr_t)realAccum) + 15) & ~15);

            graph = null;

            // DG: added these for CheckDeviceAndRecoverIfNeeded()
            alcResetDeviceSOFT = null;
            resetRetryCount = 0;
            lastCheckTime = 0;

            // DG: no point in initializing OpenAL if sound is disabled with s_noSound
            if (s_noSound.Bool) { common.Printf("Sound disabled with s_noSound 1 !\n"); openalDevice = ALDevice.Null; openalContext = ALContext.Null; }
            else
            {
                // set up openal device and context
                common.Printf("Setup OpenAL device and context\n");

                var device = s_device.String;
                if (device.Length < 1) device = null;
                else if (string.Equals(device, "default", StringComparison.OrdinalIgnoreCase)) device = null;

                if (ALC.IsExtensionPresent(ALDevice.Null, "ALC_ENUMERATE_ALL_EXT"))
                {
                    var devs = ALC.GetString(ALDevice.Null, AlcGetStringList.AllDevicesSpecifier);
                    var found = false;
                    foreach (var dev in devs)
                    {
                        common.Printf($"OpenAL: found device '{dev}'");
                        if (device != null && string.Equals(dev, device, StringComparison.OrdinalIgnoreCase)) { common.Printf(" (ACTIVE)\n"); found = true; }
                        else common.Printf("\n");
                    }

                    if (device != null && !found) { common.Printf($"OpenAL: device {device} not found, using default\n"); device = null; }
                }

                openalDevice = ALC.OpenDevice(device);
                if (openalDevice == ALDevice.Null && device != null) { common.Printf($"OpenAL: failed to open device '{device}' (0x{AL.GetError():x}), trying default...\n"); openalDevice = ALC.OpenDevice(null); }

                // DG: handle the possibility that opening the default device or creating context failed
                if (openalDevice == ALDevice.Null) { common.Printf($"OpenAL: failed to open default device (0x{AL.GetError():x}), disabling sound\n"); openalContext = ALContext.Null; }
                else
                {
                    openalContext = ALC.CreateContext(openalDevice, (int[])null);
                    if (openalContext.Handle == IntPtr.Zero) { common.Printf($"OpenAL: failed to create context (0x{ALC.GetError(openalDevice):x}), disabling sound\n"); ALC.CloseDevice(openalDevice); openalDevice = ALDevice.Null; }
                }
            }

            // DG: only do these things if opening device and creating context succeeded and sound is enabled (if sound is disabled with s_noSound, openalContext is NULL)
            if (openalContext.Handle != IntPtr.Zero)
            {
                ISampleDecoder.Init();
                soundCache = new SoundCache();

                ALC.MakeContextCurrent(openalContext);

                // log openal info
                common.Printf($"OpenAL vendor: {AL.Get(ALGetString.Vendor)}\n");
                common.Printf($"OpenAL renderer: {AL.Get(ALGetString.Renderer)}\n");
                common.Printf($"OpenAL version: {AL.Get(ALGetString.Version)}\n");

                // DG: extensions needed for CheckDeviceAndRecoverIfNeeded()
                var hasAlcExtDisconnect = ALC.IsExtensionPresent(openalDevice, "ALC_EXT_disconnect");
                var hasAlcSoftHrtf = ALC.IsExtensionPresent(openalDevice, "ALC_SOFT_HRTF");
                if (hasAlcExtDisconnect && hasAlcSoftHrtf)
                {
                    common.Printf("OpenAL: found extensions for resetting disconnected devices\n");
                    alcResetDeviceSOFT = ALBase.LoadDelegate<HRTF.ResetDeviceSoftArrayDelegate>(openalDevice, "alcResetDeviceSOFT");
                }

                // try to obtain EFX extensions
                if (ALC.IsExtensionPresent(openalDevice, "ALC_EXT_EFX"))
                {
                    common.Printf("OpenAL: found EFX extension\n");
                    EFXAvailable = 1;

                    alGenEffects = ALBase.LoadDelegate<EFX.GenEffectsRefDelegate>("alGenEffects");
                    alDeleteEffects = ALBase.LoadDelegate<EFX.DeleteEffectsRefDelegate>("alDeleteEffects");
                    alIsEffect = ALBase.LoadDelegate<EFX.IsEffectDelegate>("alIsEffect");
                    alEffecti = ALBase.LoadDelegate<EFX.EffectiDelegate>("alEffecti");
                    alEffectf = ALBase.LoadDelegate<EFX.EffectfDelegate>("alEffectf");
                    alEffectfv = ALBase.LoadDelegate<EFX.EffectfvArrayDelegate>("alEffectfv");
                    alGenFilters = ALBase.LoadDelegate<EFX.GenFiltersRefDelegate>("alGenFilters");
                    alDeleteFilters = ALBase.LoadDelegate<EFX.DeleteFiltersRefDelegate>("alDeleteFilters");
                    alIsFilter = ALBase.LoadDelegate<EFX.IsFilterDelegate>("alIsFilter");
                    alFilteri = ALBase.LoadDelegate<EFX.FilteriDelegate>("alFilteri");
                    alFilterf = ALBase.LoadDelegate<EFX.FilterfDelegate>("alFilterf");
                    alGenAuxiliaryEffectSlots = ALBase.LoadDelegate<EFX.GenAuxiliaryEffectSlotsRefDelegate>("alGenAuxiliaryEffectSlots");
                    alDeleteAuxiliaryEffectSlots = ALBase.LoadDelegate<EFX.DeleteAuxiliaryEffectSlotsRefDelegate>("alDeleteAuxiliaryEffectSlots");
                    alIsAuxiliaryEffectSlot = ALBase.LoadDelegate<EFX.IsAuxiliaryEffectSlotDelegate>("alIsAuxiliaryEffectSlot"); ;
                    alAuxiliaryEffectSloti = ALBase.LoadDelegate<EFX.AuxiliaryEffectSlotiDelegate>("alAuxiliaryEffectSloti");
                }
                else
                {
                    common.Printf("OpenAL: EFX extension not found\n");
                    EFXAvailable = 0;
                    s_useEAXReverb.Bool = false;

                    alGenEffects = null;
                    alDeleteEffects = null;
                    alIsEffect = null;
                    alEffecti = null;
                    alEffectf = null;
                    alEffectfv = null;
                    alGenFilters = null;
                    alDeleteFilters = null;
                    alIsFilter = null;
                    alFilteri = null;
                    alFilterf = null;
                    alGenAuxiliaryEffectSlots = null;
                    alDeleteAuxiliaryEffectSlots = null;
                    alIsAuxiliaryEffectSlot = null;
                    alAuxiliaryEffectSloti = null;
                }

                var handle = 0;
                openalSourceCount = 0;

                while (openalSourceCount < 256)
                {
                    AL.GetError();
                    AL.GenSources(1, ref handle);
                    if (AL.GetError() != ALError.NoError) break;
                    else
                    {
                        // store in source array
                        openalSources[openalSourceCount].handle = handle;
                        openalSources[openalSourceCount].startTime = 0;
                        openalSources[openalSourceCount].chan = null;
                        openalSources[openalSourceCount].inUse = false;
                        openalSources[openalSourceCount].looping = false;

                        // initialise sources
                        AL.Source(handle, ALSourcef.RolloffFactor, 0f);

                        // found one source
                        openalSourceCount++;
                    }
                }

                common.Printf($"OpenAL: found {openalSourceCount} hardware voices\n");

                // adjust source count to allow for at least eight stereo sounds to play
                openalSourceCount -= 8;

                useEFXReverb = s_useEAXReverb.Bool;
                efxloaded = false;
            }

            cmdSystem.AddCommand("listSounds", ListSounds_f, CMD_FL.SOUND, "lists all sounds");
            cmdSystem.AddCommand("listSoundDecoders", ListSoundDecoders_f, CMD_FL.SOUND, "list active sound decoders");
            cmdSystem.AddCommand("reloadSounds", SoundReloadSounds_f, CMD_FL.SOUND | CMD_FL.CHEAT, "reloads all sounds");
            cmdSystem.AddCommand("testSound", TestSound_f, CMD_FL.SOUND | CMD_FL.CHEAT, "tests a sound", CmdArgs.ArgCompletion_SoundName);
            cmdSystem.AddCommand("s_restart", SoundSystemRestart_f, CMD_FL.SOUND, "restarts the sound system");
        }

        // shutdown routine
        public virtual void Shutdown()
        {
            ShutdownHW();

            // EFX or not, the list needs to be cleared
            EFXDatabase.Clear();

            efxloaded = false;

            // adjust source count back up to allow for freeing of all resources
            openalSourceCount += 8;

            for (var i = 0; i < openalSourceCount; i++)
            {
                // stop source
                AL.SourceStop(openalSources[i].handle);
                AL.Source(openalSources[i].handle, ALSourcei.Buffer, 0);

                // delete source
                AL.DeleteSources(1, ref openalSources[i].handle);

                // clear entry in source array
                openalSources[i].handle = 0;
                openalSources[i].startTime = 0;
                openalSources[i].chan = null;
                openalSources[i].inUse = false;
                openalSources[i].looping = false;
            }

            // destroy all the sounds (hardware buffers as well)
            soundCache = null;

            // destroy openal device and context
            ALC.MakeContextCurrent(ALContext.Null);
            ALC.DestroyContext(openalContext); openalContext = ALContext.Null;
            ALC.CloseDevice(openalDevice); openalDevice = ALDevice.Null;

            ISampleDecoder.Shutdown();
        }

        // sound is attached to the window, and must be recreated when the window is changed
        public virtual bool InitHW()
        {
            var numSpeakers = s_numberOfSpeakers.Integer;
            if (numSpeakers != 2 && numSpeakers != 6)
            {
                common.Warning("invalid value for s_numberOfSpeakers. Use either 2 or 6");
                numSpeakers = 2;
                s_numberOfSpeakers.Integer = numSpeakers;
            }

            // DG: if OpenAL context couldn't be created (maybe there were no audio devices), keep audio disabled.
            if (s_noSound.Bool || openalContext == ALContext.Null)
                return false;

            // put the real number in there
            s_numberOfSpeakers.Integer = numSpeakers;

            isInitialized = true;
            shutdown = false;

            return true;
        }

        public virtual bool ShutdownHW()
        {
            if (!isInitialized) return false;

            shutdown = true;        // don't do anything at AsyncUpdate() time
            SysW.Sleep(100);     // sleep long enough to make sure any async sound talking to hardware has returned

            common.Printf("Shutting down sound hardware\n");

            isInitialized = false;

            if (graph != null) { Marshal.FreeHGlobal((IntPtr)graph); graph = null; }
            return true;
        }

        // async loop, called at 60Hz
        // called from async sound thread when com_asyncSound == 2
        // DG: using this for the "traditional" sound updates that only happen about every 100ms(and lead to delays between 1 and 110ms between
        // starting a sound in gamecode and it being played), for people who like that..
        public virtual int AsyncUpdate(int time)
        {
            if (!isInitialized || shutdown) return 0;

            // here we do it in samples ( overflows in 27 hours or so )
            var dwCurrentWritePos = MathX.Ftol(SysW.Milliseconds * 44.1f) % (Simd.MIXBUFFER_SAMPLES * ROOM_SLICES_IN_BUFFER);
            var dwCurrentBlock = dwCurrentWritePos / Simd.MIXBUFFER_SAMPLES;

            if (nextWriteBlock == 0xffffffff) nextWriteBlock = dwCurrentBlock;
            if (dwCurrentBlock != nextWriteBlock) return 0;

            soundStats.runs++;
            soundStats.activeSounds = 0;

            var numSpeakers = s_numberOfSpeakers.Integer;

            nextWriteBlock++;
            nextWriteBlock %= ROOM_SLICES_IN_BUFFER;

            var newPosition = nextWriteBlock * Simd.MIXBUFFER_SAMPLES;
            if (newPosition < olddwCurrentWritePos) buffers++; // buffer wrapped

            // nextWriteSample is in multi-channel samples inside the buffer
            var nextWriteSamples = nextWriteBlock * Simd.MIXBUFFER_SAMPLES;

            olddwCurrentWritePos = newPosition;

            // newSoundTime is in multi-channel samples since the sound system was started
            var newSoundTime = (int)((buffers * Simd.MIXBUFFER_SAMPLES * ROOM_SLICES_IN_BUFFER) + nextWriteSamples);

            // check for impending overflow
            // FIXME: we don't handle sound wrap-around correctly yet
            if (newSoundTime > 0x6fffffff) buffers = 0;

            if (newSoundTime - CurrentSoundTime > Simd.MIXBUFFER_SAMPLES) soundStats.missedWindow++;

            // enable audio hardware caching
            ALC.SuspendContext(openalContext);

            // let the active sound world mix all the channels in unless muted or avi demo recording
            if (!muted && currentSoundWorld != null && currentSoundWorld.fpa[0] == null)
                fixed (float* finalMixBuffer_ = finalMixBuffer) currentSoundWorld.MixLoop(newSoundTime, numSpeakers, finalMixBuffer_);

            // disable audio hardware caching (this updates ALL settings since last alcSuspendContext)
            ALC.ProcessContext(openalContext);

            CurrentSoundTime = newSoundTime;

            soundStats.timeinprocess = SysW.Milliseconds - time;

            return soundStats.timeinprocess;
        }

        // async loop, when the sound driver uses a write strategy
        // DG: using this now for 60Hz sound updates called from async sound thread when com_asyncSound is 3 or 1
        // also called from main thread if com_asyncSound == 0 (those were the default values used in dhewm3 on unix-likes (except mac) or rest)
        // with this, once every async tic new sounds are started and existing ones updated, instead of once every ~100ms.
        public virtual int AsyncUpdateWrite(int time)
        {
            if (!isInitialized || shutdown) return 0;

            var sampleTime = (int)(time * 44.1f);
            var numSpeakers = s_numberOfSpeakers.Integer;

            // enable audio hardware caching
            ALC.SuspendContext(openalContext);

            // let the active sound world mix all the channels in unless muted or avi demo recording
            if (!muted && currentSoundWorld != null && currentSoundWorld.fpa[0] == null)
                fixed (float* finalMixBuffer_ = finalMixBuffer) currentSoundWorld.MixLoop(sampleTime, numSpeakers, finalMixBuffer_);

            // disable audio hardware caching (this updates ALL settings since last alcSuspendContext)
            ALC.ProcessContext(openalContext);

            CurrentSoundTime = sampleTime;

            return SysW.Milliseconds - time;
        }

        // direct mixing called from the sound driver thread for OSes that support it
        // Mac OSX version. The system uses it's own thread and an IOProc callback
        public virtual int AsyncMix(int soundTime, float* mixBuffer)
        {
            if (!isInitialized || shutdown) return 0;

            var inTime = SysW.Milliseconds;
            var numSpeakers = s_numberOfSpeakers.Integer;

            // let the active sound world mix all the channels in unless muted or avi demo recording
            if (!muted && currentSoundWorld != null && currentSoundWorld.fpa[0] == null) currentSoundWorld.MixLoop(soundTime, numSpeakers, mixBuffer);

            CurrentSoundTime = soundTime;

            return SysW.Milliseconds - inTime;
        }

        public virtual void SetMute(bool mute)
            => muted = mute;

        static readonly uint[] ImageForTime_colors = { 0xff007f00, 0xff007f7f, 0xff00007f, 0xff00ff00, 0xff00ffff, 0xff0000ff };
        public unsafe virtual CinData ImageForTime(int milliseconds, bool waveform)
        {
            CinData ret = default;
            int i, j;

            if (!isInitialized) return ret;

            ISystem.EnterCriticalSection();
            if (graph == null) graph = (byte*)Marshal.AllocHGlobal(256 * 128 * 4);
            Unsafe.InitBlock(graph, 0, 256 * 128 * 4);
            var graphI = (uint*)graph;
            var accum = finalMixBuffer;  // unfortunately, these are already clamped
            var time = SysW.Milliseconds;
            var numSpeakers = s_numberOfSpeakers.Integer;

            if (!waveform)
            {
                for (j = 0; j < numSpeakers; j++)
                {
                    var meter = 0;
                    for (i = 0; i < Simd.MIXBUFFER_SAMPLES; i++)
                    {
                        var result = MathX.Fabs(accum[i * numSpeakers + j]);
                        if (result > meter) meter = (int)result;
                    }

                    meter /= 256;       // 32768 becomes 128
                    if (meter > 128) meter = 128;
                    int offset, xsize;
                    if (numSpeakers == 6) { offset = j * 40; xsize = 20; }
                    else { offset = j * 128; xsize = 63; }
                    int x, y;
                    var color = 0xff00ff00;
                    for (y = 0; y < 128; y++)
                    {
                        for (x = 0; x < xsize; x++) graphI[(127 - y) * 256 + offset + x] = color;
#if false
                        if (y == 80) color = 0xff00ffff;
                        else if (y == 112) color = 0xff0000ff;
#endif
                        if (y > meter) break;
                    }

                    if (meter > meterTops[j]) { meterTops[j] = meter; meterTopsTime[j] = time + s_meterTopTime.Integer; }
                    else if (time > meterTopsTime[j] && meterTops[j] > 0) { meterTops[j]--; if (meterTops[j] != 0) meterTops[j]--; }
                }
                for (j = 0; j < numSpeakers; j++)
                {
                    var meter = meterTops[j];
                    int offset, xsize;
                    if (numSpeakers == 6) { offset = j * 40; xsize = 20; }
                    else { offset = j * 128; xsize = 63; }
                    int x, y;
                    uint color;
                    if (meter <= 80) color = 0xff007f00;
                    else if (meter <= 112) color = 0xff007f7f;
                    else color = 0xff00007f;
                    for (y = meter; y < 128 && y < meter + 4; y++) for (x = 0; x < xsize; x++) graphI[(127 - y) * 256 + offset + x] = color;
                }
            }
            else
            {
                for (j = 0; j < numSpeakers; j++)
                {
                    var xx = 0; float fmeter;
                    var step = Simd.MIXBUFFER_SAMPLES / 256;
                    for (i = 0; i < Simd.MIXBUFFER_SAMPLES; i += step)
                    {
                        fmeter = 0f;
                        for (var x = 0; x < step; x++) { var result = accum[(i + x) * numSpeakers + j]; result /= 32768f; fmeter += result; }
                        fmeter /= 4f;
                        if (fmeter < -1f) fmeter = -1f;
                        else if (fmeter > 1f) fmeter = 1f;
                        var meter = (int)(fmeter * 63f);
                        graphI[(meter + 64) * 256 + xx] = ImageForTime_colors[j];

                        if (meter < 0) meter = -meter;
                        if (meter > meterTops[xx]) { meterTops[xx] = meter; meterTopsTime[xx] = time + 100; }
                        else if (time > meterTopsTime[xx] && meterTops[xx] > 0) { meterTops[xx]--; if (meterTops[xx] != 0) meterTops[xx]--; }
                        xx++;
                    }
                }
                for (i = 0; i < 256; i++)
                {
                    var meter = meterTops[i];
                    for (var y = -meter; y < meter; y++) graphI[(y + 64) * 256 + i] = ImageForTime_colors[j];
                }
            }
            ret.imageHeight = 128;
            ret.imageWidth = 256;
            ret.image = graph;
            ISystem.LeaveCriticalSection();
            return ret;
        }

        public int GetSoundDecoderInfo(int index, out SoundDecoderInfo decoderInfo)
        {
            int i, j;
            var sw = soundSystemLocal.currentSoundWorld;

            int firstEmitter, firstChannel;
            if (index < 0) { firstEmitter = 0; firstChannel = 0; }
            else { firstEmitter = index / SOUND_MAX_CHANNELS; firstChannel = index - firstEmitter * SOUND_MAX_CHANNELS + 1; }

            decoderInfo = new SoundDecoderInfo();
            for (i = firstEmitter; i < sw.emitters.Count; i++)
            {
                var sound = sw.emitters[i];
                if (sound == null) continue;

                // run through all the channels
                for (j = firstChannel; j < SOUND_MAX_CHANNELS; j++)
                {
                    var chan = sound.channels[j];
                    if (chan.decoder == null) continue;

                    var sample = chan.decoder.Sample;
                    if (sample == null) continue;

                    decoderInfo.name = sample.name;
                    decoderInfo.format = sample.objectInfo.wFormatTag == WAVE_FORMAT_TAG.OGG ? "OGG" : "WAV";
                    decoderInfo.numChannels = sample.objectInfo.nChannels;
                    decoderInfo.numSamplesPerSecond = sample.objectInfo.nSamplesPerSec;
                    decoderInfo.num44kHzSamples = sample.LengthIn44kHzSamples;
                    decoderInfo.numBytes = sample.objectMemSize;
                    decoderInfo.looping = (chan.parms.soundShaderFlags & ISoundSystem.SSF_LOOPING) != 0;
                    decoderInfo.lastVolume = chan.lastVolume;
                    decoderInfo.start44kHzTime = chan.trigger44kHzTime;
                    decoderInfo.current44kHzTime = soundSystemLocal.Current44kHzTime;
                    return i * SOUND_MAX_CHANNELS + j;
                }

                firstChannel = 0;
            }
            return -1;
        }

        // if rw == NULL, no portal occlusion or rendered debugging is available
        public virtual ISoundWorld AllocSoundWorld(IRenderWorld rw)
        {
            var local = new SoundWorldLocal();
            local.Init(rw);
            return local;
        }

        // some tools, like the sound dialog, may be used in both the game and the editor This can return NULL, so check!
        // specifying NULL will cause silence to be played
        public virtual ISoundWorld PlayingSoundWorld
        {
            get => currentSoundWorld;
            set => currentSoundWorld = (SoundWorldLocal)value;
        }

        public virtual void BeginLevelLoad()
        {
            if (!isInitialized) return;

            soundCache.BeginLevelLoad();
            if (efxloaded)
            {
                EFXDatabase.Clear();
                efxloaded = false;
            }
        }

        public virtual void EndLevelLoad(string mapString)
        {
            if (!isInitialized) return;

            soundCache.EndLevelLoad();
            if (!useEFXReverb) return;

            var efxname = "efxs/";
            var mapname = mapString;

            mapname = PathX.SetFileExtension(mapname, ".efx");
            mapname = PathX.StripPath(mapname);
            efxname += mapname;

            efxloaded = EFXDatabase.LoadFile(efxname);

            common.Printf(efxloaded ? $"sound: found {efxname}\n" : $"sound: missing {efxname}\n");
        }

        public virtual void PrintMemInfo(MemInfo mi)
            => soundCache.PrintMemInfo(mi);

        public virtual int IsEFXAvailable
#if ID_DEDICATED
            => -1;
#else
            => EFXAvailable;
#endif

        //-------------------------

        public int Current44kHzTime
            => isInitialized
                ? CurrentSoundTime
                : MathX.FtoiFast(SysW.Milliseconds * 176.4f); // NOTE: this would overflow 31bits within about 1h20 // ((SysW.Milliseconds() * 441) / 10) * 4;

        public float dB2Scale(float val)
        {
            if (val == 0f) return 1f;                // most common
            else if (val <= -60f) return 0f;
            else if (val >= 60f) return (float)Math.Pow(2f, val * (1f / 6f));
            var ival = (int)((val + 60f) * 10f);
            return volumesDB[ival];
        }
        public int SamplesToMilliseconds(int samples)
            => samples / (PRIMARYFREQ / 1000);
        public int MillisecondsToSamples(int ms)
            => ms * (PRIMARYFREQ / 1000);

        public unsafe void DoEnviroSuit(float* samples, int numSamples, int numSpeakers)
        {
            // TODO port to OpenAL
            Debug.Assert(false);

            if (fxList.Count == 0)
                for (var i = 0; i < 6; i++)
                {
                    fxList.Add(new SoundFX_Lowpass { Channel = i }); // lowpass filter
                    fxList.Add(new SoundFX_Comb { Channel = i, Parameter = i * 100 }); // comb
                    fxList.Add(new SoundFX_Comb { Channel = i, Parameter = i * 100 + 5 }); // comb
                }

            var o_ = stackalloc float[10000]; float* oF = o_ + 2;
            var i_ = stackalloc float[10000]; float* iF = i_ + 2;

            for (var i = 0; i < numSpeakers; i++)
            {
                int j;

                // restore previous samples
                Unsafe.InitBlock(i_, 0, 10000 * sizeof(float));
                Unsafe.InitBlock(o_, 0, 10000 * sizeof(float));

                // fx loop
                for (var k = 0; k < fxList.Count; k++)
                {
                    var fx = fxList[k];

                    // skip if we're not the right channel
                    if (fx.Channel != i) continue;

                    // get samples and continuity
                    fx.GetContinuitySamples(out iF[-1], out iF[-2], out oF[-1], out oF[-2]);
                    for (j = 0; j < numSamples; j++) iF[j] = samples[j * numSpeakers + i] * s_enviroSuitVolumeScale.Float;

                    // process fx loop
                    for (j = 0; j < numSamples; j++) fx.ProcessSample(iF + j, oF + j);

                    // store samples and continuity
                    fx.SetContinuitySamples(iF[numSamples - 2], iF[numSamples - 3], oF[numSamples - 2], oF[numSamples - 3]);

                    for (j = 0; j < numSamples; j++) samples[j * numSpeakers + i] = oF[j];
                }
            }
        }

        public int AllocOpenALSource(SoundChannel chan, bool looping, bool stereo)
        {
            var timeOldestZeroVolSingleShot = SysW.Milliseconds;
            var timeOldestZeroVolLooping = SysW.Milliseconds;
            var timeOldestSingle = SysW.Milliseconds;
            var iOldestZeroVolSingleShot = -1;
            var iOldestZeroVolLooping = -1;
            var iOldestSingle = -1;
            var iUnused = -1;
            var index = -1;

            // Grab current msec time
            var time = SysW.Milliseconds;

            // Cycle through all sources
            for (var i = 0; i < openalSourceCount; i++)
                // Use any unused source first, Then find oldest single shot quiet source, Then find oldest looping quiet source and Lastly find oldest single shot non quiet source..
                if (!openalSources[i].inUse) { iUnused = i; break; }
                else if (!openalSources[i].looping && openalSources[i].chan.lastVolume < SND_EPSILON)
                {
                    if (openalSources[i].startTime < timeOldestZeroVolSingleShot) { timeOldestZeroVolSingleShot = openalSources[i].startTime; iOldestZeroVolSingleShot = i; }
                }
                else if (openalSources[i].looping && openalSources[i].chan.lastVolume < SND_EPSILON)
                {
                    if (openalSources[i].startTime < timeOldestZeroVolLooping) { timeOldestZeroVolLooping = openalSources[i].startTime; iOldestZeroVolLooping = i; }
                }
                else if (!openalSources[i].looping)
                {
                    if (openalSources[i].startTime < timeOldestSingle) { timeOldestSingle = openalSources[i].startTime; iOldestSingle = i; }
                }

            if (iUnused != -1) index = iUnused;
            else if (iOldestZeroVolSingleShot != -1) index = iOldestZeroVolSingleShot;
            else if (iOldestZeroVolLooping != -1) index = iOldestZeroVolLooping;
            else if (iOldestSingle != -1) index = iOldestSingle;

            if (index != -1)
            {
                // stop the channel that is being ripped off
                if (openalSources[index].chan != null)
                {
                    // stop the channel only when not looping
                    if (!openalSources[index].looping) openalSources[index].chan.Stop();
                    else openalSources[index].chan.triggered = true;

                    // Free hardware resources
                    openalSources[index].chan.ALStop();
                }

                // Initialize structure
                openalSources[index].startTime = time;
                openalSources[index].chan = chan;
                openalSources[index].inUse = true;
                openalSources[index].looping = looping;
                openalSources[index].stereo = stereo;

                return openalSources[index].handle;
            }
            else return 0;
        }

        public void FreeOpenALSource(int handle)
        {
            for (var i = 0; i < openalSourceCount; i++)
                if (openalSources[i].handle == handle)
                {
                    if (openalSources[i].chan != null) openalSources[i].chan.openalSource = 0;

                    // Initialize structure
                    openalSources[i].startTime = 0;
                    openalSources[i].chan = null;
                    openalSources[i].inUse = false;
                    openalSources[i].looping = false;
                    openalSources[i].stereo = false;
                }
        }

        // returns true if openalDevice is still available, otherwise it will try to recover the device and return false while it's gone
        // (display audio sound devices sometimes disappear for a few seconds when switching resolution)
        const int CheckDeviceAndRecoverIfNeeded_maxRetries = 20;
        public bool CheckDeviceAndRecoverIfNeeded()
        {
            if (alcResetDeviceSOFT == null) return true; // we can't check or reset, just pretend everything is fine..

            var curTime = (uint)SysW.Milliseconds;
            if (curTime - lastCheckTime >= 1000) // check once per second
            {
                lastCheckTime = curTime;

                // ALC_CONNECTED needs ALC_EXT_disconnect (we check for that in Init())
                ALC.GetInteger(openalDevice, (AlcGetInteger)0, 1, out int connected);
                if (connected != 0) { resetRetryCount = 0; return true; }
                if (resetRetryCount == 0) { common.Warning("OpenAL device disconnected! Will try to reconnect.."); resetRetryCount = 1; }
                else if (resetRetryCount > CheckDeviceAndRecoverIfNeeded_maxRetries)
                {   // give up after 20 seconds
                    if (resetRetryCount == CheckDeviceAndRecoverIfNeeded_maxRetries + 1)
                    {
                        common.Warning("OpenAL device still disconnected! Giving up!");
                        ++resetRetryCount; // this makes sure the warning is only shown once

                        // TODO: can we shut down sound without things blowing up?
                        //       if we can, we could do that if we don't have alcResetDeviceSOFT but ALC_EXT_disconnect
                    }
                    return false;
                }

                if (alcResetDeviceSOFT(openalDevice, null)) { common.Printf("OpenAL: resetting device succeeded!\n"); resetRetryCount = 0; return true; }

                ++resetRetryCount;
                return false;
            }

            return resetRetryCount == 0; // if it's 0, state on last check was ok
        }

        public SoundCache soundCache;

        public SoundWorldLocal currentSoundWorld;   // the one to mix each async tic

        public uint olddwCurrentWritePos;   // statistics
        public int buffers;                // statistics
        public int CurrentSoundTime;       // set by the async thread and only used by the main thread

        public uint nextWriteBlock;

        public float[] realAccum = new float[6 * Simd.MIXBUFFER_SAMPLES + 16];
        public float[] finalMixBuffer;          // points inside realAccum at a 16 byte aligned boundary

        public bool isInitialized;
        public bool muted;
        public bool shutdown;

        public SoundStats soundStats;             // NOTE: updated throughout the code, not displayed anywhere

        public int[] meterTops = new int[256];
        public int[] meterTopsTime = new int[256];

        public byte* graph;

        public float[] volumesDB = new float[1200];      // dB to float volume conversion

        public List<SoundFX> fxList = new();

        public ALDevice openalDevice;
        public ALContext openalContext;
        public int openalSourceCount;
        public OpenalSource[] openalSources = new OpenalSource[256];

        public EFX.GenEffectsRefDelegate alGenEffects;
        public EFX.DeleteEffectsRefDelegate alDeleteEffects;
        public EFX.IsEffectDelegate alIsEffect;
        public EFX.EffectiDelegate alEffecti;
        public EFX.EffectfDelegate alEffectf;
        public EFX.EffectfvArrayDelegate alEffectfv;
        public EFX.GenFiltersRefDelegate alGenFilters;
        public EFX.DeleteFiltersRefDelegate alDeleteFilters;
        public EFX.IsFilterDelegate alIsFilter;
        public EFX.FilteriDelegate alFilteri;
        public EFX.FilterfDelegate alFilterf;
        public EFX.GenAuxiliaryEffectSlotsRefDelegate alGenAuxiliaryEffectSlots;
        public EFX.DeleteAuxiliaryEffectSlotsRefDelegate alDeleteAuxiliaryEffectSlots;
        public EFX.IsAuxiliaryEffectSlotDelegate alIsAuxiliaryEffectSlot;
        public EFX.AuxiliaryEffectSlotiDelegate alAuxiliaryEffectSloti;
        public EFXFile EFXDatabase;
        public bool efxloaded;

        // latches
        public static bool useEFXReverb = false;
        // mark available during initialization, or through an explicit test
        public static int EFXAvailable = -1;

        // DG: for CheckDeviceAndRecoverIfNeeded()
        public HRTF.ResetDeviceSoftArrayDelegate alcResetDeviceSOFT; // needs ALC_SOFT_HRTF extension
        public int resetRetryCount;
        public uint lastCheckTime;

#if ID_DEDICATED
        public static readonly CVar s_noSound = new("s_noSound", "1", CVAR.SOUND | CVAR.BOOL | CVAR.ROM, "");
#else
        public static readonly CVar s_noSound = new("s_noSound", "0", CVAR.SOUND | CVAR.BOOL | CVAR.NOCHEAT, "");
#endif
        public static readonly CVar s_device = new("s_device", "default", CVAR.SOUND | CVAR.NOCHEAT | CVAR.ARCHIVE, "the audio device to use ('default' for the default audio device)");
        public static readonly CVar s_quadraticFalloff = new("s_quadraticFalloff", "1", CVAR.SOUND | CVAR.BOOL, "");
        public static readonly CVar s_drawSounds = new("s_drawSounds", "0", CVAR.SOUND | CVAR.INTEGER, "", 0, 2, CmdArgs.ArgCompletion_Integer(0, 2));
        public static readonly CVar s_useOcclusion = new("s_useOcclusion", "1", CVAR.SOUND | CVAR.BOOL, "");
        public static readonly CVar s_showStartSound = new("s_showStartSound", "0", CVAR.SOUND | CVAR.BOOL, "");
        public static readonly CVar s_maxSoundsPerShader = new("s_maxSoundsPerShader", "0", CVAR.SOUND | CVAR.ARCHIVE, "", 0, 10, CmdArgs.ArgCompletion_Integer(0, 10));
        public static readonly CVar s_showLevelMeter = new("s_showLevelMeter", "0", CVAR.SOUND | CVAR.BOOL, "");
        public static readonly CVar s_constantAmplitude = new("s_constantAmplitude", "-1", CVAR.SOUND | CVAR.FLOAT, "");
        public static readonly CVar s_minVolume6 = new("s_minVolume6", "0", CVAR.SOUND | CVAR.FLOAT, "");
        public static readonly CVar s_dotbias6 = new("s_dotbias6", "0.8", CVAR.SOUND | CVAR.FLOAT, "");
        public static readonly CVar s_minVolume2 = new("s_minVolume2", "0.25", CVAR.SOUND | CVAR.FLOAT, "");
        public static readonly CVar s_dotbias2 = new("s_dotbias2", "1.1", CVAR.SOUND | CVAR.FLOAT, "");
        public static readonly CVar s_spatializationDecay = new("s_spatializationDecay", "2", CVAR.SOUND | CVAR.ARCHIVE | CVAR.FLOAT, "");
        public static readonly CVar s_reverse = new("s_reverse", "0", CVAR.SOUND | CVAR.ARCHIVE | CVAR.BOOL, "");
        public static readonly CVar s_meterTopTime = new("s_meterTopTime", "2000", CVAR.SOUND | CVAR.ARCHIVE | CVAR.INTEGER, "");
        public static readonly CVar s_volume = new("s_volume_dB", "0", CVAR.SOUND | CVAR.ARCHIVE | CVAR.FLOAT, "volume in dB");
        public static readonly CVar s_playDefaultSound = new("s_playDefaultSound", "1", CVAR.SOUND | CVAR.ARCHIVE | CVAR.BOOL, "play a beep for missing sounds");
        public static readonly CVar s_subFraction = new("s_subFraction", "0.75", CVAR.SOUND | CVAR.ARCHIVE | CVAR.FLOAT, "volume to subwoofer in 5.1");
        public static readonly CVar s_globalFraction = new("s_globalFraction", "0.8", CVAR.SOUND | CVAR.ARCHIVE | CVAR.FLOAT, "volume to all speakers when not spatialized");
        public static readonly CVar s_doorDistanceAdd = new("s_doorDistanceAdd", "150", CVAR.SOUND | CVAR.ARCHIVE | CVAR.FLOAT, "reduce sound volume with this distance when going through a door");
        public static readonly CVar s_singleEmitter = new("s_singleEmitter", "0", CVAR.SOUND | CVAR.INTEGER, "mute all sounds but this emitter");
        public static readonly CVar s_numberOfSpeakers = new("s_numberOfSpeakers", "2", CVAR.SOUND | CVAR.ARCHIVE, "number of speakers");
        public static readonly CVar s_force22kHz = new("s_force22kHz", "0", CVAR.SOUND | CVAR.BOOL, "");
        public static readonly CVar s_clipVolumes = new("s_clipVolumes", "1", CVAR.SOUND | CVAR.BOOL, "");
        public static readonly CVar s_realTimeDecoding = new("s_realTimeDecoding", "1", CVAR.SOUND | CVAR.BOOL | CVAR.INIT, "");

        public static readonly CVar s_slowAttenuate = new("s_slowAttenuate", "1", CVAR.SOUND | CVAR.BOOL, "slowmo sounds attenuate over shorted distance");
        public static readonly CVar s_enviroSuitCutoffFreq = new("s_enviroSuitCutoffFreq", "2000", CVAR.SOUND | CVAR.FLOAT, "");
        public static readonly CVar s_enviroSuitCutoffQ = new("s_enviroSuitCutoffQ", "2", CVAR.SOUND | CVAR.FLOAT, "");
        public static readonly CVar s_reverbTime = new("s_reverbTime", "1000", CVAR.SOUND | CVAR.FLOAT, "");
        public static readonly CVar s_reverbFeedback = new("s_reverbFeedback", "0.333", CVAR.SOUND | CVAR.FLOAT, "");
        public static readonly CVar s_enviroSuitVolumeScale = new("s_enviroSuitVolumeScale", "0.9", CVAR.SOUND | CVAR.FLOAT, "");
        public static readonly CVar s_skipHelltimeFX = new("s_skipHelltimeFX", "0", CVAR.SOUND | CVAR.BOOL, "");

#if !ID_DEDICATED
        public static readonly CVar s_useEAXReverb = new("s_useEAXReverb", "1", CVAR.SOUND | CVAR.BOOL | CVAR.ARCHIVE, "use EFX reverb");
        public static readonly CVar s_decompressionLimit = new("s_decompressionLimit", "6", CVAR.SOUND | CVAR.INTEGER | CVAR.ARCHIVE, "specifies maximum uncompressed sample length in seconds");
#else
        public static readonly CVar s_useEAXReverb = new("s_useEAXReverb", "0", CVAR.SOUND | CVAR.BOOL | CVAR.ROM, "EFX not available in this build");
        public static readonly CVar s_decompressionLimit = new("s_decompressionLimit", "6", CVAR.SOUND | CVAR.INTEGER | CVAR.ROM, "specifies maximum uncompressed sample length in seconds");
#endif

        //public static readonly CVar s_enviroSuitSkipLowpass;
        //public static readonly CVar s_enviroSuitSkipReverb;

        #region Commands

        // this is called from the main thread
        static void SoundReloadSounds_f(CmdArgs args)
        {
            if (soundSystemLocal.soundCache == null) return;
            var force = args.Count == 2;
            soundSystem.SetMute(true);
            soundSystemLocal.soundCache.ReloadSounds(force);
            soundSystem.SetMute(false);
            common.Printf("sound: changed sounds reloaded\n");
        }

        // Optional parameter to only list sounds containing that string
        static void ListSounds_f(CmdArgs args)
        {
            if (soundSystemLocal.soundCache == null) { common.Printf("No sound.\n"); return; }

            var snd = args[1];
            var totalSounds = 0;
            var totalSamples = 0;
            var totalMemory = 0;
            var totalPCMMemory = 0;
            for (var i = 0; i < soundSystemLocal.soundCache.NumObjects; i++)
            {
                var sample = soundSystemLocal.soundCache.GetObject(i);
                if (sample == null) continue;
                if (snd != null && !sample.name.Contains(snd, StringComparison.OrdinalIgnoreCase)) continue;

                var info = sample.objectInfo;
                var stereo = info.nChannels == 2 ? "ST" : "  ";
                var format = info.wFormatTag == WAVE_FORMAT_TAG.OGG ? "OGG" : "WAV";
                var defaulted = sample.defaultSound ? "(DEFAULTED)" : sample.purged ? "(PURGED)" : "";
                common.Printf($"{stereo} {sample.objectInfo.nSamplesPerSec / 1000}kHz {soundSystemLocal.SamplesToMilliseconds(sample.LengthIn44kHzSamples):6}ms {sample.objectMemSize >> 10:5}kB {format:4} {sample.name}{defaulted}\n");

                if (!sample.purged)
                {
                    totalSamples += sample.objectSize;
                    if (info.wFormatTag != WAVE_FORMAT_TAG.OGG) totalPCMMemory += sample.objectMemSize;
                    if (!sample.hardwareBuffer) totalMemory += sample.objectMemSize;
                }
                totalSounds++;
            }
            common.Printf($"{totalSounds:8} total sounds\n");
            common.Printf($"{totalSamples:8} total samples loaded\n");
            common.Printf($"{totalMemory >> 10:8} kB total system memory used\n");
        }

        static void ListSoundDecoders_f(CmdArgs args)
        {
            int i, j, numActiveDecoders, numWaitingDecoders;
            var sw = soundSystemLocal.currentSoundWorld;

            numActiveDecoders = numWaitingDecoders = 0;

            for (i = 0; i < sw.emitters.Count; i++)
            {
                var sound = sw.emitters[i];
                if (sound == null) continue;

                // run through all the channels
                for (j = 0; j < SoundSystemLocal.SOUND_MAX_CHANNELS; j++)
                {
                    var chan = sound.channels[j];
                    if (chan.decoder == null) continue;

                    var sample = chan.decoder.Sample;
                    if (sample != null) continue;

                    var format = chan.leadinSample.objectInfo.wFormatTag == WAVE_FORMAT_TAG.OGG ? "OGG" : "WAV";
                    common.Printf($"{numWaitingDecoders:3} waiting {format}: {chan.leadinSample.name}\n");

                    numWaitingDecoders++;
                }
            }

            for (i = 0; i < sw.emitters.Count; i++)
            {
                var sound = sw.emitters[i];
                if (sound == null) continue;

                // run through all the channels
                for (j = 0; j < SoundSystemLocal.SOUND_MAX_CHANNELS; j++)
                {
                    var chan = sound.channels[j];
                    if (chan.decoder == null) continue;

                    var sample = chan.decoder.Sample;
                    if (sample == null) continue;

                    var format = sample.objectInfo.wFormatTag == WAVE_FORMAT_TAG.OGG ? "OGG" : "WAV";
                    var localTime = soundSystemLocal.Current44kHzTime - chan.trigger44kHzTime;
                    var sampleTime = sample.LengthIn44kHzSamples * sample.objectInfo.nChannels;
                    var percent = localTime > sampleTime ?
                        (chan.parms.soundShaderFlags & ISoundSystem.SSF_LOOPING) != 0 ? (localTime % sampleTime) * 100 / sampleTime : 100
                        : localTime * 100 / sampleTime;
                    common.Printf($"{numActiveDecoders:3} decoding {percent:3}% {format}: {sample.name}\n");

                    numActiveDecoders++;
                }
            }

            common.Printf($"{numWaitingDecoders + numActiveDecoders} decoders\n");
            common.Printf($"{numWaitingDecoders} waiting decoders\n");
            common.Printf($"{numActiveDecoders} active decoders\n");
            common.Printf($"{ISampleDecoder.UsedBlockMemory >> 10} kB decoder memory in {ISampleDecoder.NumUsedBlocks} blocks\n");
        }

        // this is called from the main thread
        static void TestSound_f(CmdArgs args)
        {
            if (args.Count != 2) { common.Printf("Usage: testSound <file>\n"); return; }
            soundSystemLocal.currentSoundWorld?.PlayShaderDirectly(args[1]);
        }

        // restart the sound thread. this is called from the main thread
        static void SoundSystemRestart_f(CmdArgs args)
        {
            soundSystem.SetMute(true);
            soundSystemLocal.ShutdownHW();
            soundSystemLocal.InitHW();
            soundSystem.SetMute(false);
        }

        // DG: make this function callable from idSessionLocal::Frame() without having to change the public idSoundSystem interface - that would break mod DLL compat,
        // and this is not relevant for gamecode.
        static bool CheckOpenALDeviceAndRecoverIfNeeded()
           => soundSystemLocal.isInitialized ? soundSystemLocal.CheckDeviceAndRecoverIfNeeded() : true;

        #endregion
    }

    public unsafe class SoundFX_Lowpass : SoundFX
    {
        public override void ProcessSample(float* i, float* o)
        {
            float c, a1, a2, a3, b1, b2;
            var resonance = SoundSystemLocal.s_enviroSuitCutoffQ.Float;
            var cutoffFrequency = SoundSystemLocal.s_enviroSuitCutoffFreq.Float;

            Initialize();

            c = 1f / MathX.Tan16(MathX.PI * cutoffFrequency / 44100);

            // compute coefs
            a1 = 1f / (1f + resonance * c + c * c);
            a2 = 2 * a1;
            a3 = a1;
            b1 = 2f * (1f - c * c) * a1;
            b2 = (1f - resonance * c + c * c) * a1;

            // compute output value
            o[0] = a1 * i[0] + a2 * i[-1] + a3 * i[-2] - b1 * o[-1] - b2 * o[-2];
        }
    }

    public unsafe class SoundFX_LowpassFast : SoundFX
    {
        float freq, res, a1, a2, a3, b1, b2;

        public override void ProcessSample(float* i, float* o)
            => o[0] = a1 * i[0] + a2 * i[-1] + a3 * i[-2] - b1 * o[-1] - b2 * o[-2];

        public void SetParms(float p1 = 0, float p2 = 0, float p3 = 0)
        {
            float c;

            // set the vars
            freq = p1;
            res = p2;

            // precompute the coefs
            c = 1f / MathX.Tan(MathX.PI * freq / 44100);

            // compute coefs
            a1 = 1f / (1f + res * c + c * c);
            a2 = 2 * a1;
            a3 = a1;

            b1 = 2f * (1f - c * c) * a1;
            b2 = (1f - res * c + c * c) * a1;
        }
    }

    public unsafe class SoundFX_Comb : SoundFX
    {
        int currentTime;

        public override void Initialize()
        {
            if (initialized) return;

            initialized = true;
            maxlen = 50000;
            buffer = new float[maxlen];
            currentTime = 0;
        }

        public override void ProcessSample(float* i, float* o)
        {
            var gain = SoundSystemLocal.s_reverbFeedback.Float;
            var len = (int)(SoundSystemLocal.s_reverbTime.Float + param);

            Initialize();

            // sum up and output
            o[0] = buffer[currentTime];
            buffer[currentTime] = buffer[currentTime] * gain + i[0];

            // increment current time
            currentTime++;
            if (currentTime >= len) currentTime -= len;
        }
    }
}