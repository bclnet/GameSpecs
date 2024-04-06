using System;
using System.Collections.Generic;
using System.NumericsX;
using System.NumericsX.OpenStack;
using System.Runtime.InteropServices;
using static System.NumericsX.OpenStack.OpenStack;

namespace Gengine.Render
{
    #region Records

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class AseFace
    {
        public int[] vertexNum = new int[3];
        public int[] tVertexNum = new int[3];
        public Vector3 faceNormal;
        public Vector3[] vertexNormals = new Vector3[3];
        public byte[][] vertexColors = new byte[3][];
    }

    public class AseMesh
    {
        public int timeValue;

        public int numVertexes;
        public int numTVertexes;
        public int numCVertexes;
        public int numFaces;
        public int numTVFaces;
        public int numCVFaces;

        public Vector3[] transform = new Vector3[4];            // applied to normals

        public bool colorsParsed;
        public bool normalsParsed;
        public Vector3[] vertexes;
        public Vector2[] tvertexes;
        public Vector3[] cvertexes;
        public AseFace[] faces;
    }

    public class AseMaterial
    {
        public string name;
        public float uOffset, vOffset;      // max lets you offset by material without changing texCoords
        public float uTiling, vTiling;      // multiply tex coords by this
        public float angle;                 // in clockwise radians
    }

    public class AseObject
    {
        public string name;
        public int materialRef;

        public AseMesh mesh;

        // frames are only present with animations
        public List<AseMesh> frames = new();           // aseMesh_t
    }

    public class AseModel
    {
        public DateTime timeStamp;
        public List<AseMaterial> materials = new();
        public List<AseObject> objects = new();
    }

    #endregion

    static unsafe class ModelXAse
    {
        static void VERBOSE(string x) { if (ase.verbose) common.Printf(x); }

        // working variables used during parsing
        internal unsafe class Ase
        {
            public byte[] buffer;
            public int curpos;
            public int len;
            public char[] tokenBuf = new char[1024];
            public string token;

            public bool verbose;

            public AseModel model;
            public AseObject currentObject;
            public AseMesh currentMesh;
            public AseMaterial currentMaterial;
            public int currentFace;
            public int currentVertex;
        }

        internal static Ase ase = new();

        static AseMesh ASE_GetCurrentMesh()
            => ase.currentMesh;

        static bool CharIsTokenDelimiter(char ch)
            => ch <= 32;

        static bool ASE_GetToken(bool restOfLine)
        {
            var i = 0;

            if (ase.buffer == null) return false;
            if (ase.curpos == ase.len) return false;

            // skip over crap
            while (ase.curpos < ase.len && ase.buffer[ase.curpos] <= 32) ase.curpos++;
            while (ase.curpos < ase.len)
            {
                ase.tokenBuf[i] = (char)ase.buffer[ase.curpos];
                ase.curpos++;
                i++;
                if ((CharIsTokenDelimiter(ase.tokenBuf[i - 1]) && !restOfLine) || (ase.tokenBuf[i - 1] == '\n') || (ase.tokenBuf[i - 1] == '\r')) { ase.tokenBuf[i - 1] = '\x0'; break; }
            }

            ase.tokenBuf[i] = '\x0';
            ase.token = new string(ase.tokenBuf, 0, i);

            return true;
        }

        static void ASE_ParseBracedBlock(Action<string> parser)
        {
            var indent = 0;
            while (ASE_GetToken(false))
                if (ase.token == "{") indent++;
                else if (ase.token == "}") { --indent; if (indent == 0) break; else if (indent < 0) common.Error("Unexpected '}'"); }
                else parser?.Invoke(ase.token);
        }

        static void ASE_SkipEnclosingBraces()
        {
            var indent = 0;
            while (ASE_GetToken(false))
                if (ase.token == "{") indent++;
                else if (ase.token == "}") { indent--; if (indent == 0) break; else if (indent < 0) common.Error("Unexpected '}'"); }
        }

        static void ASE_SkipRestOfLine()
            => ASE_GetToken(true);

        static void ASE_KeyMAP_DIFFUSE(string token)
        {
            AseMaterial material;

            if (token == "*BITMAP")
            {
                ASE_GetToken(false);

                // remove the quotes
                var s = ase.token.IndexOf("\"", 1);
                if (s != -1) ase.token = ase.token.Remove(s);
                var matname = ase.token + 1;

                // convert the 3DSMax material pathname to a qpath
                var qpath = fileSystem.OSPathToRelativePath(PathX.BackSlashesToSlashes(matname));
                ase.currentMaterial.name = qpath;
            }
            else if (token == "*UVW_U_OFFSET") { material = ase.model.materials[^1]; ASE_GetToken(false); material.uOffset = float.Parse(ase.token); }
            else if (token == "*UVW_V_OFFSET") { material = ase.model.materials[^1]; ASE_GetToken(false); material.vOffset = float.Parse(ase.token); }
            else if (token == "*UVW_U_TILING") { material = ase.model.materials[^1]; ASE_GetToken(false); material.uTiling = float.Parse(ase.token); }
            else if (token == "*UVW_V_TILING") { material = ase.model.materials[^1]; ASE_GetToken(false); material.vTiling = float.Parse(ase.token); }
            else if (token == "*UVW_ANGLE") { material = ase.model.materials[^1]; ASE_GetToken(false); material.angle = float.Parse(ase.token); }
        }

        static void ASE_KeyMATERIAL(string token)
        {
            if (token == "*MAP_DIFFUSE") ASE_ParseBracedBlock(ASE_KeyMAP_DIFFUSE);
        }

        static void ASE_KeyMATERIAL_LIST(string token)
        {
            if (token == "*MATERIAL_COUNT") { ASE_GetToken(false); VERBOSE($"..num materials: {ase.token}\n"); }
            else if (token == "*MATERIAL")
            {
                VERBOSE($"..material {ase.model.materials.Count}\n");

                ase.currentMaterial = new AseMaterial { uTiling = 1, vTiling = 1 };
                ase.model.materials.Add(ase.currentMaterial);

                ASE_ParseBracedBlock(ASE_KeyMATERIAL);
            }
        }

        static void ASE_KeyNODE_TM(string token)
        {
            if (token == "*TM_ROW0")
            {
                ASE_GetToken(false); ase.currentObject.mesh.transform[0].x = float.Parse(ase.token);
                ASE_GetToken(false); ase.currentObject.mesh.transform[0].y = float.Parse(ase.token);
                ASE_GetToken(false); ase.currentObject.mesh.transform[0].z = float.Parse(ase.token);
            }
            else if (token == "*TM_ROW1")
            {
                ASE_GetToken(false); ase.currentObject.mesh.transform[1].x = float.Parse(ase.token);
                ASE_GetToken(false); ase.currentObject.mesh.transform[1].y = float.Parse(ase.token);
                ASE_GetToken(false); ase.currentObject.mesh.transform[1].z = float.Parse(ase.token);
            }
            else if (token == "*TM_ROW2")
            {
                ASE_GetToken(false); ase.currentObject.mesh.transform[2].x = float.Parse(ase.token);
                ASE_GetToken(false); ase.currentObject.mesh.transform[2].y = float.Parse(ase.token);
                ASE_GetToken(false); ase.currentObject.mesh.transform[2].z = float.Parse(ase.token);
            }
            else if (token == "*TM_ROW3")
            {
                ASE_GetToken(false); ase.currentObject.mesh.transform[3].x = float.Parse(ase.token);
                ASE_GetToken(false); ase.currentObject.mesh.transform[3].y = float.Parse(ase.token);
                ASE_GetToken(false); ase.currentObject.mesh.transform[3].z = float.Parse(ase.token);
            }
        }

        static void ASE_KeyMESH_VERTEX_LIST(string token)
        {
            var pMesh = ASE_GetCurrentMesh();

            if (token == "*MESH_VERTEX")
            {
                ASE_GetToken(false);        // skip number
                ASE_GetToken(false); pMesh.vertexes[ase.currentVertex].x = float.Parse(ase.token);
                ASE_GetToken(false); pMesh.vertexes[ase.currentVertex].y = float.Parse(ase.token);
                ASE_GetToken(false); pMesh.vertexes[ase.currentVertex].z = float.Parse(ase.token);

                ase.currentVertex++;
                if (ase.currentVertex > pMesh.numVertexes) common.Error("ase.currentVertex >= pMesh.numVertexes");
            }
            else common.Error($"Unknown token '{token}' while parsing MESH_VERTEX_LIST");
        }

        static void ASE_KeyMESH_FACE_LIST(string token)
        {
            var pMesh = ASE_GetCurrentMesh();

            if (token == "*MESH_FACE")
            {
                ASE_GetToken(false);    // skip face number

                // we are flipping the order here to change the front/back facing from 3DS to our standard (clockwise facing out)
                ASE_GetToken(false);    // skip label
                ASE_GetToken(false); pMesh.faces[ase.currentFace].vertexNum[0] = (int)floatX.Parse(ase.token);

                ASE_GetToken(false);    // skip label
                ASE_GetToken(false); pMesh.faces[ase.currentFace].vertexNum[2] = (int)floatX.Parse(ase.token);

                ASE_GetToken(false);    // skip label
                ASE_GetToken(false); pMesh.faces[ase.currentFace].vertexNum[1] = (int)floatX.Parse(ase.token);

                ASE_GetToken(true);

                // we could parse material id and smoothing groups here
                //if ((p = strstr(ase.token, "*MESH_MTLID")) != 0)
                //{
                //    p += "*MESH_MTLID".Length + 1;
                //    mtlID = int.Parse(p);
                //}
                //else common.Error("No *MESH_MTLID found for face!");

                ase.currentFace++;
            }
            else common.Error($"Unknown token '{token}' while parsing MESH_FACE_LIST");
        }

        static void ASE_KeyTFACE_LIST(string token)
        {
            var pMesh = ASE_GetCurrentMesh();

            if (token == "*MESH_TFACE")
            {
                ASE_GetToken(false);
                ASE_GetToken(false); pMesh.faces[ase.currentFace].tVertexNum[0] = int.Parse(ase.token);
                ASE_GetToken(false); pMesh.faces[ase.currentFace].tVertexNum[1] = int.Parse(ase.token);
                ASE_GetToken(false); pMesh.faces[ase.currentFace].tVertexNum[2] = int.Parse(ase.token);

                ase.currentFace++;
            }
            else common.Error($"Unknown token '{token}' in MESH_TFACE");
        }

        static int[] ASE_KeyCFACE_LIST_remap = { 0, 2, 1 };
        static void ASE_KeyCFACE_LIST(string token)
        {
            var pMesh = ASE_GetCurrentMesh();

            if (token == "*MESH_CFACE")
            {
                ASE_GetToken(false);

                for (var i = 0; i < 3; i++)
                {
                    ASE_GetToken(false);
                    var a = int.Parse(ase.token);

                    // we flip the vertex order to change the face direction to our style
                    pMesh.faces[ase.currentFace].vertexColors[ASE_KeyCFACE_LIST_remap[i]][0] = (byte)(pMesh.cvertexes[a].x * 255);
                    pMesh.faces[ase.currentFace].vertexColors[ASE_KeyCFACE_LIST_remap[i]][1] = (byte)(pMesh.cvertexes[a].y * 255);
                    pMesh.faces[ase.currentFace].vertexColors[ASE_KeyCFACE_LIST_remap[i]][2] = (byte)(pMesh.cvertexes[a].z * 255);
                }

                ase.currentFace++;
            }
            else common.Error($"Unknown token '{token}' in MESH_CFACE");
        }

        static void ASE_KeyMESH_TVERTLIST(string token)
        {
            var pMesh = ASE_GetCurrentMesh();

            if (token == "*MESH_TVERT")
            {
                ASE_GetToken(false);
                ASE_GetToken(false); var u = ase.token;
                ASE_GetToken(false); var v = ase.token;
                ASE_GetToken(false); var w = ase.token;

                pMesh.tvertexes[ase.currentVertex].x = float.Parse(u);
                // our OpenGL second texture axis is inverted from MAX's sense
                pMesh.tvertexes[ase.currentVertex].y = 1.0f - float.Parse(v);

                ase.currentVertex++;

                if (ase.currentVertex > pMesh.numTVertexes) common.Error("ase.currentVertex > pMesh.numTVertexes");
            }
            else common.Error($"Unknown token '{token}' while parsing MESH_TVERTLIST");
        }

        static void ASE_KeyMESH_CVERTLIST(string token)
        {
            var pMesh = ASE_GetCurrentMesh();

            pMesh.colorsParsed = true;

            if (token == "*MESH_VERTCOL")
            {
                ASE_GetToken(false);
                ASE_GetToken(false); pMesh.cvertexes[ase.currentVertex][0] = float.Parse(token);
                ASE_GetToken(false); pMesh.cvertexes[ase.currentVertex][1] = float.Parse(token);
                ASE_GetToken(false); pMesh.cvertexes[ase.currentVertex][2] = float.Parse(token);

                ase.currentVertex++;

                if (ase.currentVertex > pMesh.numCVertexes) common.Error("ase.currentVertex > pMesh.numCVertexes");
            }
            else common.Error($"Unknown token '{token}' while parsing MESH_CVERTLIST");
        }

        static void ASE_KeyMESH_NORMALS(string token)
        {
            AseFace f; Vector3 n;

            var pMesh = ASE_GetCurrentMesh();

            pMesh.normalsParsed = true;
            f = pMesh.faces[ase.currentFace];

            if (token == "*MESH_FACENORMAL")
            {
                ASE_GetToken(false); var num = int.Parse(ase.token);

                if (num >= pMesh.numFaces || num < 0) common.Error($"MESH_NORMALS face index out of range: {num}");
                if (num != ase.currentFace) common.Error("MESH_NORMALS face index != currentFace");

                ASE_GetToken(false); n.x = float.Parse(ase.token);
                ASE_GetToken(false); n.y = float.Parse(ase.token);
                ASE_GetToken(false); n.z = float.Parse(ase.token);

                f.faceNormal.x = n.x * pMesh.transform[0].x + n.y * pMesh.transform[1].x + n.z * pMesh.transform[2].x;
                f.faceNormal.y = n.x * pMesh.transform[0].y + n.y * pMesh.transform[1].y + n.z * pMesh.transform[2].y;
                f.faceNormal.z = n.x * pMesh.transform[0].z + n.y * pMesh.transform[1].z + n.z * pMesh.transform[2].z;

                f.faceNormal.Normalize();

                ase.currentFace++;
            }
            else if (token == "*MESH_VERTEXNORMAL")
            {
                ASE_GetToken(false); var num = int.Parse(ase.token);

                if (num >= pMesh.numVertexes || num < 0) common.Error($"MESH_NORMALS vertex index out of range: {num}");

                f = pMesh.faces[ase.currentFace - 1];

                int v;
                for (v = 0; v < 3; v++) if (num == f.vertexNum[v]) break;

                if (v == 3) common.Error("MESH_NORMALS vertex index doesn't match face");

                ASE_GetToken(false); n.x = float.Parse(ase.token);
                ASE_GetToken(false); n.y = float.Parse(ase.token);
                ASE_GetToken(false); n.z = float.Parse(ase.token);

                f.vertexNormals[v].x = n.x * pMesh.transform[0].x + n.y * pMesh.transform[1].x + n.z * pMesh.transform[2].x;
                f.vertexNormals[v].y = n.x * pMesh.transform[0].y + n.y * pMesh.transform[1].y + n.z * pMesh.transform[2].y;
                f.vertexNormals[v].z = n.x * pMesh.transform[0].z + n.y * pMesh.transform[1].z + n.z * pMesh.transform[2].z;

                f.vertexNormals[v].Normalize();
            }
        }

        static void ASE_KeyMESH(string token)
        {
            var pMesh = ASE_GetCurrentMesh();

            if (token == "*TIMEVALUE") { ASE_GetToken(false); pMesh.timeValue = int.Parse(ase.token); VERBOSE($".....timevalue: {pMesh.timeValue}\n"); }
            else if (token == "*MESH_NUMVERTEX") { ASE_GetToken(false); pMesh.numVertexes = int.Parse(ase.token); VERBOSE($".....num vertexes: {pMesh.numVertexes}\n"); }
            else if (token == "*MESH_NUMTVERTEX") { ASE_GetToken(false); pMesh.numTVertexes = int.Parse(ase.token); VERBOSE($".....num tvertexes: {pMesh.numTVertexes}\n"); }
            else if (token == "*MESH_NUMCVERTEX") { ASE_GetToken(false); pMesh.numCVertexes = int.Parse(ase.token); VERBOSE($".....num cvertexes: {pMesh.numCVertexes}\n"); }
            else if (token == "*MESH_NUMFACES") { ASE_GetToken(false); pMesh.numFaces = int.Parse(ase.token); VERBOSE($".....num faces: {pMesh.numFaces}\n"); }
            else if (token == "*MESH_NUMTVFACES") { ASE_GetToken(false); pMesh.numTVFaces = int.Parse(ase.token); VERBOSE($".....num tvfaces: {pMesh.numTVFaces}\n"); if (pMesh.numTVFaces != pMesh.numFaces) common.Error("MESH_NUMTVFACES != MESH_NUMFACES"); }
            else if (token == "*MESH_NUMCVFACES") { ASE_GetToken(false); pMesh.numCVFaces = int.Parse(ase.token); VERBOSE($".....num cvfaces: {pMesh.numCVFaces}\n"); if (pMesh.numTVFaces != pMesh.numFaces) common.Error("MESH_NUMCVFACES != MESH_NUMFACES"); }
            else if (token == "*MESH_VERTEX_LIST") { ase.currentVertex = 0; pMesh.vertexes = new Vector3[pMesh.numVertexes]; VERBOSE(".....parsing MESH_VERTEX_LIST\n"); ASE_ParseBracedBlock(ASE_KeyMESH_VERTEX_LIST); }
            else if (token == "*MESH_TVERTLIST") { ase.currentVertex = 0; pMesh.tvertexes = new Vector2[pMesh.numTVertexes]; VERBOSE(".....parsing MESH_TVERTLIST\n"); ASE_ParseBracedBlock(ASE_KeyMESH_TVERTLIST); }
            else if (token == "*MESH_CVERTLIST") { ase.currentVertex = 0; pMesh.cvertexes = new Vector3[pMesh.numCVertexes]; VERBOSE(".....parsing MESH_CVERTLIST\n"); ASE_ParseBracedBlock(ASE_KeyMESH_CVERTLIST); }
            else if (token == "*MESH_FACE_LIST") { ase.currentFace = 0; pMesh.faces = new AseFace[pMesh.numFaces]; VERBOSE(".....parsing MESH_FACE_LIST\n"); ASE_ParseBracedBlock(ASE_KeyMESH_FACE_LIST); }
            else if (token == "*MESH_TFACELIST") { if (pMesh.faces == null) common.Error("*MESH_TFACELIST before *MESH_FACE_LIST"); ase.currentFace = 0; VERBOSE(".....parsing MESH_TFACE_LIST\n"); ASE_ParseBracedBlock(ASE_KeyTFACE_LIST); }
            else if (token == "*MESH_CFACELIST") { if (pMesh.faces == null) common.Error("*MESH_CFACELIST before *MESH_FACE_LIST"); ase.currentFace = 0; VERBOSE(".....parsing MESH_CFACE_LIST\n"); ASE_ParseBracedBlock(ASE_KeyCFACE_LIST); }
            else if (token == "*MESH_NORMALS") { if (pMesh.faces == null) common.Warning("*MESH_NORMALS before *MESH_FACE_LIST"); ase.currentFace = 0; VERBOSE(".....parsing MESH_NORMALS\n"); ASE_ParseBracedBlock(ASE_KeyMESH_NORMALS); }
        }

        static void ASE_KeyMESH_ANIMATION(string token)
        {
            // loads a single animation frame
            if (token == "*MESH") { VERBOSE("...found MESH\n"); ase.currentObject.frames.Add(ase.currentMesh = new AseMesh()); ASE_ParseBracedBlock(ASE_KeyMESH); }
            else common.Error($"Unknown token '{token}' while parsing MESH_ANIMATION");
        }

        static void ASE_KeyGEOMOBJECT(string token)
        {
            var obj = ase.currentObject;

            if (token == "*NODE_NAME") { ASE_GetToken(true); obj.name = ase.token; VERBOSE($" {ase.token}\n"); }
            else if (token == "*NODE_PARENT") { ASE_SkipRestOfLine(); }
            // ignore unused data blocks
            else if (token == "*NODE_TM" || token == "*TM_ANIMATION") { ASE_ParseBracedBlock(ASE_KeyNODE_TM); }
            // ignore regular meshes that aren't part of animation
            else if (token == "*MESH") { ase.currentMesh = ase.currentObject.mesh; ASE_ParseBracedBlock(ASE_KeyMESH); }
            // according to spec these are obsolete
            else if (token == "*MATERIAL_REF") { ASE_GetToken(false); obj.materialRef = int.Parse(ase.token); }
            // loads a sequence of animation frames
            else if (token == "*MESH_ANIMATION") { VERBOSE("..found MESH_ANIMATION\n"); ASE_ParseBracedBlock(ASE_KeyMESH_ANIMATION); }
            // skip unused info
            else if (token == "*PROP_MOTIONBLUR" || token == "*PROP_CASTSHADOW" || token == "*PROP_RECVSHADOW") { ASE_SkipRestOfLine(); }
        }

        static void ASE_ParseGeomObject()
        {
            VERBOSE("GEOMOBJECT");
            AseObject obj;
            ase.model.objects.Add(obj = new AseObject());
            ase.currentObject = obj;
            obj.frames.Resize(32, 32);
            ASE_ParseBracedBlock(ASE_KeyGEOMOBJECT);
        }

        static void ASE_KeyGROUP(string token)
        {
            if (token == "*GEOMOBJECT") { ASE_ParseGeomObject(); }
        }

        public static AseModel ASE_Parse(byte[] buffer, bool verbose)
        {
            ase = new Ase
            {
                verbose = verbose,

                buffer = buffer,
                len = buffer.Length,
                curpos = 0,
                currentObject = null,

                model = new AseModel(),
            };
            ase.model.objects.Resize(32, 32);
            ase.model.materials.Resize(32, 32);

            while (ASE_GetToken(false))
            {
                if (ase.token == "*3DSMAX_ASCIIEXPORT" || ase.token == "*COMMENT") { ASE_SkipRestOfLine(); }
                else if (ase.token == "*SCENE") { ASE_SkipEnclosingBraces(); }
                else if (ase.token == "*GROUP") { ASE_GetToken(false); ASE_ParseBracedBlock(ASE_KeyGROUP); } // group name
                else if (ase.token == "*SHAPEOBJECT") { ASE_SkipEnclosingBraces(); }
                else if (ase.token == "*CAMERAOBJECT") { ASE_SkipEnclosingBraces(); }
                else if (ase.token == "*MATERIAL_LIST") { VERBOSE("MATERIAL_LIST\n"); ASE_ParseBracedBlock(ASE_KeyMATERIAL_LIST); }
                else if (ase.token == "*GEOMOBJECT") { ASE_ParseGeomObject(); }
                else if (!string.IsNullOrEmpty(ase.token)) { common.Printf($"Unknown token '{ase.token}'\n"); }
            }

            return ase.model;
        }
    
        public static AseModel ASE_Load(string fileName)
        {
            fileSystem.ReadFile(fileName, out var buf, out var timeStamp);
            if (buf == null) return null;

            var ase = ModelXAse.ASE_Parse(buf, false);
            ase.timeStamp = timeStamp;
            fileSystem.FreeFile(buf);

            return ase;
        }

        public static void ASE_Free(AseModel ase)
        {
            if (ase == null) return;
            ase.objects.Clear();
            ase.materials.Clear();
        }
    }
}
