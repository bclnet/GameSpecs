using System;
using System.Runtime.InteropServices;

namespace GameX.IW.Zone
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct XModelAngle
    {
        public ushort x;
        public ushort y;
        public ushort z;
        public ushort @base; // defines the 90-degree point for the shorts
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct XModelTagPos
    {
        public float x;
        public float y;
        public float z;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct DObjAnimMat
    {
        public fixed float quat[4];
        public fixed float trans[3];
        public float transWeight;
    }

    // NOOOOOPE
    /*
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct XBoneInfo
    {
        public float bounds[2][3];
        public float offset[3];
        public float radiusSquared;
    }
    */

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct XSurfaceCTEntry
    {
        public fixed char pad[24];
        public int numNode;
        public char* node; // el size 16
        public int numLeaf;
        public short* leaf;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct XSurfaceCT
    {
        public int pad;
        public int pad2;
        public XSurfaceCTEntry* entry;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct GfxPackedVertex
    {
        public float x;
        public float y;
        public float z;
        public uint color;
        public fixed ushort texCoords[2];
        public fixed float normal[3];
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct Face
    {
        public ushort v1;
        public ushort v2;
        public ushort v3;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct XSurface
    {
        public short pad; // +0
        public ushort numVertices; // +2
        public ushort numPrimitives; // +4
        public byte streamHandle; // something to do with buffers, +6
        public char pad2; // +7
        public int pad3; // +8
        public Face* indexBuffer; // +12
        public short blendNum1; // +16
        public short blendNum2; // +18
        public short blendNum3; // +20
        public short blendNum4; // +22
        public char* blendInfo; // +24
        public GfxPackedVertex* vertexBuffer; // +28
        public int numCT; // +32
        public XSurfaceCT* ct; // +36
        public fixed char pad5[24]; // +40

        // pad5 matches XModelSurfaces pad
        // total size, 64
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct XModelSurfaces
    {
        public char* name; // +0
        public XSurface* surfaces; // +4
        public int numSurfaces; // +8
        public fixed char pad[24]; // +12, matches XSurface pad5
    } // total size, 36

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct XSurfaceLod
    {
        public fixed char pad[4]; // +0
        public short numSurfs; // +4
        public short pad2;// +6
        public XModelSurfaces* surfaces; // +8
        public fixed char pad3[32]; // +12
    } // +44

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct XColSurf
    {
        public void* tris; // +0, sizeof 48
        public int count; // +4
        public fixed char pad[36]; // +8
    } // +44

    [StructLayout(LayoutKind.Sequential)]
    public unsafe partial struct XModel
    {
        public char* name; // +0
        public char numBones; // +4
        public char numRootBones; // +5
        public char numSurfaces; // +6
        public char pad2; // +7
        public fixed char pad3[28]; // +8
        public short* boneNames; // +36
        public char* parentList; // +40
        public XModelAngle* tagAngles; // +44, element size 8
        public XModelTagPos* tagPositions; // +48, element size 12
        public char* partClassification; // +52
        public DObjAnimMat* animMatrix; // +56, element size 32
        public Material** materials; // +60
        public XSurfaceLod* lods(int idx) => throw new NotImplementedException(); // +64
        public XSurfaceLod lods0; public XSurfaceLod lods1; public XSurfaceLod lods2; public XSurfaceLod lods3; // +64
        public int pad4; // +240
        public XColSurf* colSurf; // +244
        public int numColSurfs; // +248
        public int pad6;
        public char* boneInfo; // bone count, +256, element size 28
        public fixed char pad5[36];
        public PhysPreset* physPreset;
        public PhysGeomList* physCollmap;
    } // total size 304
}