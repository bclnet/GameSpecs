using System.Text.Json;

namespace GameSpec.Id
{
    /// <summary>
    /// IdGame
    /// </summary>
    /// <seealso cref="GameSpec.FamilyGame" />
    public class IdGame : FamilyGame
    {
        public IdGame(Family family, string id, JsonElement elem, FamilyGame dgame) : base(family, id, elem, dgame) { }

        /// <summary>
        /// Ensures this instance.
        /// </summary>
        /// <returns></returns>
        public override FamilyGame Ensure()
        {
            switch (Id)
            {
                case "Q": Games.Q.Database.Ensure(this); return this;
                default: return this;
            }
        }
    }
}