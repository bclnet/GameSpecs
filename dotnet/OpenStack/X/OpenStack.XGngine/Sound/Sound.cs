using System.NumericsX.OpenStack.Gngine.Framework;
using System.NumericsX.OpenStack.Gngine.Render;
using ChannelType = System.Int32; // the game uses its own series of enums, and we don't want to require casts

namespace System.NumericsX.OpenStack.Gngine
{
    // these options can be overriden from sound shader defaults on a per-emitter and per-channel basis
    public class SoundShaderParms
    {
        public float minDistance;
        public float maxDistance;
        public float volume;                // in dB, unfortunately.  Negative values get quieter
        public float shakes;
        public int soundShaderFlags;        // SSF_* bit flags
        public int soundClass;              // for global fading of sounds

        public void memset()
        {
            throw new NotImplementedException();
        }
    }

    public interface ISoundShader { }

    public interface ISoundEmitter
    {
        // a non-immediate free will let all currently playing sounds complete soundEmitters are not actually deleted, they are just marked as
        // reusable by the soundWorld
        void Free(bool immediate);

        // the parms specified will be the default overrides for all sounds started on this emitter. NULL is acceptable for parms
        void UpdateEmitter(Vector3 origin, int listenerId, SoundShaderParms parms);

        // returns the length of the started sound in msec
        int StartSound(ISoundShader shader, ChannelType channel, float diversity = 0, int shaderFlags = 0, bool allowSlow = true);

        // pass SCHANNEL_ANY to effect all channels
        void ModifySound(ChannelType channel, SoundShaderParms parms);
        void StopSound(ChannelType channel);
        void FadeSound(ChannelType channel, float to, float over); // to is in Db (sigh), over is in seconds

        // returns true if there are any sounds playing from this emitter.  There is some conservative slop at the end to remove inconsistent race conditions with the sound thread updates.
        // FIXME: network game: on a dedicated server, this will always be false
        bool CurrentlyPlaying { get; }

        // returns a 0.0 to 1.0 value based on the current sound amplitude, allowing graphic effects to be modified in time with the audio.
        // just samples the raw wav file, it doesn't account for volume overrides in the
        float CurrentAmplitude { get; }

        // for save games.  Index will always be > 0
        int Index { get; }
    }

    public interface ISoundWorld
    {
        // call at each map start
        void ClearAllSoundEmitters();
        void StopAllSounds();

        // get a new emitter that can play sounds in this world
        ISoundEmitter AllocSoundEmitter();

        // for load games, index 0 will return NULL
        ISoundEmitter EmitterForIndex(int index);

        // query sound samples from all emitters reaching a given position
        float CurrentShakeAmplitudeForPosition(int time, Vector3? listenerPosition);

        // where is the camera/microphone listenerId allows listener-private and antiPrivate sounds to be filtered
        // gameTime is in msec, and is used to time sound queries and removals so that they are independent of any race conditions with the async update
        void PlaceListener(Vector3 origin, Matrix3x3 axis, int listenerId, int gameTime, string areaName);

        // fade all sounds in the world with a given shader soundClass to is in Db (sigh), over is in seconds
        void FadeSoundClasses(int soundClass, float to, float over);

        // background music
        void PlayShaderDirectly(string name, int channel = -1);

        // dumps the current state and begins archiving commands
        void StartWritingDemo(VFileDemo demo);
        void StopWritingDemo();

        // read a sound command from a demo file
        void ProcessDemoCommand(VFileDemo demo);

        // pause and unpause the sound world
        void Pause();
        void UnPause();
        bool IsPaused { get; }

        // Write the sound output to multiple wav files.  Note that this does not use the work done by AsyncUpdate, it mixes explicitly in the foreground every PlaceOrigin(),
        // under the assumption that we are rendering out screenshots and the gameTime is going much slower than real time.
        // path should not include an extension, and the generated filenames will be:
        // <path>_left.raw, <path>_right.raw, or <path>_51left.raw, <path>_51right.raw,
        // <path>_51center.raw, <path>_51lfe.raw, <path>_51backleft.raw, <path>_51backright.raw,
        // If only two channel mixing is enabled, the left and right .raw files will also be combined into a stereo .wav file.
        void AVIOpen(string path, string name);
        void AVIClose();

        // SaveGame / demo Support
        void WriteToSaveGame(VFile savefile);
        void ReadFromSaveGame(VFile savefile);

        void SetSlowmo(bool active);
        void SetSlowmoSpeed(float speed);
        void SetEnviroSuit(bool active);
    }

    public class SoundDecoderInfo
    {
        public string name;
        public string format;
        public int numChannels;
        public int numSamplesPerSecond;
        public int num44kHzSamples;
        public int numBytes;
        public bool looping;
        public float lastVolume;
        public int start44kHzTime;
        public int current44kHzTime;
    }

    public interface ISoundSystem
    {
        // sound shader flags
        public const int SSF_PRIVATE_SOUND = 1 << 0;    // only plays for the current listenerId
        public const int SSF_ANTI_PRIVATE_SOUND = 1 << 1;   // plays for everyone but the current listenerId
        public const int SSF_NO_OCCLUSION = 1 << 2; // don't flow through portals, only use straight line
        public const int SSF_GLOBAL = 1 << 3;   // play full volume to all speakers and all listeners
        public const int SSF_OMNIDIRECTIONAL = 1 << 4;  // fall off with distance, but play same volume in all speakers
        public const int SSF_LOOPING = 1 << 5;  // repeat the sound continuously
        public const int SSF_PLAY_ONCE = 1 << 6;    // never restart if already playing on any channel of a given emitter
        public const int SSF_UNCLAMPED = 1 << 7;    // don't clamp calculated volumes at 1.0
        public const int SSF_NO_FLICKER = 1 << 8;   // always return 1.0 for volume queries
        public const int SSF_NO_DUPS = 1 << 9;  // try not to play the same sound twice in a row

        public const int SOUND_MAX_LIST_WAVS = 32;
        // sound classes are used to fade most sounds down inside cinematics, leaving dialog flagged with a non-zero class full volume
        const int SOUND_MAX_CLASSES = 4;

        // sound channels
        public const int SCHANNEL_ANY = 0; // used in queries and commands to effect every channel at once, in startSound to have it not override any other channel
        public const int SCHANNEL_ONE = 1; // any following integer can be used as a channel number

        // unfortunately, our minDistance / maxDistance is specified in meters, and we have far too many of them to change at this time.
        public const float DOOM_TO_METERS = 0.0254f;                   // doom to meters
        public const float METERS_TO_DOOM = 1.0f / DOOM_TO_METERS;   // meters to doom

        // all non-hardware initialization
        void Init();

        // shutdown routine
        void Shutdown();

        // sound is attached to the window, and must be recreated when the window is changed
        bool InitHW();
        bool ShutdownHW();

        // asyn loop, called at 60Hz
        int AsyncUpdate(int time);

        // async loop, when the sound driver uses a write strategy
        int AsyncUpdateWrite(int time);

        // it is a good idea to mute everything when starting a new level, because sounds may be started before a valid listener origin is specified
        void SetMute(bool mute);

        // for the sound level meter window
        CinData ImageForTime(int milliseconds, bool waveform);

        // get sound decoder info
        int GetSoundDecoderInfo(int index, out SoundDecoderInfo decoderInfo);

        // if rw == NULL, no portal occlusion or rendered debugging is available
        ISoundWorld AllocSoundWorld(IRenderWorld rw);

        // some tools, like the sound dialog, may be used in both the game and the editor specifying NULL will cause silence to be played
        // This can return NULL, so check!
        ISoundWorld PlayingSoundWorld { get; set; }

        // Mark all soundSamples as currently unused, but don't free anything.
        void BeginLevelLoad();

        // Free all soundSamples marked as unused
        // We might want to defer the loading of new sounds to this point, as we do with images, to avoid having a union in memory at one time.
        void EndLevelLoad(string mapString);

        // direct mixing for OSes that support it
        unsafe int AsyncMix(int soundTime, float* mixBuffer);

        // prints memory info
        void PrintMemInfo(MemInfo mi);

        // is EFX support present - -1: disabled at compile time, 0: no suitable hardware, 1: ok
        int IsEFXAvailable { get; }
    }
}