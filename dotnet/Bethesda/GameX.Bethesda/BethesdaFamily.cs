using System.Text.Json;

namespace GameX.Bethesda
{
    /// <summary>
    /// BethesdaFamily
    /// </summary>
    /// <seealso cref="GameX.Family" />
    public class BethesdaFamily : Family {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownFamily"/> class.
        /// </summary>
        public BethesdaFamily(JsonElement elem) : base(elem) { }
    }
}