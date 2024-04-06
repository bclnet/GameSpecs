using GameX.Formats;
using GameX.Meta;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static OpenStack.Debug;

namespace GameX.Arkane.Formats.Danae
{
    public unsafe class Binary_Fts : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Fts(r));

        #region E Struct

        struct ANCHOR_DATA
        {
            public static (string, int) Struct = ("<?", sizeof(ANCHOR_DATA));
            public Vector3 Pos;
            public short NumLinked;
            public short Flags;
            public int[] Linked;
            public float Radius;
            public float Height;
        }

        public struct E_BKG_INFO
        {
            public static (string, int) Struct = ("<?", sizeof(E_BKG_INFO));
            public byte Treat;
            public bool Nothing;
            public short NumPoly;
            public short NumIAnchors;
            public short NumPolyin;
            public float FrustrumMinY;
            public float FrustrumMaxY;
            public E_POLY[] Polydata;
            //public E_POLY[][] Polyin;
            public int[] IAnchors; // index on anchors list
            public int Flags;
            public float TileMinY;
            public float TileMaxY;
        }

        struct E_SMINMAX
        {
            public static (string, int) Struct = ("<2h", sizeof(E_SMINMAX));
            public short Min;
            public short Max;
        }

        //const int MAX_GOSUB = 10;
        //const int MAX_SHORTCUT = 80;
        //const int MAX_SCRIPTTIMERS = 5;
        //const int FBD_TREAT = 1;
        //const int FBD_NOTHING = 2;

        struct FAST_BKG_DATA
        {
            public static (string, int) Struct = ("<?", sizeof(FAST_BKG_DATA));
            public byte Treat;
            public byte Nothing;
            public short NumPoly;
            public short NumIAnchors;
            public short NumPolyin;
            public int Flags;
            public float FrustrumMinY;
            public float FrustrumMaxY;
            public E_POLY[] Polydata;
            public E_POLY[][] Polyin;
            public int[] IAnchors; // index on anchors list
        }

        const int MAX_BKGX = 160;
        const int MAX_BKGZ = 160;
        const int BKG_SIZX = 100;
        const int BKG_SIZZ = 100;

        class E_BACKGROUND
        {
            public FAST_BKG_DATA[,] fastdata = new FAST_BKG_DATA[MAX_BKGX, MAX_BKGZ];
            public int exist = 1;
            public short XSize;
            public short ZSize;
            public short Xdiv;
            public short Zdiv;
            public float Xmul;
            public float Zmul;
            public E_BKG_INFO[] Backg;
            public Vector3 Ambient;
            public Vector3 Ambient255;
            public E_SMINMAX[] MinMax;
            public int NumAnchors;
            public ANCHOR_DATA[] Anchors;
            public string Name;
            public E_BACKGROUND(short sx = MAX_BKGX, short sz = MAX_BKGZ, short xdiv = BKG_SIZX, short zdiv = BKG_SIZZ)
            {
                XSize = sx;
                ZSize = sz;
                if (xdiv < 0) xdiv = 1;
                if (zdiv < 0) zdiv = 1;
                Xdiv = xdiv;
                Zdiv = zdiv;
                Xmul = 1f / Xdiv;
                Zmul = 1f / Zdiv;
                Backg = new E_BKG_INFO[sx * sz];
                for (var i = 0; i < Backg.Length; i++) Backg[i].Nothing = true;
                MinMax = new E_SMINMAX[sz];
                for (var i = 0; i < MinMax.Length; i++)
                {
                    MinMax[i].Min = 9999;
                    MinMax[i].Max = -1;
                }
            }
        }

        #endregion

        #region FTS Headers

        const float NON_PORTAL_VERSION = 0.136f;
        const float FTS_VERSION = 0.141f;

        [StructLayout(LayoutKind.Sequential)]
        struct FTS_HEADER
        {
            public static (string, int) Struct = ("<256cifi3i", 256 + 24);
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string Path;
            public int Count;
            public float Version;
            public int Compressedsize;
            public fixed int Pad[3];
        }

        [StructLayout(LayoutKind.Sequential)]
        struct FTS_HEADER2
        {
            public static (string, int) Struct = ("<256c", 256);
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string Path;
        }

        const int SIZ_WRK = 10;

        public class FastLevel
        {
            public Vector3 PlayerPos;
            public Vector3 MscenePos;
            public E_TEXTURE[] Textures;
            public E_BKG_INFO[] Backg;
            public E_PORTAL_DATA Portals;
            public int NumRoomDistance;
            public ROOM_DIST_DATA[] RoomDistance;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct FAST_VERTEX
        {
            public static (string, int) Struct = ("<5f", sizeof(FAST_VERTEX));
            public float sy;
            public float ssx;
            public float ssz;
            public float stu;
            public float stv;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct FAST_EERIEPOLY
        {
            public static (string, int) Struct = ("<?", sizeof(FAST_EERIEPOLY));
            public FAST_VERTEX V0; public FAST_VERTEX V1; public FAST_VERTEX V2; public FAST_VERTEX V3;
            public int TexPtr;
            public Vector3 Norm;
            public Vector3 Norm2;
            public Vector3 Nrml0; public Vector3 Nrml1; public Vector3 Nrml2; public Vector3 Nrml3;
            public float Transval;
            public float Area;
            public POLY Type;
            public short Room;
            public short Paddy;
        }

        struct FAST_SCENE_HEADER
        {
            public static (string, int) Struct = ("<f5i6f2i", sizeof(FAST_SCENE_HEADER));
            public float Version;
            public int SizeX;
            public int SizeZ;
            public int NumTextures;
            public int NumPolys;
            public int NumAnchors;
            public Vector3 PlayerPos;
            public Vector3 MscenePos;
            public int NumPortals;
            public int NumRooms;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct FAST_TEXTURE_CONTAINER
        {
            public static (string, int) Struct = ("<2i256c", 8 + 256);
            public int TcPtr;
            public int TempPtr;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string Fic;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct FAST_ANCHOR_DATA
        {
            public static (string, int) Struct = ("<5f2h", sizeof(FAST_ANCHOR_DATA));
            public Vector3 Pos;
            public float Radius;
            public float Height;
            public short NumLinked;
            public short Flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct FAST_SCENE_INFO
        {
            public static (string, int) Struct = ("<2I", sizeof(FAST_SCENE_INFO));
            public int NumPoly;
            public int NumIAnchors;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct ROOM_DIST_DATA_SAVE
        {
            public static (string, int) Struct = ("<7f", sizeof(ROOM_DIST_DATA_SAVE));
            public float Distance; // -1 means use truedist
            public Vector3 StartPos;
            public Vector3 EndPos;
        }

        public struct ROOM_DIST_DATA
        {
            public static (string, int) Struct = ("<7f", sizeof(ROOM_DIST_DATA));
            public float Distance; // -1 means use truedist
            public Vector3 StartPos;
            public Vector3 EndPos;
        }

        #endregion

        public FastLevel Level;
        E_BACKGROUND Bkg;

        // https://github.com/OpenSourcedGames/Arx-Fatalis/blob/master/Sources/EERIE/EERIEPoly.cpp#L3755
        public Binary_Fts(BinaryReader r)
        {
            int i, j, k, kk;
            var header = r.ReadS<FTS_HEADER>();
            if (header.Version != FTS_VERSION) throw new FormatException("BAD MAGIC");
            //Log($"Header1: {r.Position():x}, {header.Path}");
            if (header.Count > 0)
            {
                var count = 0;
                while (count < header.Count)
                {
                    r.ReadS<FTS_HEADER2>();
                    r.Skip(512); // skip check
                    //Log($"Unique[{count}]: {r.Position():x}");
                    count++;
                    if (count > 60) throw new FormatException("BAD HEADER");
                }
            }
            //Log($"Unique: {r.Position():x}");

            Level = new FastLevel();
            Bkg = new E_BACKGROUND();
            var s = new MemoryStream(r.DecompressBlast((int)(r.BaseStream.Length - r.BaseStream.Position), header.Compressedsize));
            using var r2 = new BinaryReader(s);

            // read
            var fsh = r2.ReadS<FAST_SCENE_HEADER>();
            if (fsh.Version != FTS_VERSION) throw new FormatException("BAD MAGIC");
            if (fsh.SizeX != Bkg.XSize) throw new FormatException("BAD HEADER");
            if (fsh.SizeZ != Bkg.ZSize) throw new FormatException("BAD HEADER");
            Level.PlayerPos = fsh.PlayerPos;
            Level.MscenePos = fsh.MscenePos;
            Log($"Header2: {r2.Tell():x}, {sizeof(FAST_SCENE_HEADER)}");

            // textures
            var textures = Level.Textures = new E_TEXTURE[fsh.NumTextures];
            for (k = 0; k < textures.Length; k++)
            {
                var ftc = r2.ReadS<FAST_TEXTURE_CONTAINER>();
                textures[k] = new E_TEXTURE { Id = ftc.TcPtr, Path = ftc.Fic };
            }
            //Log($"Texture: {r2.Position():x}");

            // backg
            var backg = Bkg.Backg;
            for (j = 0; j < fsh.SizeZ; j++)
                for (i = 0; i < fsh.SizeX; i++)
                {
                    ref E_BKG_INFO bi = ref backg[i + j * fsh.SizeX];
                    var fsi = r2.ReadS<FAST_SCENE_INFO>();
                    //if (fsi.NumPoly > 0) Log($"F[{j},{i}]: {r2.Position():x}, {fsi.NumPoly}, {fsi.NumIAnchors}");
                    bi.NumIAnchors = (short)fsi.NumIAnchors;
                    bi.NumPoly = (short)fsi.NumPoly;
                    bi.Polydata = fsi.NumPoly > 0 ? new E_POLY[fsi.NumPoly] : null;
                    bi.Treat = 0;
                    bi.Nothing = fsi.NumPoly == 0;
                    bi.FrustrumMaxY = -99999999f;
                    bi.FrustrumMinY = 99999999f;
                    for (k = 0; k < fsi.NumPoly; k++)
                    {
                        var ep = r2.ReadS<FAST_EERIEPOLY>();
                        var tex = ep.TexPtr != 0
                            ? textures.FirstOrDefault(x => x.Id == ep.TexPtr)
                            : null;
                        ref E_POLY ep2 = ref bi.Polydata[k];
                        ep2.memset();
                        ep2.Room = ep.Room;
                        ep2.Area = ep.Area;
                        ep2.Norm = ep.Norm;
                        ep2.Norm2 = ep.Norm2;
                        ep2.Nrml = new Vector3[] { ep.Nrml0, ep.Nrml1, ep.Nrml2, ep.Nrml3 };
                        ep2.Tex = tex;
                        ep2.TransVal = ep.Transval;
                        ep2.Type = ep.Type;
                        ep2.V = new TLVERTEX[] {
                            new TLVERTEX { Color = 0xFFFFFFFF, Rhw = 1, Specular = 1, S = new Vector3(ep.V0.ssx, ep.V0.sy, ep.V0.ssz), T = new Vector2(ep.V0.stu, ep.V0.stv) },
                            new TLVERTEX { Color = 0xFFFFFFFF, Rhw = 1, Specular = 1, S = new Vector3(ep.V1.ssx, ep.V1.sy, ep.V1.ssz), T = new Vector2(ep.V1.stu, ep.V1.stv) },
                            new TLVERTEX { Color = 0xFFFFFFFF, Rhw = 1, Specular = 1, S = new Vector3(ep.V2.ssx, ep.V2.sy, ep.V2.ssz), T = new Vector2(ep.V2.stu, ep.V2.stv) },
                            new TLVERTEX { Color = 0xFFFFFFFF, Rhw = 1, Specular = 1, S = new Vector3(ep.V3.ssx, ep.V3.sy, ep.V3.ssz), T = new Vector2(ep.V3.stu, ep.V3.stv) },
                        };

                        // clone v
                        ep2.Tv = (TLVERTEX[])ep2.V.Clone();
                        for (kk = 0; kk < 4; kk++) ep2.Tv[kk].Color = 0xFF000000;

                        // re-center
                        int to; float div;
                        if ((ep.Type & POLY.QUAD) != 0) { to = 4; div = 0.25f; }
                        else { to = 3; div = 0.333333333333f; }
                        ep2.Center = Vector3.Zero;
                        for (var h = 0; h < to; h++)
                        {
                            ep2.Center.X += ep2.V[h].S.X;
                            ep2.Center.Y += ep2.V[h].S.Y;
                            ep2.Center.Z += ep2.V[h].S.Z;
                            if (h != 0)
                            {
                                ep2.Max.X = Math.Max(ep2.Max.X, ep2.V[h].S.X);
                                ep2.Min.X = Math.Min(ep2.Min.X, ep2.V[h].S.X);
                                ep2.Max.Y = Math.Max(ep2.Max.Y, ep2.V[h].S.Y);
                                ep2.Min.Y = Math.Min(ep2.Min.Y, ep2.V[h].S.Y);
                                ep2.Max.Z = Math.Max(ep2.Max.Z, ep2.V[h].S.Z);
                                ep2.Min.Z = Math.Min(ep2.Min.Z, ep2.V[h].S.Z);
                            }
                            else
                            {
                                ep2.Min.X = ep2.Max.X = ep2.V[0].S.X;
                                ep2.Min.Y = ep2.Max.Y = ep2.V[0].S.Y;
                                ep2.Min.Z = ep2.Max.Z = ep2.V[0].S.Z;
                            }
                        }
                        ep2.Center.X *= div;
                        ep2.Center.Y *= div;
                        ep2.Center.Z *= div;

                        // distance
                        var dist = 0f; for (var h = 0; h < to; h++) dist = Math.Max(dist, Vector3.Distance(ep2.V[h].S, ep2.Center));
                        ep2.V[0].Rhw = dist;

                        // declare
                        DeclareEGInfo(Bkg, ep2.Center.X, ep2.Center.Y, ep2.Center.Z);
                        DeclareEGInfo(Bkg, ep2.V[0].S.X, ep2.V[0].S.Y, ep2.V[0].S.Z);
                        DeclareEGInfo(Bkg, ep2.V[1].S.X, ep2.V[1].S.Y, ep2.V[1].S.Z);
                        DeclareEGInfo(Bkg, ep2.V[2].S.X, ep2.V[2].S.Y, ep2.V[2].S.Z);
                        if ((ep.Type & POLY.QUAD) != 0) DeclareEGInfo(Bkg, ep2.V[3].S.X, ep2.V[3].S.Y, ep2.V[3].S.Z);
                    }

                    bi.IAnchors = fsi.NumIAnchors <= 0
                        ? null
                        : r2.ReadTArray<int>(sizeof(int), fsi.NumIAnchors);
                }
            //Log($"Background: {r2.Position():x}");

            // anchors
            Bkg.NumAnchors = fsh.NumAnchors;
            var anchors = Bkg.Anchors = fsh.NumAnchors > 0
                ? new ANCHOR_DATA[fsh.NumAnchors]
                : null;
            for (i = 0; i < fsh.NumAnchors; i++)
            {
                ref ANCHOR_DATA a = ref anchors[i];
                var fad = r2.ReadS<FAST_ANCHOR_DATA>();
                a.Flags = fad.Flags;
                a.Pos = fad.Pos;
                a.NumLinked = fad.NumLinked;
                a.Height = fad.Height;
                a.Radius = fad.Radius;
                a.Linked = fad.NumLinked > 0
                    ? r2.ReadTArray<int>(sizeof(int), fad.NumLinked)
                    : null;
            }
            //Log($"Anchors: {r2.Position():x}");

            // rooms
            E_PORTAL_DATA portals = null;
            if (fsh.NumRooms > 0)
            {
                portals = Level.Portals = new E_PORTAL_DATA();
                portals.NumRooms = fsh.NumRooms;
                portals.Room = new E_ROOM_DATA[portals.NumRooms + 1];
                portals.NumTotal = fsh.NumPortals;
                var levelPortals = portals.Portals = new E_PORTALS[portals.NumTotal];
                for (i = 0; i < portals.NumTotal; i++)
                {
                    ref E_PORTALS p = ref levelPortals[i];
                    var epo = r2.ReadS<E_SAVE_PORTALS>();
                    p.memset();
                    p.Room1 = epo.Room1;
                    p.Room2 = epo.Room2;
                    p.UsePortal = epo.UsePortal;
                    p.Paddy = epo.Paddy;
                    p.Poly.Area = epo.Poly.Area;
                    p.Poly.Type = epo.Poly.Type;
                    p.Poly.TransVal = epo.Poly.TransVal;
                    p.Poly.Room = epo.Poly.Room;
                    p.Poly.Misc = epo.Poly.Misc;
                    p.Poly.Center = epo.Poly.Center;
                    p.Poly.Max = epo.Poly.Max;
                    p.Poly.Min = epo.Poly.Min;
                    p.Poly.Norm = epo.Poly.Norm;
                    p.Poly.Norm2 = epo.Poly.Norm2;
                    p.Poly.Nrml = new Vector3[] { epo.Poly.Nrml0, epo.Poly.Nrml1, epo.Poly.Nrml2, epo.Poly.Nrml3 };
                    p.Poly.V = new TLVERTEX[] { epo.Poly.V0, epo.Poly.V1, epo.Poly.V2, epo.Poly.V3 };
                    p.Poly.Tv = new TLVERTEX[] { epo.Poly.Tv0, epo.Poly.Tv1, epo.Poly.Tv2, epo.Poly.Tv3 };
                }
                for (i = 0; i < portals.NumRooms + 1; i++)
                {
                    var rd = portals.Room[i] = new E_ROOM_DATA();
                    var erd = r2.ReadS<E_SAVE_ROOM_DATA>();
                    rd.NumPortals = erd.NumPortals;
                    rd.NumPolys = erd.NumPolys;
                    rd.Portals = rd.NumPortals > 0
                        ? r2.ReadTArray<int>(sizeof(int), rd.NumPortals)
                        : null;
                    rd.EpData = rd.NumPolys > 0
                        ? r2.ReadTArray<EP_DATA>(sizeof(EP_DATA), rd.NumPolys)
                        : null;
                }
            }
            //Log($"Portals: {r2.Position():x}");

            if (portals != null)
            {
                var numRoomDistance = Level.NumRoomDistance = portals.NumRooms + 1;
                Level.RoomDistance = new ROOM_DIST_DATA[numRoomDistance * numRoomDistance];
                for (var n = 0; n < numRoomDistance; n++)
                    for (var m = 0; m < numRoomDistance; m++)
                    {
                        var rdds = r2.ReadS<ROOM_DIST_DATA_SAVE>();
                        SetRoomDistance(Level, m, n, rdds.Distance, ref rdds.StartPos, ref rdds.EndPos);
                    }
            }
            else
            {
                Level.NumRoomDistance = 0;
                Level.RoomDistance = null;
            }
            //Log($"RoomDistance: {r2.Position():x}");
            ComputePolyIn();
            //PATHFINDER_Create();
            //PORTAL_Blend_Portals_And_Rooms();
            //ComputePortalVertexBuffer();
        }

        static void DeclareEGInfo(E_BACKGROUND bkg, float x, float y, float z)
        {
            var posx = (int)(float)(x * bkg.Xmul);
            if (posx < 0) return;
            else if (posx >= bkg.XSize) return;

            var posz = (int)(float)(z * bkg.Zmul);
            if (posz < 0) return;
            else if (posz >= bkg.ZSize) return;

            ref E_BKG_INFO eg = ref bkg.Backg[posx + posz * bkg.XSize];
            eg.Nothing = false;
        }

        static void SetRoomDistance(FastLevel level, long i, long j, float val, ref Vector3 p1, ref Vector3 p2)
        {
            if (i < 0 || j < 0 || i >= level.NumRoomDistance || j >= level.NumRoomDistance || level.RoomDistance == null) return;
            var offs = i + j * level.NumRoomDistance;
            ref ROOM_DIST_DATA rd = ref level.RoomDistance[offs];
            rd.StartPos = p1;
            rd.EndPos = p2;
            rd.Distance = val;
        }

        static void ComputePolyIn()
        {
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo("BinaryFTS", items: new List<MetaInfo> {
                    //new MetaInfo($"Type: {Type}"),
                })
            };
            return nodes;
        }
    }
}
