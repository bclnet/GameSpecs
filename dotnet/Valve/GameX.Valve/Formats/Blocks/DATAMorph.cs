using OpenStack.Graphics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace GameX.Valve.Formats.Blocks
{
    //was:Resource/ResourceTypes/Morph
    public class DATAMorph : DATABinaryKV3OrNTRO
    {
        public enum MorphBundleType //was:Resource/Enum/MorphBundleType
        {
            None = 0,
            PositionSpeed = 1,
            NormalWrinkle = 2,
        }

        public Dictionary<string, Vector3[]> FlexData { get; private set; }

        public DATAMorph() : base("MorphSetData_t") { }

        public async Task LoadFlexData(PakFile fileLoader)
        {
            var atlasPath = Data.Get<string>("m_pTextureAtlas");
            if (string.IsNullOrEmpty(atlasPath)) return;

            var textureResource = await fileLoader.LoadFileObject<DATATexture>(atlasPath + "_c");
            if (textureResource == null) return;

            LocalFunction();
            // Note the use of a local non-async function so you can use `Span<T>`
            void LocalFunction()
            {
                var width = Data.GetInt32("m_nWidth");
                var height = Data.GetInt32("m_nHeight");

                FlexData = new Dictionary<string, Vector3[]>();
                var texture = textureResource; // as ITexture;
                var texWidth = texture.Width;
                var texHeight = texture.Height;
                var texPixels = texture.ReadOne(0);
                // Some vmorf_c may be another old struct(NTROValue, eg: models/heroes/faceless_void/faceless_void_body.vmdl_c). the latest struct is IKeyValueCollection.
                var morphDatas = GetMorphKeyValueCollection(Data, "m_morphDatas");
                if (morphDatas == null || !morphDatas.Any()) return;

                var bundleTypes = GetMorphKeyValueCollection(Data, "m_bundleTypes").Select(kv => ParseBundleType(kv.Value)).ToArray();

                foreach (var pair in morphDatas)
                {
                    if (!(pair.Value is IDictionary<string, object> morphData)) continue;

                    var morphName = morphData.Get<string>("m_name");
                    if (string.IsNullOrEmpty(morphName)) continue; // Exist some empty names may need skip.

                    var rectData = new Vector3[height * width];
                    rectData.Initialize();

                    var morphRectDatas = morphData.GetSub("m_morphRectDatas");
                    foreach (var morphRectData in morphRectDatas)
                    {
                        var rect = morphRectData.Value as IDictionary<string, object>;
                        var xLeftDst = rect.GetInt32("m_nXLeftDst");
                        var yTopDst = rect.GetInt32("m_nYTopDst");
                        var rectWidth = (int)Math.Round(rect.GetFloat("m_flUWidthSrc") * texWidth, 0);
                        var rectHeight = (int)Math.Round(rect.GetFloat("m_flVHeightSrc") * texHeight, 0);
                        var bundleDatas = rect.GetSub("m_bundleDatas");

                        foreach (var bundleData in bundleDatas)
                        {
                            var bundleKey = int.Parse(bundleData.Key, CultureInfo.InvariantCulture);

                            // We currently only support Position. TODO: Add Normal support for gltf
                            if (bundleTypes[bundleKey] != MorphBundleType.PositionSpeed) continue;

                            var bundle = bundleData.Value as IDictionary<string, object>;
                            var rectU = (int)Math.Round(bundle.GetFloat("m_flULeftSrc") * texWidth, 0);
                            var rectV = (int)Math.Round(bundle.GetFloat("m_flVTopSrc") * texHeight, 0);
                            var ranges = bundle.Get<float[]>("m_ranges");
                            var offsets = bundle.Get<float[]>("m_offsets");

                            throw new NotImplementedException();
                            //for (var row = rectV; row < rectV + rectHeight; row++)
                            //    for (var col = rectU; col < rectU + rectWidth; col++)
                            //    {
                            //        var colorIndex = row * texWidth + col;
                            //        var color = texPixels[colorIndex];
                            //        var dstI = row - rectV + yTopDst;
                            //        var dstJ = col - rectU + xLeftDst;

                            //        rectData[dstI * width + dstJ] = new Vector3(
                            //            color.Red / 255f * ranges[0] + offsets[0],
                            //            color.Green / 255f * ranges[1] + offsets[1],
                            //            color.Blue / 255f * ranges[2] + offsets[2]
                            //        );
                            //    }
                        }
                    }
                    FlexData.Add(morphName, rectData);
                }
            }
        }

        static MorphBundleType ParseBundleType(object bundleType)
            => bundleType is uint bundleTypeEnum ? (MorphBundleType)bundleTypeEnum
            : bundleType is string bundleTypeString ? bundleTypeString switch
            {
                "MORPH_BUNDLE_TYPE_POSITION_SPEED" => MorphBundleType.PositionSpeed,
                "BUNDLE_TYPE_POSITION_SPEED" => MorphBundleType.PositionSpeed,
                "MORPH_BUNDLE_TYPE_NORMAL_WRINKLE" => MorphBundleType.NormalWrinkle,
                _ => throw new NotImplementedException($"Unhandled bundle type: {bundleTypeString}"),
            }
            : throw new NotImplementedException("Unhandled bundle type");

        static IDictionary<string, object> GetMorphKeyValueCollection(IDictionary<string, object> data, string name)
        {
            throw new NotImplementedException();
            //var kvObj = data.Get<object>(name);
            //if (kvObj is NTROStruct ntroStruct) return ntroStruct.ToKVObject();
            //if (kvObj is NTROValue[] ntroArray)
            //{
            //    var kv = new KVObject("root", true);
            //    foreach (var ntro in ntroArray) kv.AddProperty("", ntro.ToKVValue());
            //    return kv;
            //}
            //return kvObj as IDictionary<string, object>;
        }
    }
}
