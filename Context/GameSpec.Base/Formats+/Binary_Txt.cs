using GameSpec.Metadata;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Formats
{
    public class Binary_Txt : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Txt(r, (int)f.FileSize));

        public Binary_Txt() { }
        public Binary_Txt(BinaryReader r, int fileSize) => Data = r.ReadEncoding(fileSize);

        public string Data;

        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => new List<MetaInfo> {
            new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = Data }),
        };
    }
}
