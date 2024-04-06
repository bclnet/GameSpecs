using OpenStack.Graphics.Algorithms;
using OpenStack.Graphics.DirectX;
using OpenStack.Graphics.Renderer1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static OpenStack.Graphics.Renderer1.OnDiskBufferData;

namespace GameX.Valve.Formats.Blocks
{
    /// <summary>
    /// "VBIB" block.
    /// </summary>
    //was:Resource/Blocks/VBIB
    public class VBIB : Block, IVBIB
    {
        public List<OnDiskBufferData> VertexBuffers { get; }
        public List<OnDiskBufferData> IndexBuffers { get; }

        public VBIB()
        {
            VertexBuffers = new List<OnDiskBufferData>();
            IndexBuffers = new List<OnDiskBufferData>();
        }

        public VBIB(IDictionary<string, object> data) : this()
        {
            var vertexBuffers = data.GetArray("m_vertexBuffers");
            foreach (var vb in vertexBuffers)
            {
                var vertexBuffer = BufferDataFromDATA(vb);
                var decompressedSize = vertexBuffer.ElementCount * vertexBuffer.ElementSizeInBytes;
                if (vertexBuffer.Data.Length != decompressedSize) vertexBuffer.Data = MeshOptimizerVertexDecoder.DecodeVertexBuffer((int)vertexBuffer.ElementCount, (int)vertexBuffer.ElementSizeInBytes, vertexBuffer.Data);
                VertexBuffers.Add(vertexBuffer);
            }
            var indexBuffers = data.GetArray("m_indexBuffers");
            foreach (var ib in indexBuffers)
            {
                var indexBuffer = BufferDataFromDATA(ib);
                var decompressedSize = indexBuffer.ElementCount * indexBuffer.ElementSizeInBytes;
                if (indexBuffer.Data.Length != decompressedSize) indexBuffer.Data = MeshOptimizerIndexDecoder.DecodeIndexBuffer((int)indexBuffer.ElementCount, (int)indexBuffer.ElementSizeInBytes, indexBuffer.Data);
                IndexBuffers.Add(indexBuffer);
            }
        }

        public override void Read(Binary_Pak parent, BinaryReader r)
        {
            r.Seek(Offset);
            var vertexBufferOffset = r.ReadUInt32();
            var vertexBufferCount = r.ReadUInt32();
            var indexBufferOffset = r.ReadUInt32();
            var indexBufferCount = r.ReadUInt32();

            r.Seek(Offset + vertexBufferOffset);
            for (var i = 0; i < vertexBufferCount; i++)
            {
                var vertexBuffer = ReadOnDiskBufferData(r);
                var decompressedSize = vertexBuffer.ElementCount * vertexBuffer.ElementSizeInBytes;
                if (vertexBuffer.Data.Length != decompressedSize) vertexBuffer.Data = MeshOptimizerVertexDecoder.DecodeVertexBuffer((int)vertexBuffer.ElementCount, (int)vertexBuffer.ElementSizeInBytes, vertexBuffer.Data);
                VertexBuffers.Add(vertexBuffer);
            }

            r.Seek(Offset + 8 + indexBufferOffset); // 8 to take into account vertexOffset / count
            for (var i = 0; i < indexBufferCount; i++)
            {
                var indexBuffer = ReadOnDiskBufferData(r);
                var decompressedSize = indexBuffer.ElementCount * indexBuffer.ElementSizeInBytes;
                if (indexBuffer.Data.Length != decompressedSize) indexBuffer.Data = MeshOptimizerIndexDecoder.DecodeIndexBuffer((int)indexBuffer.ElementCount, (int)indexBuffer.ElementSizeInBytes, indexBuffer.Data);
                IndexBuffers.Add(indexBuffer);
            }
        }

        static OnDiskBufferData ReadOnDiskBufferData(BinaryReader r)
        {
            var buffer = default(OnDiskBufferData);

            buffer.ElementCount = r.ReadUInt32();            //0
            buffer.ElementSizeInBytes = r.ReadUInt32();      //4

            var refA = r.BaseStream.Position;
            var attributeOffset = r.ReadUInt32();  //8
            var attributeCount = r.ReadUInt32();   //12

            var refB = r.BaseStream.Position;
            var dataOffset = r.ReadUInt32();       //16
            var totalSize = r.ReadInt32();        //20

            r.Seek(refA + attributeOffset);
            buffer.Attributes = Enumerable.Range(0, (int)attributeCount)
                .Select(j =>
                {
                    var attribute = default(OnDiskBufferData.Attribute);
                    var previousPosition = r.BaseStream.Position;
                    attribute.SemanticName = r.ReadZUTF8().ToUpperInvariant(); //32 bytes long null-terminated string
                    r.BaseStream.Position = previousPosition + 32; // Offset is always 40 bytes from the start
                    attribute.SemanticIndex = r.ReadInt32();
                    attribute.Format = (DXGI_FORMAT)r.ReadUInt32();
                    attribute.Offset = r.ReadUInt32();
                    attribute.Slot = r.ReadInt32();
                    attribute.SlotType = (OnDiskBufferData.RenderSlotType)r.ReadUInt32();
                    attribute.InstanceStepRate = r.ReadInt32();
                    return attribute;
                }).ToArray();

            r.Seek(refB + dataOffset);
            buffer.Data = r.ReadBytes(totalSize); //can be compressed

            r.Seek(refB + 8); //Go back to the index array to read the next iteration.
            return buffer;
        }

        static OnDiskBufferData BufferDataFromDATA(IDictionary<string, object> data)
        {
            var buffer = new OnDiskBufferData
            {
                ElementCount = data.GetUInt32("m_nElementCount"),
                ElementSizeInBytes = data.GetUInt32("m_nElementSizeInBytes"),
            };

            var inputLayoutFields = data.GetArray("m_inputLayoutFields");
            buffer.Attributes = inputLayoutFields.Select(il => new OnDiskBufferData.Attribute
            {
                //null-terminated string
                SemanticName = Encoding.UTF8.GetString(il.Get<byte[]>("m_pSemanticName")).TrimEnd((char)0),
                SemanticIndex = il.GetInt32("m_nSemanticIndex"),
                Format = (DXGI_FORMAT)il.GetUInt32("m_Format"),
                Offset = il.GetUInt32("m_nOffset"),
                Slot = il.GetInt32("m_nSlot"),
                SlotType = (RenderSlotType)il.GetUInt32("m_nSlotType"),
                InstanceStepRate = il.GetInt32("m_nInstanceStepRate")
            }).ToArray();

            buffer.Data = data.Get<byte[]>("m_pData");
            return buffer;
        }

        public static float[] ReadVertexAttribute(int offset, OnDiskBufferData vertexBuffer, OnDiskBufferData.Attribute attribute)
        {
            offset = (int)(offset * vertexBuffer.ElementSizeInBytes) + (int)attribute.Offset;
            // Useful reference: https://github.com/apitrace/dxsdk/blob/master/Include/d3dx_dxgiformatconvert.inl
            float[] result;
            switch (attribute.Format)
            {
                case DXGI_FORMAT.R32G32B32_FLOAT:
                    {
                        result = new float[3];
                        Buffer.BlockCopy(vertexBuffer.Data, offset, result, 0, 12);
                        return result;
                    }
                case DXGI_FORMAT.R32G32B32A32_FLOAT:
                    {
                        result = new float[4];
                        Buffer.BlockCopy(vertexBuffer.Data, offset, result, 0, 16);
                        return result;
                    }
                case DXGI_FORMAT.R16G16_UNORM:
                    {
                        var shorts = new ushort[2];
                        Buffer.BlockCopy(vertexBuffer.Data, offset, shorts, 0, 4);
                        result = new[] { (float)shorts[0] / ushort.MaxValue, (float)shorts[1] / ushort.MaxValue };
                        return result;
                    }
                case DXGI_FORMAT.R16G16_SNORM:
                    {
                        var shorts = new short[2];
                        Buffer.BlockCopy(vertexBuffer.Data, offset, shorts, 0, 4);
                        result = new[] { (float)shorts[0] / short.MaxValue, (float)shorts[1] / short.MaxValue };
                        return result;
                    }
                case DXGI_FORMAT.R16G16_FLOAT:
                    {
                        result = new[] { (float)BitConverterX.ToHalf(vertexBuffer.Data, offset), (float)BitConverterX.ToHalf(vertexBuffer.Data, offset + 2) };
                        return result;
                    }
                case DXGI_FORMAT.R32_FLOAT:
                    {
                        result = new float[1];
                        Buffer.BlockCopy(vertexBuffer.Data, offset, result, 0, 4);
                        return result;
                    }
                case DXGI_FORMAT.R32G32_FLOAT:
                    {
                        result = new float[2];
                        Buffer.BlockCopy(vertexBuffer.Data, offset, result, 0, 8);
                        return result;
                    }
                case DXGI_FORMAT.R16G16_SINT:
                    {
                        var shorts = new short[2];
                        Buffer.BlockCopy(vertexBuffer.Data, offset, shorts, 0, 4);
                        result = new float[2];
                        for (var i = 0; i < 2; i++) result[i] = shorts[i];
                        return result;
                    }
                case DXGI_FORMAT.R16G16B16A16_SINT:
                    {
                        var shorts = new short[4];
                        Buffer.BlockCopy(vertexBuffer.Data, offset, shorts, 0, 8);
                        result = new float[4];
                        for (var i = 0; i < 4; i++) result[i] = shorts[i];
                        return result;
                    }
                case DXGI_FORMAT.R8G8B8A8_UINT:
                case DXGI_FORMAT.R8G8B8A8_UNORM:
                    {
                        var bytes = new byte[4];
                        Buffer.BlockCopy(vertexBuffer.Data, offset, bytes, 0, 4);
                        result = new float[4];
                        for (var i = 0; i < 4; i++) result[i] = attribute.Format == DXGI_FORMAT.R8G8B8A8_UNORM ? (float)bytes[i] / byte.MaxValue : bytes[i];
                        return result;
                    }
                default: throw new NotImplementedException($"Unsupported \"{attribute.SemanticName}\" DXGI_FORMAT.{attribute.Format}");
            }
        }

        public override void WriteText(IndentedTextWriter w)
        {
            w.WriteLine("Vertex buffers:");
            foreach (var vertexBuffer in VertexBuffers)
            {
                w.WriteLine($"Count: {vertexBuffer.ElementCount}");
                w.WriteLine($"Size: {vertexBuffer.ElementSizeInBytes}");
                for (var i = 0; i < vertexBuffer.Attributes.Length; i++)
                {
                    var vertexAttribute = vertexBuffer.Attributes[i];
                    w.WriteLine($"Attribute[{i}]"); w.Indent++;
                    w.WriteLine($"SemanticName = {vertexAttribute.SemanticName}");
                    w.WriteLine($"SemanticIndex = {vertexAttribute.SemanticIndex}");
                    w.WriteLine($"Offset = {vertexAttribute.Offset}");
                    w.WriteLine($"Format = {vertexAttribute.Format}");
                    w.WriteLine($"Slot = {vertexAttribute.Slot}");
                    w.WriteLine($"SlotType = {vertexAttribute.SlotType}");
                    w.WriteLine($"InstanceStepRate = {vertexAttribute.InstanceStepRate}"); w.Indent--;
                }
                w.WriteLine();
            }
            w.WriteLine();
            w.WriteLine("Index buffers:");
            foreach (var indexBuffer in IndexBuffers)
            {
                w.WriteLine($"Count: {indexBuffer.ElementCount}");
                w.WriteLine($"Size: {indexBuffer.ElementSizeInBytes}");
                w.WriteLine();
            }
        }

        static (int ElementSize, int ElementCount) GetFormatInfo(OnDiskBufferData.Attribute attribute)
            => attribute.Format switch
            {
                DXGI_FORMAT.R32G32B32_FLOAT => (4, 3),
                DXGI_FORMAT.R32G32B32A32_FLOAT => (4, 4),
                DXGI_FORMAT.R16G16_UNORM => (2, 2),
                DXGI_FORMAT.R16G16_SNORM => (2, 2),
                DXGI_FORMAT.R16G16_FLOAT => (2, 2),
                DXGI_FORMAT.R32_FLOAT => (4, 1),
                DXGI_FORMAT.R32G32_FLOAT => (4, 2),
                DXGI_FORMAT.R16G16_SINT => (2, 2),
                DXGI_FORMAT.R16G16B16A16_SINT => (2, 4),
                DXGI_FORMAT.R8G8B8A8_UINT => (1, 4),
                DXGI_FORMAT.R8G8B8A8_UNORM => (1, 4),
                _ => throw new NotImplementedException($"Unsupported \"{attribute.SemanticName}\" DXGI_FORMAT.{attribute.Format}"),
            };

        public static int[] CombineRemapTables(int[][] remapTables)
        {
            remapTables = remapTables.Where(remapTable => remapTable.Length != 0).ToArray();
            var newRemapTable = remapTables[0].AsEnumerable();
            for (var i = 1; i < remapTables.Length; i++)
            {
                var remapTable = remapTables[i];
                newRemapTable = newRemapTable.Select(j => j != -1 ? remapTable[j] : -1);
            }
            return newRemapTable.ToArray();
        }

        public IVBIB RemapBoneIndices(int[] remapTable)
        {
            var res = new VBIB();
            res.VertexBuffers.AddRange(VertexBuffers.Select(buf =>
            {
                var blendIndices = Array.FindIndex(buf.Attributes, field => field.SemanticName == "BLENDINDICES");
                if (blendIndices != -1)
                {
                    var field = buf.Attributes[blendIndices];
                    var (formatElementSize, formatElementCount) = GetFormatInfo(field);
                    var formatSize = formatElementSize * formatElementCount;
                    buf.Data = buf.Data.ToArray();
                    var bufSpan = buf.Data.AsSpan();
                    for (var i = (int)field.Offset; i < buf.Data.Length; i += (int)buf.ElementSizeInBytes)
                        for (var j = 0; j < formatSize; j += formatElementSize)
                        {
                            switch (formatElementSize)
                            {
                                case 4:
                                    BitConverter.TryWriteBytes(bufSpan.Slice(i + j), remapTable[BitConverter.ToUInt32(buf.Data, i + j)]);
                                    break;
                                case 2:
                                    BitConverter.TryWriteBytes(bufSpan.Slice(i + j), (short)remapTable[BitConverter.ToUInt16(buf.Data, i + j)]);
                                    break;
                                case 1:
                                    buf.Data[i + j] = (byte)remapTable[buf.Data[i + j]];
                                    break;
                                default: throw new NotImplementedException();
                            }
                        }
                }
                return buf;
            }));
            res.IndexBuffers.AddRange(IndexBuffers);
            return res;
        }
    }
}
