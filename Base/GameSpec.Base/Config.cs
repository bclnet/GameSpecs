#define Arkane

namespace GameSpec
{
    public partial class FamilyManager
    {
#if AC
        static string[] FamilyKeys = new[] { "AC", "Unknown" };

        public static DefaultOptions AppDefaultOptions = new DefaultOptions
        {
            ForceOpen = true,
            Family = "AC",
            GameId = "AC", // Asheron's Call
        };
#elif Arkane
        static string[] FamilyKeys = new[] { "Arkane", "Unknown" };

        public static DefaultOptions AppDefaultOptions = new DefaultOptions
        {
            //ForcePath = "data.pak:GAME/GRAPH/INTERFACE/BOOK/RUNES/LACET.FTL",
            //ForcePath = "data.pak:GAME/GRAPH/OBJ3D/INTERACTIVE/NPC/RATMAN_BASE/RATMAN_BASE.FTL",
            ForcePath = "data.pak:GAME/GRAPH/Levels/Level1/fast.fts",
            //ForcePath = "data.pak:GAME/GRAPH/OBJ3D/INTERACTIVE/NPC/Y_MX/Y_MX.FTL",
            ForceOpen = true,
            Family = "Arkane",
            GameId = "AF", // Arx Fatalis
            //GameId = "DOM", // Dark Messiah of Might and Magic
            //GameId = "D", // Dishonored
            //GameId = "D2", // Dishonored 2
            //GameId = "P", // Prey
            //GameId = "D:DOTO", // Dishonored: Death of the Outsider
            //GameId = "W:YB", // Wolfenstein: Youngblood
            //GameId = "W:CP", // Wolfenstein: Cyberpilot
            //GameId = "DL", // Deathloop
            //Missing: GameId = "RF", // Redfall (future)
        };
#elif Bioware
        static string[] FamilyKeys = new[] { "Bioware", "Unknown" };

        public static DefaultOptions AppDefaultOptions = new DefaultOptions
        {
            ForceOpen = true,
            Family = "Bioware",
            //GameId = "SWTOR", // Star Wars: The Old Republic
            //ForcePath = "swtor_en-us_alliance_1.tor:resources/en-us/fxe/cnv/alliance/alderaan/lokin/lokin.fxe",
            //GameId = "NWN", // Neverwinter Nights
            //GameId = "NWN2", // Neverwinter Nights 2
            //GameId = "KotOR", // Star Wars: Knights of the Old Republic
        };
#elif Blizzard
        static string[] FamilyKeys = new[] { "Blizzard", "Unknown" };

        public static DefaultOptions AppDefaultOptions = new DefaultOptions
        {
            ForceOpen = true,
            Family = "Blizzard",
            //GameId = "SC", // StarCraft
            //GameId = "D2R", // Diablo II: Resurrected
            //Missing: GameId = "W3", // Warcraft III: Reign of Chaos
            //GameId = "WOW", // World of Warcraft
            //Missing: GameId = "WOWC", // World of Warcraft: Classic
            //GameId = "SC2", // StarCraft II: Wings of Liberty
            //GameId = "D3", // Diablo III
            //GameId = "HS", // Hearthstone
            //GameId = "HOTS", // Heroes of the Storm
            //GameId = "DI", // Diablo Immortal
            //GameId = "OW2", // Overwatch 2
            //Missing: GameId = "D4", // Diablo IV
        };
#elif Cry
        static string[] FamilyKeys = new[] { "Cry", "Unknown" };

        public static DefaultOptions AppDefaultOptions = new DefaultOptions
        {
            ForceOpen = true,
            Family = "Cry",
            GameId = "ArcheAge", // ArcheAge
            GameId = "Hunt", // Hunt: Showdown
            GameId = "MWO", // MechWarrior Online
            GameId = "Warface", // Warface
            GameId = "Wolcen", // Wolcen: Lords of Mayhem
            GameId = "Crysis", // Crysis Remastered
            GameId = "Ryse", // Ryse: Son of Rome
            GameId = "Robinson", // Robinson: The Journey
            GameId = "Snow", // SNOW - The Ultimate Edition
        };
#elif Cyanide
        static string[] FamilyKeys = new[] { "Cyanide", "Unknown" };

        public static DefaultOptions AppDefaultOptions = new DefaultOptions
        {
            ForceOpen = true,
            Family = "Cry",
            GameId = "Council", // Council
            GameId = "Werewolf:TA", // Werewolf: The Apocalypse - Earthblood
        };
#elif Hpl
        static string[] FamilyKeys = new[] { "Hpl", "Unknown" };

        public static DefaultOptions AppDefaultOptions = new DefaultOptions
        {
            ForceOpen = true,
            Family = "Hpl",
            GameId = "P:O", // Penumbra: Overture
            GameId = "P:BP", // Penumbra: Black Plague
            GameId = "P:R", // Penumbra: Requiem
            GameId = "A:TDD", // Amnesia: The Dark Descent
            GameId = "A:AMFP", // Amnesia: A Machine for Pigs
            GameId = "SOMA", // SOMA
            GameId = "A:R", // Amnesia: Rebirth
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
#elif Lith
        static string[] FamilyKeys = new[] { "Lith", "Unknown" };

        public static DefaultOptions AppDefaultOptions = new DefaultOptions
        {
            ForceOpen = true,
            Family = "Lith",
            GameId = "FEAR", // F.E.A.R.
            GameId = "FEAR:EP", // F.E.A.R.: Extraction Point
            GameId = "FEAR:PM", // F.E.A.R.: Perseus Mandate
            GameId = "FEAR2", // F.E.A.R. 2: Project Origin
            GameId = "FEAR3", // F.E.A.R. 3
        };
#else
        static string[] FamilyKeys = new[] { "AC", "Arkane", "Bioware", "Blizzard", "Cry", "Cyanide", "Hpl", "IW", "Lith", "Origin", "Red", "Rsi", "Tes", "Unity", "Unknown", "Unreal", "Valve" };

        public static DefaultOptions AppDefaultOptions = new DefaultOptions { };
#endif
    }
}
