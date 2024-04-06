using GameX.Crytek.Formats.Core;
using GameX.Crytek.Formats.Core.Chunks;
using GameX.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static OpenStack.Debug;

namespace GameX.Crytek.Formats
{
    public partial class CryFile
    {
        public static Task<object> Factory(BinaryReader r, FileSource m, PakFile s)
        {
            var file = new CryFile(m.Path);
            file.LoadFromPak(r.BaseStream, m, s);
            return Task.FromResult((object)file);
        }

        /// <summary>
        /// File extensions processed by CryEngine
        /// </summary>
        static HashSet<string> _validExtensions = new HashSet<string>
        {
            ".soc",
            ".cgf",
            ".cga",
            ".chr",
            ".skin",
            ".anim"
        };

        public CryFile(string fileName)
        {
            // Validate file extension - handles .cgam / skinm
            if (!_validExtensions.Contains(Path.GetExtension(fileName))) throw new FileLoadException("Warning: Unsupported file extension - please use a cga, cgf, chr, skin or anim file", fileName);
            InputFile = fileName;
        }

        public void LoadFromFile()
        {
            var files = new List<(string, Stream)> { (InputFile, File.Open(InputFile, FileMode.Open)) };
            var mFilePath = Path.ChangeExtension(InputFile, $"{Path.GetExtension(InputFile)}m");
            if (File.Exists(mFilePath))
            {
                Log($"Found geometry file {Path.GetFileName(mFilePath)}");
                files.Add((mFilePath, File.Open(mFilePath, FileMode.Open))); // Add to list of files to process
            }
            LoadAsync(null, files, FindMaterialFromFile, path => Task.FromResult<(string, Stream)>((path, File.Open(path, FileMode.Open)))).Wait();
        }

        public void LoadFromPak(Stream stream, FileSource metadata, PakFile pak)
        {
            var files = new List<(string, Stream)> { (InputFile, stream) };
            var mFilePath = Path.ChangeExtension(InputFile, $"{Path.GetExtension(InputFile)}m");
            if (pak.Contains(mFilePath))
            {
                Log($"Found geometry file {Path.GetFileName(mFilePath)}");
                files.Add((mFilePath, pak.LoadFileData(mFilePath).Result)); // Add to list of files to process
            }
            LoadAsync(pak, files, FindMaterialFromPak, path => Task.FromResult<(string, Stream)>((path, pak.LoadFileData(path).Result))).Wait();
        }

        static string FindMaterialFromFile(PakFile pak, string materialPath, string fileName, string cleanName)
        {
            // First try relative to file being processed
            if (Path.GetExtension(materialPath) != ".mtl") materialPath = Path.ChangeExtension(materialPath, "mtl");
            // Then try just the last part of the chunk, relative to the file being processed
            if (!File.Exists(materialPath)) materialPath = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileName(cleanName));
            if (Path.GetExtension(materialPath) != ".mtl") materialPath = Path.ChangeExtension(materialPath, "mtl");
            // Then try relative to the ObjectDir
            if (!File.Exists(materialPath)) materialPath = Path.Combine("Data", cleanName);
            if (Path.GetExtension(materialPath) != ".mtl") materialPath = Path.ChangeExtension(materialPath, "mtl");
            // Then try just the fileName.mtl
            if (!File.Exists(materialPath)) materialPath = fileName;
            if (Path.GetExtension(materialPath) != ".mtl") materialPath = Path.ChangeExtension(materialPath, "mtl");
            // TODO: Try more paths
            return File.Exists(materialPath) ? materialPath : null;
        }

        static string FindMaterialFromPak(PakFile pak, string materialPath, string fileName, string cleanName)
        {
            // First try relative to file being processed
            if (Path.GetExtension(materialPath) != ".mtl") materialPath = Path.ChangeExtension(materialPath, "mtl");
            // Then try just the last part of the chunk, relative to the file being processed
            if (!pak.Contains(materialPath)) materialPath = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileName(cleanName));
            if (Path.GetExtension(materialPath) != ".mtl") materialPath = Path.ChangeExtension(materialPath, "mtl");
            // Then try relative to the ObjectDir
            if (!pak.Contains(materialPath)) materialPath = Path.Combine("Data", cleanName);
            if (Path.GetExtension(materialPath) != ".mtl") materialPath = Path.ChangeExtension(materialPath, "mtl");
            // Then try just the fileName.mtl
            if (!pak.Contains(materialPath)) materialPath = fileName;
            if (Path.GetExtension(materialPath) != ".mtl") materialPath = Path.ChangeExtension(materialPath, "mtl");
            // TODO: Try more paths
            return pak.Contains(materialPath) ? materialPath : null;
        }

        public async Task LoadAsync(PakFile pak, IEnumerable<(string, Stream)> files, Func<PakFile, string, string, string, string> getMaterialPath, Func<string, Task<(string, Stream)>> getFileAsync)
        {
            try
            {
                Models = new List<Model> { };
                foreach (var file in files)
                {
                    // Each file (.cga and .cgam if applicable) will have its own RootNode.  This can cause problems.  .cga files with a .cgam files won't have geometry for the one root node.
                    var model = new Model(file);
                    if (RootNode == null) RootNode = model.RootNode; // This makes the assumption that we read the .cga file before the .cgam file.
                    Bones ??= model.Bones;
                    Models.Add(model);
                }
                SkinningInfo = ConsolidateSkinningInfo();
                // For eanch node with geometry info, populate that node's Mesh Chunk GeometryInfo with the geometry data.
                ConsolidateGeometryInfo();
                // Get the material file name
                var fileName = files.First().Item1;
                foreach (ChunkMtlName mtlChunk in Models.SelectMany(a => a.ChunkMap.Values).Where(c => c.ChunkType == ChunkType.MtlName))
                {
                    // Don't process child or collision materials for now
                    if (mtlChunk.MatType == MtlNameType.Child || mtlChunk.MatType == MtlNameType.Unknown1) continue;
                    // The Replace part is for SC files that point to a _core material file that doesn't exist.
                    var cleanName = mtlChunk.Name.Replace("_core", string.Empty);
                    //
                    string materialFilePath;
                    if (mtlChunk.Name.Contains("default_body"))
                    {
                        // New MWO models for some crazy reason don't put the actual mtl file name in the mtlchunk.  They just have /objects/mechs/default_body
                        // have to assume that it's /objects/mechs/<mechname>/body/<mechname>_body.mtl.  There is also a <mechname>.mtl that contains mtl 
                        // info for hitboxes, but not needed.
                        // TODO:  This isn't right.  Fix it.
                        var charsToClean = cleanName.ToCharArray().Intersect(Path.GetInvalidFileNameChars()).ToArray();
                        if (charsToClean.Length > 0) foreach (char character in charsToClean) cleanName = cleanName.Replace(character.ToString(), string.Empty);
                        materialFilePath = Path.Combine(Path.GetDirectoryName(fileName), cleanName);
                    }
                    else if (mtlChunk.Name.Contains("/") || mtlChunk.Name.Contains("\\"))
                    {
                        // The mtlname has a path.  Most likely starts at the Objects directory.
                        var stringSeparators = new[] { "/", "\\" };
                        // if objectdir is provided, check objectdir + mtlchunk.name
                        materialFilePath = Path.Combine("Data", mtlChunk.Name);
                        //else // object dir not provided, but we have a path.  Just grab the last part of the name and check the dir of the cga file
                        //{
                        //    var r = mtlChunk.Name.Split(stringSeparators, StringSplitOptions.None);
                        //    materialFilePath = r[r.Length - 1];
                        //}
                    }
                    else
                    {
                        var charsToClean = cleanName.ToCharArray().Intersect(Path.GetInvalidFileNameChars()).ToArray();
                        if (charsToClean.Length > 0) foreach (var character in charsToClean) cleanName = cleanName.Replace(character.ToString(), string.Empty);
                        materialFilePath = Path.Combine(Path.GetDirectoryName(fileName), cleanName);
                    }
                    // Populate CryEngine_Core.Material
                    var materialPath = getMaterialPath(pak, materialFilePath, fileName, cleanName);
                    var material = materialPath != null ? Material.FromFile(await getFileAsync(materialPath)) : null;
                    if (material != null)
                    {
                        Log($"Located material file {Path.GetFileName(materialPath)}");
                        Materials = FlattenMaterials(material).Where(m => m.Textures != null).ToArray();
                        // only one material, so it's a material file with no submaterials.  Check and set the name
                        if (Materials.Length == 1) Materials[0].Name = RootNode.Name;
                        return; // Early return - we have the material map
                    }
                    else Log($"Unable to locate material file {mtlChunk.Name}.mtl");
                }
                Log("Unable to locate any material file");
                Materials = new Material[0];
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        void ConsolidateGeometryInfo()
        {
            //foreach (Model model in Models)
            //{
            //    var nodes = model.ChunkNodes;
            //}
        }

        SkinningInfo ConsolidateSkinningInfo()
        {
            var skin = new SkinningInfo();
            foreach (var model in Models)
            {
                skin.HasSkinningInfo = Models.Any(a => a.SkinningInfo.HasSkinningInfo);
                skin.HasBoneMapDatastream = Models.Any(a => a.SkinningInfo.HasBoneMapDatastream);
                if (model.SkinningInfo.IntFaces != null) skin.IntFaces = model.SkinningInfo.IntFaces;
                if (model.SkinningInfo.IntVertices != null) skin.IntVertices = model.SkinningInfo.IntVertices;
                if (model.SkinningInfo.LookDirectionBlends != null) skin.LookDirectionBlends = model.SkinningInfo.LookDirectionBlends;
                if (model.SkinningInfo.MorphTargets != null) skin.MorphTargets = model.SkinningInfo.MorphTargets;
                if (model.SkinningInfo.PhysicalBoneMeshes != null) skin.PhysicalBoneMeshes = model.SkinningInfo.PhysicalBoneMeshes;
                if (model.SkinningInfo.BoneEntities != null) skin.BoneEntities = model.SkinningInfo.BoneEntities;
                if (model.SkinningInfo.BoneMapping != null) skin.BoneMapping = model.SkinningInfo.BoneMapping;
                if (model.SkinningInfo.Collisions != null) skin.Collisions = model.SkinningInfo.Collisions;
                if (model.SkinningInfo.CompiledBones != null) skin.CompiledBones = model.SkinningInfo.CompiledBones;
                if (model.SkinningInfo.Ext2IntMap != null) skin.Ext2IntMap = model.SkinningInfo.Ext2IntMap;
            }
            return skin;
        }

        /// <summary>
        /// There will be one Model for each model in this object.  
        /// </summary>
        public List<Model> Models { get; internal set; }
        public Material[] Materials { get; internal set; }
        public ChunkNode RootNode { get; internal set; }
        public ChunkCompiledBones Bones { get; internal set; }
        public SkinningInfo SkinningInfo { get; set; }
        public string InputFile { get; internal set; }
        public string Name => Path.GetFileName(InputFile);

        Chunk[] _chunks;
        public Chunk[] Chunks
        {
            get
            {
                if (_chunks == null) _chunks = Models.SelectMany(m => m.ChunkMap.Values).ToArray();
                return _chunks;
            }
        }

        // Cannot use the Node name for the key.  Across a couple files, you may have multiple nodes with same name.
        public Dictionary<string, ChunkNode> _nodeMap;
        public Dictionary<string, ChunkNode> NodeMap
        {
            get
            {
                if (_nodeMap == null)
                {
                    _nodeMap = new Dictionary<string, ChunkNode>(StringComparer.InvariantCultureIgnoreCase) { };
                    ChunkNode rootNode = null;
                    //Log("Mapping Nodes");
                    foreach (var model in Models)
                    {
                        model.RootNode = rootNode ??= model.RootNode; // Each model will have it's own rootnode.
                        foreach (var node in model.ChunkMap.Values.Where(c => c.ChunkType == ChunkType.Node).Select(c => c as ChunkNode))
                        {
                            // Preserve existing parents
                            if (_nodeMap.ContainsKey(node.Name))
                            {
                                var parentNode = _nodeMap[node.Name].ParentNode;
                                if (parentNode != null) parentNode = _nodeMap[parentNode.Name];
                                node.ParentNode = parentNode;
                            }
                            _nodeMap[node.Name] = node; // TODO:  fix this.  The node name can conflict.
                        }
                    }
                }
                return _nodeMap;
            }
        }

        /// <summary>
        /// Flatten all child materials into a one dimensional list
        /// </summary>
        /// <param name="material"></param>
        /// <returns></returns>
        public static IEnumerable<Material> FlattenMaterials(Material material)
        {
            if (material != null)
            {
                yield return material;
                if (material.SubMaterials != null)
                    foreach (var subMaterial in material.SubMaterials.SelectMany(m => FlattenMaterials(m)))
                        yield return subMaterial;
            }
        }

        public IEnumerable<string> GetTexturePaths()
        {
            foreach (var texture in Materials.SelectMany(x => x.Textures))
                if (!string.IsNullOrEmpty(texture.File))
                    yield return $@"Data\{texture.File}";
        }
    }
}