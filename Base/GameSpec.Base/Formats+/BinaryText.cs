using GameSpec.Explorer;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Formats
{
    public class BinaryText : IGetExplorerInfo
    {
        public static Task<object> Factory(BinaryReader r, FileMetadata f, PakFile s) => Task.FromResult((object)new BinaryText(r, (int)f.FileSize));

        public BinaryText() { }
        public BinaryText(BinaryReader r, int fileSize) => Data = r.ReadStringAsBytes(fileSize);

        public string Data;

        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag) => new List<ExplorerInfoNode> {
            new ExplorerInfoNode(null, new ExplorerContentTab { Type = "Text", Name = Path.GetFileName(file.Path), Value = Data }),
        };
    }
}
