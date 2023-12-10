//#define Arkane
#define Bethesda
//#define Bioware
//#define Black
//#define Blizzard
//#define Capcom
//#define Cig
//#define Cryptic
//#define Crytek
//#define Cyanide
//#define Epic
//#define Frictional
//#define Frontier
//#define Id
//#define IW
//#define Monolith
//#define Origin
//#define Red
//#define Unreal
//#define Valve
//#define WbB

namespace GameSpec
{
    public partial class FamilyManager
    {
#if Arkane
        static string[] FamilyKeys = new[] { "Arkane", "Unknown" };

        public static DefaultOptions AppDefaultOptions = new DefaultOptions
        {
            //AF
            //ForcePath = "data.pak:GRAPH/particles/DEFAULT.jpg", //AF:Texture.jpg
            //ForcePath = "data.pak:GAME/GRAPH/Levels/Level10/loading.bmp", //AF:Texture.bmp
            //ForcePath = "data.pak:GAME/GRAPH/INTERFACE/BOOK/RUNES/LACET.FTL", //AF:Model
            //ForcePath = "data.pak:GAME/GRAPH/OBJ3D/INTERACTIVE/NPC/RATMAN_BASE/RATMAN_BASE.FTL", //AF:Model
            //ForcePath = "data.pak:GAME/GRAPH/Levels/Level10/fast.fts", //AF:Level
            //D2
            //ForcePath = "", //D2:

            ForceOpen = true,
            Family = "Arkane",
            //GameId = "AF", // Arx Fatalis [open, read, texture:GL]
            //GameId = "DOM", // Dark Messiah of Might and Magic [open, read]
            //GameId = "D", // Dishonored [unreal]
            GameId = "D2", // Dishonored 2 [open, read]
            //GameId = "P", // Prey [open, read]
            //GameId = "D:DOTO", // Dishonored: Death of the Outsider
            //GameId = "W:YB", // Wolfenstein: Youngblood
            //GameId = "W:CP", // Wolfenstein: Cyberpilot
            //GameId = "DL", // Deathloop
            //Missing: GameId = "RF", // Redfall (future)
        };
#elif Bethesda
        static string[] FamilyKeys = new[] { "Bethesda", "Unknown" };

        public static DefaultOptions AppDefaultOptions = new DefaultOptions
        {
            // Game:Morrowind
            //ForcePath = "bookart/boethiah_256.dds",
            //ForcePath = "icons/handtohand.dds",

            //ForceOpen = true,
            Family = "Bethesda",

            GameId = "Morrowind", // The Elder Scrolls III: Morrowind
            //GameId = "Oblivion", // The Elder Scrolls IV: Oblivion
            //GameId = "Fallout3", // Fallout 3
            //GameId = "FalloutNV", // Fallout New Vegas
            //GameId = "Skyrim", // The Elder Scrolls V: Skyrim
            //GameId = "Fallout4", // Fallout 4
            //GameId = "SkyrimSE", // The Elder Scrolls V: Skyrim – Special Edition
            //GameId = "Fallout:S", // Fallout Shelter
            //GameId = "Fallout4VR", // Fallout 4 VR
            //GameId = "SkyrimVR", // The Elder Scrolls V: Skyrim VR
            //GameId = "Fallout76", // Fallout 76
            //GameId = "Starfield", // Starfield (future)
            //GameId = "Unknown1", // The Elder Scrolls VI (future)
            //GameId = "Fallout5", // Fallout 5 (future)
        };
#elif Bioware
        static string[] FamilyKeys = new[] { "Bioware", "Unknown" };

        public static DefaultOptions AppDefaultOptions = new DefaultOptions
        {
            //ForcePath = "swtor_en-us_alliance_1.tor:resources/en-us/fxe/cnv/alliance/alderaan/lokin/lokin.fxe", SWTOR:Unknown

            ForceOpen = true,
            Family = "Bioware",
            //GameId = "SWTOR", // Star Wars: The Old Republic
            //GameId = "NWN", // Neverwinter Nights
            //GameId = "NWN2", // Neverwinter Nights 2
            //GameId = "KotOR", // Star Wars: Knights of the Old Republic
        };
#elif Black
        static string[] FamilyKeys = new[] { "Black", "Unknown" };

        public static DefaultOptions AppDefaultOptions = new DefaultOptions
        {
            // Game:Fallout
            //ForcePath = "CRITTER.DAT:ART/CRITTERS/CRITTERS.LST",
            //ForcePath = "MASTER.DAT:ART/BACKGRND/BACK1.FRM",
            //ForcePath = "MASTER.DAT:ART/ITEMS/ALIEN1.FRM",
            //ForcePath = "MASTER.DAT:COLOR.PAL",
            // Game:Fallout2
            //ForcePath = "master.dat:art/backgrnd/BACK1.FRM",
            //ForcePath = "master.dat:art/splash/SPLASH0.rix",
            //ForcePath = "master.dat:art/intrface/death.frm",
            //ForcePath = "master.dat:art/intrface/DP.FRM",

            ForceOpen = true,
            Family = "Black",
            //GameId = "Fallout", // Fallout
            //GameId = "Fallout2", // Fallout 2
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
#elif Capcom
        static string[] FamilyKeys = new[] { "Capcom", "Unknown" };

        public static DefaultOptions AppDefaultOptions = new DefaultOptions
        {
            ForceOpen = false,
            Family = "Capcom",
            GameId = "XX", // XX
        };
#elif Cig
        static string[] FamilyKeys = new[] { "Cig", "Unknown" };

        public static DefaultOptions AppDefaultOptions = new DefaultOptions
        {
            ForcePath = "app:DataForge",
            //ForcePath = "app:StarWords",
            //ForcePath = "app:Subsumption",
            //ForcePath = "Data/dedicated.cfg",
            //ForcePath = "Data/Game.dcb", //StarCitizen:Dataforge
            //ForcePath = "Data/Textures/bubble_ddna.dds.a", //StarCitizen:Texture
            //ForcePath = "Data/Textures/references/color.dds", //StarCitizen:Texture
            //ForcePath = "Data/Textures/asteroids/asteroid_dmg_brown_organic_01_ddn.dds", //StarCitizen:Texture

            ForceOpen = true,
            Family = "Cig",
            GameId = "StarCitizen", // Star Citizen
        };
#elif Cryptic
        static string[] FamilyKeys = new[] { "Cryptic", "Unknown" };

        public static DefaultOptions AppDefaultOptions = new DefaultOptions
        {
            //ForcePath = "Data/Textures/asteroids/asteroid_dmg_brown_organic_01_ddn.dds", Hunt:Texture
            
            ForceOpen = true,
            Family = "Cryptic",
            GameId = "CO", // Champions Online [open, read]
            //GameId = "STO", // Star Trek Online [open, read]
            //GameId = "NVW", // Neverwinter [open, read]
        };
#elif Crytek
        static string[] FamilyKeys = new[] { "Crytek", "Unknown" };

        public static DefaultOptions AppDefaultOptions = new DefaultOptions
        {
            //ForcePath = "Data/Textures/asteroids/asteroid_dmg_brown_organic_01_ddn.dds", Hunt:Texture
            
            ForceOpen = true,
            Family = "Crytek",
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
            Family = "Cyanide",
            GameId = "Council", // Council
            GameId = "Werewolf:TA", // Werewolf: The Apocalypse - Earthblood
        };
#elif Epic
        static string[] FamilyKeys = new[] { "Epic", "Unknown" };

        public static DefaultOptions AppDefaultOptions = new DefaultOptions
        {
            //UE1
            ForcePath = "Maps/Bluff.unr", //Map

            ForceOpen = true,
            Family = "Epic",
            GameId = "UE1", // Unreal

            //GameId = "BioShock", // BioShock
            //GameId = "BioShockR", // BioShock Remastered
            //GameId = "BioShock2", // BioShock 2
            //GameId = "BioShock2R", // BioShock 2 Remastered
            //GameId = "BioShock:Inf", // BioShock Infinite
        };
#elif Frictional
        static string[] FamilyKeys = new[] { "Frictional", "Unknown" };

        public static DefaultOptions AppDefaultOptions = new DefaultOptions
        {
            ForceOpen = true,
            Family = "Frictional",
            GameId = "P:O", // Penumbra: Overture
            GameId = "P:BP", // Penumbra: Black Plague
            GameId = "P:R", // Penumbra: Requiem
            GameId = "A:TDD", // Amnesia: The Dark Descent
            GameId = "A:AMFP", // Amnesia: A Machine for Pigs
            GameId = "SOMA", // SOMA
            GameId = "A:R", // Amnesia: Rebirth
        };
#elif Frontier
        static string[] FamilyKeys = new[] { "Frontier", "Unknown" };

        public static DefaultOptions AppDefaultOptions = new DefaultOptions
        {
            ForceOpen = false,
            Family = "Frontier",
            GameId = "ED"
        };
#elif Id
        static string[] FamilyKeys = new[] { "Id", "Unknown" };

        public static DefaultOptions AppDefaultOptions = new DefaultOptions
        {
            ForceOpen = true,
            Family = "Id",
            //GameId = "Q", // Quake
            //GameId = "Q2", // Quake II
            //GameId = "Q3:A", // Quake III Arena
            //GameId = "D3", // Doom 3
            //GameId = "Q:L", // Quake Live
            //GameId = "R", // Rage
            //GameId = "D", // Doom
            //GameId = "D:VFR", // Doom VFR
            //GameId = "R2", // Rage 2
            //GameId = "D:E", // Doom Eternal
            //GameId = "Q:C", // Quake Champions
        };
#elif IW
        static string[] FamilyKeys = new[] { "IW", "Unknown" };

        public static DefaultOptions AppDefaultOptions = new DefaultOptions
        {
            //ForcePath = "iw_08.iwd/images/155_cannon.iwi", //COD2:Texture
            //ForcePath = "mp_farm.ff/images/155_cannon.iwi", //COD4:Texture

            ForceOpen = true,
            Family = "IW",
            //GameId = "COD2", // Call of Duty 2 - IWD 
            //GameId = "COD3", // Call of Duty 3 - XBOX only
            //GameId = "COD4", // Call of Duty 4: Modern Warfare - IWD, FF
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
#elif Monolith
        static string[] FamilyKeys = new[] { "Monolith", "Unknown" };

        public static DefaultOptions AppDefaultOptions = new DefaultOptions
        {
            ForceOpen = true,
            Family = "Monolith",
            //GameId = "FEAR", // F.E.A.R.
            //GameId = "FEAR:EP", // F.E.A.R.: Extraction Point
            //GameId = "FEAR:PM", // F.E.A.R.: Perseus Mandate
            //GameId = "FEAR2", // F.E.A.R. 2: Project Origin
            //GameId = "FEAR3", // F.E.A.R. 3
        };
#elif Origin
        static string[] FamilyKeys = new[] { "Origin", "Unknown" };

        public static DefaultOptions AppDefaultOptions = new DefaultOptions
        {
            ForceOpen = true,
            Family = "Origin",
            //GameId = "UO", // Ultima Online
            //GameId = "UltimaIX", // Ultima IX
        };
#elif Red
        static string[] FamilyKeys = new[] { "Red", "Unknown" };

        public static DefaultOptions AppDefaultOptions = new DefaultOptions
        {
            ForceOpen = true,
            Family = "Red",
            //GameId = "Witcher", // The Witcher Enhanced Edition
            //GameId = "Witcher2", // The Witcher 2
            //GameId = "Witcher3", // The Witcher 3: Wild Hunt
            //GameId = "CP77", // Cyberpunk 2077
            //GameId = "Witcher4", // The Witcher 4 Polaris (future)
        };
#elif Unity
        static string[] FamilyKeys = new[] { "Unity", "Unknown" };

        public static DefaultOptions AppDefaultOptions = new DefaultOptions
        {
            ForceOpen = true,
            Family = "Unity",
            //GameId = "AmongUs", // Among Us
            //GameId = "Cities", // Cities: Skylines
            //GameId = "Tabletop", // Tabletop Simulator
            //GameId = "UBoat", // Destroyer: The U-Boat Hunter
            //GameId = "7D2D", // 7 Days to Die
        };
#elif Valve
        static string[] FamilyKeys = new[] { "Valve", "Unknown" };

        public static DefaultOptions AppDefaultOptions = new DefaultOptions
        {
            //HL
            ForcePath = "valve/maps/boot_camp.bsp", //Map
            //ForcePath = "valve/cached.wad:LOADING.pic", //Texture
            //ForcePath = "valve/decals.wad:REFLECT1.tex", //Texture
            //ForcePath = "valve/decals.wad:{LARGE#S0.tex", //Texture
            //ForcePath = "valve/fonts.wad:FONT2.fnt", //Texture
            //ForcePath = "valve/sprites:640_logo.spr", //Sprite
            //TF
            //ForcePath = "game.tga", //Image
            //ForcePath = "cached.wad:CONBACK640.pic", //Texture
            //ForcePath = "tfc.WAD:{EASTLINE1.pic", //Texture
            //HL2
            //ForcePath = "pak01_dir.vpk:textures/dev/albedo_chart.vtex_c", //Texture
            //CS:GO
            //ForcePath = "pak01_dir.vpk:textures/dev/albedo_chart.vtex_c", //Texture
            //ForcePath = "pak01_dir.vpk:models/dev/materialforerrormodel.vmat_c", //Material
            //ForcePath = "pak01_dir.vpk:models/dev/error.vmesh_c", //Mesh
            //ForcePath = "pak01_dir.vpk:models/dev/error.vphy_c", //Phy
            //ForcePath = "pak01_dir.vpk:models/dev/error.vmdl_c", //Model
            //Dota2
            //ForcePath = "pak01_dir.vpk:textures/dev/albedo_chart.vtex_c", //Texture
            //ForcePath = "pak01_dir.vpk:models/dev/materialforerrormodel.vmat_c", //Material
            //ForcePath = "pak01_dir.vpk:models/dev/error.vmesh_c", //Mesh
            //ForcePath = "pak01_dir.vpk:models/dev/error.vphy_c", //Phy
            //ForcePath = "pak01_dir.vpk:models/dev/error.vmdl_c", //Model
            //TheLab:RR
            //ForcePath = "pak01_dir.vpk:textures/dev/albedo_chart.vtex_c", //Texture
            //ForcePath = "pak01_dir.vpk:models/dev/materialforerrormodel.vmat_c", //Material
            //ForcePath = "pak01_dir.vpk:models/dev/error.vmesh_c", //Mesh
            //ForcePath = "pak01_dir.vpk:models/dev/error.vphy_c", //Phy
            //ForcePath = "pak01_dir.vpk:models/dev/error.vmdl_c", //Model
            //HL:Alyx
            //ForcePath = "pak01_dir.vpk:textures/dev/albedo_chart.vtex_c", //Texture
            //ForcePath = "pak01_dir.vpk:models/dev/materialforerrormodel.vmat_c", //Material
            //ForcePath = "pak01_dir.vpk:models/dev/error.vmesh_c", //Mesh
            //ForcePath = "pak01_dir.vpk:models/dev/error.vphy_c", //Phy
            //ForcePath = "pak01_dir.vpk:models/dev/error.vmdl_c", //Model

            ForceOpen = true,
            Family = "Valve",
            GameId = "HL", // Half-Life [open, read, texture:GL]
            //GameId = "TF", // Team Fortress Classic [open, read, texture:GL]
            //GameId = "CS", // Counter-Strike [open, read]
            //GameId = "Ricochet", // Ricochet [open, read]
            //GameId = "HL:BS", // Half-Life: Blue Shift [open, read]
            //GameId = "DOD", // Day of Defeat [open, read]
            //GameId = "CS:CZ", // Counter-Strike: Condition Zero [open, read]
            //GameId = "HL:Src", // Half-Life: Source [open, read]
            //GameId = "CS:Src", // Counter-Strike: Source [open, read]
            //GameId = "HL2", // Half-Life 2 [open, read]
            //GameId = "HL2:DM", // Half-Life 2: Deathmatch [open, read]
            //GameId = "HL:DM:Src", // Half-Life Deathmatch: Source [open, read]
            //GameId = "HL2:E1", // Half-Life 2: Episode One [open, read]
            //GameId = "Portal", // Portal [open, read]
            //GameId = "HL2:E2", // Half-Life 2: Episode Two [open]
            //GameId = "TF2", // Team Fortress 2 [open, read]
            //GameId = "L4D", // Left 4 Dead [open, read]
            //GameId = "L4D2", // Left 4 Dead 2 [open, read]
            //GameId = "DOD:Src", // Day of Defeat: Source [open, read]
            //GameId = "Portal2", // Portal 2 [open, read]
            //GameId = "CS:GO", // Counter-Strike: Global Offensive [open, read]
            //GameId = "D2", // Dota 2 [open, read, texture:GL, model:GL]
            //GameId = "TheLab:RR", // The Lab: Robot Repair [open, read, texture:GL, model:GL]
            //GameId = "TheLab:SS", // The Lab: Secret Shop [!unity]
            //GameId = "TheLab:TL", // The Lab: The Lab [!unity]
            //GameId = "HL:Alyx", // Half-Life: Alyx [open, read, texture:GL, model:GL]
        };
#elif WbB
        static string[] FamilyKeys = new[] { "WbB", "Unknown" };

        public static DefaultOptions AppDefaultOptions = new DefaultOptions
        {
            //ForcePath = "TabooTable/0E00001E.taboo", // Ac:Data
            //ForcePath = "Texture/06000133.tex", // AC:Texture.R8G8B8
            //ForcePath = "Texture/06000FAA.tex", // AC:Texture.A8R8G8B8
            //ForcePath = "Texture/06007529.tex", // AC:Texture.INDEX16
            //ForcePath = "Texture/06007575.tex", // AC:Texture.DXT1
            //ForcePath = "Texture/06007576.tex", // AC:Texture.JPG
            //ForcePath = "Texture/0600127D.tex", // AC:Texture.R8G8B8
            //ForcePath = "Texture/06001343.tex", // AC:Texture.R8G8B8
            ForcePath = "Texture/06007529.tex", // AC:Texture.PAL

            ForceOpen = true,
            Family = "WbB",
            GameId = "AC", // Asheron's Call [open, read, texture:GL]
        };
#else
        static string[] FamilyKeys = new[] { "Arkane", "Bethesda", "Bioware", "Black", "Blizzard", "Capcom", "Cig", "Cryptic", "Crytek", "Cyanide", "Epic", "Frictional", "Frontier", "Id", "IW", "Monolith", "Origin", "Red", "Unity", "Unknown", "Valve", "WbB" };

        public static DefaultOptions AppDefaultOptions = new DefaultOptions
        {
        };
#endif
    }
}
