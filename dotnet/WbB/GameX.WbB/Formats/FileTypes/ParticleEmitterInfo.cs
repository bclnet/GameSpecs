using GameX.WbB.Formats.Props;
using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

namespace GameX.WbB.Formats.FileTypes
{
    /// <summary>
    /// These are client_portal.dat files starting with 0x32. 
    /// </summary>
    [PakFileType(PakFileType.ParticleEmitter)]
    public class ParticleEmitterInfo : FileType, IHaveMetaInfo
    {
        public readonly uint Unknown;
        public readonly EmitterType EmitterType;
        public readonly ParticleType ParticleType;
        public readonly uint GfxObjId; public readonly uint HwGfxObjId;
        public readonly double Birthrate;
        public readonly int MaxParticles; public readonly int InitialParticles; public readonly int TotalParticles;
        public readonly double TotalSeconds;
        public readonly double Lifespan; public readonly double LifespanRand;
        public readonly Vector3 OffsetDir; public readonly float MinOffset; public readonly float MaxOffset;
        public readonly Vector3 A; public readonly float MinA; public readonly float MaxA;
        public readonly Vector3 B; public readonly float MinB; public readonly float MaxB;
        public readonly Vector3 C; public readonly float MinC; public readonly float MaxC;
        public readonly float StartScale; public readonly float FinalScale; public readonly float ScaleRand;
        public readonly float StartTrans; public readonly float FinalTrans; public readonly float TransRand;
        public readonly int IsParentLocal;

        public ParticleEmitterInfo(BinaryReader r)
        {
            Id = r.ReadUInt32();
            Unknown = r.ReadUInt32();
            EmitterType = (EmitterType)r.ReadInt32();
            ParticleType = (ParticleType)r.ReadInt32();
            GfxObjId = r.ReadUInt32(); HwGfxObjId = r.ReadUInt32();
            Birthrate = r.ReadDouble();
            MaxParticles = r.ReadInt32(); InitialParticles = r.ReadInt32(); TotalParticles = r.ReadInt32();
            TotalSeconds = r.ReadDouble();
            Lifespan = r.ReadDouble(); LifespanRand = r.ReadDouble();
            OffsetDir = r.ReadVector3(); MinOffset = r.ReadSingle(); MaxOffset = r.ReadSingle();
            A = r.ReadVector3(); MinA = r.ReadSingle(); MaxA = r.ReadSingle();
            B = r.ReadVector3(); MinB = r.ReadSingle(); MaxB = r.ReadSingle();
            C = r.ReadVector3(); MinC = r.ReadSingle(); MaxC = r.ReadSingle();
            StartScale = r.ReadSingle(); FinalScale = r.ReadSingle(); ScaleRand = r.ReadSingle();
            StartTrans = r.ReadSingle(); FinalTrans = r.ReadSingle(); TransRand = r.ReadSingle();
            IsParentLocal = r.ReadInt32();
        }

        //: FileTypes.ParticleEmitterInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"{nameof(ParticleEmitterInfo)}: {Id:X8}", items: new List<MetaInfo> {
                    new MetaInfo($"EmitterType: {EmitterType}"),
                    new MetaInfo($"ParticleType: {ParticleType}"),
                    new MetaInfo($"GfxObjId: {GfxObjId:X8}", clickable: true),
                    new MetaInfo($"HWGfxObjId: {HwGfxObjId:X8}", clickable: true),
                    new MetaInfo($"Birthrate: {Birthrate}"),
                    new MetaInfo($"MaxParticles: {MaxParticles} InitialParticles: {InitialParticles} TotalParticles: {TotalParticles}"),
                    new MetaInfo($"TotalSeconds: {TotalSeconds}"),
                    new MetaInfo($"Lifespan: {Lifespan} LifespanRand: {LifespanRand}"),
                    new MetaInfo($"OffsetDir: {OffsetDir} MinOffset: {MinOffset} MaxOffset: {MaxOffset}"),
                    new MetaInfo($"A: {A} MinA: {MinA}: MaxA: {MaxA}"),
                    new MetaInfo($"B: {B} MinB: {MinB}: MaxB: {MaxB}"),
                    new MetaInfo($"C: {C} MinC: {MinC}: MaxC: {MaxC}"),
                    new MetaInfo($"StartScale: {StartScale} FinalScale: {FinalScale}: ScaleRand: {ScaleRand}"),
                    new MetaInfo($"StartTrans: {StartTrans} FinalTrans: {FinalTrans}: TransRand: {TransRand}"),
                    new MetaInfo($"IsParentLocal: {IsParentLocal}"),
                })
            };
            return nodes;
        }

        public override string ToString()
        {
            var b = new StringBuilder();
            b.AppendLine("------------------");
            b.AppendLine($"ID: {Id:X8}");
            b.AppendLine($"EmitterType: {EmitterType}");
            b.AppendLine($"ParticleType: {ParticleType}");
            b.AppendLine($"GfxObjID: {GfxObjId:X8} HWGfxObjID: {HwGfxObjId:X8}");
            b.AppendLine($"Birthrate: {Birthrate}");
            b.AppendLine($"MaxParticles: {MaxParticles} InitialParticles: {InitialParticles} TotalParticles: {TotalParticles}");
            b.AppendLine($"TotalSeconds: {TotalSeconds}");
            b.AppendLine($"Lifespan: {Lifespan} LifespanRand: {LifespanRand}");
            b.AppendLine($"OffsetDir: {OffsetDir} MinOffset: {MinOffset} MaxOffset: {MaxOffset}");
            b.AppendLine($"A: {A} MinA: {MinA} MaxA: {MaxA}");
            b.AppendLine($"B: {B} MinB: {MinB} MaxB: {MaxB}");
            b.AppendLine($"C: {C} MinC: {MinC} MaxC: {MaxC}");
            b.AppendLine($"StartScale: {StartScale} FinalScale: {FinalScale} ScaleRand: {ScaleRand}");
            b.AppendLine($"StartTrans: {StartTrans} FinalTrans: {FinalTrans} TransRand: {TransRand}");
            b.AppendLine($"IsParentLocal: {IsParentLocal}");
            return b.ToString();
        }
    }
}
