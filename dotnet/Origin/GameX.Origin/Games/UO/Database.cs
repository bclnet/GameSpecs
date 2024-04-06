using System;
using static OpenStack.Debug;

namespace GameX.Origin.Games.UO
{
    public static class Database
    {
        public static PakFile PakFile = FamilyManager.GetFamily("Origin").OpenPakFile(new Uri("game:/#UO"));

        //Games.UO.Database.PakFile?.LoadFileObject<Binary_StringTable>("Cliloc.enu").Result;
        //public static int ItemIDMask => ClientVersion.InstallationIsUopFormat ? 0xffff : 0x3fff;

        //internal static FamilyGame Ensure(FamilyGame game)
        //{
        //    if (PakFile != null) return game;
        //    try
        //    {
        //        PakFile = game.Family.OpenPakFile(new Uri("game:/#UO"));
        //        //PakFile.LoadFileObject<object>("Cliloc.enu").Wait();
        //        //Cliloc = Binary_StringTable.Records;
        //        Log($"Successfully opened {PakFile} file");
        //    }
        //    catch (Exception e)
        //    {
        //        Log($"An exception occured while attempting to open {PakFile} file. This needs to be corrected in order for Landblocks to load.");
        //        Log($"Exception: {e.Message}");
        //    }
        //    return game;
        //}
    }
}
