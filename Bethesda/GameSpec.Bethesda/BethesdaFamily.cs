using System.Text.Json;

namespace GameSpec.Bethesda
{
    /// <summary>
    /// BethesdaFamily
    /// </summary>
    /// <seealso cref="GameSpec.Family" />
    public class BethesdaFamily : Family {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownFamily"/> class.
        /// </summary>
        public BethesdaFamily(JsonElement elem) : base(elem) { }
    }
}