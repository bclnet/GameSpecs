using GameSpec.Formats;
using GameSpec.Meta;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GameSpec.Origin.Formats.UO
{
    public unsafe class Binary_Speech : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Speech(r));

        #region Records

        public class Record
        {
            public int Index;
            public List<string> Strings = new List<string>();
            public List<Regex> Regex = new List<Regex>();
            public Record(int index) => Index = index;
        }

        public static void GetSpeechTriggers(string text, string lang, out int count, out int[] triggers)
        {
            var t = new List<int>();
            var speechTable = 0;
            foreach (var e in Records[speechTable])
                for (var i = 0; i < e.Value.Regex.Count; i++)
                    if (e.Value.Regex[i].IsMatch(text) && !t.Contains(e.Key))
                        t.Add(e.Key);
            count = t.Count;
            triggers = t.ToArray();
        }

        static readonly List<Dictionary<int, Record>> Records = new List<Dictionary<int, Record>>();

        #endregion

        // file: speech.mul
        public Binary_Speech(BinaryReader r)
        {
            var lastIndex = -1;
            Dictionary<int, Record> record = null;
            while (r.PeekChar() >= 0)
            {
                var index = (r.ReadByte() << 8) | r.ReadByte();
                var length = (r.ReadByte() << 8) | r.ReadByte();
                var text = Encoding.UTF8.GetString(r.ReadBytes(length)).Trim();
                if (text.Length == 0) continue;
                if (record == null || lastIndex > index)
                {
                    if (index == 0 && text == "*withdraw*") Records.Insert(0, record = new Dictionary<int, Record>());
                    else Records.Add(record = new Dictionary<int, Record>());
                }
                lastIndex = index;
                record.TryGetValue(index, out var entry);
                if (entry == null) record[index] = entry = new Record(index);
                entry.Strings.Add(text);
                entry.Regex.Add(new Regex(text.Replace("*", @".*"), RegexOptions.IgnoreCase));
            }
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "Speech File" }),
                new MetaInfo("Speech", items: new List<MetaInfo> {
                    new MetaInfo($"Count: {Records.Count}"),
                })
            };
            return nodes;
        }
    }
}
