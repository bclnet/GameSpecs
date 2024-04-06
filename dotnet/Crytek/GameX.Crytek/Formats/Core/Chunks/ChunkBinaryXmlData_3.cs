using System.IO;
using System.Xml;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public class ChunkBinaryXmlData_3 : ChunkBinaryXmlData
    {
        public XmlDocument Data;

        public override void Read(BinaryReader r)
        {
            base.Read(r);

            //var bytesToRead = (int)(Size - Math.Max(r.BaseStream.Position - Offset, 0));
            //var buffer = r.ReadBytes(bytesToRead);
            //using var memoryStream = new MemoryStream(buffer);
            //Data = new CryXmlFile(memoryStream);
            Data = new CryXmlFile(r, true);
        }
    }
}