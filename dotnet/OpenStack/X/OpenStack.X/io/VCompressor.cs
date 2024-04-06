using System.Diagnostics;
using System.Runtime.CompilerServices;
using static System.NumericsX.Platform;

namespace System.NumericsX.OpenStack
{
    #region VCompressor

    public abstract class VCompressor : VFile
    {
        // compressor allocation
        public static VCompressor AllocNoCompression() => new VCompressor_None();
        public static VCompressor AllocBitStream() => new VCompressor_BitStream();
        public static VCompressor AllocRunLength() => new VCompressor_RunLength();
        public static VCompressor AllocRunLength_ZeroBased() => new VCompressor_RunLength_ZeroBased();
        public static VCompressor AllocHuffman() => throw new NotImplementedException(); //new VCompressor_Huffman();
        public static VCompressor AllocArithmetic() => new VCompressor_Arithmetic();
        public static VCompressor AllocLZSS() => new VCompressor_LZSS();
        public static VCompressor AllocLZSS_WordAligned() => new VCompressor_LZSS_WordAligned();
        public static VCompressor AllocLZW() => new VCompressor_LZW();

        // initialization
        public abstract void Init(VFile f, bool compress, int wordLength);
        public abstract void FinishCompress();
        public abstract float CompressionRatio { get; }
    }

    #endregion

    #region VCompressor_None

    public class VCompressor_None : VCompressor
    {
        protected VFile file;
        protected bool compress;

        public VCompressor_None()
        {
            file = null;
            compress = true;
        }

        public override void Init(VFile f, bool compress, int wordLength)
        {
            this.file = f;
            this.compress = compress;
        }
        public override void FinishCompress() { }
        public override float CompressionRatio
            => 0f;
        public override string Name
            => file != null ? file.Name : string.Empty;
        public override string FullPath
            => file != null ? file.FullPath : string.Empty;
        public override int Read(byte[] outData, int outLength)
            => compress || outLength <= 0 ? 0 : file.Read(outData, outLength);
        public override int Write(byte[] inData, int inLength)
            => !compress || inLength <= 0 ? 0 : file.Write(inData, inLength);
        public override int Length
            => file != null ? file.Length : 0;
        public override DateTime Timestamp
            => file != null ? file.Timestamp : DateTime.MinValue;
        public override int Tell
            => file != null ? file.Tell : 0;
        public override void ForceFlush()
           => file?.ForceFlush();
        public override void Flush()
            => file?.ForceFlush();
        public override int Seek(long offset, FS_SEEK origin)
        {
            Error("cannot seek on idCompressor");
            return -1;
        }
    }

    #endregion

    #region VCompressor_BitStream

    /// <summary>
    /// Base class for bit stream compression.
    /// </summary>
    /// <seealso cref="Gengine.Render.VCompressor_None" />
    public class VCompressor_BitStream : VCompressor_None
    {
        protected byte[] buffer = new byte[65536];
        protected int wordLength;

        protected int readTotalBytes;
        protected int readLength;
        protected int readByte;
        protected int readBit;
        protected byte[] readData;

        protected int writeTotalBytes;
        protected int writeLength;
        protected int writeByte;
        protected int writeBit;
        protected byte[] writeData;

        public override void Init(VFile f, bool compress, int wordLength)
        {
            Debug.Assert(wordLength >= 1 && wordLength <= 32);

            this.file = f;
            this.compress = compress;
            this.wordLength = wordLength;

            readTotalBytes = 0;
            readLength = 0;
            readByte = 0;
            readBit = 0;
            readData = null;

            writeTotalBytes = 0;
            writeLength = 0;
            writeByte = 0;
            writeBit = 0;
            writeData = null;
        }

        protected void InitCompress(byte[] inData, int inLength)
        {
            readLength = inLength;
            readByte = 0;
            readBit = 0;
            readData = inData;

            if (writeLength == 0)
            {
                writeLength = buffer.Length;
                writeByte = 0;
                writeBit = 0;
                writeData = buffer;
            }
        }

        protected void InitDecompress(byte[] outData, int outLength)
        {
            if (readLength == 0)
            {
                readLength = file.Read(buffer, buffer.Length);
                readByte = 0;
                readBit = 0;
                readData = buffer;
            }

            writeLength = outLength;
            writeByte = 0;
            writeBit = 0;
            writeData = outData;
        }

        protected void WriteBits(int value, int numBits)
        {
            int put, fraction;

            // Short circuit for writing single bytes at a time
            if (writeBit == 0 && numBits == 8 && writeByte < writeLength)
            {
                writeByte++;
                writeTotalBytes++;
                writeData[writeByte - 1] = (byte)value;
                return;
            }

            while (numBits != 0)
            {
                if (writeBit == 0)
                {
                    if (writeByte >= writeLength)
                        if (writeData == buffer)
                        {
                            file.Write(buffer, writeByte);
                            writeByte = 0;
                        }
                        else
                        {
                            put = numBits;
                            writeBit = put & 7;
                            writeByte += (put >> 3) + (writeBit != 0 ? 1 : 0);
                            writeTotalBytes += (put >> 3) + (writeBit != 0 ? 1 : 0);
                            return;
                        }
                    writeData[writeByte] = 0;
                    writeByte++;
                    writeTotalBytes++;
                }
                put = 8 - writeBit;
                if (put > numBits) put = numBits;
                fraction = value & ((1 << put) - 1);
                writeData[writeByte - 1] |= (byte)(fraction << writeBit);
                numBits -= put;
                value >>= put;
                writeBit = (writeBit + put) & 7;
            }
        }

        protected int ReadBits(int numBits)
        {
            int value, valueBits, get, fraction;

            value = 0;
            valueBits = 0;

            // Short circuit for reading single bytes at a time
            if (readBit == 0 && numBits == 8 && readByte < readLength)
            {
                readByte++;
                readTotalBytes++;
                return readData[readByte - 1];
            }

            while (valueBits < numBits)
            {
                if (readBit == 0)
                {
                    if (readByte >= readLength)
                        if (readData == buffer)
                        {
                            readLength = file.Read(buffer, buffer.Length);
                            readByte = 0;
                        }
                        else
                        {
                            get = numBits - valueBits;
                            readBit = get & 7;
                            readByte += (get >> 3) + (readBit != 0 ? 1 : 0);
                            readTotalBytes += (get >> 3) + (readBit != 0 ? 1 : 0);
                            return value;
                        }
                    readByte++;
                    readTotalBytes++;
                }
                get = 8 - readBit;
                if (get > (numBits - valueBits)) get = numBits - valueBits;
                fraction = readData[readByte - 1];
                fraction >>= readBit;
                fraction &= (1 << get) - 1;
                value |= fraction << valueBits;
                valueBits += get;
                readBit = (readBit + get) & 7;
            }

            return value;
        }

        protected void UnreadBits(int numBits)
        {
            readByte -= numBits >> 3;
            readTotalBytes -= numBits >> 3;
            if (readBit == 0) readBit = 8 - (numBits & 7);
            else
            {
                readBit -= numBits & 7;
                if (readBit <= 0)
                {
                    readByte--;
                    readTotalBytes--;
                    readBit = (readBit + 8) & 7;
                }
            }
            if (readByte < 0)
            {
                readByte = 0;
                readBit = 0;
            }
        }

        protected static unsafe int Compare(byte[] src1, int bitPtr1, byte[] src2, int bitPtr2, int maxBits)
        {
            int i;

            // If the two bit pointers are aligned then we can use a faster comparison
            if ((bitPtr1 & 7) == (bitPtr2 & 7) && maxBits > 16)
            {
                fixed (byte* src1_ = &src1[bitPtr1 >> 3], src2_ = &src2[bitPtr2 >> 3])
                {
                    byte* p1 = src1_;
                    byte* p2 = src2_;

                    int bits = 0, bitsRemain = maxBits;

                    // Compare the first couple bits (if any)
                    if ((bitPtr1 & 7) != 0)
                    {
                        for (i = (bitPtr1 & 7); i < 8; i++, bits++)
                        {
                            if ((((*p1 >> i) ^ (*p2 >> i)) & 1) != 0) return bits;
                            bitsRemain--;
                        }
                        p1++;
                        p2++;
                    }

                    var remain = bitsRemain >> 3;

                    // Compare the middle bytes as ints
                    while (remain >= 4 && (*(int*)p1 == *(int*)p2))
                    {
                        p1 += 4;
                        p2 += 4;
                        remain -= 4;
                        bits += 32;
                    }

                    // Compare the remaining bytes
                    while (remain > 0 && (*p1 == *p2))
                    {
                        p1++;
                        p2++;
                        remain--;
                        bits += 8;
                    }

                    // Compare the last couple of bits (if any)
                    var finalBits = 8;
                    if (remain == 0)
                        finalBits = (bitsRemain & 7);
                    for (i = 0; i < finalBits; i++, bits++) if ((((*p1 >> i) ^ (*p2 >> i)) & 1) != 0) return bits;

                    Debug.Assert(bits == maxBits);
                    return bits;
                }
            }
            else
            {
                for (i = 0; i < maxBits; i++)
                {
                    if ((((src1[bitPtr1 >> 3] >> (bitPtr1 & 7)) ^ (src2[bitPtr2 >> 3] >> (bitPtr2 & 7))) & 1) != 0) break;
                    bitPtr1++;
                    bitPtr2++;
                }
                return i;
            }
        }

        public override int Write(byte[] inData, int inLength)
        {
            if (!compress || inLength <= 0) return 0;

            InitCompress(inData, inLength);

            int i; for (i = 0; i < inLength; i++) WriteBits(ReadBits(8), 8);
            return i;
        }

        public override void FinishCompress()
        {
            if (!compress) return;

            if (writeByte == 0) file.Write(buffer, writeByte);
            writeLength = 0;
            writeByte = 0;
            writeBit = 0;
        }

        public override int Read(byte[] outData, int outLength)
        {
            if (compress || outLength <= 0) return 0;

            InitDecompress(outData, outLength);

            int i; for (i = 0; i < outLength && readLength >= 0; i++) WriteBits(ReadBits(8), 8);
            return i;
        }

        public override float CompressionRatio
            => compress
                ? (readTotalBytes - writeTotalBytes) * 100f / readTotalBytes
                : (writeTotalBytes - readTotalBytes) * 100f / writeTotalBytes;
    }

    #endregion

    #region VCompressor_RunLength

    /// <summary>
    /// The following algorithm implements run length compression with an arbitrary word size.
    /// </summary>
    /// <seealso cref="Gengine.Render.VCompressor_BitStream" />
    public class VCompressor_RunLength : VCompressor_BitStream
    {
        int runLengthCode;

        public override void Init(VFile f, bool compress, int wordLength)
        {
            base.Init(f, compress, wordLength);
            runLengthCode = (1 << wordLength) - 1;
        }

        public override int Write(byte[] inData, int inLength)
        {
            int bits, nextBits, count;

            if (!compress || inLength <= 0) return 0;

            InitCompress(inData, inLength);

            while (readByte <= readLength)
            {
                count = 1;
                bits = ReadBits(wordLength);
                for (nextBits = ReadBits(wordLength); nextBits == bits; nextBits = ReadBits(wordLength))
                {
                    count++;
                    if (count >= (1 << wordLength)) if (count >= (1 << wordLength) + 3 || bits == runLengthCode) break;
                }
                if (nextBits != bits) UnreadBits(wordLength);
                if (count > 3 || bits == runLengthCode)
                {
                    WriteBits(runLengthCode, wordLength);
                    WriteBits(bits, wordLength);
                    if (bits != runLengthCode) count -= 3;
                    WriteBits(count - 1, wordLength);
                }
                else while (count-- != 0) WriteBits(bits, wordLength);
            }

            return inLength;
        }

        public override int Read(byte[] outData, int outLength)
        {
            int bits, count;

            if (compress || outLength <= 0) return 0;

            InitDecompress(outData, outLength);

            while (writeByte <= writeLength && readLength >= 0)
            {
                bits = ReadBits(wordLength);
                if (bits == runLengthCode)
                {
                    bits = ReadBits(wordLength);
                    count = ReadBits(wordLength) + 1;
                    if (bits != runLengthCode) count += 3;
                    while (count-- != 0) WriteBits(bits, wordLength);
                }
                else WriteBits(bits, wordLength);
            }

            return writeByte;
        }
    }

    #endregion

    #region VCompressor_RunLength_ZeroBased

    /// <summary>
    /// The following algorithm implements run length compression with an arbitrary word size for data with a lot of zero bits.
    /// </summary>
    /// <seealso cref="Gengine.Render.VCompressor_BitStream" />
    public class VCompressor_RunLength_ZeroBased : VCompressor_BitStream
    {
        public override int Write(byte[] inData, int inLength)
        {
            int bits, count;

            if (!compress || inLength <= 0) return 0;

            InitCompress(inData, inLength);

            while (readByte <= readLength)
            {
                count = 0;
                for (bits = ReadBits(wordLength); bits == 0 && count < (1 << wordLength); bits = ReadBits(wordLength)) count++;
                if (count != 0)
                {
                    WriteBits(0, wordLength);
                    WriteBits(count - 1, wordLength);
                    UnreadBits(wordLength);
                }
                else WriteBits(bits, wordLength);
            }

            return inLength;
        }

        public override int Read(byte[] outData, int outLength)
        {
            int bits, count;

            if (compress || outLength <= 0) return 0;

            InitDecompress(outData, outLength);

            while (writeByte <= writeLength && readLength >= 0)
            {
                bits = ReadBits(wordLength);
                if (bits == 0)
                {
                    count = ReadBits(wordLength) + 1;
                    while (count-- > 0) WriteBits(0, wordLength);
                }
                else WriteBits(bits, wordLength);
            }

            return writeByte;
        }
    }

    #endregion

    #region VCompressor_Huffman

#if false //:sky
    /// <summary>
    /// The following algorithm is based on the adaptive Huffman algorithm described in Sayood's Data Compression book. The ranks are not actually stored, but
	/// implicitly defined by the location of a node within a doubly-linked list
    /// </summary>
    /// <seealso cref="Gengine.Render.VCompressor_None" />
    public class VCompressor_Huffman : VCompressor_None
    {
        const int HMAX = 256;               // Maximum symbol
        const int NYT = HMAX;               // NYT = Not Yet Transmitted
        const int INTERNAL_NODE = HMAX + 1;         // internal node

        class HuffmanNode
        {
            public HuffmanNode left, right, parent; // tree structure
            public HuffmanNode next, prev;         // doubly-linked list
            public HuffmanNode head;                   // highest ranked node in block
            public int weight;
            public int symbol;
        }

        byte[] seq = new byte[65536];
        int bloc;
        int blocMax;
        int blocIn;
        int blocNode;
        int blocPtrs;

        int compressedSize;
        int unCompressedSize;

        HuffmanNode tree;
        HuffmanNode lhead;
        HuffmanNode ltail;
        HuffmanNode[] loc = new HuffmanNode[HMAX + 1];
        HuffmanNode[] freelist;

        HuffmanNode[] nodeList = new HuffmanNode[768];
        HuffmanNode[] nodePtrs = new HuffmanNode[768];

        public override void Init(VFile f, bool compress, int wordLength)
        {
            int i;

            this.file = f;
            this.compress = compress;
            bloc = 0;
            blocMax = 0;
            blocIn = 0;
            blocNode = 0;
            blocPtrs = 0;
            compressedSize = 0;
            unCompressedSize = 0;

            tree = null;
            lhead = null;
            ltail = null;
            for (i = 0; i < (HMAX + 1); i++) loc[i] = null;
            freelist = null;

            for (i = 0; i < 768; i++)
            {
                nodeList[i] = new HuffmanNode();
                nodePtrs[i] = null;
            }

            if (compress)
            {
                // Add the NYT (not yet transmitted) node into the tree/list
                tree = lhead = loc[NYT] = nodeList[blocNode++];
                tree.symbol = NYT;
                tree.weight = 0;
                lhead.next = lhead.prev = null;
                tree.parent = tree.left = tree.right = null;
                loc[NYT] = tree;
            }
            else
            {
                // Initialize the tree & list with the NYT node
                tree = lhead = ltail = loc[NYT] = nodeList[blocNode++];
                tree.symbol = NYT;
                tree.weight = 0;
                lhead.next = lhead.prev = null;
                tree.parent = tree.left = tree.right = null;
            }
        }

        void PutBit(int bit, byte[] fout, ref int offset)
        {
            bloc = offset;
            if ((bloc & 7) == 0) fout[bloc >> 3] = 0;
            fout[bloc >> 3] |= (byte)(bit << (bloc & 7));
            bloc++;
            offset = bloc;
        }

        int GetBit(byte[] fout, ref int offset)
        {
            bloc = offset;
            var t = (fout[bloc >> 3] >> (bloc & 7)) & 0x1;
            bloc++;
            offset = bloc;
            return t;
        }

        // Add a bit to the output file (buffered)
        void Add_bit(char bit, byte[] fout)
        {
            if ((bloc & 7) == 0) fout[(bloc >> 3)] = 0;
            fout[(bloc >> 3)] |= (byte)(bit << (bloc & 7));
            bloc++;
        }

        // Get one bit from the input file (buffered)
        int Get_bit()
        {
            int t, wh = bloc >> 3, whb = wh >> 16;
            if (whb != blocIn)
            {
                blocMax += file.Read(seq, seq.Length);
                blocIn++;
            }
            wh &= 0xffff;
            t = (seq[wh] >> (bloc & 7)) & 0x1;
            bloc++;
            return t;
        }

        HuffmanNode[] Get_ppnode()
        {
            HuffmanNode tppnode;
            if (freelist == null) return nodePtrs[blocPtrs++];
            else
            {
                tppnode = freelist;
                freelist = (HuffmanNode)tppnode;
                return tppnode;
            }
        }

        void Free_ppnode(ref HuffmanNode ppnode)
        {
            ppnode = (HuffmanNode)freelist;
            freelist = ppnode;
        }

        // Swap the location of the given two nodes in the tree.
        void Swap(HuffmanNode node1, HuffmanNode node2)
        {
            HuffmanNode par1, par2;

            par1 = node1.parent;
            par2 = node2.parent;

            if (par1 != null)
            {
                if (par1.left == node1) par1.left = node2;
                else par1.right = node2;
            }
            else tree = node2;

            if (par2 != null)
            {
                if (par2.left == node2) par2.left = node1;
                else par2.right = node1;
            }
            else tree = node1;

            node1.parent = par2;
            node2.parent = par1;
        }

        // Swap the given two nodes in the linked list (update ranks)
        void Swaplist(HuffmanNode node1, HuffmanNode node2)
        {
            HuffmanNode par1;

            par1 = node1.next; node1.next = node2.next; node2.next = par1;
            par1 = node1.prev; node1.prev = node2.prev; node2.prev = par1;

            if (node1.next == node1) node1.next = node2;
            if (node2.next == node2) node2.next = node1;
            if (node1.next != null) node1.next.prev = node1;
            if (node2.next != null) node2.next.prev = node2;
            if (node1.prev != null) node1.prev.next = node1;
            if (node2.prev != null) node2.prev.next = node2;
        }

        void Increment(HuffmanNode node)
        {
            HuffmanNode lnode;

            if (node == null) return;

            if (node.next != null && node.next.weight == node.weight)
            {
                lnode = node.head;
                if (lnode != node.parent) Swap(lnode, node);
                Swaplist(lnode, node);
            }
            if (node.prev != null && node.prev.weight == node.weight) node.head = node.prev;
            else { node.head = null; Free_ppnode(ref node.head); }
            node.weight++;
            if (node.next != null && node.next.weight == node.weight) node.head = node.next.head;
            else { node.head = Get_ppnode(); node.head = node; }
            if (node.parent != null)
            {
                Increment(node.parent);
                if (node.prev == node.parent)
                {
                    Swaplist(node, node.parent);
                    if (node.head == node) node.head = node.parent;
                }
            }
        }

        void AddRef(byte ch)
        {
            HuffmanNode tnode, tnode2;
            if (loc[ch] == null)
            { // if this is the first transmission of this node
                tnode = nodeList[blocNode++];
                tnode2 = nodeList[blocNode++];

                tnode2.symbol = INTERNAL_NODE;
                tnode2.weight = 1;
                tnode2.next = lhead.next;
                if (lhead.next != null)
                {
                    lhead.next.prev = tnode2;
                    if (lhead.next.weight == 1) tnode2.head = lhead.next.head;
                    else { tnode2.head = Get_ppnode(); tnode2.head = tnode2; }
                }
                else
                {
                    tnode2.head = Get_ppnode();
                    tnode2.head = tnode2;
                }
                lhead.next = tnode2;
                tnode2.prev = lhead;

                tnode.symbol = ch;
                tnode.weight = 1;
                tnode.next = lhead.next;
                if (lhead.next != null)
                {
                    lhead.next.prev = tnode;
                    if (lhead.next.weight == 1) tnode.head = lhead.next.head;
                    else
                    {
                        // this should never happen
                        tnode.head = Get_ppnode();
                        tnode.head = tnode2;
                    }
                }
                else
                {
                    // this should never happen
                    tnode.head = Get_ppnode();
                    tnode.head = tnode;
                }
                lhead.next = tnode;
                tnode.prev = lhead;
                tnode.left = tnode.right = null;

                if (lhead.parent != null)
                {
                    if (lhead.parent.left == lhead) lhead.parent.left = tnode2; // lhead is guaranteed to by the NYT
                    else lhead.parent.right = tnode2;
                }
                else tree = tnode2;

                tnode2.right = tnode;
                tnode2.left = lhead;

                tnode2.parent = lhead.parent;
                lhead.parent = tnode.parent = tnode2;

                loc[ch] = tnode;

                Increment(tnode2.parent);
            }
            else Increment(loc[ch]);
        }

        // Get a symbol.
        int Receive(HuffmanNode node, ref int ch)
        {
            while (node != null && node.symbol == INTERNAL_NODE) node = Get_bit() != 0 ? node.right : node.left;
            return node == null ? 0 : (ch = node.symbol);
        }

        // Send the prefix code for this node.
        void Send(HuffmanNode node, HuffmanNode child, byte[] fout)
        {
            if (node.parent != null) Send(node.parent, node, fout);
            if (child != null) Add_bit((char)(node.right == child ? 1 : 0), fout);
        }

        // Send a symbol.
        void Transmit(int ch, byte[] fout)
        {
            if (loc[ch] == null)
            {
                // HuffmanNode hasn't been transmitted, send a NYT, then the symbol
                Transmit(NYT, fout);
                for (var i = 7; i >= 0; i--) Add_bit((char)((ch >> i) & 0x1), fout);
            }
            else Send(loc[ch], null, fout);
        }

        public override int Write(byte[] inData, int inLength)
        {
            int i, ch;

            if (!compress || inLength <= 0) return 0;

            for (i = 0; i < inLength; i++)
            {
                ch = inData[i];
                Transmit(ch, seq);              // Transmit symbol
                AddRef((byte)ch);               // Do update
                var b = bloc >> 3;
                if (b > 32768)
                {
                    file.Write(seq, b);
                    seq[0] = seq[b];
                    bloc &= 7;
                    compressedSize += b;
                }
            }

            unCompressedSize += i;
            return i;
        }

        public override void FinishCompress()
        {
            if (!compress) return;

            bloc += 7;
            var str = (bloc >> 3);
            if (str != 0)
            {
                file.Write(seq, str);
                compressedSize += str;
            }
        }

        public override int Read(byte[] outData, int outLength)
        {
            int i, j, ch;

            if (compress || outLength <= 0) return 0;

            if (bloc == 0)
            {
                blocMax = file.Read(seq, seq.Length);
                blocIn = 0;
            }

            for (i = 0; i < outLength; i++)
            {
                ch = 0;
                // don't overflow reading from the file
                if ((bloc >> 3) > blocMax) break;
                Receive(tree, ref ch);      // Get a character
                if (ch == NYT)
                {                           // We got a NYT, get the symbol associated with it
                    ch = 0;
                    for (j = 0; j < 8; j++) ch = (ch << 1) + Get_bit();
                }

                outData[i] = (byte)ch;          // Write symbol
                AddRef((byte)ch);               // Increment node
            }

            compressedSize = bloc >> 3;
            unCompressedSize += i;
            return i;
        }

        public override float CompressionRatio
            => (unCompressedSize - compressedSize) * 100f / unCompressedSize;
    }
#endif

    #endregion

    #region VCompressor_Arithmetic

    /// <summary>
    /// The following algorithm is based on the Arithmetic Coding methods described by Mark Nelson. The probability table is implicitly stored.
    /// </summary>
    /// <seealso cref="Gengine.Render.VCompressor_None" />
    public class VCompressor_Arithmetic : VCompressor_BitStream
    {
        const int AC_WORD_LENGTH = 8;
        const int AC_NUM_BITS = 16;
        const int AC_MSB_SHIFT = 15;
        const int AC_MSB2_SHIFT = 14;
        const int AC_MSB_MASK = 0x8000;
        const int AC_MSB2_MASK = 0x4000;
        const int AC_HIGH_INIT = 0xffff;
        const int AC_LOW_INIT = 0x0000;

        struct AcProbs
        {
            public uint low;
            public uint high;
        }

        struct AcSymbol
        {
            public uint low;
            public uint high;
            public int position;
        }

        AcProbs[] probabilities = new AcProbs[1 << AC_WORD_LENGTH];

        int symbolBuffer;
        int symbolBit;

        ushort low;
        ushort high;
        ushort code;
        uint underflowBits;
        uint scale;

        public override void Init(VFile f, bool compress, int wordLength)
        {
            base.Init(f, compress, wordLength);

            symbolBuffer = 0;
            symbolBit = 0;
        }

        void InitProbabilities()
        {
            high = AC_HIGH_INIT;
            low = AC_LOW_INIT;
            underflowBits = 0;
            code = 0;

            for (var i = 0U; i < (1 << AC_WORD_LENGTH); i++)
            {
                probabilities[i].low = i;
                probabilities[i].high = i + 1;
            }

            scale = 1 << AC_WORD_LENGTH;
        }

        void UpdateProbabilities(ref AcSymbol symbol)
        {
            int i, x;

            x = symbol.position;

            probabilities[x].high++;

            for (i = x + 1; i < (1 << AC_WORD_LENGTH); i++)
            {
                probabilities[i].low++;
                probabilities[i].high++;
            }

            scale++;
        }

        int CurrentCount
            => (int)(((code - low + 1) * scale - 1) / (high - low + 1));

        int ProbabilityForCount(uint count)
        {
            int len, mid, offset, res;

            len = 1 << AC_WORD_LENGTH;
            mid = len;
            offset = 0;
            res = 0;
            while (mid > 0)
            {
                mid = len >> 1;
                if (count >= probabilities[offset + mid].high) { offset += mid; len -= mid; res = 1; }
                else if (count < probabilities[offset + mid].low) { len -= mid; res = 0; }
                else return offset + mid;
            }
            return offset + res;
        }

        int SymbolFromCount(uint count, out AcSymbol symbol)
        {
            var p = ProbabilityForCount(count);
            symbol = new AcSymbol
            {
                low = probabilities[p].low,
                high = probabilities[p].high,
                position = p
            };
            return p;
        }

        void RemoveSymbolFromStream(ref AcSymbol symbol)
        {
            int range;

            range = high - low + 1;
            high = (ushort)(low + (ushort)(range * symbol.high / scale - 1));
            low = (ushort)(low + (ushort)(range * symbol.low / scale));

            while (true)
            {
                if ((high & AC_MSB_MASK) == (low & AC_MSB_MASK)) { }
                else if ((low & AC_MSB2_MASK) == AC_MSB2_MASK && (high & AC_MSB2_MASK) == 0)
                {
                    code ^= AC_MSB2_MASK;
                    low &= AC_MSB2_MASK - 1;
                    high |= AC_MSB2_MASK;
                }
                else
                {
                    UpdateProbabilities(ref symbol);
                    return;
                }

                low <<= 1;
                high <<= 1;
                high |= 1;
                code <<= 1;
                code |= (ushort)ReadBits(1);
            }
        }

        int GetBit()
        {
            int getbit;

            if (symbolBit <= 0)
            {
                // read a new symbol out
                symbolBuffer = SymbolFromCount((uint)CurrentCount, out var symbol);
                RemoveSymbolFromStream(ref symbol);
                symbolBit = AC_WORD_LENGTH;
            }

            getbit = (symbolBuffer >> (AC_WORD_LENGTH - symbolBit)) & 1;
            symbolBit--;

            return getbit;
        }

        void EncodeSymbol(ref AcSymbol symbol)
        {
            uint range;

            // rescale high and low for the new symbol.
            range = (uint)(high - low + 1);
            high = (ushort)(low + (ushort)(range * symbol.high / scale - 1));
            low = (ushort)(low + (ushort)(range * symbol.low / scale));

            while (true)
            {
                if ((high & AC_MSB_MASK) == (low & AC_MSB_MASK))
                {
                    // the high digits of low and high have converged, and can be written to the stream
                    WriteBits(high >> AC_MSB_SHIFT, 1);
                    while (underflowBits > 0) { WriteBits(~high >> AC_MSB_SHIFT, 1); underflowBits--; }
                }
                else if ((low & AC_MSB2_MASK) != 0 && (high & AC_MSB2_MASK) == 0)
                {
                    // underflow is in danger of happening, 2nd digits are converging but 1st digits don't match
                    underflowBits += 1;
                    low &= AC_MSB2_MASK - 1;
                    high |= AC_MSB2_MASK;
                }
                else
                {
                    UpdateProbabilities(ref symbol);
                    return;
                }

                low <<= 1;
                high <<= 1;
                high |= 1;
            }
        }

        void CharToSymbol(byte c, out AcSymbol symbol)
        {
            symbol = new AcSymbol
            {
                low = probabilities[c].low,
                high = probabilities[c].high,
                position = c
            };
        }

        void PutBit(int bit)
        {
            symbolBuffer |= (bit & 1) << symbolBit;
            symbolBit++;

            if (symbolBit >= AC_WORD_LENGTH)
            {
                CharToSymbol((byte)symbolBuffer, out var symbol);
                EncodeSymbol(ref symbol);

                symbolBit = 0;
                symbolBuffer = 0;
            }
        }

        void WriteOverflowBits()
        {
            WriteBits(low >> AC_MSB2_SHIFT, 1);

            underflowBits++;
            while (underflowBits-- > 0)
                WriteBits(~low >> AC_MSB2_SHIFT, 1);
        }

        public override int Write(byte[] inData, int inLength)
        {
            int i, j;

            if (!compress || inLength <= 0) return 0;

            InitCompress(inData, inLength);

            for (i = 0; i < inLength; i++)
            {
                if ((readTotalBytes & ((1 << 14) - 1)) == 0)
                {
                    if (readTotalBytes != 0)
                    {
                        WriteOverflowBits();
                        WriteBits(0, 15);
                        while (writeBit != 0) WriteBits(0, 1);
                        WriteBits(255, 8);
                    }
                    InitProbabilities();
                }
                for (j = 0; j < 8; j++) PutBit(ReadBits(1));
            }

            return inLength;
        }

        public override void FinishCompress()
        {
            if (!compress) return;

            WriteOverflowBits();

            base.FinishCompress();
        }

        public override int Read(byte[] outData, int outLength)
        {
            int i, j;

            if (compress || outLength <= 0) return 0;

            InitDecompress(outData, outLength);

            for (i = 0; i < outLength && readLength >= 0; i++)
            {
                if ((writeTotalBytes & ((1 << 14) - 1)) == 0)
                {
                    if (writeTotalBytes != 0)
                    {
                        while (readBit != 0) ReadBits(1);
                        while (ReadBits(8) == 0 && readLength > 0) { }
                    }
                    InitProbabilities();
                    for (j = 0; j < AC_NUM_BITS; j++)
                    {
                        code <<= 1;
                        code |= (ushort)ReadBits(1);
                    }
                }
                for (j = 0; j < 8; j++) WriteBits(GetBit(), 1);
            }

            return i;
        }
    }

    #endregion

    #region VCompressor_LZSS

    /// <summary>
    /// In 1977 Abraham Lempel and Jacob Ziv presented a dictionary based scheme for text compression called LZ77. For any new text LZ77 outputs an offset/length
    /// pair to previously seen text and the next new byte after the previously seen text.
    /// 
    /// In 1982 James Storer and Thomas Szymanski presented a modification on the work of Lempel and Ziv called LZSS. LZ77 always outputs an offset/length pair, even
    /// if a match is only one byte long. An offset/length pair usually takes more than a single byte to store and the compression is not optimal for small match sizes.
    /// LZSS uses a bit flag which tells whether the following data is a literal (byte)  or an offset/length pair.
    /// 
    /// The following algorithm is an implementation of LZSS with arbitrary word size.
    /// </summary>
    /// <seealso cref="Gengine.Render.VCompressor_None" />
    public class VCompressor_LZSS : VCompressor_BitStream
    {
        protected const int LZSS_BLOCK_SIZE = 65535;
        protected const int LZSS_HASH_BITS = 10;
        protected const int LZSS_HASH_SIZE = (1 << LZSS_HASH_BITS);
        protected const int LZSS_HASH_MASK = (1 << LZSS_HASH_BITS) - 1;
        protected const int LZSS_OFFSET_BITS = 11;
        protected const int LZSS_LENGTH_BITS = 5;

        protected int offsetBits;
        protected int lengthBits;
        protected int minMatchWords;

        protected byte[] block = new byte[LZSS_BLOCK_SIZE];
        protected int blockSize;
        protected int blockIndex;

        protected int[] hashTable = new int[LZSS_HASH_SIZE];
        protected int[] hashNext = new int[LZSS_BLOCK_SIZE * 8];

        public override void Init(VFile f, bool compress, int wordLength)
        {
            base.Init(f, compress, wordLength);

            offsetBits = LZSS_OFFSET_BITS;
            lengthBits = LZSS_LENGTH_BITS;

            minMatchWords = (offsetBits + lengthBits + wordLength) / wordLength;
            blockSize = 0;
            blockIndex = 0;
        }

        protected bool FindMatch(int startWord, int startValue, out int wordOffset, out int numWords)
        {
            int i, n, hash, bottom, maxBits;

            wordOffset = startWord;
            numWords = minMatchWords - 1;

            bottom = Math.Max(0, startWord - ((1 << offsetBits) - 1));
            maxBits = (blockSize << 3) - startWord * wordLength;

            hash = startValue & LZSS_HASH_MASK;
            for (i = hashTable[hash]; i >= bottom; i = hashNext[i])
            {
                n = Compare(block, i * wordLength, block, startWord * wordLength, Math.Min(maxBits, (startWord - i) * wordLength));
                if (n > numWords * wordLength)
                {
                    numWords = n / wordLength;
                    wordOffset = i;
                    if (numWords > ((1 << lengthBits) - 1 + minMatchWords) - 1)
                    {
                        numWords = ((1 << lengthBits) - 1 + minMatchWords) - 1;
                        break;
                    }
                }
            }

            return numWords >= minMatchWords;
        }

        protected void AddToHash(int index, int hash)
        {
            hashNext[index] = hashTable[hash];
            hashTable[hash] = index;
        }

        protected int GetWordFromBlock(int wordOffset)
        {
            int blockBit, blockByte, value, valueBits, get, fraction;

            blockBit = (wordOffset * wordLength) & 7;
            blockByte = (wordOffset * wordLength) >> 3;
            if (blockBit != 0)
                blockByte++;

            value = 0;
            valueBits = 0;

            while (valueBits < wordLength)
            {
                if (blockBit == 0)
                {
                    if (blockByte >= LZSS_BLOCK_SIZE) return value;
                    blockByte++;
                }
                get = 8 - blockBit;
                if (get > (wordLength - valueBits))
                    get = (wordLength - valueBits);
                fraction = block[blockByte - 1];
                fraction >>= blockBit;
                fraction &= (1 << get) - 1;
                value |= fraction << valueBits;
                valueBits += get;
                blockBit = (blockBit + get) & 7;
            }

            return value;
        }

        protected unsafe virtual void CompressBlock()
        {
            int i, startWord, startValue;

            InitCompress(block, blockSize);

            fixed (void* hashTable_ = hashTable, hashNext_ = hashNext)
            {
                UnsafeX.InitBlock(hashTable_, -1, (uint)hashTable.Length);
                UnsafeX.InitBlock(hashNext_, -1, (uint)hashNext.Length);
            }

            startWord = 0;
            while (readByte < readLength)
            {
                startValue = ReadBits(wordLength);
                if (FindMatch(startWord, startValue, out var wordOffset, out var numWords))
                {
                    WriteBits(1, 1);
                    WriteBits(startWord - wordOffset, offsetBits);
                    WriteBits(numWords - minMatchWords, lengthBits);
                    UnreadBits(wordLength);
                    for (i = 0; i < numWords; i++)
                    {
                        startValue = ReadBits(wordLength);
                        AddToHash(startWord, startValue & LZSS_HASH_MASK);
                        startWord++;
                    }
                }
                else
                {
                    WriteBits(0, 1);
                    WriteBits(startValue, wordLength);
                    AddToHash(startWord, startValue & LZSS_HASH_MASK);
                    startWord++;
                }
            }

            blockSize = 0;
        }

        protected virtual void DecompressBlock()
        {
            int i, offset, startWord, numWords;

            InitDecompress(block, LZSS_BLOCK_SIZE);

            startWord = 0;
            while (writeByte < writeLength && readLength >= 0)
                if (ReadBits(1) != 0)
                {
                    offset = startWord - ReadBits(offsetBits);
                    numWords = ReadBits(lengthBits) + minMatchWords;
                    for (i = 0; i < numWords; i++)
                    {
                        WriteBits(GetWordFromBlock(offset + i), wordLength);
                        startWord++;
                    }
                }
                else
                {
                    WriteBits(ReadBits(wordLength), wordLength);
                    startWord++;
                }

            blockSize = Math.Min(writeByte, LZSS_BLOCK_SIZE);
        }

        public override int Write(byte[] inData, int inLength)
        {
            int i, n;

            if (!compress || inLength <= 0) return 0;

            for (i = 0; i < inLength; i += n)
            {
                n = LZSS_BLOCK_SIZE - blockSize;
                if (inLength - i >= n)
                {
                    Unsafe.CopyBlock(ref block[blockSize], ref inData[i], (uint)n);
                    blockSize = LZSS_BLOCK_SIZE;
                    CompressBlock();
                    blockSize = 0;
                }
                else
                {
                    Unsafe.CopyBlock(ref block[blockSize], ref inData[i], (uint)(inLength - i));
                    n = inLength - i;
                    blockSize += n;
                }
            }

            return inLength;
        }

        public override void FinishCompress()
        {
            if (!compress) return;
            if (blockSize != 0) CompressBlock();
            base.FinishCompress();
        }

        public override int Read(byte[] outData, int outLength)
        {
            int i, n;

            if (compress || outLength <= 0) return 0;

            if (blockSize == 0) DecompressBlock();

            for (i = 0; i < outLength; i += n)
            {
                if (blockSize == 0) return i;
                n = blockSize - blockIndex;
                if (outLength - i >= n)
                {
                    Unsafe.CopyBlock(ref outData[i], ref block[blockIndex], (uint)n);
                    DecompressBlock();
                    blockIndex = 0;
                }
                else
                {
                    Unsafe.CopyBlock(ref outData[i], ref block[blockIndex], (uint)(outLength - i));
                    n = outLength - i;
                    blockIndex += n;
                }
            }

            return outLength;
        }
    }

    #endregion

    #region VCompressor_LZSS_WordAligned

    /// <summary>
    /// Outputs word aligned compressed data.
    /// </summary>
    /// <seealso cref="Gengine.Library.Core.VCompressor_LZSS" />
    public class VCompressor_LZSS_WordAligned : VCompressor_LZSS
    {
        public override void Init(VFile f, bool compress, int wordLength)
        {
            base.Init(f, compress, wordLength);

            offsetBits = 2 * wordLength;
            lengthBits = wordLength;

            minMatchWords = (offsetBits + lengthBits + wordLength) / wordLength;
            blockSize = 0;
            blockIndex = 0;
        }

        protected unsafe override void CompressBlock()
        {
            int i, startWord, startValue, wordOffset, numWords;

            InitCompress(block, blockSize);

            fixed (void* hashTable_ = hashTable, hashNext_ = hashNext)
            {
                UnsafeX.InitBlock(hashTable_, -1, (uint)hashTable.Length);
                UnsafeX.InitBlock(hashNext_, -1, (uint)hashNext.Length);
            }

            startWord = 0;
            while (readByte < readLength)
            {
                startValue = ReadBits(wordLength);
                if (FindMatch(startWord, startValue, out wordOffset, out numWords))
                {
                    WriteBits(numWords - (minMatchWords - 1), lengthBits);
                    WriteBits(startWord - wordOffset, offsetBits);
                    UnreadBits(wordLength);
                    for (i = 0; i < numWords; i++)
                    {
                        startValue = ReadBits(wordLength);
                        AddToHash(startWord, startValue & LZSS_HASH_MASK);
                        startWord++;
                    }
                }
                else
                {
                    WriteBits(0, lengthBits);
                    WriteBits(startValue, wordLength);
                    AddToHash(startWord, startValue & LZSS_HASH_MASK);
                    startWord++;
                }
            }

            blockSize = 0;
        }

        protected override void DecompressBlock()
        {
            int i, offset, startWord, numWords;

            InitDecompress(block, LZSS_BLOCK_SIZE);

            startWord = 0;
            while (writeByte < writeLength && readLength >= 0)
            {
                numWords = ReadBits(lengthBits);
                if (numWords != 0)
                {
                    numWords += (minMatchWords - 1);
                    offset = startWord - ReadBits(offsetBits);
                    for (i = 0; i < numWords; i++)
                    {
                        WriteBits(GetWordFromBlock(offset + i), wordLength);
                        startWord++;
                    }
                }
                else
                {
                    WriteBits(ReadBits(wordLength), wordLength);
                    startWord++;
                }
            }

            blockSize = Math.Min(writeByte, LZSS_BLOCK_SIZE);
        }
    }

    #endregion

    #region VCompressor_LZW

    /// <summary>
    /// This is the same compression scheme used by GIF with the exception that the EOI and clear codes are not explicitly stored.  Instead EOI happens
    /// when the input stream runs dry and CC happens when the table gets to big.
    ///
    /// This is a derivation of LZ78, but the dictionary starts with all single character values so only code words are output.  It is similar in theory
    /// to LZ77, but instead of using the previous X bytes as a lookup table, a table is built as the stream is read.  The	compressor and decompressor use the
    /// same formula, so the tables should be exactly alike.  The only catch is the decompressor is always one step behind the compressor and may get a code not
    /// yet in the table.  In this case, it is easy to determine what the next code is going to be (it will be the previous string plus the first byte of the
    /// previous string).
    ///
    /// The dictionary can be any size, but 12 bits seems to produce best results for most sample data.  The code size is variable.  It starts with the minimum
    /// number of bits required to store the dictionary and automatically increases as the dictionary gets bigger (it starts at 9 bits and grows to 10 bits when
    /// item 512 is added, 11 bits when 1024 is added, etc...) once the the dictionary is filled (4096 items for a 12 bit dictionary), the whole thing is cleared and
    /// the process starts over again.
    /// 
    /// The compressor increases the bit size after it adds the item, while the decompressor does before it adds the item.  The difference is subtle, but
    /// it's because decompressor being one step behind.  Otherwise, the decompressor would read 512 with only 9 bits.
    /// 
    /// If "Hello" is in the dictionary, then "Hell", "Hel", "He" and "H" will be too. We use this to our advantage by storing the index of the previous code, and
    /// the value of the last character.  This means when we traverse through the dictionary, we get the characters in reverse.
    /// 
    /// Dictionary entries 0-255 are always going to have the values 0-255
    /// 
    /// http://www.unisys.com/about__unisys/lzw
    /// http://www.dogma.net/markn/articles/lzw/lzw.htm
    /// http://www.cs.cf.ac.uk/Dave/Multimedia/node214.html
    /// http://www.cs.duke.edu/csed/curious/compression/lzw.html
    /// http://oldwww.rasip.fer.hr/research/compress/algorithms/fund/lz/lzw.html
    /// </summary>
    /// <seealso cref="Gengine.Library.Core.VCompressor_BitStream" />
    public class VCompressor_LZW : VCompressor_BitStream
    {
        const int LZW_BLOCK_SIZE = 32767;
        const int LZW_START_BITS = 9;
        const int LZW_FIRST_CODE = (1 << (LZW_START_BITS - 1));
        const int LZW_DICT_BITS = 12;
        const int LZW_DICT_SIZE = 1 << LZW_DICT_BITS;

        // Dictionary data
        (int k, int w)[] dictionary = new (int k, int w)[LZW_DICT_SIZE];
        HashIndex index;

        int nextCode;
        int codeBits;

        // Block data
        byte[] block = new byte[LZW_BLOCK_SIZE];
        int blockSize;
        int blockIndex;

        // Used by the compressor
        int w;

        // Used by the decompressor
        int oldCode;

        public override void Init(VFile f, bool compress, int wordLength)
        {
            base.Init(f, compress, wordLength);

            for (var i = 0; i < LZW_FIRST_CODE; i++)
            {
                dictionary[i].k = i;
                dictionary[i].w = -1;
            }
            index.Clear();

            nextCode = LZW_FIRST_CODE;
            codeBits = LZW_START_BITS;

            blockSize = 0;
            blockIndex = 0;

            w = -1;
            oldCode = -1;
        }

        public override int Read(byte[] outData, int outLength)
        {
            int i, n;

            if (compress || outLength <= 0) return 0;

            if (blockSize == 0) DecompressBlock();

            for (i = 0; i < outLength; i += n)
            {
                if (blockSize == 0) return i;
                n = blockSize - blockIndex;
                if (outLength - i >= n)
                {
                    Unsafe.CopyBlock(ref outData[i], ref block[blockIndex], (uint)n);
                    DecompressBlock();
                    blockIndex = 0;
                }
                else
                {
                    Unsafe.CopyBlock(ref outData[i], ref block[blockIndex], (uint)(outLength - i));
                    n = outLength - i;
                    blockIndex += n;
                }
            }

            return outLength;
        }

        protected int Lookup(int w, int k)
        {
            int j;

            if (w == -1) return k;
            else for (j = index.First(w ^ k); j >= 0; j = index.Next(j)) if (dictionary[j].k == k && dictionary[j].w == w) return j;

            return -1;
        }

        protected int AddToDict(int w, int k)
        {
            dictionary[nextCode].k = k;
            dictionary[nextCode].w = w;
            index.Add(w ^ k, nextCode);
            return nextCode++;
        }

        /// <summary>
        /// Possibly increments codeBits
        /// Returns true if the dictionary was cleared
        /// </summary>
        /// <returns></returns>
        protected bool BumpBits()
        {
            if (nextCode == (1 << codeBits))
            {
                codeBits++;
                if (codeBits > LZW_DICT_BITS)
                {
                    nextCode = LZW_FIRST_CODE;
                    codeBits = LZW_START_BITS;
                    index.Clear();
                    return true;
                }
            }
            return false;
        }

        public override void FinishCompress()
        {
            WriteBits(w, codeBits);
            base.FinishCompress();
        }

        public override int Write(byte[] inData, int inLength)
        {
            int i;

            InitCompress(inData, inLength);

            for (i = 0; i < inLength; i++)
            {
                var k = ReadBits(8);

                var code = Lookup(w, k);
                if (code >= 0) w = code;
                else
                {
                    WriteBits(w, codeBits);
                    if (!BumpBits()) AddToDict(w, k);
                    w = k;
                }
            }

            return inLength;
        }

        /// <summary>
        /// The chain is stored backwards, so we have to write it to a buffer then output the buffer in reverse
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        protected int WriteChain(int code)
        {
            var chain = new byte[LZW_DICT_SIZE];
            var i = 0;
            do
            {
                Debug.Assert(i < LZW_DICT_SIZE - 1 && code >= 0);
                chain[i++] = (byte)dictionary[code].k;
                code = dictionary[code].w;
            } while (code >= 0);
            var firstChar = chain[--i];
            for (; i >= 0; i--) WriteBits(chain[i], 8);
            return firstChar;
        }

        protected void DecompressBlock()
        {
            int code, firstChar;

            InitDecompress(block, LZW_BLOCK_SIZE);

            while (writeByte < writeLength - LZW_DICT_SIZE && readLength > 0)
            {
                Debug.Assert(codeBits <= LZW_DICT_BITS);

                code = ReadBits(codeBits);
                if (readLength == 0) break;

                if (oldCode == -1)
                {
                    Debug.Assert(code < 256);
                    WriteBits(code, 8);
                    oldCode = code;
                    firstChar = code;
                    continue;
                }

                if (code >= nextCode)
                {
                    Debug.Assert(code == nextCode);
                    firstChar = WriteChain(oldCode);
                    WriteBits(firstChar, 8);
                }
                else firstChar = WriteChain(code);
                AddToDict(oldCode, firstChar);
                oldCode = BumpBits() ? -1 : code;
            }

            blockSize = Math.Min(writeByte, LZW_BLOCK_SIZE);
        }
    }

    #endregion
}