using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GameX.WbB.Formats.FileTypes
{
    /// <summary>
    /// These are client_portal.dat files starting with 0x31.
    /// This is called a "String" in the client; It has been renamed to avoid conflicts with the generic "String" class.
    /// </summary>
    [PakFileType(PakFileType.String)]
    public class LanguageString : FileType, IHaveMetaInfo
    {
        public string CharBuffer;

        public LanguageString(BinaryReader r)
        {
            Id = r.ReadUInt32();
            CharBuffer = r.ReadC32Encoding(Encoding.Default); //:TODO ?FALLBACK
        }

        //: New
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"{nameof(LanguageString)}: {Id:X8}", items: new List<MetaInfo> {
                })
            };
            return nodes;
        }
    }
}
