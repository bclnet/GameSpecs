using GameSpec.Metadata;
using GameSpec.Formats;
using GameSpec.Graphics;
using OpenStack.Graphics.Renderer;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace GameSpec.Valve.Formats.Blocks
{
    //was:Resource/ResourceTypes/Mesh
    public class DATAMesh : DATABinaryKV3OrNTRO, IMeshInfo, IGetMetadataInfo
    {
        [Flags]
        public enum RenderMeshDrawPrimitiveFlags //was:Resource/Enum/RenderMeshDrawPrimitiveFlags
        {
            None = 0x0,
            UseShadowFastPath = 0x1,
            UseCompressedNormalTangent = 0x2,
            IsOccluder = 0x4,
            InputLayoutIsNotMatchedToMaterial = 0x8,
            HasBakedLightingFromVertexStream = 0x10,
            HasBakedLightingFromLightmap = 0x20,
            CanBatchWithDynamicShaderConstants = 0x40,
            DrawLast = 0x80,
            HasPerInstanceBakedLightingData = 0x100,
        }

        IVBIB _cachedVBIB;
        public IVBIB VBIB
        {
            //new format has VBIB block, for old format we can get it from NTRO DATA block
            get => _cachedVBIB ?? (_cachedVBIB = Parent.VBIB ?? new VBIB(Data));
            set => _cachedVBIB = value;
        }
        public Vector3 MinBounds { get; private set; }
        public Vector3 MaxBounds { get; private set; }
        public DATAMorph MorphData { get; set; }

        public DATAMesh(BinaryPak pak) : base("PermRenderMeshData_t") { }

        void GetBounds()
        {
            var sceneObjects = Data.GetArray("m_sceneObjects");
            if (sceneObjects.Length == 0)
            {
                MinBounds = MaxBounds = new Vector3(0, 0, 0);
                return;
            }
            var minBounds = sceneObjects[0].GetVector3("m_vMinBounds"); //: sceneObjects[0].GetSub("m_vMinBounds").ToVector3();
            var maxBounds = sceneObjects[0].GetVector3("m_vMaxBounds"); //: sceneObjects[0].GetSub("m_vMaxBounds").ToVector3();
            for (var i = 1; i < sceneObjects.Length; ++i)
            {
                var localMin = sceneObjects[i].GetVector3("m_vMinBounds"); //: sceneObjects[i].GetSub("m_vMinBounds").ToVector3();
                var localMax = sceneObjects[i].GetVector3("m_vMaxBounds"); //: sceneObjects[i].GetSub("m_vMaxBounds").ToVector3();
                minBounds.X = Math.Min(minBounds.X, localMin.X);
                minBounds.Y = Math.Min(minBounds.Y, localMin.Y);
                minBounds.Z = Math.Min(minBounds.Z, localMin.Z);
                maxBounds.X = Math.Max(maxBounds.X, localMax.X);
                maxBounds.Y = Math.Max(maxBounds.Y, localMax.Y);
                maxBounds.Z = Math.Max(maxBounds.Z, localMax.Z);
            }
            MinBounds = minBounds;
            MaxBounds = maxBounds;
        }

        public static bool IsCompressedNormalTangent(IDictionary<string, object> drawCall)
        {
            if (drawCall.ContainsKey("m_bUseCompressedNormalTangent")) return drawCall.Get<bool>("m_bUseCompressedNormalTangent");
            if (!drawCall.ContainsKey("m_nFlags")) return false;
            var flags = drawCall.Get<object>("m_nFlags");
            return flags switch
            {
                string flagsString => flagsString.Contains("MESH_DRAW_FLAGS_USE_COMPRESSED_NORMAL_TANGENT", StringComparison.InvariantCulture),
                long flagsLong => ((RenderMeshDrawPrimitiveFlags)flagsLong & RenderMeshDrawPrimitiveFlags.UseCompressedNormalTangent) != 0,
                byte flagsByte => ((RenderMeshDrawPrimitiveFlags)flagsByte & RenderMeshDrawPrimitiveFlags.UseCompressedNormalTangent) != 0,
                _ => false
            };
        }

        public async void LoadExternalMorphData(PakFile fileLoader)
        {
            if (MorphData == null)
            {
                var morphSetPath = Data.Get<string>("m_morphSet");
                if (!string.IsNullOrEmpty(morphSetPath))
                {
                    var morphSetResource = await fileLoader.LoadFileObjectAsync<BinaryPak>(morphSetPath + "_c");
                    if (morphSetResource != null)
                    {
                        //MorphData = morphSetResource.GetBlockByType<MRPH>() as DATAMorph;
                        var abc = morphSetResource.GetBlockByType<MRPH>();
                        MorphData = abc as object as DATAMorph;
                    }
                }
            }

            await MorphData?.LoadFlexData(fileLoader);
        }

        public List<MetadataInfo> GetInfoNodes(MetadataManager resource, FileMetadata file, object tag) => (Parent as IGetMetadataInfo).GetInfoNodes(resource, file, tag);
    }
}
