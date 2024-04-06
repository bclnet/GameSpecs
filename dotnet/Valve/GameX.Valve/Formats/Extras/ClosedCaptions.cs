using GameX.Algorithms;
using GameX.Meta;
using GameX.Formats;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GameX.Valve.Formats.Extras
{
    public class ClosedCaption
    {
        public uint Hash;
        public uint UnknownV2;
        public int Blocknum;
        public ushort Offset;
        public ushort Length;
        public string Text;
    }

    public class ClosedCaptions : IEnumerable<ClosedCaption>, IHaveMetaInfo
    {
        public const int MAGIC = 0x44434356; // "VCCD"

        public ClosedCaptions() { }
        public ClosedCaptions(BinaryReader r) => Read(r);

        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => new List<MetaInfo> {
            new MetaInfo(null, new MetaContent { Type = "DataGrid", Name = "Captions", Value = Captions }),
            new MetaInfo("ClosedCaptions", items: new List<MetaInfo> {
                new MetaInfo($"Count: {Captions.Count}"),
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
            if (version != 1 && version != 2) throw new InvalidDataException($"Unsupported VCCD version: {version}");

            // numblocks, not actually required for hash lookups or populating entire list
            r.ReadUInt32();
            var blocksize = r.ReadUInt32();
            var directorysize = r.ReadUInt32();
            var dataoffset = r.ReadUInt32();
            for (var i = 0U; i < directorysize; i++)
                Captions.Add(new ClosedCaption
                {
                    Hash = r.ReadUInt32(),
                    UnknownV2 = version >= 2 ? r.ReadUInt32() : 0,
                    Blocknum = r.ReadInt32(),
                    Offset = r.ReadUInt16(),
                    Length = r.ReadUInt16()
                });

            // Probably could be inside the for loop above, but I'm unsure what the performance costs are of moving the position head manually a bunch compared to reading sequentually
            foreach (var caption in Captions)
            {
                r.Seek(dataoffset + caption.Blocknum * blocksize + caption.Offset);
                caption.Text = r.ReadZEncoding(Encoding.Unicode);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<ClosedCaption>)Captions).GetEnumerator();
    }
}
