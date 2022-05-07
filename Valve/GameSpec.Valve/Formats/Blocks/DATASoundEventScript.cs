using System;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.Valve.Formats.Blocks
{
    public class DATASoundEventScript : DATABinaryNTRO
    {
        public Dictionary<string, string> SoundEventScriptValue { get; private set; }

        public override void Read(BinaryPak parent, BinaryReader r)
        {
            base.Read(parent, r);
            //SoundEventScriptValue = new Dictionary<string, string>();
            //// DAT is VSoundEventScript_t we need to iterate m_SoundEvents inside it.
            //var soundEvents = (NTROArray)Data["m_SoundEvents"];
            //foreach (var entry in soundEvents)
            //{
            //    // sound is VSoundEvent_t
            //    var sound = ((NTROValue<NTROStruct>)entry).Value;
            //    var soundName = ((NTROValue<string>)sound["m_SoundName"]).Value;
            //    var soundValue = ((NTROValue<string>)sound["m_OperatorsKV"]).Value.Replace("\n", Environment.NewLine); // make sure we have new lines
            //    if (SoundEventScriptValue.ContainsKey(soundName))
            //        SoundEventScriptValue.Remove(soundName); // Valve have duplicates, assume last is correct?
            //    SoundEventScriptValue.Add(soundName, soundValue);
            //}
        }

        public override void WriteText(IndentedTextWriter w)
        {
            foreach (var entry in SoundEventScriptValue)
            {
                w.WriteLine($"\"{entry.Key}\" {{");
                w.Indent++;
                // m_OperatorsKV wont be indented, so we manually indent it here, removing the last indent so we can close brackets later correctly.
                w.Write(entry.Value.Replace(Environment.NewLine, $"{Environment.NewLine}\t").TrimEnd('\t'));
                w.Indent--; w.WriteLine("}");
                w.WriteLine(string.Empty); // There is an empty line after every entry (including the last)
            }
        }
    }
}
