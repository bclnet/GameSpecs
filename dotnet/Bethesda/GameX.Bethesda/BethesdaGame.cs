using System.Text.Json;

namespace GameX.Bethesda
{
    /// <summary>
    /// BethesdaGame
    /// </summary>
    /// <seealso cref="GameX.FamilyGame" />
    public class BethesdaGame : FamilyGame
    {
        public BethesdaGame(Family family, string id, JsonElement elem, FamilyGame dgame) : base(family, id, elem, dgame) { }

        /// <summary>
        /// Ensures this instance.
        /// </summary>
        /// <returns></returns>
        public override FamilyGame Ensure() => DatabaseManager.Ensure(this);
    }
}