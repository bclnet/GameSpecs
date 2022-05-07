using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GameSpec.AC.Formats.FileTypes
{
    /// <summary>
    /// These are client_portal.dat files starting with 0x31.
    /// This is called a "String" in the client; It has been renamed to avoid conflicts with the generic "String" class.
    /// </summary>
    [PakFileType(PakFileType.String)]
    public class LanguageString : FileType, IGetExplorerInfo
    {
        public string CharBuffer;

        public LanguageString(BinaryReader r)
        {
            Id = r.ReadUInt32();
            CharBuffer = r.ReadC32String(Encoding.Default); //:TODO ?FALLBACK
        }

        //: New
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"{nameof(LanguageString)}: {Id:X8}", items: new List<ExplorerInfoNode> {
                })
            };
            return nodes;
        }
    }
}
