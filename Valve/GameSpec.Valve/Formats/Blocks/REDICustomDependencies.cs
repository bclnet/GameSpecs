using System;
using System.IO;

namespace GameSpec.Valve.Formats.Blocks
{
    public class REDICustomDependencies : REDIAbstract
    {
        public override void Read(BinaryPak parent, BinaryReader r)
        {
            r.Position(Offset);
            if (Size > 0) throw new NotImplementedException("CustomDependencies block is not handled.");
        }

        public override void WriteText(IndentedTextWriter w)
        {
            w.WriteLine($"Struct m_CustomDependencies[{0}] = ["); w.Indent++;
            w.Indent--; w.WriteLine("]");
        }
    }
}
