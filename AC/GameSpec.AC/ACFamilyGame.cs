namespace GameSpec.AC
{
    /// <summary>
    /// ACFamilyGame
    /// </summary>
    /// <seealso cref="GameSpec.FamilyGame" />
    public class ACFamilyGame : FamilyGame
    {
        /// <summary>
        /// Ensures this instance.
        /// </summary>
        /// <returns></returns>
        public override FamilyGame Ensure() => DatabaseManager.Ensure(this);
    }
}