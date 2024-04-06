namespace System.Globalization
{
    /// <summary>
    /// Grammar
    /// </summary>
    public static class Grammar
    {
        public static bool StartsWithVowel(this string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            var firstLetter = s.ToLower()[0];
            return "aeiou".IndexOf(firstLetter) >= 0;
        }

        /// <summary>
        /// For objects that don't have a PropertyString.PluralName
        /// </summary>
        public static string Pluralize(this string name)
            => name.EndsWith("us") ? name + "s" // This should be i but pcap shows "You have killed 4 Sarcophaguss! Your task is complete!"
            : (name.EndsWith("ch") || name.EndsWith("s") || name.EndsWith("sh") || name.EndsWith("x") || name.EndsWith("z")) ? name + "es"
            : name.EndsWith("th") ? name
            : name + "s";

        /// <summary>
        /// Converts an input integer to its ordinal number
        /// </summary>
        /// <param name="s">The integer to convert</param>
        /// <returns>Returns a string of the ordinal</returns>
        public static string ToOrdinal(this int s) => s + ToOrdinalSuffix(s);

        /// <summary>
        /// Converts an input integer to its ordinal suffix
        /// Useful if you need to format the suffix separately of the number itself
        /// </summary>
        /// <param name="s">The integer to convert</param>
        /// <returns>Returns a string of the ordinal suffix</returns>
        public static string ToOrdinalSuffix(this int s)
        {
            // TODO: this only handles English ordinals - in future we may wish to consider the culture
            // note, we are allowing zeroth - http://en.wikipedia.org/wiki/Zeroth
            if (s < 0) throw new ArgumentOutOfRangeException(nameof(s), "Ordinal numbers cannot be negative");

            // first check special case, if the result ends in 11, 12, 13, should be th
            switch (s % 100) { case 11: case 12: case 13: return "th"; }
            // else we just check the last digit
            return (s % 10) switch
            {
                1 => "st",
                2 => "nd",
                3 => "rd",
                _ => "th",
            };
        }
    }
}
