using System.Collections.Generic;
using System.IO;

namespace GameX.Valve.Formats.Blocks
{
    /// <summary>
    /// "REDI" block. ResourceEditInfoBlock_t.
    /// </summary>
    public class REDI : Block
    {
        /// <summary>
        /// This is not a real Valve enum, it's just the order they appear in.
        /// </summary>
        public enum REDIStruct
        {
            InputDependencies,
            AdditionalInputDependencies,
            ArgumentDependencies,
            SpecialDependencies,
            CustomDependencies,
            AdditionalRelatedFiles,
            ChildResourceList,
            ExtraIntData,
            ExtraFloatData,
            ExtraStringData,
            End,
        }

        public Dictionary<REDIStruct, REDIAbstract> Structs { get; private set; } = new Dictionary<REDIStruct, REDIAbstract>();

        public override void Read(Binary_Pak parent, BinaryReader r)
        {
            r.Seek(Offset);
            for (var i = REDIStruct.InputDependencies; i < REDIStruct.End; i++)
            {
                var block = REDIFactory(i);
                block.Offset = (uint)r.BaseStream.Position + r.ReadUInt32();
                block.Size = r.ReadUInt32();
                Structs.Add(i, block);
            }
            foreach (var block in Structs) block.Value.Read(parent, r);
        }

        public override void WriteText(IndentedTextWriter w)
        {
            w.WriteLine("ResourceEditInfoBlock_t {"); w.Indent++;
            foreach (var dep in Structs) dep.Value.WriteText(w);
            w.Indent--; w.WriteLine("}");
        }

        static REDIAbstract REDIFactory(REDIStruct id)
            => id switch
            {
                REDIStruct.InputDependencies => new REDIInputDependencies(),
                REDIStruct.AdditionalInputDependencies => new REDIAdditionalInputDependencies(),
                REDIStruct.ArgumentDependencies => new REDIArgumentDependencies(),
                REDIStruct.SpecialDependencies => new REDISpecialDependencies(),
                REDIStruct.CustomDependencies => new REDICustomDependencies(),
                REDIStruct.AdditionalRelatedFiles => new REDIAdditionalRelatedFiles(),
                REDIStruct.ChildResourceList => new REDIChildResourceList(),
                REDIStruct.ExtraIntData => new REDIExtraIntData(),
                REDIStruct.ExtraFloatData => new REDIExtraFloatData(),
                REDIStruct.ExtraStringData => new REDIExtraStringData(),
                _ => throw new InvalidDataException("Unknown struct in REDI block."),
            };
    }
}
