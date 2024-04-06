using OpenStack.Graphics.DirectX;
using OpenStack.Graphics.Renderer1;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace OpenStack.Graphics.OpenGL.Renderer1
{
    //was:Render/GPUMeshBufferCache
    public class GLMeshBufferCache
    {
        Dictionary<IVBIB, GLMeshBuffers> _gpuBuffers = new Dictionary<IVBIB, GLMeshBuffers>();
        Dictionary<VAOKey, uint> _vertexArrayObjects = new Dictionary<VAOKey, uint>();

        struct VAOKey
        {
            public GLMeshBuffers VBIB;
            public Shader Shader;
            public uint VertexIndex;
            public uint IndexIndex;
            public uint BaseVertex;
        }

        public GLMeshBuffers GetVertexIndexBuffers(IVBIB vbib)
        {
            if (_gpuBuffers.TryGetValue(vbib, out var gpuVbib)) return gpuVbib;
            else
            {
                var newGpuVbib = new GLMeshBuffers(vbib);
                _gpuBuffers.Add(vbib, newGpuVbib);
                return newGpuVbib;
            }
        }

        public uint GetVertexArrayObject(IVBIB vbib, Shader shader, uint vtxIndex, uint idxIndex, uint baseVertex)
        {
            var gpuVbib = GetVertexIndexBuffers(vbib);
            var vaoKey = new VAOKey
            {
                VBIB = gpuVbib,
                Shader = shader,
                VertexIndex = vtxIndex,
                IndexIndex = idxIndex,
                BaseVertex = baseVertex,
            };

            if (_vertexArrayObjects.TryGetValue(vaoKey, out var vaoHandle)) return vaoHandle;
            else
            {
                GL.GenVertexArrays(1, out uint newVaoHandle);

                GL.BindVertexArray(newVaoHandle);
                GL.BindBuffer(BufferTarget.ArrayBuffer, gpuVbib.VertexBuffers[vtxIndex].Handle);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, gpuVbib.IndexBuffers[idxIndex].Handle);

                var curVertexBuffer = vbib.VertexBuffers[(int)vtxIndex];
                var texCoordNum = 0;
                var colorNum = 0;
                foreach (var attribute in curVertexBuffer.Attributes)
                {
                    var attributeName = $"v{attribute.SemanticName}";

                    if (attribute.SemanticName == "TEXCOORD" && texCoordNum++ > 0) attributeName += texCoordNum;
                    else if (attribute.SemanticName == "COLOR" && colorNum++ > 0) attributeName += colorNum;

                    BindVertexAttrib(attribute, attributeName, shader.Program, (int)curVertexBuffer.ElementSizeInBytes, baseVertex);
                }

                GL.BindVertexArray(0);

                _vertexArrayObjects.Add(vaoKey, newVaoHandle);
                return newVaoHandle;
            }
        }

        static void BindVertexAttrib(OnDiskBufferData.Attribute attribute, string attributeName, int shaderProgram, int stride, uint baseVertex)
        {
            var attributeLocation = GL.GetAttribLocation(shaderProgram, attributeName);
            if (attributeLocation == -1) return; // Ignore this attribute if it is not found in the shader

            GL.EnableVertexAttribArray(attributeLocation);
            switch (attribute.Format)
            {
                case DXGI_FORMAT.R32G32B32_FLOAT: GL.VertexAttribPointer(attributeLocation, 3, VertexAttribPointerType.Float, false, stride, (IntPtr)(baseVertex + attribute.Offset)); break;
                case DXGI_FORMAT.R8G8B8A8_UNORM: GL.VertexAttribPointer(attributeLocation, 4, VertexAttribPointerType.UnsignedByte, false, stride, (IntPtr)(baseVertex + attribute.Offset)); break;
                case DXGI_FORMAT.R32G32_FLOAT: GL.VertexAttribPointer(attributeLocation, 2, VertexAttribPointerType.Float, false, stride, (IntPtr)(baseVertex + attribute.Offset)); break;
                case DXGI_FORMAT.R16G16_FLOAT: GL.VertexAttribPointer(attributeLocation, 2, VertexAttribPointerType.HalfFloat, false, stride, (IntPtr)(baseVertex + attribute.Offset)); break;
                case DXGI_FORMAT.R32G32B32A32_FLOAT: GL.VertexAttribPointer(attributeLocation, 4, VertexAttribPointerType.Float, false, stride, (IntPtr)(baseVertex + attribute.Offset)); break;
                case DXGI_FORMAT.R8G8B8A8_UINT: GL.VertexAttribPointer(attributeLocation, 4, VertexAttribPointerType.UnsignedByte, false, stride, (IntPtr)(baseVertex + attribute.Offset)); break;
                case DXGI_FORMAT.R16G16_SINT: GL.VertexAttribIPointer(attributeLocation, 2, VertexAttribIntegerType.Short, stride, (IntPtr)(baseVertex + attribute.Offset)); break;
                case DXGI_FORMAT.R16G16B16A16_SINT: GL.VertexAttribIPointer(attributeLocation, 4, VertexAttribIntegerType.Short, stride, (IntPtr)(baseVertex + attribute.Offset)); break;
                case DXGI_FORMAT.R16G16_SNORM: GL.VertexAttribPointer(attributeLocation, 2, VertexAttribPointerType.Short, true, stride, (IntPtr)(baseVertex + attribute.Offset)); break;
                case DXGI_FORMAT.R16G16_UNORM: GL.VertexAttribPointer(attributeLocation, 2, VertexAttribPointerType.UnsignedShort, true, stride, (IntPtr)(baseVertex + attribute.Offset)); break;
                default: throw new FormatException($"Unknown attribute format {attribute.Format}");
            }
        }
    }
}
