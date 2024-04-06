using OpenStack.Graphics.Algorithms;
using System;

namespace OpenStack.Graphics.DirectX
{
    /// <summary>
    /// DirectXExtensions
    /// </summary>
    public static partial class DirectXExtensions
    {
        // https://github.com/apitrace/dxsdk/blob/master/Include/d3dx_dxgiformatconvert.inl
        public static float[] ReadVertex(this DXGI_FORMAT source, byte[] buffer, int offset)
        {
            float[] result;
            switch (source)
            {
                case DXGI_FORMAT.R32G32B32_FLOAT: { result = new float[3]; Buffer.BlockCopy(buffer, offset, result, 0, 12); break; }
                case DXGI_FORMAT.R32G32B32A32_FLOAT: { result = new float[4]; Buffer.BlockCopy(buffer, offset, result, 0, 16); break; }
                case DXGI_FORMAT.R16G16_UNORM: { var shorts = new ushort[2]; Buffer.BlockCopy(buffer, offset, shorts, 0, 4); result = new[] { shorts[0] / 65535f, shorts[1] / 65535f }; break; }
                case DXGI_FORMAT.R16G16_FLOAT: { var shorts = new ushort[2]; Buffer.BlockCopy(buffer, offset, shorts, 0, 4); result = new[] { HalfPrecConverter.ToSingle(shorts[0]), HalfPrecConverter.ToSingle(shorts[1]) }; break; }
                case DXGI_FORMAT.R32_FLOAT: { result = new float[1]; Buffer.BlockCopy(buffer, offset, result, 0, 4); break; }
                case DXGI_FORMAT.R32G32_FLOAT: { result = new float[2]; Buffer.BlockCopy(buffer, offset, result, 0, 8); break; }
                case DXGI_FORMAT.R16G16B16A16_SINT: { var shorts = new short[4]; Buffer.BlockCopy(buffer, offset, shorts, 0, 8); result = new float[4]; for (var i = 0; i < 4; i++) result[i] = shorts[i]; break; }
                case DXGI_FORMAT.R8G8B8A8_UINT:
                case DXGI_FORMAT.R8G8B8A8_UNORM: { var bytes = new byte[4]; Buffer.BlockCopy(buffer, offset, bytes, 0, 4); result = new float[4]; for (var i = 0; i < 4; i++) result[i] = source == DXGI_FORMAT.R8G8B8A8_UNORM ? bytes[i] / 255f : bytes[i]; break; }
                default: throw new ArgumentOutOfRangeException(nameof(source), source.ToString());
            }
            return result;
        }

    }
}