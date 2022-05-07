namespace GameSpec.AC
{
    /// <summary>
    /// ACFamily
    /// </summary>
    /// <seealso cref="GameSpec.Family" />
    public class ACFamily : Family
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ACEstate"/> class.
        /// </summary>
        public ACFamily() : base() { }

        /// <summary>
        /// Ensures this instance.
        /// </summary>
        /// <returns></returns>
        public override Family Ensure() => DatabaseManager.Ensure(this);
    }
}