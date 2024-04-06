using OggVorbis;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static OggVorbis.Vorbis;
using static System.NumericsX.OpenStack.Gngine.Sound.Lib;
using static System.NumericsX.OpenStack.OpenStack;
using FourCC = System.Int32;

namespace System.NumericsX.OpenStack.Gngine.Sound
{
    // flags for wFormatTag field of WAVEFORMAT
    public enum WAVE_FORMAT_TAG : short
    {
        PCM = 1,
        OGG = 2
    }

    public struct WaveformatEx
    {
        public WAVE_FORMAT_TAG wFormatTag;        // format type
        public short nChannels;         // number of channels (i.e. mono, stereo...)
        public int nSamplesPerSec;    // sample rate
        public int nAvgBytesPerSec;   // for buffer estimation
        public short nBlockAlign;       // block size of data
        public short wBitsPerSample;    // Number of bits per sample of mono data
        public short cbSize;            // The count in bytes of the size of extra information (after cbSize)
    }

    // OLD general waveform format structure (information common to all formats)
    [StructLayout(LayoutKind.Sequential)]
    public struct Waveformat
    {
        public WAVE_FORMAT_TAG wFormatTag;        // format type
        public short nChannels;         // number of channels (i.e. mono, stereo, etc.)
        public int nSamplesPerSec;    // sample rate
        public int nAvgBytesPerSec;   // for buffer estimation
        public short nBlockAlign;       // block size of data
    }

    // specific waveform format structure for PCM data
    [StructLayout(LayoutKind.Sequential)]
    public struct PcmWaveFormat
    {
        public Waveformat wf;
        public short wBitsPerSample;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct WaveformatExtensibleSamples
    {
        [FieldOffset(0)] public short wValidBitsPerSample;       // bits of precision 
        [FieldOffset(0)] public short wSamplesPerBlock;          // valid if wBitsPerSample==0
        [FieldOffset(0)] public short wReserved;                 // If neither applies, set to zero
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WaveformatExtensible
    {
        //public static implicit operator WaveformatExtensible(WaveformatEx format)
        //{
        //    throw new NotImplementedException();
        //}
        public WaveformatEx Format;
        public WaveformatExtensibleSamples Samples;
        public int dwChannelMask;      // which channels are
        public int SubFormat; // present in stream 

        internal unsafe void memcpy(ref PcmWaveFormat pcmWaveFormat)
        {
            fixed (PcmWaveFormat* _ = &pcmWaveFormat) this = *(WaveformatExtensible*)_;
        }
    }

    // RIFF chunk information data structure
    public struct Mminfo
    {
        public FourCC ckid;           // chunk ID //
        public uint cksize;         // chunk size //
        public FourCC fccType;        // form type or list type //
        public int dwDataOffset;   // offset of data portion of chunk //
    }

    // OggVorbis file loading/decoding.
    public static class OggVorbis
    {
        unsafe static nint FS_ReadOGG(byte* dest, nint size1, nint size2, object fh)
        {
            var f = (VFile)fh;
            return f.Read(dest, (int)(size1 * size2));
        }

        static int FS_SeekOGG(object fh, long to, int type)
        {
            const int SEEK_SET = 0;
            const int SEEK_CUR = 1;
            const int SEEK_END = 2;

            var retype = FS_SEEK.SET;
            if (type == SEEK_CUR) retype = FS_SEEK.CUR;
            else if (type == SEEK_END) retype = FS_SEEK.END;
            else if (type == SEEK_SET) retype = FS_SEEK.SET;
            else common.FatalError("fs_seekOGG: seek without type\n");
            var f = (VFile)fh;
            return f.Seek(to, retype);
        }

        static int FS_CloseOGG(object fh)
            => 0;

        static nint FS_TellOGG(object fh)
        {
            var f = (VFile)fh;
            return f.Tell;
        }

        public unsafe static int ov_openFile(VFile f, OggVorbis_File vf)
        {
            vf.memset();
            ov_callbacks callbacks;
            callbacks.read_func = Marshal.GetFunctionPointerForDelegate<ov_callbacks.ReadFuncDelegate>(FS_ReadOGG);
            callbacks.seek_func = Marshal.GetFunctionPointerForDelegate<ov_callbacks.SeekFuncDelegate>(FS_SeekOGG);
            callbacks.close_func = Marshal.GetFunctionPointerForDelegate<ov_callbacks.CloseFuncDelegate>(FS_CloseOGG);
            callbacks.tell_func = Marshal.GetFunctionPointerForDelegate<ov_callbacks.TellFuncDelegate>(FS_TellOGG);
            return ov_open_callbacks(f, vf, null, -1, callbacks);
        }
    }

    public partial class WaveFile
    {
        unsafe int OpenOGG(string strFileName, out WaveformatEx pwfx)
        {
            pwfx = default;
            mhmmio = fileSystem.OpenFileRead(strFileName);
            if (mhmmio == null) return -1;

            ISystem.EnterCriticalSection(CRITICAL_SECTION.SECTION_ONE);

            var ov = new OggVorbis_File();

            if (OggVorbis.ov_openFile(mhmmio, ov) < 0) { ISystem.LeaveCriticalSection(CRITICAL_SECTION.SECTION_ONE); fileSystem.CloseFile(mhmmio); mhmmio = null; return -1; }

            mfileTime = mhmmio.Timestamp;

            var vi = ov_info(ov, -1);

            mpwfx.Format.nSamplesPerSec = (int)vi->rate;
            mpwfx.Format.nChannels = (short)vi->channels;
            mpwfx.Format.wBitsPerSample = sizeof(short) * 8;
            mdwSize = (int)(ov_pcm_total(ov, -1) * vi->channels);   // pcm samples * num channels
            mbIsReadingFromMemory = false;

            if (SoundSystemLocal.s_realTimeDecoding.Bool)
            {
                ov_clear(ov);
                fileSystem.CloseFile(mhmmio);
                mhmmio = null;

                mpwfx.Format.wFormatTag = WAVE_FORMAT_TAG.OGG;
                mhmmio = fileSystem.OpenFileRead(strFileName);
                mMemSize = mhmmio.Length;
            }
            else
            {
                ogg = ov;

                mpwfx.Format.wFormatTag = WAVE_FORMAT_TAG.PCM;
                mMemSize = mdwSize * sizeof(short);
            }

            pwfx = mpwfx.Format;

            ISystem.LeaveCriticalSection(CRITICAL_SECTION.SECTION_ONE);
            isOgg = true;
            return 0;
        }

        unsafe int ReadOGG(byte* pBuffer, int dwSizeToRead, Action<int> pdwSizeRead)
        {
            var total = dwSizeToRead;
            var bufferPtr = pBuffer;
            var ov = (OggVorbis_File)ogg;

            do
            {
                var ret = (int)ov_read(ov, bufferPtr, total >= 4096 ? 4096 : total, Platform.IsBigEndian ? 1 : 0, 2, 1, null);
                if (ret == 0) break;
                if (ret < 0) return -1;
                bufferPtr += ret;
                total -= ret;
            } while (total > 0);

            dwSizeToRead = (int)(bufferPtr - pBuffer);

            pdwSizeRead?.Invoke(dwSizeToRead);
            return dwSizeToRead;
        }

        unsafe int CloseOGG()
        {
            var ov = (OggVorbis_File)ogg;
            if (ov != null)
            {
                ISystem.EnterCriticalSection(CRITICAL_SECTION.SECTION_ONE);
                ov_clear(ov);
                ISystem.LeaveCriticalSection(CRITICAL_SECTION.SECTION_ONE);
                fileSystem.CloseFile(mhmmio);
                mhmmio = null;
                ogg = null;
                return 0;
            }
            return -1;
        }
    }

    // Sound sample decoder
    public interface ISampleDecoder
    {
        const int MIN_OGGVORBIS_MEMORY = 768 * 1024;
        // Thread safe decoder memory allocator. Each OggVorbis decoder consumes about 150kB of memory.
        internal static DynamicBlockAlloc<byte> decoderMemoryAllocator = new(1 << 20, 128, 0, x => new byte[x]);
        internal static BlockAlloc<SampleDecoderLocal> sampleDecoderAllocator = new(64);

        static DynamicElement<byte> decoder_malloc(int size)
        {
            var ptr = decoderMemoryAllocator.Alloc(size);
            Debug.Assert(size == 0 || ptr != null);
            return ptr;
        }

        static DynamicElement<byte> decoder_calloc(int num, int size)
        {
            var ptr = decoderMemoryAllocator.Alloc(num * size);
            Debug.Assert((num * size) == 0 || ptr != null);
            return ptr;
        }

        static DynamicElement<byte> decoder_realloc(DynamicElement<byte> memblock, int size)
        {
            var ptr = decoderMemoryAllocator.Resize(memblock, size);
            Debug.Assert(size == 0 || ptr.Value != null);
            return ptr;
        }

        static void decoder_free(DynamicElement<byte> memblock)
            => decoderMemoryAllocator.Free(memblock);

        public static void Init()
        {
            decoderMemoryAllocator.Init();
            decoderMemoryAllocator.SetLockMemory(true);
            decoderMemoryAllocator.SetFixedBlocks(SoundSystemLocal.s_realTimeDecoding.Bool ? 10 : 1);
        }

        public static void Shutdown()
        {
            decoderMemoryAllocator.Shutdown();
            sampleDecoderAllocator.Shutdown();
        }
        public static ISampleDecoder Alloc()
        {
            var decoder = sampleDecoderAllocator.Alloc();
            decoder.Clear();
            return decoder;
        }
        public static void Free(ISampleDecoder decoder)
        {
            var localDecoder = (SampleDecoderLocal)decoder;
            localDecoder.ClearDecoder();
            sampleDecoderAllocator.Free(localDecoder);
        }

        public static int NumUsedBlocks
            => decoderMemoryAllocator.NumUsedBlocks;

        public static int UsedBlockMemory
            => decoderMemoryAllocator.UsedBlockMemory;

        unsafe void Decode(SoundSample sample, int sampleOffset44k, int sampleCount44k, float* dest);
        void ClearDecoder();
        SoundSample Sample { get; }
        int LastDecodeTime { get; }
    }

    public class SampleDecoderLocal : BlockAllocElement<SampleDecoderLocal>, ISampleDecoder
    {
        bool failed;                    // set if decoding failed
        WAVE_FORMAT_TAG lastFormat;     // last format being decoded
        SoundSample lastSample;         // last sample being decoded
        int lastSampleOffset;           // last offset into the decoded sample
        int lastDecodeTime;             // last time decoding sound
        VFile_Memory file;              // encoded file in memory

        OggVorbis_File ogg;             // OggVorbis file

        public unsafe virtual void Decode(SoundSample sample, int sampleOffset44k, int sampleCount44k, float* dest)
        {
            if (sample.objectInfo.wFormatTag != lastFormat || sample != lastSample) ClearDecoder();

            lastDecodeTime = soundSystemLocal.CurrentSoundTime;

            if (failed) { UnsafeX.InitBlock(dest, 0, sampleCount44k * sizeof(float)); return; }

            // samples can be decoded both from the sound thread and the main thread for shakes
            ISystem.EnterCriticalSection(CRITICAL_SECTION.SECTION_ONE);
            var readSamples44k = sample.objectInfo.wFormatTag switch
            {
                WAVE_FORMAT_TAG.PCM => DecodePCM(sample, sampleOffset44k, sampleCount44k, dest),
                WAVE_FORMAT_TAG.OGG => DecodeOGG(sample, sampleOffset44k, sampleCount44k, dest),
                _ => 0,
            };
            ISystem.LeaveCriticalSection(CRITICAL_SECTION.SECTION_ONE);

            if (readSamples44k < sampleCount44k) UnsafeX.InitBlock(dest + readSamples44k, 0, (sampleCount44k - readSamples44k) * sizeof(float));
        }

        public virtual void ClearDecoder()
        {
            ISystem.EnterCriticalSection(CRITICAL_SECTION.SECTION_ONE);
            switch (lastFormat)
            {
                case WAVE_FORMAT_TAG.PCM: break;
                case WAVE_FORMAT_TAG.OGG: ov_clear(ogg); ogg.memset(); break;
            }
            Clear();
            ISystem.LeaveCriticalSection(CRITICAL_SECTION.SECTION_ONE);
        }

        public virtual SoundSample Sample => lastSample;

        public virtual int LastDecodeTime => lastDecodeTime;

        public void Clear()
        {
            failed = false;
            lastFormat = WAVE_FORMAT_TAG.PCM;
            lastSample = null;
            lastSampleOffset = 0;
            lastDecodeTime = 0;
        }

        public unsafe int DecodePCM(SoundSample sample, int sampleOffset44k, int sampleCount44k, float* dest)
        {
            lastFormat = WAVE_FORMAT_TAG.PCM;
            lastSample = sample;

            var shift = 22050 / sample.objectInfo.nSamplesPerSec;
            var sampleOffset = sampleOffset44k >> shift;
            var sampleCount = sampleCount44k >> shift;

            if (sample.nonCacheData == null) { Debug.Assert(false); failed = true; return 0; }  // this should never happen ( note: I've seen that happen with the main thread down in idGameLocal::MapClear clearing entities - TTimo )
            if (!sample.FetchFromCache(sampleOffset * sizeof(short), out var first, out var pos, out var size, false)) { failed = true; return 0; }

            var readSamples = size - pos < sampleCount * sizeof(short) ? (size - pos) / sizeof(short) : sampleCount;

            // duplicate samples for 44kHz output
            fixed (byte* _ = &first.v[first.o + pos]) Simd.UpSamplePCMTo44kHz(dest, (short*)_, readSamples, sample.objectInfo.nSamplesPerSec, sample.objectInfo.nChannels);

            return readSamples << shift;
        }

        public unsafe int DecodeOGG(SoundSample sample, int sampleOffset44k, int sampleCount44k, float* dest)
        {
            int readSamples, totalSamples;

            var shift = 22050 / sample.objectInfo.nSamplesPerSec;
            var sampleOffset = sampleOffset44k >> shift;
            var sampleCount = sampleCount44k >> shift;

            // open OGG file if not yet opened
            if (lastSample == null)
            {
                // make sure there is enough space for another decoder
                if (ISampleDecoder.decoderMemoryAllocator.FreeBlockMemory < ISampleDecoder.MIN_OGGVORBIS_MEMORY) return 0;

                if (sample.nonCacheData == null) { Debug.Assert(false); failed = true; return 0; } // this should never happen
                file.SetData(sample.nonCacheData, sample.objectMemSize);
                if (OggVorbis.ov_openFile(file, ogg) < 0) { failed = true; return 0; }
                lastFormat = WAVE_FORMAT_TAG.OGG;
                lastSample = sample;
            }

            // seek to the right offset if necessary
            if (sampleOffset != lastSampleOffset && ov_pcm_seek(ogg, sampleOffset / sample.objectInfo.nChannels) != 0) { failed = true; return 0; }

            lastSampleOffset = sampleOffset;

            // decode OGG samples
            totalSamples = sampleCount;
            readSamples = 0;
            do
            {
                float** samples;
                var ret = (int)ov_read_float(ogg, &samples, totalSamples / sample.objectInfo.nChannels, null);
                if (ret == 0) { failed = true; break; }
                if (ret < 0) { failed = true; return 0; }
                ret *= sample.objectInfo.nChannels;

                Simd.UpSampleOGGTo44kHz(dest + (readSamples << shift), samples, ret, sample.objectInfo.nSamplesPerSec, sample.objectInfo.nChannels);

                readSamples += ret;
                totalSamples -= ret;
            } while (totalSamples > 0);

            lastSampleOffset += readSamples;

            return readSamples << shift;
        }
    }
}