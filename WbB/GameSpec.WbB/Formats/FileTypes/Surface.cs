using GameSpec.WbB.Formats.Props;
using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace GameSpec.WbB.Formats.FileTypes
{
    /// <summary>
    /// These are client_portal.dat files starting with 0x08.
    /// As the name implies this contains surface info for an object. Either texture reference or color and whatever effects applied to it.
    /// </summary>
    [PakFileType(PakFileType.Surface)]
    public class Surface : FileType, IGetMetadataInfo
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
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileSource file, object tag)
        {
            var hasSurface = Type.HasFlag(SurfaceType.Base1Image) || Type.HasFlag(SurfaceType.Base1ClipMap);
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"{nameof(Surface)}: {Id:X8}", items: new List<MetadataInfo> {
                    new MetadataInfo($"Type: {Type}"),
                    hasSurface ? new MetadataInfo($"Surface Texture: {OrigTextureId:X8}", clickable: true) : null,
                    hasSurface && OrigPaletteId != 0 ? new MetadataInfo($"Palette ID: {OrigPaletteId:X8}", clickable: true) : null,
                    !hasSurface ? new MetadataInfo($"Color: {ColorX.ToRGBA(ColorValue)}") : null,
                    /*Translucency != 0f ?*/ new MetadataInfo($"Translucency: {Translucency}") /*: null*/,
                    /*Luminosity != 0f ?*/ new MetadataInfo($"Luminosity: {Luminosity}") /*: null*/,
                    /*Diffuse != 1f ?*/ new MetadataInfo($"Diffuse: {Diffuse}") /*: null*/,
                })
            };
            return nodes;
        }
    }
}
