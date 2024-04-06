using System;
using System.Runtime.InteropServices;

// https://docs.microsoft.com/en-us/windows/win32/direct3ddds/dds-header-dxt10
// https://docs.microsoft.com/en-us/windows/win32/api/d3d10/ne-d3d10-d3d10_resource_dimension
namespace OpenStack.Graphics.DirectX
{
    public enum DDS_ALPHA_MODE : uint
    {
        ALPHA_MODE_UNKNOWN = 0,
        ALPHA_MODE_STRAIGHT = 1,
        ALPHA_MODE_PREMULTIPLIED = 2,
        ALPHA_MODE_OPAQUE = 3,
        ALPHA_MODE_CUSTOM = 4,
    }

    [Flags]
    public enum D3D10_RESOURCE_DIMENSION : uint
    {
        /// <summary>
        /// Resource is of unknown type.
        /// </summary>
        UNKNOWN = 0,
        /// <summary>
        /// Resource is a buffer.
        /// </summary>
        BUFFER = 1,
        /// <summary>
        /// Resource is a 1D texture. The dwWidth member of DDS_HEADER specifies the size of the texture. Typically, you set the dwHeight member of DDS_HEADER to 1; you also must set the DDSD_HEIGHT flag in the dwFlags member of DDS_HEADER.
        /// </summary>
        TEXTURE1D = 2,
        /// <summary>
        /// Resource is a 2D texture with an area specified by the dwWidth and dwHeight members of DDS_HEADER. You can also use this type to identify a cube-map texture. For more information about how to identify a cube-map texture, see miscFlag and arraySize members.
        /// </summary>
        TEXTURE2D = 3,
        /// <summary>
        /// Resource is a 3D texture with a volume specified by the dwWidth, dwHeight, and dwDepth members of DDS_HEADER. You also must set the DDSD_DEPTH flag in the dwFlags member of DDS_HEADER.
        /// </summary>
        TEXTURE3D = 4,
    }

    /// <summary>
    /// DDS header extension to handle resource arrays, DXGI pixel formats that don't map to the legacy Microsoft DirectDraw pixel format structures, and additional metadata.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
    public struct DDS_HEADER_DXT10
    {
        /// <summary>
        /// Struct
        /// </summary>
        public static (string, int) Struct = ($"<5I", 20);

        /// <summary>
        /// The surface pixel format (see DXGI_FORMAT).
        /// </summary>
        [MarshalAs(UnmanagedType.I4)] public DXGI_FORMAT dxgiFormat;
        /// <summary>
        /// Identifies the type of resource. The following values for this member are a subset of the values in the D3D10_RESOURCE_DIMENSION or D3D11_RESOURCE_DIMENSION enumeration:
        /// </summary>
        [MarshalAs(UnmanagedType.U4)] public D3D10_RESOURCE_DIMENSION resourceDimension;
        /// <summary>
        /// Identifies other, less common options for resources. The following value for this member is a subset of the values in the D3D10_RESOURCE_MISC_FLAG or D3D11_RESOURCE_MISC_FLAG enumeration:
        /// </summary>
        public uint miscFlag;
        /// <summary>
        /// The number of elements in the array.
        /// </summary>
        public uint arraySize;
        /// <summary>
        /// Contains additional metadata (formerly was reserved). The lower 3 bits indicate the alpha mode of the associated resource. The upper 29 bits are reserved and are typically 0.
        /// </summary>
        public uint miscFlags2; // see DDS_MISC_FLAGS2
    }
}