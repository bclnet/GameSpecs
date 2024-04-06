using GameX.Formats;
using GameX.Meta;
using OpenStack.Graphics.Renderer1;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace GameX.Valve.Formats.Blocks
{
    //was:Resource/ResourceTypes/Mesh
    public class DATAMesh : DATABinaryKV3OrNTRO, IMesh, IHaveMetaInfo
    {
        IVBIB _vbib;
        public IVBIB VBIB
        {
            //new format has VBIB block, for old format we can get it from NTRO DATA block
            get => _vbib ??= Parent.VBIB ?? new VBIB(Data);
            set => _vbib = value;
        }
        public Vector3 MinBounds { get; private set; }
        public Vector3 MaxBounds { get; private set; }
        public DATAMorph MorphData { get; set; }

        public DATAMesh(Binary_Pak pak) : base("PermRenderMeshData_t") { }

        public void GetBounds()
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

        public async void LoadExternalMorphData(PakFile fileLoader)
        {
            if (MorphData == null)
            {
                var morphSetPath = Data.Get<string>("m_morphSet");
                if (!string.IsNullOrEmpty(morphSetPath))
                {
                    var morphSetResource = await fileLoader.LoadFileObject<Binary_Pak>(morphSetPath + "_c");
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

        public List<MetaInfo> GetInfoNodes(MetaManager resource, FileSource file, object tag) => (Parent as IHaveMetaInfo).GetInfoNodes(resource, file, tag);
    }
}
