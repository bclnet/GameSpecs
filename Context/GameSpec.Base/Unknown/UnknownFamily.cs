using System.Text.Json;

namespace GameSpec.Unknown
{
    /// <summary>
    /// UnknownFamily
    /// </summary>
    /// <seealso cref="GameSpec.Family" />
    public class UnknownFamily : Family
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownFamily"/> class.
        /// </summary>
        internal UnknownFamily() : base() { }
        public UnknownFamily(JsonElement elem) : base(elem) { }
    }
}