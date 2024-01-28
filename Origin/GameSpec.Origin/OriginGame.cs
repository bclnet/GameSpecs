using System.Text.Json;

namespace GameSpec.Origin
{
    /// <summary>
    /// OriginGame
    /// </summary>
    /// <seealso cref="GameSpec.FamilyGame" />
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
                //case "U8": Structs.UO.Database.Ensure(this); return this;
                default: return this;
            }
        }
    }
}