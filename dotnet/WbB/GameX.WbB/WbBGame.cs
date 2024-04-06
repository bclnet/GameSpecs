using System.Text.Json;

namespace GameX.WbB
{
    /// <summary>
    /// WbBGame
    /// </summary>
    /// <seealso cref="GameX.FamilyGame" />
    public class WbBGame : FamilyGame
    {
        public WbBGame(Family family, string id, JsonElement elem, FamilyGame dgame) : base(family, id, elem, dgame) { }

        /// <summary>
        /// Ensures this instance.
        /// </summary>
        /// <returns></returns>
        public override FamilyGame Ensure() => DatabaseManager.Ensure(this);
    }
}