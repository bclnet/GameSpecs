using System;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.Valve.Formats.Blocks
{
    public class DATASoundStackScript : DATA
    {
        public Dictionary<string, string> SoundStackScriptValue { get; private set; }

        public override void Read(BinaryPak parent, BinaryReader r)
        {
            r.Seek(Offset);
            var version = r.ReadInt32();
            if (version != 8) throw new NotImplementedException($"Unknown soundstack version: {version}");
            SoundStackScriptValue = new Dictionary<string, string>();
            var count = r.ReadInt32();
            var offset = r.BaseStream.Position;
            for (var i = 0; i < count; i++)
            {
                var offsetToName = offset + r.ReadInt32();
                offset += 4;
                var offsetToValue = offset + r.ReadInt32();
                offset += 4;
                r.BaseStream.Position = offsetToName;
                var name = r.ReadZUTF8();
                r.BaseStream.Position = offsetToValue;
                var value = r.ReadZUTF8();
                r.BaseStream.Position = offset;
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
