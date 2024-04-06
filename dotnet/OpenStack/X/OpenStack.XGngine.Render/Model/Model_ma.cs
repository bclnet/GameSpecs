using System;
using System.Collections.Generic;
using System.NumericsX;
using System.NumericsX.OpenStack;
using System.Runtime.InteropServices;
using System.Text;
using static System.NumericsX.OpenStack.OpenStack;

namespace Gengine.Render
{
    #region Records

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    struct MaNodeHeader
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string name;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string parent;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    struct MaAttribHeader
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string name;
        public int size;
    }

    class MaTransform
    {
        public Vector3 translate;
        public Vector3 rotate;
        public Vector3 scale;
        public MaTransform parent;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MaFace
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public int[] edge;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public int[] vertexNum;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public int[] tVertexNum;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public int[] vertexColors;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public Vector3[] vertexNormals;
    }

    struct MaMesh
    {
        // Transform to be applied
        public MaTransform transform;

        // Vertss
        public int numVertexes;
        public Vector3[] vertexes;
        public int numVertTransforms;
        public Vector4[] vertTransforms;
        public int nextVertTransformIndex;

        // Texture Coordinates
        public int numTVertexes;
        public Vector2[] tvertexes;

        // Edges
        public int numEdges;
        public Vector3[] edges;

        // Colors
        public int numColors;
        public byte[] colors;

        // Faces
        public int numFaces;
        public MaFace[] faces;

        // Normals
        public int numNormals;
        public Vector3[] normals;
        public bool normalsParsed;
        public int nextNormal;
    }

    class MaMaterial
    {
        public string name;
        public float uOffset, vOffset;     // max lets you offset by material without changing texCoords
        public float uTiling, vTiling;     // multiply tex coords by this
        public float angle;                 // in clockwise radians
    }

    class MaObject
    {
        public string name;
        public int materialRef;
        public string materialName;
        public MaMesh mesh;
    }

    class MaFileNode
    {
        public string name;
        public string path;
    }

    class MaMaterialNode
    {
        public string name;
        public MaMaterialNode child;
        public MaFileNode file;
    }

    class MaModel
    {
        public DateTime timeStamp;
        public List<MaMaterial> materials = new();
        public List<MaObject> objects = new();
        public Dictionary<string, MaTransform> transforms = new();

        // Material Resolution
        public Dictionary<string, MaFileNode> fileNodes = new();
        public Dictionary<string, MaMaterialNode> materialNodes = new();
    }

    #endregion

    static unsafe class ModelXMa
    {
        struct Ma
        {
            public bool verbose;
            public MaModel model;
            public MaObject currentObject;
        }

        static Ma maGlobal; // working variables used during parsing

        static void MA_VERBOSE(string x) { if (maGlobal.verbose) { common.Printf(x); } }

        static void MA_ParseNodeHeader(Parser parser, out MaNodeHeader header)
        {
            header = default;
            while (parser.ReadToken(out var token))
                if (token == "-")
                {
                    parser.ReadToken(out token);
                    if (string.Equals(token, "n", StringComparison.OrdinalIgnoreCase)) { parser.ReadToken(out token); header.name = token; }
                    else if (string.Equals(token, "p", StringComparison.OrdinalIgnoreCase)) { parser.ReadToken(out token); header.parent = token; }
                }
                else if (token == ";") break;
        }

        static bool MA_ParseHeaderIndex(MaAttribHeader header, out int minIndex, out int maxIndex, string headerType, string skipString)
        {
            Parser miniParse = new(); miniParse.LoadMemory(header.name, header.name.Length, headerType);
            if (skipString != null) miniParse.SkipUntilString(skipString);

            if (!miniParse.SkipUntilString("[")) { minIndex = default; maxIndex = default; return false; } // This was just a header
            minIndex = miniParse.ParseInt(); miniParse.ReadToken(out var token);
            maxIndex = token == "]" ? minIndex : miniParse.ParseInt();
            return true;
        }

        static bool MA_ParseAttribHeader(Parser parser, out MaAttribHeader header)
        {
            header = default;
            parser.ReadToken(out var token);
            if (token == "-")
            {
                parser.ReadToken(out token);
                if (string.Equals(token, "s", StringComparison.OrdinalIgnoreCase)) { header.size = parser.ParseInt(); parser.ReadToken(out token); }
            }
            header.name = token;
            return true;
        }

        static bool MA_ReadVec3(Parser parser, out Vector3 vec)
        {
            if (!parser.SkipUntilString("double3")) throw new Exception($"Maya Loader '{parser.FileName}': Invalid Vec3");

            // We need to flip y and z because of the maya coordinate system
            vec.x = parser.ParseFloat();
            vec.z = parser.ParseFloat();
            vec.y = parser.ParseFloat();

            return true;
        }

        static bool IsNodeComplete(Token token)
            => string.Equals(token, "createNode", StringComparison.OrdinalIgnoreCase) || string.Equals(token, "connectAttr", StringComparison.OrdinalIgnoreCase) || string.Equals(token, "select", StringComparison.OrdinalIgnoreCase));

        static bool MA_ParseTransform(Parser parser)
        {
            MaNodeHeader header;
            MaTransform transform;

            // Allocate room for the transform
            transform = new MaTransform();
            transform.scale.x = transform.scale.y = transform.scale.z = 1;

            //Get the header info from the transform
            MA_ParseNodeHeader(parser, out header);

            // Read the transform attributes
            while (parser.ReadToken(out var token))
            {
                if (IsNodeComplete(token)) { parser.UnreadToken(token); break; }
                if (string.Equals(token, "setAttr", StringComparison.OrdinalIgnoreCase))
                {
                    parser.ReadToken(out token);
                    if (string.Equals(token, ".t", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!MA_ReadVec3(parser, out transform.translate)) return false;
                        transform.translate.y *= -1;
                    }
                    else if (string.Equals(token, ".r", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!MA_ReadVec3(parser, out transform.rotate)) return false;
                    }
                    else if (string.Equals(token, ".s", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!MA_ReadVec3(parser, out transform.scale)) return false;
                    }
                    else parser.SkipRestOfLine();
                }
            }

            // Find the parent
            if (!string.IsNullOrEmpty(header.parent) && maGlobal.model.transforms.TryGetValue(header.parent, out var parent))
                transform.parent = parent;

            // Add this transform to the list
            maGlobal.model.transforms.Add(header.name, transform);
            return true;
        }

        static bool MA_ParseVertex(Parser parser, in MaAttribHeader header)
        {
            var pMesh = maGlobal.currentObject.mesh;

            // Allocate enough space for all the verts if this is the first attribute for verticies
            if (pMesh.vertexes == null)
            {
                pMesh.numVertexes = header.size; // XXX: +1?
                pMesh.vertexes = new Vector3[pMesh.numVertexes];
            }

            //Get the start and end index for this attribute
            if (!MA_ParseHeaderIndex(header, out var minIndex, out var maxIndex, "VertexHeader", null)) return true; // This was just a header

            // Read each vert
            for (var i = minIndex; i <= maxIndex; i++)
            {
                pMesh.vertexes[i].x = parser.ParseFloat();
                pMesh.vertexes[i].z = parser.ParseFloat();
                pMesh.vertexes[i].y = -parser.ParseFloat();
            }

            return true;
        }

        static bool MA_ParseVertexTransforms(Parser parser, in MaAttribHeader header)
        {
            var pMesh = maGlobal.currentObject.mesh;

            // Allocate enough space for all the verts if this is the first attribute for verticies
            if (pMesh.vertTransforms == null)
            {
                pMesh.numVertTransforms = header.size == 0 ? 1 : header.size;
                pMesh.vertTransforms = new Vector4[pMesh.numVertTransforms];
                pMesh.nextVertTransformIndex = 0;
            }

            //Get the start and end index for this attribute
            if (!MA_ParseHeaderIndex(header, out var minIndex, out var maxIndex, "VertexTransformHeader", null)) return true; // This was just a header

            parser.ReadToken(out var token);
            if (token == "-")
            {
                parser.ReadToken(out var tk2);
                if (string.Equals(tk2, "type", StringComparison.OrdinalIgnoreCase)) parser.SkipUntilString("float3");
                else { parser.UnreadToken(tk2); parser.UnreadToken(token); }
            }
            else parser.UnreadToken(token);

            // Read each vert
            for (var i = minIndex; i <= maxIndex; i++)
            {
                pMesh.vertTransforms[pMesh.nextVertTransformIndex].x = parser.ParseFloat();
                pMesh.vertTransforms[pMesh.nextVertTransformIndex].z = parser.ParseFloat();
                pMesh.vertTransforms[pMesh.nextVertTransformIndex].y = -parser.ParseFloat();

                // w hold the vert index
                pMesh.vertTransforms[pMesh.nextVertTransformIndex].w = i;

                pMesh.nextVertTransformIndex++;
            }

            return true;
        }

        static bool MA_ParseEdge(Parser parser, in MaAttribHeader header)
        {
            var pMesh = maGlobal.currentObject.mesh;

            // Allocate enough space for all the verts if this is the first attribute for verticies
            if (pMesh.edges == null)
            {
                pMesh.numEdges = header.size;
                pMesh.edges = new Vector3[pMesh.numEdges];
            }

            // Get the start and end index for this attribute
            if (!MA_ParseHeaderIndex(header, out var minIndex, out var maxIndex, "EdgeHeader", null)) return true; // This was just a header

            // Read each vert
            for (var i = minIndex; i <= maxIndex; i++)
            {
                pMesh.edges[i].x = parser.ParseFloat();
                pMesh.edges[i].y = parser.ParseFloat();
                pMesh.edges[i].z = parser.ParseFloat();
            }

            return true;
        }

        static bool MA_ParseNormal(Parser parser, in MaAttribHeader header)
        {
            var pMesh = maGlobal.currentObject.mesh;

            // Allocate enough space for all the verts if this is the first attribute for verticies
            if (pMesh.normals == null)
            {
                pMesh.numNormals = header.size;
                pMesh.normals = new Vector3[pMesh.numNormals];
            }

            // Get the start and end index for this attribute
            if (!MA_ParseHeaderIndex(header, out var minIndex, out var maxIndex, "NormalHeader", null)) return true; // This was just a header

            parser.ReadToken(out var token);
            if (token == "-")
            {
                parser.ReadToken(out var tk2);
                if (string.Equals(tk2, "type", StringComparison.OrdinalIgnoreCase)) parser.SkipUntilString("float3");
                else { parser.UnreadToken(tk2); parser.UnreadToken(token); }
            }
            else parser.UnreadToken(token);

            // Read each vert
            for (var i = minIndex; i <= maxIndex; i++)
            {
                pMesh.normals[i].x = parser.ParseFloat();

                // Adjust the normals for the change in coordinate systems
                pMesh.normals[i].z = parser.ParseFloat();
                pMesh.normals[i].y = -parser.ParseFloat();

                pMesh.normals[i].Normalize();

            }

            pMesh.normalsParsed = true;
            pMesh.nextNormal = 0;

            return true;
        }

        static bool MA_ParseFace(Parser parser, in MaAttribHeader header)
        {
            var pMesh = maGlobal.currentObject.mesh;

            // Allocate enough space for all the verts if this is the first attribute for verticies
            if (pMesh.faces == null)
            {
                pMesh.numFaces = header.size;
                pMesh.faces = new MaFace[pMesh.numFaces];
            }

            // Get the start and end index for this attribute
            if (!MA_ParseHeaderIndex(header, out var minIndex, out var maxIndex, "FaceHeader", null)) return true; // This was just a header

            // Read the face data
            var currentFace = minIndex - 1;
            while (parser.ReadToken(out var token))
            {
                if (IsNodeComplete(token)) { parser.UnreadToken(token); break; }

                if (string.Equals(token, "f", StringComparison.OrdinalIgnoreCase))
                {
                    var count = parser.ParseInt();
                    if (count != 3) throw new Exception($"Maya Loader '{parser.FileName}': Face is not a triangle.");
                    // Increment the face number because a new face always starts with an "f" token
                    currentFace++;

                    // We cannot reorder edges until later because the normal processing assumes the edges are in the original order
                    pMesh.faces[currentFace].edge[0] = parser.ParseInt();
                    pMesh.faces[currentFace].edge[1] = parser.ParseInt();
                    pMesh.faces[currentFace].edge[2] = parser.ParseInt();

                    // Some more init stuff
                    pMesh.faces[currentFace].vertexColors[0] = pMesh.faces[currentFace].vertexColors[1] = pMesh.faces[currentFace].vertexColors[2] = -1;
                }
                if (string.Equals(token, "mu", StringComparison.OrdinalIgnoreCase))
                {
                    parser.ParseInt();
                    var count = parser.ParseInt();
                    if (count != 3) throw new Exception($"Maya Loader '{parser.FileName}': Invalid texture coordinates.");
                    pMesh.faces[currentFace].tVertexNum[0] = parser.ParseInt();
                    pMesh.faces[currentFace].tVertexNum[1] = parser.ParseInt();
                    pMesh.faces[currentFace].tVertexNum[2] = parser.ParseInt();
                }
                if (string.Equals(token, "mf", StringComparison.OrdinalIgnoreCase))
                {
                    var count = parser.ParseInt();
                    if (count != 3) throw new Exception($"Maya Loader '{parser.FileName}': Invalid texture coordinates.");
                    pMesh.faces[currentFace].tVertexNum[0] = parser.ParseInt();
                    pMesh.faces[currentFace].tVertexNum[1] = parser.ParseInt();
                    pMesh.faces[currentFace].tVertexNum[2] = parser.ParseInt();
                }
                if (string.Equals(token, "fc", StringComparison.OrdinalIgnoreCase))
                {
                    var count = parser.ParseInt();
                    if (count != 3) throw new Exception($"Maya Loader '{parser.FileName}': Invalid vertex color.");
                    pMesh.faces[currentFace].vertexColors[0] = parser.ParseInt();
                    pMesh.faces[currentFace].vertexColors[1] = parser.ParseInt();
                    pMesh.faces[currentFace].vertexColors[2] = parser.ParseInt();
                }
            }

            return true;
        }

        static bool MA_ParseColor(Parser parser, in MaAttribHeader header)
        {
            var pMesh = maGlobal.currentObject.mesh;

            // Allocate enough space for all the verts if this is the first attribute for verticies
            if (pMesh.colors == null)
            {
                pMesh.numColors = header.size;
                pMesh.colors = new byte[pMesh.numColors * 4];
            }

            // Get the start and end index for this attribute
            if (!MA_ParseHeaderIndex(header, out var minIndex, out var maxIndex, "ColorHeader", null)) return true; // This was just a header

            // Read each vert
            for (var i = minIndex; i <= maxIndex; i++)
            {
                pMesh.colors[i * 4] = (byte)(parser.ParseFloat() * 255);
                pMesh.colors[i * 4 + 1] = (byte)(parser.ParseFloat() * 255);
                pMesh.colors[i * 4 + 2] = (byte)(parser.ParseFloat() * 255);
                pMesh.colors[i * 4 + 3] = (byte)(parser.ParseFloat() * 255);
            }

            return true;
        }

        static bool MA_ParseTVert(Parser parser, in MaAttribHeader header)
        {
            var pMesh = maGlobal.currentObject.mesh;

            // This is not the texture coordinates. It is just the name so ignore it
            if (header.name.Contains("uvsn")) return true;

            // Allocate enough space for all the data
            if (pMesh.tvertexes == null)
            {
                pMesh.numTVertexes = header.size;
                pMesh.tvertexes = new Vector2[pMesh.numTVertexes];
            }

            // Get the start and end index for this attribute
            if (!MA_ParseHeaderIndex(header, out var minIndex, out var maxIndex, "TextureCoordHeader", "uvsp")) return true; //This was just a header

            parser.ReadToken(out var token);
            if (token == "-")
            {
                parser.ReadToken(out var tk2);
                if (string.Equals(tk2, "type", StringComparison.OrdinalIgnoreCase)) parser.SkipUntilString("float2");
                else { parser.UnreadToken(tk2); parser.UnreadToken(token); }
            }
            else parser.UnreadToken(token);

            // Read each tvert
            for (var i = minIndex; i <= maxIndex; i++)
            {
                pMesh.tvertexes[i].x = parser.ParseFloat();
                pMesh.tvertexes[i].y = 1f - parser.ParseFloat();
            }

            return true;
        }

        // Quick check to see if the vert participates in a shared normal
        static bool MA_QuickIsVertShared(int faceIndex, int vertIndex)
        {
            var pMesh = maGlobal.currentObject.mesh;
            var vertNum = pMesh.faces[faceIndex].vertexNum[vertIndex];

            for (var i = 0; i < 3; i++)
            {
                var edge = pMesh.faces[faceIndex].edge[i];
                if (edge < 0) edge = (int)(MathX.Fabs(edge) - 1);
                if (pMesh.edges[edge].z == 1 && (pMesh.edges[edge].x == vertNum || pMesh.edges[edge].y == vertNum))
                    return true;
            }
            return false;
        }

        static void MA_GetSharedFace(int faceIndex, int vertIndex, out int sharedFace, out int sharedVert)
        {
            var pMesh = maGlobal.currentObject.mesh;
            var vertNum = pMesh.faces[faceIndex].vertexNum[vertIndex];

            sharedFace = -1;
            sharedVert = -1;

            // Find a shared edge on this face that contains the specified vert
            for (var edgeIndex = 0; edgeIndex < 3; edgeIndex++)
            {
                var edge = pMesh.faces[faceIndex].edge[edgeIndex];
                if (edge < 0) edge = (int)(MathX.Fabs(edge) - 1);

                if (pMesh.edges[edge].z == 1 && (pMesh.edges[edge].x == vertNum || pMesh.edges[edge].y == vertNum))
                    for (var i = 0; i < faceIndex; i++)
                        for (var j = 0; j < 3; j++)
                            if (pMesh.faces[i].vertexNum[j] == vertNum) { sharedFace = i; sharedVert = j; break; }
                if (sharedFace != -1) break;
            }
        }

        static void MA_ParseMesh(Parser parser)
        {
            MaObject obj = new MaObject();
            maGlobal.model.objects.Add(obj);
            maGlobal.currentObject = obj;
            obj.materialRef = -1;

            // Get the header info from the mesh
            MA_ParseNodeHeader(parser, out var header);

            // Find my parent
            if (!string.IsNullOrEmpty(header.parent) && maGlobal.model.transforms.TryGetValue(header.parent, out var parent))
                maGlobal.currentObject.mesh.transform = parent;

            obj.name = header.name;

            // Read the transform attributes
            while (parser.ReadToken(out var token))
            {
                if (IsNodeComplete(token)) { parser.UnreadToken(token); break; }
                if (string.Equals(token, "setAttr", StringComparison.OrdinalIgnoreCase))
                {
                    MA_ParseAttribHeader(parser, out var header2);

                    if (header.name.Contains(".vt")) MA_ParseVertex(parser, header2);
                    else if (header.name.Contains(".ed")) MA_ParseEdge(parser, header2);
                    else if (header.name.Contains(".pt")) MA_ParseVertexTransforms(parser, header2);
                    else if (header.name.Contains(".n")) MA_ParseNormal(parser, header2);
                    else if (header.name.Contains(".fc")) MA_ParseFace(parser, header2);
                    else if (header.name.Contains(".clr")) MA_ParseColor(parser, header2);
                    else if (header.name.Contains(".uvst")) MA_ParseTVert(parser, header2);
                    else parser.SkipRestOfLine();
                }
            }

            var pMesh = maGlobal.currentObject.mesh;

            // Get the verts from the edge
            for (var i = 0; i < pMesh.numFaces; i++)
                for (var j = 0; j < 3; j++)
                {
                    var edge = pMesh.faces[i].edge[j];
                    if (edge < 0) { edge = (int)(MathX.Fabs(edge) - 1); pMesh.faces[i].vertexNum[j] = (int)pMesh.edges[edge].y; }
                    else pMesh.faces[i].vertexNum[j] = (int)pMesh.edges[edge].x;
                }

            // Get the normals
            if (pMesh.normalsParsed)
                for (var i = 0; i < pMesh.numFaces; i++)
                    for (var j = 0; j < 3; j++)
                    {
                        // Is this vertex shared
                        var sharedFace = -1;
                        var sharedVert = -1;

                        if (MA_QuickIsVertShared(i, j)) MA_GetSharedFace(i, j, out sharedFace, out sharedVert);

                        // Get the normal from the share
                        if (sharedFace != -1) pMesh.faces[i].vertexNormals[j] = pMesh.faces[sharedFace].vertexNormals[sharedVert];
                        else
                        {
                            // The vertex is not shared so get the next normal
                            if (pMesh.nextNormal >= pMesh.numNormals) throw new Exception($"Maya Loader '{parser.FileName}': Invalid Normals Index."); // We are using more normals than exist
                            pMesh.faces[i].vertexNormals[j] = pMesh.normals[pMesh.nextNormal];
                            pMesh.nextNormal++;
                        }
                    }

            // Now that the normals are good...lets reorder the verts to make the tris face the right way
            for (var i = 0; i < pMesh.numFaces; i++)
            {
                var tmp = pMesh.faces[i].vertexNum[1];
                pMesh.faces[i].vertexNum[1] = pMesh.faces[i].vertexNum[2];
                pMesh.faces[i].vertexNum[2] = tmp;

                var tmpVec = pMesh.faces[i].vertexNormals[1];
                pMesh.faces[i].vertexNormals[1] = pMesh.faces[i].vertexNormals[2];
                pMesh.faces[i].vertexNormals[2] = tmpVec;

                tmp = pMesh.faces[i].tVertexNum[1];
                pMesh.faces[i].tVertexNum[1] = pMesh.faces[i].tVertexNum[2];
                pMesh.faces[i].tVertexNum[2] = tmp;

                tmp = pMesh.faces[i].vertexColors[1];
                pMesh.faces[i].vertexColors[1] = pMesh.faces[i].vertexColors[2];
                pMesh.faces[i].vertexColors[2] = tmp;
            }

            // Now apply the pt transformations
            for (var i = 0; i < pMesh.numVertTransforms; i++)
            {
                var idx = (int)pMesh.vertTransforms[i].w;
                if (idx < 0 || idx >= pMesh.numVertexes)
                {
                    // this happens with d3xp/models/david/hell_h7.ma in the d3xp hell level
                    // TODO: if it happens for other models, too, maybe it's intended and the .ma parsing is broken
                    common.Warning($"Model {parser.FileName} tried to set an out-of-bounds vertex transform ({idx}, but max vert. index is {pMesh.numVertexes - 1})!");
                    continue;
                }
                pMesh.vertexes[idx] += pMesh.vertTransforms[i].ToVec3();
            }

            MA_VERBOSE($"MESH {header.name} - parent {header.parent}\n");
            MA_VERBOSE($"\tverts:{maGlobal.currentObject.mesh.numVertexes}\n");
            MA_VERBOSE($"\tfaces:{maGlobal.currentObject.mesh.numFaces}\n");
        }

        static void MA_ParseFileNode(Parser parser)
        {
            // Get the header info from the node
            MA_ParseNodeHeader(parser, out var header);

            // Read the transform attributes
            while (parser.ReadToken(out var token))
            {
                if (IsNodeComplete(token)) { parser.UnreadToken(token); break; }
                if (string.Equals(token, "setAttr", StringComparison.OrdinalIgnoreCase))
                {
                    MA_ParseAttribHeader(parser, out var attribHeader);

                    if (attribHeader.name.Contains(".ftn"))
                    {
                        parser.SkipUntilString("string");
                        parser.ReadToken(out token);
                        if (token == "(") parser.ReadToken(out token);

                        var fileNode = new MaFileNode
                        {
                            name = header.name,
                            path = token
                        };
                        maGlobal.model.fileNodes.Add(fileNode.name, fileNode);
                    }
                    else parser.SkipRestOfLine();
                }
            }
        }

        static void MA_ParseMaterialNode(Parser parser)
        {
            // Get the header info from the node
            MA_ParseNodeHeader(parser, out var header);

            var matNode = new MaMaterialNode
            {
                name = header.name
            };
            maGlobal.model.materialNodes.Add(matNode.name, matNode);
        }

        static void MA_ParseCreateNode(Parser parser)
        {
            parser.ReadToken(out var token);

            if (string.Equals(token, "transform", StringComparison.OrdinalIgnoreCase)) MA_ParseTransform(parser);
            else if (string.Equals(token, "mesh", StringComparison.OrdinalIgnoreCase)) MA_ParseMesh(parser);
            else if (string.Equals(token, "file", StringComparison.OrdinalIgnoreCase)) MA_ParseFileNode(parser);
            else if (string.Equals(token, "shadingEngine", StringComparison.OrdinalIgnoreCase) || string.Equals(token, "lambert", StringComparison.OrdinalIgnoreCase) || string.Equals(token, "phong", StringComparison.OrdinalIgnoreCase) || string.Equals(token, "blinn", StringComparison.OrdinalIgnoreCase)) MA_ParseMaterialNode(parser);
        }

        static int MA_AddMaterial(string materialName)
        {
            if (maGlobal.model.materialNodes.TryGetValue(materialName, out var destNode))
            {
                var matNode = destNode;

                // Iterate down the tree until we get a file
                while (matNode != null && matNode.file == null) matNode = matNode.child;
                if (matNode != null && matNode.file != null)
                {
                    var material = new MaMaterial
                    {
                        name = fileSystem.OSPathToRelativePath(matNode.file.path), // Remove the OS stuff
                    };
                    maGlobal.model.materials.Add(material);
                    return maGlobal.model.materials.Count - 1;
                }
            }
            return -1;
        }

        static bool MA_ParseConnectAttr(Parser parser)
        {
            string temp, srcName, srcType, destName, destType;

            parser.ReadToken(out var token); temp = token;
            var dot = temp.IndexOf(".");
            if (dot == -1) throw new Exception($"Maya Loader '{parser.FileName}': Invalid Connect Attribute.");
            srcName = temp.Substring(0, dot);
            srcType = temp.Substring(dot + 1, temp.Length - dot - 1);

            parser.ReadToken(out token); temp = token;
            dot = temp.IndexOf(".");
            if (dot == -1) throw new Exception($"Maya Loader '{parser.FileName}': Invalid Connect Attribute.");
            destName = temp.Substring(0, dot);
            destType = temp.Substring(dot + 1, temp.Length - dot - 1);

            if (srcType.IndexOf("oc") != -1)
            {
                // Is this attribute a material node attribute
                if (maGlobal.model.materialNodes.TryGetValue(srcName, out var matNode) && maGlobal.model.materialNodes.TryGetValue(destName, out var destNode)) destNode.child = matNode;

                // Is this attribute a file node
                if (maGlobal.model.fileNodes.TryGetValue(srcName, out var fileNode) && maGlobal.model.materialNodes.TryGetValue(destName, out destNode)) destNode.file = fileNode;
            }

            if (srcType.IndexOf("iog") != -1) // Is this an attribute for one of our meshes
                for (var i = 0; i < maGlobal.model.objects.Count; i++)
                    if (maGlobal.model.objects[i].name == srcName)
                    {
                        //maGlobal.model.objects[i].materialRef = MA_AddMaterial(destName);
                        maGlobal.model.objects[i].materialName = destName;
                        break;
                    }

            return true;
        }

        static void MA_BuildScale(in Matrix4x4 mat, float x, float y, float z)
        {
            mat.Identity();
            mat[0].x = x;
            mat[1].y = y;
            mat[2].z = z;
        }

        static void MA_BuildAxisRotation(in Matrix4x4 mat, float ang, int axis)
        {
            var sinAng = MathX.Sin(ang);
            var cosAng = MathX.Cos(ang);

            mat.Identity();
            switch (axis)
            {
                case 0: // x
                    mat[1].y = cosAng; mat[1].z = sinAng;
                    mat[2].y = -sinAng; mat[2].z = cosAng;
                    break;
                case 1: // y
                    mat[0].x = cosAng; mat[0].z = -sinAng;
                    mat[2].x = sinAng; mat[2].z = cosAng;
                    break;
                case 2: // z
                    mat[0].x = cosAng; mat[0].y = sinAng;
                    mat[1].x = -sinAng; mat[1].y = cosAng;
                    break;
            }
        }

        static void MA_ApplyTransformation(MaModel model)
        {
            for (var i = 0; i < model.objects.Count; i++)
            {
                var mesh = model.objects[i].mesh;
                var transform = mesh.transform;

                while (transform != null)
                {
                    Matrix4x4 rotx = new(), roty = new(), rotz = new(), scale = new();

                    rotx.Identity();
                    roty.Identity();
                    rotz.Identity();

                    if (Math.Abs(transform.rotate.x) > 0f) MA_BuildAxisRotation(rotx, MathX.DEG2RAD(-transform.rotate.x), 0);
                    if (Math.Abs(transform.rotate.y) > 0f) MA_BuildAxisRotation(roty, MathX.DEG2RAD(transform.rotate.y), 1);
                    if (Math.Abs(transform.rotate.z) > 0f) MA_BuildAxisRotation(rotz, MathX.DEG2RAD(-transform.rotate.z), 2);

                    MA_BuildScale(scale, transform.scale.x, transform.scale.y, transform.scale.z);

                    // Apply the transformation to each vert
                    for (var j = 0; j < mesh.numVertexes; j++)
                    {
                        mesh.vertexes[j] = scale * mesh.vertexes[j];

                        mesh.vertexes[j] = rotx * mesh.vertexes[j];
                        mesh.vertexes[j] = rotz * mesh.vertexes[j];
                        mesh.vertexes[j] = roty * mesh.vertexes[j];

                        mesh.vertexes[j] = mesh.vertexes[j] + transform.translate;
                    }

                    transform = transform.parent;
                }
            }
        }

        static MaModel MA_Parse(string buffer, string filename, bool verbose)
        {
            maGlobal = default;
            maGlobal.verbose = verbose;
            maGlobal.currentObject = null;

            // NOTE: using new operator because aseModel_t contains idList class objects
            maGlobal.model = new MaModel();
            maGlobal.model.objects.Resize(32, 32);
            maGlobal.model.materials.Resize(32, 32);

            Parser parser = new();
            parser.Flags = LEXFL.NOSTRINGCONCAT;
            parser.LoadMemory(buffer, buffer.Length, filename);

            while (parser.ReadToken(out var token))
                if (string.Equals(token, "createNode", StringComparison.OrdinalIgnoreCase)) MA_ParseCreateNode(parser);
                else if (string.Equals(token, "connectAttr", StringComparison.OrdinalIgnoreCase)) MA_ParseConnectAttr(parser);

            // Resolve The Materials
            for (var i = 0; i < maGlobal.model.objects.Count; i++)
                maGlobal.model.objects[i].materialRef = MA_AddMaterial(maGlobal.model.objects[i].materialName);


            // Apply Transformation
            MA_ApplyTransformation(maGlobal.model);

            return maGlobal.model;
        }

        internal static MaModel MA_Load(string fileName)
        {
            MaModel ma;

            fileSystem.ReadFile(fileName, out var buf, out var timeStamp);
            if (buf == null) return null;

            try
            {
                ma = MA_Parse(Encoding.ASCII.GetString(buf), fileName, false);
                ma.timeStamp = timeStamp;
            }
            catch (Exception e)
            {
                common.Warning(e.Message);
                if (maGlobal.model != null) MA_Free(maGlobal.model);
                ma = null;
            }

            fileSystem.FreeFile(buf);

            return ma;
        }

        internal static void MA_Free(MaModel ma) { }
    }
}
