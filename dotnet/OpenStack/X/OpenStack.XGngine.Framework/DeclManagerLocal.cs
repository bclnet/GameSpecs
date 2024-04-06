//#define USE_COMPRESSED_DECLS
#define GET_HUFFMAN_FREQUENCIES
using System.Collections.Generic;
using System.Diagnostics;
using System.NumericsX.OpenStack.Gngine.Render;
using System.NumericsX.OpenStack.Gngine.Sound;
using System.Runtime.CompilerServices;
using System.Text;
using static System.NumericsX.OpenStack.Gngine.Framework.Framework;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.Framework
{
    class DeclType_
    {
        public string typeName;
        public DECL type;
        public Func<Decl> allocator;
    }

    class DeclFolder
    {
        public string folder;
        public string extension;
        public DECL defaultType;
    }

    class DeclLocal : DeclBase
    {
        internal Decl self;

        internal string name;                    // name of the decl
        internal string text;              // decl text definition
        internal int textLength;             // length of textSource
        int compressedLength;       // compressed length
        internal DeclFile sourceFile;                // source file in which the decl was defined
        internal int sourceTextOffset;       // offset in source file to decl text
        internal int sourceTextLength;       // length of decl text in source file
        internal int sourceLine;             // this is where the actual declaration token starts
        internal int checksum;               // checksum of the decl text
        internal DECL type;                  // decl type
        internal DeclState declState;                // decl state
        internal int index;                  // index in the per-type list

        internal bool parsedOutsideLevelLoad;    // these decls will never be purged
        internal bool everReferenced;            // set to true if the decl was ever used
        internal bool referencedThisLevel;   // set to true when the decl is used for the current level
        internal bool redefinedInReload;     // used during file reloading to make sure a decl that has its source removed will be defaulted
        internal DeclLocal nextInFile;               // next decl in the decl file

        public DeclLocal()
        {
            name = "unnamed";
            text = null;
            textLength = 0;
            compressedLength = 0;
            sourceFile = null;
            sourceTextOffset = 0;
            sourceTextLength = 0;
            sourceLine = 0;
            checksum = 0;
            type = DECL.ENTITYDEF;
            index = 0;
            declState = DeclState.DS_UNPARSED;
            parsedOutsideLevelLoad = false;
            referencedThisLevel = false;
            everReferenced = false;
            redefinedInReload = false;
            nextInFile = null;
        }

        public override string Name => name;
        public override DECL Type => type;
        public override DeclState State => declState;
        public override bool IsImplicit => sourceFile == declManagerLocal.ImplicitDeclFile;
        public override bool IsValid => declState != DeclState.DS_UNPARSED;
        public override void Invalidate() => declState = DeclState.DS_UNPARSED;
        public override void Reload() => this.sourceFile.Reload(false);
        public override void EnsureNotPurged()
        {
            if (declState == DeclState.DS_UNPARSED)
                ParseLocal();
        }
        public override int Index => index;
        public override int LineNum => sourceLine;
        public override string FileName => sourceFile != null ? sourceFile.fileName : "*invalid*";
        public override int Size => 0;
        public override string Text
        {
            get
            {
#if USE_COMPRESSED_DECLS
                Huffman.HuffmanDecompressText(out text, text, compressedLength);
#else
                return text;
#endif
            }
            // Set text possibly with compression.
            set
            {
                checksum = byteX.MD5Checksum(Encoding.ASCII.GetBytes(value));
#if GET_HUFFMAN_FREQUENCIES
                unchecked
                {
                    for (var i = 0; i < value.Length; i++)
                        Huffman.HuffmanFrequencies[value[i]]++;
                }
#endif
#if USE_COMPRESSED_DECLS
                var maxBytesPerCode = (Huffman.maxHuffmanBits + 7) >> 3;
                byte* compressed = (byte*)_alloca(length * maxBytesPerCode);
                compressedLength = Huffman.HuffmanCompressText(value, length, compressed);
                textSource = (char*)Mem_Alloc(compressedLength);
                memcpy(textSource, compressed, compressedLength);
#else
                compressedLength = value.Length;
                text = value;
#endif
                textLength = value.Length;
            }
        }
        public override int TextLength => textLength;

        public override bool ReplaceSourceFileText()
        {
            common.Printf($"Writing \'{Name}\' to \'{FileName}\'...\n");

            if (sourceFile == declManagerLocal.implicitDecls) { common.Warning($"Can't save implicit declaration {Name}."); return false; }

            // get length and allocate buffer to hold the file
            var oldFileLength = sourceFile.fileSize;
            var newFileLength = oldFileLength - sourceTextLength + textLength;
            var buffer = new byte[Math.Max(newFileLength, oldFileLength)];
            VFile file;

            // read original file
            if (sourceFile.fileSize != 0)
            {
                file = fileSystem.OpenFileRead(FileName);
                if (file == null) { common.Warning($"Couldn't open {FileName} for reading."); return false; }
                if (file.Length != sourceFile.fileSize || file.Timestamp != sourceFile.timestamp) { common.Warning($"The file {FileName} has been modified outside of the engine."); return false; }
                file.Read(buffer, oldFileLength);
                fileSystem.CloseFile(file);
                if (byteX.MD5Checksum(buffer) != sourceFile.checksum) { common.Warning($"The file {FileName} has been modified outside of the engine."); return false; }
            }

            // insert new text
            var declText = Encoding.ASCII.GetBytes(Text);
            Unsafe.CopyBlock(ref buffer[sourceTextOffset + textLength], ref buffer[sourceTextOffset + sourceTextLength], (uint)(oldFileLength - sourceTextOffset - sourceTextLength));
            Unsafe.CopyBlock(ref buffer[sourceTextOffset], ref declText[0], (uint)textLength);

            // write out new file
            file = fileSystem.OpenFileWrite(FileName, "fs_devpath");
            if (file == null) { common.Warning($"Couldn't open {FileName} for writing."); return false; }
            file.Write(buffer, newFileLength);
            fileSystem.CloseFile(file);

            // set new file size, checksum and timestamp
            sourceFile.fileSize = newFileLength;
            sourceFile.checksum = byteX.MD5Checksum(buffer);
            fileSystem.ReadFile(FileName, out _, out sourceFile.timestamp);

            // move all decls in the same file
            for (var decl = sourceFile.decls; decl != null; decl = decl.nextInFile)
                if (decl.sourceTextOffset > sourceTextOffset)
                    decl.sourceTextOffset += textLength - sourceTextLength;

            // set new size of text in source file
            sourceTextLength = textLength;

            return true;
        }

        public override bool SourceFileChanged()
        {
            if (sourceFile.fileSize <= 0)
                return false;

            var newLength = fileSystem.ReadFile(FileName, out _, out var newTimestamp);
            return newLength != sourceFile.fileSize || newTimestamp != sourceFile.timestamp;
        }

        static int MakeDefault_recursionLevel;
        public override void MakeDefault()
        {
            declManagerLocal.MediaPrint("DEFAULTED\n");
            declState = DeclState.DS_DEFAULTED;

            AllocateSelf();

            var defaultText = self.DefaultDefinition;

            // a parse error inside a DefaultDefinition() string could cause an infinite loop, but normal default definitions could
            // still reference other default definitions, so we can't just dump out on the first recursion
            if (++MakeDefault_recursionLevel > 100)
                common.FatalError($"Decl::MakeDefault: bad DefaultDefinition: {defaultText}");

            // always free data before parsing
            self.FreeData();

            // parse
            self.Parse(defaultText);

            // we could still eventually hit the recursion if we have enough Error() calls inside Parse...
            --MakeDefault_recursionLevel;
        }

        public override bool EverReferenced => everReferenced;

        public override bool SetDefaultText() => false;

        public override string DefaultDefinition => "{ }";

        public override bool Parse(string text)
        {
            Lexer src = new();
            src.LoadMemory(text, FileName, LineNum);
            src.Flags = DECL_LEXER_FLAGS;
            src.SkipUntilString("{");
            src.SkipBracedSection(false);
            return true;
        }

        public override void FreeData() { }

        public override void List() => common.Printf($"{Name}\n");

        public override void Print() { }

        protected internal void AllocateSelf()
        {
            if (self == null)
            {
                self = declManagerLocal.GetDeclType((int)type).allocator();
                self.base_ = this;
            }
        }

        // Parses the decl definition. After calling parse, a decl will be guaranteed usable.
        protected internal void ParseLocal()
        {
            var generatedDefaultText = false;

            AllocateSelf();

            // always free data before parsing
            self.FreeData();

            declManagerLocal.MediaPrint($"parsing {declManagerLocal.declTypes[(int)type].typeName} {name}\n");

            // if no text source try to generate default text
            if (text == null)
                generatedDefaultText = self.SetDefaultText();

            // indent for DEFAULTED or media file references
            declManagerLocal.indent++;

            // no text immediately causes a MakeDefault()
            if (text == null)
            {
                MakeDefault();
                declManagerLocal.indent--;
                return;
            }

            declState = DeclState.DS_PARSED;

            // parse
            var declText = Text;
            self.Parse(declText);

            // free generated text
            if (generatedDefaultText)
            {
                text = null;
                textLength = 0;
            }

            declManagerLocal.indent--;
        }

        // Does a MakeDefualt, but flags the decl so that it will Parse() the next time the decl is found.
        protected internal void Purge()
        {
            // never purge things that were referenced outside level load, like the console and menu graphics
            if (parsedOutsideLevelLoad)
                return;

            referencedThisLevel = false;
            MakeDefault();

            // the next Find() for this will re-parse the real data
            declState = DeclState.DS_UNPARSED;
        }
    }

    unsafe class Huffman
    {
        const int MAX_HUFFMAN_SYMBOLS = 256;

        class HuffmanNode
        {
            public int symbol;
            public int frequency;
            public HuffmanNode next;
            public HuffmanNode children0;
            public HuffmanNode children1;
        }

        unsafe struct HuffmanCode
        {
            public fixed uint bits[8];
            public int numBits;
        }

        // compression ratio = 64%
        public static int[] HuffmanFrequencies = {
            0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001,
            0x00000001, 0x00078fb6, 0x000352a7, 0x00000002, 0x00000001, 0x0002795e, 0x00000001, 0x00000001,
            0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001,
            0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001,
            0x00049600, 0x000000dd, 0x00018732, 0x0000005a, 0x00000007, 0x00000092, 0x0000000a, 0x00000919,
            0x00002dcf, 0x00002dda, 0x00004dfc, 0x0000039a, 0x000058be, 0x00002d13, 0x00014d8c, 0x00023c60,
            0x0002ddb0, 0x0000d1fc, 0x000078c4, 0x00003ec7, 0x00003113, 0x00006b59, 0x00002499, 0x0000184a,
            0x0000250b, 0x00004e38, 0x000001ca, 0x00000011, 0x00000020, 0x000023da, 0x00000012, 0x00000091,
            0x0000000b, 0x00000b14, 0x0000035d, 0x0000137e, 0x000020c9, 0x00000e11, 0x000004b4, 0x00000737,
            0x000006b8, 0x00001110, 0x000006b3, 0x000000fe, 0x00000f02, 0x00000d73, 0x000005f6, 0x00000be4,
            0x00000d86, 0x0000014d, 0x00000d89, 0x0000129b, 0x00000db3, 0x0000015a, 0x00000167, 0x00000375,
            0x00000028, 0x00000112, 0x00000018, 0x00000678, 0x0000081a, 0x00000677, 0x00000003, 0x00018112,
            0x00000001, 0x000441ee, 0x000124b0, 0x0001fa3f, 0x00026125, 0x0005a411, 0x0000e50f, 0x00011820,
            0x00010f13, 0x0002e723, 0x00003518, 0x00005738, 0x0002cc26, 0x0002a9b7, 0x0002db81, 0x0003b5fa,
            0x000185d2, 0x00001299, 0x00030773, 0x0003920d, 0x000411cd, 0x00018751, 0x00005fbd, 0x000099b0,
            0x00009242, 0x00007cf2, 0x00002809, 0x00005a1d, 0x00000001, 0x00005a1d, 0x00000001, 0x00000001,

            0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001,
            0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001,
            0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001,
            0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001,
            0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001,
            0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001,
            0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001,
            0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001,
            0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001,
            0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001,
            0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001,
            0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001,
            0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001,
            0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001,
            0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001,
            0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001, 0x00000001,
        };

        static readonly HuffmanCode[] huffmanCodes = new HuffmanCode[MAX_HUFFMAN_SYMBOLS];
        static HuffmanNode huffmanTree = null;
        static int totalUncompressedLength = 0;
        static int totalCompressedLength = 0;
        public static int maxHuffmanBits = 0;

        public static void ClearHuffmanFrequencies()
        {
            for (var i = 0; i < MAX_HUFFMAN_SYMBOLS; i++)
                HuffmanFrequencies[i] = 1;
        }

        static HuffmanNode InsertHuffmanNode(HuffmanNode firstNode, HuffmanNode node)
        {
            HuffmanNode n, lastNode = null;

            for (n = firstNode; n != null; n = n.next)
            {
                if (node.frequency <= n.frequency)
                    break;
                lastNode = n;
            }
            if (lastNode != null) { node.next = lastNode.next; lastNode.next = node; }
            else { node.next = firstNode; firstNode = node; }
            return firstNode;
        }

        static void BuildHuffmanCode_r(HuffmanNode node, HuffmanCode code, HuffmanCode[] codes)
        {
            if (node.symbol == -1)
            {
                var newCode = code;
                Debug.Assert(code.numBits < sizeof(ulong) * 8);
                newCode.numBits++;
                if (code.numBits > maxHuffmanBits) maxHuffmanBits = newCode.numBits;
                BuildHuffmanCode_r(node.children0, newCode, codes);
                newCode.bits[code.numBits >> 5] |= (uint)(1 << (code.numBits & 31));
                BuildHuffmanCode_r(node.children1, newCode, codes);
            }
            else
            {
                Debug.Assert(code.numBits <= sizeof(ulong) * 8);
                codes[node.symbol] = code;
            }
        }

        static void FreeHuffmanTree_r(HuffmanNode node)
        {
            if (node.symbol == -1)
            {
                FreeHuffmanTree_r(node.children0);
                FreeHuffmanTree_r(node.children1);
            }
        }

        static int HuffmanHeight_r(HuffmanNode node)
        {
            if (node == null)
                return -1;
            var left = HuffmanHeight_r(node.children0);
            var right = HuffmanHeight_r(node.children1);
            return left > right ? left + 1 : right + 1;
        }

        public static void SetupHuffman()
        {
            int i, height;
            HuffmanNode firstNode, node;
            HuffmanCode code;

            firstNode = null;
            for (i = 0; i < MAX_HUFFMAN_SYMBOLS; i++)
            {
                node = new HuffmanNode
                {
                    symbol = i,
                    frequency = HuffmanFrequencies[i],
                    next = null,
                    children0 = null,
                    children1 = null
                };
                firstNode = InsertHuffmanNode(firstNode, node);
            }

            for (i = 1; i < MAX_HUFFMAN_SYMBOLS; i++)
            {
                node = new HuffmanNode
                {
                    symbol = -1,
                    frequency = firstNode.frequency + firstNode.next.frequency,
                    next = null,
                    children0 = firstNode,
                    children1 = firstNode.next
                };
                firstNode = InsertHuffmanNode(firstNode.next.next, node);
            }

            maxHuffmanBits = 0;
            code = new();
            BuildHuffmanCode_r(firstNode, code, huffmanCodes);

            huffmanTree = firstNode;

            height = HuffmanHeight_r(firstNode);
            Debug.Assert(maxHuffmanBits == height);
        }

        public static void ShutdownHuffman()
        {
            if (huffmanTree != null)
                FreeHuffmanTree_r(huffmanTree);
        }

        public static int HuffmanCompressText(string text, int textLength, byte[] compressed)
        {
            int i, j;
            BitMsg msg = new();

            totalUncompressedLength += textLength;

            msg.InitW(compressed);
            msg.BeginWriting();
            for (i = 0; i < textLength; i++)
            {
                var code = huffmanCodes[(byte)text[i]];
                for (j = 0; j < (code.numBits >> 5); j++)
                    msg.WriteBits((int)code.bits[j], 32);
                if ((code.numBits & 31) != 0)
                    msg.WriteBits((int)code.bits[j], code.numBits & 31);
            }

            totalCompressedLength += msg.Size;

            return msg.Size;
        }

        public unsafe static int HuffmanDecompressText(out string text, int textLength, byte[] compressed)
        {
            int i; BitMsg msg = new(); HuffmanNode node;

            msg.InitW(compressed);
            msg.Size = compressed.Length;
            msg.BeginReading();
            var buffer = stackalloc char[textLength];
            for (i = 0; i < textLength; i++)
            {
                node = huffmanTree;
                do node = msg.ReadBits(1) == 0
                    ? node.children0
                    : node.children1;
                while (node.symbol == -1);
                buffer[i] = (char)node.symbol;
            }
            text = new string(buffer, 0, i);
            return msg.ReadCount;
        }

        public static void ListHuffmanFrequencies_f(CmdArgs args)
        {
            int i; float compression;
            compression = totalUncompressedLength == 0 ? 100 : 100 * totalCompressedLength / totalUncompressedLength;
            common.Printf($"// compression ratio = {(int)compression}%\n");
            common.Printf("static int HuffmanFrequencies[] = {\n");
            for (i = 0; i < MAX_HUFFMAN_SYMBOLS; i += 8)
                common.Printf($"\t0x{HuffmanFrequencies[i + 0]:x08}, 0x{HuffmanFrequencies[i + 1]:x08}, 0x{HuffmanFrequencies[i + 2]:x08}, 0x{HuffmanFrequencies[i + 3]:x08}, 0x{HuffmanFrequencies[i + 4]:x08}, 0x{HuffmanFrequencies[i + 5]:x08}, 0x{HuffmanFrequencies[i + 6]:x08}, 0x{HuffmanFrequencies[i + 7]:x08},\n");
            common.Printf("}\n");
        }
    }

    class DeclFile
    {
        public string fileName;
        public DECL defaultType;

        public DateTime timestamp;
        public int checksum;
        public int fileSize;
        public int numLines;

        public DeclLocal decls;

        public DeclFile()
        {
            this.fileName = "<implicit file>";
            this.defaultType = DECL.MAX_TYPES;
            this.timestamp = DateTime.MinValue;
            this.checksum = 0;
            this.fileSize = 0;
            this.numLines = 0;
            this.decls = null;
        }

        public DeclFile(string fileName, DECL defaultType)
        {
            this.fileName = fileName;
            this.defaultType = defaultType;
            this.timestamp = DateTime.MinValue;
            this.checksum = 0;
            this.fileSize = 0;
            this.numLines = 0;
            this.decls = null;
        }

        // ForceReload will cause it to reload even if the timestamp hasn't changed
        public void Reload(bool force)
        {
            // check for an unchanged timestamp
            if (!force && timestamp != DateTime.MinValue)
            {
                fileSystem.ReadFile(fileName, out _, out var testTimeStamp);
                if (testTimeStamp == timestamp)
                    return;
            }

            // parse the text
            LoadAndParse();
        }

        static int c_savedMemory = 0;
        // This is used during both the initial load, and any reloads
        public int LoadAndParse()
        {
            int i, numTypes;
            Lexer src = new();
            Token token;
            int startMarker;
            int length, size;
            int sourceLine;
            string name;
            DeclLocal newDecl;
            bool reparse;

            // load the text
            common.DPrintf($"...loading '{fileName}'\n");
            length = fileSystem.ReadFile(fileName, out var buffer, out timestamp);
            if (length == -1) { common.FatalError($"couldn't load {fileName}"); return 0; }
            if (!src.LoadMemory(Encoding.ASCII.GetString(buffer), fileName)) { common.Error($"Couldn't parse {fileName}"); return 0; }

            // mark all the defs that were from the last reload of this file
            for (var decl = decls; decl != null; decl = decl.nextInFile)
                decl.redefinedInReload = false;

            src.Flags = DeclBase.DECL_LEXER_FLAGS;

            checksum = byteX.MD5Checksum(buffer);

            fileSize = length;

            // scan through, identifying each individual declaration
            while (true)
            {
                startMarker = src.FileOffset;
                sourceLine = src.LineNum;

                // parse the decl type name
                if (!src.ReadToken(out token))
                    break;

                var identifiedType = DECL.MAX_TYPES;

                // get the decl type from the type name
                numTypes = declManagerLocal.NumDeclTypes;
                for (i = 0; i < numTypes; i++)
                {
                    var typeInfo = declManagerLocal.GetDeclType(i);
                    if (typeInfo != null && string.Equals(typeInfo.typeName, token, StringComparison.OrdinalIgnoreCase))
                    {
                        identifiedType = typeInfo.type;
                        break;
                    }
                }

                if (i >= numTypes)
                    if (token == "{")
                    {
                        // if we ever see an open brace, we somehow missed the [type] <name> prefix
                        src.Warning("Missing decl name");
                        src.SkipBracedSection(false);
                        continue;
                    }
                    else
                    {
                        if (defaultType == DECL.MAX_TYPES) { src.Warning("No type"); continue; }
                        src.UnreadToken(token);
                        // use the default type
                        identifiedType = defaultType;
                    }

                // now parse the name
                if (!src.ReadToken(out token)) { src.Warning("Type without definition at end of file"); break; }

                // if we ever see an open brace, we somehow missed the [type] <name> prefix
                if (token == "{") { src.Warning("Missing decl name"); src.SkipBracedSection(false); continue; }

                // FIXME: export decls are only used by the model exporter, they are skipped here for now
                if (identifiedType == DECL.MODELEXPORT) { src.SkipBracedSection(); continue; }

                name = token;

                // make sure there's a '{'
                if (!src.ReadToken(out token)) { src.Warning("Type without definition at end of file"); break; }
                if (token != "{") { src.Warning($"Expecting '{{' but found '{token}'"); continue; }
                src.UnreadToken(token);

                // now take everything until a matched closing brace
                src.SkipBracedSection();
                size = src.FileOffset - startMarker;

                // look it up, possibly getting a newly created default decl
                reparse = false;
                newDecl = declManagerLocal.FindTypeWithoutParsing(identifiedType, name, false);
                if (newDecl != null)
                {
                    // update the existing copy
                    if (newDecl.sourceFile != this || newDecl.redefinedInReload) { src.Warning($"{declManagerLocal.GetDeclNameFromType(identifiedType)} '{name}' previously defined at {newDecl.sourceFile.fileName}:{newDecl.sourceLine}"); continue; }
                    if (newDecl.declState != DeclState.DS_UNPARSED) reparse = true;
                }
                else
                {
                    // allow it to be created as a default, then add it to the per-file list
                    newDecl = declManagerLocal.FindTypeWithoutParsing(identifiedType, name, true);
                    newDecl.nextInFile = this.decls;
                    this.decls = newDecl;
                }

                newDecl.redefinedInReload = true;

                if (newDecl.text != null)
                    newDecl.text = null;

                newDecl.Text = Encoding.ASCII.GetString(buffer, startMarker, size);
                newDecl.sourceFile = this;
                newDecl.sourceTextOffset = startMarker;
                newDecl.sourceTextLength = size;
                newDecl.sourceLine = sourceLine;
                newDecl.declState = DeclState.DS_UNPARSED;

                // if it is currently in use, reparse it immedaitely
                if (reparse)
                    newDecl.ParseLocal();
            }

            numLines = src.LineNum;

            // any defs that weren't redefinedInReload should now be defaulted
            for (var decl = decls; decl != null; decl = decl.nextInFile)
                if (!decl.redefinedInReload)
                {
                    decl.MakeDefault();
                    decl.sourceTextOffset = decl.sourceFile.fileSize;
                    decl.sourceTextLength = 0;
                    decl.sourceLine = decl.sourceFile.numLines;
                }

            return checksum;
        }
    }

    class DeclManagerLocal : DeclManager
    {
        static T DeclAllocator<T>() where T : new() => new();
        static CmdFunction ListDecls_f(DECL type) => args => declManager.ListType(args, type);
        static CmdFunction PrintDecls_f(DECL type) => args => declManager.PrintType(args, type);
        static ArgCompletion ArgCompletion_Decl(DECL type) => (args, callback) => ArgCompletion_DeclName(args, callback, type);

        static void ArgCompletion_DeclName(CmdArgs args, Action<string> callback, DECL type)
        {
            if (declManager == null)
                return;
            var num = declManager.GetNumDecls(type);
            for (var i = 0; i < num; i++)
                callback($"{args[0]} {declManager.DeclByIndex(type, i, false).Name}");
        }

        static readonly string[] listDeclStrings = { "current", "all", "ever", null };

        internal List<DeclType_> declTypes = new();
        List<DeclFolder> declFolders = new();

        List<DeclFile> loadedFiles = new();
        HashIndex[] hashTables = new HashIndex[(int)DECL.MAX_TYPES];
        List<DeclLocal>[] linearLists = new List<DeclLocal>[(int)DECL.MAX_TYPES];
        internal DeclFile implicitDecls; // this holds all the decls that were created because explicit text definitions were not found. Decls that became default because of a parse error are not in this list.
        int checksum;       // checksum of all loaded decl text
        internal int indent;         // for MediaPrint
        bool insideLevelLoad;

        static CVar decl_show = new("decl_show", "0", CVAR.SYSTEM, "set to 1 to print parses, 2 to also print references", 0, 2, CmdArgs.ArgCompletion_Integer(0, 2));

        public DeclType_ GetDeclType(int type) => declTypes[type];

        public DeclFile ImplicitDeclFile => implicitDecls;

        public virtual void Init()
        {
            common.Printf("----- Initializing Decls -----\n");

            checksum = 0;

#if USE_COMPRESSED_DECLS
            Huffman.SetupHuffman();
#endif

#if GET_HUFFMAN_FREQUENCIES
            Huffman.ClearHuffmanFrequencies();
#endif

            // decls used throughout the engine
            RegisterDeclType("table", DECL.TABLE, DeclAllocator<DeclTable>);
            RegisterDeclType("material", DECL.MATERIAL, DeclAllocator<Material>);
            RegisterDeclType("skin", DECL.SKIN, DeclAllocator<DeclSkin>);
            RegisterDeclType("sound", DECL.SOUND, DeclAllocator<SoundShader>);

            RegisterDeclType("entityDef", DECL.ENTITYDEF, DeclAllocator<DeclEntityDef>);
            RegisterDeclType("mapDef", DECL.MAPDEF, DeclAllocator<DeclEntityDef>);
            RegisterDeclType("fx", DECL.FX, DeclAllocator<DeclFX>);
            RegisterDeclType("particle", DECL.PARTICLE, DeclAllocator<DeclParticle>);
            RegisterDeclType("articulatedFigure", DECL.AF, DeclAllocator<DeclAF>);
            RegisterDeclType("pda", DECL.PDA, DeclAllocator<DeclPDA>);
            RegisterDeclType("email", DECL.EMAIL, DeclAllocator<DeclEmail>);
            RegisterDeclType("video", DECL.VIDEO, DeclAllocator<DeclVideo>);
            RegisterDeclType("audio", DECL.AUDIO, DeclAllocator<DeclAudio>);

            RegisterDeclFolder("materials", ".mtr", DECL.MATERIAL);
            RegisterDeclFolder("skins", ".skin", DECL.SKIN);
            RegisterDeclFolder("sound", ".sndshd", DECL.SOUND);

            // add console commands
            cmdSystem.AddCommand("listDecls", ListDecls_f, CMD_FL.SYSTEM, "lists all decls");

            cmdSystem.AddCommand("reloadDecls", ReloadDecls_f, CMD_FL.SYSTEM, "reloads decls");
            cmdSystem.AddCommand("touch", TouchDecl_f, CMD_FL.SYSTEM, "touches a decl");

            cmdSystem.AddCommand("listTables", ListDecls_f(DECL.TABLE), CMD_FL.SYSTEM, "lists tables", CmdArgs.ArgCompletion_String(listDeclStrings));
            cmdSystem.AddCommand("listMaterials", ListDecls_f(DECL.MATERIAL), CMD_FL.SYSTEM, "lists materials", CmdArgs.ArgCompletion_String(listDeclStrings));
            cmdSystem.AddCommand("listSkins", ListDecls_f(DECL.SKIN), CMD_FL.SYSTEM, "lists skins", CmdArgs.ArgCompletion_String(listDeclStrings));
            cmdSystem.AddCommand("listSoundShaders", ListDecls_f(DECL.SOUND), CMD_FL.SYSTEM, "lists sound shaders", CmdArgs.ArgCompletion_String(listDeclStrings));

            cmdSystem.AddCommand("listEntityDefs", ListDecls_f(DECL.ENTITYDEF), CMD_FL.SYSTEM, "lists entity defs", CmdArgs.ArgCompletion_String(listDeclStrings));
            cmdSystem.AddCommand("listFX", ListDecls_f(DECL.FX), CMD_FL.SYSTEM, "lists FX systems", CmdArgs.ArgCompletion_String(listDeclStrings));
            cmdSystem.AddCommand("listParticles", ListDecls_f(DECL.PARTICLE), CMD_FL.SYSTEM, "lists particle systems", CmdArgs.ArgCompletion_String(listDeclStrings));
            cmdSystem.AddCommand("listAF", ListDecls_f(DECL.AF), CMD_FL.SYSTEM, "lists articulated figures", CmdArgs.ArgCompletion_String(listDeclStrings));

            cmdSystem.AddCommand("listPDAs", ListDecls_f(DECL.PDA), CMD_FL.SYSTEM, "lists PDAs", CmdArgs.ArgCompletion_String(listDeclStrings));
            cmdSystem.AddCommand("listEmails", ListDecls_f(DECL.EMAIL), CMD_FL.SYSTEM, "lists Emails", CmdArgs.ArgCompletion_String(listDeclStrings));
            cmdSystem.AddCommand("listVideos", ListDecls_f(DECL.VIDEO), CMD_FL.SYSTEM, "lists Videos", CmdArgs.ArgCompletion_String(listDeclStrings));
            cmdSystem.AddCommand("listAudios", ListDecls_f(DECL.AUDIO), CMD_FL.SYSTEM, "lists Audios", CmdArgs.ArgCompletion_String(listDeclStrings));

            cmdSystem.AddCommand("printTable", PrintDecls_f(DECL.TABLE), CMD_FL.SYSTEM, "prints a table", ArgCompletion_Decl(DECL.TABLE));
            cmdSystem.AddCommand("printMaterial", PrintDecls_f(DECL.MATERIAL), CMD_FL.SYSTEM, "prints a material", ArgCompletion_Decl(DECL.MATERIAL));
            cmdSystem.AddCommand("printSkin", PrintDecls_f(DECL.SKIN), CMD_FL.SYSTEM, "prints a skin", ArgCompletion_Decl(DECL.SKIN));
            cmdSystem.AddCommand("printSoundShader", PrintDecls_f(DECL.SOUND), CMD_FL.SYSTEM, "prints a sound shader", ArgCompletion_Decl(DECL.SOUND));

            cmdSystem.AddCommand("printEntityDef", PrintDecls_f(DECL.ENTITYDEF), CMD_FL.SYSTEM, "prints an entity def", ArgCompletion_Decl(DECL.ENTITYDEF));
            cmdSystem.AddCommand("printFX", PrintDecls_f(DECL.FX), CMD_FL.SYSTEM, "prints an FX system", ArgCompletion_Decl(DECL.FX));
            cmdSystem.AddCommand("printParticle", PrintDecls_f(DECL.PARTICLE), CMD_FL.SYSTEM, "prints a particle system", ArgCompletion_Decl(DECL.PARTICLE));
            cmdSystem.AddCommand("printAF", PrintDecls_f(DECL.AF), CMD_FL.SYSTEM, "prints an articulated figure", ArgCompletion_Decl(DECL.AF));

            cmdSystem.AddCommand("printPDA", PrintDecls_f(DECL.PDA), CMD_FL.SYSTEM, "prints an PDA", ArgCompletion_Decl(DECL.PDA));
            cmdSystem.AddCommand("printEmail", PrintDecls_f(DECL.EMAIL), CMD_FL.SYSTEM, "prints an Email", ArgCompletion_Decl(DECL.EMAIL));
            cmdSystem.AddCommand("printVideo", PrintDecls_f(DECL.VIDEO), CMD_FL.SYSTEM, "prints a Audio", ArgCompletion_Decl(DECL.VIDEO));
            cmdSystem.AddCommand("printAudio", PrintDecls_f(DECL.AUDIO), CMD_FL.SYSTEM, "prints an Video", ArgCompletion_Decl(DECL.AUDIO));

            cmdSystem.AddCommand("listHuffmanFrequencies", Huffman.ListHuffmanFrequencies_f, CMD_FL.SYSTEM, "lists decl text character frequencies");
        }

        public virtual void Shutdown()
        {
            int i, j; DeclLocal decl;

            // free decls
            for (i = 0; i < (int)DECL.MAX_TYPES; i++)
            {
                for (j = 0; j < linearLists[i].Count; j++)
                {
                    decl = linearLists[i][j];
                    decl.self?.FreeData();
                    if (decl.text != null)
                        decl.text = null;
                }
                linearLists[i].Clear();
                hashTables[i].Free();
            }

            // free decl files
            loadedFiles.Clear();

            // free the decl types and folders
            declTypes.Clear();
            declFolders.Clear();

#if USE_COMPRESSED_DECLS
            Huffman.ShutdownHuffman();
#endif
        }

        public virtual void Reload(bool force)
        {
            for (var i = 0; i < loadedFiles.Count; i++)
                loadedFiles[i].Reload(force);
        }

        public virtual void BeginLevelLoad()
        {
            insideLevelLoad = true;

            // clear all the referencedThisLevel flags and purge all the data so the next reference will cause a reparse
            for (var i = 0; i < (int)DECL.MAX_TYPES; i++)
                for (var j = 0; j < linearLists[i].Count; j++)
                    linearLists[i][j].Purge();
        }

        public virtual void EndLevelLoad()
        {
            insideLevelLoad = false;
            // we don't need to do anything here, but the image manager, model manager, and sound sample manager will need to free media that was not referenced
        }

        public virtual void RegisterDeclType(string typeName, DECL type, Func<Decl> allocator)
        {
            if ((int)type < declTypes.Count && declTypes[(int)type] != null) { common.Warning($"DeclManager::RegisterDeclType: type '{typeName}' already exists"); return; }

            var declType = new DeclType_
            {
                typeName = typeName,
                type = type,
                allocator = allocator
            };

            if ((int)type + 1 > declTypes.Count)
                declTypes.AssureSize((int)type + 1);
            declTypes[(int)type] = declType;
        }

        public virtual void RegisterDeclFolder(string folder, string extension, DECL defaultType)
        {
            int i, j; string fileName; DeclFolder declFolder; FileList fileList; DeclFile df;

            // check whether this folder / extension combination already exists
            for (i = 0; i < declFolders.Count; i++)
                if (string.Equals(declFolders[i].folder, folder, StringComparison.OrdinalIgnoreCase) && string.Equals(declFolders[i].extension, extension, StringComparison.OrdinalIgnoreCase))
                    break;
            if (i < declFolders.Count) declFolder = declFolders[i];
            else declFolders.Add(declFolder = new DeclFolder
            {
                folder = folder,
                extension = extension,
                defaultType = defaultType
            });

            // scan for decl files
            fileList = fileSystem.ListFiles(declFolder.folder, declFolder.extension, true);

            // load and parse decl files
            for (i = 0; i < fileList.NumFiles; i++)
            {
                fileName = $"{declFolder.folder}/{fileList.GetFile(i)}";

                // check whether this file has already been loaded
                for (j = 0; j < loadedFiles.Count; j++)
                    if (string.Equals(fileName, loadedFiles[j].fileName, StringComparison.OrdinalIgnoreCase))
                        break;
                if (j < loadedFiles.Count) df = loadedFiles[j];
                else loadedFiles.Add(df = new DeclFile(fileName, defaultType));
                df.LoadAndParse();
            }

            fileSystem.FreeFileList(fileList);
        }

        public unsafe virtual int GetChecksum()
        {
            int i, j, total, num;

            // get the total number of decls
            total = 0;
            for (i = 0; i < (int)DECL.MAX_TYPES; i++)
                total += linearLists[i].Count;

            var checksumData = new byte[total * 2 * sizeof(int)];
            fixed (byte* checksumDataB = checksumData)
            {
                var checksumDataI = (int*)checksumDataB;

                total = 0;
                for (i = 0; i < (int)DECL.MAX_TYPES; i++)
                {
                    var type = (DECL)i;

                    // FIXME: not particularly pretty but PDAs and associated decls are localized and should not be checksummed
                    if (type == DECL.PDA || type == DECL.VIDEO || type == DECL.AUDIO || type == DECL.EMAIL)
                        continue;

                    num = linearLists[i].Count;
                    for (j = 0; j < num; j++)
                    {
                        var decl = linearLists[i][j];
                        if (decl.sourceFile == implicitDecls)
                            continue;
                        checksumDataI[total * 2 + 0] = total;
                        checksumDataI[total * 2 + 1] = decl.checksum;
                        total++;
                    }
                }

                Platform.LittleRevBytes(checksumDataB, sizeof(int), total * 2);
            }
            return byteX.MD5Checksum(checksumData);
        }

        public virtual int NumDeclTypes
            => declTypes.Count;

        public virtual int GetNumDecls(DECL type)
        {
            var typeIndex = (int)type;
            if (typeIndex < 0 || typeIndex >= declTypes.Count || declTypes[typeIndex] == null) common.FatalError($"DeclManager::GetNumDecls: bad type: {typeIndex}");
            return linearLists[typeIndex].Count;
        }

        public virtual string GetDeclNameFromType(DECL type)
        {
            var typeIndex = (int)type;
            if (typeIndex < 0 || typeIndex >= declTypes.Count || declTypes[typeIndex] == null) common.FatalError($"DeclManager::GetDeclNameFromType: bad type: {typeIndex}");
            return declTypes[typeIndex].typeName;
        }

        public virtual DECL GetDeclTypeFromName(string typeName)
        {
            for (var i = 0; i < declTypes.Count; i++)
                if (declTypes[i] != null && string.Equals(declTypes[i].typeName, typeName, StringComparison.OrdinalIgnoreCase))
                    return declTypes[i].type;
            return DECL.MAX_TYPES;
        }

        // External users will always cause the decl to be parsed before returning
        public virtual Decl FindType(DECL type, string name, bool makeDefault = true)
        {
            DeclLocal decl;

            if (string.IsNullOrEmpty(name))
            {
                name = "_emptyName";
                //common.Warning("DeclManager::FindType: empty %s name", GetDeclType((int)type).typeName.c_str());
            }

            decl = FindTypeWithoutParsing(type, name, makeDefault);
            if (decl == null)
                return null;

            decl.AllocateSelf();

            // if it hasn't been parsed yet, parse it now
            if (decl.declState == DeclState.DS_UNPARSED)
                decl.ParseLocal();

            // mark it as referenced
            decl.referencedThisLevel = true;
            decl.everReferenced = true;
            if (insideLevelLoad)
                decl.parsedOutsideLevelLoad = false;

            return decl.self;
        }

        public virtual Decl DeclByIndex(DECL type, int index, bool forceParse = true)
        {
            var typeIndex = (int)type;

            if (typeIndex < 0 || typeIndex >= declTypes.Count || declTypes[typeIndex] == null) common.FatalError($"DeclManager::DeclByIndex: bad type: {typeIndex}");
            if (index < 0 || index >= linearLists[typeIndex].Count) common.Error($"DeclManager::DeclByIndex: out of range");
            var decl = linearLists[typeIndex][index];

            decl.AllocateSelf();

            if (forceParse && decl.declState == DeclState.DS_UNPARSED)
                decl.ParseLocal();

            return decl.self;
        }

        public virtual Decl FindDeclWithoutParsing(DECL type, string name, bool makeDefault = true)
        {
            var decl = FindTypeWithoutParsing(type, name, makeDefault);
            return decl?.self;
        }

        public virtual void ReloadFile(string filename, bool force)
        {
            for (var i = 0; i < loadedFiles.Count; i++)
                if (string.Equals(loadedFiles[i].fileName, filename, StringComparison.OrdinalIgnoreCase))
                {
                    checksum ^= loadedFiles[i].checksum;
                    loadedFiles[i].Reload(force);
                    checksum ^= loadedFiles[i].checksum;
                }
        }

        /// <summary>
        /// Lists the type.
        /// list*
        /// Lists decls currently referenced
        /// 
        /// list* ever
        /// Lists decls that have been referenced at least once since app launched
        /// 
        /// list* all
        /// Lists every decl declared, even if it hasn't been referenced or parsed
        /// 
        /// FIXME: alphabetized, wildcards?
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public virtual void ListType(CmdArgs args, DECL type)
        {
            var all = string.Equals(args[1], "all", StringComparison.OrdinalIgnoreCase);
            var ever = string.Equals(args[1], "ever", StringComparison.OrdinalIgnoreCase);

            common.Printf("--------------------\n");
            var printed = 0;
            var count = linearLists[(int)type].Count;
            for (var i = 0; i < count; i++)
            {
                var decl = linearLists[(int)type][i];
                if (!all && decl.declState == DeclState.DS_UNPARSED)
                    continue;
                if (!all && !ever && !decl.referencedThisLevel)
                    continue;

                common.Printf(decl.referencedThisLevel ? "*"
                    : decl.everReferenced ? "."
                    : " ");
                common.Printf(decl.declState == DeclState.DS_DEFAULTED ? "D" : " ");
                common.Printf($"{decl.Index:4}: ");
                printed++;
                if (decl.declState == DeclState.DS_UNPARSED)
                    // doesn't have any type specific data yet
                    common.Printf($"{decl.Name}\n");
                else decl.self.List();
            }

            common.Printf("--------------------\n");
            common.Printf($"{printed} of {count} {declTypes[(int)type].typeName}\n");
        }

        public virtual void PrintType(CmdArgs args, DECL type)
        {
            // individual decl types may use additional command parameters
            if (args.Count < 2) { common.Printf("USAGE: Print<decl type> <decl name> [type specific parms]\n"); return; }

            // look it up, skipping the public path so it won't parse or reference
            var decl = FindTypeWithoutParsing(type, args[1], false);
            if (decl == null) { common.Printf($"{declTypes[(int)type].typeName} '{args[1]}' not found.\n"); return; }

            // print information common to all decls
            common.Printf($"{declTypes[(int)type].typeName} {decl.name}:\n");
            common.Printf($"source: {decl.sourceFile.fileName}:{decl.sourceLine}\n");
            common.Printf("----------\n");
            if (decl.text != null) common.Printf($"{decl.Text}\n");
            else common.Printf("NO SOURCE\n");
            common.Printf("----------\n");
            switch (decl.declState)
            {
                case DeclState.DS_UNPARSED: common.Printf("Unparsed.\n"); break;
                case DeclState.DS_DEFAULTED: common.Printf("<DEFAULTED>\n"); break;
                case DeclState.DS_PARSED: common.Printf("Parsed.\n"); break;
            }

            common.Printf(decl.referencedThisLevel ? "Currently referenced this level.\n"
                : decl.everReferenced ? "Referenced in a previous level.\n"
                : "Never referenced.\n");

            // allow type-specific data to be printed
            if (decl.self != null)
                decl.self.Print();
        }

        public unsafe virtual Decl CreateNewDecl(DECL type, string name, string fileName)
        {
            var typeIndex = (int)type;
            int i, hash;

            if (typeIndex < 0 || typeIndex >= declTypes.Count || declTypes[typeIndex] == null) common.FatalError($"DeclManager::CreateNewDecl: bad type: {typeIndex}");

            MakeNameCanonical(name, out var canonicalName);

            fileName = PathX.BackSlashesToSlashes(fileName);

            // see if it already exists
            hash = hashTables[typeIndex].GenerateKey(canonicalName, false);
            for (i = hashTables[typeIndex].First(hash); i >= 0; i = hashTables[typeIndex].Next(i))
                if (string.Equals(linearLists[typeIndex][i].name, canonicalName, StringComparison.OrdinalIgnoreCase))
                {
                    linearLists[typeIndex][i].AllocateSelf();
                    return linearLists[typeIndex][i].self;
                }

            DeclFile sourceFile;

            // find existing source file or create a new one
            for (i = 0; i < loadedFiles.Count; i++)
                if (string.Equals(loadedFiles[i].fileName, fileName, StringComparison.OrdinalIgnoreCase))
                    break;
            if (i < loadedFiles.Count) sourceFile = loadedFiles[i];
            else loadedFiles.Add(sourceFile = new DeclFile(fileName, type));

            var decl = new DeclLocal
            {
                name = canonicalName,
                type = type,
                declState = DeclState.DS_UNPARSED,
                sourceFile = sourceFile,
                sourceTextOffset = sourceFile.fileSize,
                sourceTextLength = 0,
                sourceLine = sourceFile.numLines,
            };
            decl.AllocateSelf();
            decl.Text = $"{declTypes[typeIndex].typeName} {canonicalName} {decl.self.DefaultDefinition}";
            decl.ParseLocal();

            // add this decl to the source file list
            decl.nextInFile = sourceFile.decls;
            sourceFile.decls = decl;

            // add it to the hash table and linear list
            decl.index = linearLists[typeIndex].Count;
            hashTables[typeIndex].Add(hash, linearLists[typeIndex].Add_(decl));

            return decl.self;
        }

        // BSM Added for the material editors rename capabilities
        public virtual bool RenameDecl(DECL type, string oldName, string newName)
        {
            MakeNameCanonical(oldName, out var canonicalOldName);
            MakeNameCanonical(newName, out var canonicalNewName);

            DeclLocal decl = null;

            // make sure it already exists
            int typeIndex = (int)type;
            int i, hash;
            hash = hashTables[typeIndex].GenerateKey(canonicalOldName, false);
            for (i = hashTables[typeIndex].First(hash); i >= 0; i = hashTables[typeIndex].Next(i))
                if (string.Equals(linearLists[typeIndex][i].name, canonicalOldName, StringComparison.OrdinalIgnoreCase))
                {
                    decl = linearLists[typeIndex][i];
                    break;
                }
            if (decl == null)
                return false;

            // Change the name
            decl.name = canonicalNewName;

            // add it to the hash table
            var newhash = hashTables[typeIndex].GenerateKey(canonicalNewName, false);
            hashTables[typeIndex].Add(newhash, decl.index);

            // remove the old hash item
            hashTables[typeIndex].Remove(hash, decl.index);

            return true;
        }

        // This is just used to nicely indent media caching prints
        public virtual void MediaPrint(string fmt, params object[] args)
        {
            if (decl_show.Integer == 0)
                return;
            for (var i = 0; i < indent; i++)
                common.Printf("    ");
            common.Printf(args.Length == 0 ? fmt : string.Format(fmt, args));
        }

        public virtual void WritePrecacheCommands(VFile f)
        {
            for (var i = 0; i < declTypes.Count; i++)
            {
                if (declTypes[i] == null) continue;

                var num = linearLists[i].Count;
                for (var j = 0; j < num; j++)
                {
                    var decl = linearLists[i][j];
                    if (!decl.referencedThisLevel) continue;

                    var str = $"touch {declTypes[i].typeName} {decl.Name}\n";
                    common.Printf(str);
                    f.Printf(str);
                }
            }
        }

        public virtual Material FindMaterial(string name, bool makeDefault = true) => (Material)FindType(DECL.MATERIAL, name, makeDefault);
        public virtual DeclSkin FindSkin(string name, bool makeDefault = true) => (DeclSkin)FindType(DECL.SKIN, name, makeDefault);
        public virtual ISoundShader FindSound(string name, bool makeDefault = true) => (ISoundShader)FindType(DECL.SOUND, name, makeDefault);

        public virtual Material MaterialByIndex(int index, bool forceParse = true) => (Material)DeclByIndex(DECL.MATERIAL, index, forceParse);
        public virtual DeclSkin SkinByIndex(int index, bool forceParse = true) => (DeclSkin)DeclByIndex(DECL.SKIN, index, forceParse);
        public virtual ISoundShader SoundByIndex(int index, bool forceParse = true) => (ISoundShader)DeclByIndex(DECL.SOUND, index, forceParse);

        public static unsafe void MakeNameCanonical(string name, out string result)
        {
            var maxLength = name.Length;
            var buf = stackalloc char[maxLength];

            int i, lastDot = -1;
            for (i = 0; i < maxLength; i++)
            {
                var c = name[i];
                if (c == '\\') buf[i] = '/';
                else if (c == '.') { buf[i] = c; lastDot = i; }
                else buf[i] = char.ToLowerInvariant(c);
            }
            result = new string(buf, 0, lastDot != -1 ? lastDot : i);
        }

        // This finds or creats the decl, but does not cause a parse.  This is only used internally.
        public DeclLocal FindTypeWithoutParsing(DECL type, string name, bool makeDefault = true)
        {
            int typeIndex = (int)type;
            int i, hash;

            if (typeIndex < 0 || typeIndex >= declTypes.Count || declTypes[typeIndex] == null)
                common.FatalError($"DeclManager::FindTypeWithoutParsing: bad type: {typeIndex}");

            MakeNameCanonical(name, out var canonicalName);

            // see if it already exists
            hash = hashTables[typeIndex].GenerateKey(canonicalName, false);
            for (i = hashTables[typeIndex].First(hash); i >= 0; i = hashTables[typeIndex].Next(i))
            {
                if (string.Equals(linearLists[typeIndex][i].name, canonicalName, StringComparison.OrdinalIgnoreCase))
                {
                    // only print these when decl_show is set to 2, because it can be a lot of clutter
                    if (decl_show.Integer > 1) MediaPrint($"referencing {declTypes[(int)type].typeName} {name}\n");
                    return linearLists[typeIndex][i];
                }
            }

            if (!makeDefault)
                return null;

            var decl = new DeclLocal
            {
                self = null,
                name = canonicalName,
                type = type,
                declState = DeclState.DS_UNPARSED,
                text = null,
                textLength = 0,
                sourceFile = implicitDecls,
                referencedThisLevel = false,
                everReferenced = false,
                parsedOutsideLevelLoad = !insideLevelLoad,
                // add it to the linear list and hash table
                index = linearLists[typeIndex].Count
            };
            hashTables[typeIndex].Add(hash, linearLists[typeIndex].Add_(decl));

            return decl;
        }

        static void ListDecls_f(CmdArgs args)
        {
            int i, j;
            int totalDecls = 0;
            int totalText = 0;
            int totalStructs = 0;

            for (i = 0; i < declManagerLocal.declTypes.Count; i++)
            {
                int size, num;

                if (declManagerLocal.declTypes[i] == null) continue;

                num = declManagerLocal.linearLists[i].Count;
                totalDecls += num;

                size = 0;
                for (j = 0; j < num; j++)
                {
                    size += declManagerLocal.linearLists[i][j].Size;
                    if (declManagerLocal.linearLists[i][j].self != null)
                        size += declManagerLocal.linearLists[i][j].self.Size;
                }
                totalStructs += size;

                common.Printf($"{size >> 10:4}k {num:4} {declManagerLocal.declTypes[i].typeName}\n");
            }

            for (i = 0; i < declManagerLocal.loadedFiles.Count; i++)
            {
                var df = declManagerLocal.loadedFiles[i];
                totalText += df.fileSize;
            }

            common.Printf($"{totalDecls} total decls is {declManagerLocal.loadedFiles.Count} decl files\n");
            common.Printf($"{totalText >> 10}KB in text, {totalStructs >> 10}KB in structures\n");
        }

        // Reload will not find any new files created in the directories, it will only reload existing files. A reload will never cause anything to be purged.
        static void ReloadDecls_f(CmdArgs args)
        {
            bool force;
            if (string.Equals(args[1], "all", StringComparison.OrdinalIgnoreCase)) { force = true; common.Printf("reloading all decl files:\n"); }
            else { force = false; common.Printf("reloading changed decl files:\n"); }

            soundSystem.SetMute(true);

            declManagerLocal.Reload(force);

            soundSystem.SetMute(false);
        }

        static void TouchDecl_f(CmdArgs args)
        {
            int i;

            if (args.Count != 3)
            {
                common.Printf("usage: touch <type> <name>\n");
                common.Printf("valid types: ");
                for (i = 0; i < declManagerLocal.declTypes.Count; i++)
                    if (declManagerLocal.declTypes[i] != null) common.Printf($"{declManagerLocal.declTypes[i].typeName} ");
                common.Printf("\n");
                return;
            }

            for (i = 0; i < declManagerLocal.declTypes.Count; i++)
                if (declManagerLocal.declTypes[i] != null && string.Equals(declManagerLocal.declTypes[i].typeName, args[1], StringComparison.OrdinalIgnoreCase))
                    break;
            if (i >= declManagerLocal.declTypes.Count) { common.Printf($"unknown decl type '{args[1]}'\n"); return; }

            var decl = declManagerLocal.FindType((DECL)i, args[2], false);
            if (decl == null) common.Printf($"{declManagerLocal.declTypes[i].typeName} '{args[2]}' not found\n");
        }
    }
}
