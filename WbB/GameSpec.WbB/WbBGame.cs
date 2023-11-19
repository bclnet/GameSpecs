namespace GameSpec.WbB
{
    /// <summary>
    /// WbBGame
    /// </summary>
    /// <seealso cref="GameSpec.FamilyGame" />
    public class WbBGame : FamilyGame
    {
        /// <summary>
        /// Ensures this instance.
        /// </summary>
        /// <returns></returns>
        public override FamilyGame Ensure() => DatabaseManager.Ensure(this);
    }
}