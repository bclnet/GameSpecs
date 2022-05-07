using OpenStack.Graphics.Algorithms;
using OpenStack.Graphics.DirectX;
using OpenStack.Graphics.Renderer;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.Valve.Formats.Blocks
{
    /// <summary>
    /// "VBIB" block.
    /// </summary>
    public class VBIB : Block, IVBIB
    {
        public List<VertexBuffer> VertexBuffers { get; } = new List<VertexBuffer>();
        public List<IndexBuffer> IndexBuffers { get; } = new List<IndexBuffer>();

        public override void Read(BinaryPak parent, BinaryReader r)
        {
            r.Position(Offset);
            var vertexBufferOffset = r.ReadUInt32();
            var vertexBufferCount = r.ReadUInt32();
            var indexBufferOffset = r.ReadUInt32();
            var indexBufferCount = r.ReadUInt32();

            r.BaseStream.Position = Offset + vertexBufferOffset;
            for (var i = 0; i < vertexBufferCount; i++)
            {
                var vertexBuffer = default(VertexBuffer);

                vertexBuffer.Count = r.ReadUInt32();            //0
                vertexBuffer.Size = r.ReadUInt32();             //4
                var decompressedSize = vertexBuffer.Count * vertexBuffer.Size;

                var refA = r.BaseStream.Position;
                var attributeOffset = r.ReadUInt32();  //8
                var attributeCount = r.ReadUInt32();   //12

                //TODO: Read attributes in the future
                var refB = r.BaseStream.Position;
                var dataOffset = r.ReadUInt32();       //16
                var totalSize = r.ReadUInt32();        //20

                vertexBuffer.Attributes = new List<VertexBuffer.VertexAttribute>();

                r.BaseStream.Position = refA + attributeOffset;
                for (var j = 0; j < attributeCount; j++)
                {
                    var previousPosition = r.BaseStream.Position;

                    var attribute = default(VertexBuffer.VertexAttribute);

                    attribute.Name = r.ReadZUTF8().ToUpperInvariant();

                    // Offset is always 40 bytes from the start
                    r.BaseStream.Position = previousPosition + 36;

                    attribute.Type = (DXGI_FORMAT)r.ReadUInt32();
                    attribute.Offset = r.ReadUInt32();

                    // There's unusual amount of padding in attributes
                    r.BaseStream.Position = previousPosition + 56;

                    vertexBuffer.Attributes.Add(attribute);
                }

                r.BaseStream.Position = refB + dataOffset;

                var vertexBufferBytes = r.ReadBytes((int)totalSize);
                vertexBuffer.Buffer = totalSize == decompressedSize
                    ? vertexBufferBytes
                    : MeshOptimizerVertexDecoder.DecodeVertexBuffer((int)vertexBuffer.Count, (int)vertexBuffer.Size, vertexBufferBytes);

                VertexBuffers.Add(vertexBuffer);

                r.BaseStream.Position = refB + 4 + 4; //Go back to the vertex array to read the next iteration
            }

            r.BaseStream.Position = Offset + 8 + indexBufferOffset; //8 to take into account vertexOffset / count
            for (var i = 0; i < indexBufferCount; i++)
            {
                var indexBuffer = default(IndexBuffer);

                indexBuffer.Count = r.ReadUInt32();        //0
                indexBuffer.Size = r.ReadUInt32();         //4
                var decompressedSize = indexBuffer.Count * indexBuffer.Size;

                var unknown1 = r.ReadUInt32();     //8
                var unknown2 = r.ReadUInt32();     //12

                var refC = r.BaseStream.Position;
                var dataOffset = r.ReadUInt32();   //16
                var dataSize = r.ReadUInt32();     //20

                r.BaseStream.Position = refC + dataOffset;

                indexBuffer.Buffer = dataSize == decompressedSize
                    ? r.ReadBytes((int)dataSize)
                    : MeshOptimizerIndexDecoder.DecodeIndexBuffer((int)indexBuffer.Count, (int)indexBuffer.Size, r.ReadBytes((int)dataSize));

                IndexBuffers.Add(indexBuffer);

                r.BaseStream.Position = refC + 4 + 4; //Go back to the index array to read the next iteration.
            }
        }

        public static float[] ReadVertexAttribute(int offset, VertexBuffer vertexBuffer, VertexBuffer.VertexAttribute attribute)
        {
            offset = (int)(offset * vertexBuffer.Size) + (int)attribute.Offset;
            return attribute.Type.ReadVertex(vertexBuffer.Buffer, offset);
        }

        public override void WriteText(IndentedTextWriter w)
        {
            w.WriteLine("Vertex buffers:");
            foreach (var vertexBuffer in VertexBuffers)
            {
                w.WriteLine($"Count: {vertexBuffer.Count}");
                w.WriteLine($"Size: {vertexBuffer.Size}");
                for (var i = 0; i < vertexBuffer.Attributes.Count; i++)
                {
                    var vertexAttribute = vertexBuffer.Attributes[i];
                    w.WriteLine($"Attribute[{i}].Name = {vertexAttribute.Name}");
                    w.WriteLine($"Attribute[{i}].Offset = {vertexAttribute.Offset}");
                    w.WriteLine($"Attribute[{i}].Type = {vertexAttribute.Type}");
                }
                w.WriteLine();
            }
            w.WriteLine();
            w.WriteLine("Index buffers:");
            foreach (var indexBuffer in IndexBuffers)
            {
                w.WriteLine($"Count: {indexBuffer.Count}");
                w.WriteLine($"Size: {indexBuffer.Size}");
                w.WriteLine();
            }
        }
    }
}
