using System;
using System.IO;

namespace GameSpec.Valve.Formats.Blocks
{
    /// <summary>
    /// "SNAP" block.
    /// </summary>
    public class SNAP : Block
    {
        public override void Read(BinaryPak parent, BinaryReader r)
        {
            r.Position(Offset);
            throw new NotImplementedException();
        }
    }
}
