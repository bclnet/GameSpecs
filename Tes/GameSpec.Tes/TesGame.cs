namespace GameSpec.Tes
{
    /// <summary>
    /// TesGame
    /// </summary>
    /// <seealso cref="GameSpec.FamilyGame" />
    public class TesGame : FamilyGame
    {
        /// <summary>
        /// Ensures this instance.
        /// </summary>
        /// <returns></returns>
        public override FamilyGame Ensure() => DatabaseManager.Ensure(this);
    }
}