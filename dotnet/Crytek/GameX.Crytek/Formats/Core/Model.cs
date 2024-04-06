using GameX.Crytek.Formats.Core.Chunks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using static OpenStack.Debug;

namespace GameX.Crytek.Formats.Core
{
    /// <summary>
    /// CryObject cgf/cga/skin file handler
    /// 
    /// Structure:
    ///   HEADER        <- Provides information about the format of the file
    ///   CHUNKHEADER[] <- Provides information about locations of CHUNKs
    ///   CHUNK[]
    /// </summary>
    public class Model
    {
        /// <summary>
        /// The Root of the loaded object
        /// </summary>
        public ChunkNode RootNode { get; internal set; }

        /// <summary>
        /// Collection of all loaded Chunks
        /// </summary>
        List<ChunkHeader> ChunkHeaders = new List<ChunkHeader>();

        /// <summary>
        /// Lookup Table for Chunks, indexed by ChunkID
        /// </summary>
        public Dictionary<int, Chunk> ChunkMap { get; } = new Dictionary<int, Chunk>();

        /// <summary>
        /// The name of the currently processed file
        /// </summary>
        public string FileName { get; internal set; }

        /// <summary>
        /// The File Signature - CryTek for 3.5 and lower. CrCh for 3.6 and higher
        /// </summary>
        public string FileSignature { get; internal set; }

        /// <summary>
        /// The type of file (geometry or animation)
        /// </summary>
        public FileType FileType { get; internal set; }

        /// <summary>
        /// The version of the file
        /// </summary>
        public FileVersion FileVersion { get; internal set; }

        /// <summary>
        /// Position of the Chunk Header table
        /// </summary>
        public int ChunkTableOffset { get; internal set; }

        /// <summary>
        /// Contains all the information about bones and skinning them.  This a reference to the Cryengine object, since multiple Models can exist for a single object).
        /// </summary>
        public SkinningInfo SkinningInfo { get; set; } = new SkinningInfo();

        /// <summary>
        /// The Bones in the model.  The CompiledBones chunk will have a unique RootBone.
        /// </summary>
        public ChunkCompiledBones Bones { get; internal set; }

        public uint NumChunks { get; internal set; }

        Dictionary<int, ChunkNode> _nodeMap;
        /// <summary>
        /// Node map for this model only.
        /// </summary>
        public Dictionary<int, ChunkNode> NodeMap      // This isn't right.  Nodes can have duplicate names.
        {
            get
            {
                if (_nodeMap != null) return _nodeMap;
                _nodeMap = new Dictionary<int, ChunkNode>();
                ChunkNode rootNode = null;
                RootNode = rootNode ??= RootNode; // Each model will have it's own rootnode.
                foreach (var node in ChunkMap.Values.Where(c => c.ChunkType == ChunkType.Node).OrderBy(c => c.ID).Select(c => c as ChunkNode))
                {
                    // Preserve existing parents
                    if (_nodeMap.TryGetValue(node.ID, out var knownNode))
                    {
                        var parentNode = knownNode.ParentNode;
                        if (parentNode != null) parentNode = _nodeMap[parentNode.ID];
                        node.ParentNode = parentNode;
                    }
                    _nodeMap[node.ID] = node;
                }
                return _nodeMap;
            }
        }

        public bool HasBones => Bones != null;

        public bool HasGeometry
            => ChunkMap.Select(n => n.Value.ChunkType).Any(s => s == ChunkType.Mesh || s == ChunkType.MeshIvo);

        /// <summary>
        /// Load a cgf/cga/skin file
        /// </summary>
        /// <param name="fileName"></param>
        public Model((string fileName, Stream stream) file)
        {
            FileName = Path.GetFileName(file.fileName);
            Console.Title = $"Processing {FileName}...";
            using var r = new BinaryReader(file.stream);
            // Get the header. This isn't essential for .cgam files, but we need this info to find the version and offset to the chunk table
            ReadFileHeader(r);
            ReadChunkHeaders(r);
            ReadChunks(r);
        }

        /// <summary>
        /// Read FileHeader from stream
        /// </summary>
        /// <param name="r"></param>
        void ReadFileHeader(BinaryReader r)
        {
            // FILESIGNATURE V3.6+ : Version 3.6 or later
            r.BaseStream.Seek(0, SeekOrigin.Begin);
            FileSignature = r.ReadFYString(4);
            if (FileSignature == "CrCh")
            {
                FileVersion = (FileVersion)r.ReadUInt32(); // 0x746
                NumChunks = r.ReadUInt32(); // number of Chunks in the chunk table
                ChunkTableOffset = r.ReadInt32(); // location of the chunk table
                return;
            }
            else if (FileSignature == "#ivo")
            {
                FileVersion = (FileVersion)r.ReadUInt32(); // 0x0900
                NumChunks = r.ReadUInt32();
                ChunkTableOffset = r.ReadInt32();
                CreateDummyRootNode();
                return;
            }

            // FILESIGNATURE V3.5- : Version 3.5 or earlier
            r.BaseStream.Seek(0, SeekOrigin.Begin);
            FileSignature = r.ReadFYString(8);
            if (FileSignature == "CryTek")
            {
                FileType = (FileType)r.ReadUInt32();
                FileVersion = (FileVersion)r.ReadUInt32(); // 0x744 | 0x745
                ChunkTableOffset = r.ReadInt32() + 4;
                NumChunks = r.ReadUInt32(); // number of Chunks in the chunk table
                return;
            }
            else throw new NotSupportedException($"Unsupported FileSignature {FileSignature}");
        }

        void CreateDummyRootNode()
        {
            var rootNode = new ChunkNode_823
            {
                Name = FileName,
                ObjectNodeID = 2,      // No node IDs in #ivo files
                ParentNodeID = ~0,     // No parent
                __NumChildren = 0,     // Single object
                MatID = 0,
                Transform = Matrix4x4.Identity,
                ChunkType = ChunkType.Node,
                ID = 1
            };
            RootNode = rootNode;
            rootNode._model = this;
            ChunkMap.Add(1, rootNode);
        }

        /// <summary>
        /// Read HeaderTable from stream
        /// </summary>
        /// <typeparam name="TChunkHeader"></typeparam>
        /// <param name="r">BinaryReader of file being read</param>
        void ReadChunkHeaders(BinaryReader r)
        {
            r.BaseStream.Seek(ChunkTableOffset, SeekOrigin.Begin);
            for (var i = 0; i < NumChunks; i++)
            {
                var header = Chunk.New<ChunkHeader>((uint)FileVersion);
                header.Read(r);
                ChunkHeaders.Add(header);
            }
        }

        /// <summary>
        /// Reads all the chunks in the Cryengine file.
        /// </summary>
        /// <param name="r">BinaryReader for the Cryengine file.</param>
        void ReadChunks(BinaryReader r)
        {
            foreach (var chunkHeaderItem in ChunkHeaders)
            {
                var chunk = Chunk.New(chunkHeaderItem.ChunkType, chunkHeaderItem.Version);
                ChunkMap[chunkHeaderItem.ID] = chunk;
                chunk.Load(this, chunkHeaderItem);
                chunk.Read(r);
                // Ensure we read to end of structure
                chunk.SkipBytesRemaining(r);

                // Assume first node read in Model[0] is root node.  This may be bad if they aren't in order!
                if (chunkHeaderItem.ChunkType == ChunkType.Node && RootNode == null) RootNode = chunk as ChunkNode;

                // Add Bones to the model.  We are assuming there is only one CompiledBones chunk per file.
                if (chunkHeaderItem.ChunkType == ChunkType.CompiledBones ||
                    chunkHeaderItem.ChunkType == ChunkType.CompiledBonesSC ||
                    chunkHeaderItem.ChunkType == ChunkType.CompiledBonesIvo)
                    Bones = chunk as ChunkCompiledBones;
            }
        }

        #region Log
#if LOG
        /// <summary>
        /// Output File Header to console for testing
        /// </summary>
        public void LogFileHeader()
        {
            Log($"*** HEADER ***");
            Log($"    Header Filesignature: {FileSignature}");
            Log($"    FileType:             {FileType:X}");
            Log($"    ChunkVersion:         {FileVersion:X}");
            Log($"    ChunkTableOffset:     {ChunkTableOffset:X}");
            Log($"    NumChunks:            {NumChunks:X}");
            Log($"*** END HEADER ***");
            return;
        }

        /// <summary>
        /// Output Chunk Table to console for testing
        /// </summary>
        public void LogChunkTable()
        {
            Log("*** Chunk Header Table***");
            Log("Chunk Type              Version   ID        Size      Offset    ");
            foreach (var chkHdr in ChunkHeaders) Log($"{chkHdr.ChunkType,-24:X}{chkHdr.Version,-10:X}{chkHdr.ID,-10:X}{chkHdr.Size,-10:X}{chkHdr.Offset,-10:X}");
            Console.WriteLine("*** Chunk Header Table***");
            Console.WriteLine("Chunk Type              Version   ID        Size      Offset    ");
            foreach (var chkHdr in ChunkHeaders) Console.WriteLine($"{chkHdr.ChunkType,-24:X}{chkHdr.Version,-10:X}{chkHdr.ID,-10:X}{chkHdr.Size,-10:X}{chkHdr.Offset,-10:X}");
        }
#endif
        #endregion
    }
}
