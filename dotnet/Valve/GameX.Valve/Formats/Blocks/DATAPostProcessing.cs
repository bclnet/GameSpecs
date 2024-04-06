using System.Collections.Generic;

namespace GameX.Valve.Formats.Blocks
{
    //was:Resource/ResourceTypes/PostProcessing
    public class DATAPostProcessing : DATABinaryKV3OrNTRO
    {
        public DATAPostProcessing() : base("PostProcessingResource_t") { }

        public IDictionary<string, object> GetTonemapParams() => Data.Get<bool>("m_bHasTonemapParams") ? Data.Get<IDictionary<string, object>>("m_toneMapParams") : null;
        public IDictionary<string, object> GetBloomParams() => Data.Get<bool>("m_bHasBloomParams") ? Data.Get<IDictionary<string, object>>("m_bloomParams") : null;
        public IDictionary<string, object> GetVignetteParams() => Data.Get<bool>("m_bHasVignetteParams") ? Data.Get<IDictionary<string, object>>("m_vignetteParams") : null;
        public IDictionary<string, object> GetLocalContrastParams() => Data.Get<bool>("m_bHasLocalContrastParams") ? Data.Get<IDictionary<string, object>>("m_localConstrastParams") : null;
        public bool HasColorCorrection() => Data.TryGetValue("m_bHasColorCorrection", out var value) ? (bool)value : true; // Assumed true pre Aperture Desk Job
        public int GetColorCorrectionLUTDimension() => Data.Get<int>("m_nColorCorrectionVolumeDim");
        public byte[] GetColorCorrectionLUT() => Data.Get<byte[]>("m_colorCorrectionVolumeData");

        public byte[] GetRawData()
        {
            var lut = GetColorCorrectionLUT().Clone() as byte[];
            var j = 0;
            for (var i = 0; i < lut.Length; i++)
            {
                if (((i + 1) % 4) == 0) continue; // Skip each 4th byte
                lut[j++] = lut[i];
            }
            return lut[..j];
        }

        public string ToValvePostProcessing(bool preloadLookupTable = false, string lutFileName = "")
        {
            var outKV3 = new Dictionary<string, object>
            {
                { "_class", "CPostProcessData" }
            };

            var layers = new List<object>();

            var tonemapParams = GetTonemapParams();
            var bloomParams = GetBloomParams();
            var vignetteParams = GetVignetteParams();
            var localContrastParams = GetLocalContrastParams();

            if (tonemapParams != null)
            {
                var tonemappingLayer = new Dictionary<string, object>
                {
                    { "_class", "CToneMappingLayer" },
                    { "m_nOpacityPercent", 100L },
                    { "m_bVisible", true },
                    { "m_pLayerMask", null },
                };
                var tonemappingLayerParams = new Dictionary<string, object>();
                foreach (var kv in tonemapParams) tonemappingLayerParams.Add(kv.Key, kv.Value);
                tonemappingLayer.Add("m_params", tonemappingLayerParams);
                layers.Add(tonemappingLayer);
            }

            if (bloomParams != null)
            {
                var bloomLayer = new Dictionary<string, object>
                {
                    { "_class", "CBloomLayer" },
                    { "m_name",  "Bloom" },
                    { "m_nOpacityPercent", 100L },
                    { "m_bVisible", true },
                    { "m_pLayerMask", null },
                };
                var bloomLayerParams = new Dictionary<string, object>();
                foreach (var kv in tonemapParams) bloomLayerParams.Add(kv.Key, kv.Value);
                bloomLayer.Add("m_params", bloomLayerParams);
                layers.Add(bloomLayer);
            }

            if (vignetteParams != null) { } // TODO: How does the vignette layer look like?
            if (localContrastParams != null) { } // TODO: How does the local contrast layer look like?

            // All other layers are compiled into a 3D lookup table
            if (HasColorCorrection())
            {
                var ccLayer = new Dictionary<string, object>
                {
                    { "_class", "CColorLookupColorCorrectionLayer" },
                    { "m_name",  "VRF Extracted Lookup Table" },
                    { "m_nOpacityPercent", 100L },
                    { "m_bVisible", true },
                    { "m_pLayerMask", null },
                    { "m_fileName", lutFileName },
                };
                var lut = new List<object>();
                if (preloadLookupTable) foreach (var b in GetRawData()) lut.Add(b / 255d);
                ccLayer.Add("m_lut", lut.ToArray());
                ccLayer.Add("m_nDim", GetColorCorrectionLUTDimension());
                layers.Add(ccLayer);
            }

            outKV3.Add("m_layers", layers.ToArray());

            return new DATABinaryKV3File(outKV3).ToString();
        }
    }
}
