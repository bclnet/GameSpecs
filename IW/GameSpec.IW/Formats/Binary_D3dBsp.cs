using GameSpec.Formats;
using GameSpec.Meta;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.IW.Formats
{
    // https://github.com/SE2Dev/D3DBSP_Converter
    public class Binary_D3dBsp : IHaveMetaInfo
    {

        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => new List<MetaInfo> {
            new MetaInfo(null, new MetaContent { Type = "Model", Name = Path.GetFileName(file.Path), Value = this })
        };
    }
}
