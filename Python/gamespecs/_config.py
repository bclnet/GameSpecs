__title__ = "gamespecs"
__version__ = "0.0.1"

class DefaultOptions:
    def __init__(self, Family:str=None, Game:str=None, Edition:str=None, ForcePath:str=None, ForceOpen:bool=False):
        self.Family = Family
        self.Game = Game
        self.Edition = Edition
        self.ForcePath = ForcePath
        self.ForceOpen = ForceOpen

match 'Bethesda':
    case 'Arkane':
        familyKeys = [ "Arkane", "Unknown" ]

        appDefaultOptions = DefaultOptions(
            #AF
            # ForcePath = "data.pak:GRAPH/particles/DEFAULT.jpg", #AF:Texture.jpg
            # ForcePath = "data.pak:GAME/GRAPH/Levels/Level10/loading.bmp", #AF:Texture.bmp
            # ForcePath = "data.pak:GAME/GRAPH/INTERFACE/BOOK/RUNES/LACET.FTL", #AF:Model
            # ForcePath = "data.pak:GAME/GRAPH/OBJ3D/INTERACTIVE/NPC/RATMAN_BASE/RATMAN_BASE.FTL", #AF:Model
            # ForcePath = "data.pak:GAME/GRAPH/Levels/Level10/fast.fts", #AF:Level
            #D2
            # ForcePath = "", #D2:

            ForceOpen = True,
            Family = "Arkane",
            Game = "AF", # Arx Fatalis [open, read, texture:GL]
            # Game = "DOM", # Dark Messiah of Might and Magic [open, read]
            # Game = "D", # Dishonored [unreal]
            # Game = "D2", # Dishonored 2 [open, read]
            # Game = "P", # Prey [open, read]
            # Game = "D:DOTO", # Dishonored: Death of the Outsider
            # Game = "W:YB", # Wolfenstein: Youngblood
            # Game = "W:CP", # Wolfenstein: Cyberpilot
            # Game = "DL", # Deathloop
            #Missing: Game = "RF", # Redfall (future)
        )
    case 'Bethesda':
        familyKeys = [ "Bethesda", "Unknown" ]

        appDefaultOptions = DefaultOptions(
            # Game:Morrowind
            ForcePath = "bookart/boethiah_256.dds",
            # ForcePath = "icons/handtohand.dds",

            ForceOpen = True,
            Family = "Bethesda",

            Game = "Morrowind", # The Elder Scrolls III: Morrowind
            # Game = "Oblivion", # The Elder Scrolls IV: Oblivion
            # Game = "Fallout3", # Fallout 3
            # Game = "FalloutNV", # Fallout New Vegas
            # Game = "Skyrim", # The Elder Scrolls V: Skyrim
            # Game = "Fallout4", # Fallout 4
            # Game = "SkyrimSE", # The Elder Scrolls V: Skyrim – Special Edition
            # Game = "Fallout:S", # Fallout Shelter
            # Game = "Fallout4VR", # Fallout 4 VR
            # Game = "SkyrimVR", # The Elder Scrolls V: Skyrim VR
            # Game = "Fallout76", # Fallout 76
            # Game = "Starfield", # Starfield (future)
        )
    case 'Bioware':
        familyKeys = [ "Bioware", "Unknown" ]

        appDefaultOptions = DefaultOptions(
            # ForcePath = "swtor_en-us_alliance_1.tor:resources/en-us/fxe/cnv/alliance/alderaan/lokin/lokin.fxe", SWTOR:Unknown

            ForceOpen = True,
            Family = "Bioware",
            # Game = "SWTOR", # Star Wars: The Old Republic
            # Game = "NWN", # Neverwinter Nights
            # Game = "NWN2", # Neverwinter Nights 2
            # Game = "KotOR", # Star Wars: Knights of the Old Republic
        )
    case 'Black':
        familyKeys = [ "Black", "Unknown" ]

        appDefaultOptions = DefaultOptions(
            # Game:Fallout
            # ForcePath = "CRITTER.DAT:ART/CRITTERS/CRITTERS.LST",
            # ForcePath = "MASTER.DAT:ART/BACKGRND/BACK1.FRM",
            # ForcePath = "MASTER.DAT:ART/ITEMS/ALIEN1.FRM",
            # ForcePath = "MASTER.DAT:COLOR.PAL",
            # Game:Fallout2
            # ForcePath = "master.dat:art/backgrnd/BACK1.FRM",
            # ForcePath = "master.dat:art/splash/SPLASH0.rix",
            # ForcePath = "master.dat:art/intrface/death.frm",
            # ForcePath = "master.dat:art/intrface/DP.FRM",

            ForceOpen = True,
            Family = "Black",
            # Game = "Fallout", # Fallout
            # Game = "Fallout2", # Fallout 2
        )
    case 'Blizzard':
        familyKeys = [ "Blizzard", "Unknown" ]

        appDefaultOptions = DefaultOptions(
            ForceOpen = True,
            Family = "Blizzard",
            # Game = "SC", # StarCraft
            # Game = "D2R", # Diablo II: Resurrected
            #Missing: Game = "W3", # Warcraft III: Reign of Chaos
            # Game = "WOW", # World of Warcraft
            #Missing: Game = "WOWC", # World of Warcraft: Classic
            # Game = "SC2", # StarCraft II: Wings of Liberty
            # Game = "D3", # Diablo III
            # Game = "HS", # Hearthstone
            # Game = "HOTS", # Heroes of the Storm
            # Game = "DI", # Diablo Immortal
            # Game = "OW2", # Overwatch 2
            #Missing: Game = "D4", # Diablo IV
        )
    case 'Capcom':
        familyKeys = [ "Capcom", "Unknown" ]

        appDefaultOptions = DefaultOptions(
            ForceOpen = True,
            Family = "Capcom",
            Game = "CAS", # [Kpka] Capcom Arcade Stadium
        )
    case 'Cig':
        familyKeys = [ "Cig", "Unknown" ]

        appDefaultOptions = DefaultOptions(
            ForcePath = "app:DataForge",
            # ForcePath = "app:StarWords",
            # ForcePath = "app:Subsumption",
            # ForcePath = "Data/dedicated.cfg",
            # ForcePath = "Data/Game.dcb", #StarCitizen:Dataforge
            # ForcePath = "Data/Textures/bubble_ddna.dds.a", #StarCitizen:Texture
            # ForcePath = "Data/Textures/references/color.dds", #StarCitizen:Texture
            # ForcePath = "Data/Textures/asteroids/asteroid_dmg_brown_organic_01_ddn.dds", #StarCitizen:Texture

            ForceOpen = True,
            Family = "Cig",
            Game = "StarCitizen", # Star Citizen
        )
    case 'Cryptic':
        familyKeys = [ "Cryptic", "Unknown" ]

        appDefaultOptions = DefaultOptions(
            # ForcePath = "Data/Textures/asteroids/asteroid_dmg_brown_organic_01_ddn.dds", Hunt:Texture
            
            ForceOpen = True,
            Family = "Cryptic",
            Game = "CO", # Champions Online [open, read]
            # Game = "STO", # Star Trek Online [open, read]
            # Game = "NVW", # Neverwinter [open, read]
        )
    case 'Crytek':
        familyKeys = [ "Crytek", "Unknown" ]

        appDefaultOptions = DefaultOptions(
            # ForcePath = "Data/Textures/asteroids/asteroid_dmg_brown_organic_01_ddn.dds", Hunt:Texture
            
            ForceOpen = True,
            Family = "Crytek",
            # Game = "ArcheAge", # ArcheAge
            # Game = "Hunt", # Hunt: Showdown
            # Game = "MWO", # MechWarrior Online
            # Game = "Warface", # Warface
            # Game = "Wolcen", # Wolcen: Lords of Mayhem
            # Game = "Crysis", # Crysis Remastered
            # Game = "Ryse", # Ryse: Son of Rome
            # Game = "Robinson", # Robinson: The Journey
            # Game = "Snow", # SNOW - The Ultimate Edition
        )
    case 'Cyanide':
        familyKeys = [ "Cyanide", "Unknown" ]

        appDefaultOptions = DefaultOptions(
            ForceOpen = True,
            Family = "Cyanide",
            # Game = "Council", # Council
            # Game = "Werewolf:TA", # Werewolf: The Apocalypse - Earthblood
        )
    case 'Epic':
        familyKeys = [ "Epic", "Unknown" ]

        appDefaultOptions = DefaultOptions(
            #UE1
            ForcePath = "Maps/Bluff.unr", #Map

            ForceOpen = True,
            Family = "Epic",
            Game = "UE1", # Unreal

            # Game = "BioShock", # BioShock
            # Game = "BioShockR", # BioShock Remastered
            # Game = "BioShock2", # BioShock 2
            # Game = "BioShock2R", # BioShock 2 Remastered
            # Game = "BioShock:Inf", # BioShock Infinite
        )
    case 'Frictional':
        familyKeys = [ "Frictional", "Unknown" ]

        appDefaultOptions = DefaultOptions(
            ForceOpen = True,
            Family = "Frictional",
            # Game = "P:O", # Penumbra: Overture
            # Game = "P:BP", # Penumbra: Black Plague
            # Game = "P:R", # Penumbra: Requiem
            # Game = "A:TDD", # Amnesia: The Dark Descent
            # Game = "A:AMFP", # Amnesia: A Machine for Pigs
            # Game = "SOMA", # SOMA
            # Game = "A:R", # Amnesia: Rebirth
        )
    case 'Frontier':
        familyKeys = [ "Frontier", "Unknown" ]

        appDefaultOptions = DefaultOptions(
            ForceOpen = True,
            Family = "Frontier",
            Game = "ED"
        )
    case 'Id':
        familyKeys = [ "Id", "Unknown" ]

        appDefaultOptions = DefaultOptions(
            # ForceOpen = True,
            Family = "Id",
            Game = "Q", # Quake
            # Game = "Q2", # Quake II
            # Game = "Q3:A", # Quake III Arena
            # Game = "D3", # Doom 3
            # Game = "Q:L", # Quake Live
            # Game = "R", # Rage
            # Game = "D", # Doom
            # Game = "D:VFR", # Doom VFR
            # Game = "R2", # Rage 2
            # Game = "D:E", # Doom Eternal
            # Game = "Q:C", # Quake Champions
        )
    case 'IW':
        familyKeys = [ "IW", "Unknown" ]

        appDefaultOptions = DefaultOptions(
            # ForcePath = "iw_08.iwd/images/155_cannon.iwi", #COD2:Texture
            # ForcePath = "mp_farm.ff/images/155_cannon.iwi", #COD4:Texture

            ForceOpen = True,
            Family = "IW",
            # Game = "COD2", # Call of Duty 2 - IWD 
            # Game = "COD3", # Call of Duty 3 - XBOX only
            # Game = "COD4", # Call of Duty 4: Modern Warfare - IWD, FF
            # Game = "COD:WaW", # Call of Duty: World at War - IWD, FF
            # Game = "MW2", # Call of Duty: Modern Warfare 2
            # Game = "COD:BO", # Call of Duty: Black Ops - IWD, FF
            # Game = "MW3", # Call of Duty: Call of Duty: Modern Warfare 3
            # Game = "COD:BO2", # Call of Duty: Black Ops 2 - FF
            # Game = "COD:AW", # Call of Duty: Advanced Warfare
            # Game = "COD:BO3", # Call of Duty: Black Ops III - XPAC,FF
            # Game = "MW3", # Call of Duty: Modern Warfare 3
            # Game = "WWII", # Call of Duty: WWII

            Game = "BO4", # Call of Duty Black Ops 4
            # Game = "BOCW", # Call of Duty Black Ops Cold War
            # Game = "Vanguard", # Call of Duty Vanguard
        )
    case 'Monolith':
        familyKeys = [ "Monolith", "Unknown" ]

        appDefaultOptions = DefaultOptions(
            ForceOpen = True,
            Family = "Monolith",
            # Game = "FEAR", # F.E.A.R.
            # Game = "FEAR:EP", # F.E.A.R.: Extraction Point
            # Game = "FEAR:PM", # F.E.A.R.: Perseus Mandate
            # Game = "FEAR2", # F.E.A.R. 2: Project Origin
            # Game = "FEAR3", # F.E.A.R. 3
        )
    case 'Origin':
        familyKeys = [ "Origin", "Unknown" ]

        appDefaultOptions = DefaultOptions(
            ForceOpen = True,
            Family = "Origin",
            # Game = "UO", # Ultima Online
            # Game = "UltimaIX", # Ultima IX
        )
    case 'Red':
        familyKeys = [ "Red", "Unknown" ]

        appDefaultOptions = DefaultOptions(
            ForceOpen = True,
            Family = "Red",
            # Game = "Witcher", # The Witcher Enhanced Edition
            # Game = "Witcher2", # The Witcher 2
            # Game = "Witcher3", # The Witcher 3: Wild Hunt
            # Game = "CP77", # Cyberpunk 2077
            # Game = "Witcher4", # The Witcher 4 Polaris (future)
        )
    case 'Ubisoft':
        familyKeys = [ "Ubisoft", "Unknown" ]

        appDefaultOptions = DefaultOptions(
            ForceOpen = True,
            Family = "Ubisoft",
            # Game = "XX", # xx
        )
    case 'Unity':
        familyKeys = [ "Unity", "Unknown" ]

        appDefaultOptions = DefaultOptions(
            ForceOpen = True,
            Family = "Unity",
            # Game = "AmongUs", # Among Us
            # Game = "Cities", # Cities: Skylines
            # Game = "Tabletop", # Tabletop Simulator
            # Game = "UBoat", # Destroyer: The U-Boat Hunter
            # Game = "7D2D", # 7 Days to Die
        )
    case 'Valve':
        familyKeys = [ "Valve", "Unknown" ]

        appDefaultOptions = DefaultOptions(
            #HL
            ForcePath = "valve/maps/boot_camp.bsp", #Map
            # ForcePath = "valve/cached.wad:LOADING.pic", #Texture
            # ForcePath = "valve/decals.wad:REFLECT1.tex", #Texture
            # ForcePath = "valve/decals.wad:{LARGE#S0.tex", #Texture
            # ForcePath = "valve/fonts.wad:FONT2.fnt", #Texture
            # ForcePath = "valve/sprites:640_logo.spr", #Sprite
            #TF
            # ForcePath = "game.tga", #Image
            # ForcePath = "cached.wad:CONBACK640.pic", #Texture
            # ForcePath = "tfc.WAD:{EASTLINE1.pic", #Texture
            #HL2
            # ForcePath = "pak01_dir.vpk:textures/dev/albedo_chart.vtex_c", #Texture
            #CS:GO
            # ForcePath = "pak01_dir.vpk:textures/dev/albedo_chart.vtex_c", #Texture
            # ForcePath = "pak01_dir.vpk:models/dev/materialforerrormodel.vmat_c", #Material
            # ForcePath = "pak01_dir.vpk:models/dev/error.vmesh_c", #Mesh
            # ForcePath = "pak01_dir.vpk:models/dev/error.vphy_c", #Phy
            # ForcePath = "pak01_dir.vpk:models/dev/error.vmdl_c", #Model
            #Dota2
            # ForcePath = "pak01_dir.vpk:textures/dev/albedo_chart.vtex_c", #Texture
            # ForcePath = "pak01_dir.vpk:models/dev/materialforerrormodel.vmat_c", #Material
            # ForcePath = "pak01_dir.vpk:models/dev/error.vmesh_c", #Mesh
            # ForcePath = "pak01_dir.vpk:models/dev/error.vphy_c", #Phy
            # ForcePath = "pak01_dir.vpk:models/dev/error.vmdl_c", #Model
            #TheLab:RR
            # ForcePath = "pak01_dir.vpk:textures/dev/albedo_chart.vtex_c", #Texture
            # ForcePath = "pak01_dir.vpk:models/dev/materialforerrormodel.vmat_c", #Material
            # ForcePath = "pak01_dir.vpk:models/dev/error.vmesh_c", #Mesh
            # ForcePath = "pak01_dir.vpk:models/dev/error.vphy_c", #Phy
            # ForcePath = "pak01_dir.vpk:models/dev/error.vmdl_c", #Model
            #HL:Alyx
            # ForcePath = "pak01_dir.vpk:textures/dev/albedo_chart.vtex_c", #Texture
            # ForcePath = "pak01_dir.vpk:models/dev/materialforerrormodel.vmat_c", #Material
            # ForcePath = "pak01_dir.vpk:models/dev/error.vmesh_c", #Mesh
            # ForcePath = "pak01_dir.vpk:models/dev/error.vphy_c", #Phy
            # ForcePath = "pak01_dir.vpk:models/dev/error.vmdl_c", #Model

            ForceOpen = True,
            Family = "Valve",
            Game = "HL", # Half-Life [open, read, texture:GL]
            # Game = "TF", # Team Fortress Classic [open, read, texture:GL]
            # Game = "CS", # Counter-Strike [open, read]
            # Game = "Ricochet", # Ricochet [open, read]
            # Game = "HL:BS", # Half-Life: Blue Shift [open, read]
            # Game = "DOD", # Day of Defeat [open, read]
            # Game = "CS:CZ", # Counter-Strike: Condition Zero [open, read]
            # Game = "HL:Src", # Half-Life: Source [open, read]
            # Game = "CS:Src", # Counter-Strike: Source [open, read]
            # Game = "HL2", # Half-Life 2 [open, read]
            # Game = "HL2:DM", # Half-Life 2: Deathmatch [open, read]
            # Game = "HL:DM:Src", # Half-Life Deathmatch: Source [open, read]
            # Game = "HL2:E1", # Half-Life 2: Episode One [open, read]
            # Game = "Portal", # Portal [open, read]
            # Game = "HL2:E2", # Half-Life 2: Episode Two [open]
            # Game = "TF2", # Team Fortress 2 [open, read]
            # Game = "L4D", # Left 4 Dead [open, read]
            # Game = "L4D2", # Left 4 Dead 2 [open, read]
            # Game = "DOD:Src", # Day of Defeat: Source [open, read]
            # Game = "Portal2", # Portal 2 [open, read]
            # Game = "CS:GO", # Counter-Strike: Global Offensive [open, read]
            # Game = "D2", # Dota 2 [open, read, texture:GL, model:GL]
            # Game = "TheLab:RR", # The Lab: Robot Repair [open, read, texture:GL, model:GL]
            # Game = "TheLab:SS", # The Lab: Secret Shop [!unity]
            # Game = "TheLab:TL", # The Lab: The Lab [!unity]
            # Game = "HL:Alyx", # Half-Life: Alyx [open, read, texture:GL, model:GL]
        )
    case 'WbB':
        familyKeys = [ "WbB", "Unknown" ]

        appDefaultOptions = DefaultOptions(
            # ForcePath = "TabooTable/0E00001E.taboo", # Ac:Data
            # ForcePath = "Texture/06000133.tex", # AC:Texture.R8G8B8
            # ForcePath = "Texture/06000FAA.tex", # AC:Texture.A8R8G8B8
            # ForcePath = "Texture/06007529.tex", # AC:Texture.INDEX16
            # ForcePath = "Texture/06007575.tex", # AC:Texture.DXT1
            # ForcePath = "Texture/06007576.tex", # AC:Texture.JPG
            # ForcePath = "Texture/0600127D.tex", # AC:Texture.R8G8B8
            # ForcePath = "Texture/06001343.tex", # AC:Texture.R8G8B8
            ForcePath = "Texture/06007529.tex", # AC:Texture.PAL

            ForceOpen = True,
            Family = "WbB",
            Game = "AC", # Asheron's Call [open, read, texture:GL]
        )
    case _:
        familyKeys = [ "Arkane", "Bethesda", "Bioware", "Black", "Blizzard", "Capcom", "Cig", "Cryptic", "Crytek", "Cyanide", "Epic", "Frictional", "Frontier", "Id", "IW", "Monolith", "Origin", "Red", "Ubisoft", "Unity", "Unknown", "Valve", "WbB" ]

        appDefaultOptions = DefaultOptions(
        )
