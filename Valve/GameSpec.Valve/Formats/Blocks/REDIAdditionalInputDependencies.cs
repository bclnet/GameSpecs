using System.IO;

namespace GameSpec.Valve.Formats.Blocks
{
    public class REDIAdditionalInputDependencies : REDIInputDependencies
    {
        public override void WriteText(IndentedTextWriter w)
        {
            w.WriteLine($"Struct m_AdditionalInputDependencies[{List.Count}] = [");
            WriteList(w);
        }
    }
}
