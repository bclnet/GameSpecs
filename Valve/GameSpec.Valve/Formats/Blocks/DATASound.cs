using System;
using System.IO;
using System.Text;

namespace GameSpec.Valve.Formats.Blocks
{
    public class DATASound : DATA
    {
        public enum AudioFileType
        {
            AAC = 0,
            WAV = 1,
            MP3 = 2,
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
        public uint AudioFormat { get; private set; }

        public uint SampleSize { get; private set; }

        public uint SampleCount { get; private set; }

        public int LoopStart { get; private set; }

        public float Duration { get; private set; }

        public uint StreamingDataSize { get; private set; }

        protected BinaryPak Parent { get; private set; }

        public override void Read(BinaryPak parent, BinaryReader r)
        {
            Parent = parent;
            r.Position(Offset);
            if (parent.Version > 4) throw new InvalidDataException($"Invalid vsnd version '{parent.Version}'");
            if (parent.Version >= 4)
            {
                SampleRate = r.ReadUInt16();
                SetVersion4(r);
                SampleSize = Bits / 8;
                Channels = 1;
                AudioFormat = 1;
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
                AudioFormat = ExtractSub(bitpackedSoundInfo, 12, 2);
                SampleRate = ExtractSub(bitpackedSoundInfo, 14, 17);
            }
            LoopStart = r.ReadInt32();
            SampleCount = r.ReadUInt32();
            Duration = r.ReadSingle();
            r.Skip(12);
            StreamingDataSize = r.ReadUInt32();
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
            r.Position(Offset + Size);
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

                s.Write(headerRiff, 0, headerRiff.Length);
                s.Write(PackageInt(StreamingDataSize + 42, 4), 0, 4);

                s.Write(formatWave, 0, formatWave.Length);
                s.Write(formatTag, 0, formatTag.Length);
                s.Write(PackageInt(16, 4), 0, 4); // Subchunk1Size

                s.Write(PackageInt(AudioFormat, 2), 0, 2);
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
            s.Flush(); s.Seek(0, SeekOrigin.Begin);
            return s;
        }

        void SetVersion4(BinaryReader r)
        {
            var type = r.ReadUInt16();
            // We don't know if it's actually calculated, or if its a lookup
            switch (type)
            {
                case 0x0101: SoundType = AudioFileType.WAV; Bits = 8; break;
                case 0x0201: SoundType = AudioFileType.WAV; Bits = 8; break;
                case 0x0100: SoundType = AudioFileType.WAV; Bits = 16; break;
                case 0x0200: SoundType = AudioFileType.WAV; Bits = 32; break;
                case 0x0400: SoundType = AudioFileType.WAV; Bits = 32; break;
                case 0x0102: SoundType = AudioFileType.MP3; Bits = 16; break;
                case 0x0202: SoundType = AudioFileType.MP3; Bits = 32; break;
                //case 0x0203: // TODO: Unknown. In HL:A - pontoon_splash1 or switch_burst
                default: throw new NotImplementedException($"Unhandled v4 vsnd bits: {type}");
            }
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
            b.AppendLine($"LoopStart: {LoopStart}");
            var duration = TimeSpan.FromSeconds(Duration);
            b.AppendLine($"Duration: {duration} ({Duration})");
            return b.ToString();
        }
    }
}
