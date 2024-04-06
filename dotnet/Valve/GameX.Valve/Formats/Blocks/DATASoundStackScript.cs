using System;
using System.Collections.Generic;
using System.IO;

namespace GameX.Valve.Formats.Blocks
{
    //was:Resource/ResourceTypes/SoundStackScript
    public class DATASoundStackScript : DATA
    {
        public Dictionary<string, string> SoundStackScriptValue { get; private set; } = new Dictionary<string, string>();

        public override void Read(Binary_Pak parent, BinaryReader r)
        {
            r.Seek(Offset);
            var version = r.ReadInt32();
            if (version != 8) throw new FormatException($"Unknown version: {version}");
            var count = r.ReadInt32();
            var offset = r.BaseStream.Position;
            for (var i = 0; i < count; i++)
            {
                var offsetToName = offset + r.ReadInt32(); offset += 4;
                var offsetToValue = offset + r.ReadInt32(); offset += 4;
                r.Seek(offsetToName);
                var name = r.ReadZUTF8();
                r.Seek(offsetToValue);
                var value = r.ReadZUTF8();
                r.Seek(offset);
                if (SoundStackScriptValue.ContainsKey(name)) SoundStackScriptValue.Remove(name); // duplicates last wins
                SoundStackScriptValue.Add(name, value);
            }
        }

        public override void WriteText(IndentedTextWriter w)
        {
            foreach (var entry in SoundStackScriptValue)
            {
                w.WriteLine($"// {entry.Key}");
                w.Write(entry.Value);
                w.WriteLine(string.Empty);
            }
        }
    }
}
