using System;
using System.IO;

namespace GameSpec.Valve.Formats.Blocks
{
    public abstract class Block
    {
        /// <summary>
        /// Gets or sets the offset to the data.
        /// </summary>
        public uint Offset { get; set; }

        /// <summary>
        /// Gets or sets the data size.
        /// </summary>
        public uint Size { get; set; }

        public abstract void Read(BinaryPak parent, BinaryReader r);

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            using var w = new IndentedTextWriter();
            WriteText(w);
            return w.ToString();
        }

        /// <summary>
        /// Writers the correct object to IndentedTextWriter.
        /// </summary>
        /// <param name="w">IndentedTextWriter.</param>
        public virtual void WriteText(IndentedTextWriter w)
            => w.WriteLine("{0:X8}", Offset);

        public static Block Factory(BinaryPak source, string value)
            => value switch
            {
                "DATA" => DATA.Factory(source),
                "REDI" => new REDI(),
                "RERL" => new RERL(),
                "NTRO" => new NTRO(),
                "VBIB" => new VBIB(),
                "VXVS" => new VXVS(),
                "SNAP" => new SNAP(),
                "MBUF" => new MBUF(),
                "CTRL" => new CTRL(),
                "MDAT" => new MDAT(),
                "MRPH" => new MRPH(),
                "ANIM" => new ANIM(),
                "ASEQ" => new ASEQ(),
                "AGRP" => new AGRP(),
                "PHYS" => new PHYS(),
                _ => throw new ArgumentOutOfRangeException(nameof(value), $"Unrecognized block type '{value}'"),
            };
    }
}
