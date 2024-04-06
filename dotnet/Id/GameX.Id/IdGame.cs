using System.Text.Json;

namespace GameX.Id
{
    /// <summary>
    /// IdGame
    /// </summary>
    /// <seealso cref="GameX.FamilyGame" />
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