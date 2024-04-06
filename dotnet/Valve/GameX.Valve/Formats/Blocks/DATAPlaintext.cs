using System.IO;
using System.Text;

namespace GameX.Valve.Formats.Blocks
{
    //was:Resource/ResourceTypes/Plaintext
    public class DATAPlaintext : DATABinaryNTRO
    {
        public string Data { get; private set; }

        public override void Read(Binary_Pak parent, BinaryReader r)
        {
            r.Seek(Offset);
            Data = Encoding.UTF8.GetString(r.ReadBytes((int)Size));
        }

        public override string ToString() => Data;
    }
}
