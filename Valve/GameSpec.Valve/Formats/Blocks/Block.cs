using System;
using System.IO;

namespace GameSpec.Valve.Formats.Blocks
{
    /// <summary>
    /// Represents a block within the resource file.
    /// </summary>
    public abstract class Block //: was:Block.cs
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

        //: was:Resource.ConstructFromType()
        public static Block Factory(BinaryPak source, string value)
            => value switch
            {
                "DATA" => DATA.Factory(source),
                "REDI" => new REDI(),
                //"RED2" => new RED2(),
                "RERL" => new RERL(),
                "NTRO" => new NTRO(),
                "VBIB" => new VBIB(),
                "VXVS" => new VXVS(),
                "SNAP" => new SNAP(),
                "MBUF" => new MBUF(),
                "CTRL" => new CTRL(),
                "MDAT" => new MDAT(),
                "INSG" => new INSG(),
                "SrMa" => new SRMA(),
                "LaCo" => new LACO(),
                "MRPH" => new MRPH(),
                "ANIM" => new ANIM(),
                "ASEQ" => new ASEQ(),
                "AGRP" => new AGRP(),
                "PHYS" => new PHYS(),
                _ => throw new ArgumentOutOfRangeException(nameof(value), $"Unrecognized block type '{value}'"),
            };
    }
}
