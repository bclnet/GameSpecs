using System;
using System.IO;

namespace GameX.Valve.Formats.Blocks
{
    /// <summary>
    /// "SNAP" block.
    /// </summary>
    //was:Resource/Blocks/SNAP
    public class SNAP : Block
    {
        public override void Read(Binary_Pak parent, BinaryReader r)
        {
            r.Seek(Offset);
            throw new NotImplementedException();
        }
    }
}
