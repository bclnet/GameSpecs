﻿namespace GameSpec.Tes
{
    /// <summary>
    /// TesFamilyGame
    /// </summary>
    /// <seealso cref="GameSpec.FamilyGame" />
    public class TesFamilyGame : FamilyGame
    {
        /// <summary>
        /// Ensures this instance.
        /// </summary>
        /// <returns></returns>
        public override FamilyGame Ensure() => DatabaseManager.Ensure(this);
    }
}