using System;
using System.IO;
using static OpenStack.Debug;

namespace GameX.Bethesda
{
    public static class DatabaseManager
    {
        // Retail Iteration versions
        const int ITERATION_CELL = 982;
        const int ITERATION_PORTAL = 2072;
        const int ITERATION_HIRES = 497;
        const int ITERATION_LANGUAGE = 994;
        static int count;
        internal static bool loaded;

        public static DatabaseCell Cell { get; private set; }

        internal static FamilyGame Ensure(FamilyGame game, bool loadCell = true)
        {
            if (loaded) return game;
            loaded = true;

            //try
            //{
            //    Cell = new DatabaseCell(estate.OpenPakFile(new Uri("game:/client_cell_1.dat#AC")));
            //    count = Cell.Source.Count;
            //    Log($"Successfully opened {Cell} file, containing {count} records, iteration {Cell.GetIteration()}");
            //    if (count != ExpectedCount) Log($"{count} count does not match expected end-of-retail version of {ExpectedCount}.");
            //}
            //catch (FileNotFoundException ex)
            //{
            //    Log($"An exception occured while attempting to open {Cell} file. This needs to be corrected in order for Landblocks to load.");
            //    Log($"Exception: {ex.Message}");
            //}

            return game;
        }
    }
}
