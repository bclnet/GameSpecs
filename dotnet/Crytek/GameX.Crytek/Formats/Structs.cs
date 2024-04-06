using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using static OpenStack.Debug;

namespace GameX.Crytek.Formats
{
    /// <summary>
    /// String32 Name, int Start, int End
    /// </summary>
    public struct RangeEntity
    {
        public string Name;         // String32! 32 byte char array.
        public int Start;
        public int End;
    }

    /// <summary>
    /// Vertex with position p(Vector3) and normal n(Vector3)
    /// </summary>
    public struct Vertex
    {
        public Vector3 p;           // position
        public Vector3 n;           // normal
    }

    /// <summary>
    /// mesh face (3 vertex, Material index, smoothing group.  All ints)
    /// </summary>
    public struct Face
    {
        public int v0;              // first vertex
        public int v1;              // second vertex
        public int v2;              // third vertex
        public int Material;        // Material Index
        public int SmGroup;         // smoothing group
    }

    /// <summary>
    /// Contains data about the parts of a mesh, such as vertices, radius and center.
    /// </summary>
    public struct MeshSubset
    {
        public int FirstIndex;
        public int NumIndices;
        public int FirstVertex;
        public int NumVertices;
        public uint MatID;
        public float Radius;
        public Vector3 Center;

        #region Log
#if LOG
        public void LogMeshSubset()
        {
            Log($"*** Mesh Subset ***");
            Log($"    First Index:  {FirstIndex}");
            Log($"    Num Indices:  {NumIndices}");
            Log($"    First Vertex: {FirstVertex}");
            Log($"    Num Vertices: {NumVertices}");
            Log($"    Mat ID:       {MatID}");
            Log($"    Radius:       {Radius:F7}");
            Log($"    Center:");
            Center.LogVector3();
        }
#endif
        #endregion
    }

    public struct Key
    {
        public int Time;            // Time in ticks
        public Vector3 AbsPos;      // absolute position
        public Vector3 RelPos;      // relative position
        public Quaternion RelQuat;  // Relative Quaternion if ARG==1?
        public Vector3 Unknown1;    // If ARG==6 or 10?
        public float[] Unknown2;    // If ARG==9?  array length = 2
    }

    public struct UVFace
    {
        public int T0;              // first vertex index
        public int T1;              // second vertex index
        public int T2;              // third vertex index
    }

    public struct ControllerInfo
    {
        public uint ControllerID;
        public uint PosKeyTimeTrack;
        public uint PosTrack;
        public uint RotKeyTimeTrack;
        public uint RotTrack;
    }

    /// <summary>
    /// May also be known as ColorB.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct IRGBA
    {
        public const int SizeOf = sizeof(byte) * 4;
        public IRGBA(byte r, byte g, byte b, byte a) { value = 0; this.r = r; this.g = g; this.b = b; this.a = a; }
        [FieldOffset(0)] public byte r;              // red
        [FieldOffset(1)] public byte g;              // green
        [FieldOffset(2)] public byte b;              // blue
        [FieldOffset(3)] public byte a;              // alpha
        [FieldOffset(0)] public int value;
    }

    //public struct FRGB
    //{
    //    public float r;           // float Red
    //    public float g;           // float green
    //    public float b;           // float blue
    //}

    //public struct AABB
    //{
    //    Vector3 min;
    //    Vector3 max;
    //}

    public struct Tangent
    {
        public const int SizeOf = sizeof(float) * 4;
        // Tangents. Divide each component by 32767 to get the actual value
        public float X;
        public float Y;
        public float Z;
        public float W;             // Handness?  Either 32767 (+1.0) or -32767 (-1.0)
    }

    public struct SkinVertex
    {
        public int Volumetric;
        public int[] Index;         // Array of 4 ints
        public float[] W;           // Array of 4 floats
        public Matrix3x3 M;
    }

    public struct PhysicsGeometry
    {
        public uint physicsGeom;
        public uint flags;          // 0x0C ?
        public Vector3 min;
        public Vector3 max;
        public Vector3 spring_angle;
        public Vector3 spring_tension;
        public Vector3 damping;
        public Matrix3x3 framemtx;

        /// <summary>
        /// Read a PhysicsGeometry structure
        /// </summary>
        /// <param name="r">The b.</param>
        public void ReadPhysicsGeometry(BinaryReader r)
        {
            physicsGeom = r.ReadUInt32();
            flags = r.ReadUInt32();
            min = r.ReadVector3();
            max = r.ReadVector3();
            spring_angle = r.ReadVector3();
            spring_tension = r.ReadVector3();
            damping = r.ReadVector3();
            framemtx = r.ReadMatrix3x3();
            return;
        }

        #region Log
#if LOG
        public void LogPhysicsGeometry()
            => Log("WritePhysicsGeometry");
#endif
        #endregion
    }

    public class CompiledPhysicalBone
    {
        public uint BoneIndex;
        public uint ParentOffset;
        public int NumChildren;
        public uint ControllerID;
        public char[] prop;
        public PhysicsGeometry PhysicsGeometry;

        // Calculated values
        public long offset;
        public uint parentID;                           // ControllerID of parent
        public List<uint> childIDs = new List<uint>();  // Not part of read struct.  Contains the controllerIDs of the children to this bone.

        public void ReadCompiledPhysicalBone(BinaryReader r)
        {
            // Reads just a single 584 byte entry of a bone. At the end the seek position will be advanced, so keep that in mind.
            BoneIndex = r.ReadUInt32();                 // unique id of bone (generated from bone name)
            ParentOffset = r.ReadUInt32();
            NumChildren = (int)r.ReadUInt32();
            ControllerID = r.ReadUInt32();
            prop = r.ReadChars(32);                     // Not sure what this is used for.
            PhysicsGeometry.ReadPhysicsGeometry(r);
        }

        #region Log
#if LOG
        public void LogCompiledPhysicalBone()
        {
            // Output the bone to the console
            Log($"*** Compiled bone ID {BoneIndex}");
            Log($"    Parent Offset: {ParentOffset}");
            Log($"    Controller ID: {ControllerID}");
            Log($"*** End Bone {BoneIndex}");
        }
#endif
        #endregion
    }

    /// <summary>
    /// A bone initial position matrix.
    /// </summary>
    public struct InitialPosMatrix
    {
        Matrix3x3 Rotation;         // type="Matrix33">
        Vector3 Position;           // type="Vector3">
    }

    public struct BoneLink
    {
        public int BoneID;
        public Vector3 offset;
        public float Blending;
    }

    public class DirectionalBlends
    {
        public string AnimToken = string.Empty;
        public uint AnimTokenCRC32 = 0;
        public string ParaJointName = string.Empty;
        public short ParaJointIndex = -1;
        public short RotParaJointIndex = -1;
        public string StartJointName = string.Empty;
        public short StartJointIndex = -1;
        public short RotStartJointIndex = -1;
        public string ReferenceJointName = string.Empty;
        public short ReferenceJointIndex = -1; // by default we use the Pelvis
    };

    #region Skinning Structures

    public struct BoneEntity
    {
        int Bone_Id;                //" type="int">Bone identifier.</add>
        int Parent_Id;              //" type="int">Parent identifier.</add>
        int Num_Children;           //" type="uint" />
        uint Bone_Name_CRC32;       //" type="uint">CRC32 of bone name as listed in the BoneNameListChunk.  In python this can be calculated using zlib.crc32(name)</add>
        string Properties;          //" type="String32" />
        BonePhysics Physics;        //" type="BonePhysics" />
    }

    public struct BonePhysics       // 26 total words = 104 total bytes
    {
        uint Geometry;              //" type="Ref" template="BoneMeshChunk">Geometry of a separate mesh for this bone.</add>
        uint Flags;                 //" type="uint" />
        Vector3 Min;                //" type="Vector3" />
        Vector3 Max;                //" type="Vector3" />
        Vector3 Spring_Angle;       //" type="Vector3" />
        Vector3 Spring_Tension;     //" type="Vector3" />
        Vector3 Damping;            //" type="Vector3" />
        Matrix3x3 Frame_Matrix;     //" type="Matrix33" />
    }

    /// <summary>
    /// 4 bones, 4 weights for each vertex mapping.
    /// </summary>
    public struct MeshBoneMapping
    {
        public int[] BoneIndex;
        public int[] Weight;        // Byte / 256?
    }

    public struct MeshPhysicalStreamHeader
    {
        public uint ChunkID;
        public int NumPoints;
        public int NumIndices;
        public int NumMaterials;
    }

    public struct MeshMorphTargetHeader
    {
        public uint MeshID;
        public int NameLength;
        public int NumIntVertices;
        public int NumExtVertices;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MeshMorphTargetVertex
    {
        public const int SizeOf = sizeof(uint) + MathX.SizeOfVector3;
        public uint VertexID;
        public Vector3 Vertex;
    }

    public struct MorphTargets
    {
        readonly uint MeshID;
        readonly string Name;
        readonly List<MeshMorphTargetVertex> IntMorph;
        readonly List<MeshMorphTargetVertex> ExtMorph;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TFace
    {
        public const int SizeOf = sizeof(ushort) * 3;
        public ushort I0;
        public ushort I1;
        public ushort I2;

        //public static bool operator =(TFace face)
        //{
        //    if (face.i0 == i0 && face.i1 == i1 && face.i2 == i2) return true;
        //    else return false;
        //}
    }

    public class MeshCollisionInfo
    {
        // AABB AABB;               // Bounding box structures?
        // OBB OBB;                 // Has an M44, h and c value.
        public Vector3 Position;
        public List<short> Indices;
        public int BoneID;
    }

    public struct IntSkinVertex
    {
        public Vector3 Obsolete0;
        public Vector3 Position;
        public Vector3 Obsolete2;
        public ushort[] BoneIDs;    // 4 bone IDs
        public float[] Weights;     // Should be 4 of these
        public IRGBA Color;
    }

    public struct SpeedChunk
    {
        public float Speed;
        public float Distance;
        public float Slope;
        public int AnimFlags;
        public float[] MoveDir;
        public Quaternion StartPosition;
    }

    public struct PhysicalProxy
    {
        public uint ID;             // Chunk ID (although not technically a chunk
        public uint FirstIndex;
        public int NumIndices;
        public uint FirstVertex;
        public int NumVertices;
        public uint Material;       // Size of the weird data at the end of the hitbox structure.
        public Vector3[] Vertices;  // Array of vertices (x,y,z) length NumVertices
        public ushort[] Indices;    // Array of indices

        #region Log
#if LOG
        public void LogHitBox()
        {
            Log($"     ** Hitbox **");
            Log($"        ID: {ID:X}");
            Log($"        Num Vertices: {NumVertices:X}");
            Log($"        Num Indices:  {NumIndices:X}");
            Log($"        Material Index: {Material:X}");
        }
#endif
        #endregion
    }

    public struct PhysicalProxyStub
    {
        uint ChunkID;
        List<Vector3> Points;
        List<short> Indices;
        List<string> Materials;
    }

    #endregion

    /// <summary>
    /// Collision or hitbox info. Part of the MeshPhysicsData chunk
    /// </summary>
    //public struct PhysicsData
    //{
    //    public int Unknown4;
    //    public int Unknown5;
    //    public float[] Unknown6;  // array length 3, Inertia?
    //    public Quaternion Rot;  // Most definitely a quaternion. Probably describes rotation of the physics object.
    //    public Vector3 Center;  // Center, or position. Probably describes translation of the physics object. Often corresponds to the center of the mesh data as described in the submesh chunk.
    //    public float Unknown10; // Mass?
    //    public int Unknown11;
    //    public int Unknown12;
    //    public float Unknown13;
    //    public float Unknown14;
    //    public PhysicsPrimitiveType PrimitiveType;
    //    public PhysicsCube Cube;  // Primitive Type 0
    //    public PhysicsPolyhedron PolyHedron;  // Primitive Type 1
    //    public PhysicsCylinder Cylinder; // Primitive Type 5
    //    public PhysicsShape6 UnknownShape6;  // Primitive Type 6
    //}

    public struct PhysicsCube
    {
        public PhysicsStruct1 Unknown14;
        public PhysicsStruct1 Unknown15;
        public int Unknown16;
    }

    public struct PhysicsPolyhedron
    {
        public uint NumVertices;
        public uint NumTriangles;
        public int Unknown17;
        public int Unknown18;
        public byte HasVertexMap;
        public ushort[] VertexMap;  // Array length NumVertices.  If the (non-physics) mesh has say 200 vertices, then the first 200 entries of this map give a mapping identifying the unique vertices. The meaning of the extra entries is unknown.
        public byte UseDatasStream;
        public Vector3[] Vertices;  // Array Length NumVertices
        public ushort[] Triangles;  // Array length NumTriangles
        public byte Unknown210;
        public byte[] TriangleFlags; // Array length NumTriangles
        public ushort[] TriangleMap; // Array length NumTriangles
        public byte[] Unknown45;    // Array length 16
        public int Unknown461;      // 0
        public int Unknown462;      // 0
        public float Unknown463;    // 0.02
        public float Unknown464;
        // There is more. See cgf.xml for the rest, but probably not really needed
    }

    public struct PhysicsCylinder
    {
        public float[] Unknown1;    // array length 8
        public int Unknown2;
        public PhysicsDataType2 Unknown3;
    }

    public struct PhysicsShape6
    {
        public float[] Unknown1;    // array length 8
        public int Unknown2;
        public PhysicsDataType2 Unknown3;
    }

    public struct PhysicsDataType0
    {
        public int NumData;
        public PhysicsStruct2[] Data; // Array length NumData
        public int[] Unknown33;     // array length 3
        public float Unknown80;
    }

    public struct PhysicsDataType1
    {
        public uint NumData1;       // usually 4294967295
        public PhysicsStruct50[] Data1; // Array length NumData1
        public int NumData2;
        public PhysicsStruct50[] Data2; // Array length NumData2
        public float[] Unknown60;   // array length 6
        public Matrix3x3 Unknown61; // Rotation matrix?
        public int[] Unknown70;     // Array length 3
        public float Unknown80;
    }

    public struct PhysicsDataType2
    {
        public Matrix3x3 Unknown1;
        public int Unknown;
        public float[] Unknown3;    // array length 6
        public int Unknown4;
    }

    public struct PhysicsStruct1
    {
        public Matrix3x3 Unknown1;
        public int Unknown2;
        public float[] Unknown3;    // array length 6
    }

    public struct PhysicsStruct2
    {
        public Matrix3x3 Unknown1;
        public float[] Unknown2;    // array length 6
        public int[] Unknown3;      // array length 3
    }

    public struct PhysicsStruct50
    {
        public short Unknown11;
        public short Unknown12;
        public short Unknown21;
        public short Unknown22;
        public short Unknown23;
        public short Unknown24;
    }
}

#if false
/// <summary>
/// WORLDTOBONE is also the Bind Pose Matrix (BPM)
/// </summary>
public struct WORLDTOBONE
{
    public float[,] worldToBone;   //  4x3 structure

    public void GetWorldToBone(BinaryReader r)
    {
        worldToBone = new float[3, 4];
        for (var i = 0; i < 3; i++) for (var j = 0; j < 4; j++) worldToBone[i, j] = r.ReadSingle();
        //Log($"worldToBone: {worldToBone[i, j]:F7}");
        return;
    }

    public Matrix4x4 GetMatrix44() => new Matrix4x4
    {
        M11 = worldToBone[0, 0],
        M12 = worldToBone[0, 1],
        M13 = worldToBone[0, 2],
        M14 = worldToBone[0, 3],
        M21 = worldToBone[1, 0],
        M22 = worldToBone[1, 1],
        M23 = worldToBone[1, 2],
        M24 = worldToBone[1, 3],
        M31 = worldToBone[2, 0],
        M32 = worldToBone[2, 1],
        M33 = worldToBone[2, 2],
        M34 = worldToBone[2, 3],
        M41 = 0,
        M42 = 0,
        M43 = 0,
        M44 = 1
    };

    public void LogWorldToBone()
    {
        //Log("     *** World to Bone ***");
        Log($"     {worldToBone[0, 0]:F7}  {worldToBone[0, 1]:F7}  {worldToBone[0, 2]:F7}  {worldToBone[0, 3]:F7}");
        Log($"     {worldToBone[1, 0]:F7}  {worldToBone[1, 1]:F7}  {worldToBone[1, 2]:F7}  {worldToBone[1, 3]:F7}");
        Log($"     {worldToBone[2, 0]:F7}  {worldToBone[2, 1]:F7}  {worldToBone[2, 2]:F7}  {worldToBone[2, 3]:F7}");
    }

    internal Matrix3x3 GetWorldToBoneRotationMatrix() => new Matrix3x3
    {
        M11 = worldToBone[0, 0],
        M12 = worldToBone[0, 1],
        M13 = worldToBone[0, 2],
        M21 = worldToBone[1, 0],
        M22 = worldToBone[1, 1],
        M23 = worldToBone[1, 2],
        M31 = worldToBone[2, 0],
        M32 = worldToBone[2, 1],
        M33 = worldToBone[2, 2]
    };

    internal Vector3 GetWorldToBoneTranslationVector() => new Vector3
    {
        X = worldToBone[0, 3],
        Y = worldToBone[1, 3],
        Z = worldToBone[2, 3]
    };
}


/// <summary>
/// BONETOWORLD contains the world space location/rotation of a bone.
/// </summary>
public struct BONETOWORLD
{
    public float[,] boneToWorld;   //  4x3 structure

    public void ReadBoneToWorld(BinaryReader r)
    {
        boneToWorld = new float[3, 4];
        for (var i = 0; i < 3; i++) for (var j = 0; j < 4; j++) boneToWorld[i, j] = r.ReadSingle();
        //Log($"boneToWorld: {boneToWorld[i, j]:F7}");
        return;
    }

    /// <summary>
    /// Returns the world space rotational matrix in a Math.net 3x3 matrix.
    /// </summary>
    /// <returns>Matrix33</returns>
    public Matrix3x3 GetBoneToWorldRotationMatrix() => new Matrix3x3
    {
        M11 = boneToWorld[0, 0],
        M12 = boneToWorld[0, 1],
        M13 = boneToWorld[0, 2],
        M21 = boneToWorld[1, 0],
        M22 = boneToWorld[1, 1],
        M23 = boneToWorld[1, 2],
        M31 = boneToWorld[2, 0],
        M32 = boneToWorld[2, 1],
        M33 = boneToWorld[2, 2]
    };

    public Vector3 GetBoneToWorldTranslationVector() => new Vector3
    {
        X = boneToWorld[0, 3],
        Y = boneToWorld[1, 3],
        Z = boneToWorld[2, 3]
    };

    public void LogBoneToWorld()
    {
        Log($"*** Bone to World ***");
        Log($"{boneToWorld[0, 0]:F6}  {boneToWorld[0, 1]:F6}  {boneToWorld[0, 2]:F6} {boneToWorld[0, 3]:F6}");
        Log($"{boneToWorld[1, 0]:F6}  {boneToWorld[1, 1]:F6}  {boneToWorld[1, 2]:F6} {boneToWorld[1, 3]:F6}");
        Log($"{boneToWorld[2, 0]:F6}  {boneToWorld[2, 1]:F6}  {boneToWorld[2, 2]:F6} {boneToWorld[2, 3]:F6}");
    }
}

/// < summary >
/// This is the same as BoneDescData
/// </ summary >
public class CompiledBone
{
    public uint ControllerID { get; set; }
    public PhysicsGeometry[] physicsGeometry;   // 2 of these.  One for live objects, other for dead (ragdoll?)
    public float mass;                         // 0xD8 ?
    public WORLDTOBONE worldToBone;             // 4x3 matrix
    public BONETOWORLD boneToWorld;             // 4x3 matrix of world translations/rotations of the bones.
    public string boneName;                     // String256 in old terms; convert to a real null terminated string.
    public uint limbID;                         // ID of this limb... usually just 0xFFFFFFFF
    public int offsetParent;                    // offset to the parent in number of CompiledBone structs (584 bytes)
    public int offsetChild;                     // Offset to the first child to this bone in number of CompiledBone structs
    public uint numChildren;                    // Number of children to this bone

    public uint parentID;                       // Not part of the read structure, but the controllerID of the parent bone put into the Bone Dictionary (the key)
    public long offset;                        // Not part of the structure, but the position in the file where this bone started.
    public List<uint> childIDs;                 // Not part of read struct.  Contains the controllerIDs of the children to this bone.
    public Matrix4x4 LocalTransform = new Matrix4x4();            // Because Cryengine tends to store transform relative to world, we have to add all the transforms from the node to the root.  Calculated, row major.
    public Vector3 LocalTranslation = new Vector3();            // To hold the local rotation vector
    public Matrix3x3 LocalRotation = new Matrix3x3();             // to hold the local rotation matrix

    public CompiledBone ParentBone { get; set; }

    public void ReadCompiledBone(BinaryReader r)
    {
        // Reads just a single 584 byte entry of a bone. At the end the seek position will be advanced, so keep that in mind.
        ControllerID = r.ReadUInt32(); // unique id of bone (generated from bone name)
        physicsGeometry = new PhysicsGeometry[2];
        physicsGeometry[0].ReadPhysicsGeometry(r); // lod 0 is the physics of alive body, 
        physicsGeometry[1].ReadPhysicsGeometry(r); // lod 1 is the physics of a dead body
        mass = r.ReadSingle();
        worldToBone = new WORLDTOBONE();
        worldToBone.GetWorldToBone(r);
        boneToWorld = new BONETOWORLD();
        boneToWorld.ReadBoneToWorld(r);
        boneName = r.ReadFString(256);
        limbID = r.ReadUInt32();
        offsetParent = r.ReadInt32();
        numChildren = r.ReadUInt32();
        offsetChild = r.ReadInt32();
        childIDs = new List<uint>(); // Calculated
    }

    public Matrix4x4 ToMatrix44(float[,] boneToWorld) => new Matrix4x4
    {
        M11 = boneToWorld[0, 0],
        M12 = boneToWorld[0, 1],
        M13 = boneToWorld[0, 2],
        M14 = boneToWorld[0, 3],
        M21 = boneToWorld[1, 0],
        M22 = boneToWorld[1, 1],
        M23 = boneToWorld[1, 2],
        M24 = boneToWorld[1, 3],
        M31 = boneToWorld[2, 0],
        M32 = boneToWorld[2, 1],
        M33 = boneToWorld[2, 2],
        M34 = boneToWorld[2, 3],
        M41 = 0,
        M42 = 0,
        M43 = 0,
        M44 = 1
    };

    public void LogCompiledBone()
    {
        // Output the bone to the console
        Log($"*** Compiled bone {boneName}");
        Log($"    Parent Name: {parentID}");
        Log($"    Offset in file: {offset:X}");
        Log($"    Controller ID: {ControllerID}");
        Log($"    World To Bone:");
        boneToWorld.LogBoneToWorld();
        Log($"    Limb ID: {limbID}");
        Log($"    Parent Offset: {offsetParent}");
        Log($"    Child Offset:  {offsetChild}");
        Log($"    Number of Children:  {numChildren}");
        Log($"*** End Bone {boneName}");
    }
}
#endif