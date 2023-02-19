using System;
using System.IO;

namespace GameSpec.Valve.Formats.Blocks
{
    /// <summary>
    /// "VXVS" block.
    /// </summary>
    public class VXVS : Block
    {
        public override void Read(BinaryPak parent, BinaryReader r)
        {
            r.Seek(Offset);
            throw new NotImplementedException();
        }
    }
}
