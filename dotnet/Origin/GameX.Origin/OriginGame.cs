using System.Text.Json;

namespace GameX.Origin
{
    /// <summary>
    /// OriginGame
    /// </summary>
    /// <seealso cref="GameX.FamilyGame" />
    public class OriginGame : FamilyGame
    {
        public OriginGame(Family family, string id, JsonElement elem, FamilyGame dgame) : base(family, id, elem, dgame) { }

        /// <summary>
        /// Ensures this instance.
        /// </summary>
        /// <returns></returns>
        public override FamilyGame Ensure()
        {
            switch (Id)
            {
                case "U9": Games.U9.Database.Ensure(this); return this;
                //case "U8": Structs.UO.Database.Ensure(this); return this;
                default: return this;
            }
        }
    }
}