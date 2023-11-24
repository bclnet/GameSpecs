using GameSpec.Formats;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Cig.Formats
{
    public class BinaryDdsA
    {
        public static Task<object> Factory(BinaryReader r, FileMetadata f, PakFile s) => Task.FromResult((object)new BinaryDds(r, false));
    }
}
