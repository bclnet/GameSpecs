using System;
using static OpenStack.Debug;

namespace GameSpec.Origin.Games.UO
{
    public static class Database
    {
        static PakFile PakFile;

        internal static FamilyGame Ensure(FamilyGame game)
        {
            if (PakFile != null) return game;
            try
            {
                PakFile = game.Family.OpenPakFile(new Uri("game:/#UO"));
                Log($"Successfully opened {PakFile} file");
            }
            catch (Exception e)
            {
                Log($"An exception occured while attempting to open {PakFile} file. This needs to be corrected in order for Landblocks to load.");
                Log($"Exception: {e.Message}");
            }
            return game;
        }

        public static string GetString(int id)
            => "string";
    }
}
