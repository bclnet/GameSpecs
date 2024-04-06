using System.IO;

namespace GameX.Valve.Formats.Blocks
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
