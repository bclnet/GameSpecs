using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using static System.NumericsX.Platform;

namespace System.NumericsX.OpenStack
{
    public class BitMsg
    {
        static readonly byte[] EmptyByte = Array.Empty<byte>();

        byte[] writeData;       // pointer to data for writing
        byte[] readData;        // pointer to data for reading
        int maxSize;            // maximum size of message in bytes
        int curSize;            // current size of message in bytes
        int writeBit;           // number of bits written to the last written byte
        int readCount;          // number of bytes read so far
        int readBit;            // number of bits read from the last read byte
        bool allowOverflow;     // if false, generate an error when the message is overflowed
        bool overflowed;            // set to true if the buffer size failed (with allowOverflow set)

        public BitMsg()
        {
            writeData = null;
            readData = null;
            maxSize = 0;
            curSize = 0;
            writeBit = 0;
            readCount = 0;
            readBit = 0;
            allowOverflow = false;
            overflowed = false;
        }

        public void InitW(byte[] data)
        {
            writeData = data;
            readData = data;
            maxSize = data.Length;
        }
        public void InitW(byte[] data, int dataLength)
        {
            writeData = data;
            readData = data;
            maxSize = dataLength;
        }

        public void InitR(byte[] data)
        {
            writeData = null;
            readData = data;
            maxSize = data.Length;
        }
        public void InitR(byte[] data, int dataLength)
        {
            writeData = null;
            readData = data;
            maxSize = dataLength;
        }

        public byte[] DataW => writeData;                   // get data for writing

        public byte[] DataR => readData;                    // get data for reading

        public int MaxSize => maxSize;                      // get the maximum message size

        public void SetAllowOverflow(bool set) => allowOverflow = set; // generate error if not set and message is overflowed

        public bool IsOverflowed => overflowed;             // returns true if the message was overflowed

        public int Size
        {
            get => curSize; // size of the message in bytes
            set => curSize = value > maxSize ? maxSize : value; // set the message size
        }

        public int WriteBit
        {
            get => writeBit; // get current write bit
            set { writeBit = value & 7; if (writeBit != 0) writeData[curSize - 1] &= (byte)((1 << writeBit) - 1); } // set current write bit
        }

        public int NumBitsWritten => ((curSize << 3) - ((8 - writeBit) & 7)); // returns number of bits written

        public int RemainingWriteBits => (maxSize << 3) - NumBitsWritten; // space left in bits for writing

        public void SaveWriteState(out int s, out int b)   // save the write state
        {
            s = curSize;
            b = writeBit;
        }

        public void RestoreWriteState(int s, int b)        // restore the write state
        {
            curSize = s;
            writeBit = b & 7;
            if (writeBit != 0) writeData[curSize - 1] &= (byte)((1 << writeBit) - 1);
        }

        public int ReadCount
        {
            get => readCount; // bytes read so far
            set => readCount = value; // set the number of bytes and bits read
        }

        public int ReadBit
        {
            get => readBit; // get current read bit
            set => readBit = value & 7; // set current read bit
        }

        public int NumBitsRead => (readCount << 3) - ((8 - readBit) & 7);         // returns number of bits read

        public int RemainingReadBits => (curSize << 3) - NumBitsRead;      // number of bits left to read

        public void SaveReadState(out int c, out int b)   // save the read state
        {
            c = readCount;
            b = readBit;
        }

        public void RestoreReadState(int c, int b) // restore the read state
        {
            readCount = c;
            readBit = b & 7;
        }

        public void BeginWriting()                    // begin writing
        {
            curSize = 0;
            overflowed = false;
            writeBit = 0;
        }

        public int RemainingSpace => maxSize - curSize;       // space left in bytes

        public void WriteByteAlign() => writeBit = 0;                  // write up to the next byte boundary
        /// <summary>
        /// If the number of bits is negative a sign is included.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="numBits">The number bits.</param>
        public void WriteBits(int value, int numBits) // write the specified number of bits
        {
            if (writeData == null) Error("BitMsg::WriteBits: cannot write to message");

            // check if the number of bits is valid
            if (numBits == 0 || numBits < -31 || numBits > 32) Error($"BitMsg::WriteBits: bad numBits {numBits}");

            // check for value overflows
            // this should be an error really, as it can go unnoticed and cause either bandwidth or corrupted data transmitted
            if (numBits != 32)
            {
                if (numBits > 0)
                {
                    if (value > (1 << numBits) - 1) Warning($"BitMsg::WriteBits: value overflow {value} {numBits}");
                    else if (value < 0) Warning($"BitMsg::WriteBits: value overflow {value} {numBits}");
                }
                else
                {
                    var r = 1 << (-1 - numBits);
                    if (value > r - 1) Warning($"BitMsg::WriteBits: value overflow {value} {numBits}");
                    else if (value < -r) Warning($"BitMsg::WriteBits: value overflow {value} {numBits}");
                }
            }

            if (numBits < 0) numBits = -numBits;

            // check for msg overflow
            if (CheckOverflow(numBits)) return;

            // write the bits
            int put;
            int fraction;
            while (numBits != 0)
            {
                if (writeBit == 0) { writeData[curSize] = 0; curSize++; }
                put = 8 - writeBit;
                if (put > numBits) put = numBits;
                fraction = value & ((1 << put) - 1);
                writeData[curSize - 1] |= (byte)(fraction << writeBit);
                numBits -= put;
                value >>= put;
                writeBit = (writeBit + put) & 7;
            }
        }
        public void WriteChar(int c) => WriteBits(c, -8);
        public void WriteByte(int c) => WriteBits(c, 8);
        public void WriteShort(int c) => WriteBits(c, -16);
        public void WriteUShort(int c) => WriteBits(c, 16);
        public void WriteInt(int c) => WriteBits(c, 32);
        public void WriteFloat(float f) => WriteBits(reinterpret.cast_int(f), 32);
        public void WriteFloat(float f, int exponentBits, int mantissaBits)
        {
            var bits = MathX.FloatToBits(f, exponentBits, mantissaBits);
            WriteBits(bits, 1 + exponentBits + mantissaBits);
        }
        public void WriteAngle8(float f) => WriteByte(MathX.ANGLE2BYTE(f));
        public void WriteAngle16(float f) => WriteShort(MathX.ANGLE2SHORT(f));
        public void WriteDir(Vector3 dir, int numBits) => WriteBits(DirToBits(dir, numBits), numBits);
        public void WriteString(string s, int maxLength = -1, bool make7Bit = true)
        {
            if (s == null) WriteData(EmptyByte, 0, 1);
            else
            {
                var l = s.Length;
                if (maxLength >= 0 && l >= maxLength) l = maxLength - 1;
                var dataPtr = GetByteSpace(l + 1);
                var bytePtr = s;
                int i;
                if (make7Bit) for (i = 0; i < l; i++) dataPtr[i] = bytePtr[i] > 127 ? (byte)'.' : (byte)bytePtr[i];
                else for (i = 0; i < l; i++) dataPtr[i] = (byte)bytePtr[i];
                dataPtr[i] = 0;
            }
        }
        public void WriteData(byte[] data, int offset, int length)
            => Unsafe.CopyBlock(ref GetByteSpace(length)[0], ref data[offset], (uint)length);
        public void WriteNetadr(Netadr adr)
        {
            Unsafe.CopyBlock(ref GetByteSpace(4)[0], ref adr.ip[0], 4U);
            WriteUShort(adr.port);
        }

        public void WriteDeltaChar(int oldValue, int newValue) => WriteDelta(oldValue, newValue, -8);
        public void WriteDeltaByte(int oldValue, int newValue) => WriteDelta(oldValue, newValue, 8);
        public void WriteDeltaShort(int oldValue, int newValue) => WriteDelta(oldValue, newValue, -16);
        public void WriteDeltaInt(int oldValue, int newValue) => WriteDelta(oldValue, newValue, 32);
        public void WriteDeltaFloat(float oldValue, float newValue) => WriteDelta(reinterpret.cast_int(oldValue), reinterpret.cast_int(newValue), 32);
        public void WriteDeltaFloat(float oldValue, float newValue, int exponentBits, int mantissaBits)
        {
            var oldBits = MathX.FloatToBits(oldValue, exponentBits, mantissaBits);
            var newBits = MathX.FloatToBits(newValue, exponentBits, mantissaBits);
            WriteDelta(oldBits, newBits, 1 + exponentBits + mantissaBits);
        }
        public void WriteDeltaByteCounter(int oldValue, int newValue)
        {
            int i;
            var x = oldValue ^ newValue;
            for (i = 7; i > 0; i--) if ((x & (1 << i)) != 0) { i++; break; }
            WriteBits(i, 3);
            if (i != 0) WriteBits(((1 << i) - 1) & newValue, i);
        }
        public void WriteDeltaShortCounter(int oldValue, int newValue)
        {
            int i;
            var x = oldValue ^ newValue;
            for (i = 15; i > 0; i--) if ((x & (1 << i)) != 0) { i++; break; }
            WriteBits(i, 4);
            if (i != 0) WriteBits(((1 << i) - 1) & newValue, i);
        }
        public void WriteDeltaIntCounter(int oldValue, int newValue)
        {
            int i;
            var x = oldValue ^ newValue;
            for (i = 31; i > 0; i--) if ((x & (1 << i)) != 0) { i++; break; }
            WriteBits(i, 5);
            if (i != 0) WriteBits(((1 << i) - 1) & newValue, i);
        }
        public bool WriteDeltaDict(Dictionary<string, string> dict, Dictionary<string, string> @base)
        {
            var changed = false;
            if (@base != null)
            {
                foreach (var kv in dict) if (!@base.TryGetValue(kv.Key, out var basekvValue) || !string.Equals(basekvValue, kv.Value)) { WriteString(kv.Key); WriteString(kv.Value); changed = true; }
                WriteString(string.Empty);
                foreach (var basekv in @base) if (!dict.ContainsKey(basekv.Key)) { WriteString(basekv.Key); changed = true; }
                WriteString(string.Empty);
            }
            else
            {
                foreach (var kv in dict) { WriteString(kv.Key); WriteString(kv.Value); changed = true; }
                WriteString(string.Empty);
                WriteString(string.Empty);
            }
            return changed;
        }

        public void BeginReading()              // begin reading.
        {
            readCount = 0;
            readBit = 0;
        }
        public int RemaingData => curSize - readCount;         // number of bytes left to read
        public void ReadByteAlign() => readBit = 0;         // read up to the next byte boundary
        /// <summary>
        /// If the number of bits is negative a sign is included.
        /// </summary>
        /// <param name="numBits">The number bits.</param>
        /// <returns></returns>
        public int ReadBits(int numBits)            // read the specified number of bits
        {
            if (readData == null) FatalError("BitMsg::ReadBits: cannot read from message");

            // check if the number of bits is valid
            if (numBits == 0 || numBits < -31 || numBits > 32) FatalError($"BitMsg::ReadBits: bad numBits {numBits}");

            bool sgn;
            if (numBits < 0) { numBits = -numBits; sgn = true; }
            else sgn = false;

            // check for overflow
            if (numBits > RemainingReadBits) return -1;

            var value = 0;
            var valueBits = 0;
            int get;
            int fraction;
            while (valueBits < numBits)
            {
                if (readBit == 0) readCount++;
                get = 8 - readBit;
                if (get > (numBits - valueBits)) get = numBits - valueBits;
                fraction = readData[readCount - 1];
                fraction >>= readBit;
                fraction &= (1 << get) - 1;
                value |= fraction << valueBits;

                valueBits += get;
                readBit = (readBit + get) & 7;
            }

            if (sgn && (value & (1 << (numBits - 1))) != 0) value |= -1 ^ ((1 << numBits) - 1);

            return value;
        }
        public char ReadChar() => (char)ReadBits(-8);
        public byte ReadByte() => (byte)ReadBits(8);
        public short ReadShort() => (short)ReadBits(-16);
        public ushort ReadUShort() => (ushort)ReadBits(16);
        public int ReadInt() => ReadBits(32);
        public float ReadFloat() => reinterpret.cast_int(ReadBits(32));
        public float ReadFloat(int exponentBits, int mantissaBits)
        {
            var bits = ReadBits(1 + exponentBits + mantissaBits);
            return MathX.BitsToFloat(bits, exponentBits, mantissaBits);
        }
        public float ReadAngle8() => MathX.BYTE2ANGLE(ReadByte());
        public float ReadAngle16() => MathX.SHORT2ANGLE(ReadShort());
        public Vector3 ReadDir(int numBits) => BitsToDir(ReadBits(numBits), numBits);
        public unsafe int ReadString(out string s, int bufferSize = Platform.MAX_STRING_CHARS)
        {
            var buffer = stackalloc byte[bufferSize];
            int l; byte c;
            ReadByteAlign();
            l = 0;
            while (true)
            {
                c = (byte)ReadByte();
                if (c <= 0 || c >= 255) break;
                // translate all fmt spec to avoid crash bugs in string routines
                if (c == '%') c = (byte)'.';
                // we will read past any excessively long string, so the following data can be read, but the string will be truncated
                if (l < bufferSize) { buffer[l] = c; l++; }
            }
            s = Encoding.ASCII.GetString(buffer, l);
            return l;
        }
        public int ReadString(out string s, byte[] buffer)
        {
            int l; byte c;
            ReadByteAlign();
            l = 0;
            while (true)
            {
                c = ReadByte();
                if (c <= 0 || c >= 255) break;
                // translate all fmt spec to avoid crash bugs in string routines
                if (c == '%') c = (byte)'.';
                // we will read past any excessively long string, so the following data can be read, but the string will be truncated
                if (l < buffer.Length) { buffer[l] = c; l++; }
            }
            s = Encoding.ASCII.GetString(buffer, 0, l);
            return l;
        }
        public int ReadData(byte[] data, int length)
        {
            ReadByteAlign();
            var cnt = readCount;
            if (readCount + length > curSize)
            {
                if (data != null) Unsafe.CopyBlock(ref data[0], ref readData.AsSpan(readCount)[0], (uint)RemaingData);
                readCount = curSize;
            }
            else
            {
                if (data != null) Unsafe.CopyBlock(ref data[0], ref readData.AsSpan(readCount)[0], (uint)length);
                readCount += length;
            }
            return readCount - cnt;
        }
        public void ReadNetadr(out Netadr adr)
        {
            adr = new Netadr { type = NA.IP };
            for (var i = 0; i < 4; i++) adr.ip[i] = ReadByte();
            adr.port = ReadUShort();
        }

        public char ReadDeltaChar(int oldValue) => (char)ReadDelta(oldValue, -8);
        public byte ReadDeltaByte(int oldValue) => (byte)ReadDelta(oldValue, 8);
        public short ReadDeltaShort(int oldValue) => (short)ReadDelta(oldValue, -16);
        public int ReadDeltaInt(int oldValue) => ReadDelta(oldValue, 32);
        public float ReadDeltaFloat(float oldValue) => reinterpret.cast_int(ReadDelta(reinterpret.cast_int(oldValue), 32));
        public float ReadDeltaFloat(float oldValue, int exponentBits, int mantissaBits)
        {
            var oldBits = MathX.FloatToBits(oldValue, exponentBits, mantissaBits);
            var newBits = ReadDelta(oldBits, 1 + exponentBits + mantissaBits);
            return MathX.BitsToFloat(newBits, exponentBits, mantissaBits);
        }
        public int ReadDeltaByteCounter(int oldValue)
        {
            var i = ReadBits(3);
            if (i == 0) return oldValue;
            var newValue = ReadBits(i);
            return ((oldValue & ~((1 << i) - 1)) | newValue);
        }
        public int ReadDeltaShortCounter(int oldValue)
        {
            var i = ReadBits(4);
            if (i == 0) return oldValue;
            var newValue = ReadBits(i);
            return (oldValue & ~((1 << i) - 1)) | newValue;

        }
        public int ReadDeltaIntCounter(int oldValue)
        {
            var i = ReadBits(5);
            if (i == 0) return oldValue;
            var newValue = ReadBits(i);
            return (oldValue & ~((1 << i) - 1)) | newValue;
        }
        public bool ReadDeltaDict(Dictionary<string, string> dict, Dictionary<string, string> @base)
        {
            var keybuf = new byte[Platform.MAX_STRING_CHARS];
            var valuebuf = new byte[Platform.MAX_STRING_CHARS];
            var changed = false;

            if (@base != null) dict = @base;
            else dict.Clear();

            while (ReadString(out var key, keybuf) != 0) { ReadString(out var value, valuebuf); dict[key] = value; changed = true; }
            while (ReadString(out var key, keybuf) != 0) { dict.Remove(key); changed = true; }
            return changed;
        }

        public static int DirToBits(Vector3 dir, int numBits)
        {
            Debug.Assert(numBits >= 6 && numBits <= 32);
            Debug.Assert(dir.LengthSqr - 1.0f < 0.01f);

            numBits /= 3;
            var max = (1 << (numBits - 1)) - 1;
            var bias = 0.5f / max;

            var bits = MathX.FLOATSIGNBITSET(dir.x) ? 1 : 0 << (numBits * 3 - 1);
            bits |= (MathX.Ftoi((MathX.Fabs(dir.x) + bias) * max)) << (numBits * 2);
            bits |= MathX.FLOATSIGNBITSET(dir.y) ? 1 : 0 << (numBits * 2 - 1);
            bits |= (MathX.Ftoi((MathX.Fabs(dir.y) + bias) * max)) << (numBits * 1);
            bits |= MathX.FLOATSIGNBITSET(dir.z) ? 1 : 0 << (numBits * 1 - 1);
            bits |= (MathX.Ftoi((MathX.Fabs(dir.z) + bias) * max)) << (numBits * 0);
            return bits;
        }

        static float[] sign = new[] { 1.0f, -1.0f };
        public static Vector3 BitsToDir(int bits, int numBits)
        {
            int max; float invMax; Vector3 dir;
            Debug.Assert(numBits >= 6 && numBits <= 32);

            numBits /= 3;
            max = (1 << (numBits - 1)) - 1;
            invMax = 1.0f / max;

            dir.x = sign[(bits >> (numBits * 3 - 1)) & 1] * ((bits >> (numBits * 2)) & max) * invMax;
            dir.y = sign[(bits >> (numBits * 2 - 1)) & 1] * ((bits >> (numBits * 1)) & max) * invMax;
            dir.z = sign[(bits >> (numBits * 1 - 1)) & 1] * ((bits >> (numBits * 0)) & max) * invMax;
            dir.NormalizeFast();
            return dir;
        }

        bool CheckOverflow(int numBits)
        {
            Debug.Assert(numBits >= 0);

            if (numBits > RemainingWriteBits)
            {
                if (!allowOverflow) FatalError("BitMsg: overflow without allowOverflow set");
                if (numBits > (maxSize << 3)) FatalError($"BitMsg: {numBits} bits is > full message size");
                Printf("BitMsg: overflow\n");
                BeginWriting();
                overflowed = true;
                return true;
            }
            return false;
        }

        Span<byte> GetByteSpace(int length)
        {
            if (writeData == null) FatalError("BitMsg::GetByteSpace: cannot write to message");

            // round up to the next byte
            WriteByteAlign();

            // check for overflow
            CheckOverflow(length << 3);

            var ptr = curSize;
            curSize += length;
            return writeData.AsSpan(ptr);
        }

        void WriteDelta(int oldValue, int newValue, int numBits)
        {
            if (oldValue == newValue) { WriteBits(0, 1); return; }
            WriteBits(1, 1);
            WriteBits(newValue, numBits);
        }

        int ReadDelta(int oldValue, int numBits)
            => ReadBits(1) != 0
            ? ReadBits(numBits)
            : oldValue;
    }

    public class BitMsgDelta
    {
        const int MAX_DATA_BUFFER = 1024;

        BitMsg base_;           // base
        BitMsg newBase;      // new base
        BitMsg writeDelta;       // delta from base to new base for writing
        BitMsg readDelta;      // delta from base to new base for reading
        bool changed;       // true if the new base is different from the base

        public BitMsgDelta()
        {
            base_ = null;
            newBase = null;
            writeDelta = null;
            readDelta = null;
            changed = false;
        }

        public void InitW(BitMsg base_, BitMsg newBase, BitMsg delta)
        {
            this.base_ = base_;
            this.newBase = newBase;
            this.writeDelta = delta;
            this.readDelta = delta;
            this.changed = false;
        }

        public void InitR(BitMsg base_, BitMsg newBase, BitMsg delta)
        {
            this.base_ = base_;
            this.newBase = newBase;
            this.writeDelta = null;
            this.readDelta = delta;
            this.changed = false;
        }

        public bool HasChanged => changed;

        public void WriteBits(int value, int numBits)
        {
            newBase?.WriteBits(value, numBits);

            if (base_ == null) { writeDelta.WriteBits(value, numBits); changed = true; }
            else
            {
                var baseValue = base_.ReadBits(numBits);
                if (baseValue == value) writeDelta.WriteBits(0, 1);
                else { writeDelta.WriteBits(1, 1); writeDelta.WriteBits(value, numBits); changed = true; }
            }
        }
        public void WriteChar(int c) => WriteBits(c, -8);
        public void WriteByte(int c) => WriteBits(c, 8);
        public void WriteShort(int c) => WriteBits(c, -16);
        public void WriteUShort(int c) => WriteBits(c, 16);
        public void WriteInt(int c) => WriteBits(c, 32);
        public void WriteFloat(float f) => WriteBits(reinterpret.cast_int(f), 32);
        public void WriteFloat(float f, int exponentBits, int mantissaBits)
        {
            var bits = MathX.FloatToBits(f, exponentBits, mantissaBits);
            WriteBits(bits, 1 + exponentBits + mantissaBits);
        }
        public void WriteAngle8(float f) => WriteBits(MathX.ANGLE2BYTE(f), 8);
        public void WriteAngle16(float f) => WriteBits(MathX.ANGLE2SHORT(f), 16);
        public void WriteDir(Vector3 dir, int numBits) => WriteBits(BitMsg.DirToBits(dir, numBits), numBits);
        public void WriteString(string s, int maxLength = -1)
        {
            newBase?.WriteString(s, maxLength);

            if (base_ == null) { writeDelta.WriteString(s, maxLength); changed = true; }
            else
            {
                var baseString = new byte[MAX_DATA_BUFFER];
                base_.ReadString(out var s2, baseString);
                if (s == s2) writeDelta.WriteBits(0, 1);
                else { writeDelta.WriteBits(1, 1); writeDelta.WriteString(s, maxLength); changed = true; }
            }
        }
        public unsafe void WriteData(byte[] data, int offset, int length)
        {
            newBase?.WriteData(data, offset, length);

            if (base_ == null) { writeDelta.WriteData(data, offset, length); changed = true; }
            else
            {
                var baseData = new byte[MAX_DATA_BUFFER];
                Debug.Assert(length < baseData.Length);
                base_.ReadData(baseData, length);
                fixed (void* data_ = data, baseData_ = baseData)
                    if (UnsafeX.CompareBlock(data_, baseData_, length) == 0) writeDelta.WriteBits(0, 1);
                    else { writeDelta.WriteBits(1, 1); writeDelta.WriteData(data, offset, length); changed = true; }
            }
        }
        public void WriteDict(Dictionary<string, string> dict)
        {
            newBase?.WriteDeltaDict(dict, null);

            if (base_ == null) { writeDelta.WriteDeltaDict(dict, null); changed = true; }
            else
            {
                var baseDict = new Dictionary<string, string>();
                base_.ReadDeltaDict(baseDict, null);
                changed = writeDelta.WriteDeltaDict(dict, baseDict);
            }
        }

        public void WriteDeltaChar(int oldValue, int newValue) => WriteDelta(oldValue, newValue, -8);
        public void WriteDeltaByte(int oldValue, int newValue) => WriteDelta(oldValue, newValue, 8);
        public void WriteDeltaShort(int oldValue, int newValue) => WriteDelta(oldValue, newValue, -16);
        public void WriteDeltaInt(int oldValue, int newValue) => WriteDelta(oldValue, newValue, 32);
        public void WriteDeltaFloat(float oldValue, float newValue) => WriteDelta(reinterpret.cast_int(oldValue), reinterpret.cast_int(newValue), 32);
        public void WriteDeltaFloat(float oldValue, float newValue, int exponentBits, int mantissaBits)
        {
            var oldBits = MathX.FloatToBits(oldValue, exponentBits, mantissaBits);
            var newBits = MathX.FloatToBits(newValue, exponentBits, mantissaBits);
            WriteDelta(oldBits, newBits, 1 + exponentBits + mantissaBits);
        }
        public void WriteDeltaByteCounter(int oldValue, int newValue)
        {
            newBase?.WriteBits(newValue, 8);

            if (base_ == null) { writeDelta.WriteDeltaByteCounter(oldValue, newValue); changed = true; }
            else
            {
                var baseValue = base_.ReadBits(8);
                if (baseValue == newValue) writeDelta.WriteBits(0, 1);
                else { writeDelta.WriteBits(1, 1); writeDelta.WriteDeltaByteCounter(oldValue, newValue); changed = true; }
            }
        }
        public void WriteDeltaShortCounter(int oldValue, int newValue)
        {
            newBase?.WriteBits(newValue, 16);

            if (base_ == null) { writeDelta.WriteDeltaShortCounter(oldValue, newValue); changed = true; }
            else
            {
                var baseValue = base_.ReadBits(16);
                if (baseValue == newValue) writeDelta.WriteBits(0, 1);
                else { writeDelta.WriteBits(1, 1); writeDelta.WriteDeltaShortCounter(oldValue, newValue); changed = true; }
            }
        }
        public void WriteDeltaIntCounter(int oldValue, int newValue)
        {
            newBase?.WriteBits(newValue, 32);

            if (base_ == null) { writeDelta.WriteDeltaIntCounter(oldValue, newValue); changed = true; }
            else
            {
                var baseValue = base_.ReadBits(32);
                if (baseValue == newValue) writeDelta.WriteBits(0, 1);
                else { writeDelta.WriteBits(1, 1); writeDelta.WriteDeltaIntCounter(oldValue, newValue); changed = true; }
            }
        }

        public int ReadBits(int numBits)
        {
            int value;

            if (base_ == null) { value = readDelta.ReadBits(numBits); changed = true; }
            else
            {
                var baseValue = base_.ReadBits(numBits);
                if (readDelta == null || readDelta.ReadBits(1) == 0) value = baseValue;
                else { value = readDelta.ReadBits(numBits); changed = true; }
            }

            newBase?.WriteBits(value, numBits);
            return value;
        }
        public int ReadChar() => (char)ReadBits(-8);
        public int ReadByte() => (byte)ReadBits(8);
        public int ReadShort() => (short)ReadBits(-16);
        public int ReadUShort() => (ushort)ReadBits(16);
        public int ReadInt() => ReadBits(32);
        public float ReadFloat() => reinterpret.cast_int(ReadBits(32));
        public float ReadFloat(int exponentBits, int mantissaBits)
        {
            var bits = ReadBits(1 + exponentBits + mantissaBits);
            return MathX.BitsToFloat(bits, exponentBits, mantissaBits);
        }
        public float ReadAngle8() => MathX.BYTE2ANGLE(ReadByte());
        public float ReadAngle16() => MathX.SHORT2ANGLE(ReadShort());
        public Vector3 ReadDir(int numBits) => BitMsg.BitsToDir(ReadBits(numBits), numBits);
        public void ReadString(out string s, int bufferSize = Platform.MAX_STRING_CHARS)
        {
            if (base_ == null) { readDelta.ReadString(out s, bufferSize); changed = true; }
            else
            {
                var baseString = new byte[MAX_DATA_BUFFER];
                base_.ReadString(out s, baseString);
                if (readDelta == null || readDelta.ReadBits(1) == 0) { }
                else { readDelta.ReadString(out s, bufferSize); changed = true; }
            }

            newBase?.WriteString(s);
        }
        public void ReadString(out string s, byte[] buffer)
        {
            if (base_ == null) { readDelta.ReadString(out s, buffer); changed = true; }
            else
            {
                var baseString = new byte[MAX_DATA_BUFFER];
                base_.ReadString(out s, baseString);
                if (readDelta == null || readDelta.ReadBits(1) == 0) { }
                else { readDelta.ReadString(out s, buffer); changed = true; }
            }

            newBase?.WriteString(s);
        }
        public void ReadData(byte[] data, int length)
        {
            if (base_ == null) { readDelta.ReadData(data, length); changed = true; }
            else
            {
                var baseData = new byte[MAX_DATA_BUFFER];
                Debug.Assert(length < baseData.Length);
                base_.ReadData(baseData, length);
                if (readDelta == null || readDelta.ReadBits(1) == 0) Unsafe.CopyBlock(ref data[0], ref baseData[0], (uint)length);
                else { readDelta.ReadData(data, length); changed = true; }
            }

            newBase?.WriteData(data, 0, length);
        }
        public void ReadDict(Dictionary<string, string> dict)
        {
            if (base_ == null) { readDelta.ReadDeltaDict(dict, null); changed = true; }
            else
            {
                var baseDict = new Dictionary<string, string>();
                base_.ReadDeltaDict(baseDict, null);
                if (readDelta == null) dict = baseDict;
                else changed = readDelta.ReadDeltaDict(dict, baseDict);
            }

            newBase?.WriteDeltaDict(dict, null);
        }

        public int ReadDeltaChar(int oldValue) => (char)ReadDelta(oldValue, -8);
        public int ReadDeltaByte(int oldValue) => (byte)ReadDelta(oldValue, 8);
        public int ReadDeltaShort(int oldValue) => (short)ReadDelta(oldValue, -16);
        public int ReadDeltaInt(int oldValue) => ReadDelta(oldValue, 32);
        public float ReadDeltaFloat(float oldValue) => reinterpret.cast_int(ReadDelta(reinterpret.cast_int(oldValue), 32));
        public float ReadDeltaFloat(float oldValue, int exponentBits, int mantissaBits)
        {
            var oldBits = MathX.FloatToBits(oldValue, exponentBits, mantissaBits);
            var newBits = ReadDelta(oldBits, 1 + exponentBits + mantissaBits);
            return MathX.BitsToFloat(newBits, exponentBits, mantissaBits);
        }
        public int ReadDeltaByteCounter(int oldValue)
        {
            int value;

            if (base_ == null) { value = readDelta.ReadDeltaByteCounter(oldValue); changed = true; }
            else
            {
                var baseValue = base_.ReadBits(8);
                if (readDelta == null || readDelta.ReadBits(1) == 0) value = baseValue;
                else { value = readDelta.ReadDeltaByteCounter(oldValue); changed = true; }
            }

            newBase?.WriteBits(value, 8);
            return value;
        }
        public int ReadDeltaShortCounter(int oldValue)
        {
            int value;

            if (base_ == null) { value = readDelta.ReadDeltaShortCounter(oldValue); changed = true; }
            else
            {
                var baseValue = base_.ReadBits(16);
                if (readDelta == null || readDelta.ReadBits(1) == 0) value = baseValue;
                else { value = readDelta.ReadDeltaShortCounter(oldValue); changed = true; }
            }

            newBase?.WriteBits(value, 16);
            return value;
        }
        public int ReadDeltaIntCounter(int oldValue)
        {
            int value;

            if (base_ == null) { value = readDelta.ReadDeltaIntCounter(oldValue); changed = true; }
            else
            {
                var baseValue = base_.ReadBits(32);
                if (readDelta == null || readDelta.ReadBits(1) == 0) value = baseValue;
                else { value = readDelta.ReadDeltaIntCounter(oldValue); changed = true; }
            }

            newBase?.WriteBits(value, 32);
            return value;
        }

        void WriteDelta(int oldValue, int newValue, int numBits)
        {
            newBase?.WriteBits(newValue, numBits);

            if (base_ == null)
            {
                if (oldValue == newValue) writeDelta.WriteBits(0, 1);
                else { writeDelta.WriteBits(1, 1); writeDelta.WriteBits(newValue, numBits); }
                changed = true;
            }
            else
            {
                var baseValue = base_.ReadBits(numBits);
                if (baseValue == newValue) writeDelta.WriteBits(0, 1);
                else
                {
                    writeDelta.WriteBits(1, 1);
                    if (oldValue == newValue) { writeDelta.WriteBits(0, 1); changed = true; }
                    else { writeDelta.WriteBits(1, 1); writeDelta.WriteBits(newValue, numBits); changed = true; }
                }
            }
        }
        int ReadDelta(int oldValue, int numBits)
        {
            int value;

            if (base_ == null)
            {
                if (readDelta.ReadBits(1) == 0) value = oldValue;
                else value = readDelta.ReadBits(numBits);
                changed = true;
            }
            else
            {
                var baseValue = base_.ReadBits(numBits);
                if (readDelta == null || readDelta.ReadBits(1) == 0) value = baseValue;
                else if (readDelta.ReadBits(1) == 0) { value = oldValue; changed = true; }
                else { value = readDelta.ReadBits(numBits); changed = true; }
            }

            newBase?.WriteBits(value, numBits);
            return value;
        }
    }
}
