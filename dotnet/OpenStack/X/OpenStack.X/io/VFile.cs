using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using static System.NumericsX.OpenStack.OpenStack;
using static System.NumericsX.Platform;

namespace System.NumericsX.OpenStack
{
    // mode parm for Seek
    public enum FS_SEEK
    {
        CUR,
        END,
        SET
    }

    public static unsafe class VFileExtensions
    {
        public unsafe static string ReadZASCII(this VFile source)
        {
            int len; char* str = stackalloc char[MAX_STRING_CHARS];

            for (len = 0; len < MAX_STRING_CHARS; len++)
            {
                source.Read((byte*)&str[len], 1);
                if (str[len] == 0) break;
            }
            if (len == MAX_STRING_CHARS) Error("ReadZASCII: bad string");
            return new string(str, 0, len);
        }
        public static int ReadASCII(this VFile source, out string value, int length) => throw new NotImplementedException();
        public static int Read(this VFile source, out long value) { long val; var r = source.Read((byte*)&val, sizeof(long)); value = val; return r; }
        public static int Read(this VFile source, out ulong value) { ulong val; var r = source.Read((byte*)&val, sizeof(ulong)); value = val; return r; }
        public static int Read(this VFile source, out float value) { float val; var r = source.Read((byte*)&val, sizeof(float)); value = val; return r; }
        public static int Read(this VFile source, out int value) { int val; var r = source.Read((byte*)&val, sizeof(int)); value = val; return r; }
        public static int Read(this VFile source, out uint value) { uint val; var r = source.Read((byte*)&val, sizeof(uint)); value = val; return r; }
        public static int Read(this VFile source, out short value) { short val; var r = source.Read((byte*)&val, sizeof(short)); value = val; return r; }
        public static int Read(this VFile source, out ushort value) { ushort val; var r = source.Read((byte*)&val, sizeof(ushort)); value = val; return r; }
        public static int Read(this VFile source, out byte value) { byte val; var r = source.Read((byte*)&val, sizeof(byte)); value = val; return r; }
        public static int Read(this VFile source, out bool value) { bool val; var r = source.Read((byte*)&val, sizeof(bool)); value = val; return r; }
        public static int Read<E>(this VFile source, out E value) where E : Enum => throw new NotImplementedException();
        public static int ReadT<T>(this VFile source, out T value) where T : struct => throw new NotImplementedException();
        public static int ReadTMany<T>(this VFile source, out T[] value, int count) where T : struct => throw new NotImplementedException();
        public static void ReadDictionary(this VFile source, Dictionary<string, string> value)
        {
            value.Clear();
            source.Read(out int count);
            count = LittleInt(count);
            for (var i = 0; i < count; i++)
            {
                var key = source.ReadZASCII();
                var val = source.ReadZASCII();
                value[key] = val;
            }
        }

        public static void WriteZASCII(this VFile source, string value)
        {
            var len = value.Length;
            if (len >= MAX_STRING_CHARS - 1) Error("WriteZASCII: bad string");
            source.Write(Encoding.ASCII.GetBytes(value), len);
            source.Write((byte)0);
        }
        public static int WriteASCII(this VFile source, string value, int length) => source.Write(Encoding.ASCII.GetBytes(value), value.Length);
        public static int Write(this VFile source, long value) => source.Write((byte*)&value, sizeof(long));
        public static int Write(this VFile source, ulong value) => source.Write((byte*)&value, sizeof(ulong));
        public static int Write(this VFile source, float value) => source.Write((byte*)&value, sizeof(float));
        public static int Write(this VFile source, int value) => source.Write((byte*)&value, sizeof(int));
        public static int Write(this VFile source, uint value) => source.Write((byte*)&value, sizeof(uint));
        public static int Write(this VFile source, short value) => source.Write((byte*)&value, sizeof(short));
        public static int Write(this VFile source, ushort value) => source.Write((byte*)&value, sizeof(ushort));
        public static int Write(this VFile source, byte value) => source.Write((byte*)&value, sizeof(byte));
        public static int Write(this VFile source, bool value) => source.Write((byte*)&value, sizeof(bool));
        public static int Write<E>(this VFile source, E value) where E : Enum => throw new NotImplementedException();
        public static int WriteT<T>(this VFile source, T value) where T : struct => throw new NotImplementedException();
        public static int WriteTMany<T>(this VFile source, T[] value) where T : struct => throw new NotImplementedException();
        public static void WriteDictionary(this VFile source, Dictionary<string, string> value)
        {
            var count = LittleInt(value.Count);
            source.Write(count);
            foreach (var kv in value)
            {
                source.WriteZASCII(kv.Key);
                source.WriteZASCII(kv.Value);
            }
        }
    }

    public unsafe class VFile : IDisposable
    {
        public virtual void Dispose() { }
        // Get the name of the file.
        public virtual string Name => string.Empty;
        // Get the full file path.
        public virtual string FullPath => string.Empty;
        // Read data from the file to the buffer.
        public virtual int Read(byte* buffer, int len)
        {
            FatalError("File::Read: cannot read from File");
            return 0;
        }
        public virtual int Read(byte[] buffer, int len)
        {
            FatalError("File::Read: cannot read from File");
            return 0;
        }
        // Write data from the buffer to the file.
        public virtual int Write(byte* buffer, int len)
        {
            FatalError("File::Write: cannot write to File");
            return 0;
        }
        public virtual int Write(byte[] buffer, int len)
        {
            FatalError("File::Write: cannot write to File");
            return 0;
        }
        // Returns the length of the file.
        public virtual int Length => 0;
        // Return a time value for reload operations.
        public virtual DateTime Timestamp => DateTime.MinValue;
        // Returns offset in file.
        public virtual int Tell => 0;
        // Forces flush on files being writting to.
        public virtual void ForceFlush() { }
        // Causes any buffered data to be written to the file.
        public virtual void Flush() { }
        // Seek on a file.
        public virtual int Seek(long offset, FS_SEEK origin) => -1;
        // Go back to the beginning of the file.
        public virtual void Rewind() => Seek(0, FS_SEEK.SET);
        // Like fprintf.
        public virtual int Printf(string fmt, params object[] args)
        {
            var text = args.Length == 0 ? fmt : string.Format(fmt, args);
            text = text.Replace("\n", "\r\n");
            var buf = Encoding.ASCII.GetBytes(text);
            return Write(buf, buf.Length);
        }
        // Write a string with high precision floating point numbers to the file.
        public virtual int WriteFloatString(string fmt, params object[] args)
        {
            var text = args.Length == 0 ? fmt : string.Format(fmt, args);
            var buf = Encoding.ASCII.GetBytes(text);
            return Write(buf, buf.Length);
        }
        // Endian portable alternatives to Read(...)
        public int ReadInt(out int value)
        {
            var buf = stackalloc byte[sizeof(int)];
            var result = Read(buf, sizeof(int));
            value = LittleInt(*(int*)buf);
            return result;
        }
        public int ReadUnsignedInt(out uint value)
        {
            var buf = stackalloc byte[sizeof(uint)];
            var result = Read(buf, sizeof(uint));
            value = unchecked((uint)LittleInt(*(int*)buf));
            return result;
        }
        public int ReadShort(out short value)
        {
            var buf = stackalloc byte[sizeof(short)];
            var result = Read(buf, sizeof(short));
            value = LittleShort(*(short*)buf);
            return result;
        }
        public int ReadUnsignedShort(out ushort value)
        {
            var buf = stackalloc byte[sizeof(ushort)];
            var result = Read(buf, sizeof(ushort));
            value = unchecked((ushort)LittleShort(*(short*)buf));
            return result;
        }
        public int ReadChar(out char value)
        {
            var buf = stackalloc byte[1];
            var result = Read(buf, 1);
            value = *(char*)buf;
            return result;
        }
        public int ReadUnsignedChar(out byte value)
        {
            var buf = stackalloc byte[1];
            var result = Read(buf, 1);
            value = *buf;
            return result;
        }
        public int ReadFloat(out float value)
        {
            var buf = stackalloc byte[sizeof(float)];
            var result = Read(buf, sizeof(float));
            value = LittleFloat(*(float*)buf);
            return result;
        }
        public int ReadBool(out bool value)
        {
            var result = ReadUnsignedChar(out var c);
            value = c != 0;
            return result;
        }
        const int MaxStackLimit = 1024;
        public int ReadString(out string s)
        {
            ReadInt(out var len);
            var buf = len <= MaxStackLimit ? stackalloc byte[len] : new byte[len];
            if (len >= 0)
                fixed (byte* buf_ = buf)
                {
                    var result = Read(buf_, len);
                    if (len < result) Unsafe.InitBlock((void*)(buf_ + result), (byte)' ', (uint)(result - len));
                    s = new string((char*)buf_, 0, len);
                    return result;
                }
            s = default;
            return 0;
        }

        public int ReadVec2(out Vector2 vec)
        {
            Vector2 r; var buf = (byte*)&r;
            var result = Read(buf, sizeof(Vector2));
            LittleRevBytes(buf, sizeof(float), sizeof(Vector2) / sizeof(float));
            vec = r;
            return result;
        }
        public int ReadVec3(out Vector3 vec)
        {
            Vector3 r; var buf = (byte*)&r;
            var result = Read(buf, sizeof(Vector3));
            LittleRevBytes(buf, sizeof(float), sizeof(Vector3) / sizeof(float));
            vec = r;
            return result;
        }
        public int ReadVec4(out Vector4 vec)
        {
            Vector4 r; var buf = (byte*)&r;
            var result = Read(buf, sizeof(Vector4));
            LittleRevBytes(buf, sizeof(float), sizeof(Vector4) / sizeof(float));
            vec = r;
            return result;
        }
        public int ReadVec6(out Vector6 vec)
        {
            Vector6 r; var buf = (byte*)&r;
            var result = Read(buf, sizeof(Vector6));
            LittleRevBytes(buf, sizeof(float), sizeof(Vector6) / sizeof(float));
            vec = r;
            return result;
        }
        public int ReadMat3(out Matrix3x3 mat)
        {
            Matrix3x3 r = default;
            void* buf = &r.mat0;
            {
                var result = Read((byte*)buf, Matrix3x3.SizeOf);
                LittleRevBytes(buf, sizeof(float), Matrix3x3.SizeOf / sizeof(float));
                mat = r;
                return result;
            }
        }

        // Endian portable alternatives to Write(...)
        public int WriteInt(int value)
        {
            var v = LittleInt(value);
            return Write((byte*)&v, sizeof(int));
        }
        public int WriteUnsignedInt(uint value)
        {
            var v = unchecked(LittleInt((int)value));
            return Write((byte*)&v, sizeof(uint));
        }
        public int WriteShort(short value)
        {
            var v = LittleShort(value);
            return Write((byte*)&v, sizeof(short));
        }
        public int WriteUnsignedShort(ushort value)
        {
            var v = unchecked(LittleShort((short)value));
            return Write((byte*)&v, sizeof(ushort));
        }
        public int WriteChar(char value)
            => Write((byte*)&value, 1);
        public int WriteUnsignedChar(byte value)
            => Write(&value, 1);

        public int WriteFloat(float value)
        {
            var v = LittleFloat(value);
            return Write((byte*)&v, sizeof(float));
        }
        public int WriteBool(bool value)
            => WriteUnsignedChar(value ? (byte)1 : (byte)0);
        public int WriteString(string s)
        {
            var len = s != null ? s.Length : 0;
            WriteInt(len);
            var buf = Encoding.ASCII.GetBytes(s);
            return Write((byte*)buf[0], len);
        }

        public int WriteVec2(Vector2 vec)
        {
            Vector2 v = vec;
            LittleRevBytes(&v, sizeof(float), sizeof(Vector2) / sizeof(float));
            return Write((byte*)&v, sizeof(Vector2));
        }
        public int WriteVec3(Vector3 vec)
        {
            Vector3 v = vec;
            LittleRevBytes(&v, sizeof(float), sizeof(Vector3) / sizeof(float));
            return Write((byte*)&v, sizeof(Vector3));
        }
        public int WriteVec4(Vector4 vec)
        {
            Vector4 v = vec;
            LittleRevBytes(&v, sizeof(float), sizeof(Vector4) / sizeof(float));
            return Write((byte*)&v, sizeof(Vector4));
        }
        public int WriteVec6(Vector6 vec)
        {
            Vector6 v = vec;
            LittleRevBytes(&v, sizeof(float), sizeof(Vector6) / sizeof(float));
            return Write((byte*)&v, sizeof(Vector6));
        }
        public int WriteMat3(Matrix3x3 mat)
        {
            Matrix3x3 v = mat;
            void* mat_ = &mat.mat0;
            {
                LittleRevBytes(&mat_, sizeof(float), Matrix3x3.SizeOf / sizeof(float));
                return Write((byte*)&mat_, Matrix3x3.SizeOf);
            }
        }
    }

    public class VFile_Memory : VFile
    {
        string name;         // name of the file
        int mode;           // open mode
        int maxSize;        // maximum size of file
        int fileSize;       // size of the file
        int allocated;      // allocated size
        int granularity;    // file granularity
        byte[] filePtr;      // buffer holding the file data
        int curPtr;        // current read/write pointer

        public VFile_Memory()   // file for writing without name
        {
            name = "*unknown*";
            maxSize = 0;
            fileSize = 0;
            allocated = 0;
            granularity = 16384;

            mode = 1 << (int)FS.WRITE;
            filePtr = null;
            curPtr = 0;
        }
        public VFile_Memory(string name) // file for writing
        {
            this.name = name;
            maxSize = 0;
            fileSize = 0;
            allocated = 0;
            granularity = 16384;

            mode = 1 << (int)FS.WRITE;
            filePtr = null;
            curPtr = 0;
        }
        public VFile_Memory(string name, byte[] data, int length) // file for writing
        {
            this.name = name;
            maxSize = length;
            fileSize = 0;
            allocated = length;
            granularity = 16384;

            mode = 1 << (int)FS.WRITE;
            filePtr = data;
            curPtr = 0;
        }
        public VFile_Memory(string name, int length, byte[] data) // file for reading
        {
            this.name = name;
            maxSize = 0;
            fileSize = length;
            allocated = 0;
            granularity = 16384;

            mode = 1 << (int)FS.READ;
            filePtr = data;
            curPtr = 0;
        }

        public override string Name => name;
        public override string FullPath => name;
        public override int Read(byte[] buffer, int len)
        {
            if ((mode & (1 << (int)FS.READ)) == 0) { FatalError($"File_Memory::Read: {name} not opened in read mode"); return 0; }

            if (curPtr + len > fileSize) len = fileSize - curPtr;
            Unsafe.CopyBlock(ref buffer[0], ref filePtr[curPtr], (uint)len);
            curPtr += len;
            return len;
        }

        public override int Write(byte[] buffer, int len)
        {
            if ((mode & (1 << (int)FS.WRITE)) == 0) { FatalError($"File_Memory::Write: {name} not opened in write mode"); return 0; }

            var alloc = curPtr + len + 1 - allocated; // need room for len+1
            if (alloc > 0)
            {
                if (maxSize != 0) { Error($"File_Memory::Write: exceeded maximum size {maxSize}"); return 0; }
                var extra = granularity * (1 + alloc / granularity);
                var newPtr = new byte[allocated + extra];
                if (allocated != 0) Unsafe.CopyBlock(ref newPtr[0], ref filePtr[0], (uint)allocated);
                allocated += extra;
                //curPtr = curPtr;
                filePtr = newPtr;
            }
            Unsafe.CopyBlock(ref filePtr[curPtr], ref buffer[0], (uint)len);
            curPtr += len;
            fileSize += len;
            filePtr[fileSize] = 0; // len + 1
            return len;
        }

        public override int Length => fileSize;
        public override DateTime Timestamp => DateTime.MinValue;
        public override int Tell => curPtr;
        public override void ForceFlush() { }
        public override void Flush() { }
        public override int Seek(long offset, FS_SEEK origin)
        {
            switch (origin)
            {
                case FS_SEEK.CUR: { curPtr += (int)offset; break; }
                case FS_SEEK.END: { curPtr = fileSize - (int)offset; break; }
                case FS_SEEK.SET: { curPtr = (int)offset; break; }
                default: { FatalError($"File_Memory::Seek: bad origin for {name}\n"); return -1; }
            }
            if (curPtr < 0) { curPtr = 0; return -1; }
            if (curPtr > fileSize) { curPtr = fileSize; return -1; }
            return 0;
        }

        // changes memory file to read only
        public void MakeReadOnly()
        {
            mode = 1 << (int)FS.READ;
            Rewind();
        }
        // clear the file
        public void Clear(bool freeMemory = true)
        {
            fileSize = 0;
            granularity = 16384;
            if (freeMemory) { allocated = 0; filePtr = null; curPtr = 0; }
            else curPtr = 0;
        }
        // set data for reading
        public void SetData(byte[] data, int length)
        {
            maxSize = 0;
            fileSize = length;
            allocated = 0;
            granularity = 16384;

            mode = 1 << (int)FS.READ;
            filePtr = data;
            curPtr = 0;
        }
        // returns const pointer to the memory buffer
        public byte[] DataPtr => filePtr;
        // set the file granularity
        public void SetGranularity(int g) { Debug.Assert(g > 0); granularity = g; }
    }

    public class VFile_BitMsg : VFile
    {
        string name; // name of the file
        int mode; // open mode
        BitMsg msg;

        public VFile_BitMsg(BitMsg msg)
        {
            name = "*unknown*";
            mode = 1 << (int)FS.WRITE;
            this.msg = msg;
        }
        public VFile_BitMsg(int _, BitMsg msg)
        {
            name = "*unknown*";
            mode = 1 << (int)FS.READ;
            this.msg = msg;
        }

        public override string Name => name;
        public override string FullPath => name;
        public override int Read(byte[] buffer, int len)
        {
            if ((mode & (1 << (int)FS.READ)) == 0) { FatalError($"File_BitMsg::Read: {name} not opened in read mode"); return 0; }

            return msg.ReadData(buffer, len);
        }
        public override int Write(byte[] buffer, int len)
        {
            if ((mode & (1 << (int)FS.WRITE)) == 0) { FatalError($"File_Memory::Write: {name} not opened in write mode"); return 0; }

            msg.WriteData(buffer, 0, len);
            return len;
        }
        public override int Length => msg.Size;
        public override DateTime Timestamp => DateTime.MinValue;
        public override int Tell => (mode & (int)FS.READ) != 0 ? msg.ReadCount : msg.Size;
        public override void ForceFlush() { }
        public override void Flush() { }
        public override int Seek(long offset, FS_SEEK origin) => -1;
    }

    public class VFile_Permanent : VFile
    {
        string name;            // relative path of the file - relative path
        string fullPath;        // full file path - OS path
        int mode;               // open mode
        int fileSize;           // size of the file
        FileStream o;           // file handle
        bool handleSync;	    // true if written data is immediately flushed

        public VFile_Permanent()
        {
            name = "invalid";
            o = null;
            mode = 0;
            fileSize = 0;
            handleSync = false;
        }
        public override void Dispose()
            => o?.Dispose();

        public override string Name => name;
        public override string FullPath => fullPath;
        public override int Read(byte[] buffer, int len)
        {
            int buf, block, remaining, read, tries;

            if ((mode & (1 << (int)FS.READ)) == 0) { FatalError($"File_Permanent::Read: {name} not opened in read mode"); return 0; }

            if (o == null) return 0;

            buf = 0; remaining = len; tries = 0;
            while (remaining != 0)
            {
                block = remaining;
                read = o.Read(buffer, buf, block);
                if (read == 0)
                    // we might have been trying to read from a CD, which sometimes returns a 0 read on windows
                    if (tries == 0) tries = 1;
                    else { fileSystem.AddToReadCount(len - remaining); return len - remaining; }
                if (read == -1) FatalError($"File_Permanent::Read: -1 bytes read from {name}");

                remaining -= read;
                buf += read;
            }
            fileSystem.AddToReadCount(len);
            return len;
        }

        // Properly handles partial writes
        public override int Write(byte[] buffer, int len)
        {
            int buf, block, remaining, written, tries;

            if ((mode & (1 << (int)FS.WRITE)) == 0) { FatalError("File_Permanent::Write: {name} not opened in write mode"); return 0; }

            if (o == null) return 0;

            buf = 0; remaining = len; tries = 0;
            while (remaining != 0)
            {
                block = remaining;
                o.Write(buffer, buf, block); written = block;
                if (written == 0)
                    if (tries == 0) tries = 1;
                    else { Printf($"File_Permanent::Write: 0 bytes written to {name}\n"); return 0; }
                if (written == -1) { Printf($"File_Permanent::Write: -1 bytes written to {name}\n"); return 0; }

                remaining -= written;
                buf += written;
                fileSize += written;
            }
            if (handleSync) o.Flush();
            return len;
        }
        public override int Length => fileSize;
        public override DateTime Timestamp => Sys_FileTimeStamp(null);
        public override int Tell => (int)o.Position;
        public override void ForceFlush()
            => o.Flush();
        public override void Flush()
            => o.Flush();
        // returns zero on success and -1 on failure
        public override int Seek(long offset, FS_SEEK origin)
        {
            SeekOrigin _origin;
            switch (origin)
            {
                case FS_SEEK.CUR: { _origin = SeekOrigin.Current; break; }
                case FS_SEEK.END: { _origin = SeekOrigin.End; break; }
                case FS_SEEK.SET: { _origin = SeekOrigin.Begin; break; }
                default: { _origin = SeekOrigin.Current; FatalError($"File_Permanent::Seek: bad origin for {name}\n"); break; }
            }

            return (int)o.Seek(offset, _origin);
        }

        // returns file pointer
        public FileStream FilePtr => o;
    }

    /*
    public class VFile_InZip : VFile
    {
        string name;            // name of the file in the pak
        string fullPath;        // full file path including pak file name
        object zipFilePos;      // zip file info position in pak
        int fileSize;           // size of the file
        object z;				// unzip info

        public VFile_InZip()
        {
            name = "invalid";
            zipFilePos = 0;
            fileSize = 0;
            memset(&z, 0, sizeof(z));
        }
        public override void Dispose()
        {
            unzCloseCurrentFile(z);
            unzClose(z);
        }

        public override string Name => name;
        public override string FullPath => fullPath;
        public override int Read(byte[] buffer, int len)
        {
            var l = unzReadCurrentFile(z, buffer, len);
            fileSystem.AddToReadCount(l);
            return l;
        }
        public override int Write(byte[] buffer, int len)
        {
            FatalError($"File_InZip::Write: cannot write to the zipped file {name}");
            return 0;
        }
        public override int Length => fileSize;
        public override DateTime Timestamp => DateTime.MinValue;
        public override int Tell => unztell(z);
        public override void ForceFlush() => FatalError($"File_InZip::ForceFlush: cannot flush the zipped file {name}");
        public override void Flush() => FatalError($"File_InZip::Flush: cannot flush the zipped file {name}");
        const int ZIP_SEEK_BUF_SIZE = 1 << 15;
        // returns zero on success and -1 on failure
        public override int Seek(long offset, FS_SEEK origin)
        {
            int res, i;

            switch (origin)
            {
                case FS_SEEK.END:
                    {
                        offset = fileSize - offset;
                        goto case FS_SEEK.SET;
                    }
                case FS_SEEK.SET:
                    {
                        // set the file position in the zip file (also sets the current file info)
                        unzSetOffset64(z, zipFilePos);
                        unzOpenCurrentFile(z);
                        if (offset <= 0) return 0;
                        goto case FS_SEEK.CUR;
                    }
                case FS_SEEK.CUR:
                    {
                        var buf = stackalloc byte[ZIP_SEEK_BUF_SIZE];
                        for (i = 0; i < (offset - ZIP_SEEK_BUF_SIZE); i += ZIP_SEEK_BUF_SIZE)
                        {
                            res = unzReadCurrentFile(z, buf, ZIP_SEEK_BUF_SIZE);
                            if (res < ZIP_SEEK_BUF_SIZE) return -1;
                        }
                        res = i + unzReadCurrentFile(z, buf, offset - i);
                        return res == offset ? 0 : -1;
                    }
                default:
                    {
                        FatalError($"File_InZip::Seek: bad origin for {name}\n");
                        break;
                    }
            }
            return -1;
        }
    }
    */
}