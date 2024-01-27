using GameSpec.Origin.Formats.UO;
using System;
using System.Collections;
using static OpenStack.Debug;

namespace GameSpec.Origin.Games.UO
{
    public static class Database
    {
        static PakFile PakFile;
        static Hashtable Cliloc;
        public static int ItemIDMask => ClientVersion.InstallationIsUopFormat ? 0xffff : 0x3fff;

        internal static FamilyGame Ensure(FamilyGame game)
        {
            if (PakFile != null) return game;
            try
            {
                PakFile = game.Family.OpenPakFile(new Uri("game:/#UO"));
                PakFile.LoadFileObject<object>("Cliloc.enu").Wait();
                Cliloc = Binary_StringTable.Records;
                Log($"Successfully opened {PakFile} file");
            }
            catch (Exception e)
            {
                Log($"An exception occured while attempting to open {PakFile} file. This needs to be corrected in order for Landblocks to load.");
                Log($"Exception: {e.Message}");
            }
            return game;
        }

        public static string GetString(int id) => (string)Cliloc[id];
    }
}
