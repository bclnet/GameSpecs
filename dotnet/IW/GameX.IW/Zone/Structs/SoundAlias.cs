using System.Runtime.InteropServices;

namespace GameX.IW.Zone
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct SndCurve
    {
        public char* name;
        public fixed char field_4[132];
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct SpeakerLevels
    {
        public int speaker;
        public int numLevels;
        public fixed float levels[2];
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ChannelMap
    {
        public int entryCount; // how many entries are used
        public SpeakerLevels speakers0; public SpeakerLevels speakers1; public SpeakerLevels speakers2; public SpeakerLevels speakers3; public SpeakerLevels speakers4; public SpeakerLevels speakers5;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct SpeakerMap
    {
        public bool isDefault;
        public char* name;
        public ChannelMap* channelMaps0;
        public ChannelMap* channelMaps1;
    }

    public enum snd_alias_type_t : byte
    {
        SAT_UNKNOWN = 0x0,
        SAT_LOADED = 0x1,
        SAT_STREAMED = 0x2
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct MssSound
    {
        public fixed char unknown1[8];
        public int dataLenth;
        public fixed char unknown2[24];
        public char* soundData;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct LoadedSound
    {
        public char* name;
        public MssSound data;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct StreamedSound
    {
        public char* dir;
        public char* name;
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct SoundData //: union
    {
        [FieldOffset(0)] public LoadedSound* loaded;
        [FieldOffset(0)] public StreamedSound stream;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct SoundFile    // 0xC
    {
        public snd_alias_type_t type;
        public bool exists;
        public SoundData data;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct SoundAlias
    {
        public char* name;
        public char* subtitle;
        public char* secondaryAliasName;
        public char* chainAliasName;
        public char* string4;
        public SoundFile* soundFile;
        public int sequence;
        public float volMin;
        public float volMax;
        public float pitchMin;
        public float pitchMax;
        public float distMin;
        public float distMax;
        public int flags;
        public float slavePercentage;
        public float probability;
        public float lfePercentage;
        public float centerPercentage;
        public int startDelay;
        public int pad;
        public SndCurve* volumeFalloffCurve;
        public float envelopMin;
        public float envelopMax;
        public float envelopPercentage;
        public SpeakerMap* speakerMap;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct SoundAliasList
    {
        public char* name;
        public SoundAlias* head;
        public int count;
    }
}