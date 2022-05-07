using GameSpec.Algorithms;
using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GameSpec.Valve.Formats
{
    public class ClosedCaption
    {
        public uint Hash { get; set; }
        public uint UnknownV2 { get; set; }
        public int Blocknum { get; set; }
        public ushort Offset { get; set; }
        public ushort Length { get; set; }
        public string Text { get; set; }
    }

    public class ClosedCaptions : IEnumerable<ClosedCaption>, IGetExplorerInfo
    {
        public const int MAGIC = 0x44434356; // "VCCD"

        public ClosedCaptions() { }
        public ClosedCaptions(BinaryReader r) => Read(r);

        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag) => new List<ExplorerInfoNode> {
            new ExplorerInfoNode(null, new ExplorerContentTab { Type = "DataGrid", Name = "Captions", Value = Captions }),
            new ExplorerInfoNode("ClosedCaptions", items: new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"Count: {Captions.Count}"),
            }),
        };

        public List<ClosedCaption> Captions { get; private set; }

        public IEnumerator<ClosedCaption> GetEnumerator() => ((IEnumerable<ClosedCaption>)Captions).GetEnumerator();

        public ClosedCaption this[string key]
        {
            get
            {
                var hash = Crc32Digest.Compute(Encoding.UTF8.GetBytes(key));
                return Captions.Find(caption => caption.Hash == hash);
            }
        }

        public void Read(BinaryReader r)
        {
            Captions = new List<ClosedCaption>();
            if (r.ReadUInt32() != MAGIC) throw new InvalidDataException("Given file is not a VCCD.");

            var version = r.ReadUInt32();
            if (version != 1 && version != 2) throw new InvalidDataException("Unsupported VCCD version: " + version);

            // numblocks, not actually required for hash lookups or populating entire list
            r.ReadUInt32();
            var blocksize = r.ReadUInt32();
            var directorysize = r.ReadUInt32();
            var dataoffset = r.ReadUInt32();
            for (var i = 0U; i < directorysize; i++)
            {
                var caption = new ClosedCaption();
                caption.Hash = r.ReadUInt32();
                if (version >= 2) caption.UnknownV2 = r.ReadUInt32();
                caption.Blocknum = r.ReadInt32();
                caption.Offset = r.ReadUInt16();
                caption.Length = r.ReadUInt16();
                Captions.Add(caption);
            }

            // Probably could be inside the for loop above, but I'm unsure what the performance costs are of moving the position head manually a bunch compared to reading sequentually
            foreach (var caption in Captions)
            {
                r.Position(dataoffset + (caption.Blocknum * blocksize) + caption.Offset);
                caption.Text = r.ReadZEncoding(Encoding.Unicode);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<ClosedCaption>)Captions).GetEnumerator();
    }
}
