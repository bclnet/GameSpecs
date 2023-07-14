using GameSpec.Formats;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Rsi.Formats
{
    public class BinaryDdsA
    {
        public static Task<object> Factory(BinaryReader r, FileMetadata f, PakFile s) => Task.FromResult((object)new BinaryDds(r, false));
    }
}
