using System;
using System.Collections.Generic;
using System.IO;

namespace GameX.Valve.Formats.Blocks
{
    //was:Resource/ResourceTypes/SoundEventScript
    public class DATASoundEventScript : DATABinaryNTRO
    {
        public Dictionary<string, string> SoundEventScriptValue { get; private set; } = new Dictionary<string, string>();

        public override void Read(Binary_Pak parent, BinaryReader r)
        {
            base.Read(parent, r);

            // Data is VSoundEventScript_t we need to iterate m_SoundEvents inside it.
            var soundEvents = Data.Get<IDictionary<string, object>>("m_SoundEvents");
            foreach (IDictionary<string, object> entry in soundEvents.Values)
            {
                // sound is VSoundEvent_t
                var soundName = entry.Get<string>("m_SoundName");
                var soundValue = entry.Get<string>("m_OperatorsKV").Replace("\n", Environment.NewLine); // make sure we have new lines
                if (SoundEventScriptValue.ContainsKey(soundName)) SoundEventScriptValue.Remove(soundName); // Duplicates last one wins
                SoundEventScriptValue.Add(soundName, soundValue);
            }
        }

        public override void WriteText(IndentedTextWriter w)
        {
            foreach (var entry in SoundEventScriptValue)
            {
                w.WriteLine($"\"{entry.Key}\" {{"); w.Indent++;
                // m_OperatorsKV wont be indented, so we manually indent it here, removing the last indent so we can close brackets later correctly.
                w.Write(entry.Value.Replace(Environment.NewLine, $"{Environment.NewLine}\t").TrimEnd('\t'));
                w.Indent--; w.WriteLine("}");
                w.WriteLine(string.Empty); // There is an empty line after every entry (including the last)
            }
        }
    }
}
