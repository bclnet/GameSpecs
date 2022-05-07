namespace GameSpec.Tes
{
    /// <summary>
    /// TesFamily
    /// </summary>
    /// <seealso cref="GameSpec.Family" />
    public class TesFamily : Family
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TesFamily"/> class.
        /// </summary>
        public TesFamily() : base() { }

        /// <summary>
        /// Ensures this instance.
        /// </summary>
        /// <returns></returns>
        public override Family Ensure() => DatabaseManager.Ensure(this);
    }
}