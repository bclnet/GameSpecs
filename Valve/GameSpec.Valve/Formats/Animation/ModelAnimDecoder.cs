using System;

namespace GameSpec.Valve.Formats.Blocks.Animation
{
    internal static class ModelAnimDecoder
    {
        public static int Size(this ModelAnimDecoderType t)
        {
            switch (t)
            {
                case ModelAnimDecoderType.CCompressedFullVector3: return 12;
                case ModelAnimDecoderType.CCompressedStaticVector3:
                case ModelAnimDecoderType.CCompressedAnimVector3:
                case ModelAnimDecoderType.CCompressedAnimQuaternion: return 6;
                default: return 0;
            }
        }

        public static ModelAnimDecoderType FromString(string s)
        {
            switch (s)
            {
                case "CCompressedReferenceFloat": return ModelAnimDecoderType.CCompressedReferenceFloat;
                case "CCompressedStaticFloat": return ModelAnimDecoderType.CCompressedStaticFloat;
                case "CCompressedFullFloat": return ModelAnimDecoderType.CCompressedFullFloat;
                case "CCompressedReferenceVector3": return ModelAnimDecoderType.CCompressedReferenceVector3;
                case "CCompressedStaticVector3": return ModelAnimDecoderType.CCompressedStaticVector3;
                case "CCompressedStaticFullVector3": return ModelAnimDecoderType.CCompressedStaticFullVector3;
                case "CCompressedAnimVector3": return ModelAnimDecoderType.CCompressedAnimVector3;
                case "CCompressedDeltaVector3": return ModelAnimDecoderType.CCompressedDeltaVector3;
                case "CCompressedFullVector3": return ModelAnimDecoderType.CCompressedFullVector3;
                case "CCompressedReferenceQuaternion": return ModelAnimDecoderType.CCompressedReferenceQuaternion;
                case "CCompressedStaticQuaternion": return ModelAnimDecoderType.CCompressedStaticQuaternion;
                case "CCompressedAnimQuaternion": return ModelAnimDecoderType.CCompressedAnimQuaternion;
                case "CCompressedFullQuaternion": return ModelAnimDecoderType.CCompressedFullQuaternion;
                case "CCompressedReferenceInt": return ModelAnimDecoderType.CCompressedReferenceInt;
                case "CCompressedStaticChar": return ModelAnimDecoderType.CCompressedStaticChar;
                case "CCompressedFullChar": return ModelAnimDecoderType.CCompressedFullChar;
                case "CCompressedStaticShort": return ModelAnimDecoderType.CCompressedStaticShort;
                case "CCompressedFullShort": return ModelAnimDecoderType.CCompressedFullShort;
                case "CCompressedStaticInt": return ModelAnimDecoderType.CCompressedStaticInt;
                case "CCompressedFullInt": return ModelAnimDecoderType.CCompressedFullInt;
                case "CCompressedReferenceBool": return ModelAnimDecoderType.CCompressedReferenceBool;
                case "CCompressedStaticBool": return ModelAnimDecoderType.CCompressedStaticBool;
                case "CCompressedFullBool": return ModelAnimDecoderType.CCompressedFullBool;
                case "CCompressedReferenceColor32": return ModelAnimDecoderType.CCompressedReferenceColor32;
                case "CCompressedStaticColor32": return ModelAnimDecoderType.CCompressedStaticColor32;
                case "CCompressedFullColor32": return ModelAnimDecoderType.CCompressedFullColor32;
                case "CCompressedReferenceVector2D": return ModelAnimDecoderType.CCompressedReferenceVector2D;
                case "CCompressedStaticVector2D": return ModelAnimDecoderType.CCompressedStaticVector2D;
                case "CCompressedFullVector2D": return ModelAnimDecoderType.CCompressedFullVector2D;
                case "CCompressedReferenceVector4D": return ModelAnimDecoderType.CCompressedReferenceVector4D;
                case "CCompressedStaticVector4D": return ModelAnimDecoderType.CCompressedStaticVector4D;
                case "CCompressedFullVector4D": return ModelAnimDecoderType.CCompressedFullVector4D;
                default: Console.Error.WriteLine($"Unhandled AnimDecoderType string: {s}"); return ModelAnimDecoderType.Unknown;
            }
        }
    }
}
