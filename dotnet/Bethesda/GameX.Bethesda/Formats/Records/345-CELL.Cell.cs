using GameX.Formats;
using OpenStack.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace GameX.Bethesda.Formats.Records
{
    public unsafe class CELLRecord : Record, ICellRecord
    {
        [Flags]
        public enum CELLFlags : ushort
        {
            Interior = 0x0001,
            HasWater = 0x0002,
            InvertFastTravel = 0x0004, //: IllegalToSleepHere
            BehaveLikeExterior = 0x0008, //: BehaveLikeExterior (Tribunal), Force hide land (exterior cell) / Oblivion interior (interior cell)
            Unknown1 = 0x0010,
            PublicArea = 0x0020, // Public place
            HandChanged = 0x0040,
            ShowSky = 0x0080, // Behave like exterior
            UseSkyLighting = 0x0100,
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct XCLCField
        {
            public static (string, int) StructN = ("<2iI", -1);
            public int GridX;
            public int GridY;
            public uint Flags;
            public override string ToString() => $"{GridX}x{GridY}";
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct XCLLField
        {
            public static (string, int) StructN = ("<12c2f2i3f", -1);
            public ColorRef4 AmbientColor;
            public ColorRef4 DirectionalColor; //: SunlightColor
            public ColorRef4 FogColor;
            public float FogNear; //: FogDensity
            // TES4
            public float FogFar;
            public int DirectionalRotationXY;
            public int DirectionalRotationZ;
            public float DirectionalFade;
            public float FogClipDist;
            // TES5
            public float FogPow;
        }

        public class XOWNGroup
        {
            public FMIDField<Record> XOWN;
            public IN32Field XRNK; // Faction rank
            public FMIDField<Record> XGLB;
        }

        public class RefObj
        {
            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public struct XYZAField
            {
                public static (string, int) Struct = ("<3f3f", sizeof(XYZAField)); 
                public Float3 Position;
                public Float3 EulerAngles;
            }

            public UI32Field? FRMR; // Object Index (starts at 1)
                                    // This is used to uniquely identify objects in the cell. For new files the index starts at 1 and is incremented for each new object added. For modified
                                    // objects the index is kept the same.
            public override string ToString() => $"CREF: {EDID.Value}";
            public STRVField EDID; // Object ID
            public FLTVField? XSCL; // Scale (Static)
            public IN32Field? DELE; // Indicates that the reference is deleted.
            public XYZAField? DODT; // XYZ Pos, XYZ Rotation of exit
            public STRVField DNAM; // Door exit name (Door objects)
            public FLTVField? FLTV; // Follows the DNAM optionally, lock level
            public STRVField KNAM; // Door key
            public STRVField TNAM; // Trap name
            public BYTEField? UNAM; // Reference Blocked (only occurs once in MORROWIND.ESM)
            public STRVField ANAM; // Owner ID string
            public STRVField BNAM; // Global variable/rank ID
            public IN32Field? INTV; // Number of uses, occurs even for objects that don't use it
            public UI32Field? NAM9; // Unknown
            public STRVField XSOL; // Soul Extra Data (ID string of creature)
            public XYZAField DATA; // Ref Position Data
            //
            public STRVField CNAM; // Unknown
            public UI32Field? NAM0; // Unknown
            public IN32Field? XCHG; // Unknown
            public IN32Field? INDX; // Unknown
        }

        public override string ToString() => $"CELL: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID. Can be an empty string for exterior cells in which case the region name is used instead.
        public STRVField FULL; // Full Name / TES3:RGNN - Region name
        public UI16Field DATA; // Flags
        public XCLCField? XCLC; // Cell Data (only used for exterior cells)
        public XCLLField? XCLL; // Lighting (only used for interior cells)
        public FLTVField? XCLW; // Water Height
        // TES3
        public UI32Field? NAM0; // Number of objects in cell in current file (Optional)
        public INTVField INTV; // Unknown
        public CREFField? NAM5; // Map Color (COLORREF)
        // TES4
        public FMIDField<REGNRecord>[] XCLRs; // Regions
        public BYTEField? XCMT; // Music (optional)
        public FMIDField<CLMTRecord>? XCCM; // Climate
        public FMIDField<WATRRecord>? XCWT; // Water
        public List<XOWNGroup> XOWNs = new List<XOWNGroup>(); // Ownership

        // Referenced Object Data Grouping
        public bool InFRMR = false;
        public List<RefObj> RefObjs = new List<RefObj>();
        RefObj _lastRef;

        public bool IsInterior => (DATA.Value & 0x01) == 0x01;
        public Int3 GridId; // => new Int3(XCLC.Value.GridX, XCLC.Value.GridY, !IsInterior ? 0 : -1);
        public GXColor? AmbientLight => XCLL != null ? (GXColor?)XCLL.Value.AmbientColor.ToColor32() : null;

        public override bool CreateField(BinaryReader r, BethesdaFormat format, string type, int dataSize)
        {
            //Console.WriteLine($"   {type}");
            if (!InFRMR && type == "FRMR")
                InFRMR = true;
            if (!InFRMR)
                switch (type)
                {
                    case "EDID":
                    case "NAME": EDID = r.ReadSTRV(dataSize); return true;
                    case "FULL":
                    case "RGNN": FULL = r.ReadSTRV(dataSize); return true;
                    case "DATA": DATA = r.ReadINTV(format == BethesdaFormat.TES3 ? 4 : dataSize).ToUI16Field(); if (format == BethesdaFormat.TES3) goto case "XCLC"; return true;
                    case "XCLC": XCLC = r.ReadS2<XCLCField>(format == BethesdaFormat.TES3 ? 8 : dataSize); return true;
                    case "XCLL":
                    case "AMBI": XCLL = r.ReadS2<XCLLField>(dataSize); return true;
                    case "XCLW":
                    case "WHGT": XCLW = r.ReadS2<FLTVField>(dataSize); return true;
                    // TES3
                    case "NAM0": NAM0 = r.ReadS2<UI32Field>(dataSize); return true;
                    case "INTV": INTV = r.ReadINTV(dataSize); return true;
                    case "NAM5": NAM5 = r.ReadS2<CREFField>(dataSize); return true;
                    // TES4
                    case "XCLR":
                        XCLRs = new FMIDField<REGNRecord>[dataSize >> 2];
                        for (var i = 0; i < XCLRs.Length; i++) XCLRs[i] = new FMIDField<REGNRecord>(r, 4); return true;
                    case "XCMT": XCMT = r.ReadS2<BYTEField>(dataSize); return true;
                    case "XCCM": XCCM = new FMIDField<CLMTRecord>(r, dataSize); return true;
                    case "XCWT": XCWT = new FMIDField<WATRRecord>(r, dataSize); return true;
                    case "XOWN": XOWNs.Add(new XOWNGroup { XOWN = new FMIDField<Record>(r, dataSize) }); return true;
                    case "XRNK": XOWNs.Last().XRNK = r.ReadS2<IN32Field>(dataSize); return true;
                    case "XGLB": XOWNs.Last().XGLB = new FMIDField<Record>(r, dataSize); return true;
                    default: return false;
                }
            // Referenced Object Data Grouping
            else switch (type)
                {
                    // RefObjDataGroup sub-records
                    case "FRMR":
                        _lastRef = new RefObj(); RefObjs.Add(_lastRef);
                        _lastRef.FRMR = r.ReadS2<UI32Field>(dataSize); return true;
                    case "NAME": _lastRef.EDID = r.ReadSTRV(dataSize); return true;
                    case "XSCL": _lastRef.XSCL = r.ReadS2<FLTVField>(dataSize); return true;
                    case "DODT": _lastRef.DODT = r.ReadS2<RefObj.XYZAField>(dataSize); return true;
                    case "DNAM": _lastRef.DNAM = r.ReadSTRV(dataSize); return true;
                    case "FLTV": _lastRef.FLTV = r.ReadS2<FLTVField>(dataSize); return true;
                    case "KNAM": _lastRef.KNAM = r.ReadSTRV(dataSize); return true;
                    case "TNAM": _lastRef.TNAM = r.ReadSTRV(dataSize); return true;
                    case "UNAM": _lastRef.UNAM = r.ReadS2<BYTEField>(dataSize); return true;
                    case "ANAM": _lastRef.ANAM = r.ReadSTRV(dataSize); return true;
                    case "BNAM": _lastRef.BNAM = r.ReadSTRV(dataSize); return true;
                    case "INTV": _lastRef.INTV = r.ReadS2<IN32Field>(dataSize); return true;
                    case "NAM9": _lastRef.NAM9 = r.ReadS2<UI32Field>(dataSize); return true;
                    case "XSOL": _lastRef.XSOL = r.ReadSTRV(dataSize); return true;
                    case "DATA": _lastRef.DATA = r.ReadS2<RefObj.XYZAField>(dataSize); return true;
                    //
                    case "CNAM": _lastRef.CNAM = r.ReadSTRV(dataSize); return true;
                    case "NAM0": _lastRef.NAM0 = r.ReadS2<UI32Field>(dataSize); return true;
                    case "XCHG": _lastRef.XCHG = r.ReadS2<IN32Field>(dataSize); return true;
                    case "INDX": _lastRef.INDX = r.ReadS2<IN32Field>(dataSize); return true;
                    default: return false;
                }
        }
    }
}