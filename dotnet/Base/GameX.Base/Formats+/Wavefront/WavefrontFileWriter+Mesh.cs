using GameX.Formats.Unknown;
using System;
using System.IO;
using System.Linq;
using static OpenStack.Debug;

namespace GameX.Formats.Wavefront
{
    partial class WavefrontFileWriter
    {
        // Pass a node to this to have it write to the Stream
        void WriteMesh(StreamWriter w, UnknownMesh mesh)
        {
            w.WriteLine($"o {mesh.Name}");

            // We only use 3 things in obj files: vertices, normals and UVs. No need to process the Tangents.
            int tempVertexPosition = CurrentVertexPosition, tempIndicesPosition = CurrentIndicesPosition;
            foreach (var subset in mesh.Subsets)
            {
                // Write vertices data for each MeshSubSet (v)
                w.WriteLine($"g {GroupOverride ?? mesh.Name}");

                // WRITE VERTICES OUT (V, VT)
                if ((mesh.Effects & UnknownMesh.Effect.ScaleOffset) != 0)
                {
                    var (scale, offset) = mesh.ScaleOffset3;
                    foreach (var v in mesh[subset].Vertexs)
                    {
                        var vertex = mesh.GetTransform(v * scale + offset); // Rotate/translate the vertex
                        w.WriteLine($"v {MathX.Safe(vertex.X):F7} {MathX.Safe(vertex.Y):F7} {MathX.Safe(vertex.Z):F7}");
                    }
                }
                else
                    foreach (var v in mesh[subset].Vertexs)
                    {
                        var vertex = mesh.GetTransform(v); // Rotate/translate the vertex
                        w.WriteLine($"v {MathX.Safe(vertex.X):F7} {MathX.Safe(vertex.Y):F7} {MathX.Safe(vertex.Z):F7}");
                    }
                w.WriteLine();
                foreach (var uv in mesh[subset].UVs)
                    w.WriteLine($"vt {MathX.Safe(uv.X):F7} {MathX.Safe(1 - uv.Y):F7} 0");

                w.WriteLine();

                // WRITE NORMALS BLOCK (VN)
                if (mesh.Normals != null)
                    foreach (var normal in mesh[subset].Normals)
                        w.WriteLine($"vn {normal.X:F7} {normal.Y:F7} {normal.Z:F7}");

                // WRITE GROUP (G)
                // w.WriteLine($"g {this.GroupOverride ?? chunkNode.Name}");

                if (Smooth) w.WriteLine($"s {FaceIndex++}");

                // WRITE MATERIAL BLOCK (USEMTL)
                var materials = File.Materials.ToArray();
                if (materials.Length > subset.MatId) w.WriteLine("usemtl {0}", materials[subset.MatId].Name);
                else
                {
                    if (materials.Length > 0) Log($"Missing Material {subset.MatId}");
                    // The material file doesn't have any elements with the Name of the material.  Use the object name.
                    w.WriteLine($"usemtl {File.Name}_{subset.MatId}");
                }

                // Now write out the faces info based on the MtlName
                var indexs = mesh[subset].Indexs;
                for (var j = 0; j < indexs.Length; j += 3)
                    w.WriteLine("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}", // Vertices, UVs, Normals
                        indexs[j] + 1 + CurrentVertexPosition,
                        indexs[j + 1] + 1 + CurrentVertexPosition,
                        indexs[j + 2] + 1 + CurrentVertexPosition);

                tempVertexPosition += mesh[subset].Vertexs.Length;  // add the number of vertices so future objects can start at the right place
                tempIndicesPosition += indexs.Length;  // Not really used...
            }

            // Extend the current vertex, uv and normal positions by the length of those arrays.
            CurrentVertexPosition = tempVertexPosition;
            CurrentIndicesPosition = tempIndicesPosition;
        }
    }
}