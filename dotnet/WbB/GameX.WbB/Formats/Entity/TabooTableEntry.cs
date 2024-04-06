using System.IO;
using System.Text.RegularExpressions;

namespace GameX.WbB.Formats.Entity
{
    public class TabooTableEntry
    {
        public readonly uint Unknown1; // This always seems to be 0x00010101
        public readonly ushort Unknown2; // This always seems to be 0
        /// <summary>
        /// All patterns are lower case<para />
        /// Patterns are expected in the following format: [*]word[*]<para />
        /// The asterisk is optional. They can be used to forbid strings that contain a pattern, require the pattern to be the whole word, or require the word to either start/end with the pattern.
        /// </summary>
        public readonly string[] BannedPatterns;

        public TabooTableEntry(BinaryReader r)
        {
            Unknown1 = r.ReadUInt32();
            Unknown2 = r.ReadUInt16();
            BannedPatterns = r.ReadL32FArray(x => x.ReadString());
        }

        /// <summary>
        /// This will search all the BannedPatterns to see if the input passes or fails.
        /// </summary>
        public bool ContainsBadWord(string input)
        {
            // Our entire banned patterns list should be lower case
            input = input.ToLowerInvariant();
            // First, we need to split input into separate words
            var words = input.Split(' ');
            foreach (var word in words)
                foreach (var bannedPattern in BannedPatterns) if (Regex.IsMatch(word, $"^{bannedPattern.Replace("*", ".*")}$")) return true;
            return false;
        }
    }
}
