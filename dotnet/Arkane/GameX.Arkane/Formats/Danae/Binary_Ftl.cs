using GameX.Formats;
using GameX.Meta;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameX.Arkane.Formats.Danae
{
    public unsafe class Binary_Ftl : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Ftl(r));

        #region FTL Headers

        const int FTL_MAGIC = 0x004c5446;
        const float FTL_VERSION = 0.83257f;

        [StructLayout(LayoutKind.Sequential)]
        struct FTL_HEADER
        {
            public static (string, int) Struct = ("<6i", sizeof(FTL_HEADER));
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
            public static (string, int) Struct = ("<i", sizeof(FTL_PROGRESSIVEHEADER)); 
            public int NumVertex;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct FTL_CLOTHESHEADER
        {
            public static (string, int) Struct = ("<2i", sizeof(FTL_CLOTHESHEADER));
            public int NumCvert;
            public int NumSprings;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct FTL_COLLISIONSPHERESHEADER
        {
            public static (string, int) Struct = ("<i", sizeof(FTL_COLLISIONSPHERESHEADER)); 
            public int NumSpheres;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct FTL_3DHEADER
        {
            public static (string, int) Struct = ("<7i256s", 28 + 256);
            public int NumVertex;
            public int NumFaces;
            public int NumMaps;
            public int NumGroups;
            public int NumAction;
            public int NumSelections;
            public int Origin;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string Name;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct FTL_VERTEX
        {
            public static (string, int) Struct = ($"<{"4f2I3f"}f3f3", sizeof(FTL_VERTEX));
            public TLVERTEX Vert;
            public Vector3 V;
            public Vector3 Norm;
            public static implicit operator E_VERTEX(FTL_VERTEX s)
                => new E_VERTEX
                {
                    Vert = s.Vert,
                    V = s.V,
                    Norm = s.Norm,
                    VWorld = default,
                };
        }

        [StructLayout(LayoutKind.Sequential)]
        struct FTL_TEXTURE
        {
            public static (string, int) Struct = ("<256s", 256);
            public const int SizeOf = 256;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string Name;
            public static implicit operator E_TEXTURE(FTL_TEXTURE s)
            {
                var name = s.Name;
                POLY poly = 0;
                if (name.Contains("NPC_")) poly |= POLY.LATE_MIP;
                if (name.Contains("nocol")) poly |= POLY.NOCOL;
                if (name.Contains("climb")) poly |= POLY.CLIMB; // change string depending on GFX guys
                if (name.Contains("fall")) poly |= POLY.FALL;
                if (name.Contains("lava")) poly |= POLY.LAVA;
                if (name.Contains("water")) poly |= POLY.WATER | POLY.TRANS;
                else if (name.Contains("spider_web")) poly |= POLY.WATER | POLY.TRANS;
                else if (name.Contains("[metal]")) poly |= POLY.METAL;
                return new E_TEXTURE
                {
                    Path = s.Name,
                    Poly = poly,
                };
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct FTL_FACE
        {
            public static (string, int) Struct = ("<?", sizeof(FTL_FACE));
            public int FaceType;  // 0 = flat, 1 = text, 2 = Double-Side
            public Vector3<int> Rgb;
            public Vector3<ushort> Vid;
            public short TexId;
            public Vector3 U;
            public Vector3 V;
            public Vector3<short> Ou;
            public Vector3<short> Ov;
            public float TransVal;
            public Vector3 Norm;
            public Vector3 Nrmls0; public Vector3 Nrmls1; public Vector3 Nrmls2;
            public float Temp;
            public static implicit operator E_FACE(FTL_FACE s)
                => new E_FACE
                {
                    FaceType = s.FaceType,
                    TexId = s.TexId,
                    U = s.U,
                    V = s.V,
                    Ou = s.Ou,
                    Ov = s.Ov,
                    TransVal = s.TransVal,
                    Norm = s.Norm,
                    Nrmls = new[] { s.Nrmls0, s.Nrmls1, s.Nrmls2 },
                    Temp = s.Temp,
                };
        }

        [StructLayout(LayoutKind.Sequential)]
        struct FTL_GROUPLIST
        {
            public static (string, int) Struct = ("<256s3if", 256 + 16);
            public const int SizeOf = 256 + 16;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string Name;
            public int Origin;
            public int NumIndex;
            public int Trash; // Indexes;
            public float Size;
            public static implicit operator E_GROUPLIST(FTL_GROUPLIST s)
                => new E_GROUPLIST
                {
                    Name = s.Name,
                    Origin = s.Origin,
                    NumIndex = s.NumIndex,
                    Size = s.Size,
                };
        }

        [StructLayout(LayoutKind.Sequential)]
        struct FTL_ACTIONLIST
        {
            public static (string, int) Struct = ("<256s3i", 256 + 12);
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string Name;
            public int Idx; //index vertex;
            public int Act; //action
            public int Sfx; //sfx
            public static implicit operator E_ACTIONLIST(FTL_ACTIONLIST s)
                => new E_ACTIONLIST
                {
                    Name = s.Name,
                    Idx = s.Idx,
                    Act = s.Act,
                    Sfx = s.Sfx,
                };
        }

        [StructLayout(LayoutKind.Sequential)]
        struct FTL_SELECTIONS
        {
            public static (string, int) Struct = ("<64s2i", 64 + 8);
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)] string Name;
            public int NumSelected;
            public int Trash; //Selected;
            public static implicit operator E_SELECTIONS(FTL_SELECTIONS s)
                => new E_SELECTIONS
                {
                    Name = s.Name,
                    NumSelected = s.NumSelected,
                };
        }

        #endregion

        public E_3DOBJ Obj;

        // https://github.com/OpenSourcedGames/Arx-Fatalis/blob/master/Sources/DANAE/ARX_FTL.cpp#L575
        public Binary_Ftl(BinaryReader r)
        {
            Obj = new E_3DOBJ();
            var magic = r.ReadUInt32();
            if (magic != FTL_MAGIC) throw new FormatException($"Invalid FTL magic: \"{magic}\".");
            var version = r.ReadSingle();
            if (version != FTL_VERSION) throw new FormatException($"Invalid FLT version: \"{version}\".");
            r.Skip(512); // skip checksum
            var header = r.ReadS<FTL_HEADER>();

            // Check For & Load 3D Data
            if (header.Offset3Ddata != -1)
            {
                r.Seek(header.Offset3Ddata);
                var _3Dh = r.ReadS<FTL_3DHEADER>();
                Obj.NumVertex = _3Dh.NumVertex;
                Obj.NumFaces = _3Dh.NumFaces;
                Obj.NumMaps = _3Dh.NumMaps;
                Obj.NumGroups = _3Dh.NumGroups;
                Obj.NumAction = _3Dh.NumAction;
                Obj.NumSelections = _3Dh.NumSelections;
                Obj.Origin = _3Dh.Origin;
                Obj.File = _3Dh.Name;

                // Alloc'n'Copy vertices
                if (_3Dh.NumVertex > 0)
                {
                    var vertexList = r.ReadTArray<FTL_VERTEX>(sizeof(FTL_VERTEX), _3Dh.NumVertex);
                    Obj.VertexList = new E_VERTEX[_3Dh.NumVertex];
                    for (var i = 0; i < Obj.VertexList.Length; i++)
                    {
                        Obj.VertexList[i] = vertexList[i];
                        Obj.VertexList[i].Vert.Color = 0xFF000000;
                    }
                    Obj.Point0 = Obj.VertexList[Obj.Origin].V;
                }

                // Alloc'n'Copy faces
                if (_3Dh.NumFaces > 0)
                {
                    var faceList = r.ReadTArray<FTL_FACE>(sizeof(FTL_FACE), _3Dh.NumFaces);
                    Obj.FaceList = new E_FACE[_3Dh.NumFaces];
                    for (var i = 0; i < Obj.FaceList.Length; i++)
                        Obj.FaceList[i] = faceList[i];
                }

                // Alloc'n'Copy textures
                if (_3Dh.NumMaps > 0)
                {
                    var textures = r.ReadTEach<FTL_TEXTURE>(FTL_TEXTURE.SizeOf, _3Dh.NumMaps);
                    Obj.Textures = new E_TEXTURE[_3Dh.NumMaps];
                    for (var i = 0; i < Obj.Textures.Length; i++)
                        Obj.Textures[i] = textures[i];
                }

                // Alloc'n'Copy groups
                if (_3Dh.NumGroups > 0)
                {
                    var groupList = r.ReadTEach<FTL_GROUPLIST>(FTL_GROUPLIST.SizeOf, _3Dh.NumGroups);
                    Obj.GroupList = new E_GROUPLIST[_3Dh.NumGroups];
                    for (var i = 0; i < Obj.GroupList.Length; i++)
                    {
                        Obj.GroupList[i] = groupList[i];
                        if (Obj.GroupList[i].NumIndex > 0) Obj.GroupList[i].Indexes = r.ReadTArray<int>(sizeof(int), Obj.GroupList[i].NumIndex);
                    }
                }

                // Alloc'n'Copy action points
                if (_3Dh.NumAction > 0)
                {
                    var actionList = r.ReadTEach<FTL_ACTIONLIST>(FTL_ACTIONLIST.Struct.Item2, _3Dh.NumAction);
                    Obj.ActionList = new E_ACTIONLIST[_3Dh.NumAction];
                    for (var i = 0; i < Obj.ActionList.Length; i++)
                        Obj.ActionList[i] = actionList[i];
                }

                // Alloc'n'Copy selections
                if (_3Dh.NumSelections > 0)
                {
                    var selections = r.ReadFArray(x => r.ReadS<FTL_SELECTIONS>(), _3Dh.NumSelections);
                    Obj.Selections = new E_SELECTIONS[_3Dh.NumSelections];
                    for (var i = 0; i < Obj.Selections.Length; i++)
                    {
                        Obj.Selections[i] = selections[i];
                        Obj.Selections[i].Selected = r.ReadTArray<int>(sizeof(int), Obj.Selections[i].NumSelected);
                    }
                }
            }

            // Alloc'n'Copy Collision Spheres Data
            if (header.OffsetCollisionSpheres != -1)
            {
                r.Seek(header.OffsetCollisionSpheres);
                var csh = r.ReadS<FTL_COLLISIONSPHERESHEADER>();
                Obj.Sdata = new COLLISION_SPHERES_DATA
                {
                    NumSpheres = csh.NumSpheres,
                    Spheres = r.ReadTArray<COLLISION_SPHERE>(sizeof(COLLISION_SPHERE), csh.NumSpheres),
                };
            }

            // Alloc'n'Copy Progressive DATA
            if (header.OffsetProgressiveData != -1)
            {
                r.Seek(header.OffsetProgressiveData);
                var ph = r.ReadS<FTL_PROGRESSIVEHEADER>();
                r.Skip(sizeof(PROGRESSIVE_DATA) * ph.NumVertex);
            }

            // Alloc'n'Copy Clothes DATA
            if (header.OffsetClothesData != -1)
            {
                r.Seek(header.OffsetClothesData);
                var ch = r.ReadS<FTL_CLOTHESHEADER>();
                Obj.Cdata = new CLOTHES_DATA
                {
                    NumCvert = (short)ch.NumCvert,
                    NumSprings = (short)ch.NumSprings,
                    Cvert = r.ReadTArray<CLOTHESVERTEX>(sizeof(CLOTHESVERTEX), ch.NumCvert),
                    Springs = r.ReadTArray<E_SPRINGS>(sizeof(E_SPRINGS), ch.NumSprings),
                };
            }
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo("BinaryFTL", items: new List<MetaInfo> {
                    new MetaInfo($"Obj: {Obj}"),
                })
            };
            return nodes;
        }
    }
}
