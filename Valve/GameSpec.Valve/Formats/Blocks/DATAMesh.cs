using GameSpec.Metadata;
using GameSpec.Formats;
using GameSpec.Graphics;
using OpenStack.Graphics.Renderer;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace GameSpec.Valve.Formats.Blocks
{
    public class DATAMesh : IMeshInfo, IGetMetadataInfo
    {
        readonly BinaryPak _source;
        readonly DATA _data;

        public DATAMesh(BinaryPak source)
        {
            _source = source;
            _data = source.DATA;
            VBIB = source.VBIB;
            GetBounds();
        }
        public DATAMesh(DATA data, IVBIB vbib)
        {
            _data = data;
            VBIB = vbib;
            GetBounds();
        }

        public List<MetadataInfo> GetInfoNodes(MetadataManager resource, FileMetadata file, object tag) => (_source as IGetMetadataInfo).GetInfoNodes(resource, file, tag);

        public IVBIB VBIB { get; }

        public Vector3 MinBounds { get; private set; }
        public Vector3 MaxBounds { get; private set; }

        public IDictionary<string, object> Data
            => _data switch
            {
                DATABinaryKV3 kv3 => kv3.Data,
                DATABinaryNTRO ntro => ntro.Data,
                _ => throw new InvalidOperationException($"Unknown model data type {Data.GetType().Name}"),
            };

        void GetBounds()
        {
            var sceneObjects = Data.GetArray("m_sceneObjects");
            if (sceneObjects.Length == 0) { MinBounds = MaxBounds = new Vector3(0, 0, 0); return; }
            var minBounds = sceneObjects[0].GetVector3("m_vMinBounds");
            var maxBounds = sceneObjects[0].GetVector3("m_vMaxBounds");
            for (var i = 1; i < sceneObjects.Length; ++i)
            {
                var localMin = sceneObjects[i].GetVector3("m_vMinBounds");
                var localMax = sceneObjects[i].GetVector3("m_vMaxBounds");
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
    }
}
