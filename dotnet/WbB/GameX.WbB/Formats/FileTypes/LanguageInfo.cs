using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.FileTypes
{
    /// <summary>
    /// This is in client_local_English.dat with the ID of 0x41000000.
    /// 
    /// Contains some very basic language and formatting rules.
    /// </summary>
    [PakFileType(PakFileType.StringState)]
    public class LanguageInfo : FileType, IHaveMetaInfo
    {
        public const uint FILE_ID = 0x41000000;

        public readonly int Version;
        public readonly short Base;
        public readonly short NumDecimalDigits;
        public readonly bool LeadingZero;

        public readonly short GroupingSize;
        public readonly char[] Numerals;
        public readonly char[] DecimalSeperator;
        public readonly char[] GroupingSeperator;
        public readonly char[] NegativeNumberFormat;
        public readonly bool IsZeroSingular;
        public readonly bool IsOneSingular;
        public readonly bool IsNegativeOneSingular;
        public readonly bool IsTwoOrMoreSingular;
        public readonly bool IsNegativeTwoOrLessSingular;

        public readonly char[] TreasurePrefixLetters;
        public readonly char[] TreasureMiddleLetters;
        public readonly char[] TreasureSuffixLetters;
        public readonly char[] MalePlayerLetters;
        public readonly char[] FemalePlayerLetters;
        public readonly uint ImeEnabledSetting;

        public readonly uint SymbolColor;
        public readonly uint SymbolColorText;
        public readonly uint SymbolHeight;
        public readonly uint SymbolTranslucence;
        public readonly uint SymbolPlacement;
        public readonly uint CandColorBase;
        public readonly uint CandColorBorder;
        public readonly uint CandColorText;
        public readonly uint CompColorInput;
        public readonly uint CompColorTargetConv;
        public readonly uint CompColorConverted;
        public readonly uint CompColorTargetNotConv;
        public readonly uint CompColorInputErr;
        public readonly uint CompTranslucence;
        public readonly uint CompColorText;
        public readonly uint OtherIME;

        public readonly int WordWrapOnSpace;
        public readonly char[] AdditionalSettings;
        public readonly uint AdditionalFlags;

        public LanguageInfo(BinaryReader r)
        {
            Version = r.ReadInt32();
            Base = r.ReadInt16();
            NumDecimalDigits = r.ReadInt16();
            LeadingZero = r.ReadBoolean();

            GroupingSize = r.ReadInt16();
            Numerals = UnpackList(r);
            DecimalSeperator = UnpackList(r);
            GroupingSeperator = UnpackList(r);
            NegativeNumberFormat = UnpackList(r);
            IsZeroSingular = r.ReadBoolean();
            IsOneSingular = r.ReadBoolean();
            IsNegativeOneSingular = r.ReadBoolean();
            IsTwoOrMoreSingular = r.ReadBoolean();
            IsNegativeTwoOrLessSingular = r.ReadBoolean(); r.Align();

            TreasurePrefixLetters = UnpackList(r);
            TreasureMiddleLetters = UnpackList(r);
            TreasureSuffixLetters = UnpackList(r);
            MalePlayerLetters = UnpackList(r);
            FemalePlayerLetters = UnpackList(r);
            ImeEnabledSetting = r.ReadUInt32();

            SymbolColor = r.ReadUInt32();
            SymbolColorText = r.ReadUInt32();
            SymbolHeight = r.ReadUInt32();
            SymbolTranslucence = r.ReadUInt32();
            SymbolPlacement = r.ReadUInt32();
            CandColorBase = r.ReadUInt32();
            CandColorBorder = r.ReadUInt32();
            CandColorText = r.ReadUInt32();
            CompColorInput = r.ReadUInt32();
            CompColorTargetConv = r.ReadUInt32();
            CompColorConverted = r.ReadUInt32();
            CompColorTargetNotConv = r.ReadUInt32();
            CompColorInputErr = r.ReadUInt32();
            CompTranslucence = r.ReadUInt32();
            CompColorText = r.ReadUInt32();
            OtherIME = r.ReadUInt32();

            WordWrapOnSpace = r.ReadInt32();
            AdditionalSettings = UnpackList(r);
            AdditionalFlags = r.ReadUInt32();
        }

        //: New
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"{nameof(LanguageInfo)}: {Id:X8}", items: new List<MetaInfo> {
                })
            };
            return nodes;
        }

        static char[] UnpackList(BinaryReader r)
        {
            var l = new List<char>();
            var numElements = r.ReadByte();
            for (var i = 0; i < numElements; i++) l.Add((char)r.ReadUInt16());
            return l.ToArray();
        }
    }
}
