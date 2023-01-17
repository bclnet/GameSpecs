using GameSpec.Formats;
using GameSpec.Metadata;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameSpec.Arkane.Formats
{
    public class BinaryFtl : IGetMetadataInfo
    {
        public static Task<object> Factory(BinaryReader r, FileMetadata f, PakFile s) => Task.FromResult((object)new BinaryFtl(r));

        public BinaryFtl() { }
        public BinaryFtl(BinaryReader r) => Read(r);

        const int FTL_MAGIC = 0x004c5446;
        const float FTL_VERSION = 0.83257f;

        [StructLayout(LayoutKind.Sequential)]
        struct FTL_HEADER
        {
            public int Offset3Ddata;                // -1 = no
            public int OffsetCylinder;              // -1 = no
            public int OffsetProgressiveData;       // -1 = no
            public int OffsetClothesData;           // -1 = no
            public int OffsetCollisionSpheres;      // -1 = no
            public int OffsetPhysicsBox;            // -1 = no
        }

        [StructLayout(LayoutKind.Sequential)]
        struct FTL_PROGRESSIVEHEADER
        {
            public int NumVertex;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct FTL_CLOTHESHEADER
        {
            public int NumCvert;
            public int NumSprings;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct FTL_COLLISIONSPHERESHEADER
        {
            public int NumSpheres;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct FTL_3DHEADER
        {
            public const int SizeOf = 28 + 256;
            public int NumVertex;
            public int NumFaces;
            public int NumMaps;
            public int NumGroups;
            public int NumAction;
            public int NumSelections;
            public int Origin;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string Name;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct FTL_VERTEX // Aligned 1 2 4
        {
            public Vector2 Vert;
            public Vector3 V;
            public Vector3 Norm;

            public static implicit operator E_VERTEX(FTL_VERTEX old)
                => new E_VERTEX
                {
                    Vert = old.Vert,
                    V = old.V,
                    Norm = old.Norm,
                    VWorld = default,
                };
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public unsafe struct FTL_FACE // Aligned 1 2 4
        {
            public int FaceType;  // 0 = flat, 1 = text, 2 = Double-Side
            public Vector2 Rgb0; public Vector2 Rgb1; public Vector2 Rgb2;
            public fixed ushort Vid[3];
            public short TexId;
            public fixed float U[3];
            public fixed float V[3];
            public fixed short Ou[3];
            public fixed short Ov[3];
            public float TransVal;
            public Vector3 Norm;
            public Vector3 Nrmls0; public Vector3 Nrmls1; public Vector3 Nrmls2;
            public float Temp;
        }

        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo("BinaryFLT", items: new List<MetadataInfo> {
                    //new MetadataInfo($"Type: {Type}"),
                })
            };
            return nodes;
        }

        // https://github.com/OpenSourcedGames/Arx-Fatalis/blob/master/Sources/DANAE/ARX_FTL.cpp#L575
        public unsafe void Read(BinaryReader r)
        {
            var magic = r.ReadUInt32();
            if (magic != FTL_MAGIC) throw new FormatException($"Invalid FTL magic: \"{magic}\".");
            var version = r.ReadSingle();
            if (version != FTL_VERSION) throw new FormatException($"Invalid FLT version: \"{version}\".");
            r.Skip(512); // skip checksum
            var header = r.ReadT<FTL_HEADER>(sizeof(FTL_HEADER));
            var obj = new E_3DOBJ();

            // Check For & Load 3D Data
            if (header.Offset3Ddata != -1)
            {
                r.Seek(header.Offset3Ddata);
                var _3Dh = r.ReadT<FTL_3DHEADER>(FTL_3DHEADER.SizeOf);
                obj.NumVertex = _3Dh.NumVertex;
                obj.NumFaces = _3Dh.NumFaces;
                obj.NumMaps = _3Dh.NumMaps;
                obj.NumGroups = _3Dh.NumGroups;
                obj.NumAction = _3Dh.NumAction;
                obj.NumSelections = _3Dh.NumSelections;
                obj.Origin = _3Dh.Origin;
                obj.File = _3Dh.Name;

                // Alloc'n'Copy vertices
                if (obj.NumVertex > 0)
                {
                    var vertexList = r.ReadTArray<FTL_VERTEX>(sizeof(FTL_VERTEX), obj.NumVertex);
                    obj.VertexList = new E_VERTEX[obj.NumVertex];
                    for (var i = 0; i < obj.NumVertex; i++) obj.VertexList[i] = vertexList[i];
                    obj.Point0 = obj.VertexList[obj.Origin].V;
                    for (var i = 0; i < obj.NumVertex; i++)
                        obj.VertexList[i].Vert.X = 0xFF000000;
                }

                // Alloc'n'Copy faces
                if (obj.NumFaces > 0)
                {
                    obj.FaceList = new E_FACE[obj.NumFaces];
                    for (var i = 0; i < obj.NumFaces; i++)
                    {
                        var f = r.ReadT<FTL_FACE>(sizeof(FTL_FACE));
                        obj.FaceList[i].FaceType = f.FaceType;
                        obj.FaceList[i].TexId = f.TexId;
                        obj.FaceList[i].TransVal = f.TransVal;
                        obj.FaceList[i].Temp = f.Temp;
                        obj.FaceList[i].Norm = f.Norm;
                        obj.FaceList[i].Nrmls = new[] { f.Nrmls0, f.Nrmls1, f.Nrmls2 };
                        obj.FaceList[i].Vid = new ushort[3];
                        obj.FaceList[i].U = new float[3];
                        obj.FaceList[i].V = new float[3];
                        obj.FaceList[i].Ou = new short[3];
                        obj.FaceList[i].Ov = new short[3];
                        for (var k = 0; k < 3; k++)
                        {
                            obj.FaceList[i].Vid[k] = f.Vid[k];
                            obj.FaceList[i].U[k] = f.U[k];
                            obj.FaceList[i].V[k] = f.V[k];
                            obj.FaceList[i].Ou[k] = f.Ou[k];
                            obj.FaceList[i].Ov[k] = f.Ov[k];
                        }
                    }
                }

                // Alloc'n'Copy textures
                //if (_3Dh.NumMaps > 0)
                //{
                //    char ficc[256];
                //    char ficc2[256];
                //    char ficc3[256];

                //    //todo free
                //    obj.texturecontainer = (TextureContainer**)malloc(sizeof(TextureContainer*) * af3Ddh.nb_maps);

                //    for (long i = 0; i < af3Ddh.nb_maps; i++)
                //    {
                //        memcpy(ficc2, dat + pos, 256);
                //        strcpy(ficc3, Project.workingdir);
                //        strcat(ficc3, ficc2);
                //        File_Standardize(ficc3, ficc);

                //        obj.texturecontainer[i] = D3DTextr_CreateTextureFromFile(ficc, Project.workingdir, 0, 0, EERIETEXTUREFLAG_LOADSCENE_RELEASE);

                //        if (GDevice && obj.texturecontainer[i] && !obj.texturecontainer[i].m_pddsSurface)
                //            obj.texturecontainer[i].Restore(GDevice);

                //        MakeUserFlag(obj.texturecontainer[i]);
                //        pos += 256;
                //    }
                //}

                // Alloc'n'Copy groups
                //if (obj.NumGroups > 0)
                //{
                //    obj.GroupList = r.ReadTArray<E_GROUPLIST>(E_GROUPLIST.SizeOf, obj.NumGroups);
                //    for (var i = 0; i < obj.NumGroups; i++)
                //        if (obj.NumGroups[i].NumIndex > 0)
                //        {
                //            //TO DO: FREE+++++++++++++++++++++++
                //            obj.grouplist[i].indexes = (long*)malloc(sizeof(long) * obj.grouplist[i].nb_index);
                //            memcpy(obj.grouplist[i].indexes, dat + pos, sizeof(long) * obj.grouplist[i].nb_index);
                //            pos += sizeof(long) * obj.grouplist[i].nb_index;
                //        }
                //}

                // Alloc'n'Copy action points
                //if (obj.NumAction > 0)
                //{
                //    obj.ActionList = r.ReadTArray<E_ACTIONLIST>(E_ACTIONLIST.SizeOf, obj.NumAction);
                //}

                // Alloc'n'Copy selections
                //if (obj.NumSelections > 0)
                //{
                //    obj.Selections = r.ReadTArray(x => r.ReadT<E_SELECTIONS>(E_SELECTIONS.SizeOf), obj.NumSelections);
                //    for (var i = 0; i < obj.NumSelections; i++)
                //        obj.Selections[i].Selected = r.ReadTArray<int>(sizeof(int), obj.Selections[i].NumSelected);
                //}

                //obj.Pbox = null;
            }

            // Alloc'n'Copy Collision Spheres Data
            if (header.OffsetCollisionSpheres != -1)
            {
                r.Seek(header.OffsetCollisionSpheres);
                var csh = r.ReadT<FTL_COLLISIONSPHERESHEADER>(sizeof(FTL_COLLISIONSPHERESHEADER));
                obj.Sdata = new COLLISION_SPHERES_DATA
                {
                    NumSpheres = csh.NumSpheres,
                    Spheres = r.ReadTArray<COLLISION_SPHERE>(sizeof(COLLISION_SPHERE), csh.NumSpheres),
                };
            }

            // Alloc'n'Copy Progressive DATA
            if (header.OffsetProgressiveData != -1)
            {
                r.Seek(header.OffsetProgressiveData);
                var ph = r.ReadT<FTL_PROGRESSIVEHEADER>(sizeof(FTL_PROGRESSIVEHEADER));
                obj.Pdata = new PROGRESSIVE_DATA();
            }

            // Alloc'n'Copy Clothes DATA
            if (header.OffsetClothesData != -1)
            {
                r.Seek(header.OffsetClothesData);
            }
        }
    }
}
