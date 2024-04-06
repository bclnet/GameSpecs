using System;
using System.Globalization;
using System.IO;
using System.Text;
using static GameX.Formats.Unknown.IUnknownFileObject;

namespace GameX.Valve.Formats.Blocks
{
    public readonly struct EmphasisSample
    {
        public float Time { get; }
        public float Value { get; }
    }

    public readonly struct PhonemeTag
    {
        internal PhonemeTag(float startTime, float endTime, ushort phonemeCode)
        {
            StartTime = startTime; 
            EndTime = endTime;
            PhonemeCode = phonemeCode;
        }
        public float StartTime { get; }
        public float EndTime { get; }
        public ushort PhonemeCode { get; }
    }

    public class Sentence
    {
        internal Sentence(PhonemeTag[] runTimePhonemes)
        {
            RunTimePhonemes = runTimePhonemes;
        }
        public bool ShouldVoiceDuck { get;}
        public PhonemeTag[] RunTimePhonemes { get; }
        public EmphasisSample[] EmphasisSamples { get; }
    }

    //was:Resource/ResourceTypes/Sound
    public class DATASound : DATA
    {
        public enum AudioFileType
        {
            AAC = 0,
            WAV = 1,
            MP3 = 2,
        }

        public enum AudioFormatV4
        {
            PCM16 = 0,
            PCM8 = 1,
            MP3 = 2,
            ADPCM = 3,
        }

        // https://github.com/naudio/NAudio/blob/fb35ce8367f30b8bc5ea84e7d2529e172cf4c381/NAudio.Core/Wave/WaveFormats/WaveFormatEncoding.cs
        public enum WaveAudioFormat : uint
        {
            Unknown = 0,
            PCM = 1,
            ADPCM = 2,
        }

        /// <summary>
        /// Gets the audio file type.
        /// </summary>
        /// <value>The file type.</value>
        public AudioFileType SoundType { get; private set; }

        /// <summary>
        /// Gets the samples per second.
        /// </summary>
        /// <value>The sample rate.</value>
        public uint SampleRate { get; private set; }

        /// <summary>
        /// Gets the bit size.
        /// </summary>
        /// <value>The bit size.</value>
        public uint Bits { get; private set; }

        /// <summary>
        /// Gets the number of channels. 1 for mono, 2 for stereo.
        /// </summary>
        /// <value>The number of channels.</value>
        public uint Channels { get; private set; }

        /// <summary>
        /// Gets the bitstream encoding format.
        /// </summary>
        /// <value>The audio format.</value>
        public WaveAudioFormat AudioFormat { get; private set; }

        public uint SampleSize { get; private set; }

        public uint SampleCount { get; private set; }

        public int LoopStart { get; private set; }

        public int LoopEnd { get; private set; }

        public float Duration { get; private set; }

        public Sentence Sentence { get; private set; }

        public uint StreamingDataSize { get; private set; }

        protected Binary_Pak Parent { get; private set; }

        public override void Read(Binary_Pak parent, BinaryReader r)
        {
            Parent = parent;
            r.Seek(Offset);
            if (parent.Version > 4) throw new InvalidDataException($"Invalid vsnd version '{parent.Version}'");
            if (parent.Version >= 4)
            {
                SampleRate = r.ReadUInt16();
                var soundFormat = (AudioFormatV4)r.ReadByte();
                Channels = r.ReadByte();
                switch (soundFormat)
                {
                    case AudioFormatV4.PCM8:
                        SoundType = AudioFileType.WAV;
                        Bits = 8;
                        SampleSize = 1;
                        AudioFormat = WaveAudioFormat.PCM;
                        break;
                    case AudioFormatV4.PCM16:
                        SoundType = AudioFileType.WAV;
                        Bits = 16;
                        SampleSize = 2;
                        AudioFormat = WaveAudioFormat.PCM;
                        break;
                    case AudioFormatV4.MP3:
                        SoundType = AudioFileType.MP3;
                        break;
                    case AudioFormatV4.ADPCM:
                        SoundType = AudioFileType.WAV;
                        Bits = 4;
                        SampleSize = 1;
                        AudioFormat = WaveAudioFormat.ADPCM;
                        throw new NotImplementedException("ADPCM is currently not implemented correctly.");
                    default: throw new ArgumentOutOfRangeException(nameof(soundFormat), $"Unexpected audio type {soundFormat}");
                }
            }
            else
            {
                var bitpackedSoundInfo = r.ReadUInt32();
                var type = ExtractSub(bitpackedSoundInfo, 0, 2);
                if (type > 2) throw new InvalidDataException($"Unknown sound type in old vsnd version: {type}");
                SoundType = (AudioFileType)type;
                Bits = ExtractSub(bitpackedSoundInfo, 2, 5);
                Channels = ExtractSub(bitpackedSoundInfo, 7, 2);
                SampleSize = ExtractSub(bitpackedSoundInfo, 9, 3);
                AudioFormat = (WaveAudioFormat)ExtractSub(bitpackedSoundInfo, 12, 2);
                SampleRate = ExtractSub(bitpackedSoundInfo, 14, 17);
            }
            LoopStart = r.ReadInt32();
            SampleCount = r.ReadUInt32();
            Duration = r.ReadSingle();

            var sentenceOffset = (long)r.ReadUInt32();
            r.Skip(4);
            if (sentenceOffset != 0) sentenceOffset = r.BaseStream.Position + sentenceOffset;

            r.Skip(4); // Skipping over m_pHeader
            StreamingDataSize = r.ReadUInt32();

            if (parent.Version >= 1)
            {
                var d = r.ReadUInt32();
                if (d != 0) throw new ArgumentOutOfRangeException(nameof(d), $"Unexpected {d}");
                var e = r.ReadUInt32();
                if (e != 0) throw new ArgumentOutOfRangeException(nameof(e), $"Unexpected {e}");
            }
            // v2 and v3 are the same?
            if (parent.Version >= 2)
            {
                var f = r.ReadUInt32();
                if (f != 0) throw new ArgumentOutOfRangeException(nameof(f), $"Unexpected {f}");
            }
            if (parent.Version >= 4) LoopEnd = r.ReadInt32();

            ReadPhonemeStream(r, sentenceOffset);
        }

        void ReadPhonemeStream(BinaryReader r, long sentenceOffset)
        {
            if (sentenceOffset == 0) return;
            r.Seek(sentenceOffset);
            var numPhonemeTags = r.ReadInt32();
            var a = r.ReadInt32(); // numEmphasisSamples ?
            var b = r.ReadInt32(); // Sentence.ShouldVoiceDuck ?
            // Skip sounds that have these
            if (a != 0 || b != 0) return;
            Sentence = new Sentence(new PhonemeTag[numPhonemeTags]);
            for (var i = 0; i < numPhonemeTags; i++)
            {
                Sentence.RunTimePhonemes[i] = new PhonemeTag(r.ReadSingle(), r.ReadSingle(), r.ReadUInt16());
                r.Skip(2);
            }
        }

        static uint ExtractSub(uint l, byte offset, byte nrBits)
        {
            var rightShifted = l >> offset;
            var mask = (1 << nrBits) - 1;
            return (uint)(rightShifted & mask);
        }

        /// <summary>
        /// Returns a fully playable sound data.
        /// In case of WAV files, header is automatically generated as Valve removes it when compiling.
        /// </summary>
        /// <returns>Byte array containing sound data.</returns>
        public byte[] GetSound()
        {
            using var sound = GetSoundStream();
            return sound.ToArray();
        }

        /// <summary>
        /// Returns a fully playable sound data.
        /// In case of WAV files, header is automatically generated as Valve removes it when compiling.
        /// </summary>
        /// <returns>Memory stream containing sound data.</returns>
        public MemoryStream GetSoundStream()
        {
            var r = Parent.Reader;
            r.Seek(Offset + Size);
            var s = new MemoryStream();
            if (SoundType == AudioFileType.WAV)
            {
                // http://soundfile.sapp.org/doc/WaveFormat/
                // http://www.codeproject.com/Articles/129173/Writing-a-Proper-Wave-File
                var headerRiff = new byte[] { 0x52, 0x49, 0x46, 0x46 };
                var formatWave = new byte[] { 0x57, 0x41, 0x56, 0x45 };
                var formatTag = new byte[] { 0x66, 0x6d, 0x74, 0x20 };
                var subChunkId = new byte[] { 0x64, 0x61, 0x74, 0x61 };

                var byteRate = SampleRate * Channels * (Bits / 8);
                var blockAlign = Channels * (Bits / 8);
                if (AudioFormat == WaveAudioFormat.ADPCM)
                {
                    byteRate = 1;
                    blockAlign = 4;
                }

                s.Write(headerRiff, 0, headerRiff.Length);
                s.Write(PackageInt(StreamingDataSize + 42, 4), 0, 4);

                s.Write(formatWave, 0, formatWave.Length);
                s.Write(formatTag, 0, formatTag.Length);
                s.Write(PackageInt(16, 4), 0, 4); // Subchunk1Size

                s.Write(PackageInt((uint)AudioFormat, 2), 0, 2);
                s.Write(PackageInt(Channels, 2), 0, 2);
                s.Write(PackageInt(SampleRate, 4), 0, 4);
                s.Write(PackageInt(byteRate, 4), 0, 4);
                s.Write(PackageInt(blockAlign, 2), 0, 2);
                s.Write(PackageInt(Bits, 2), 0, 2);
                //s.Write(PackageInt(0,2), 0, 2); // Extra param size
                s.Write(subChunkId, 0, subChunkId.Length);
                s.Write(PackageInt(StreamingDataSize, 4), 0, 4);
            }
            r.BaseStream.CopyTo(s, (int)StreamingDataSize);
            // Flush and reset position so that consumers can read it
            s.Flush();
            s.Seek(0, SeekOrigin.Begin);
            return s;
        }

        static byte[] PackageInt(uint source, int length)
        {
            var retVal = new byte[length];
            retVal[0] = (byte)(source & 0xFF);
            retVal[1] = (byte)((source >> 8) & 0xFF);
            if (length == 4)
            {
                retVal[2] = (byte)((source >> 0x10) & 0xFF);
                retVal[3] = (byte)((source >> 0x18) & 0xFF);
            }
            return retVal;
        }

        public override string ToString()
        {
            var b = new StringBuilder();
            b.AppendLine($"SoundType: {SoundType}");
            b.AppendLine($"Sample Rate: {SampleRate}");
            b.AppendLine($"Bits: {Bits}");
            b.AppendLine($"SampleSize: {SampleSize}");
            b.AppendLine($"SampleCount: {SampleCount}");
            b.AppendLine($"Format: {AudioFormat}");
            b.AppendLine($"Channels: {Channels}");
            b.AppendLine($"LoopStart: ({TimeSpan.FromSeconds(LoopStart)}) {LoopStart}");
            b.AppendLine($"LoopEnd: ({TimeSpan.FromSeconds(LoopEnd)}) {LoopEnd}");
            b.AppendLine($"Duration: {TimeSpan.FromSeconds(Duration)} ({Duration})");
            b.AppendLine($"StreamingDataSize: {StreamingDataSize}");
            if (Sentence != null)
            {
                b.AppendLine($"Sentence[{Sentence.RunTimePhonemes.Length}]:");
                foreach (var phoneme in Sentence.RunTimePhonemes) b.AppendLine($"\tPhonemeTag(StartTime={phoneme.StartTime}, EndTime={phoneme.EndTime}, PhonemeCode={phoneme.PhonemeCode})");
            }
            return b.ToString();
        }
    }
}
