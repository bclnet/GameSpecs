#define X
namespace GameSpec
{
    public partial class FamilyManager
    {
#if Blizzard
        static string[] FamilyKeys = new[] { "Blizzard", "Unknown" };

        public static DefaultOptions AppDefaultOptions = new DefaultOptions
        {
            ForceOpen = true,
            Family = "Blizzard",
            //GameId = "SC", // StarCraft
            //GameId = "D2R", // Diablo II: Resurrected
            //Missing: GameId = "W3", // Warcraft III: Reign of Chaos
            //Missing: GameId = "WOW", // World of Warcraft
            //Missing: GameId = "WOWC", // World of Warcraft: Classic
            //GameId = "SC2", // StarCraft II: Wings of Liberty
            GameId = "D3", // Diablo III
            //Missing: GameId = "HS", // Hearthstone
            //Missing: GameId = "HOTS", // Heroes of the Storm
            //Missing: GameId = "OW", // Overwatch
            //Missing: GameId = "DI", // Diablo Immortal
            //Missing: GameId = "OW2", // Overwatch 2
            //Missing: GameId = "D4", // Diablo IV
        };
#elif IW
        static string[] FamilyKeys = new[] { "IW", "Unknown" };

        /* Sample: Data
       * OK - Family = "AC", GameId = "AC", ForcePath = "TabooTable/0E00001E.taboo", ForceOpen = true,
       */

        /* Sample: Texture
         * BAD - Family = "AC", GameId = "AC", ForcePath = "Texture060043BE", ForceOpen = true,
         * BAD - Family = "Cry", GameId = "Hunt", ForcePath = "Data/Textures/asteroids/asteroid_dmg_brown_organic_01_ddn.dds", ForceOpen = true,
         * BAD - Family = "Rsi", GameId = "StarCitizen", ForcePath = "Data/Textures/references/color.dds", ForceOpen = true,
         * BAD - Family = "Rsi", GameId = "StarCitizen", ForcePath = "Data/Textures/asteroids/asteroid_dmg_brown_organic_01_ddn.dds", ForceOpen = true,
         * BAD - Family = "Tes", GameId = "Morrowind", ForcePath = "Data/Textures/asteroids/asteroid_dmg_brown_organic_01_ddn.dds", ForceOpen = true,
         * BAD - Family = "Tes", GameId = "StarCitizen", ForcePath = "Data/Textures/asteroids/asteroid_dmg_brown_organic_01_ddn.dds", ForceOpen = true,
         * OK - Family = "Valve", GameId = "Dota2", ForcePath = "materials/console_background_color_psd_b9e26a4.vtex_c", ForceOpen = true,
         */
        public static DefaultOptions AppDefaultOptions = new DefaultOptions
        {
            ForceOpen = true,
            Family = "IW",
            //GameId = "COD2", // Call of Duty 2 - IWD //ForcePath = "iw_08.iwd/images/155_cannon.iwi",
            //GameId = "COD3", // Call of Duty 3 - XBOX only
            //GameId = "COD4", // Call of Duty 4: Modern Warfare - IWD, FF //ForcePath = "mp_farm.ff/images/155_cannon.iwi",
            //GameId = "COD:WaW", // Call of Duty: World at War - IWD, FF
            //GameId = "MW2", // Call of Duty: Modern Warfare 2
            //GameId = "COD:BO", // Call of Duty: Black Ops - IWD, FF
            //GameId = "MW3", // Call of Duty: Call of Duty: Modern Warfare 3
            //GameId = "COD:BO2", // Call of Duty: Black Ops 2 - FF
            //GameId = "COD:AW", // Call of Duty: Advanced Warfare
            //GameId = "COD:BO3", // Call of Duty: Black Ops III - XPAC,FF
            //GameId = "MW3", // Call of Duty: Modern Warfare 3
            //GameId = "WWII", // Call of Duty: WWII

            GameId = "BO4", // Call of Duty Black Ops 4
            //GameId = "BOCW", // Call of Duty Black Ops Cold War
            //GameId = "Vanguard", // Call of Duty Vanguard
        };
#else
        static string[] FamilyKeys = new[] { "AC", "Arkane", "Aurora", "Blizzard", "Cry", "Cyanide", "Hpl", "IW", "Lith", "Origin", "Red", "Rsi", "Tes", "Unity", "Unknown", "Unreal", "Valve" };

        public static DefaultOptions AppDefaultOptions = new DefaultOptions { };
#endif
    }
}
