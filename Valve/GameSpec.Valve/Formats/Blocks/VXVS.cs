using System;
using System.IO;

namespace GameSpec.Valve.Formats.Blocks
{
    /// <summary>
    /// "VXVS" block.
    /// </summary>
    //was:Resource/Blocks/VXVS
    public class VXVS : Block
    {
        public override void Read(BinaryPak parent, BinaryReader r)
        {
            r.Seek(Offset);
            throw new NotImplementedException();
        }

        public override void WriteText(IndentedTextWriter w)
            => w.WriteLine("{0:X8}", Offset);
    }
}
