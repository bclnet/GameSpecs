using System;
using System.Collections.Generic;
using System.IO;

namespace GameX.Valve.Formats.Blocks
{
    /// <summary>
    /// "DATA" block.
    /// </summary>
    //was:Resource/Blocks/ResourceData
    public class DATA : Block
    {
        //: Enums.ResourceType
        public enum ResourceType //was:Resource/Enums/ResourceType
        {
            Unknown = 0,
            [ExtensionX("vanim")] Animation,
            [ExtensionX("vagrp")] AnimationGroup,
            [ExtensionX("vanmgrph")] AnimationGraph,
            [ExtensionX("valst")] ActionList,
            [ExtensionX("vseq")] Sequence,
            [ExtensionX("vpcf")] Particle,
            [ExtensionX("vmat")] Material,
            [ExtensionX("vmks")] Sheet,
            [ExtensionX("vmesh")] Mesh,
            [ExtensionX("vtex")] Texture,
            [ExtensionX("vmdl")] Model,
            [ExtensionX("vphys")] PhysicsCollisionMesh,
            [ExtensionX("vsnd")] Sound,
            [ExtensionX("vmorf")] Morph,
            [ExtensionX("vrman")] ResourceManifest,
            [ExtensionX("vwrld")] World,
            [ExtensionX("vwnod")] WorldNode,
            [ExtensionX("vvis")] WorldVisibility,
            [ExtensionX("vents")] EntityLump,
            [ExtensionX("vsurf")] SurfaceProperties,
            [ExtensionX("vsndevts")] SoundEventScript,
            [ExtensionX("vmix")] VMix,
            [ExtensionX("vsndstck")] SoundStackScript,
            [ExtensionX("vfont")] BitmapFont,
            [ExtensionX("vrmap")] ResourceRemapTable,
            [ExtensionX("vcdlist")] ChoreoSceneFileData,
            // All Panorama* are compiled just as CompilePanorama
            [ExtensionX("vtxt")] Panorama, // vtxt is not a real extension
            [ExtensionX("vcss")] PanoramaStyle,
            [ExtensionX("vxml")] PanoramaLayout,
            [ExtensionX("vpdi")] PanoramaDynamicImages,
            [ExtensionX("vjs")] PanoramaScript,
            [ExtensionX("vts")] PanoramaTypescript,
            [ExtensionX("vsvg")] PanoramaVectorGraphic,
            [ExtensionX("vpsf")] ParticleSnapshot,
            [ExtensionX("vmap")] Map,
            [ExtensionX("vpost")] PostProcessing,
            [ExtensionX("vdata")] VData,
            [ExtensionX("item")] ArtifactItem,
            [ExtensionX("sbox")] SboxManagedResource, // TODO: Managed resources can have any extension
        }

        public IDictionary<string, object> AsKeyValue()
        {
            if (this is DATABinaryNTRO ntro) return ntro.Data;
            else if (this is DATABinaryKV3 kv3) return kv3.Data;
            return default;
        }

        public override void Read(Binary_Pak parent, BinaryReader r) { }

        //: was:Resource.ConstructFromType()
        public static Block Factory(Binary_Pak source, string value)
            => value switch
            {
                "DATA" => Factory(source),
                "REDI" => new REDI(),
                "RED2" => new RED2(),
                "RERL" => new RERL(),
                "NTRO" => new NTRO(),
                "VBIB" => new VBIB(),
                "VXVS" => new VXVS(),
                "SNAP" => new SNAP(),
                "MBUF" => new MBUF(),
                "CTRL" => new CTRL(),
                "MDAT" => new MDAT(),
                "INSG" => new INSG(),
                "SrMa" => new SRMA(),
                "LaCo" => new LACO(),
                "MRPH" => new MRPH(),
                "ANIM" => new ANIM(),
                "ASEQ" => new ASEQ(),
                "AGRP" => new AGRP(),
                "PHYS" => new PHYS(),
                _ => throw new ArgumentOutOfRangeException(nameof(value), $"Unrecognized block type '{value}'"),
            };

        //: Resource.ConstructResourceType()
        internal static DATA Factory(Binary_Pak source) => source.DataType switch
        {
            var x when x == ResourceType.Panorama || x == ResourceType.PanoramaScript || x == ResourceType.PanoramaTypescript || x == ResourceType.PanoramaDynamicImages || x == ResourceType.PanoramaVectorGraphic => new DATAPanorama(),
            ResourceType.PanoramaStyle => new DATAPanoramaStyle(),
            ResourceType.PanoramaLayout => new DATAPanoramaLayout(),
            ResourceType.Sound => new DATASound(),
            ResourceType.Texture => new DATATexture(),
            ResourceType.Model => new DATAModel(),
            ResourceType.World => new DATAWorld(),
            ResourceType.WorldNode => new DATAWorldNode(),
            ResourceType.EntityLump => new DATAEntityLump(),
            ResourceType.Material => new DATAMaterial(),
            ResourceType.SoundEventScript => new DATASoundEventScript(),
            ResourceType.SoundStackScript => new DATASoundStackScript(),
            ResourceType.Particle => new DATAParticleSystem(),
            ResourceType.PostProcessing => new DATAPostProcessing(),
            ResourceType.ResourceManifest => new DATAResourceManifest(),
            var x when x == ResourceType.SboxManagedResource || x == ResourceType.ArtifactItem => new DATAPlaintext(),
            ResourceType.PhysicsCollisionMesh => new DATAPhysAggregateData(),
            ResourceType.Mesh => new DATAMesh(source),
            //ResourceType.Mesh => source.Version != 0 ? new DATABinaryKV3() : source.ContainsBlockType<NTRO>() ? new DATABinaryNTRO() : new DATA(),
            _ => source.ContainsBlockType<NTRO>() ? new DATABinaryNTRO() : new DATA(),
        };

        internal static ResourceType DetermineResourceTypeByFileExtension(string fileName, string extension = null)
        {
            extension ??= Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(extension)) return ResourceType.Unknown;
            extension = extension.EndsWith("_c", StringComparison.Ordinal) ? extension[1..^2] : extension[1..];
            foreach (ResourceType typeValue in Enum.GetValues(typeof(ResourceType)))
            {
                if (typeValue == ResourceType.Unknown) continue;
                var type = typeof(ResourceType).GetMember(typeValue.ToString())[0];
                var typeExt = (ExtensionXAttribute)type.GetCustomAttributes(typeof(ExtensionXAttribute), false)[0];
                if (typeExt.Extension == extension) return typeValue;
            }
            return ResourceType.Unknown;
        }

        internal static bool IsHandledType(ResourceType type) =>
            type == ResourceType.Model ||
            type == ResourceType.World ||
            type == ResourceType.WorldNode ||
            type == ResourceType.Particle ||
            type == ResourceType.Material ||
            type == ResourceType.EntityLump ||
            type == ResourceType.PhysicsCollisionMesh ||
            type == ResourceType.Morph ||
            type == ResourceType.PostProcessing;

        internal static ResourceType DetermineTypeByCompilerIdentifier(REDISpecialDependencies.SpecialDependency value)
        {
            var identifier = value.CompilerIdentifier;
            if (identifier.StartsWith("Compile", StringComparison.Ordinal)) identifier = identifier.Remove(0, "Compile".Length);
            return identifier switch
            {
                "Psf" => ResourceType.ParticleSnapshot,
                "AnimGroup" => ResourceType.AnimationGroup,
                "Animgraph" => ResourceType.AnimationGraph,
                "VPhysXData" => ResourceType.PhysicsCollisionMesh,
                "Font" => ResourceType.BitmapFont,
                "RenderMesh" => ResourceType.Mesh,
                "ChoreoSceneFileData" => ResourceType.ChoreoSceneFileData,
                "Panorama" => value.String switch
                {
                    "Panorama Style Compiler Version" => ResourceType.PanoramaStyle,
                    "Panorama Script Compiler Version" => ResourceType.PanoramaScript,
                    "Panorama Layout Compiler Version" => ResourceType.PanoramaLayout,
                    "Panorama Dynamic Images Compiler Version" => ResourceType.PanoramaDynamicImages,
                    _ => ResourceType.Panorama,
                },
                "VectorGraphic" => ResourceType.PanoramaVectorGraphic,
                "VData" => ResourceType.VData,
                "DotaItem" => ResourceType.ArtifactItem,
                var x when x == "SBData" || x == "ManagedResourceCompiler" => ResourceType.SboxManagedResource, // This is without the "Compile" prefix
                _ => Enum.TryParse(identifier, false, out ResourceType resourceType) ? resourceType : ResourceType.Unknown,
            };
        }
    }
}
