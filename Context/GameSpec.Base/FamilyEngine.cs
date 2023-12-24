using System;
using System.Text.Json;
using static GameSpec.Util;

namespace GameSpec
{
    /// <summary>
    /// FamilyEngine
    /// </summary>
    public class FamilyEngine
    {
        /// <summary>
        /// Gets or sets the family.
        /// </summary>
        public Family Family { get; set; }
        /// <summary>
        /// Gets or sets the game identifier.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Gets or sets the game name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// FamilyEngine
        /// </summary>
        /// <param name="family"></param>
        /// <param name="id"></param>
        /// <param name="elem"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public FamilyEngine(Family family, string id, JsonElement elem)
        {
            Family = family;
            Id = id;
            Name = _value(elem, "name") ?? throw new ArgumentNullException("name");
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => Name;
    }
}