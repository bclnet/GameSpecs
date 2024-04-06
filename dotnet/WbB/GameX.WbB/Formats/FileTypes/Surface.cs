using GameX.WbB.Formats.Props;
using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace GameX.WbB.Formats.FileTypes
{
    /// <summary>
    /// These are client_portal.dat files starting with 0x08.
    /// As the name implies this contains surface info for an object. Either texture reference or color and whatever effects applied to it.
    /// </summary>
    [PakFileType(PakFileType.Surface)]
    public class Surface : FileType, IHaveMetaInfo
    {
        public readonly SurfaceType Type;
        public readonly uint OrigTextureId;
        public readonly uint OrigPaletteId;
        public readonly uint ColorValue;
        public readonly float Translucency;
        public readonly float Luminosity;
        public readonly float Diffuse;

        public Surface(BinaryReader r)
        {
            Type = (SurfaceType)r.ReadUInt32();
            if (Type.HasFlag(SurfaceType.Base1Image) || Type.HasFlag(SurfaceType.Base1ClipMap)) { OrigTextureId = r.ReadUInt32(); OrigPaletteId = r.ReadUInt32(); } // image or clipmap
            else ColorValue = r.ReadUInt32(); // solid color
            Translucency = r.ReadSingle();
            Luminosity = r.ReadSingle();
            Diffuse = r.ReadSingle();
        }

        //: FileTypes.Surface
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var hasSurface = Type.HasFlag(SurfaceType.Base1Image) || Type.HasFlag(SurfaceType.Base1ClipMap);
            var nodes = new List<MetaInfo> {
                new MetaInfo($"{nameof(Surface)}: {Id:X8}", items: new List<MetaInfo> {
                    new MetaInfo($"Type: {Type}"),
                    hasSurface ? new MetaInfo($"Surface Texture: {OrigTextureId:X8}", clickable: true) : null,
                    hasSurface && OrigPaletteId != 0 ? new MetaInfo($"Palette ID: {OrigPaletteId:X8}", clickable: true) : null,
                    !hasSurface ? new MetaInfo($"Color: {ColorX.ToRGBA(ColorValue)}") : null,
                    /*Translucency != 0f ?*/ new MetaInfo($"Translucency: {Translucency}") /*: null*/,
                    /*Luminosity != 0f ?*/ new MetaInfo($"Luminosity: {Luminosity}") /*: null*/,
                    /*Diffuse != 1f ?*/ new MetaInfo($"Diffuse: {Diffuse}") /*: null*/,
                })
            };
            return nodes;
        }
    }
}
