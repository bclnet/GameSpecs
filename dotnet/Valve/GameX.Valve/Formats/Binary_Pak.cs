using GameX.Meta;
using GameX.Formats;
using GameX.Valve.Formats.Blocks;
using OpenStack.Graphics;
using OpenStack.Graphics.Renderer1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GameX.Valve.Formats.Extras;
using static GameX.Valve.Formats.Blocks.DATA;
using static GameX.Valve.Formats.Blocks.DATATexture;

namespace GameX.Valve.Formats
{
    //was:Resource/Resource
    public class Binary_Pak : IDisposable, IHaveMetaInfo, IRedirected<ITexture>, IRedirected<IMaterial>, IRedirected<IMesh>, IRedirected<IModel>, IRedirected<IParticleSystem>
    {
        internal const ushort KnownHeaderVersion = 12;

        public Binary_Pak() { }
        public Binary_Pak(BinaryReader r) => Read(r);

        public void Dispose()
        {
            Reader?.Dispose();
            Reader = null;
            GC.SuppressFinalize(this);
        }

        ITexture IRedirected<ITexture>.Value => DATA as ITexture;
        IMaterial IRedirected<IMaterial>.Value => DATA as IMaterial;
        IMesh IRedirected<IMesh>.Value => DataType == ResourceType.Mesh ? new DATAMesh(this) as IMesh : null;
        IModel IRedirected<IModel>.Value => DATA as IModel;
        IParticleSystem IRedirected<IParticleSystem>.Value => DATA as IParticleSystem;

        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo("BinaryPak", items: new List<MetaInfo> {
                    new MetaInfo($"FileSize: {FileSize}"),
                    new MetaInfo($"Version: {Version}"),
                    new MetaInfo($"Blocks: {Blocks.Count}"),
                    new MetaInfo($"DataType: {DataType}"),
                })
            };
            switch (DataType)
            {
                case ResourceType.Texture:
                    {
                        var data = (DATATexture)DATA;
                        try
                        {
                            nodes.AddRange(new List<MetaInfo> {
                                //new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "PICTURE" }), //(tex.GenerateBitmap().ToBitmap(), tex.Width, tex.Height)
                                new MetaInfo(null, new MetaContent { Type = "Texture", Name = "Texture", Value = this, Dispose = this }),
                                new MetaInfo("Texture", items: new List<MetaInfo> {
                                    new MetaInfo($"Width: {data.Width}"),
                                    new MetaInfo($"Height: {data.Height}"),
                                    new MetaInfo($"NumMipMaps: {data.NumMipMaps}"),
                                })
                            });
                        }
                        catch (Exception e)
                        {
                            nodes.Add(new MetaInfo(null, new MetaContent { Type = "Text", Name = "Exception", Value = e.Message }));
                        }
                    }
                    break;
                case ResourceType.Panorama:
                    {
                        var data = (DATAPanorama)DATA;
                        nodes.AddRange(new List<MetaInfo> {
                            new MetaInfo(null, new MetaContent { Type = "DataGrid", Name = "Panorama Names", Value = data.Names }),
                            new MetaInfo("Panorama", items: new List<MetaInfo> {
                                new MetaInfo($"Names: {data.Names.Count}"),
                            })
                        });
                    }
                    break;
                case ResourceType.PanoramaLayout: break;
                case ResourceType.PanoramaScript: break;
                case ResourceType.PanoramaStyle: break;
                case ResourceType.Particle: nodes.Add(new MetaInfo(null, new MetaContent { Type = "Particle", Name = "Particle", Value = this, Dispose = this })); break;
                case ResourceType.Sound:
                    {
                        var sound = (DATASound)DATA;
                        var stream = sound.GetSoundStream();
                        nodes.Add(new MetaInfo(null, new MetaContent { Type = "AudioPlayer", Name = "Sound", Value = stream, Tag = $".{sound.SoundType}", Dispose = this }));
                    }
                    break;
                case ResourceType.World: nodes.Add(new MetaInfo(null, new MetaContent { Type = "World", Name = "World", Value = (DATAWorld)DATA, Dispose = this })); break;
                case ResourceType.WorldNode: nodes.Add(new MetaInfo(null, new MetaContent { Type = "World", Name = "World Node", Value = (DATAWorldNode)DATA, Dispose = this })); break;
                case ResourceType.Model: nodes.Add(new MetaInfo(null, new MetaContent { Type = "Model", Name = "Model", Value = this, Dispose = this })); break;
                case ResourceType.Mesh: nodes.Add(new MetaInfo(null, new MetaContent { Type = "Model", Name = "Mesh", Value = this, Dispose = this })); break;
                case ResourceType.Material: nodes.Add(new MetaInfo(null, new MetaContent { Type = "Material", Name = "Material", Value = this, Dispose = this })); break;
            }
            foreach (var block in Blocks)
            {
                if (block is RERL repl) { nodes.Add(new MetaInfo(null, new MetaContent { Type = "DataGrid", Name = "External Refs", Value = repl.RERLInfos })); continue; }
                else if (block is NTRO ntro)
                {
                    if (ntro.ReferencedStructs.Count > 0) nodes.Add(new MetaInfo(null, new MetaContent { Type = "DataGrid", Name = "Introspection Manifest: Structs", Value = ntro.ReferencedStructs }));
                    if (ntro.ReferencedEnums.Count > 0) nodes.Add(new MetaInfo(null, new MetaContent { Type = "DataGrid", Name = "Introspection Manifest: Enums", Value = ntro.ReferencedEnums }));
                }
                var tab = new MetaContent { Type = "Text", Name = block.GetType().Name };
                nodes.Add(new MetaInfo(null, tab));
                if (block is DATA)
                    switch (DataType)
                    {
                        case ResourceType.Sound: tab.Value = ((DATASound)block).ToString(); break;
                        case ResourceType.Particle:
                        case ResourceType.Mesh:
                            if (block is DATABinaryKV3 kv3) tab.Value = kv3.ToString();
                            else if (block is NTRO blockNTRO) tab.Value = blockNTRO.ToString();
                            break;
                        default: tab.Value = block.ToString(); break;
                    }
                else tab.Value = block.ToString();
            }
            if (!nodes.Any(x => (x.Tag as MetaContent)?.Dispose != null)) Dispose();
            return nodes;
        }

        public BinaryReader Reader { get; private set; }

        public uint FileSize { get; private set; }

        public ushort Version { get; private set; }

        public RERL RERL => GetBlockByType<RERL>();
        public REDI REDI => GetBlockByType<REDI>();
        public NTRO NTRO => GetBlockByType<NTRO>();
        public VBIB VBIB => GetBlockByType<VBIB>();
        public DATA DATA => GetBlockByType<DATA>();

        public T GetBlockByIndex<T>(int index) where T : Block => Blocks[index] as T;

        public T GetBlockByType<T>() where T : Block => (T)Blocks.Find(b => typeof(T).IsAssignableFrom(b.GetType()));

        public bool ContainsBlockType<T>() where T : Block => Blocks.Exists(b => typeof(T).IsAssignableFrom(b.GetType()));

        public bool TryGetBlockType<T>(out T value) where T : Block => (value = (T)Blocks.Find(b => typeof(T).IsAssignableFrom(b.GetType()))) != null;

        public readonly List<Block> Blocks = new List<Block>();

        public ResourceType DataType;

        /// <summary>
        /// Resource files have a FileSize in the metadata, however certain file types such as sounds have streaming audio data come
        /// after the resource file, and the size is specified within the DATA block. This property attemps to return the correct size.
        /// </summary>
        public uint FullFileSize
        {
            get
            {
                var size = FileSize;
                if (DataType == ResourceType.Sound)
                {
                    var data = (DATASound)DATA;
                    size += data.StreamingDataSize;
                }
                else if (DataType == ResourceType.Texture)
                {
                    var data = (DATATexture)DATA;
                    size += (uint)data.CalculateTextureDataSize();
                }
                return size;
            }
        }

        public void Read(BinaryReader r, bool verifyFileSize = false) //:true
        {
            Reader = r;
            FileSize = r.ReadUInt32();
            if (FileSize == 0x55AA1234) throw new FormatException("VPK file");
            else if (FileSize == CompiledShader.MAGIC) throw new FormatException("Shader file");
            else if (FileSize != r.BaseStream.Length) { }
            var headerVersion = r.ReadUInt16();
            if (headerVersion != KnownHeaderVersion) throw new FormatException($"Bad Magic: {headerVersion}, expected {KnownHeaderVersion}");
            //if (FileName != null) DataType = DetermineResourceTypeByFileExtension();
            Version = r.ReadUInt16();
            var blockOffset = r.ReadUInt32();
            var blockCount = r.ReadUInt32();
            r.Skip(blockOffset - 8); // 8 is uint32 x2 we just read
            for (var i = 0; i < blockCount; i++)
            {
                var blockType = Encoding.UTF8.GetString(r.ReadBytes(4));
                var position = r.BaseStream.Position;
                var offset = (uint)position + r.ReadUInt32();
                var size = r.ReadUInt32();
                var block = size >= 4 && blockType == "DATA" && !DATA.IsHandledType(DataType) ? r.Peek(z =>
                    {
                        var magic = z.ReadUInt32();
                        return magic == DATABinaryKV3.MAGIC || magic == DATABinaryKV3.MAGIC2 || magic == DATABinaryKV3.MAGIC3
                            ? (Block)new DATABinaryKV3()
                            : magic == DATABinaryKV1.MAGIC ? (Block)new DATABinaryKV1() : null;
                    }) : null;
                block ??= Factory(this, blockType);
                block.Offset = offset;
                block.Size = size;
                if (blockType == "REDI" || blockType == "RED2" || blockType == "NTRO") block.Read(this, r);
                Blocks.Add(block);
                switch (block)
                {
                    case REDI redi:
                        // Try to determine resource type by looking at first compiler indentifier
                        if (DataType == ResourceType.Unknown && REDI.Structs.TryGetValue(REDI.REDIStruct.SpecialDependencies, out var specialBlock))
                        {
                            var specialDeps = (REDISpecialDependencies)specialBlock;
                            if (specialDeps.List.Count > 0) DataType = DetermineTypeByCompilerIdentifier(specialDeps.List[0]);
                        }
                        // Try to determine resource type by looking at the input dependency if there is only one
                        if (DataType == ResourceType.Unknown && REDI.Structs.TryGetValue(REDI.REDIStruct.InputDependencies, out var inputBlock))
                        {
                            var inputDeps = (REDIInputDependencies)inputBlock;
                            if (inputDeps.List.Count == 1) DataType = DetermineResourceTypeByFileExtension(Path.GetExtension(inputDeps.List[0].ContentRelativeFilename));
                        }
                        break;
                    case NTRO ntro:
                        if (DataType == ResourceType.Unknown && ntro.ReferencedStructs.Count > 0)
                            switch (ntro.ReferencedStructs[0].Name)
                            {
                                case "VSoundEventScript_t": DataType = ResourceType.SoundEventScript; break;
                                case "CWorldVisibility": DataType = ResourceType.WorldVisibility; break;
                            }
                        break;
                }
                r.BaseStream.Position = position + 8;
            }
            foreach (var block in Blocks) if (!(block is REDI) && !(block is RED2) && !(block is NTRO)) block.Read(this, r);

            var fullFileSize = FullFileSize;
            if (verifyFileSize && Reader.BaseStream.Length != fullFileSize)
            {
                if (DataType == ResourceType.Texture)
                {
                    var data = (DATATexture)DATA;
                    // TODO: We do not currently have a way of calculating buffer size for these types, Texture.GenerateBitmap also just reads until end of the buffer
                    if (data.Format == VTexFormat.JPEG_DXT5 || data.Format == VTexFormat.JPEG_RGBA8888) return;
                    // TODO: Valve added null bytes after the png for whatever reason, so assume we have the full file if the buffer is bigger than the size we calculated
                    if (data.Format == VTexFormat.PNG_DXT5 || data.Format == VTexFormat.PNG_RGBA8888 && Reader.BaseStream.Length > fullFileSize) return;
                }
                throw new InvalidDataException($"File size ({Reader.BaseStream.Length}) does not match size specified in file ({fullFileSize}) ({DataType}).");
            }
        }
    }
}