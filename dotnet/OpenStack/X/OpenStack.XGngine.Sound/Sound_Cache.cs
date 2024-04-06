//#define USE_SOUND_CACHE_ALLOCATOR
using System.Collections.Generic;
using System.IO;
using System.NumericsX.OpenAL;
using System.NumericsX.OpenStack.Gngine.Framework;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.Sound
{
    // The actual sound cache
    public class SoundCache
    {
#if USE_SOUND_CACHE_ALLOCATOR
		public static DynamicBlockAlloc<byte> soundCacheAllocator = new(1 << 20, 1 << 10);
#else
        public static DynamicAlloc<byte> soundCacheAllocator = new(1 << 20, 1 << 10);
#endif

        bool insideLevelLoad;
        List<SoundSample> listCache = new();

        public SoundCache()
        {
            soundCacheAllocator.Init();
            soundCacheAllocator.SetLockMemory(true);
            listCache.AssureSize(1024);
            listCache.SetGranularity(256);
            insideLevelLoad = false;
        }

        public void Dispose()
        {
            listCache.Clear();
            soundCacheAllocator.Shutdown();
        }

        // Adds a sound object to the cache and returns a handle for it.
        public SoundSample FindSound(string filename, bool loadOnDemandOnly)
        {
            var fname = PathX.BackSlashesToSlashes(filename).ToLowerInvariant();

            declManager.MediaPrint($"{fname}\n");

            // check to see if object is already in cache
            SoundSample def;
            for (var i = 0; i < listCache.Count; i++)
            {
                def = listCache[i];
                if (def != null && def.name == fname)
                {
                    def.levelLoadReferenced = true;
                    if (def.purged && !loadOnDemandOnly) def.Load();
                    return def;
                }
            }

            // create a new entry
            def = new SoundSample();

            var shandle = listCache.FindIndex(x => x == null);
            if (shandle != -1) listCache[shandle] = def;
            else shandle = listCache.Add_(def);

            def.name = fname;
            def.levelLoadReferenced = true;
            def.onDemand = loadOnDemandOnly;
            def.purged = true;

            // this may make it a default sound if it can't be loaded
            if (!loadOnDemandOnly) def.Load();
            return def;
        }

        public int NumObjects
            => listCache.Count;

        // returns a single cached object pointer
        public SoundSample GetObject(int index)
            => index < 0 || index > listCache.Count ? null : listCache[index];

        // Completely nukes the current cache
        public void ReloadSounds(bool force)
        {
            for (var i = 0; i < listCache.Count; i++) { var def = listCache[i]; def?.Reload(force); }
        }

        // Mark all file based images as currently unused, but don't free anything.  Calls to ImageFromFile() will
        // either mark the image as used, or create a new image without loading the actual data.
        public void BeginLevelLoad()
        {
            insideLevelLoad = true;
            for (var i = 0; i < listCache.Count; i++)
            {
                var sample = listCache[i];
                if (sample == null) continue;
                if (C.com_purgeAll.Bool) sample.PurgeSoundSample();
                sample.levelLoadReferenced = false;
            }
            soundCacheAllocator.FreeEmptyBaseBlocks();
        }

        // Free all samples marked as unused
        public void EndLevelLoad()
        {
            int useCount, purgeCount;
            common.Printf("----- SoundCache::EndLevelLoad -----\n");
            insideLevelLoad = false;

            // purge the ones we don't need
            useCount = 0;
            purgeCount = 0;
            for (var i = 0; i < listCache.Count; i++)
            {
                var sample = listCache[i];
                if (sample == null) continue;
                if (sample.purged) continue;
                if (!sample.levelLoadReferenced)
                {
                    //common.Printf($"Purging {sample.name}\n");
                    purgeCount += sample.objectMemSize;
                    sample.PurgeSoundSample();
                }
                else useCount += sample.objectMemSize;
            }

            soundCacheAllocator.FreeEmptyBaseBlocks();

            common.Printf($"{useCount / 1024:5}k referenced\n");
            common.Printf($"{purgeCount / 1024:5}k purged\n");
        }

        public void PrintMemInfo(MemInfo mi)
        {
            int i, j, num = 0, total = 0;
            int[] sortIndex;
            VFile f;

            f = fileSystem.OpenFileWrite($"{mi.filebase}_sounds.txt");
            if (f == null)
                return;

            // count
            for (i = 0; i < listCache.Count; i++, num++) if (listCache[i] == null) break;

            // sort first
            sortIndex = new int[num];

            for (i = 0; i < num; i++) sortIndex[i] = i;

            for (i = 0; i < num - 1; i++)
                for (j = i + 1; j < num; j++)
                    if (listCache[sortIndex[i]].objectMemSize < listCache[sortIndex[j]].objectMemSize)
                    {
                        var temp = sortIndex[i];
                        sortIndex[i] = sortIndex[j];
                        sortIndex[j] = temp;
                    }

            // print next
            for (i = 0; i < num; i++)
            {
                var sample = listCache[sortIndex[i]];

                // this is strange
                if (sample == null) continue;

                total += sample.objectMemSize;
                f.Printf($"{sample.objectMemSize:n} {sample.name}\n");
            }

            mi.soundAssetsTotal = total;

            f.Printf("\nTotal sound bytes allocated: {total:n}\n");
            fileSystem.CloseFile(f);
        }
    }

    // This class holds the actual wavefile bitmap, size, and info
    public class SoundSample
    {
        public const int SCACHE_SIZE = Simd.MIXBUFFER_SAMPLES * 20; // 1/2 of a second (aroundabout)

        public string name;                     // name of the sample file
        public DateTime timestamp;              // the most recent of all images used in creation, for reloadImages command

        public WaveformatEx objectInfo;         // what are we caching
        public int objectSize;                  // size of waveform in samples, excludes the header
        public int objectMemSize;               // object size in memory
        public byte[] nonCacheData;             // if it's not cached
        public byte[] amplitudeData;                // precomputed min,max amplitude pairs
        public int openalBuffer;                // openal buffer
        public bool hardwareBuffer;
        public bool defaultSound;
        public bool onDemand;
        public bool purged;
        public bool levelLoadReferenced;        // so we can tell which samples aren't needed any more

        public SoundSample()
        {
            objectInfo = default;
            objectSize = 0;
            objectMemSize = 0;
            nonCacheData = null;
            amplitudeData = null;
            openalBuffer = 0;
            hardwareBuffer = false;
            defaultSound = false;
            onDemand = false;
            purged = false;
            levelLoadReferenced = false;
        }

        public void Dispose()
            => PurgeSoundSample();

        // objectSize is samples
        public int LengthIn44kHzSamples
        {
            get
            {
                if (objectInfo.nSamplesPerSec == 11025) return objectSize << 2;
                else if (objectInfo.nSamplesPerSec == 22050) return objectSize << 1;
                else return objectSize << 0;
            }
        }

        public DateTime NewTimeStamp
        {
            get
            {
                fileSystem.ReadFile(name, out var timestamp);
                if (timestamp == DateTime.MinValue)
                {
                    var oggName = $"{name.Substring(0, name.Length - Path.GetExtension(name).Length)}.ogg";
                    fileSystem.ReadFile(oggName, out timestamp);
                }
                return timestamp;
            }
        }

        public unsafe void MakeDefault()             // turns it into a beep
        {
            objectInfo = default;
            objectInfo.nChannels = 1;
            objectInfo.wBitsPerSample = 16;
            objectInfo.nSamplesPerSec = 44100;

            objectSize = Simd.MIXBUFFER_SAMPLES * 2;
            objectMemSize = objectSize * sizeof(short);

            nonCacheData = SoundCache.soundCacheAllocator.Alloc(objectMemSize);

            fixed (byte* valueB = nonCacheData)
            {
                var ncd = (short*)valueB;
                for (var i = 0; i < Simd.MIXBUFFER_SAMPLES; i++)
                {
                    var v = (float)Math.Sin(MathX.PI * 2 * i / 64);
                    var sample = (short)(v * 0x4000);
                    ncd[i * 2 + 0] = sample;
                    ncd[i * 2 + 1] = sample;
                }
            }

            AL.GetError();
            AL.GenBuffers(1, ref openalBuffer);
            if (AL.GetError() != ALError.NoError) common.Error("SoundCache: error generating OpenAL hardware buffer");

            AL.GetError();
            AL.BufferData(openalBuffer, objectInfo.nChannels == 1 ? ALFormat.Mono16 : ALFormat.Stereo16, ref nonCacheData[0], objectMemSize, objectInfo.nSamplesPerSec);
            if (AL.GetError() != ALError.NoError) { common.Warning("SoundCache: error loading data into OpenAL hardware buffer"); }
            else hardwareBuffer = true;

            defaultSound = true;
        }

        // Loads based on name, possibly doing a MakeDefault if necessary
        public unsafe void Load()                        // loads the current sound based on name
        {
            defaultSound = false;
            purged = false;
            hardwareBuffer = false;

            timestamp = NewTimeStamp;

            if (timestamp == DateTime.MinValue) { common.Warning($"Couldn't load sound '{name}' using default"); MakeDefault(); return; }

            // load it
            WaveFile fh = new();
            if (fh.Open(name, out var info) == -1) { common.Warning($"Couldn't load sound '{name}' using default"); MakeDefault(); return; }
            if (info.nChannels != 1 && info.nChannels != 2) { common.Warning($"SoundSample: {name} has {info.nChannels} channels, using default"); fh.Close(); MakeDefault(); return; }
            if (info.wBitsPerSample != 16) { common.Warning($"SoundSample: {name} is {info.wBitsPerSample}bits, expected 16bits using default"); fh.Close(); MakeDefault(); return; }
            if (info.nSamplesPerSec != 44100 && info.nSamplesPerSec != 22050 && info.nSamplesPerSec != 11025) { common.Warning("SoundCache: {name} is {info.nSamplesPerSec}Hz, expected 11025, 22050 or 44100 Hz. Using default"); fh.Close(); MakeDefault(); return; }

            objectInfo = info;
            objectSize = fh.OutputSize;
            objectMemSize = fh.MemorySize;

            nonCacheData = SoundCache.soundCacheAllocator.Alloc(objectMemSize);
            fixed (byte* nonCacheData_ = nonCacheData) fh.Read(nonCacheData_, objectMemSize, null);

            // optionally convert it to 22kHz to save memory
            CheckForDownSample();

            // create hardware audio buffers. PCM loads directly
            if (objectInfo.wFormatTag == WAVE_FORMAT_TAG.PCM)
            {
                AL.GetError();
                AL.GenBuffers(1, ref openalBuffer);
                if (AL.GetError() != ALError.NoError)
                    common.Error("SoundCache: error generating OpenAL hardware buffer");
                if (AL.IsBuffer(openalBuffer))
                {
                    AL.GetError();
                    AL.BufferData(openalBuffer, objectInfo.nChannels == 1 ? ALFormat.Mono16 : ALFormat.Stereo16, ref nonCacheData[0], objectMemSize, objectInfo.nSamplesPerSec);
                    if (AL.GetError() != ALError.NoError) { common.Warning("SoundCache: error loading data into OpenAL hardware buffer"); hardwareBuffer = false; }
                    else hardwareBuffer = true;
                }

                // OGG decompressed at load time (when smaller than s_decompressionLimit seconds, 6 seconds by default)
                if (objectInfo.wFormatTag == WAVE_FORMAT_TAG.OGG && objectSize < (objectInfo.nSamplesPerSec * SoundSystemLocal.s_decompressionLimit.Integer))
                {
                    AL.GetError();
                    AL.GenBuffers(1, ref openalBuffer);
                    if (AL.GetError() != ALError.NoError) common.Error("SoundCache: error generating OpenAL hardware buffer");
                    if (AL.IsBuffer(openalBuffer))
                    {
                        var decoder = ISampleDecoder.Alloc();
                        var destData = SoundCache.soundCacheAllocator.Alloc((LengthIn44kHzSamples + 1) * sizeof(float));
                        fixed (byte* destDataB = destData)
                        {
                            var destDataF = (float*)destDataB;
                            var destDataS = (short*)destDataB;

                            // Decoder *always* outputs 44 kHz data
                            decoder.Decode(this, 0, LengthIn44kHzSamples, destDataF);

                            // Downsample back to original frequency (save memory)
                            if (objectInfo.nSamplesPerSec == 11025)
                                for (var i = 0; i < objectSize; i++)
                                {
                                    if (destDataF[i * 4] < -32768.0f) destDataS[i] = -32768;
                                    else if (destDataF[i * 4] > 32767.0f) destDataS[i] = 32767;
                                    else destDataS[i] = (short)MathX.FtoiFast(destDataF[i * 4]);
                                }
                            else if (objectInfo.nSamplesPerSec == 22050)
                                for (var i = 0; i < objectSize; i++)
                                {
                                    if (destDataF[i * 2] < -32768.0f) destDataS[i] = -32768;
                                    else if (destDataF[i * 2] > 32767.0f) destDataS[i] = 32767;
                                    else destDataS[i] = (short)MathX.FtoiFast(destDataF[i * 2]);
                                }
                            else
                                for (var i = 0; i < objectSize; i++)
                                {
                                    if (destDataF[i] < -32768.0f) destDataS[i] = -32768;
                                    else if (destDataF[i] > 32767.0f) destDataS[i] = 32767;
                                    else destDataS[i] = (short)MathX.FtoiFast(destDataF[i]);
                                }

                            AL.GetError();
                            AL.BufferData(openalBuffer, objectInfo.nChannels == 1 ? ALFormat.Mono16 : ALFormat.Stereo16, ref destData[0], objectSize * sizeof(short), objectInfo.nSamplesPerSec);
                            if (AL.GetError() != ALError.NoError) { common.Warning("SoundCache: error loading data into OpenAL hardware buffer"); hardwareBuffer = false; }
                            else hardwareBuffer = true;

                            SoundCache.soundCacheAllocator.Free(destData);
                        }
                        ISampleDecoder.Free(decoder);
                    }
                }
            }

            fh.Close();
        }

        public void Reload(bool force)        // reloads if timestamp has changed, or always if force
        {
            if (!force)
            {
                // check the timestamp
                var newTimestamp = NewTimeStamp;
                if (newTimestamp == DateTime.MinValue) { if (!defaultSound) { common.Warning($"Couldn't load sound '{name}' using default"); MakeDefault(); } return; }
                if (newTimestamp == timestamp) return; // don't need to reload it
            }

            common.Printf($"reloading {name}\n");
            PurgeSoundSample();
            Load();
        }

        public void PurgeSoundSample()            // frees all data
        {
            purged = true;

            AL.GetError();
            AL.DeleteBuffers(1, ref openalBuffer);
            if (AL.GetError() != ALError.NoError) common.Warning("SoundCache: error unloading data from OpenAL hardware buffer");

            openalBuffer = 0;
            hardwareBuffer = false;

            if (amplitudeData != null) SoundCache.soundCacheAllocator.Free(amplitudeData);
            if (nonCacheData != null) SoundCache.soundCacheAllocator.Free(nonCacheData);
        }

        public unsafe void CheckForDownSample()      // down sample if required
        {
            if (!SoundSystemLocal.s_force22kHz.Bool)
                return;
            if (objectInfo.wFormatTag != WAVE_FORMAT_TAG.PCM || objectInfo.nSamplesPerSec != 44100)
                return;
            var shortSamples = objectSize >> 1;
            var converted = SoundCache.soundCacheAllocator.Alloc(shortSamples * sizeof(short));
            fixed (byte* nonCacheDataB = nonCacheData, convertedB = converted)
            {
                var nonCacheDataS = (short*)nonCacheDataB;
                var convertedS = (short*)convertedB;
                if (objectInfo.nChannels == 1)
                    for (var i = 0; i < shortSamples; i++) convertedS[i] = nonCacheDataS[i * 2];
                else
                    for (var i = 0; i < shortSamples; i += 2)
                    {
                        convertedS[i + 0] = nonCacheDataS[i * 2 + 0];
                        convertedS[i + 1] = nonCacheDataS[i * 2 + 1];
                    }
                SoundCache.soundCacheAllocator.Free(nonCacheData);
                nonCacheData = converted;
                objectSize >>= 1;
                objectMemSize >>= 1;
                objectInfo.nAvgBytesPerSec >>= 1;
                objectInfo.nSamplesPerSec >>= 1;
            }
        }

        // Returns true on success.
        public bool FetchFromCache(int offset, out (byte[] v, int o) output, out int position, out int size, bool allowIO)
        {
            offset = unchecked((int)(offset & 0xfffffffe));

            if (objectSize == 0 || offset < 0 || offset > objectSize * sizeof(short) || nonCacheData == null) { output = default; position = 0; size = 0; return false; }

            output = (nonCacheData, offset);
            position = 0;
            size = objectSize * sizeof(short) - offset;
            if (size > SCACHE_SIZE) size = SCACHE_SIZE;
            return true;
        }
    }
}