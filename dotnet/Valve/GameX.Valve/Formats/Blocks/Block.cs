using System.IO;

namespace GameX.Valve.Formats.Blocks
{
    /// <summary>
    /// Represents a block within the resource file.
    /// </summary>
    //was:Resource/Block
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

        public abstract void Read(Binary_Pak parent, BinaryReader r);

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
        public virtual void WriteText(IndentedTextWriter w) => w.WriteLine("{0:X8}", Offset);
    }
}
