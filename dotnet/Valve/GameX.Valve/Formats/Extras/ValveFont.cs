using System.IO;
using System.Text;

namespace GameX.Valve.Formats.Extras
{
    public class ValveFont
    {
        const string MAGIC = "VFONT1";
        const byte MAGICTRICK = 167;

        public byte[] Read(BinaryReader r)
        {
            // Magic is at the end
            r.BaseStream.Seek(-MAGIC.Length, SeekOrigin.End);
            if (Encoding.ASCII.GetString(r.ReadBytes(MAGIC.Length)) != MAGIC) throw new InvalidDataException("Given file is not a vfont, version 1.");
            r.End(-1 - MAGIC.Length);

            // How many magic bytes there are
            var bytes = r.ReadByte();
            var output = new byte[r.BaseStream.Length - MAGIC.Length - bytes];
            var magic = (int)MAGICTRICK;

            // Read the magic bytes
            r.Skip(-bytes);
            bytes--;
            for (var i = 0; i < bytes; i++) magic ^= (r.ReadByte() + MAGICTRICK) % 256;

            // Decode the rest
            r.Seek(0);
            for (var i = 0; i < output.Length; i++)
            {
                var currentByte = r.ReadByte();
                output[i] = (byte)(currentByte ^ magic);
                magic = (currentByte + MAGICTRICK) % 256;
            }
            return output;
        }
    }
}
