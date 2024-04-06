using System.Collections.Generic;
using System.Linq;
using System.Text;
using static System.NumericsX.OpenStack.OpenStack;
using static System.NumericsX.Platform;

namespace System.NumericsX.OpenStack
{
    public class VFileDemo : VFile
    {
        public enum DS : int
        {
            FINISHED,
            RENDER,
            SOUND,
            VERSION
        }

        bool writing;
        byte[] fileImage;
        VFile f;
        VCompressor compressor;

        List<string> demoStrings = new();
        VFile fLog;
        bool log;
        string logStr;

        static CVar com_logDemos = new("com_logDemos", "0", CVAR.SYSTEM | CVAR.BOOL, "Write demo.log with debug information in it");
        static CVar com_compressDemos = new("com_compressDemos", "1", CVAR.SYSTEM | CVAR.INTEGER | CVAR.ARCHIVE, "Compression scheme for demo files\n0: None    (Fast, large files)\n1: LZW     (Fast to compress, Fast to decompress, medium/small files)\n2: LZSS    (Slow to compress, Fast to decompress, small files)\n3: Huffman (Fast to compress, Slow to decompress, medium files)\nSee also: The 'CompressDemo' command");
        static CVar com_preloadDemos = new("com_preloadDemos", "0", CVAR.SYSTEM | CVAR.BOOL | CVAR.ARCHIVE, "Load the whole demo in to RAM before running it");
        static readonly byte[] DEMO_MAGIC = Encoding.ASCII.GetBytes(GAME_NAME + " RDEMO");

        public VFileDemo()
        {
            f = null;
            fLog = null;
            log = false;
            fileImage = null;
            compressor = null;
            writing = false;
        }
        public override void Dispose()
            => Close();

        public override string Name => f != null ? f.Name : string.Empty;
        public override string FullPath => f != null ? f.FullPath : string.Empty;

        public void SetLog(bool b, string p)
        {
            log = b;
            if (p != null) logStr = p;
        }

        public void Log(string p)
        {
            if (fLog != null && !string.IsNullOrEmpty(p))
            {
                var text = Encoding.ASCII.GetBytes(p);
                fLog.Write(text, text.Length);
            }
        }

        public bool OpenForReading(string fileName)
        {
            int compression, fileLength;
            var magicBuffer = new byte[DEMO_MAGIC.Length];

            Close();

            f = fileSystem.OpenFileRead(fileName);
            if (f == null) return false;

            fileLength = f.Length;

            if (com_preloadDemos.Bool)
            {
                fileImage = new byte[fileLength];
                f.Read(fileImage, fileLength);
                fileSystem.CloseFile(f);
                f = new VFile_Memory($"preloaded({fileName})", fileImage, fileLength);
            }

            if (com_logDemos.Bool) fLog = fileSystem.OpenFileWrite("demoread.log");

            writing = false;

            f.Read(magicBuffer, DEMO_MAGIC.Length);
            if (Enumerable.SequenceEqual(magicBuffer, DEMO_MAGIC)) f.ReadInt(out compression);
            // Ideally we would error out if the magic string isn't there, but for backwards compatibility we are going to assume it's just an uncompressed demo file
            else { compression = 0; f.Rewind(); }

            compressor = AllocCompressor(compression);
            compressor.Init(f, false, 8);

            return true;
        }

        public bool OpenForWriting(string fileName)
        {
            Close();

            f = fileSystem.OpenFileWrite(fileName);
            if (f == null) return false;

            if (com_logDemos.Bool) fLog = fileSystem.OpenFileWrite("demowrite.log");

            writing = true;

            f.Write(DEMO_MAGIC, DEMO_MAGIC.Length);
            f.WriteInt(com_compressDemos.Integer);
            f.Flush();

            compressor = AllocCompressor(com_compressDemos.Integer);
            compressor.Init(f, true, 8);

            return true;
        }

        public void Close()
        {
            if (writing && compressor != null) compressor.FinishCompress();

            if (f != null) { fileSystem.CloseFile(f); f = null; }
            if (fLog != null) { fileSystem.CloseFile(fLog); fLog = null; }
            if (fileImage != null) fileImage = null;
            if (compressor != null) compressor = null;

            demoStrings.Clear();
        }

        public string ReadHashString()
        {
            if (log && fLog != null)
            {
                var text = Encoding.ASCII.GetBytes($"{logStr} > Reading hash string\n");
                fLog.Write(text, text.Length);
            }

            ReadInt(out var index);

            if (index == -1)
            {
                // read a new string for the table
                ReadString(out var str);
                demoStrings.Add(str);
                return str;
            }

            if (index < -1 || index >= demoStrings.Count) { Close(); Error("demo hash index out of range"); }

            return demoStrings[index];
        }

        public void WriteHashString(string str)
        {
            if (log && fLog != null)
            {
                var text = Encoding.ASCII.GetBytes($"{logStr} > Writing hash string\n");
                fLog.Write(text, text.Length);
            }
            // see if it is already in the has table
            for (var i = 0; i < demoStrings.Count; i++)
                if (demoStrings[i] == str)
                {
                    WriteInt(i);
                    return;
                }

            // add it to our table and the demo table
            demoStrings.Add(str);
            WriteInt(-1);
            WriteString(str);
        }

        public void ReadDict(Dictionary<string, string> dict)
        {
            dict.Clear();
            ReadInt(out var c);
            for (var i = 0; i < c; i++) dict[ReadHashString()] = ReadHashString();
        }

        public void WriteDict(Dictionary<string, string> dict)
        {
            var c = dict.ToArray();
            WriteInt(c.Length);
            for (var i = 0; i < c.Length; i++)
            {
                WriteHashString(c[i].Key);
                WriteHashString(c[i].Value);
            }
        }

        public override int Read(byte[] buffer, int len)
        {
            var read = compressor.Read(buffer, len);
            if (read == 0 && len >= 4) throw new NotImplementedException(); //*(DemoSystem)buffer = DS_FINISHED;
            return read;
        }

        public override int Write(byte[] buffer, int len)
            => compressor.Write(buffer, len);

        static VCompressor AllocCompressor(int type)
            => type switch
            {
                0 => VCompressor.AllocNoCompression(),
                1 => VCompressor.AllocLZW(),
                2 => VCompressor.AllocLZSS(),
                3 => VCompressor.AllocHuffman(),
                _ => VCompressor.AllocLZW(),
            };
    }
}
