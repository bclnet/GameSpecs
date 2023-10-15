namespace GameSpec.AC
{
    /// <summary>
    /// ACGame
    /// </summary>
    /// <seealso cref="GameSpec.FamilyGame" />
    public class ACGame : FamilyGame
    {
        /// <summary>
        /// Ensures this instance.
        /// </summary>
        /// <returns></returns>
        public override FamilyGame Ensure() => DatabaseManager.Ensure(this);
    }
}