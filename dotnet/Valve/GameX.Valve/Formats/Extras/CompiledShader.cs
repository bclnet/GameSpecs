using GameX.Meta;
using GameX.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Decoder = SevenZip.Compression.LZMA.Decoder;

namespace GameX.Valve.Formats.Extras
{
    public class CompiledShader : IHaveMetaInfo
    {
        public const int MAGIC = 0x32736376; // "vcs2"

        string ShaderType;
        string ShaderPlatform;
        string Shader;

        public CompiledShader() { }
        public CompiledShader(BinaryReader r, string filename) => Read(r, filename);

        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => new List<MetaInfo> {
            new MetaInfo(null, new MetaContent { Type = "Text", Name = "Shader", Value = Shader }),
            new MetaInfo("CompiledShader", items: new List<MetaInfo> {
                new MetaInfo($"ShaderType: {ShaderType}"),
                new MetaInfo($"ShaderPlatform: {ShaderPlatform}"),
            }),
        };

        public void Read(BinaryReader r, string filename)
        {
            var b = new StringBuilder();
            if (filename.EndsWith("vs.vcs")) ShaderType = "vertex";
            else if (filename.EndsWith("ps.vcs")) ShaderType = "pixel";
            else if (filename.EndsWith("features.vcs")) ShaderType = "features";
            if (filename.Contains("vulkan")) ShaderPlatform = "vulkan";
            else if (filename.Contains("pcgl")) ShaderPlatform = "opengl";
            else if (filename.Contains("pc_")) ShaderPlatform = "directx";
            if (r.ReadUInt32() != MAGIC) throw new FormatException("Given file is not a vcs2.");

            // Known versions:
            //  62 - April 2016
            //  63 - March 2017
            //  64 - May 2017
            var version = r.ReadUInt32();
            if (version != 64) throw new FormatException($"Unsupported VCS2 version: {version}");
            if (ShaderType == "features") ReadFeatures(r, b);
            else ReadShader(r, b);
            Shader = b.ToString();
        }

        void ReadFeatures(BinaryReader r, StringBuilder b_)
        {
            var anotherFileRef = r.ReadInt32(); // new in version 64, mostly 0 but sometimes 1

            var wtf = r.ReadUInt32(); // appears to be 0 in 'features'
            b_.AppendLine($"wtf: {wtf}");

            var name = Encoding.UTF8.GetString(r.ReadBytes(r.ReadInt32()));
            r.ReadByte(); // null term?

            b_.AppendLine($"Name: {name} - Offset: {r.BaseStream.Position}");

            var a = r.ReadInt32();
            var b = r.ReadInt32();
            var c = r.ReadInt32();
            var d = r.ReadInt32();
            var e = r.ReadInt32();
            var f = r.ReadInt32();
            var g = r.ReadInt32();
            var h = r.ReadInt32();
            if (anotherFileRef == 1) { var i = r.ReadInt32(); b_.AppendLine($"{a} {b} {c} {d} {e} {f} {g} {h} {i}"); }
            else b_.AppendLine($"{a} {b} {c} {d} {e} {f} {g} {h}");
            var count = r.ReadUInt32();
            long prevPos;
            b_.AppendLine($"Count: {count}");
            for (var i = 0; i < count; i++)
            {
                prevPos = r.BaseStream.Position;

                name = r.ReadZUTF8();
                r.Seek(prevPos + 128);

                var type = r.ReadUInt32();
                b_.AppendLine($"Name: {name} - Type: {type} - Offset: {r.BaseStream.Position}");

                if (type == 1)
                {
                    prevPos = r.BaseStream.Position;
                    var subname = r.ReadZUTF8();
                    b_.AppendLine(subname);
                    r.BaseStream.Position = prevPos + 64;
                    r.ReadUInt32();
                }
            }

            var identifierCount = 8;
            if (anotherFileRef == 1) identifierCount++;

            // Appears to be always 128 bytes in version 63 and higher, 112 before
            for (var i = 0; i < identifierCount; i++)
            {
                // either 6 or 7 is cs (compute shader)
                // 0 - ?
                // 1 - vertex shader
                // 2 - pixel shader
                // 3 - geometry shader
                // 4 - hull shader
                // 5 - domain shader
                // 6 - ?
                // 7 - ?, new in version 63
                // 8 - pixel shader render state (only if uint in version 64+ at pos 8 is 1)
                var identifier = r.ReadBytes(16);
                b_.AppendLine($"#{i} identifier: {BitConverter.ToString(identifier)}");
            }

            r.ReadUInt32(); // 0E 00 00 00

            count = r.ReadUInt32();
            for (var i = 0; i < count; i++)
            {
                prevPos = r.BaseStream.Position;
                name = r.ReadZUTF8();
                r.BaseStream.Position = prevPos + 64;
                prevPos = r.BaseStream.Position;
                var desc = r.ReadZUTF8();
                r.BaseStream.Position = prevPos + 84;
                var subcount = r.ReadUInt32();
                b_.AppendLine($"Name: {name} - Desc: {desc} - Count: {subcount} - Offset: {r.BaseStream.Position}");
                for (var j = 0; j < subcount; j++) b_.AppendLine($"     {r.ReadZUTF8()}");
            }

            count = r.ReadUInt32();
            b_.AppendLine($"Count: {count}");
        }

        void ReadShader(BinaryReader r, StringBuilder b_)
        {
            // This uint controls whether or not there's an additional uint and file identifier in header for features shader, might be something different in these.
            var unk0_a = r.ReadInt32(); // new in version 64, mostly 0 but sometimes 1

            var fileIdentifier = r.ReadBytes(16);
            var staticIdentifier = r.ReadBytes(16);

            b_.AppendLine($"File identifier: {BitConverter.ToString(fileIdentifier)}");
            b_.AppendLine($"Static identifier: {BitConverter.ToString(staticIdentifier)}");

            var unk0_b = r.ReadUInt32();
            b_.AppendLine($"wtf {unk0_b}"); // Always 14?

            // Chunk 1
            var count = r.ReadUInt32();
            b_.AppendLine($"[CHUNK 1] Count: {count} - Offset: {r.BaseStream.Position}");

            for (var i = 0; i < count; i++)
            {
                var previousPosition = r.BaseStream.Position;
                var name = r.ReadZUTF8();
                r.BaseStream.Position = previousPosition + 128;

                var unk1_a = r.ReadInt32();
                var unk1_b = r.ReadInt32();
                var unk1_c = r.ReadInt32();
                var unk1_d = r.ReadInt32();
                var unk1_e = r.ReadInt32();
                var unk1_f = r.ReadInt32();
                b_.AppendLine($"{unk1_a} {unk1_b} {unk1_c} {unk1_d} {unk1_e} {unk1_f} {name}");
            }

            // Chunk 2 - Similar structure to chunk 4, same chunk size
            count = r.ReadUInt32();
            b_.AppendLine($"[CHUNK 2] Count: {count} - Offset: {r.BaseStream.Position}");

            for (var i = 0; i < count; i++)
            {
                // Initial research based on brushsplat_pc_40_ps, might be different for other shaders
                var unk2_a = r.ReadUInt32(); // always 3?
                var unk2_b = r.ReadUInt32(); // always 2?
                var unk2_c = r.ReadUInt16(); // always 514?
                var unk2_d = r.ReadUInt16(); // always 514?
                var unk2_e = r.ReadUInt32();
                var unk2_f = r.ReadUInt32();
                var unk2_g = r.ReadUInt32();
                var unk2_h = r.ReadUInt32();
                var unk2_i = r.ReadUInt32();
                var unk2_j = r.ReadUInt32();
                var unk2_k = r.ReadUInt32();
                r.ReadBytes(176); // Chunk of mostly FF
                r.ReadBytes(256); // Chunk of 0s. padding?
                b_.AppendLine($"{unk2_a} {unk2_b} {unk2_c} {unk2_d} {unk2_e} {unk2_f} {unk2_g} {unk2_h} {unk2_i} {unk2_j} {unk2_k}");
            }

            // 3
            count = r.ReadUInt32();
            b_.AppendLine($"[CHUNK 3] Count: {count} - Offset: {r.BaseStream.Position}");

            for (var i = 0; i < count; i++)
            {
                var previousPosition = r.BaseStream.Position;
                var name = r.ReadZUTF8();
                r.BaseStream.Position = previousPosition + 128;

                var unk3_a = r.ReadInt32();
                var unk3_b = r.ReadInt32();
                var unk3_c = r.ReadInt32();
                var unk3_d = r.ReadInt32();
                var unk3_e = r.ReadInt32();
                var unk3_f = r.ReadInt32();
                b_.AppendLine($"{unk3_a} {unk3_b} {unk3_c} {unk3_d} {unk3_e} {unk3_f} {name}");
            }

            // 4 - Similar structure to chunk 2, same chunk size
            count = r.ReadUInt32();
            b_.AppendLine($"[CHUNK 4] Count: {count} - Offset: {r.BaseStream.Position}");

            for (var i = 0; i < count; i++)
            {
                var unk4_a = r.ReadUInt32();
                var unk4_b = r.ReadUInt32();
                var unk4_c = r.ReadUInt16();
                var unk4_d = r.ReadUInt16();
                var unk4_e = r.ReadUInt32();
                var unk4_f = r.ReadUInt32();
                var unk4_g = r.ReadUInt32();
                var unk4_h = r.ReadUInt32();
                var unk4_i = r.ReadUInt32();

                r.ReadBytes(184); // Chunk of mostly FF
                r.ReadBytes(256); // Chunk of 0s. padding?
                b_.AppendLine($"{unk4_a} {unk4_b} {unk4_c} {unk4_d} {unk4_e} {unk4_f} {unk4_g} {unk4_h} {unk4_i}");
            }

            // 5 - Globals?
            count = r.ReadUInt32();
            b_.AppendLine($"[CHUNK 5] Count: {count} - Offset: {r.BaseStream.Position}");

            for (var i = 0; i < count; i++)
            {
                var previousPosition = r.BaseStream.Position;
                var name = r.ReadZUTF8();
                r.BaseStream.Position = previousPosition + 128; // ??

                var hasDesc = r.ReadInt32();
                var unk5_a = r.ReadInt32();

                var desc = string.Empty;

                if (hasDesc > 0)
                    desc = r.ReadZUTF8();

                r.BaseStream.Position = previousPosition + 200;
                var type = r.ReadInt32();
                var length = r.ReadInt32();
                r.BaseStream.Position = previousPosition + 480;

                // Don't know what content of this chunk is yet, but size seems to depend on type.
                // If we read the amount of bytes below per type the rest of the file will process as usual (and get to the LZMA stuff).
                // CHUNK SIZES:
                //  Type 0: 480
                //  Type 1: 480 + LENGTH + 4!
                //  Type 2: 480 (brushsplat_pc_40_ps.vcs)
                //  Type 5: 480 + LENGTH + 4! (debugoverlay_wireframe_pc_40_vs.vcs)
                //  Type 6: 480 + LENGTH + 4! (depth_only_pc_30_ps.vcs)
                //  Type 7: 480 + LENGTH + 4! (grasstile_preview_pc_41_ps.vcs)
                //  Type 10: 480 (brushsplat_pc_40_ps.vcs)
                //  Type 11: 480 (post_process_pc_30_ps.vcs)
                //  Type 13: 480 (spriteentity_pc_41_vs.vcs)
                // Needs further investigation. This is where parsing a lot of shaders break right now.
                if (length > -1 && type != 0 && type != 2 && type != 10 && type != 11 && type != 13)
                {
                    if (type != 1 && type != 5 && type != 6 && type != 7) b_.AppendLine($"!!! Unknown type of type {type} encountered at position {r.BaseStream.Position - 8}. Assuming normal sized chunk.");
                    else
                    {
                        var unk5_b = r.ReadBytes(length);
                        var unk5_c = r.ReadUInt32();
                    }
                }

                var unk5_d = r.ReadUInt32();
                b_.AppendLine($"{type} {length} {name} {hasDesc} {desc}");
            }

            // 6
            count = r.ReadUInt32();
            b_.AppendLine($"[CHUNK 6] Count: {count} - Offset: {r.BaseStream.Position}");

            for (var i = 0; i < count; i++)
            {
                var unk6_a = r.ReadBytes(4); // unsure, maybe shorts or bytes
                var unk6_b = r.ReadUInt32(); // 12, 13, 14 or 15 in brushplat_pc_40_ps.vcs
                var unk6_c = r.ReadBytes(12); // FF
                var unk6_d = r.ReadUInt32();

                var previousPosition = r.BaseStream.Position;
                var name = r.ReadZUTF8();
                r.BaseStream.Position = previousPosition + 256;

                b_.AppendLine($"{unk6_b} {unk6_d} {name}");
            }

            // 7 - Input buffer layout
            count = r.ReadUInt32();
            b_.AppendLine($"[CHUNK 7] Count: {count} - Offset: {r.BaseStream.Position}");

            for (var i = 0; i < count; i++)
            {
                var prevPos = r.BaseStream.Position;
                var name = r.ReadZUTF8(8);
                r.BaseStream.Position = prevPos + 64;

                var a = r.ReadUInt32();
                var b = r.ReadUInt32();
                var subCount = r.ReadUInt32();
                b_.AppendLine($"[SUB CHUNK] Name: {name} - unk1: {a} - unk2: {b} - Count: {subCount} - Offset: {r.BaseStream.Position}");

                for (var j = 0; j < subCount; j++)
                {
                    var previousPosition = r.BaseStream.Position;
                    var subname = r.ReadZUTF8();
                    r.BaseStream.Position = previousPosition + 64;

                    var bufferOffset = r.ReadUInt32(); // Offset in the buffer
                    var components = r.ReadUInt32(); // Number of components in this element
                    var componentSize = r.ReadUInt32(); // Number of floats per component
                    var repetitions = r.ReadUInt32(); // Number of repetitions?
                    b_.AppendLine($"     Name: {subname} - offset: {bufferOffset} - components: {components} - compSize: {componentSize} - num: {repetitions}");
                }

                r.ReadBytes(4);
            }

            b_.AppendLine($"Offset: {r.BaseStream.Position}");

            // Vertex shader has a string chunk which seems to be vertex buffer specifications
            if (ShaderType == "vertex")
            {
                var bufferCount = r.ReadUInt32();
                b_.AppendLine($"{bufferCount} vertex buffer descriptors");
                for (var h = 0; h < bufferCount; h++)
                {
                    count = r.ReadUInt32(); // number of attributes
                    b_.AppendLine($"Buffer #{h}, {count} attributes");

                    for (var i = 0; i < count; i++)
                    {
                        var name = r.ReadZUTF8();
                        var type = r.ReadZUTF8();
                        var option = r.ReadZUTF8();
                        var unk = r.ReadUInt32(); // 0, 1, 2, 13 or 14
                        b_.AppendLine($"     Name: {name}, Type: {type}, Option: {option}, Unknown uint: {unk}");
                    }
                }
            }

            var lzmaCount = r.ReadUInt32();
            b_.AppendLine($"Offset: {r.BaseStream.Position}");

            var unkLongs = new long[lzmaCount];
            for (var i = 0; i < lzmaCount; i++) unkLongs[i] = r.ReadInt64();

            var lzmaOffsets = new int[lzmaCount];
            for (var i = 0; i < lzmaCount; i++) lzmaOffsets[i] = r.ReadInt32();

            for (var i = 0; i < lzmaCount; i++)
            {
                b_.AppendLine("Extracting shader {i}..");
                // File.WriteAllBytes(Path.Combine(@"D:\shaders\PCGL DotA Core\processed spritecard\", "shader_out_" + i + ".bin"), ReadShaderChunk(lzmaOffsets[i]));

                // Skip non-PCGL shaders for now, need to figure out platform without checking filename
                if (ShaderPlatform != "opengl") continue;

                // What follows here is super experimental and barely works as is. It is a very rough implementation to read and extract shader stringblocks for PCGL shaders.
                using var inputStream = new MemoryStream(ReadShaderChunk(r, b_, lzmaOffsets[i]));
                using var chunkReader = new BinaryReader(inputStream);
                while (chunkReader.BaseStream.Position < chunkReader.BaseStream.Length)
                {
                    // Read count that also doubles as mode?
                    var modeAndCount = chunkReader.ReadInt16();

                    // Mode never seems to be 20 for anything but the FF chunk before shader stringblock
                    if (modeAndCount != 20)
                    {
                        chunkReader.ReadInt16();
                        var unk2 = chunkReader.ReadInt32();
                        var unk3 = chunkReader.ReadInt32();

                        // If the mode isn't the same as unk3, skip shader for now
                        if (modeAndCount != unk3) { b_.AppendLine($"Having issues reading shader {i}, skipping.."); chunkReader.BaseStream.Position = chunkReader.BaseStream.Length; continue; }

                        chunkReader.ReadBytes(unk3 * 4);

                        var unk4 = chunkReader.ReadUInt16();

                        // Seems to be 1 if there's a string there, read 26 byte stringblock, roll back if not
                        if (unk4 == 1) chunkReader.ReadBytes(26);
                        else chunkReader.BaseStream.Position -= 2;
                    }
                    else if (modeAndCount == 20)
                    {
                        // Read 40 byte 0xFF chunk
                        chunkReader.ReadBytes(40);

                        // Read 5 unknown bytes
                        chunkReader.ReadBytes(5);

                        // Shader stringblock count
                        var shaderContentCount = chunkReader.ReadUInt32();

                        // Read trailing byte
                        chunkReader.ReadByte();

                        // If shader stringblock count is ridiculously high stop reading this shader and bail
                        if (shaderContentCount > 100) { b_.AppendLine($"Having issues reading shader {i}, skipping.."); chunkReader.BaseStream.Position = chunkReader.BaseStream.Length; continue; }

                        // Read and dump all shader stringblocks
                        for (var j = 0; j < shaderContentCount; j++)
                        {
                            var shaderLengthInclHeader = chunkReader.ReadInt32();
                            var unk = chunkReader.ReadUInt32(); //type?
                            b_.AppendLine(unk.ToString());
                            var shaderContentLength = chunkReader.ReadInt32();
                            var shaderContent = chunkReader.ReadChars(shaderContentLength);

                            // File.WriteAllText(Path.Combine(@"D:\shaders\PCGL DotA Core\processed spritecard", "shader_out_" + i + "_" + j + ".txt"), new string(shaderContent));
                            var shaderContentChecksum = chunkReader.ReadBytes(16);
                        }

                        // Reached end of shader content, skip remaining file length
                        chunkReader.ReadBytes((int)chunkReader.BaseStream.Length - (int)chunkReader.BaseStream.Position);
                    }
                }
            }
        }

        byte[] ReadShaderChunk(BinaryReader r, StringBuilder b_, int offset)
        {
            var prevPos = r.BaseStream.Position;
            r.BaseStream.Position = offset;
            var chunkSize = r.ReadUInt32();

            if (r.ReadUInt32() != 0x414D5A4C) throw new InvalidDataException("Not LZMA?");

            var uncompressedSize = r.ReadUInt32();
            var compressedSize = r.ReadUInt32();

            b_.AppendLine($"Chunk size: {chunkSize}");
            b_.AppendLine($"Compressed size: {compressedSize}");
            b_.AppendLine($"Uncompressed size: {uncompressedSize} ({(uncompressedSize - compressedSize) / (double)uncompressedSize:P2} compression)");

            var decoder = new Decoder();
            decoder.SetDecoderProperties(r.ReadBytes(5));

            var compressedBuffer = r.ReadBytes((int)compressedSize);

            r.BaseStream.Position = prevPos;

            using var inputStream = new MemoryStream(compressedBuffer);
            using var outStream = new MemoryStream((int)uncompressedSize);
            decoder.Code(inputStream, outStream, compressedBuffer.Length, uncompressedSize, null);
            return outStream.ToArray();
        }
    }
}
