using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static OpenStack.Debug;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public abstract class Chunk : IBinaryChunk
    {
        protected static readonly Random _rnd = new Random();
        protected static readonly HashSet<int> _alreadyPickedRandoms = new HashSet<int>();

        static readonly Dictionary<Type, Dictionary<uint, Func<dynamic>>> _chunkFactoryCache = new Dictionary<Type, Dictionary<uint, Func<dynamic>>> { };

        internal ChunkHeader _header;
        internal Model _model;

        /// <summary>
        /// Position of the start of the chunk
        /// </summary>
        public uint Offset { get; internal set; }
        /// <summary>
        /// The Type of the Chunk
        /// </summary>
        public ChunkType ChunkType { get; internal set; }
        /// <summary>
        /// The Version of this Chunk
        /// </summary>
        internal uint Version;
        /// <summary>
        /// The ID of this Chunk
        /// </summary>
        internal int ID;
        /// <summary>
        /// The Size of this Chunk (in Bytes)
        /// </summary>
        internal uint Size;
        /// <summary>
        /// Size of the data in the chunk. This is the chunk size, minus the header (if there is one)
        /// </summary>
        public uint DataSize { get; set; }

        internal Dictionary<long, byte> SkippedBytes = new Dictionary<long, byte> { };

        public static Chunk New(ChunkType chunkType, uint version)
            => chunkType switch
            {
                ChunkType.SourceInfo => New<ChunkSourceInfo>(version),
                ChunkType.Timing => New<ChunkTimingFormat>(version),
                ChunkType.ExportFlags => New<ChunkExportFlags>(version),
                ChunkType.MtlName => New<ChunkMtlName>(version),
                ChunkType.DataStream => New<ChunkDataStream>(version),
                ChunkType.Mesh => New<ChunkMesh>(version),
                ChunkType.MeshSubsets => New<ChunkMeshSubsets>(version),
                ChunkType.Node => New<ChunkNode>(version),
                ChunkType.Helper => New<ChunkHelper>(version),
                ChunkType.Controller => New<ChunkController>(version),
                ChunkType.SceneProps => New<ChunkSceneProp>(version),
                ChunkType.MeshPhysicsData => New<ChunkMeshPhysicsData>(version),
                ChunkType.BoneAnim => New<ChunkBoneAnim>(version),
                // Compiled chunks
                ChunkType.CompiledBones => New<ChunkCompiledBones>(version),
                ChunkType.CompiledPhysicalProxies => New<ChunkCompiledPhysicalProxies>(version),
                ChunkType.CompiledPhysicalBones => New<ChunkCompiledPhysicalBones>(version),
                ChunkType.CompiledIntSkinVertices => New<ChunkCompiledIntSkinVertices>(version),
                ChunkType.CompiledMorphTargets => New<ChunkCompiledMorphTargets>(version),
                ChunkType.CompiledExt2IntMap => New<ChunkCompiledExtToIntMap>(version),
                ChunkType.CompiledIntFaces => New<ChunkCompiledIntFaces>(version),
                // Star Citizen equivalents
                ChunkType.CompiledBonesSC => New<ChunkCompiledBones>(version),
                ChunkType.CompiledPhysicalBonesSC => New<ChunkCompiledPhysicalBones>(version),
                ChunkType.CompiledExt2IntMapSC => New<ChunkCompiledExtToIntMap>(version),
                ChunkType.CompiledIntFacesSC => New<ChunkCompiledIntFaces>(version),
                ChunkType.CompiledIntSkinVerticesSC => New<ChunkCompiledIntSkinVertices>(version),
                ChunkType.CompiledMorphTargetsSC => New<ChunkCompiledMorphTargets>(version),
                ChunkType.CompiledPhysicalProxiesSC => New<ChunkCompiledPhysicalProxies>(version),
                // Star Citizen IVO chunks
                ChunkType.MtlNameIvo => New<ChunkMtlName>(version),
                ChunkType.CompiledBonesIvo => New<ChunkCompiledBones>(version),
                ChunkType.MeshIvo => New<ChunkMesh>(version),
                ChunkType.IvoSkin => New<ChunkIvoSkin>(version),
                // Star Citizen
                ChunkType.BinaryXmlDataSC => New<ChunkBinaryXmlData>(version),
                // Old chunks
                ChunkType.BoneNameList => New<ChunkBoneNameList>(version),
                ChunkType.MeshMorphTarget => New<ChunkMeshMorphTargets>(version),
                ChunkType.Mtl => new ChunkUnknown(),// Obsolete. Not used
                _ => new ChunkUnknown(),
            };

        public static T New<T>(uint version) where T : Chunk
        {
            if (!_chunkFactoryCache.TryGetValue(typeof(T), out var versionMap)) _chunkFactoryCache[typeof(T)] = versionMap = new Dictionary<uint, Func<dynamic>> { };
            if (!versionMap.TryGetValue(version, out var factory))
            {
                var targetType = typeof(T).Assembly.GetTypes()
                    .FirstOrDefault(type => !type.IsAbstract && type.IsClass && !type.IsGenericType && typeof(T).IsAssignableFrom(type) && type.Name == $"{typeof(T).Name}_{version:X}");
                if (targetType != null) factory = () => Activator.CreateInstance(targetType) as T;
                _chunkFactoryCache[typeof(T)][version] = factory;
            }
            return (factory?.Invoke() as T) ?? throw new NotSupportedException($"Version {version:X} of {typeof(T).Name} is not supported");
        }

        public void Load(Model model, ChunkHeader header)
        {
            _model = model;
            _header = header;
        }

        public void SkipBytes(BinaryReader r, long bytesToSkip)
        {
            if (bytesToSkip == 0) return;
            if (r.BaseStream.Position > Offset + Size && Size > 0) Log($"Buffer Overflow in {GetType().Name} 0x{ID:X} ({r.BaseStream.Position - Offset - Size} bytes)");
            if (r.BaseStream.Length < Offset + Size) Log($"Corrupt Headers in {GetType().Name} 0x{ID:X}");
            //if (!bytesToSkip.HasValue) bytesToSkip = Size - Math.Max(r.BaseStream.Position - Offset, 0);
            for (var i = 0L; i < bytesToSkip; i++) SkippedBytes[r.BaseStream.Position - Offset] = r.ReadByte();
        }
        public void SkipBytesRemaining(BinaryReader r) => SkipBytes(r, Size - Math.Max(r.BaseStream.Position - Offset, 0));

        public virtual void Read(BinaryReader r)
        {
            if (r == null) throw new ArgumentNullException(nameof(r));
            ChunkType = _header.ChunkType;
            Version = _header.Version;
            Offset = _header.Offset;
            ID = _header.ID;
            Size = _header.Size;
            DataSize = Size; // For SC files, there is no header in chunks.  But need Datasize to calculate things.

            r.BaseStream.Seek(_header.Offset, SeekOrigin.Begin);

            // Star Citizen files don't have the type, version, offset and ID at the start of a chunk, so don't read them.
            if (_model.FileVersion == FileVersion.CryTek_3_4 || _model.FileVersion == FileVersion.CryTek_3_5)
            {
                ChunkType = (ChunkType)r.ReadUInt32();
                Version = r.ReadUInt32();
                Offset = r.ReadUInt32();
                ID = r.ReadInt32();
                DataSize = Size - 16;
            }
            if (Offset != _header.Offset || Size != _header.Size)
            {
                Log($"Conflict in chunk definition");
                Log($"{_header.Offset:X}+{_header.Size:X}");
                Log($"{Offset:X}+{Size:X}");
            }
        }

        /// <summary>
        /// Gets a link to the SkinningInfo model.
        /// </summary>
        /// <returns>Link to the SkinningInfo model.</returns>
        public SkinningInfo GetSkinningInfo()
        {
            if (_model.SkinningInfo == null) _model.SkinningInfo = new SkinningInfo();
            return _model.SkinningInfo;
        }

        public virtual void Write(BinaryWriter w) => throw new NotImplementedException();

        public override string ToString()
            => $@"Chunk Type: {ChunkType}, Ver: {Version:X}, Offset: {Offset:X}, ID: {ID:X}, Size: {Size}";

        protected static int GetNextRandom()
        {
            var available = false;
            var rand = 0;
            while (!available)
            {
                rand = _rnd.Next(100000);
                if (!_alreadyPickedRandoms.Contains(rand)) { _alreadyPickedRandoms.Add(rand); available = true; }
            }
            return rand;
        }

        #region Log
#if LOG
        public virtual void LogChunk()
        {
            Log($"*** CHUNK ***");
            Log($"    ChunkType: {ChunkType}");
            Log($"    ChunkVersion: {Version:X}");
            Log($"    Offset: {Offset:X}");
            Log($"    ID: {ID:X}");
            Log($"    Size: {Size:X}");
            Log($"*** END CHUNK ***");
        }
#endif
        #endregion
    }
}