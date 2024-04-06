using GameX.Formats;
using GameX.Meta;
using System.Collections.Generic;
using System.IO;

namespace GameX.IW.Formats
{
    // https://github.com/SE2Dev/gsc_parser
    public class Binary_Gsc : IHaveMetaInfo
    {
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => new List<MetaInfo> {
            new MetaInfo(null, new MetaContent { Type = "GSC", Name = Path.GetFileName(file.Path), Value = this })
        };
    }
}
