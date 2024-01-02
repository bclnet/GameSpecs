using GameSpec.Formats;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Cig.Formats
{
    public class Binary_DdsA
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Dds(r, false));
    }
}
