using GameX.Meta;
using GameX.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using static OpenStack.Debug;

namespace GameX.Bethesda.Formats
{
    // Refers to an object before the current one in the hierarchy.
    public struct Ptr<T>
    {
        public int Value;
        public bool IsNull => Value < 0;
        public void Deserialize(BinaryReader r) => Value = r.ReadInt32();
    }

    // Refers to an object after the current one in the hierarchy.
    public struct Ref<T>
    {
        public int Value;
        public bool IsNull => Value < 0;
        public void Deserialize(BinaryReader r) => Value = r.ReadInt32();
    }

    public class NiReaderUtils
    {
        public static Ptr<T> ReadPtr<T>(BinaryReader r)
        {
            var ptr = new Ptr<T>();
            ptr.Deserialize(r);
            return ptr;
        }

        public static Ref<T> ReadRef<T>(BinaryReader r)
        {
            var readRef = new Ref<T>();
            readRef.Deserialize(r);
            return readRef;
        }

        public static Ref<T>[] ReadLengthPrefixedRefs32<T>(BinaryReader r)
        {
            var refs = new Ref<T>[r.ReadUInt32()];
            for (var i = 0; i < refs.Length; i++)
                refs[i] = ReadRef<T>(r);
            return refs;
        }

        public static NiAVObject.NiFlags ReadFlags(BinaryReader r) => (NiAVObject.NiFlags)r.ReadUInt16();

        public static T Read<T>(BinaryReader r)
        {
            if (typeof(T) == typeof(float)) { return (T)(object)r.ReadSingle(); }
            else if (typeof(T) == typeof(byte)) { return (T)(object)r.ReadByte(); }
            else if (typeof(T) == typeof(string)) { return (T)(object)r.ReadL32Encoding(); }
            else if (typeof(T) == typeof(Vector3)) { return (T)(object)r.ReadVector3(); }
            else if (typeof(T) == typeof(Quaternion)) { return (T)(object)r.ReadQuaternionWFirst(); }
            else if (typeof(T) == typeof(Color4)) { var color = new Color4(); color.Deserialize(r); return (T)(object)color; }
            else throw new NotImplementedException("Tried to read an unsupported type.");
        }

        public static NiObject ReadNiObject(BinaryReader r)
        {
            var nodeType = r.ReadL32AString();
            switch (nodeType)
            {
                case "NiNode": { var node = new NiNode(); node.Deserialize(r); return node; }
                case "NiTriShape": { var triShape = new NiTriShape(); triShape.Deserialize(r); return triShape; }
                case "NiTexturingProperty": { var prop = new NiTexturingProperty(); prop.Deserialize(r); return prop; }
                case "NiSourceTexture": { var srcTexture = new NiSourceTexture(); srcTexture.Deserialize(r); return srcTexture; }
                case "NiMaterialProperty": { var prop = new NiMaterialProperty(); prop.Deserialize(r); return prop; }
                case "NiMaterialColorController": { var controller = new NiMaterialColorController(); controller.Deserialize(r); return controller; }
                case "NiTriShapeData": { var data = new NiTriShapeData(); data.Deserialize(r); return data; }
                case "RootCollisionNode": { var node = new RootCollisionNode(); node.Deserialize(r); return node; }
                case "NiStringExtraData": { var data = new NiStringExtraData(); data.Deserialize(r); return data; }
                case "NiSkinInstance": { var instance = new NiSkinInstance(); instance.Deserialize(r); return instance; }
                case "NiSkinData": { var data = new NiSkinData(); data.Deserialize(r); return data; }
                case "NiAlphaProperty": { var prop = new NiAlphaProperty(); prop.Deserialize(r); return prop; }
                case "NiZBufferProperty": { var prop = new NiZBufferProperty(); prop.Deserialize(r); return prop; }
                case "NiVertexColorProperty": { var prop = new NiVertexColorProperty(); prop.Deserialize(r); return prop; }
                case "NiBSAnimationNode": { var node = new NiBSAnimationNode(); node.Deserialize(r); return node; }
                case "NiBSParticleNode": { var node = new NiBSParticleNode(); node.Deserialize(r); return node; }
                case "NiParticles": { var node = new NiParticles(); node.Deserialize(r); return node; }
                case "NiParticlesData": { var data = new NiParticlesData(); data.Deserialize(r); return data; }
                case "NiRotatingParticles": { var node = new NiRotatingParticles(); node.Deserialize(r); return node; }
                case "NiRotatingParticlesData": { var data = new NiRotatingParticlesData(); data.Deserialize(r); return data; }
                case "NiAutoNormalParticles": { var node = new NiAutoNormalParticles(); node.Deserialize(r); return node; }
                case "NiAutoNormalParticlesData": { var data = new NiAutoNormalParticlesData(); data.Deserialize(r); return data; }
                case "NiUVController": { var controller = new NiUVController(); controller.Deserialize(r); return controller; }
                case "NiUVData": { var data = new NiUVData(); data.Deserialize(r); return data; }
                case "NiTextureEffect": { var effect = new NiTextureEffect(); effect.Deserialize(r); return effect; }
                case "NiTextKeyExtraData": { var data = new NiTextKeyExtraData(); data.Deserialize(r); return data; }
                case "NiVertWeightsExtraData": { var data = new NiVertWeightsExtraData(); data.Deserialize(r); return data; }
                case "NiParticleSystemController": { var controller = new NiParticleSystemController(); controller.Deserialize(r); return controller; }
                case "NiBSPArrayController": { var controller = new NiBSPArrayController(); controller.Deserialize(r); return controller; }
                case "NiGravity": { var obj = new NiGravity(); obj.Deserialize(r); return obj; }
                case "NiParticleBomb": { var modifier = new NiParticleBomb(); modifier.Deserialize(r); return modifier; }
                case "NiParticleColorModifier": { var modifier = new NiParticleColorModifier(); modifier.Deserialize(r); return modifier; }
                case "NiParticleGrowFade": { var modifier = new NiParticleGrowFade(); modifier.Deserialize(r); return modifier; }
                case "NiParticleMeshModifier": { var modifier = new NiParticleMeshModifier(); modifier.Deserialize(r); return modifier; }
                case "NiParticleRotation": { var modifier = new NiParticleRotation(); modifier.Deserialize(r); return modifier; }
                case "NiKeyframeController": { var controller = new NiKeyframeController(); controller.Deserialize(r); return controller; }
                case "NiKeyframeData": { var data = new NiKeyframeData(); data.Deserialize(r); return data; }
                case "NiColorData": { var data = new NiColorData(); data.Deserialize(r); return data; }
                case "NiGeomMorpherController": { var controller = new NiGeomMorpherController(); controller.Deserialize(r); return controller; }
                case "NiMorphData": { var data = new NiMorphData(); data.Deserialize(r); return data; }
                case "AvoidNode": { var node = new AvoidNode(); node.Deserialize(r); return node; }
                case "NiVisController": { var controller = new NiVisController(); controller.Deserialize(r); return controller; }
                case "NiVisData": { var data = new NiVisData(); data.Deserialize(r); return data; }
                case "NiAlphaController": { var controller = new NiAlphaController(); controller.Deserialize(r); return controller; }
                case "NiFloatData": { var data = new NiFloatData(); data.Deserialize(r); return data; }
                case "NiPosData": { var data = new NiPosData(); data.Deserialize(r); return data; }
                case "NiBillboardNode": { var data = new NiBillboardNode(); data.Deserialize(r); return data; }
                case "NiShadeProperty": { var property = new NiShadeProperty(); property.Deserialize(r); return property; }
                default: { Log($"Tried to read an unsupported NiObject type ({nodeType})."); return null; }
            }
        }

        public static Matrix4x4 Read3x3RotationMatrix(BinaryReader r) => r.ReadRowMajorMatrix3x3();
    }

    public class NiFile : IHaveMetaInfo
    {
        public NiFile(string name) => Name = name;

        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => new List<MetaInfo> {
            new MetaInfo(null, new MetaContent { Type = "Engine", Name = Name, Value = this }),
            new MetaInfo("Nif", items: new List<MetaInfo> {
                new MetaInfo($"NumBlocks: {Header.NumBlocks}"),
            }),
        };

        public string Name;
        public NiHeader Header;
        public NiObject[] Blocks;
        public NiFooter Footer;

        public void Read(BinaryReader r)
        {
            Header = new NiHeader();
            Header.Deserialize(r);
            Blocks = new NiObject[Header.NumBlocks];
            for (var i = 0; i < Header.NumBlocks; i++) Blocks[i] = NiReaderUtils.ReadNiObject(r);
            Footer = new NiFooter();
            Footer.Deserialize(r);
        }

        public IEnumerable<string> GetTexturePaths()
        {
            foreach (var niObject in Blocks)
                if (niObject is NiSourceTexture niSourceTexture && !string.IsNullOrEmpty(niSourceTexture.FileName)) yield return niSourceTexture.FileName;
        }
    }

    #region Enums

    // texture enums
    public enum ApplyMode : uint
    {
        APPLY_REPLACE = 0,
        APPLY_DECAL = 1,
        APPLY_MODULATE = 2,
        APPLY_HILIGHT = 3,
        APPLY_HILIGHT2 = 4
    }

    public enum TexClampMode : uint
    {
        CLAMP_S_CLAMP_T = 0,
        CLAMP_S_WRAP_T = 1,
        WRAP_S_CLAMP_T = 2,
        WRAP_S_WRAP_T = 3
    }

    public enum TexFilterMode : uint
    {
        FILTER_NEAREST = 0,
        FILTER_BILERP = 1,
        FILTER_TRILERP = 2,
        FILTER_NEAREST_MIPNEAREST = 3,
        FILTER_NEAREST_MIPLERP = 4,
        FILTER_BILERP_MIPNEAREST = 5
    }

    public enum PixelLayout : uint
    {
        PIX_LAY_PALETTISED = 0,
        PIX_LAY_HIGH_COLOR_16 = 1,
        PIX_LAY_TRUE_COLOR_32 = 2,
        PIX_LAY_COMPRESSED = 3,
        PIX_LAY_BUMPMAP = 4,
        PIX_LAY_PALETTISED_4 = 5,
        PIX_LAY_DEFAULT = 6
    }

    public enum MipMapFormat : uint
    {
        MIP_FMT_NO = 0,
        MIP_FMT_YES = 1,
        MIP_FMT_DEFAULT = 2
    }

    public enum AlphaFormat : uint
    {
        ALPHA_NONE = 0,
        ALPHA_BINARY = 1,
        ALPHA_SMOOTH = 2,
        ALPHA_DEFAULT = 3
    }

    // miscellaneous
    public enum VertMode : uint
    {
        VERT_MODE_SRC_IGNORE = 0,
        VERT_MODE_SRC_EMISSIVE = 1,
        VERT_MODE_SRC_AMB_DIF = 2
    }

    public enum LightMode : uint
    {
        LIGHT_MODE_EMISSIVE = 0,
        LIGHT_MODE_EMI_AMB_DIF = 1
    }

    public enum KeyType : uint
    {
        LINEAR_KEY = 1,
        QUADRATIC_KEY = 2,
        TBC_KEY = 3,
        XYZ_ROTATION_KEY = 4,
        CONST_KEY = 5
    }

    public enum EffectType : uint
    {
        EFFECT_PROJECTED_LIGHT = 0,
        EFFECT_PROJECTED_SHADOW = 1,
        EFFECT_ENVIRONMENT_MAP = 2,
        EFFECT_FOG_MAP = 3
    }

    public enum CoordGenType : uint
    {
        CG_WORLD_PARALLEL = 0,
        CG_WORLD_PERSPECTIVE = 1,
        CG_SPHERE_MAP = 2,
        CG_SPECULAR_CUBE_MAP = 3,
        CG_DIFFUSE_CUBE_MAP = 4
    }

    public enum FieldType : uint
    {
        FIELD_WIND = 0,
        FIELD_POINT = 1
    }

    public enum DecayType : uint
    {
        DECAY_NONE = 0,
        DECAY_LINEAR = 1,
        DECAY_EXPONENTIAL = 2
    }

    #endregion // Enums

    #region Misc Classes

    public class BoundingBox
    {
        public uint unknownInt;
        public Vector3 translation;
        public Matrix4x4 rotation;
        public Vector3 radius;

        public void Deserialize(BinaryReader r)
        {
            unknownInt = r.ReadUInt32();
            translation = r.ReadVector3();
            rotation = NiReaderUtils.Read3x3RotationMatrix(r);
            radius = r.ReadVector3();
        }
    }

    public struct Color3
    {
        public float r;
        public float g;
        public float b;

        public void Deserialize(BinaryReader r)
        {
            this.r = r.ReadSingle();
            g = r.ReadSingle();
            b = r.ReadSingle();
        }
    }

    public struct Color4
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public void Deserialize(BinaryReader r)
        {
            this.r = r.ReadSingle();
            g = r.ReadSingle();
            b = r.ReadSingle();
            a = r.ReadSingle();
        }
    }

    public class TexDesc
    {
        public Ref<NiSourceTexture> source;
        public TexClampMode clampMode;
        public TexFilterMode filterMode;
        public uint UVSet;
        public short PS2L;
        public short PS2K;
        public ushort unknown1;

        public void Deserialize(BinaryReader r)
        {
            source = NiReaderUtils.ReadRef<NiSourceTexture>(r);
            clampMode = (TexClampMode)r.ReadUInt32();
            filterMode = (TexFilterMode)r.ReadUInt32();
            UVSet = r.ReadUInt32();
            PS2L = r.ReadInt16();
            PS2K = r.ReadInt16();
            unknown1 = r.ReadUInt16();
        }
    }

    public class TexCoord
    {
        public float u;
        public float v;

        public void Deserialize(BinaryReader r)
        {
            u = r.ReadSingle();
            v = r.ReadSingle();
        }
    }

    public class Triangle
    {
        public ushort v1;
        public ushort v2;
        public ushort v3;

        public void Deserialize(BinaryReader r)
        {
            v1 = r.ReadUInt16();
            v2 = r.ReadUInt16();
            v3 = r.ReadUInt16();
        }
    }

    public class MatchGroup
    {
        public ushort numVertices;
        public ushort[] vertexIndices;

        public void Deserialize(BinaryReader r)
        {
            numVertices = r.ReadUInt16();
            vertexIndices = new ushort[numVertices];
            for (var i = 0; i < vertexIndices.Length; i++) vertexIndices[i] = r.ReadUInt16();
        }
    }

    public class TBC
    {
        public float t;
        public float b;
        public float c;

        public void Deserialize(BinaryReader r)
        {
            t = r.ReadSingle();
            b = r.ReadSingle();
            c = r.ReadSingle();
        }
    }

    public class Key<T>
    {
        public float time;
        public T value;
        public T forward;
        public T backward;
        public TBC TBC;

        public void Deserialize(BinaryReader r, KeyType keyType)
        {
            time = r.ReadSingle();
            value = NiReaderUtils.Read<T>(r);
            if (keyType == KeyType.QUADRATIC_KEY) { forward = NiReaderUtils.Read<T>(r); backward = NiReaderUtils.Read<T>(r); }
            else if (keyType == KeyType.TBC_KEY) { TBC = new TBC(); TBC.Deserialize(r); }
        }
    }
    public class KeyGroup<T>
    {
        public uint numKeys;
        public KeyType interpolation;
        public Key<T>[] keys;

        public void Deserialize(BinaryReader r)
        {
            numKeys = r.ReadUInt32();
            if (numKeys != 0) interpolation = (KeyType)r.ReadUInt32();
            keys = new Key<T>[numKeys];
            for (var i = 0; i < keys.Length; i++) { keys[i] = new Key<T>(); keys[i].Deserialize(r, interpolation); }
        }
    }

    public class QuatKey<T>
    {
        public float time;
        public T value;
        public TBC TBC;

        public void Deserialize(BinaryReader r, KeyType keyType)
        {
            time = r.ReadSingle();
            if (keyType != KeyType.XYZ_ROTATION_KEY) value = NiReaderUtils.Read<T>(r);
            if (keyType == KeyType.TBC_KEY) { TBC = new TBC(); TBC.Deserialize(r); }
        }
    }

    public class SkinData
    {
        public SkinTransform skinTransform;
        public Vector3 boundingSphereOffset;
        public float boundingSphereRadius;
        public ushort numVertices;
        public SkinWeight[] vertexWeights;

        public void Deserialize(BinaryReader r)
        {
            skinTransform = new SkinTransform();
            skinTransform.Deserialize(r);
            boundingSphereOffset = r.ReadVector3();
            boundingSphereRadius = r.ReadSingle();
            numVertices = r.ReadUInt16();
            vertexWeights = new SkinWeight[numVertices];
            for (var i = 0; i < vertexWeights.Length; i++) { vertexWeights[i] = new SkinWeight(); vertexWeights[i].Deserialize(r); }
        }
    }

    public class SkinWeight
    {
        public ushort index;
        public float weight;

        public void Deserialize(BinaryReader r)
        {
            index = r.ReadUInt16();
            weight = r.ReadSingle();
        }
    }

    public class SkinTransform
    {
        public Matrix4x4 rotation;
        public Vector3 translation;
        public float scale;

        public void Deserialize(BinaryReader r)
        {
            rotation = NiReaderUtils.Read3x3RotationMatrix(r);
            translation = r.ReadVector3();
            scale = r.ReadSingle();
        }
    }

    public class Particle
    {
        public Vector3 velocity;
        public Vector3 unknownVector;
        public float lifetime;
        public float lifespan;
        public float timestamp;
        public ushort unknownShort;
        public ushort vertexID;

        public void Deserialize(BinaryReader r)
        {
            velocity = r.ReadVector3();
            unknownVector = r.ReadVector3();
            lifetime = r.ReadSingle();
            lifespan = r.ReadSingle();
            timestamp = r.ReadSingle();
            unknownShort = r.ReadUInt16();
            vertexID = r.ReadUInt16();
        }
    }

    public class Morph
    {
        public uint numKeys;
        public KeyType interpolation;
        public Key<float>[] keys;
        public Vector3[] vectors;

        public void Deserialize(BinaryReader r, uint numVertices)
        {
            numKeys = r.ReadUInt32();
            interpolation = (KeyType)r.ReadUInt32();
            keys = new Key<float>[numKeys];
            for (var i = 0; i < keys.Length; i++) { keys[i] = new Key<float>(); keys[i].Deserialize(r, interpolation); }
            vectors = new Vector3[numVertices];
            for (var i = 0; i < vectors.Length; i++) vectors[i] = r.ReadVector3();
        }
    }

    #endregion

    public class NiHeader
    {
        public byte[] Str; // 40 bytes (including \n)
        public uint Version;
        public uint NumBlocks;

        public void Deserialize(BinaryReader r)
        {
            Str = r.ReadBytes(40);
            Version = r.ReadUInt32();
            NumBlocks = r.ReadUInt32();
        }
    }

    public class NiFooter
    {
        public uint NumRoots;
        public int[] Roots;

        public void Deserialize(BinaryReader r)
        {
            NumRoots = r.ReadUInt32();
            Roots = new int[NumRoots];
            for (var i = 0; i < NumRoots; i++) Roots[i] = r.ReadInt32();
        }
    }

    /// <summary>
    /// These are the main units of data that NIF files are arranged in.
    /// </summary>
    public abstract class NiObject
    {
        public virtual void Deserialize(BinaryReader r) { }
    }

    /// <summary>
    /// An object that can be controlled by a controller.
    /// </summary>
    public abstract class NiObjectNET : NiObject
    {
        public string Name;
        public Ref<NiExtraData> ExtraData;
        public Ref<NiTimeController> Controller;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            Name = r.ReadL32Encoding();
            ExtraData = NiReaderUtils.ReadRef<NiExtraData>(r);
            Controller = NiReaderUtils.ReadRef<NiTimeController>(r);
        }
    }

    public abstract class NiAVObject : NiObjectNET
    {
        [Flags]
        public enum NiFlags : ushort
        {
            Hidden = 0x1
        }

        public NiFlags Flags; //: ushort
        public Vector3 Translation;
        public Matrix4x4 Rotation;
        public float Scale;
        public Vector3 Velocity;
        //public uint numProperties;
        public Ref<NiProperty>[] Properties;
        public bool HasBoundingBox;
        public BoundingBox BoundingBox;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            Flags = NiReaderUtils.ReadFlags(r);
            Translation = r.ReadVector3();
            Rotation = NiReaderUtils.Read3x3RotationMatrix(r);
            Scale = r.ReadSingle();
            Velocity = r.ReadVector3();
            Properties = NiReaderUtils.ReadLengthPrefixedRefs32<NiProperty>(r);
            HasBoundingBox = r.ReadBool32();
            if (HasBoundingBox) { BoundingBox = new BoundingBox(); BoundingBox.Deserialize(r); }
        }
    }

    // Nodes
    public class NiNode : NiAVObject
    {
        //public uint numChildren;
        public Ref<NiAVObject>[] Children;
        //public uint numEffects;
        public Ref<NiDynamicEffect>[] Effects;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            Children = NiReaderUtils.ReadLengthPrefixedRefs32<NiAVObject>(r);
            Effects = NiReaderUtils.ReadLengthPrefixedRefs32<NiDynamicEffect>(r);
        }
    }

    public class RootCollisionNode : NiNode { }

    public class NiBSAnimationNode : NiNode { }

    public class NiBSParticleNode : NiNode { }

    public class NiBillboardNode : NiNode { }

    public class AvoidNode : NiNode { }

    // Geometry
    public abstract class NiGeometry : NiAVObject
    {
        public Ref<NiGeometryData> Data;
        public Ref<NiSkinInstance> SkinInstance;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            Data = NiReaderUtils.ReadRef<NiGeometryData>(r);
            SkinInstance = NiReaderUtils.ReadRef<NiSkinInstance>(r);
        }
    }

    public abstract class NiGeometryData : NiObject
    {
        public ushort NumVertices;
        public bool HasVertices;
        public Vector3[] Vertices;
        public bool HasNormals;
        public Vector3[] Normals;
        public Vector3 Center;
        public float Radius;
        public bool HasVertexColors;
        public Color4[] VertexColors;
        public ushort NumUVSets;
        public bool HasUV;
        public TexCoord[,] UVSets;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            NumVertices = r.ReadUInt16();
            HasVertices = r.ReadBool32();
            if (HasVertices)
            {
                Vertices = new Vector3[NumVertices];
                for (var i = 0; i < Vertices.Length; i++) Vertices[i] = r.ReadVector3();
            }
            HasNormals = r.ReadBool32();
            if (HasNormals)
            {
                Normals = new Vector3[NumVertices];
                for (var i = 0; i < Normals.Length; i++) Normals[i] = r.ReadVector3();
            }
            Center = r.ReadVector3();
            Radius = r.ReadSingle();
            HasVertexColors = r.ReadBool32();
            if (HasVertexColors)
            {
                VertexColors = new Color4[NumVertices];
                for (var i = 0; i < VertexColors.Length; i++) { VertexColors[i] = new Color4(); VertexColors[i].Deserialize(r); }
            }
            NumUVSets = r.ReadUInt16();
            HasUV = r.ReadBool32();
            if (HasUV)
            {
                UVSets = new TexCoord[NumUVSets, NumVertices];
                for (var i = 0; i < NumUVSets; i++)
                    for (var j = 0; j < NumVertices; j++) { UVSets[i, j] = new TexCoord(); UVSets[i, j].Deserialize(r); }
            }
        }
    }

    public abstract class NiTriBasedGeom : NiGeometry
    {
        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
        }
    }

    public abstract class NiTriBasedGeomData : NiGeometryData
    {
        public ushort NumTriangles;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            NumTriangles = r.ReadUInt16();
        }
    }

    public class NiTriShape : NiTriBasedGeom
    {
        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
        }
    }

    public class NiTriShapeData : NiTriBasedGeomData
    {
        public uint NumTrianglePoints;
        public Triangle[] Triangles;
        public ushort NumMatchGroups;
        public MatchGroup[] MatchGroups;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            NumTrianglePoints = r.ReadUInt32();
            Triangles = new Triangle[NumTriangles];
            for (var i = 0; i < Triangles.Length; i++) { Triangles[i] = new Triangle(); Triangles[i].Deserialize(r); }
            NumMatchGroups = r.ReadUInt16();
            MatchGroups = new MatchGroup[NumMatchGroups];
            for (var i = 0; i < MatchGroups.Length; i++) { MatchGroups[i] = new MatchGroup(); MatchGroups[i].Deserialize(r); }
        }
    }

    // Properties
    public abstract class NiProperty : NiObjectNET
    {
        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
        }
    }

    public class NiTexturingProperty : NiProperty
    {
        public NiAVObject.NiFlags Flags;
        public ApplyMode ApplyMode;
        public uint TextureCount;
        //public bool HasBaseTexture;
        public TexDesc BaseTexture;
        //public bool HasDarkTexture;
        public TexDesc DarkTexture;
        //public bool HasDetailTexture;
        public TexDesc DetailTexture;
        //public bool HasGlossTexture;
        public TexDesc GlossTexture;
        //public bool HasGlowTexture;
        public TexDesc GlowTexture;
        //public bool HasBumpMapTexture;
        public TexDesc BumpMapTexture;
        //public bool HasDecal0Texture;
        public TexDesc Decal0Texture;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            Flags = NiReaderUtils.ReadFlags(r);
            ApplyMode = (ApplyMode)r.ReadUInt32();
            TextureCount = r.ReadUInt32();
            var hasBaseTexture = r.ReadBool32();
            if (hasBaseTexture) { BaseTexture = new TexDesc(); BaseTexture.Deserialize(r); }
            var hasDarkTexture = r.ReadBool32();
            if (hasDarkTexture) { DarkTexture = new TexDesc(); DarkTexture.Deserialize(r); }
            var hasDetailTexture = r.ReadBool32();
            if (hasDetailTexture) { DetailTexture = new TexDesc(); DetailTexture.Deserialize(r); }
            var hasGlossTexture = r.ReadBool32();
            if (hasGlossTexture) { GlossTexture = new TexDesc(); GlossTexture.Deserialize(r); }
            var hasGlowTexture = r.ReadBool32();
            if (hasGlowTexture) { GlowTexture = new TexDesc(); GlowTexture.Deserialize(r); }
            var hasBumpMapTexture = r.ReadBool32();
            if (hasBumpMapTexture) { BumpMapTexture = new TexDesc(); BumpMapTexture.Deserialize(r); }
            var hasDecal0Texture = r.ReadBool32();
            if (hasDecal0Texture) { Decal0Texture = new TexDesc(); Decal0Texture.Deserialize(r); }
        }
    }

    public class NiAlphaProperty : NiProperty
    {
        public ushort Flags;
        public byte Threshold;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            Flags = r.ReadUInt16();
            Threshold = r.ReadByte();
        }
    }

    public class NiZBufferProperty : NiProperty
    {
        public ushort Flags;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            Flags = r.ReadUInt16();
        }
    }

    public class NiVertexColorProperty : NiProperty
    {
        public NiAVObject.NiFlags Flags;
        public VertMode VertexMode;
        public LightMode LightingMode;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            Flags = NiReaderUtils.ReadFlags(r);
            VertexMode = (VertMode)r.ReadUInt32();
            LightingMode = (LightMode)r.ReadUInt32();
        }
    }

    public class NiShadeProperty : NiProperty
    {
        public NiAVObject.NiFlags Flags;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            Flags = NiReaderUtils.ReadFlags(r);
        }
    }

    // Data
    public class NiUVData : NiObject
    {
        public KeyGroup<float>[] UVGroups;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            UVGroups = new KeyGroup<float>[4];
            for (var i = 0; i < UVGroups.Length; i++) { UVGroups[i] = new KeyGroup<float>(); UVGroups[i].Deserialize(r); }
        }
    }

    public class NiKeyframeData : NiObject
    {
        public uint NumRotationKeys;
        public KeyType RotationType;
        public QuatKey<Quaternion>[] QuaternionKeys;
        public float UnknownFloat;
        public KeyGroup<float>[] XYZRotations;
        public KeyGroup<Vector3> Translations;
        public KeyGroup<float> Scales;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            NumRotationKeys = r.ReadUInt32();
            if (NumRotationKeys != 0)
            {
                RotationType = (KeyType)r.ReadUInt32();
                if (RotationType != KeyType.XYZ_ROTATION_KEY)
                {
                    QuaternionKeys = new QuatKey<Quaternion>[NumRotationKeys];
                    for (var i = 0; i < QuaternionKeys.Length; i++) { QuaternionKeys[i] = new QuatKey<Quaternion>(); QuaternionKeys[i].Deserialize(r, RotationType); }
                }
                else
                {
                    UnknownFloat = r.ReadSingle();
                    XYZRotations = new KeyGroup<float>[3];
                    for (var i = 0; i < XYZRotations.Length; i++) { XYZRotations[i] = new KeyGroup<float>(); XYZRotations[i].Deserialize(r); }
                }
            }
            Translations = new KeyGroup<Vector3>();
            Translations.Deserialize(r);
            Scales = new KeyGroup<float>();
            Scales.Deserialize(r);
        }
    }

    public class NiColorData : NiObject
    {
        public KeyGroup<Color4> Data;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            Data = new KeyGroup<Color4>();
            Data.Deserialize(r);
        }
    }

    public class NiMorphData : NiObject
    {
        public uint NumMorphs;
        public uint NumVertices;
        public byte RelativeTargets;
        public Morph[] Morphs;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            NumMorphs = r.ReadUInt32();
            NumVertices = r.ReadUInt32();
            RelativeTargets = r.ReadByte();
            Morphs = new Morph[NumMorphs];
            for (var i = 0; i < Morphs.Length; i++) { Morphs[i] = new Morph(); Morphs[i].Deserialize(r, NumVertices); }
        }
    }

    public class NiVisData : NiObject
    {
        public uint NumKeys;
        public Key<byte>[] Keys;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            NumKeys = r.ReadUInt32();
            Keys = new Key<byte>[NumKeys];
            for (var i = 0; i < Keys.Length; i++) { Keys[i] = new Key<byte>(); Keys[i].Deserialize(r, KeyType.LINEAR_KEY); }
        }
    }

    public class NiFloatData : NiObject
    {
        public KeyGroup<float> Data;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            Data = new KeyGroup<float>();
            Data.Deserialize(r);
        }
    }

    public class NiPosData : NiObject
    {
        public KeyGroup<Vector3> Data;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            Data = new KeyGroup<Vector3>();
            Data.Deserialize(r);
        }
    }

    public class NiExtraData : NiObject
    {
        public Ref<NiExtraData> NextExtraData;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            NextExtraData = NiReaderUtils.ReadRef<NiExtraData>(r);
        }
    }

    public class NiStringExtraData : NiExtraData
    {
        public uint BytesRemaining;
        public string Str;

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            BytesRemaining = reader.ReadUInt32();
            Str = reader.ReadL32Encoding();
        }
    }

    public class NiTextKeyExtraData : NiExtraData
    {
        public uint UnknownInt1;
        public uint NumTextKeys;
        public Key<string>[] TextKeys;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            UnknownInt1 = r.ReadUInt32();
            NumTextKeys = r.ReadUInt32();
            TextKeys = new Key<string>[NumTextKeys];
            for (var i = 0; i < TextKeys.Length; i++) { TextKeys[i] = new Key<string>(); TextKeys[i].Deserialize(r, KeyType.LINEAR_KEY); }
        }
    }

    public class NiVertWeightsExtraData : NiExtraData
    {
        public uint NumBytes;
        public ushort NumVertices;
        public float[] Weights;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            NumBytes = r.ReadUInt32();
            NumVertices = r.ReadUInt16();
            Weights = new float[NumVertices];
            for (var i = 0; i < Weights.Length; i++) Weights[i] = r.ReadSingle();
        }
    }

    // Particles
    public class NiParticles : NiGeometry { }

    public class NiParticlesData : NiGeometryData
    {
        public ushort NumParticles;
        public float ParticleRadius;
        public ushort NumActive;
        public bool HasSizes;
        public float[] Sizes;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            NumParticles = r.ReadUInt16();
            ParticleRadius = r.ReadSingle();
            NumActive = r.ReadUInt16();
            HasSizes = r.ReadBool32();
            if (HasSizes)
            {
                Sizes = new float[NumVertices];
                for (var i = 0; i < Sizes.Length; i++) Sizes[i] = r.ReadSingle();
            }
        }
    }

    public class NiRotatingParticles : NiParticles { }

    public class NiRotatingParticlesData : NiParticlesData
    {
        public bool HasRotations;
        public Quaternion[] Rotations;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            HasRotations = r.ReadBool32();
            if (HasRotations)
            {
                Rotations = new Quaternion[NumVertices];
                for (var i = 0; i < Rotations.Length; i++) Rotations[i] = r.ReadQuaternionWFirst();
            }
        }
    }

    public class NiAutoNormalParticles : NiParticles { }

    public class NiAutoNormalParticlesData : NiParticlesData { }

    public class NiParticleSystemController : NiTimeController
    {
        public float Speed;
        public float SpeedRandom;
        public float VerticalDirection;
        public float VerticalAngle;
        public float HorizontalDirection;
        public float HorizontalAngle;
        public Vector3 UnknownNormal;
        public Color4 UnknownColor;
        public float Size;
        public float EmitStartTime;
        public float EmitStopTime;
        public byte UnknownByte;
        public float EmitRate;
        public float Lifetime;
        public float LifetimeRandom;
        public ushort EmitFlags;
        public Vector3 StartRandom;
        public Ptr<NiObject> Emitter;
        public ushort UnknownShort2;
        public float UnknownFloat13;
        public uint UnknownInt1;
        public uint UnknownInt2;
        public ushort UnknownShort3;
        public ushort NumParticles;
        public ushort NumValid;
        public Particle[] Particles;
        public Ref<NiObject> UnknownLink;
        public Ref<NiParticleModifier> ParticleExtra;
        public Ref<NiObject> UnknownLink2;
        public byte Trailer;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            Speed = r.ReadSingle();
            SpeedRandom = r.ReadSingle();
            VerticalDirection = r.ReadSingle();
            VerticalAngle = r.ReadSingle();
            HorizontalDirection = r.ReadSingle();
            HorizontalAngle = r.ReadSingle();
            UnknownNormal = r.ReadVector3();
            UnknownColor = new Color4();
            UnknownColor.Deserialize(r);
            Size = r.ReadSingle();
            EmitStartTime = r.ReadSingle();
            EmitStopTime = r.ReadSingle();
            UnknownByte = r.ReadByte();
            EmitRate = r.ReadSingle();
            Lifetime = r.ReadSingle();
            LifetimeRandom = r.ReadSingle();
            EmitFlags = r.ReadUInt16();
            StartRandom = r.ReadVector3();
            Emitter = NiReaderUtils.ReadPtr<NiObject>(r);
            UnknownShort2 = r.ReadUInt16();
            UnknownFloat13 = r.ReadSingle();
            UnknownInt1 = r.ReadUInt32();
            UnknownInt2 = r.ReadUInt32();
            UnknownShort3 = r.ReadUInt16();
            NumParticles = r.ReadUInt16();
            NumValid = r.ReadUInt16();
            Particles = new Particle[NumParticles];
            for (var i = 0; i < Particles.Length; i++)
            {
                Particles[i] = new Particle();
                Particles[i].Deserialize(r);
            }
            UnknownLink = NiReaderUtils.ReadRef<NiObject>(r);
            ParticleExtra = NiReaderUtils.ReadRef<NiParticleModifier>(r);
            UnknownLink2 = NiReaderUtils.ReadRef<NiObject>(r);
            Trailer = r.ReadByte();
        }
    }

    public class NiBSPArrayController : NiParticleSystemController { }

    // Particle Modifiers
    public abstract class NiParticleModifier : NiObject
    {
        public Ref<NiParticleModifier> NextModifier;
        public Ptr<NiParticleSystemController> Controller;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            NextModifier = NiReaderUtils.ReadRef<NiParticleModifier>(r);
            Controller = NiReaderUtils.ReadPtr<NiParticleSystemController>(r);
        }
    }

    public class NiGravity : NiParticleModifier
    {
        public float UnknownFloat1;
        public float Force;
        public FieldType Type;
        public Vector3 Position;
        public Vector3 Direction;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            UnknownFloat1 = r.ReadSingle();
            Force = r.ReadSingle();
            Type = (FieldType)r.ReadUInt32();
            Position = r.ReadVector3();
            Direction = r.ReadVector3();
        }
    }

    public class NiParticleBomb : NiParticleModifier
    {
        public float Decay;
        public float Duration;
        public float DeltaV;
        public float Start;
        public DecayType DecayType;
        public Vector3 Position;
        public Vector3 Direction;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            Decay = r.ReadSingle();
            Duration = r.ReadSingle();
            DeltaV = r.ReadSingle();
            Start = r.ReadSingle();
            DecayType = (DecayType)r.ReadUInt32();
            Position = r.ReadVector3();
            Direction = r.ReadVector3();
        }
    }

    public class NiParticleColorModifier : NiParticleModifier
    {
        public Ref<NiColorData> ColorData;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            ColorData = NiReaderUtils.ReadRef<NiColorData>(r);
        }
    }

    public class NiParticleGrowFade : NiParticleModifier
    {
        public float Grow;
        public float Fade;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            Grow = r.ReadSingle();
            Fade = r.ReadSingle();
        }
    }

    public class NiParticleMeshModifier : NiParticleModifier
    {
        public uint NumParticleMeshes;
        public Ref<NiAVObject>[] ParticleMeshes;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            NumParticleMeshes = r.ReadUInt32();
            ParticleMeshes = new Ref<NiAVObject>[NumParticleMeshes];
            for (var i = 0; i < ParticleMeshes.Length; i++)
                ParticleMeshes[i] = NiReaderUtils.ReadRef<NiAVObject>(r);
        }
    }

    public class NiParticleRotation : NiParticleModifier
    {
        public byte RandomInitialAxis;
        public Vector3 InitialAxis;
        public float RotationSpeed;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            RandomInitialAxis = r.ReadByte();
            InitialAxis = r.ReadVector3();
            RotationSpeed = r.ReadSingle();
        }
    }

    // Controllers
    public abstract class NiTimeController : NiObject
    {
        public Ref<NiTimeController> NextController;
        public ushort Flags;
        public float Frequency;
        public float Phase;
        public float StartTime;
        public float StopTime;
        public Ptr<NiObjectNET> Target;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            NextController = NiReaderUtils.ReadRef<NiTimeController>(r);
            Flags = r.ReadUInt16();
            Frequency = r.ReadSingle();
            Phase = r.ReadSingle();
            StartTime = r.ReadSingle();
            StopTime = r.ReadSingle();
            Target = NiReaderUtils.ReadPtr<NiObjectNET>(r);
        }
    }

    public class NiUVController : NiTimeController
    {
        public ushort UnknownShort;
        public Ref<NiUVData> Data;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            UnknownShort = r.ReadUInt16();
            Data = NiReaderUtils.ReadRef<NiUVData>(r);
        }
    }

    public abstract class NiInterpController : NiTimeController { }

    public abstract class NiSingleInterpController : NiInterpController { }

    public class NiKeyframeController : NiSingleInterpController
    {
        public Ref<NiKeyframeData> Data;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            Data = NiReaderUtils.ReadRef<NiKeyframeData>(r);
        }
    }

    public class NiGeomMorpherController : NiInterpController
    {
        public Ref<NiMorphData> Data;
        public byte AlwaysUpdate;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            Data = NiReaderUtils.ReadRef<NiMorphData>(r);
            AlwaysUpdate = r.ReadByte();
        }
    }

    public abstract class NiBoolInterpController : NiSingleInterpController { }

    public class NiVisController : NiBoolInterpController
    {
        public Ref<NiVisData> Data;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            Data = NiReaderUtils.ReadRef<NiVisData>(r);
        }
    }

    public abstract class NiFloatInterpController : NiSingleInterpController { }

    public class NiAlphaController : NiFloatInterpController
    {
        public Ref<NiFloatData> Data;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            Data = NiReaderUtils.ReadRef<NiFloatData>(r);
        }
    }

    // Skin Stuff
    public class NiSkinInstance : NiObject
    {
        public Ref<NiSkinData> Data;
        public Ptr<NiNode> SkeletonRoot;
        public uint NumBones;
        public Ptr<NiNode>[] Bones;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            Data = NiReaderUtils.ReadRef<NiSkinData>(r);
            SkeletonRoot = NiReaderUtils.ReadPtr<NiNode>(r);
            NumBones = r.ReadUInt32();
            Bones = new Ptr<NiNode>[NumBones];
            for (var i = 0; i < Bones.Length; i++)
                Bones[i] = NiReaderUtils.ReadPtr<NiNode>(r);
        }
    }

    public class NiSkinData : NiObject
    {
        public SkinTransform SkinTransform;
        public uint NumBones;
        public Ref<NiSkinPartition> SkinPartition;
        public SkinData[] BoneList;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            SkinTransform = new SkinTransform();
            SkinTransform.Deserialize(r);
            NumBones = r.ReadUInt32();
            SkinPartition = NiReaderUtils.ReadRef<NiSkinPartition>(r);
            BoneList = new SkinData[NumBones];
            for (var i = 0; i < BoneList.Length; i++)
            {
                BoneList[i] = new SkinData();
                BoneList[i].Deserialize(r);
            }
        }
    }

    public class NiSkinPartition : NiObject { }

    // Miscellaneous
    public abstract class NiTexture : NiObjectNET
    {
        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
        }
    }

    public class NiSourceTexture : NiTexture
    {
        public byte UseExternal;
        public string FileName;
        public PixelLayout PixelLayout;
        public MipMapFormat UseMipMaps;
        public AlphaFormat AlphaFormat;
        public byte IsStatic;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            UseExternal = r.ReadByte();
            FileName = r.ReadL32Encoding();
            PixelLayout = (PixelLayout)r.ReadUInt32();
            UseMipMaps = (MipMapFormat)r.ReadUInt32();
            AlphaFormat = (AlphaFormat)r.ReadUInt32();
            IsStatic = r.ReadByte();
        }
    }

    public abstract class NiPoint3InterpController : NiSingleInterpController
    {
        public Ref<NiPosData> Data;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            Data = NiReaderUtils.ReadRef<NiPosData>(r);
        }
    }

    public class NiMaterialProperty : NiProperty
    {
        public NiAVObject.NiFlags Flags;
        public Color3 AmbientColor;
        public Color3 DiffuseColor;
        public Color3 SpecularColor;
        public Color3 EmissiveColor;
        public float Glossiness;
        public float Alpha;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            Flags = NiReaderUtils.ReadFlags(r);
            AmbientColor = new Color3();
            AmbientColor.Deserialize(r);
            DiffuseColor = new Color3();
            DiffuseColor.Deserialize(r);
            SpecularColor = new Color3();
            SpecularColor.Deserialize(r);
            EmissiveColor = new Color3();
            EmissiveColor.Deserialize(r);
            Glossiness = r.ReadSingle();
            Alpha = r.ReadSingle();
        }
    }

    public class NiMaterialColorController : NiPoint3InterpController { }

    public abstract class NiDynamicEffect : NiAVObject
    {
        uint NumAffectedNodeListPointers;
        uint[] AffectedNodeListPointers;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            NumAffectedNodeListPointers = r.ReadUInt32();
            AffectedNodeListPointers = new uint[NumAffectedNodeListPointers];
            for (var i = 0; i < AffectedNodeListPointers.Length; i++)
                AffectedNodeListPointers[i] = r.ReadUInt32();
        }
    }

    public class NiTextureEffect : NiDynamicEffect
    {
        public Matrix4x4 ModelProjectionMatrix;
        public Vector3 ModelProjectionTransform;
        public TexFilterMode TextureFiltering;
        public TexClampMode TextureClamping;
        public EffectType TextureType;
        public CoordGenType CoordinateGenerationType;
        public Ref<NiSourceTexture> SourceTexture;
        public byte ClippingPlane;
        public Vector3 UnknownVector;
        public float UnknownFloat;
        public short PS2L;
        public short PS2K;
        public ushort UnknownShort;

        public override void Deserialize(BinaryReader r)
        {
            base.Deserialize(r);
            ModelProjectionMatrix = NiReaderUtils.Read3x3RotationMatrix(r);
            ModelProjectionTransform = r.ReadVector3();
            TextureFiltering = (TexFilterMode)r.ReadUInt32();
            TextureClamping = (TexClampMode)r.ReadUInt32();
            TextureType = (EffectType)r.ReadUInt32();
            CoordinateGenerationType = (CoordGenType)r.ReadUInt32();
            SourceTexture = NiReaderUtils.ReadRef<NiSourceTexture>(r);
            ClippingPlane = r.ReadByte();
            UnknownVector = r.ReadVector3();
            UnknownFloat = r.ReadSingle();
            PS2L = r.ReadInt16();
            PS2K = r.ReadInt16();
            UnknownShort = r.ReadUInt16();
        }
    }
}