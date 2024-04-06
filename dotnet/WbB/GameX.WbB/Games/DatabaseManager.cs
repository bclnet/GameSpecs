using System;
using System.IO;
using static OpenStack.Debug;

namespace GameX.WbB
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
        public static DatabasePortal Portal { get; private set; }
        public static Database HighRes { get; private set; }
        public static DatabaseLanguage Language { get; private set; }

#if true
        internal static FamilyGame Ensure(FamilyGame game, bool loadCell = true)
        {
            if (loaded) return game;
            loaded = true;

            var family = game.Family;
            if (loadCell)
                try
                {
                    Cell = new DatabaseCell(family.OpenPakFile(new Uri("game:/client_cell_1.dat#AC")));
                    count = Cell.Source.Count;
                    Log($"Successfully opened {Cell} file, containing {count} records, iteration {Cell.GetIteration()}");
                    if (Cell.GetIteration() != ITERATION_CELL) Log($"{Cell} iteration does not match expected end-of-retail version of {ITERATION_CELL}.");
                }
                catch (FileNotFoundException ex)
                {
                    Log($"An exception occured while attempting to open {Cell} file. This needs to be corrected in order for Landblocks to load.");
                    Log($"Exception: {ex.Message}");
                }

            try
            {
                Portal = new DatabasePortal(family.OpenPakFile(new Uri("game:/client_portal.dat#AC")));
                Portal.SkillTable.AddRetiredSkills();
                count = Portal.Source.Count;
                Log($"Successfully opened {Portal} file, containing {count} records, iteration {Portal.GetIteration()}");
                if (Portal.GetIteration() != ITERATION_PORTAL) Log($"{Portal} iteration does not match expected end-of-retail version of {ITERATION_PORTAL}.");
            }
            catch (FileNotFoundException ex)
            {
                Log($"An exception occured while attempting to open {Portal} file.");
                Log($"Exception: {ex.Message}");
            }

            // Load the client_highres.dat file. This is not required for ACE operation, so no exception needs to be generated.
            HighRes = new Database(family.OpenPakFile(new Uri("game:/client_highres.dat#AC")));
            count = HighRes.Source.Count;
            Log($"Successfully opened {HighRes} file, containing {count} records, iteration {HighRes.GetIteration()}");
            if (HighRes.GetIteration() != ITERATION_HIRES) Log($"{HighRes} iteration does not match expected end-of-retail version of {ITERATION_HIRES}.");

            try
            {
                Language = new DatabaseLanguage(family.OpenPakFile(new Uri("game:/client_local_English.dat#AC")));
                count = Language.Source.Count;
                Log($"Successfully opened {Language} file, containing {count} records, iteration {Language.GetIteration()}");
                if (Language.GetIteration() != ITERATION_LANGUAGE) Log($"{Language} iteration does not match expected end-of-retail version of {ITERATION_LANGUAGE}.");
            }
            catch (FileNotFoundException ex)
            {
                Log($"An exception occured while attempting to open {Language} file.");
                Log($"Exception: {ex.Message}");
            }

            return game;
        }
#endif
    }
}
