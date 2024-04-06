using GameX.Formats;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Cig.Formats
{
    public class Binary_DdsA
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Dds(r, false));
    }
}
