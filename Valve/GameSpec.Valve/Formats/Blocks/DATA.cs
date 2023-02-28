using System;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.Valve.Formats.Blocks
{
    /// <summary>
    /// "DATA" block.
    /// </summary>
    public class DATA : Block
    {
        //: Enums.ResourceType
        public enum ResourceType //was:Resource/Enums/ResourceType
        {
            Unknown = 0,
            [Extension("vanim")] Animation,
            [Extension("vagrp")] AnimationGroup,
            [Extension("vanmgrph")] AnimationGraph,
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
            [Extension("vmix")] VMix,
            [Extension("vsndstck")] SoundStackScript,
            [Extension("vfont")] BitmapFont,
            [Extension("vrmap")] ResourceRemapTable,
            [Extension("vcdlist")] ChoreoSceneFileData,
            // All Panorama* are compiled just as CompilePanorama
            [Extension("vtxt")] Panorama, // vtxt is not a real extension
            [Extension("vcss")] PanoramaStyle,
            [Extension("vxml")] PanoramaLayout,
            [Extension("vpdi")] PanoramaDynamicImages,
            [Extension("vjs")] PanoramaScript,
            [Extension("vts")] PanoramaTypescript,
            [Extension("vsvg")] PanoramaVectorGraphic,
            [Extension("vpsf")] ParticleSnapshot,
            [Extension("vmap")] Map,
            [Extension("vpost")] PostProcessing,
            [Extension("vdata")] VData,
            [Extension("item")] ArtifactItem,
            [Extension("sbox")] SboxManagedResource, // TODO: Managed resources can have any extension
        }

        public IDictionary<string, object> AsKeyValue()
        {
            if (this is DATABinaryNTRO ntro) return ntro.Data;
            else if (this is DATABinaryKV3 kv3) return kv3.Data;
            return default;
        }

        public override void Read(BinaryPak parent, BinaryReader r) { }

        internal static bool IsHandledType(ResourceType type) =>
            type == ResourceType.Model ||
            type == ResourceType.World ||
            type == ResourceType.WorldNode ||
            type == ResourceType.Particle ||
            type == ResourceType.Material ||
            type == ResourceType.EntityLump;

        internal static ResourceType DetermineTypeByCompilerIdentifier(REDISpecialDependencies.SpecialDependency value)
        {
            var identifier = value.CompilerIdentifier;
            if (identifier.StartsWith("Compile", StringComparison.Ordinal)) identifier = identifier.Remove(0, "Compile".Length);
            return identifier switch
            {
                "Psf" => ResourceType.ParticleSnapshot,
                "AnimGroup" => ResourceType.AnimationGroup,
                "VPhysXData" => ResourceType.PhysicsCollisionMesh,
                "Font" => ResourceType.BitmapFont,
                "RenderMesh" => ResourceType.Mesh,
                "Panorama" => value.String switch
                {
                    "Panorama Style Compiler Version" => ResourceType.PanoramaStyle,
                    "Panorama Script Compiler Version" => ResourceType.PanoramaScript,
                    "Panorama Layout Compiler Version" => ResourceType.PanoramaLayout,
                    "Panorama Dynamic Images Compiler Version" => ResourceType.PanoramaDynamicImages,
                    _ => ResourceType.Panorama,
                },
                "VectorGraphic" => ResourceType.PanoramaVectorGraphic,
                _ => Enum.TryParse(identifier, false, out ResourceType type) ? type : ResourceType.Unknown,
            };
        }

        //: Resource.ConstructResourceType()
        internal static DATA Factory(BinaryPak source) => source.DataType switch
        {
            ResourceType.Panorama or ResourceType.PanoramaScript or ResourceType.PanoramaTypescript or ResourceType.PanoramaDynamicImages or ResourceType.PanoramaVectorGraphic => new DATAPanorama(),
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
            ResourceType.Mesh => source.Version != 0 ? new DATABinaryKV3() : source.ContainsBlockType<NTRO>() ? new DATABinaryNTRO() : new DATA(),
            _ => source.ContainsBlockType<NTRO>() ? new DATABinaryNTRO() : new DATA(),
        };
    }
}
