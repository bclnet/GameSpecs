using System;
using System.IO;

namespace GameSpec.Valve.Formats.Blocks
{
    /// <summary>
    /// "DATA" block.
    /// </summary>
    public class DATA : Block
    {
        public enum DataType
        {
#pragma warning disable 1591
            Unknown = 0,
            [Extension("vanim")] Animation,
            [Extension("vagrp")] AnimationGroup,
            [Extension("valst")] ActionList,
            [Extension("vseq")] Sequence,
            [Extension("vpcf")] Particle,
            [Extension("vmat")] Material,
            [Extension("vmks")] Sheet,
            [Extension("vmesh")] Mesh,
            [Extension("vtex")] Texture,
            [Extension("vmdl")] Model,
            [Extension("vphys")] PhysicsCollisionMesh,
            [Extension("vsnd")] Sound,
            [Extension("vmorf")] Morph,
            [Extension("vrman")] ResourceManifest,
            [Extension("vwrld")] World,
            [Extension("vwnod")] WorldNode,
            [Extension("vvis")] WorldVisibility,
            [Extension("vents")] EntityLump,
            [Extension("vsurf")] SurfaceProperties,
            [Extension("vsndevts")] SoundEventScript,
            [Extension("vsndstck")] SoundStackScript,
            [Extension("vfont")] BitmapFont,
            [Extension("vrmap")] ResourceRemapTable,
            // All Panorama* are compiled just as CompilePanorama
            [Extension("vtxt")] Panorama, // vtxt is not a real extension
            [Extension("vcss")] PanoramaStyle,
            [Extension("vxml")] PanoramaLayout,
            [Extension("vpdi")] PanoramaDynamicImages,
            [Extension("vjs")] PanoramaScript,
            [Extension("vsvg")] PanoramaVectorGraphic,
            [Extension("vpsf")] ParticleSnapshot,
            [Extension("vmap")] Map,
#pragma warning restore 1591
        }

        public override void Read(BinaryPak parent, BinaryReader r) { }

        internal static bool IsHandledType(DataType type) =>
            type == DataType.Model ||
            type == DataType.World ||
            type == DataType.WorldNode ||
            type == DataType.Particle ||
            type == DataType.Material ||
            type == DataType.EntityLump;

        internal static DataType DetermineTypeByCompilerIdentifier(REDISpecialDependencies.SpecialDependency value)
        {
            var identifier = value.CompilerIdentifier;
            if (identifier.StartsWith("Compile", StringComparison.Ordinal)) identifier = identifier.Remove(0, "Compile".Length);
            return identifier switch
            {
                "Psf" => DataType.ParticleSnapshot,
                "AnimGroup" => DataType.AnimationGroup,
                "VPhysXData" => DataType.PhysicsCollisionMesh,
                "Font" => DataType.BitmapFont,
                "RenderMesh" => DataType.Mesh,
                "Panorama" => value.String switch
                {
                    "Panorama Style Compiler Version" => DataType.PanoramaStyle,
                    "Panorama Script Compiler Version" => DataType.PanoramaScript,
                    "Panorama Layout Compiler Version" => DataType.PanoramaLayout,
                    "Panorama Dynamic Images Compiler Version" => DataType.PanoramaDynamicImages,
                    _ => DataType.Panorama,
                },
                "VectorGraphic" => DataType.PanoramaVectorGraphic,
                _ => Enum.TryParse(identifier, false, out DataType type) ? type : DataType.Unknown,
            };
        }

        internal static DATA Factory(BinaryPak source)
        {
            switch (source.DataType)
            {
                case DataType.Panorama:
                case DataType.PanoramaStyle:
                case DataType.PanoramaScript:
                case DataType.PanoramaLayout:
                case DataType.PanoramaDynamicImages:
                case DataType.PanoramaVectorGraphic: return new DATAPanorama();
                case DataType.Sound: return new DATASound();
                case DataType.Texture: return new DATATexture();
                case DataType.Model: return new DATAModel();
                case DataType.World: return new DATAWorld();
                case DataType.WorldNode: return new DATAWorldNode();
                case DataType.EntityLump: return new DATAEntityLump();
                case DataType.Material: return new DATAMaterial();
                case DataType.SoundEventScript: return new DATASoundEventScript();
                case DataType.SoundStackScript: return new DATASoundStackScript();
                case DataType.Particle: return new DATAParticleSystem();
                case DataType.Mesh: return source.Version != 0 ? new DATABinaryKV3() : source.ContainsBlockType<NTRO>() ? new DATABinaryNTRO() : new DATA();
                default: return source.ContainsBlockType<NTRO>() ? new DATABinaryNTRO() : new DATA();
            }
        }
    }
}
