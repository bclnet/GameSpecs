Game Specs
===

Game Specs is an open-source, cross-platform solution for delivering game assets as a service.

### Game Specs Benefits:
* Portable (windows, apple, linux, mobile, intel, arm)
* Loads textures, models, animations, sounds, and levels
* Avaliable with streaming assets (cached)
* References assets with a uniform resource location (url)
* Loaders for OpenGL, Unreal, Unity and Vulken
* Locates installed games
* Includes a desktop app to explore assets
* Includes a command line interface to export assets (list, unpack, shred)
* *future:* Usage tracking (think Spotify)

### Components:
1. Context - the interface for interacting with this service
2. Family - the grouping of games by a shared aspect
3. Platform - endpoints for using game assets like unity, unreal, etc
4. Application - a collection of application to interact with


## [Applications](Documents/Applications/Readme.md)
Multiple applications are included in GameSpecs to make it easier to work with the game assets.

The following are the current applications:

| ID                                               | Name
| --                                               | --  
| [Command Line Interface](Documents/Applications/Command%20Line%20Interface/Readme.md)| A CLI tool.
| [Explorer](Documents/Applications/Explorer/Readme.md)                   | An application explorer.
| [Unity Plugin](Documents/Applications/Unity%20Plugin/Readme.md)         | A Unity plugin.
| [Unreal Plugin](Documents/Applications/Unreal%20Plugin/Readme.md)       | A Unreal plugin.

## [Context](Documents/Context/Readme.md)
Context provides the interface for interacting with this service

* Resource - a uri formated resource with a path and game component
* Family - represents a family of games by a shared aspect
* FamilyGame - represents a single game
* FamilyManager - a static interface for the service
* FamilyPlatform - represents the current platform
* PakFile - represents a games collection of assets


### Loading an asset:
1. service locates all installed games
2. (*optional*) initiate a game platform: `UnityPlatform.Startup()`
3. get a family reference: `var family = FamilyManager.GetFamily("ID")`
4. open a game specific archive file: `var pakFile = family.OpenPakFile("game:/Archive#ID")`
5. load a game specific asset: `var obj = await pakFile.LoadFileObjectAsync<object>("Path");`
6. service parses game objects for the specifed resource: textures, models, levels, etc
7. service adapts the game objects to the current platform: unity, unreal, etc
8. platform now contains the specified game asset
9. additionally the service provides a collection of applications


## [Families](docs/Families/Readme.md)
Families are the primary grouping mechanism for interacting with the asset services.

Usually file formats center around the game developer or game engine being used, and are modified, instead of replaced, as the studio releases new versions.

The following are the current familes:

| ID                                               | Name                      | Sample Game       | Status
| --                                               | --                        | --                | --
| [Arkane](Documents/Families/Arkane/Readme.md)    | Arkane Studios            | Dishonored 2      | In Development
| [Bethesda](Documents/Families/Bethesda/Readme.md)| The Elder Scrolls         | Skyrim            | In Development
| [Bioware](Documents/Families/Bioware/Readme.md)  | BioWare Bioware           | Neverwinter Nights| In Development
| [Blizzard](Documents/Families/Blizzard/Readme.md)| Blizzard                  | StarCraft         | In Development
| [Capcom](Documents/Families/Capcom/Readme.md)    | Capcom                    | -                 | In Development
| [Cig](Documents/Families/Cig/Readme.md)          | Cloud Imperium Games      | Star Citizen      | In Development
| [Cryptic](Documents/Families/Cryptic/Readme.md)  | Cryptic                   | Star Trek Online  | In Development
| [Crytek](Documents/Families/Cry/Readme.md)       | Crytek                    | MechWarrior Online| In Development
| [Cyanide](Documents/Families/Cyanide/Readme.md)  | Cyanide Formats           | The Council       | In Development
| [Frictional](Documents/Families/Frictional/Readme.md)| Frictional Games      | SOMA              | In Development
| [Frontier](Documents/Families/Frontier/Readme.md)| Frontier Developments     | -                 | In Development
| [Id](Documents/Families/Id/Readme.md)            | id Software               | Doom              | In Development
| [IW](Documents/Families/IW/Readme.md)            | Infinity Ward             | Call of Duty      | In Development
| [Monolith](Documents/Families/Monolith/Readme.md)| Monolith                  | F.E.A.R.          | In Development
| [Origin](Documents/Families/Origin/Readme.md)    | Origin Systems            | Ultima Online     | In Development
| [Red](Documents/Families/Red/Readme.md)          | REDengine                 | The Witcher 3: Wild Hunt | In Development
| [Unity](Documents/Families/Unity/Readme.md)      | Unity                     | AmongUs           | In Development
| [Unknown](Documents/Families/Unknown/Readme.md)  | Unknown                   | N/A               | In Development
| [Unreal](Documents/Families/Unreal/Readme.md)    | Unreal                    | BioShock          | In Development
| [Valve](Documents/Families/Valve/Readme.md)      | Valve                     | Dota 2            | In Development
| [WbB](Documents/Families/WbB/Readme.md)          | Asheron's Call            | Asheron's Call    | In Development

## [Platforms](Documents/Platforms/Readme.md)
Platforms provide the interface to each gaming platform.

## Games
---
The following are the current games:

| ID | Name | Open | Read | Texure | Model | Level
| -- | --   | --   | --   | --     | --    | --
| **Arkane** | **Arkane Studios**
| [AF](['https://www.gog.com/en/game/arx_fatalis']) | Arx Fatalis | open | read | gl -- -- | -- -- -- | -- -- --
| [DOM](['https://store.steampowered.com/app/2100']) | Dark Messiah of Might and Magic | open | read | -- -- -- | -- -- -- | -- -- --
| [KS]([]) | KarmaStar | - | - | -- -- -- | -- -- -- | -- -- --
| [D](['https://www.gog.com/en/game/dishonored_definitive_edition']) | Dishonored | - | - | -- -- -- | -- -- -- | -- -- --
| [D2](['https://www.gog.com/index.php/game/dishonored_2']) | Dishonored 2 | open | read | -- -- -- | -- -- -- | -- -- --
| [P](['https://www.gog.com/en/game/prey']) | Prey | open | read | -- -- -- | -- -- -- | -- -- --
| [D:DOTO](['https://www.gog.com/en/game/dishonored_death_of_the_outsider']) | Dishonored: Death of the Outsider | - | - | -- -- -- | -- -- -- | -- -- --
| [W:YB](['https://store.steampowered.com/app/1056960']) | Wolfenstein: Youngblood | - | - | -- -- -- | -- -- -- | -- -- --
| [W:CP](['https://store.steampowered.com/app/1056970']) | Wolfenstein: Cyberpilot | - | - | -- -- -- | -- -- -- | -- -- --
| [DL](['https://store.steampowered.com/app/1252330']) | Deathloop | - | - | -- -- -- | -- -- -- | -- -- --
| [RF](['https://bethesda.net/en/game/redfall']) | Redfall (future) | - | - | -- -- -- | -- -- -- | -- -- --
| **Bethesda** | **The Elder Scrolls**
| [Fallout](['https://store.steampowered.com/app/38400']) | Fallout | - | - | -- -- -- | -- -- -- | -- -- --
| [Fallout2](['https://store.steampowered.com/app/38410']) | Fallout 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [Morrowind](['https://store.steampowered.com/app/22320']) | The Elder Scrolls III: Morrowind | - | - | -- -- -- | -- -- -- | -- -- --
| [IHRA](['https://en.wikipedia.org/wiki/IHRA_Drag_Racing']) | IHRA Professional Drag Racing 2005 | - | - | -- -- -- | -- -- -- | -- -- --
| [Oblivion](['https://store.steampowered.com/app/22330']) | The Elder Scrolls IV: Oblivion | - | - | -- -- -- | -- -- -- | -- -- --
| [Fallout3](['https://store.steampowered.com/app/22370']) | Fallout 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [FalloutNV](['https://store.steampowered.com/app/22380']) | Fallout New Vegas | - | - | -- -- -- | -- -- -- | -- -- --
| [Skyrim](['https://store.steampowered.com/app/72850']) | The Elder Scrolls V: Skyrim | - | - | -- -- -- | -- -- -- | -- -- --
| [Fallout4](['https://store.steampowered.com/app/377160']) | Fallout 4 | - | - | -- -- -- | -- -- -- | -- -- --
| [SkyrimSE](['https://store.steampowered.com/app/489830']) | The Elder Scrolls V: Skyrim – Special Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [Fallout:S](['https://store.steampowered.com/app/588430']) | Fallout Shelter | - | - | -- -- -- | -- -- -- | -- -- --
| [Fallout4VR](['https://store.steampowered.com/app/611660']) | Fallout 4 VR | - | - | -- -- -- | -- -- -- | -- -- --
| [SkyrimVR](['https://store.steampowered.com/app/611670']) | The Elder Scrolls V: Skyrim VR | - | - | -- -- -- | -- -- -- | -- -- --
| [Blades](['https://elderscrolls.bethesda.net/en/blades']) | The Elder Scrolls: Blades | - | - | -- -- -- | -- -- -- | -- -- --
| [Fallout76](['https://store.steampowered.com/app/1151340']) | Fallout 76 | - | - | -- -- -- | -- -- -- | -- -- --
| [Starfield](['https://store.steampowered.com/app/1716740']) | Starfield | - | - | -- -- -- | -- -- -- | -- -- --
| [Unknown1](['https://en.wikipedia.org/wiki/The_Elder_Scrolls']) | The Elder Scrolls VI (future) | - | - | -- -- -- | -- -- -- | -- -- --
| [Fallout5](['https://en.wikipedia.org/wiki/Fallout_(series)']) | Fallout 5 (future) | - | - | -- -- -- | -- -- -- | -- -- --
| **Bioware** | **BioWare**
| [SS](['https://www.gog.com/en/game/shattered_steel']) | Shattered Steel | - | - | -- -- -- | -- -- -- | -- -- --
| [BG](['https://www.gog.com/en/game/baldurs_gate_enhanced_edition']) | Baldur's Gate | - | - | -- -- -- | -- -- -- | -- -- --
| [MDK2](['https://www.gog.com/en/game/mdk_2']) | MDK2 | - | - | -- -- -- | -- -- -- | -- -- --
| [BG2](['https://www.gog.com/en/game/baldurs_gate_2_enhanced_edition']) | Baldur's Gate II: Shadows of Amn | - | - | -- -- -- | -- -- -- | -- -- --
| [NWN](['https://store.steampowered.com/app/704450', 'https://www.gog.com/en/game/neverwinter_nights_enhanced_edition_pack']) | Neverwinter Nights | - | - | -- -- -- | -- -- -- | -- -- --
| [KotOR](['https://store.steampowered.com/app/32370', 'https://www.gog.com/en/game/star_wars_knights_of_the_old_republic']) | Star Wars: Knights of the Old Republic | - | - | -- -- -- | -- -- -- | -- -- --
| [JE](['https://www.gog.com/en/game/jade_empire_special_edition']) | Jade Empire | - | - | -- -- -- | -- -- -- | -- -- --
| [ME](['https://store.steampowered.com/app/17460']) | Mass Effect | - | - | -- -- -- | -- -- -- | -- -- --
| [NWN2](['https://www.gog.com/en/game/neverwinter_nights_2_complete']) | Neverwinter Nights 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [SC:TDB](['https://en.wikipedia.org/wiki/Sonic_Chronicles:_The_Dark_Brotherhood']) | Sonic Chronicles: The Dark Brotherhood | - | - | -- -- -- | -- -- -- | -- -- --
| [ME:G](['https://en.wikipedia.org/wiki/Mass_Effect_Galaxy']) | Mass Effect Galaxy | - | - | -- -- -- | -- -- -- | -- -- --
| [DA:O](['https://store.steampowered.com/app/47810']) | Dragon Age: Origins | - | - | -- -- -- | -- -- -- | -- -- --
| [ME2](['https://store.steampowered.com/app/24980']) | Mass Effect 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [DA2](['https://store.steampowered.com/app/1238040']) | Dragon Age II | - | - | -- -- -- | -- -- -- | -- -- --
| [DA:L](['https://en.wikipedia.org/wiki/Dragon_Age_Legends']) | Dragon Age Legends | - | - | -- -- -- | -- -- -- | -- -- --
| [SWTOR](['https://store.steampowered.com/app/1286830']) | Star Wars: The Old Republic | - | - | -- -- -- | -- -- -- | -- -- --
| [ME3](['https://store.steampowered.com/app/1238020']) | Mass Effect 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [WO](['https://en.wikipedia.org/wiki/Warhammer_Online:_Wrath_of_Heroes']) | Warhammer Online: Wrath of Heroes (Cancelled) | - | - | -- -- -- | -- -- -- | -- -- --
| [CC](['https://en.wikipedia.org/wiki/Warhammer_Online:_Wrath_of_Heroes']) | Command & Conquer: Generals 2 (Cancelled) | - | - | -- -- -- | -- -- -- | -- -- --
| [DA:I](['https://store.steampowered.com/app/1222690']) | Dragon Age: Inquisition | - | - | -- -- -- | -- -- -- | -- -- --
| [SR](['https://en.wikipedia.org/wiki/Shadow_Realms']) | Shadow Realms (Cancelled) | - | - | -- -- -- | -- -- -- | -- -- --
| [ME:A](['https://store.steampowered.com/app/1238000']) | Mass Effect: Andromeda | - | - | -- -- -- | -- -- -- | -- -- --
| [A](['https://www.ea.com/games/anthem/buy/pc']) | Anthem | - | - | -- -- -- | -- -- -- | -- -- --
| [ME:LE](['https://store.steampowered.com/app/1328670']) | Mass Effect: Legendary Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [DA:D](['https://www.ea.com/en-gb/games/dragon-age/dragon-age-dreadwolf']) | Dragon Age: Dreadwolf (future) | - | - | -- -- -- | -- -- -- | -- -- --
| [ME5](['https://en.wikipedia.org/wiki/Mass_Effect']) | Mass Effect 5 (future) | - | - | -- -- -- | -- -- -- | -- -- --
| **Blizzard** | **Blizzard**
| [TDAROS]([]) | The Death and Return of Superman | - | - | -- -- -- | -- -- -- | -- -- --
| [B]([]) | Blackthorne | - | - | -- -- -- | -- -- -- | -- -- --
| [W1]([]) | Warcraft: Orcs & Humans | - | - | -- -- -- | -- -- -- | -- -- --
| [JLTF]([]) | Justice League Task Force | - | - | -- -- -- | -- -- -- | -- -- --
| [W2]([]) | Warcraft II: Tides of Darkness | - | - | -- -- -- | -- -- -- | -- -- --
| [D1]([]) | Diablo | - | - | -- -- -- | -- -- -- | -- -- --
| [TLV2]([]) | The Lost Vikings 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [SC](['https://us.shop.battle.net/en-us/product/starcraft']) | StarCraft | - | - | -- -- -- | -- -- -- | -- -- --
| [D2]([]) | Diablo II | - | - | -- -- -- | -- -- -- | -- -- --
| [D2R](['https://us.shop.battle.net/en-us/product/diablo_ii_resurrected']) | Diablo II: Resurrected | - | - | -- -- -- | -- -- -- | -- -- --
| [W3](['https://us.shop.battle.net/en-us/product/warcraft-iii-reforged']) | Warcraft III: Reign of Chaos | - | - | -- -- -- | -- -- -- | -- -- --
| [WOW](['https://us.shop.battle.net/en-us/family/world-of-warcraft']) | World of Warcraft | - | - | -- -- -- | -- -- -- | -- -- --
| [WOWC](['https://us.shop.battle.net/en-us/family/world-of-warcraft-classic']) | World of Warcraft: Classic | - | - | -- -- -- | -- -- -- | -- -- --
| [SC2](['https://us.shop.battle.net/en-us/product/starcraft-ii']) | StarCraft II: Wings of Liberty | - | - | -- -- -- | -- -- -- | -- -- --
| [D3](['https://us.shop.battle.net/en-us/product/diablo-iii']) | Diablo III | - | - | -- -- -- | -- -- -- | -- -- --
| [HOTS](['']) | Heroes of the Storm | - | - | -- -- -- | -- -- -- | -- -- --
| [HS](['https://us.shop.battle.net/en-us/product/hearthstone-heroes-of-warcraft']) | Hearthstone | - | - | -- -- -- | -- -- -- | -- -- --
| [OW](['https://us.shop.battle.net/en-us/family/overwatch']) | Overwatch | - | - | -- -- -- | -- -- -- | -- -- --
| [CB](['https://us.shop.battle.net/en-us/family/crash-bandicoot-4']) | Crash Bandicoot™ 4: It’s About Time | - | - | -- -- -- | -- -- -- | -- -- --
| [DI](['https://diabloimmortal.blizzard.com/en-us/']) | Diablo Immortal | - | - | -- -- -- | -- -- -- | -- -- --
| [OW2](['https://us.shop.battle.net/en-us/product/overwatch']) | Overwatch 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [D4](['https://diablo4.blizzard.com/en-us/']) | Diablo IV | - | - | -- -- -- | -- -- -- | -- -- --
| [Other]([]) | Other | - | - | -- -- -- | -- -- -- | -- -- --
| **Capcom+0-D** | **None**
| [1941:CA](['https://en.wikipedia.org/wiki/1941:_Counter_Attack']) | 1941: Counter Attack | - | - | -- -- -- | -- -- -- | -- -- --
| [1942](['https://en.wikipedia.org/wiki/1942_(video_game)']) | 1942 | - | - | -- -- -- | -- -- -- | -- -- --
| [1942:FS](['https://en.wikipedia.org/wiki/1942_(video_game)']) | 1942: First Strike | - | - | -- -- -- | -- -- -- | -- -- --
| [1942:JS](['https://en.wikipedia.org/wiki/1942:_Joint_Strike']) | 1942: Joint Strike | - | - | -- -- -- | -- -- -- | -- -- --
| [1943:TBoM](['https://en.wikipedia.org/wiki/1943:_The_Battle_of_Midway']) | 1943: The Battle of Midway | - | - | -- -- -- | -- -- -- | -- -- --
| [1944:TLM](['https://en.wikipedia.org/wiki/1944:_The_Loop_Master']) | 1944: The Loop Master | - | - | -- -- -- | -- -- -- | -- -- --
| [19XX:TWAD](['https://en.wikipedia.org/wiki/19XX:_The_War_Against_Destiny']) | 19XX: The War Against Destiny | - | - | -- -- -- | -- -- -- | -- -- --
| [AAI:ME](['https://en.wikipedia.org/wiki/Ace_Attorney_Investigations:_Miles_Edgeworth']) | Ace Attorney Investigations: Miles Edgeworth | - | - | -- -- -- | -- -- -- | -- -- --
| [AQ:CW]([]) | Adventure Quiz: Capcom World | - | - | -- -- -- | -- -- -- | -- -- --
| [AQ:CW2]([]) | Adventure Quiz: Capcom World 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [AQ2:H]([]) | Adventure Quiz 2: Hatena? no Daibouken | - | - | -- -- -- | -- -- -- | -- -- --
| [AITMK](['https://en.wikipedia.org/wiki/Adventures_in_the_Magic_Kingdom']) | Adventures in the Magic Kingdom | - | - | -- -- -- | -- -- -- | -- -- --
| [AOB](['https://en.wikipedia.org/wiki/Age_of_Booty']) | Age of Booty | - | - | -- -- -- | -- -- -- | -- -- --
| [Airborne]([]) | Airborne | - | - | -- -- -- | -- -- -- | -- -- --
| [AVP](['https://en.wikipedia.org/wiki/Alien_vs._Predator_(arcade_game)']) | Alien vs. Predator | - | - | -- -- -- | -- -- -- | -- -- --
| [AJ:AA](['https://en.wikipedia.org/wiki/Apollo_Justice:_Ace_Attorney']) | Apollo Justice: Ace Attorney | - | - | -- -- -- | -- -- -- | -- -- --
| [AYSTA5G]([]) | Are You Smarter Than a 5th Grader? 2009 Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [ArmoredWarriors](['https://en.wikipedia.org/wiki/Armored_Warriors']) | Armored Warriors | - | - | -- -- -- | -- -- -- | -- -- --
| [AANM:IT](['https://en.wikipedia.org/wiki/Arthur_to_Astaroth_no_Nazomakaimura:_Incredible_Toons']) | Arthur & Astrot NazoMakaimura: Incredible Toons | - | - | -- -- -- | -- -- -- | -- -- --
| [ANJ2:TASR]([]) | Ashita no Joe 2: The Anime Super Remix | - | - | -- -- -- | -- -- -- | -- -- --
| [AsuraWrath](['https://en.wikipedia.org/wiki/Asura%27s_Wrath']) | Asura's Wrath | - | - | -- -- -- | -- -- -- | -- -- --
| [Ataxx](['https://en.wikipedia.org/wiki/Ataxx']) | Ataxx | - | - | -- -- -- | -- -- -- | -- -- --
| [AM](['https://en.wikipedia.org/wiki/Auto_Modellista']) | Auto Modellista | - | - | -- -- -- | -- -- -- | -- -- --
| [Avengers](['https://en.wikipedia.org/wiki/Avengers_(1987_video_game)']) | Avengers | - | - | -- -- -- | -- -- -- | -- -- --
| [BattleCircuit](['https://en.wikipedia.org/wiki/Battle_Circuit']) | Battle Circuit | - | - | -- -- -- | -- -- -- | -- -- --
| [BD:FoV](['https://en.wikipedia.org/wiki/Beat_Down:_Fists_of_Vengeance']) | Beat Down: Fists of Vengeance | - | - | -- -- -- | -- -- -- | -- -- --
| [BigBangBar]([]) | Big Bang Bar | - | - | -- -- -- | -- -- -- | -- -- --
| [RE7:BH](['https://en.wikipedia.org/wiki/Resident_Evil_7:_Biohazard']) | Resident Evil 7: Biohazard | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:VC](['https://en.wikipedia.org/wiki/Resident_Evil_%E2%80%93_Code:_Veronica']) | Resident Evil – Code: Veronica | - | - | -- -- -- | -- -- -- | -- -- --
| [BionicCommando:Arcade](['https://en.wikipedia.org/wiki/Bionic_Commando_(1987_video_game)']) | Bionic Commando (arcade) | - | - | -- -- -- | -- -- -- | -- -- --
| [BionicCommando:NES](['']) | Bionic Commando (NES video game) | - | - | -- -- -- | -- -- -- | -- -- --
| [BionicCommando:GB](['https://en.wikipedia.org/wiki/Bionic_Commando_(1988_video_game)']) | Bionic Commando (Game Boy) | - | - | -- -- -- | -- -- -- | -- -- --
| [BionicCommando](['https://store.steampowered.com/app/21670', 'https://en.wikipedia.org/wiki/Bionic_Commando_(2009_video_game)']) | Bionic Commando (2009 video game) | - | - | -- -- -- | -- -- -- | -- -- --
| [BionicCommando:R](['https://store.steampowered.com/app/21680', 'https://en.wikipedia.org/wiki/Bionic_Commando_Rearmed']) | Bionic Commando Rearmed | - | - | -- -- -- | -- -- -- | -- -- --
| [BionicCommando:R2](['https://en.wikipedia.org/wiki/Bionic_Commando_Rearmed_2']) | Bionic Commando Rearmed 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [BlackTiger](['https://en.wikipedia.org/wiki/Black_Tiger_(video_game)']) | Black Tiger | - | - | -- -- -- | -- -- -- | -- -- --
| [BlockBlock](['https://en.wikipedia.org/wiki/Capcom_Classics_Collection']) | Block Block | - | - | -- -- -- | -- -- -- | -- -- --
| [BlackCommand]([]) | Black Command | - | - | -- -- -- | -- -- -- | -- -- --
| [Bombastic](['https://en.wikipedia.org/wiki/Bombastic_(video_game)']) | Bombastic | - | - | -- -- -- | -- -- -- | -- -- --
| [BombLink]([]) | BombLink | - | - | -- -- -- | -- -- -- | -- -- --
| [Bonkers](['https://en.wikipedia.org/wiki/Bonkers_(SNES_video_game)']) | Bonkers | - | - | -- -- -- | -- -- -- | -- -- --
| [Bounty Hunter Sara]([]) | Bounty Hunter Sara: Holy Mountain no Teiou | - | - | -- -- -- | -- -- -- | -- -- --
| [BreakShot](['https://en.wikipedia.org/wiki/Break_Shot']) | BreakShot | - | - | -- -- -- | -- -- -- | -- -- --
| [BOF](['https://en.wikipedia.org/wiki/Breath_of_Fire_(video_game)']) | Breath of Fire | - | - | -- -- -- | -- -- -- | -- -- --
| [BOF2](['https://en.wikipedia.org/wiki/Breath_of_Fire_II']) | Breath of Fire II | - | - | -- -- -- | -- -- -- | -- -- --
| [BOF3](['https://en.wikipedia.org/wiki/Breath_of_Fire_III']) | Breath of Fire III | - | - | -- -- -- | -- -- -- | -- -- --
| [BOF4](['https://en.wikipedia.org/wiki/Breath_of_Fire_IV']) | Breath of Fire IV | - | - | -- -- -- | -- -- -- | -- -- --
| [BOF6](['https://en.wikipedia.org/wiki/Breath_of_Fire_6']) | Breath of Fire 6 | - | - | -- -- -- | -- -- -- | -- -- --
| [BOF:DQ](['https://en.wikipedia.org/wiki/Breath_of_Fire:_Dragon_Quarter']) | Breath of Fire: Dragon Quarter | - | - | -- -- -- | -- -- -- | -- -- --
| [BusterBros](['https://en.wikipedia.org/wiki/Buster_Bros.']) | Buster Bros. | - | - | -- -- -- | -- -- -- | -- -- --
| [BusterBros:C](['https://en.wikipedia.org/wiki/Buster_Bros._Collection']) | Buster Bros. Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [Cabal](['https://en.wikipedia.org/wiki/Cabal_(video_game)']) | Cabal | - | - | -- -- -- | -- -- -- | -- -- --
| [Cadillacs+Dinosaurs](['https://en.wikipedia.org/wiki/Cadillacs_and_Dinosaurs_(video_game)']) | Cadillacs and Dinosaurs | - | - | -- -- -- | -- -- -- | -- -- --
| [CannonSpike](['https://en.wikipedia.org/wiki/Cannon_Spike']) | Cannon Spike | - | - | -- -- -- | -- -- -- | -- -- --
| [Arcade:S](['https://store.steampowered.com/app/1755910', 'https://en.wikipedia.org/wiki/Capcom_Arcade_Stadium#Capcom_Arcade_2nd_Stadium']) | Capcom Arcade 2nd Stadium | - | - | -- -- -- | -- -- -- | -- -- --
| [Arcade:C](['https://en.wikipedia.org/wiki/Capcom_Arcade_Cabinet']) | Capcom Arcade Cabinet | - | - | -- -- -- | -- -- -- | -- -- --
| [Arcade:V2]([]) | Capcom Arcade Hits Volume 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [Baseball:SGO]([]) | Capcom Baseball - Suketto Gaijin Oo-Abare | - | - | -- -- -- | -- -- -- | -- -- --
| [BEU:B](['https://store.steampowered.com/app/885150']) | Capcom Beat ‘Em Up Bundle | - | - | -- -- -- | -- -- -- | -- -- --
| [Bowling](['https://en.wikipedia.org/wiki/Capcom_Bowling']) | Capcom Bowling | - | - | -- -- -- | -- -- -- | -- -- --
| [Class:V1](['https://en.wikipedia.org/wiki/Capcom_Classics_Collection']) | Capcom Classics Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [Class:RL](['https://en.wikipedia.org/wiki/Capcom_Classics_Collection#Capcom_Classics_Collection_Reloaded']) | Capcom Classics Collection Reloaded | - | - | -- -- -- | -- -- -- | -- -- --
| [Class:RM](['https://en.wikipedia.org/wiki/Capcom_Classics_Collection#Capcom_Classics_Collection_Remixed']) | Capcom Classics Collection Remixed | - | - | -- -- -- | -- -- -- | -- -- --
| [Class:V2](['https://en.wikipedia.org/wiki/Capcom_Classics_Collection']) | Capcom Classics Collection Vol. 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [Class:MM](['https://en.wikipedia.org/wiki/Capcom_Classics_Collection#Capcom_Classics_Mini_Mix']) | Capcom Classics Mini-Mix | - | - | -- -- -- | -- -- -- | -- -- --
| [FightingEvolution](['https://en.wikipedia.org/wiki/Capcom_Fighting_Evolution']) | Capcom Fighting Evolution | - | - | -- -- -- | -- -- -- | -- -- --
| [Gen1](['https://en.wikipedia.org/wiki/Capcom_Generations#Capcom_Generations_1:_Wings_of_Destiny']) | Capcom Generation 1 | - | - | -- -- -- | -- -- -- | -- -- --
| [Gen2](['https://en.wikipedia.org/wiki/Capcom_Generations#Capcom_Generations_2:_Chronicles_of_Arthur']) | Capcom Generation 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [Gen3](['https://en.wikipedia.org/wiki/Capcom_Generations#Capcom_Generations_3:_The_First_Generation']) | Capcom Generation 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [Gen4](['https://en.wikipedia.org/wiki/Capcom_Generations#Capcom_Generations_4:_Blazing_Guns']) | Capcom Generation 4 | - | - | -- -- -- | -- -- -- | -- -- --
| [Gen5](['https://en.wikipedia.org/wiki/Capcom_Generations#Capcom_Generations_5:_Street_Fighter_Collection_2']) | Capcom Generation 5 | - | - | -- -- -- | -- -- -- | -- -- --
| [Golf]([]) | Capcom Golf | - | - | -- -- -- | -- -- -- | -- -- --
| [Quiz:T]([]) | Capcom no Quiz: Tonosama no Yabou | - | - | -- -- -- | -- -- -- | -- -- --
| [Puzzle:World](['https://en.wikipedia.org/wiki/Capcom_Puzzle_World']) | Capcom Puzzle World | - | - | -- -- -- | -- -- -- | -- -- --
| [Quiz:H]([]) | Capcom Quiz: Hatena? no Daibouken | - | - | -- -- -- | -- -- -- | -- -- --
| [SportsClub]([]) | Capcom Sports Club | - | - | -- -- -- | -- -- -- | -- -- --
| [TaisenFanDisk]([]) | Capcom Taisen Fan Disc | - | - | -- -- -- | -- -- -- | -- -- --
| [VS2](['https://en.wikipedia.org/wiki/Capcom_vs._SNK_2']) | Capcom vs. SNK 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [VS2:EO](['https://en.wikipedia.org/wiki/Capcom_vs._SNK_2']) | Capcom vs. SNK 2 EO | - | - | -- -- -- | -- -- -- | -- -- --
| [VS2:MM](['https://en.wikipedia.org/wiki/Capcom_vs._SNK_2']) | Capcom vs. SNK 2: Mark of the Millennium | - | - | -- -- -- | -- -- -- | -- -- --
| [VS2:MF01](['https://en.wikipedia.org/wiki/Capcom_vs._SNK_2']) | Capcom vs. SNK 2: Millionaire Fighting 2001 | - | - | -- -- -- | -- -- -- | -- -- --
| [VS:P](['https://en.wikipedia.org/wiki/Capcom_vs._SNK:_Millennium_Fight_2000#Versions']) | Capcom vs. SNK Pro | - | - | -- -- -- | -- -- -- | -- -- --
| [VS:MF00](['https://en.wikipedia.org/wiki/Capcom_vs._SNK:_Millennium_Fight_2000']) | Capcom vs. SNK: Millennium Fight 2000 | - | - | -- -- -- | -- -- -- | -- -- --
| [VS:MF00P](['https://en.wikipedia.org/wiki/Capcom_vs._SNK:_Millennium_Fight_2000']) | Capcom vs. SNK: Millennium Fight 2000 Pro | - | - | -- -- -- | -- -- -- | -- -- --
| [Football:MVP](['https://en.wikipedia.org/wiki/Capcom%27s_MVP_Football']) | Capcom's MVP Football | - | - | -- -- -- | -- -- -- | -- -- --
| [Soccer:Shootout]([]) | Capcom's Soccer Shootout | - | - | -- -- -- | -- -- -- | -- -- --
| [CaptainCommando](['https://en.wikipedia.org/wiki/Captain_Commando']) | Captain Commando | - | - | -- -- -- | -- -- -- | -- -- --
| [CarrierAirWing](['https://en.wikipedia.org/wiki/Carrier_Air_Wing_(video_game)']) | Carrier Air Wing | - | - | -- -- -- | -- -- -- | -- -- --
| [CashCab](['https://en.wikipedia.org/wiki/Cash_Cab_(British_game_show)']) | Cash Cab | - | - | -- -- -- | -- -- -- | -- -- --
| [Catan](['https://en.wikipedia.org/wiki/Catan']) | Catan | - | - | -- -- -- | -- -- -- | -- -- --
| [ChaosLegion](['https://en.wikipedia.org/wiki/Chaos_Legion']) | Chaos Legion | - | - | -- -- -- | -- -- -- | -- -- --
| [CTHCC](['https://en.wikipedia.org/wiki/Cherry_Tree_High_Comedy_Club']) | Cherry Tree High Comedy Club | - | - | -- -- -- | -- -- -- | -- -- --
| [CND:RR](['https://en.wikipedia.org/wiki/Chip_%27n_Dale_Rescue_Rangers_(video_game)']) | Chip 'n Dale Rescue Rangers | - | - | -- -- -- | -- -- -- | -- -- --
| [CND:RR2](['https://en.wikipedia.org/wiki/Chip_%27n_Dale_Rescue_Rangers_2']) | Chip 'n Dale Rescue Rangers 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [Choko]([]) | Choko | - | - | -- -- -- | -- -- -- | -- -- --
| [CT3](['https://en.wikipedia.org/wiki/Clock_Tower_3']) | Clock Tower 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [CN:V](['https://en.wikipedia.org/wiki/Code_Name:_Viper']) | Code Name: Viper | - | - | -- -- -- | -- -- -- | -- -- --
| [Commando](['https://en.wikipedia.org/wiki/Commando_(video_game)']) | Commando | - | - | -- -- -- | -- -- -- | -- -- --
| [CrimsonTears](['https://en.wikipedia.org/wiki/Crimson_Tears']) | Crimson Tears | - | - | -- -- -- | -- -- -- | -- -- --
| [CriticalBullet]([]) | Critical Bullet: 7th Target | - | - | -- -- -- | -- -- -- | -- -- --
| [Cyberbots](['https://en.wikipedia.org/wiki/Cyberbots:_Full_Metal_Madness']) | Cyberbots: Full Metal Madness | - | - | -- -- -- | -- -- -- | -- -- --
| [DS:TNW](['https://en.wikipedia.org/wiki/Darkstalkers:_The_Night_Warriors']) | Darkstalkers: The Night Warriors | - | - | -- -- -- | -- -- -- | -- -- --
| [DS3](['https://en.wikipedia.org/wiki/Darkstalkers_3']) | Darkstalkers 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [DS:TCT](['https://en.wikipedia.org/wiki/Darkstalkers_Chronicle:_The_Chaos_Tower']) | Darkstalkers Chronicle: The Chaos Tower | - | - | -- -- -- | -- -- -- | -- -- --
| [DS:R](['https://en.wikipedia.org/wiki/Darkstalkers_Resurrection']) | Darkstalkers Resurrection | - | - | -- -- -- | -- -- -- | -- -- --
| [DV](['https://store.steampowered.com/app/45710', 'https://en.wikipedia.org/wiki/Dark_Void']) | Dark Void | - | - | -- -- -- | -- -- -- | -- -- --
| [DV:Z](['https://store.steampowered.com/app/45730/Dark_Void_Zero/', 'https://en.wikipedia.org/wiki/Dark_Void_Zero']) | Dark Void Zero | - | - | -- -- -- | -- -- -- | -- -- --
| [Darkwatch](['https://en.wikipedia.org/wiki/Darkwatch']) | Darkwatch | - | - | -- -- -- | -- -- -- | -- -- --
| [DarkwingDuck](['https://en.wikipedia.org/wiki/Darkwing_Duck_(Capcom_video_game)']) | Darkwing Duck | - | - | -- -- -- | -- -- -- | -- -- --
| [DeadPhoenix](['https://en.wikipedia.org/wiki/Capcom_Five#Dead_Phoenix']) | Dead Phoenix | - | - | -- -- -- | -- -- -- | -- -- --
| [DR](['https://store.steampowered.com/app/427190', 'https://en.wikipedia.org/wiki/Dead_Rising_(video_game)']) | Dead Rising | - | - | -- -- -- | -- -- -- | -- -- --
| [DR2](['https://store.steampowered.com/app/45740', 'https://en.wikipedia.org/wiki/Dead_Rising_2']) | Dead Rising 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [DR2:CW](['https://en.wikipedia.org/wiki/Dead_Rising_2#Case_West']) | Dead Rising 2: Case West | - | - | -- -- -- | -- -- -- | -- -- --
| [DR2:CZ](['https://en.wikipedia.org/wiki/Dead_Rising_2#Case_Zero']) | Dead Rising 2: Case Zero | - | - | -- -- -- | -- -- -- | -- -- --
| [DR2:OtR](['https://store.steampowered.com/app/45770', 'https://en.wikipedia.org/wiki/Dead_Rising_2:_Off_the_Record']) | Dead Rising 2: Off the Record | - | - | -- -- -- | -- -- -- | -- -- --
| [DR3](['https://store.steampowered.com/app/265550', 'https://en.wikipedia.org/wiki/Dead_Rising_3']) | Dead Rising 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [DR4](['https://store.steampowered.com/app/543460', 'https://en.wikipedia.org/wiki/Dead_Rising_4']) | Dead Rising 4 | - | - | -- -- -- | -- -- -- | -- -- --
| [DR4:FBP]([]) | Dead Rising 4: Frank's Big Package | - | - | -- -- -- | -- -- -- | -- -- --
| [DR:CTYD](['https://en.wikipedia.org/wiki/Dead_Rising:_Chop_Till_You_Drop']) | Dead Rising: Chop Till You Drop | - | - | -- -- -- | -- -- -- | -- -- --
| [DeepDown](['https://en.wikipedia.org/wiki/Deep_Down_(video_game)']) | Deep Down | - | - | -- -- -- | -- -- -- | -- -- --
| [DemonsCrest](['https://en.wikipedia.org/wiki/Demon%27s_Crest']) | Demon's Crest | - | - | -- -- -- | -- -- -- | -- -- --
| [Desperado](['https://en.wikipedia.org/wiki/Gun.Smoke']) | Desperado | - | - | -- -- -- | -- -- -- | -- -- --
| [DOAE](['https://en.wikipedia.org/wiki/Destiny_of_an_Emperor']) | Destiny of an Emperor | - | - | -- -- -- | -- -- -- | -- -- --
| [DOAE2](['https://en.wikipedia.org/wiki/Tenchi_wo_Kurau_II']) | Destiny of an Emperor II | - | - | -- -- -- | -- -- -- | -- -- --
| [DMC](['https://en.wikipedia.org/wiki/Devil_May_Cry_(video_game)']) | Devil May Cry | - | - | -- -- -- | -- -- -- | -- -- --
| [DMC2](['https://en.wikipedia.org/wiki/Devil_May_Cry_2']) | Devil May Cry 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [DMC3:DA](['https://en.wikipedia.org/wiki/Devil_May_Cry_3:_Dante%27s_Awakening']) | Devil May Cry 3: Dante's Awakening | - | - | -- -- -- | -- -- -- | -- -- --
| [DMC3:S](['https://store.steampowered.com/app/6550', 'https://en.wikipedia.org/wiki/Devil_May_Cry_3:_Dante%27s_Awakening#Special_Edition']) | Devil May Cry 3: Special Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [DMC4](['https://en.wikipedia.org/wiki/Devil_May_Cry_4']) | Devil May Cry 4 | - | - | -- -- -- | -- -- -- | -- -- --
| [DMC4:R](['https://en.wikipedia.org/wiki/Devil_May_Cry_4']) | Devil May Cry 4: Refrain | - | - | -- -- -- | -- -- -- | -- -- --
| [DMC4:S](['https://store.steampowered.com/app/329050', 'https://en.wikipedia.org/wiki/Devil_May_Cry_4:_Special_Edition']) | Devil May Cry 4: Special Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [DMC5](['https://store.steampowered.com/app/601150', 'https://en.wikipedia.org/wiki/Devil_May_Cry_5']) | Devil May Cry 5 | - | - | -- -- -- | -- -- -- | -- -- --
| [DMC5:S](['https://en.wikipedia.org/wiki/Devil_May_Cry_5#Special_Edition']) | Devil May Cry 5: Special Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [DMC:HD](['https://store.steampowered.com/app/631510', 'https://en.wikipedia.org/wiki/Devil_May_Cry']) | Devil May Cry: HD Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [Dimahoo](['https://en.wikipedia.org/wiki/Dimahoo']) | Dimahoo | - | - | -- -- -- | -- -- -- | -- -- --
| [DC](['https://en.wikipedia.org/wiki/Dino_Crisis_(video_game)']) | Dino Crisis | - | - | -- -- -- | -- -- -- | -- -- --
| [DC2](['https://en.wikipedia.org/wiki/Dino_Crisis_2']) | Dino Crisis 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [DC3](['https://en.wikipedia.org/wiki/Dino_Crisis_3']) | Dino Crisis 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [DC:S](['https://en.wikipedia.org/wiki/Dino_Stalker']) | Dino Stalker | - | - | -- -- -- | -- -- -- | -- -- --
| [Disney:A](['https://en.wikipedia.org/wiki/Disney%27s_Aladdin_(Capcom_video_game)']) | Disney's Aladdin | - | - | -- -- -- | -- -- -- | -- -- --
| [Disney:HaS](['https://en.wikipedia.org/wiki/Disney%27s_Hide_and_Sneak']) | Disney's Hide and Sneak | - | - | -- -- -- | -- -- -- | -- -- --
| [Disney:MM](['https://en.wikipedia.org/wiki/Disney%27s_Magical_Mirror_Starring_Mickey_Mouse']) | Disney's Magical Mirror Starring Mickey Mouse | - | - | -- -- -- | -- -- -- | -- -- --
| [Disney:MQ](['https://en.wikipedia.org/wiki/Disney%27s_Magical_Quest#The_Magical_Quest_Starring_Mickey_Mouse']) | Disney's Magical Quest Starring Mickey Mouse | - | - | -- -- -- | -- -- -- | -- -- --
| [Disney:MQ2](['https://en.wikipedia.org/wiki/Disney%27s_Magical_Quest']) | Disney's Magical Quest 2 Starring Mickey and Minnie | - | - | -- -- -- | -- -- -- | -- -- --
| [Disney:MQ3](['https://en.wikipedia.org/wiki/Disney%27s_Magical_Quest#Disney.27s_Magical_Quest_3_Starring_Mickey_.26_Donald']) | Disney's Magical Quest 3 Starring Mickey & Donald | - | - | -- -- -- | -- -- -- | -- -- --
| [DMC:DMC](['https://store.steampowered.com/app/220440', 'https://en.wikipedia.org/wiki/DmC:_Devil_May_Cry']) | DmC: Devil May Cry | - | - | -- -- -- | -- -- -- | -- -- --
| [Dokaben](['https://en.wikipedia.org/wiki/Dokaben']) | Dokaben | - | - | -- -- -- | -- -- -- | -- -- --
| [Dokaben2](['https://en.wikipedia.org/wiki/Dokaben']) | Dokaben II | - | - | -- -- -- | -- -- -- | -- -- --
| [Dragon](['https://store.steampowered.com/app/367500', 'https://en.wikipedia.org/wiki/Dragon%27s_Dogma']) | Dragon's Dogma | - | - | -- -- -- | -- -- -- | -- -- --
| [Dragon:DA](["https://en.wikipedia.org/wiki/Dragon%27s_Dogma#Dragon's_Dogma:_Dark_Arisen"]) | Dragon's Dogma: Dark Arisen | - | - | -- -- -- | -- -- -- | -- -- --
| [Dragon2](['https://en.wikipedia.org/wiki/Dragon%27s_Dogma#Sequel']) | Dragon's Dogma II | - | - | -- -- -- | -- -- -- | -- -- --
| [DT](['https://en.wikipedia.org/wiki/DuckTales_(video_game)']) | DuckTales | - | - | -- -- -- | -- -- -- | -- -- --
| [DT2](['https://en.wikipedia.org/wiki/DuckTales_2']) | DuckTales 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [DT:R](['https://store.steampowered.com/app/237630', 'https://en.wikipedia.org/wiki/DuckTales:_Remastered']) | DuckTales: Remastered | - | - | -- -- -- | -- -- -- | -- -- --
| [DND:C](['https://en.wikipedia.org/wiki/Dungeons_%26_Dragons:_Tower_of_Doom#Dungeons_&_Dragons_Collection']) | Dungeons & Dragons Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [DMC:CoM](['https://store.steampowered.com/app/229480', 'https://en.wikipedia.org/wiki/Dungeons_%26_Dragons:_Chronicles_of_Mystara']) | Dungeons & Dragons: Chronicles of Mystara | - | - | -- -- -- | -- -- -- | -- -- --
| [DMC:ToD](['https://en.wikipedia.org/wiki/Dungeons_%26_Dragons:_Tower_of_Doom']) | Dungeons & Dragons: Tower of Doom | - | - | -- -- -- | -- -- -- | -- -- --
| [Dustforce](['https://en.wikipedia.org/wiki/Dustforce']) | Dustforce | - | - | -- -- -- | -- -- -- | -- -- --
| [Dynasty Wars](['https://en.wikipedia.org/wiki/Dynasty_Wars']) | Dynasty Wars | - | - | -- -- -- | -- -- -- | -- -- --
| **Capcom+E-L** | **None**
| [EXTroopers](['https://en.wikipedia.org/wiki/E.X._Troopers']) | E.X. Troopers | - | - | -- -- -- | -- -- -- | -- -- --
| [EcoFighters](['https://en.wikipedia.org/wiki/Eco_Fighters']) | Eco Fighters | - | - | -- -- -- | -- -- -- | -- -- --
| [EDG:V1](['https://en.wikipedia.org/wiki/El_Dorado_Gate']) | El Dorado Gate Volume 1 | - | - | -- -- -- | -- -- -- | -- -- --
| [EDG:V2](['https://en.wikipedia.org/wiki/El_Dorado_Gate']) | El Dorado Gate Volume 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [EDG:V3](['https://en.wikipedia.org/wiki/El_Dorado_Gate']) | El Dorado Gate Volume 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [EDG:V4](['https://en.wikipedia.org/wiki/El_Dorado_Gate']) | El Dorado Gate Volume 4 | - | - | -- -- -- | -- -- -- | -- -- --
| [EDG:V5](['https://en.wikipedia.org/wiki/19XX:_The_War_Against_Destiny']) | El Dorado Gate Volume 5 | - | - | -- -- -- | -- -- -- | -- -- --
| [EDG:V6](['https://en.wikipedia.org/wiki/El_Dorado_Gate']) | El Dorado Gate Volume 6 | - | - | -- -- -- | -- -- -- | -- -- --
| [EDG:V7](['https://en.wikipedia.org/wiki/El_Dorado_Gate']) | El Dorado Gate Volume 7 | - | - | -- -- -- | -- -- -- | -- -- --
| [EtherVapor:R]([]) | Ether Vapor Remaster | - | - | -- -- -- | -- -- -- | -- -- --
| [Everblue](['https://en.wikipedia.org/wiki/Everblue']) | Everblue | - | - | -- -- -- | -- -- -- | -- -- --
| [Everblue2](['https://en.wikipedia.org/wiki/Everblue_2']) | Everblue 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [ExedExes](['https://en.wikipedia.org/wiki/Exed_Exes']) | Exed Exes | - | - | -- -- -- | -- -- -- | -- -- --
| [Exoprimal](['https://en.wikipedia.org/wiki/Exoprimal']) | Exoprimal | - | - | -- -- -- | -- -- -- | -- -- --
| [EOTB](['https://en.wikipedia.org/wiki/Eye_of_the_Beholder_(video_game)']) | Eye of the Beholder | - | - | -- -- -- | -- -- -- | -- -- --
| [F1Dream](['https://en.wikipedia.org/wiki/F-1_Dream']) | F-1 Dream | - | - | -- -- -- | -- -- -- | -- -- --
| [FairyBloomFreesia]([]) | Fairy Bloom Freesia | - | - | -- -- -- | -- -- -- | -- -- --
| [Fate:TC](['https://en.wikipedia.org/wiki/Fate/tiger_colosseum']) | Fate/tiger colosseum | - | - | -- -- -- | -- -- -- | -- -- --
| [Fate:UC](['https://en.wikipedia.org/wiki/Fate/unlimited_codes']) | Fate/unlimited codes | - | - | -- -- -- | -- -- -- | -- -- --
| [FeverChance]([]) | Fever Chance | - | - | -- -- -- | -- -- -- | -- -- --
| [FightingStreet](['https://en.wikipedia.org/wiki/Fighting_Street']) | Fighting Street | - | - | -- -- -- | -- -- -- | -- -- --
| [FF](['https://en.wikipedia.org/wiki/Final_Fight_(video_game)']) | Final Fight | - | - | -- -- -- | -- -- -- | -- -- --
| [FF2](['https://en.wikipedia.org/wiki/Final_Fight_2']) | Final Fight 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [FF3](['https://en.wikipedia.org/wiki/Final_Fight_3']) | Final Fight 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [FF:G](['https://en.wikipedia.org/wiki/Final_Fight_(video_game)#Super_NES_(Final_Fight_and_Final_Fight_Guy)']) | Final Fight Guy | - | - | -- -- -- | -- -- -- | -- -- --
| [FF:O](['https://en.wikipedia.org/wiki/Final_Fight_(video_game)#Game_Boy_Advance_(Final_Fight_One)']) | Final Fight One | - | - | -- -- -- | -- -- -- | -- -- --
| [FF:R](['https://en.wikipedia.org/wiki/Final_Fight_Revenge']) | Final Fight Revenge | - | - | -- -- -- | -- -- -- | -- -- --
| [FF:DI](['https://en.wikipedia.org/wiki/Final_Fight:_Double_Impact']) | Final Fight: Double Impact | - | - | -- -- -- | -- -- -- | -- -- --
| [FF:S](['https://en.wikipedia.org/wiki/Final_Fight:_Streetwise']) | Final Fight: Streetwise | - | - | -- -- -- | -- -- -- | -- -- --
| [FinderLove](['https://en.wikipedia.org/wiki/Finder_Love']) | Finder Love | - | - | -- -- -- | -- -- -- | -- -- --
| [FlipperFootball]([]) | Flipper Football | - | - | -- -- -- | -- -- -- | -- -- --
| [Flock](['https://store.steampowered.com/app/21640', 'https://en.wikipedia.org/wiki/Flock!']) | Flock! | - | - | -- -- -- | -- -- -- | -- -- --
| [ForgottenWorlds](['https://en.wikipedia.org/wiki/Forgotten_Worlds']) | Forgotten Worlds | - | - | -- -- -- | -- -- -- | -- -- --
| [FoxHunt](['https://en.wikipedia.org/wiki/Fox_Hunt_(video_game)']) | Fox Hunt | - | - | -- -- -- | -- -- -- | -- -- --
| [FushigiDeka]([]) | Fushigi Deka | - | - | -- -- -- | -- -- -- | -- -- --
| [GIJ:TAF](['https://en.wikipedia.org/wiki/G.I._Joe:_The_Atlantis_Factor']) | G.I. Joe: The Atlantis Factor | - | - | -- -- -- | -- -- -- | -- -- --
| [Gaia:SD]([]) | Gaia Master Kessen!: Seikiou Densetsu | - | - | -- -- -- | -- -- -- | -- -- --
| [Gaia:KBG]([]) | Gaia Master: Kami no Board Game | - | - | -- -- -- | -- -- -- | -- -- --
| [GaistCrusher](['https://en.wikipedia.org/wiki/Gaist_Crusher']) | Gaist Crusher | - | - | -- -- -- | -- -- -- | -- -- --
| [Gakkou:HGK]([]) | Gakkou no Kowai Uwasa: Hanako-san ga Kita!! | - | - | -- -- -- | -- -- -- | -- -- --
| [GargoyleQuest](['https://en.wikipedia.org/wiki/Gargoyle%27s_Quest']) | Gargoyle's Quest | - | - | -- -- -- | -- -- -- | -- -- --
| [GargoyleQuest2](['https://en.wikipedia.org/wiki/Gargoyle%27s_Quest_II']) | Gargoyle's Quest II | - | - | -- -- -- | -- -- -- | -- -- --
| [GenmaOnimusha](['https://en.wikipedia.org/wiki/Genma_Onimusha']) | Genma Onimusha | - | - | -- -- -- | -- -- -- | -- -- --
| [GNG](['https://en.wikipedia.org/wiki/Ghosts_%27n_Goblins_(video_game)']) | Ghosts 'n Goblins | - | - | -- -- -- | -- -- -- | -- -- --
| [GNG:GK](['https://en.wikipedia.org/wiki/Ghosts_%27n_Goblins:_Gold_Knights']) | Ghosts 'n Goblins: Gold Knights | - | - | -- -- -- | -- -- -- | -- -- --
| [GNG:GK2](['https://en.wikipedia.org/wiki/Ghosts_%27n_Goblins:_Gold_Knights']) | Ghosts 'n Goblins: Gold Knights II | - | - | -- -- -- | -- -- -- | -- -- --
| [GhoulsGhosts](['https://en.wikipedia.org/wiki/Ghouls_%27n_Ghosts']) | Ghouls 'n Ghosts | - | - | -- -- -- | -- -- -- | -- -- --
| [GigaWing](['https://en.wikipedia.org/wiki/Giga_Wing']) | Giga Wing | - | - | -- -- -- | -- -- -- | -- -- --
| [GigaWing2](['https://en.wikipedia.org/wiki/Giga_Wing_2']) | Giga Wing 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [GioGio:BA](['https://en.wikipedia.org/wiki/GioGio%27s_Bizarre_Adventure']) | GioGio's Bizarre Adventure | - | - | -- -- -- | -- -- -- | -- -- --
| [GlassRose](['https://en.wikipedia.org/wiki/Glass_Rose']) | Glass Rose | - | - | -- -- -- | -- -- -- | -- -- --
| [GodHand](['https://en.wikipedia.org/wiki/God_Hand']) | God Hand | - | - | -- -- -- | -- -- -- | -- -- --
| [GOW](['https://en.wikipedia.org/wiki/God_of_War_(2005_video_game)']) | God of War | - | - | -- -- -- | -- -- -- | -- -- --
| [GOW2](['https://en.wikipedia.org/wiki/God_of_War_II']) | God of War II | - | - | -- -- -- | -- -- -- | -- -- --
| [GOW:C](['https://en.wikipedia.org/wiki/God_of_War_Collection']) | God of War Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [GoldMedalChallenge](['https://en.wikipedia.org/wiki/Gold_Medal_Challenge']) | Gold Medal Challenge | - | - | -- -- -- | -- -- -- | -- -- --
| [GoofTroop](['https://en.wikipedia.org/wiki/Goof_Troop_(video_game)']) | Goof Troop | - | - | -- -- -- | -- -- -- | -- -- --
| [GotchaForce](['https://en.wikipedia.org/wiki/Gotcha_Force']) | Gotcha Force | - | - | -- -- -- | -- -- -- | -- -- --
| [GTA3](['https://en.wikipedia.org/wiki/Grand_Theft_Auto_III']) | Grand Theft Auto III | - | - | -- -- -- | -- -- -- | -- -- --
| [GTA:VC](['https://en.wikipedia.org/wiki/Grand_Theft_Auto:_Vice_City']) | Grand Theft Auto: Vice City | - | - | -- -- -- | -- -- -- | -- -- --
| [GTA:SA](['https://en.wikipedia.org/wiki/Grand_Theft_Auto:_San_Andreas']) | Grand Theft Auto: San Andreas | - | - | -- -- -- | -- -- -- | -- -- --
| [GTA4](['https://en.wikipedia.org/wiki/Grand_Theft_Auto_IV']) | Grand Theft Auto IV | - | - | -- -- -- | -- -- -- | -- -- --
| [GTA:LCS](['https://en.wikipedia.org/wiki/Grand_Theft_Auto:_Liberty_City_Stories']) | Grand Theft Auto: Liberty City Stories | - | - | -- -- -- | -- -- -- | -- -- --
| [GTA:VCS](['https://en.wikipedia.org/wiki/Grand_Theft_Auto:_Vice_City_Stories']) | Grand Theft Auto: Vice City Stories | - | - | -- -- -- | -- -- -- | -- -- --
| [GregoryHorrorShow](['https://en.wikipedia.org/wiki/Gregory_Horror_Show_(video_game)']) | Gregory Horror Show | - | - | -- -- -- | -- -- -- | -- -- --
| [Group S Challenge](['https://en.wikipedia.org/wiki/Group_S_Challenge']) | Group S Challenge | - | - | -- -- -- | -- -- -- | -- -- --
| [GunSmoke](['https://en.wikipedia.org/wiki/Gun.Smoke']) | Gun.Smoke | - | - | -- -- -- | -- -- -- | -- -- --
| [GyakutenKenji2](['https://en.wikipedia.org/wiki/Gyakuten_Kenji_2']) | Gyakuten Kenji 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [HarveyBirdman:AaL](['https://en.wikipedia.org/wiki/Capcom_Generations#Capcom_Generations_3:_The_First_Generation']) | Harvey Birdman: Attorney at Law | - | - | -- -- -- | -- -- -- | -- -- --
| [HatTrick](['https://en.wikipedia.org/wiki/Hat_Trick_(arcade_game)']) | Hat Trick | - | - | -- -- -- | -- -- -- | -- -- --
| [Haunting Ground](['https://en.wikipedia.org/wiki/Haunting_Ground']) | Haunting Ground | - | - | -- -- -- | -- -- -- | -- -- --
| [HeavyMetal:G](['https://en.wikipedia.org/wiki/Heavy_Metal:_Geomatrix']) | Heavy Metal: Geomatrix | - | - | -- -- -- | -- -- -- | -- -- --
| [HigemaruMakaijima:NSD](['https://en.wikipedia.org/wiki/Higemaru_Makaijima_-_Nanatsu_no_Shima_Daib%C5%8Dken']) | Higemaru Makaijima - Nanatsu no Shima Daibōken | - | - | -- -- -- | -- -- -- | -- -- --
| [HSF2:TA](['https://en.wikipedia.org/wiki/Hyper_Street_Fighter_II:_The_Anniversary_Edition']) | Hyper Street Fighter II: The Anniversary Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [Mahjong:IYJ]([]) | Ide no Yosuke no Jissen Mahjong | - | - | -- -- -- | -- -- -- | -- -- --
| [Mahjong2:IYJ]([]) | Ide no Yosuke no Jissen Mahjong II | - | - | -- -- -- | -- -- -- | -- -- --
| [Mahjong:IYMSJ]([]) | Ide Yousuke Meijin no Shinmi Jissen Mahjong | - | - | -- -- -- | -- -- -- | -- -- --
| [JoJo](['https://en.wikipedia.org/wiki/JoJo%27s_Bizarre_Adventure_(video_game)']) | JoJo's Venture | - | - | -- -- -- | -- -- -- | -- -- --
| [JoJo:HD](['https://en.wikipedia.org/wiki/JoJo%27s_Bizarre_Adventure_(video_game)']) | JoJo's Bizarre Adventure HD | - | - | -- -- -- | -- -- -- | -- -- --
| [JoJo:HftF](['https://en.wikipedia.org/wiki/Capcom_vs._SNK_2']) | JoJo's Bizarre Adventure: Heritage for the Future | - | - | -- -- -- | -- -- -- | -- -- --
| [KabuTraderShun](['https://en.wikipedia.org/wiki/Kabu_Trader_Shun']) | Kabu Trader Shun | - | - | -- -- -- | -- -- -- | -- -- --
| [KenKen:TYB]([]) | KenKen: Train Your Brain | - | - | -- -- -- | -- -- -- | -- -- --
| [Killer7](['https://en.wikipedia.org/wiki/Killer7']) | killer7 | - | - | -- -- -- | -- -- -- | -- -- --
| [Kingpin]([]) | Kingpin | - | - | -- -- -- | -- -- -- | -- -- --
| [Knights of the Round](['https://en.wikipedia.org/wiki/Knights_of_the_Round_(video_game)']) | Knights of the Round | - | - | -- -- -- | -- -- -- | -- -- --
| [Kunitsu:PotG]([]) | Kunitsu-Gami: Path of the Goddess | - | - | -- -- -- | -- -- -- | -- -- --
| [Kyojin:H](['https://en.wikipedia.org/wiki/Kyojin_no_Hoshi']) | Kyojin no Hoshi | - | - | -- -- -- | -- -- -- | -- -- --
| [LastDuel:IPW](['https://en.wikipedia.org/wiki/Last_Duel_(video_game)']) | Last Duel: Inter Planet War 2012 | - | - | -- -- -- | -- -- -- | -- -- --
| [LastRanker](['https://en.wikipedia.org/wiki/Last_Ranker']) | Last Ranker | - | - | -- -- -- | -- -- -- | -- -- --
| [LaytonVGyakuten](['https://en.wikipedia.org/wiki/Layton-kyoju_VS_Gyakuten_Saiban']) | Layton-kyoju VS Gyakuten Saiban | - | - | -- -- -- | -- -- -- | -- -- --
| [LEDStorm]([]) | LED Storm | - | - | -- -- -- | -- -- -- | -- -- --
| [LegendKay](['https://en.wikipedia.org/wiki/Legend_of_Kay']) | Legend of Kay | - | - | -- -- -- | -- -- -- | -- -- --
| [LegendaryWings](['https://en.wikipedia.org/wiki/Legendary_Wings']) | Legendary Wings | - | - | -- -- -- | -- -- -- | -- -- --
| [LilPirates]([]) | Lil' Pirates | - | - | -- -- -- | -- -- -- | -- -- --
| [LittleLeague](['https://en.wikipedia.org/wiki/Little_League']) | Little League | - | - | -- -- -- | -- -- -- | -- -- --
| [LittleNemo:TDM](['https://en.wikipedia.org/wiki/Little_Nemo:_The_Dream_Master']) | Little Nemo: The Dream Master | - | - | -- -- -- | -- -- -- | -- -- --
| [LP:EC](['https://store.steampowered.com/app/6510', 'https://en.wikipedia.org/wiki/Lost_Planet:_Extreme_Condition']) | Lost Planet: Extreme Condition | - | - | -- -- -- | -- -- -- | -- -- --
| [LP:C](['https://en.wikipedia.org/wiki/Lost_Planet:_Extreme_Condition#Collector.27s_and_Colonies_Edition']) | Lost Planet: Colonies | - | - | -- -- -- | -- -- -- | -- -- --
| [LP2](['https://en.wikipedia.org/wiki/Lost_Planet_2']) | Lost Planet 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [LP3](['https://store.steampowered.com/app/226720', 'https://en.wikipedia.org/wiki/Lost_Planet_3']) | Lost Planet 3 | - | - | -- -- -- | -- -- -- | -- -- --
| **Capcom+M** | **None**
| [MagicSword](['https://en.wikipedia.org/wiki/Magic_Sword_(video_game)']) | Magic Sword | - | - | -- -- -- | -- -- -- | -- -- --
| [Tetris:MC](['https://en.wikipedia.org/wiki/Magical_Tetris_Challenge']) | Magical Tetris Challenge | - | - | -- -- -- | -- -- -- | -- -- --
| [Mahjong:G]([]) | Mahjong Gakuen | - | - | -- -- -- | -- -- -- | -- -- --
| [Makaimura:FW](['https://en.wikipedia.org/wiki/Makaimura_for_WonderSwan']) | Makaimura for WonderSwan | - | - | -- -- -- | -- -- -- | -- -- --
| [MarsMatrix:HSS](['https://en.wikipedia.org/wiki/Mars_Matrix:_Hyper_Solid_Shooting']) | Mars Matrix: Hyper Solid Shooting | - | - | -- -- -- | -- -- -- | -- -- --
| [Marusa:O](['https://en.wikipedia.org/wiki/Marusa_no_Onna']) | Marusa no Onna | - | - | -- -- -- | -- -- -- | -- -- --
| [MSH](['https://en.wikipedia.org/wiki/Marvel_Super_Heroes_(video_game)']) | Marvel Super Heroes | - | - | -- -- -- | -- -- -- | -- -- --
| [MSHVSF](['https://en.wikipedia.org/wiki/Marvel_Super_Heroes_vs._Street_Fighter']) | Marvel Super Heroes vs. Street Fighter | - | - | -- -- -- | -- -- -- | -- -- --
| [MSHVSF:EX](['https://en.wikipedia.org/wiki/Marvel_Super_Heroes_vs._Street_Fighter']) | Marvel Super Heroes vs. Street Fighter EX Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [MSH:IWotG](['https://en.wikipedia.org/wiki/Marvel_Super_Heroes_In_War_of_the_Gems']) | Marvel Super Heroes In War of the Gems | - | - | -- -- -- | -- -- -- | -- -- --
| [MVC](['https://en.wikipedia.org/wiki/Marvel_vs._Capcom:_Clash_of_Super_Heroes']) | Marvel vs. Capcom | - | - | -- -- -- | -- -- -- | -- -- --
| [MVC2](['https://en.wikipedia.org/wiki/Marvel_vs._Capcom_2']) | Marvel vs. Capcom 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [MVC2:NAoH](['https://en.wikipedia.org/wiki/Marvel_vs._Capcom_2:_New_Age_of_Heroes']) | Marvel vs. Capcom 2: New Age of Heroes | - | - | -- -- -- | -- -- -- | -- -- --
| [MVC3:AFoTW](['https://en.wikipedia.org/wiki/Marvel_vs._Capcom_3:_Fate_of_Two_Worlds']) | Marvel vs. Capcom 3: Fate of Two Worlds | - | - | -- -- -- | -- -- -- | -- -- --
| [MVC:CoSH](['https://en.wikipedia.org/wiki/Marvel_vs._Capcom:_Clash_of_Super_Heroes']) | Marvel vs. Capcom: Clash of Super Heroes | - | - | -- -- -- | -- -- -- | -- -- --
| [MVC:CoSH:EX](['https://en.wikipedia.org/wiki/Marvel_vs._Capcom:_Clash_of_Super_Heroes']) | Marvel vs. Capcom Clash of Super Heroes EX Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [MVC:I](['https://store.steampowered.com/app/493840', 'https://en.wikipedia.org/wiki/Marvel_vs._Capcom:_Infinite']) | Marvel vs. Capcom: Infinite | - | - | -- -- -- | -- -- -- | -- -- --
| [MVC:O](['https://en.wikipedia.org/wiki/Marvel_vs._Capcom_Origins']) | Marvel vs. Capcom Origins | - | - | -- -- -- | -- -- -- | -- -- --
| [Maximo:AoZ](['https://en.wikipedia.org/wiki/Maximo_vs._Army_of_Zin']) | Maximo vs. Army of Zin | - | - | -- -- -- | -- -- -- | -- -- --
| [Maximo:GtG](['https://en.wikipedia.org/wiki/Maximo:_Ghosts_to_Glory']) | Maximo: Ghosts to Glory | - | - | -- -- -- | -- -- -- | -- -- --
| [MaXplosion](['https://en.wikipedia.org/wiki/%27Splosion_Man#Controversy']) | MaXplosion | - | - | -- -- -- | -- -- -- | -- -- --
| [MM](['https://en.wikipedia.org/wiki/Mega_Man_(video_game)']) | Mega Man | - | - | -- -- -- | -- -- -- | -- -- --
| [MM:B](['https://en.wikipedia.org/wiki/Mega_Man_%26_Bass']) | Mega Man & Bass | - | - | -- -- -- | -- -- -- | -- -- --
| [MM2](['https://en.wikipedia.org/wiki/Mega_Man_2']) | Mega Man 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [MM2:TPF](['https://en.wikipedia.org/wiki/Mega_Man_2:_The_Power_Fighters']) | Mega Man 2: The Power Fighters | - | - | -- -- -- | -- -- -- | -- -- --
| [MM3](['https://en.wikipedia.org/wiki/Mega_Man_3']) | Mega Man 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [MM4](['https://en.wikipedia.org/wiki/Mega_Man_4']) | Mega Man 4 | - | - | -- -- -- | -- -- -- | -- -- --
| [MM5](['https://en.wikipedia.org/wiki/Mega_Man_5']) | Mega Man 5 | - | - | -- -- -- | -- -- -- | -- -- --
| [MM6](['https://en.wikipedia.org/wiki/Mega_Man_6']) | Mega Man 6 | - | - | -- -- -- | -- -- -- | -- -- --
| [MM:64](['https://en.wikipedia.org/wiki/Mega_Man_64']) | Mega Man 64 | - | - | -- -- -- | -- -- -- | -- -- --
| [MM7](['https://en.wikipedia.org/wiki/Mega_Man_7']) | Mega Man 7 | - | - | -- -- -- | -- -- -- | -- -- --
| [MM8](['https://en.wikipedia.org/wiki/Mega_Man_8']) | Mega Man 8 | - | - | -- -- -- | -- -- -- | -- -- --
| [MM9](['https://en.wikipedia.org/wiki/Mega_Man_9']) | Mega Man 9 | - | - | -- -- -- | -- -- -- | -- -- --
| [MM10](['https://en.wikipedia.org/wiki/Mega_Man_10']) | Mega Man 10 | - | - | -- -- -- | -- -- -- | -- -- --
| [MM11](['https://store.steampowered.com/app/742300', 'https://en.wikipedia.org/wiki/Mega_Man_11']) | Mega Man 11 | - | - | -- -- -- | -- -- -- | -- -- --
| [MM:AC](['https://en.wikipedia.org/wiki/Mega_Man_Anniversary_Collection']) | Mega Man Anniversary Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [MMB:C](['https://en.wikipedia.org/wiki/Mega_Man_Battle_%26_Chase']) | Mega Man Battle & Chase | - | - | -- -- -- | -- -- -- | -- -- --
| [MMB:CC](['https://en.wikipedia.org/wiki/Mega_Man_Battle_Chip_Challenge']) | Mega Man Battle Chip Challenge | - | - | -- -- -- | -- -- -- | -- -- --
| [MMB:N](['https://en.wikipedia.org/wiki/Mega_Man_Battle_Network_(video_game)']) | Mega Man Battle Network | - | - | -- -- -- | -- -- -- | -- -- --
| [MMB:N2](['https://en.wikipedia.org/wiki/Mega_Man_Battle_Network_2']) | Mega Man Battle Network 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [MMB:N3](['https://en.wikipedia.org/wiki/Mega_Man_Battle_Network_3']) | Mega Man Battle Network 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [MMB:N4](['https://en.wikipedia.org/wiki/Mega_Man_Battle_Network_4']) | Mega Man Battle Network 4 | - | - | -- -- -- | -- -- -- | -- -- --
| [MMB:N5](['https://en.wikipedia.org/wiki/Mega_Man_Battle_Network_5']) | Mega Man Battle Network 5 | - | - | -- -- -- | -- -- -- | -- -- --
| [MMB:N6](['https://en.wikipedia.org/wiki/Mega_Man_Battle_Network_6']) | Mega Man Battle Network 6 | - | - | -- -- -- | -- -- -- | -- -- --
| [MM:DWR](['https://en.wikipedia.org/wiki/Mega_Man:_Dr._Wily%27s_Revenge']) | Mega Man: Dr. Wily's Revenge | - | - | -- -- -- | -- -- -- | -- -- --
| [MMII](['https://en.wikipedia.org/wiki/Mega_Man_II_(Game_Boy)']) | Mega Man II | - | - | -- -- -- | -- -- -- | -- -- --
| [MMIII](['https://en.wikipedia.org/wiki/Mega_Man_III_(Game_Boy)']) | Mega Man III | - | - | -- -- -- | -- -- -- | -- -- --
| [MMIV](['https://en.wikipedia.org/wiki/Mega_Man_IV_(Game_Boy)']) | Mega Man IV | - | - | -- -- -- | -- -- -- | -- -- --
| [MML](['https://en.wikipedia.org/wiki/Mega_Man_Legends_(video_game)']) | Mega Man Legends | - | - | -- -- -- | -- -- -- | -- -- --
| [MML2](['https://en.wikipedia.org/wiki/Mega_Man_Legends_2']) | Mega Man Legends 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [MML3](['https://en.wikipedia.org/wiki/Mega_Man_Legends_3']) | Mega Man Legends 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [MM:M](['https://en.wikipedia.org/wiki/Mega_Man_Mania']) | Mega Man Mania | - | - | -- -- -- | -- -- -- | -- -- --
| [MM:NT](['https://en.wikipedia.org/wiki/Mega_Man_Network_Transmission']) | Mega Man Network Transmission | - | - | -- -- -- | -- -- -- | -- -- --
| [MM:PU](['https://en.wikipedia.org/wiki/Mega_Man_Powered_Up']) | Mega Man Powered Up | - | - | -- -- -- | -- -- -- | -- -- --
| [MM:S](['https://en.wikipedia.org/wiki/Mega_Man_Soccer']) | Mega Man Soccer | - | - | -- -- -- | -- -- -- | -- -- --
| [MMSF](['https://en.wikipedia.org/wiki/Mega_Man_Star_Force']) | Mega Man Star Force | - | - | -- -- -- | -- -- -- | -- -- --
| [MMSF2](['https://en.wikipedia.org/wiki/Mega_Man_Star_Force_2']) | Mega Man Star Force 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [MMSF3](['https://en.wikipedia.org/wiki/Mega_Man_Star_Force_3']) | Mega Man Star Force 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [MM:U](['https://en.wikipedia.org/wiki/Mega_Man_Universe']) | Mega Man Universe | - | - | -- -- -- | -- -- -- | -- -- --
| [MMV](['https://en.wikipedia.org/wiki/Mega_Man_V_(Game_Boy)']) | Mega Man V | - | - | -- -- -- | -- -- -- | -- -- --
| [MMX](['https://en.wikipedia.org/wiki/Mega_Man_X_(video_game)']) | Mega Man X | - | - | -- -- -- | -- -- -- | -- -- --
| [MMX:C](['https://en.wikipedia.org/wiki/Mega_Man_X_Collection']) | Mega Man X Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [MMX:CM](['https://en.wikipedia.org/wiki/Mega_Man_X:_Command_Mission']) | Mega Man X: Command Mission | - | - | -- -- -- | -- -- -- | -- -- --
| [MMX:LC](['https://store.steampowered.com/app/743890', 'https://en.wikipedia.org/wiki/Mega_Man_X_Legacy_Collection']) | Mega Man X Legacy Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [MMX:LC2](['https://store.steampowered.com/app/743900', 'https://en.wikipedia.org/wiki/Mega_Man_X_Legacy_Collection_2']) | Mega Man X Legacy Collection 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [MMX2](['https://en.wikipedia.org/wiki/Mega_Man_X2']) | Mega Man X2 | - | - | -- -- -- | -- -- -- | -- -- --
| [MMX3](['https://en.wikipedia.org/wiki/Mega_Man_X3']) | Mega Man X3 | - | - | -- -- -- | -- -- -- | -- -- --
| [MMX4](['https://en.wikipedia.org/wiki/Mega_Man_X4']) | Mega Man X4 | - | - | -- -- -- | -- -- -- | -- -- --
| [MMX5](['https://en.wikipedia.org/wiki/Mega_Man_X5']) | Mega Man X5 | - | - | -- -- -- | -- -- -- | -- -- --
| [MMX6](['https://en.wikipedia.org/wiki/Mega_Man_X6']) | Mega Man X6 | - | - | -- -- -- | -- -- -- | -- -- --
| [MMX7](['https://en.wikipedia.org/wiki/Mega_Man_X7']) | Mega Man X7 | - | - | -- -- -- | -- -- -- | -- -- --
| [MMX8](['https://en.wikipedia.org/wiki/Mega_Man_X8']) | Mega Man X8 | - | - | -- -- -- | -- -- -- | -- -- --
| [MM:X](['https://en.wikipedia.org/wiki/Mega_Man_Xtreme']) | Mega Man Xtreme | - | - | -- -- -- | -- -- -- | -- -- --
| [MM:X2](['https://en.wikipedia.org/wiki/Mega_Man_Xtreme_2']) | Mega Man Xtreme 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [MMZ](['https://en.wikipedia.org/wiki/Mega_Man_Zero_(video_game)']) | Mega Man Zero | - | - | -- -- -- | -- -- -- | -- -- --
| [MMZ2](['https://en.wikipedia.org/wiki/Mega_Man_Zero_2']) | Mega Man Zero 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [MMZ3](['https://en.wikipedia.org/wiki/Mega_Man_Zero_3']) | Mega Man Zero 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [MMZ4](['https://en.wikipedia.org/wiki/Mega_Man_Zero_4']) | Mega Man Zero 4 | - | - | -- -- -- | -- -- -- | -- -- --
| [MMZ:C](['https://en.wikipedia.org/wiki/Mega_Man_Zero_Collection']) | Mega Man Zero Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [MMZX](['https://en.wikipedia.org/wiki/Mega_Man_ZX']) | Mega Man ZX | - | - | -- -- -- | -- -- -- | -- -- --
| [MMZX:A](['https://en.wikipedia.org/wiki/Mega_Man_ZX_Advent']) | Mega Man ZX Advent | - | - | -- -- -- | -- -- -- | -- -- --
| [MM:MHX](['https://en.wikipedia.org/wiki/Mega_Man:_Maverick_Hunter_X']) | Mega Man: Maverick Hunter X | - | - | -- -- -- | -- -- -- | -- -- --
| [MM:TPB](['https://en.wikipedia.org/wiki/Mega_Man:_The_Power_Battle']) | Mega Man: The Power Battle | - | - | -- -- -- | -- -- -- | -- -- --
| [MM:TWW](['https://en.wikipedia.org/wiki/Mega_Man:_The_Wily_Wars']) | Mega Man: The Wily Wars | - | - | -- -- -- | -- -- -- | -- -- --
| [MegaTwins](['https://en.wikipedia.org/wiki/Mega_Twins']) | Mega Twins | - | - | -- -- -- | -- -- -- | -- -- --
| [Mercs](['https://en.wikipedia.org/wiki/Mercs']) | Mercs | - | - | -- -- -- | -- -- -- | -- -- --
| [MetalWalker](['https://en.wikipedia.org/wiki/Metal_Walker']) | Metal Walker | - | - | -- -- -- | -- -- -- | -- -- --
| [Mickey:M](['https://en.wikipedia.org/wiki/Mickey_Mousecapade']) | Mickey Mousecapade | - | - | -- -- -- | -- -- -- | -- -- --
| [Mickey:DC](['https://en.wikipedia.org/wiki/Mickey%27s_Dangerous_Chase']) | Mickey's Dangerous Chase | - | - | -- -- -- | -- -- -- | -- -- --
| [MightyFinalFight](['https://en.wikipedia.org/wiki/Mighty_Final_Fight']) | Mighty Final Fight | - | - | -- -- -- | -- -- -- | -- -- --
| [MTWI](['https://en.wikipedia.org/wiki/Minute_to_Win_It']) | Minute to Win It | - | - | -- -- -- | -- -- -- | -- -- --
| [Mizushima:D]([]) | Mizushima Shinji no Daikoushien | - | - | -- -- -- | -- -- -- | -- -- --
| [MSG:FVZA](['https://en.wikipedia.org/wiki/Gundam_Seed:_Rengou_vs._Z.A.F.T.']) | Mobile Suit Gundam SEED: Federation vs ZAFT | - | - | -- -- -- | -- -- -- | -- -- --
| [MSG:AVT]([]) | Mobile Suit Gundam: AEUG Vs Titans | - | - | -- -- -- | -- -- -- | -- -- --
| [MSG:FVZE](['https://en.wikipedia.org/wiki/Mobile_Suit_Gundam:_Federation_vs._Zeon']) | Mobile Suit Gundam: Federation vs. Zeon | - | - | -- -- -- | -- -- -- | -- -- --
| [MSG:FVZE:DX](['https://en.wikipedia.org/wiki/List_of_Gundam_video_games#Dreamcast']) | Mobile Suit Gundam: Federation vs. Zeon DX | - | - | -- -- -- | -- -- -- | -- -- --
| [MH](['https://en.wikipedia.org/wiki/Monster_Hunter_(video_game)']) | Monster Hunter | - | - | -- -- -- | -- -- -- | -- -- --
| [MH2](['https://en.wikipedia.org/wiki/Monster_Hunter_2']) | Monster Hunter 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [MH3](['https://en.wikipedia.org/wiki/Monster_Hunter_Tri#Monster_Hunter_3_Ultimate']) | Monster Hunter 3 Ultimate | - | - | -- -- -- | -- -- -- | -- -- --
| [MH4](['https://en.wikipedia.org/wiki/Monster_Hunter_4']) | Monster Hunter 4 | - | - | -- -- -- | -- -- -- | -- -- --
| [MH:F](['https://en.wikipedia.org/wiki/Monster_Hunter_Freedom']) | Monster Hunter Freedom | - | - | -- -- -- | -- -- -- | -- -- --
| [MH:F2](['https://en.wikipedia.org/wiki/Monster_Hunter_Freedom_2']) | Monster Hunter Freedom 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [MH:FU](['https://en.wikipedia.org/wiki/Monster_Hunter_Freedom_Unite']) | Monster Hunter Freedom Unite | - | - | -- -- -- | -- -- -- | -- -- --
| [MH:FG](['https://en.wikipedia.org/wiki/Monster_Hunter_Frontier_G']) | Monster Hunter Frontier G | - | - | -- -- -- | -- -- -- | -- -- --
| [MHF:FO](['https://en.wikipedia.org/wiki/Monster_Hunter_Frontier_Online']) | Monster Hunter Frontier Online | - | - | -- -- -- | -- -- -- | -- -- --
| [MH:G](['https://en.wikipedia.org/wiki/Monster_Hunter_(video_game)#Monster_Hunter_G']) | Monster Hunter G | - | - | -- -- -- | -- -- -- | -- -- --
| [MH:GX](['https://en.wikipedia.org/wiki/Monster_Hunter_Generations']) | Monster Hunter Generations / Monster Hunter X | - | - | -- -- -- | -- -- -- | -- -- --
| [MH:GU](['https://en.wikipedia.org/wiki/Monster_Hunter_Generations']) | Monster Hunter Generations Ultimate | - | - | -- -- -- | -- -- -- | -- -- --
| [MH:P3](['https://en.wikipedia.org/wiki/Monster_Hunter_Portable_3rd']) | Monster Hunter Portable 3rd | - | - | -- -- -- | -- -- -- | -- -- --
| [MH:P3HD](['https://en.wikipedia.org/wiki/Monster_Hunter_Portable_3rd']) | Monster Hunter Portable 3rd HD ver. | - | - | -- -- -- | -- -- -- | -- -- --
| [MH:S](['https://en.wikipedia.org/wiki/Monster_Hunter_Stories']) | Monster Hunter Stories | - | - | -- -- -- | -- -- -- | -- -- --
| [MH:T](['https://en.wikipedia.org/wiki/Monster_Hunter_Tri']) | Monster Hunter Tri | - | - | -- -- -- | -- -- -- | -- -- --
| [MH:W](['https://store.steampowered.com/app/582010', 'https://en.wikipedia.org/wiki/Monster_Hunter:_World']) | Monster Hunter: World | - | - | -- -- -- | -- -- -- | -- -- --
| [MHXX](['https://en.wikipedia.org/wiki/Monster_Hunter_Generations']) | Monster Hunter XX | - | - | -- -- -- | -- -- -- | -- -- --
| [MHXX:NS](['https://en.wikipedia.org/wiki/Monster_Hunter_Generations']) | Monster Hunter XX: Nintendo Switch Ver. | - | - | -- -- -- | -- -- -- | -- -- --
| [MotoGP](['https://en.wikipedia.org/wiki/MotoGP_08']) | MotoGP | - | - | -- -- -- | -- -- -- | -- -- --
| [MotoGP07](['https://en.wikipedia.org/wiki/MotoGP_%2707_(PS2)']) | MotoGP '07 | - | - | -- -- -- | -- -- -- | -- -- --
| [MotoGP08](['https://en.wikipedia.org/wiki/MotoGP_%2708']) | MotoGP '08 | - | - | -- -- -- | -- -- -- | -- -- --
| [MotoGP09](['https://en.wikipedia.org/wiki/MotoGP_09/10']) | MotoGP 09/10 | - | - | -- -- -- | -- -- -- | -- -- --
| [MotoGP10](['https://en.wikipedia.org/wiki/MotoGP_10/11']) | MotoGP 10/11 | - | - | -- -- -- | -- -- -- | -- -- --
| [MrBill](['https://en.wikipedia.org/wiki/Mr._Bill']) | Mr. Bill | - | - | -- -- -- | -- -- -- | -- -- --
| [MB](['https://en.wikipedia.org/wiki/Muscle_Bomber']) | Muscle Bomber | - | - | -- -- -- | -- -- -- | -- -- --
| [MB2](['https://en.wikipedia.org/wiki/Muscle_Bomber']) | Muscle Bomber 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [MB:D](['https://en.wikipedia.org/wiki/Muscle_Bomber_Duo']) | Muscle Bomber Duo | - | - | -- -- -- | -- -- -- | -- -- --
| [MB:TBE](['https://en.wikipedia.org/wiki/Muscle_Bomber:_The_Body_Explosion']) | Muscle Bomber: The Body Explosion | - | - | -- -- -- | -- -- -- | -- -- --
| **Capcom+N-R** | **None**
| [NamcoxCapcom](['https://en.wikipedia.org/wiki/Namco_%C3%97_Capcom']) | Namco × Capcom | - | - | -- -- -- | -- -- -- | -- -- --
| [NazoWakuYakata]([]) | Nazo Waku Yakata | - | - | -- -- -- | -- -- -- | -- -- --
| [Nemo](['https://en.wikipedia.org/wiki/Nemo_(arcade_game)']) | Nemo | - | - | -- -- -- | -- -- -- | -- -- --
| [Tennis:N]([]) | Netto de Tennis | - | - | -- -- -- | -- -- -- | -- -- --
| [SMBW:CW](['https://en.wikipedia.org/wiki/New_Super_Mario_Bros._Wii#New_Super_Mario_Bros._Wii_Coin_World']) | New Super Mario Bros. Wii Coin World | - | - | -- -- -- | -- -- -- | -- -- --
| [NightWarrior:DR](['https://en.wikipedia.org/wiki/Night_Warriors:_Darkstalkers%27_Revenge']) | Night Warriors: Darkstalkers' Revenge | - | - | -- -- -- | -- -- -- | -- -- --
| [Okami](['https://en.wikipedia.org/wiki/%C5%8Ckami']) | Ōkami | - | - | -- -- -- | -- -- -- | -- -- --
| [Okami:HD](['https://store.steampowered.com/app/587620', 'https://en.wikipedia.org/wiki/%C5%8Ckami_HD']) | Ōkami HD | - | - | -- -- -- | -- -- -- | -- -- --
| [Okamiden](['https://en.wikipedia.org/wiki/%C5%8Ckamiden']) | Ōkamiden | - | - | -- -- -- | -- -- -- | -- -- --
| [OnePieceMansion](['https://en.wikipedia.org/wiki/One_Piece_Mansion']) | One Piece Mansion | - | - | -- -- -- | -- -- -- | -- -- --
| [O2:SD](['https://en.wikipedia.org/wiki/Onimusha_2:_Samurai%27s_Destiny']) | Onimusha 2: Samurai's Destiny | - | - | -- -- -- | -- -- -- | -- -- --
| [O3:DS](['https://en.wikipedia.org/wiki/Onimusha_3:_Demon_Siege']) | Onimusha 3: Demon Siege | - | - | -- -- -- | -- -- -- | -- -- --
| [O:BW](['https://en.wikipedia.org/wiki/Onimusha_Blade_Warriors']) | Onimusha Blade Warriors | - | - | -- -- -- | -- -- -- | -- -- --
| [O:DoD](['https://en.wikipedia.org/wiki/Onimusha:_Dawn_of_Dreams']) | Onimusha: Dawn of Dreams | - | - | -- -- -- | -- -- -- | -- -- --
| [O:S](['https://en.wikipedia.org/wiki/Onimusha_Soul']) | Onimusha Soul | - | - | -- -- -- | -- -- -- | -- -- --
| [O:T](['https://en.wikipedia.org/wiki/Onimusha_Tactics']) | Onimusha Tactics | - | - | -- -- -- | -- -- -- | -- -- --
| [O:W](['https://store.steampowered.com/app/761600', 'https://en.wikipedia.org/wiki/Onimusha:_Warlords']) | Onimusha: Warlords | - | - | -- -- -- | -- -- -- | -- -- --
| [OshieteFighter]([]) | Oshiete Fighter | - | - | -- -- -- | -- -- -- | -- -- --
| [PN03](['https://en.wikipedia.org/wiki/P.N.03']) | P.N.03 | - | - | -- -- -- | -- -- -- | -- -- --
| [PanicShot:R]([]) | Panic Shot! Rockman | - | - | -- -- -- | -- -- -- | -- -- --
| [PWAA](['https://en.wikipedia.org/wiki/Phoenix_Wright:_Ace_Attorney']) | Phoenix Wright: Ace Attorney | - | - | -- -- -- | -- -- -- | -- -- --
| [PWAA:DD](['https://en.wikipedia.org/wiki/Phoenix_Wright:_Ace_Attorney_%E2%88%92_Dual_Destinies']) | Phoenix Wright: Ace Attorney − Dual Destinies | - | - | -- -- -- | -- -- -- | -- -- --
| [PWAA:JfA](['https://en.wikipedia.org/wiki/Phoenix_Wright:_Ace_Attorney_%E2%88%92_Justice_for_All']) | Phoenix Wright: Ace Attorney − Justice for All | - | - | -- -- -- | -- -- -- | -- -- --
| [PWAA:SoJ](['https://en.wikipedia.org/wiki/Phoenix_Wright:_Ace_Attorney_%E2%88%92_Spirit_of_Justice']) | Phoenix Wright: Ace Attorney − Spirit of Justice | - | - | -- -- -- | -- -- -- | -- -- --
| [PWAA:TaT](['https://en.wikipedia.org/wiki/Phoenix_Wright:_Ace_Attorney_%E2%88%92_Trials_and_Tribulations']) | Phoenix Wright: Ace Attorney − Trials and Tribulations | - | - | -- -- -- | -- -- -- | -- -- --
| [Pinball:M]([]) | Pinball Magic | - | - | -- -- -- | -- -- -- | -- -- --
| [PirateShipHigemaru](['https://en.wikipedia.org/wiki/Pirate_Ship_Higemaru']) | Pirate Ship Higemaru | - | - | -- -- -- | -- -- -- | -- -- --
| [PlanetWork]([]) | Planet Work | - | - | -- -- -- | -- -- -- | -- -- --
| [PlasmaSword:NoB](['https://en.wikipedia.org/wiki/Plasma_Sword:_Nightmare_of_Bilstein']) | Plasma Sword: Nightmare of Bilstein | - | - | -- -- -- | -- -- -- | -- -- --
| [PocketFighter](['https://en.wikipedia.org/wiki/Pocket_Fighter']) | Pocket Fighter | - | - | -- -- -- | -- -- -- | -- -- --
| [PocketRockets]([]) | Pocket Rockets | - | - | -- -- -- | -- -- -- | -- -- --
| [PowerQuest](['https://en.wikipedia.org/wiki/Power_Quest_(video_game)']) | Power Quest | - | - | -- -- -- | -- -- -- | -- -- --
| [PowerStone](['https://en.wikipedia.org/wiki/Power_Stone_(video_game)']) | Power Stone | - | - | -- -- -- | -- -- -- | -- -- --
| [PowerStone2](['https://en.wikipedia.org/wiki/Power_Stone_2']) | Power Stone 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [PowerStone:C](['https://en.wikipedia.org/wiki/Power_Stone_(video_game)']) | Power Stone Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [Pragmata](['https://en.wikipedia.org/wiki/Pragmata']) | Pragmata | - | - | -- -- -- | -- -- -- | -- -- --
| [Fishing:PCS]([]) | Pro Cast Sports Fishing | - | - | -- -- -- | -- -- -- | -- -- --
| [PWAA:VPL](['https://en.wikipedia.org/wiki/Professor_Layton_vs._Phoenix_Wright:_Ace_Attorney']) | Professor Layton vs. Phoenix Wright: Ace Attorney | - | - | -- -- -- | -- -- -- | -- -- --
| [ProYakyuu:SJ]([]) | Pro Yakyuu? Satsujin Jiken! | - | - | -- -- -- | -- -- -- | -- -- --
| [Progear](['https://en.wikipedia.org/wiki/Progear']) | Progear | - | - | -- -- -- | -- -- -- | -- -- --
| [ProjectJustice](['https://en.wikipedia.org/wiki/Project_Justice']) | Project Justice | - | - | -- -- -- | -- -- -- | -- -- --
| [ProjectXZone](['https://en.wikipedia.org/wiki/Project_X_Zone']) | Project X Zone | - | - | -- -- -- | -- -- -- | -- -- --
| [ProjectXZone2](['https://en.wikipedia.org/wiki/Project_X_Zone_2']) | Project X Zone 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [PuzzLoop](['https://en.wikipedia.org/wiki/Puzz_Loop']) | Puzz Loop | - | - | -- -- -- | -- -- -- | -- -- --
| [PuzzLoop2](['https://en.wikipedia.org/wiki/Puzz_Loop_2']) | Puzz Loop 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [PuzzleFighter](['https://en.wikipedia.org/wiki/Puzzle_Fighter']) | Puzzle Fighter | - | - | -- -- -- | -- -- -- | -- -- --
| [Quiz:D](['https://en.wikipedia.org/wiki/Quiz_%26_Dragons:_Capcom_Quiz_Game']) | Quiz & Dragons: Capcom Quiz Game | - | - | -- -- -- | -- -- -- | -- -- --
| [Quiz:ND](['https://en.wikipedia.org/wiki/Quiz_Nanairo_Dreams']) | Quiz Nanairo Dreams | - | - | -- -- -- | -- -- -- | -- -- --
| [Quiz:NDNK](['https://en.wikipedia.org/wiki/Quiz_Nanairo_Dreams']) | Quiz Nanairo Dreams: Nijiiro-cho no Kiseki | - | - | -- -- -- | -- -- -- | -- -- --
| [Quiz:SGS]([]) | Quiz San Goku Shi | - | - | -- -- -- | -- -- -- | -- -- --
| [Quiz:TY2]([]) | Quiz Tonosama no Yabou 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [RDR](['https://en.wikipedia.org/wiki/Red_Dead_Revolver']) | Red Dead Revolver | - | - | -- -- -- | -- -- -- | -- -- --
| [RedEarth](['https://en.wikipedia.org/wiki/Red_Earth_(video_game)']) | Red Earth | - | - | -- -- -- | -- -- -- | -- -- --
| [RememberMe](['https://store.steampowered.com/app/228300', 'https://en.wikipedia.org/wiki/Remember_Me_(video_game)']) | Remember Me | - | - | -- -- -- | -- -- -- | -- -- --
| [RE](['https://store.steampowered.com/app/304240', 'https://en.wikipedia.org/wiki/Resident_Evil_(1996_video_game)']) | Resident Evil | - | - | -- -- -- | -- -- -- | -- -- --
| [RE+](['https://en.wikipedia.org/wiki/Resident_Evil_(2002_video_game)']) | Resident Evil (remake) | - | - | -- -- -- | -- -- -- | -- -- --
| [RE2](['https://store.steampowered.com/app/883710', 'https://en.wikipedia.org/wiki/Resident_Evil_2']) | Resident Evil 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [RE2+](['https://en.wikipedia.org/wiki/Resident_Evil_2_(2019_video_game)']) | Resident Evil 2 (remake) | - | - | -- -- -- | -- -- -- | -- -- --
| [RE2:SDV](['https://en.wikipedia.org/wiki/Resident_Evil_2:_Dual_Shock_Version']) | Resident Evil 2: Dual Shock Version | - | - | -- -- -- | -- -- -- | -- -- --
| [RE3:N](['https://en.wikipedia.org/wiki/Resident_Evil_3:_Nemesis']) | Resident Evil 3: Nemesis | - | - | -- -- -- | -- -- -- | -- -- --
| [RE3](['https://store.steampowered.com/app/952060', 'https://en.wikipedia.org/wiki/Resident_Evil_3_(2020_video_game)']) | Resident Evil 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [RE4](['https://store.steampowered.com/app/254700', 'https://en.wikipedia.org/wiki/Resident_Evil_4']) | Resident Evil 4 | - | - | -- -- -- | -- -- -- | -- -- --
| [RE4+](['https://en.wikipedia.org/wiki/Resident_Evil_4_(2023_video_game)']) | Resident Evil 4 (remake) | - | - | -- -- -- | -- -- -- | -- -- --
| [RE4:HD](['https://en.wikipedia.org/wiki/Resident_Evil_4']) | Resident Evil 4 HD | - | - | -- -- -- | -- -- -- | -- -- --
| [RE4:UHD](['https://en.wikipedia.org/wiki/Resident_Evil_4:_Ultimate_HD_Edition']) | Resident Evil 4: Ultimate HD Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [RE4:W](['https://en.wikipedia.org/wiki/Resident_Evil_4']) | Resident Evil 4: Wii Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [RE5](['https://store.steampowered.com/app/21690', 'https://en.wikipedia.org/wiki/Resident_Evil_5']) | Resident Evil 5 | - | - | -- -- -- | -- -- -- | -- -- --
| [RE5:G](['https://en.wikipedia.org/wiki/Resident_Evil_5:_Gold_Edition']) | Resident Evil 5: Gold Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [RE6](['https://store.steampowered.com/app/221040', 'https://en.wikipedia.org/wiki/Resident_Evil_6']) | Resident Evil 6 | - | - | -- -- -- | -- -- -- | -- -- --
| [RE7](['https://store.steampowered.com/app/418370', 'https://en.wikipedia.org/wiki/Resident_Evil_7:_Biohazard']) | Resident Evil 7: Biohazard | - | - | -- -- -- | -- -- -- | -- -- --
| [REA:RE](['https://en.wikipedia.org/wiki/Resident_Evil_(2002_video_game)']) | Resident Evil Archives: Resident Evil | - | - | -- -- -- | -- -- -- | -- -- --
| [REA:REZ](['https://en.wikipedia.org/wiki/Resident_Evil_Zero']) | Resident Evil Archives: Resident Evil Zero | - | - | -- -- -- | -- -- -- | -- -- --
| [REC:V](['https://en.wikipedia.org/wiki/Resident_Evil_%E2%80%93_Code:_Veronica']) | Resident Evil – Code: Veronica | - | - | -- -- -- | -- -- -- | -- -- --
| [REC:VX](['https://en.wikipedia.org/wiki/Resident_Evil_%E2%80%93_Code:_Veronica_X']) | Resident Evil – Code: Veronica X | - | - | -- -- -- | -- -- -- | -- -- --
| [REC:VXHD](['https://en.wikipedia.org/wiki/Resident_Evil_%E2%80%93_Code:_Veronica']) | Resident Evil – Code: Veronica X HD | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:DA](['https://en.wikipedia.org/wiki/Resident_Evil:_Dead_Aim']) | Resident Evil: Dead Aim | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:DS](['https://en.wikipedia.org/wiki/Resident_Evil:_Deadly_Silence']) | Resident Evil: Deadly Silence | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:D]([]) | Resident Evil: Degeneration | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:DC](['https://en.wikipedia.org/wiki/Resident_Evil:_Director%27s_Cut']) | Resident Evil: Director's Cut | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:DCDSV](['https://en.wikipedia.org/wiki/Resident_Evil:_Director%27s_Cut']) | Resident Evil: Director's Cut Dual Shock Version | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:G](['https://en.wikipedia.org/wiki/Resident_Evil_Gaiden']) | Resident Evil Gaiden | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:ORC](['https://en.wikipedia.org/wiki/Resident_Evil:_Operation_Raccoon_City']) | Resident Evil: Operation Raccoon City | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:O](['https://en.wikipedia.org/wiki/Resident_Evil_Outbreak']) | Resident Evil Outbreak | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:OF2](['https://en.wikipedia.org/wiki/Resident_Evil_Outbreak:_File_2']) | Resident Evil Outbreak File #2 | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:P](['https://en.wikipedia.org/wiki/Resident_Evil_Portable']) | Resident Evil Portable | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:R](['https://store.steampowered.com/app/222480', 'https://en.wikipedia.org/wiki/Resident_Evil:_Revelations']) | Resident Evil: Revelations | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:R2](['https://store.steampowered.com/app/287290', 'https://en.wikipedia.org/wiki/Resident_Evil:_Revelations_2']) | Resident Evil: Revelations 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:RU](['https://en.wikipedia.org/wiki/Resident_Evil:_Revelations']) | Resident Evil: Revelations Unveiled Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [RES](['https://en.wikipedia.org/wiki/Resident_Evil_Survivor']) | Resident Evil Survivor | - | - | -- -- -- | -- -- -- | -- -- --
| [RES2C:V](['https://en.wikipedia.org/wiki/Resident_Evil_Survivor_2_Code:_Veronica']) | Resident Evil Survivor 2 Code: Veronica | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:TDC](['https://en.wikipedia.org/wiki/Resident_Evil:_The_Darkside_Chronicles']) | Resident Evil: The Darkside Chronicles | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:TM3D](['https://en.wikipedia.org/wiki/Resident_Evil:_The_Mercenaries_3D']) | Resident Evil: The Mercenaries 3D | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:TUC](['https://en.wikipedia.org/wiki/Resident_Evil:_The_Umbrella_Chronicles']) | Resident Evil: The Umbrella Chronicles | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:V](['https://store.steampowered.com/app/1196590', 'https://en.wikipedia.org/wiki/Resident_Evil_Village']) | Resident Evil Village | - | - | -- -- -- | -- -- -- | -- -- --
| [REZ](['https://store.steampowered.com/app/339340', 'https://en.wikipedia.org/wiki/Resident_Evil_Zero']) | Resident Evil Zero | - | - | -- -- -- | -- -- -- | -- -- --
| [RivalSchools:UBF](['https://en.wikipedia.org/wiki/Rival_Schools:_United_By_Fate']) | Rival Schools: United By Fate | - | - | -- -- -- | -- -- -- | -- -- --
| [Rocketmen: Axis of Evil](['https://en.wikipedia.org/wiki/Rocketmen:_Axis_of_Evil']) | Rocketmen: Axis of Evil | - | - | -- -- -- | -- -- -- | -- -- --
| [Rockman:FMKC](['https://en.wikipedia.org/wiki/Rockman_%26_Forte_Mirai_kara_no_Chosensha']) | Rockman & Forte Mirai kara no Chosensha | - | - | -- -- -- | -- -- -- | -- -- --
| [Rockman:BF](['https://en.wikipedia.org/wiki/Rockman_Battle_%26_Fighters']) | Rockman Battle & Fighters | - | - | -- -- -- | -- -- -- | -- -- --
| [RockmanCW:R](['https://en.wikipedia.org/wiki/Rockman_Complete_Works']) | Rockman Complete Works: Rockman | - | - | -- -- -- | -- -- -- | -- -- --
| [RockmanCW:R2](['https://en.wikipedia.org/wiki/Rockman_Complete_Works']) | Rockman Complete Works: Rockman 2 Dr. Wily no Nazo!! | - | - | -- -- -- | -- -- -- | -- -- --
| [RockmanCW:R3](['https://en.wikipedia.org/wiki/Rockman_Complete_Works']) | Rockman Complete Works: Rockman 3 Dr. Wily no Saigo!? | - | - | -- -- -- | -- -- -- | -- -- --
| [RockmanCW:R4](['https://en.wikipedia.org/wiki/Rockman_Complete_Works']) | Rockman Complete Works: Rockman 4 Aratanaru Yabou!! | - | - | -- -- -- | -- -- -- | -- -- --
| [RockmanCW:R5](['https://en.wikipedia.org/wiki/Rockman_Complete_Works']) | Rockman Complete Works: Rockman 5 Blues no Wana! | - | - | -- -- -- | -- -- -- | -- -- --
| [RockmanCW:R6](['https://en.wikipedia.org/wiki/Rockman_Complete_Works']) | Rockman Complete Works: Rockman 6 Shijou Saidai no Tatakai!! | - | - | -- -- -- | -- -- -- | -- -- --
| [RockmanEX:N1B](['https://en.wikipedia.org/wiki/Rockman_EXE_N1_Battle']) | Rockman EXE N1 Battle | - | - | -- -- -- | -- -- -- | -- -- --
| [RockmanEX:OSS](['https://en.wikipedia.org/wiki/Rockman_EXE_Operate_Shooting_Star']) | Rockman EXE Operate Shooting Star | - | - | -- -- -- | -- -- -- | -- -- --
| [RockmanEX:WS](['https://en.wikipedia.org/wiki/Rockman_EXE_WS']) | Rockman EXE WS | - | - | -- -- -- | -- -- -- | -- -- --
| [RockmanIQ:C]([]) | Rockman IQ Challenge | - | - | -- -- -- | -- -- -- | -- -- --
| **Capcom+S** | **None**
| [SamuraiSword]([]) | Samurai Sword | - | - | -- -- -- | -- -- -- | -- -- --
| [SaturdayNightSlamMasters](['https://en.wikipedia.org/wiki/Saturday_Night_Slam_Masters']) | Saturday Night Slam Masters | - | - | -- -- -- | -- -- -- | -- -- --
| [SectionZ](['https://en.wikipedia.org/wiki/Section_Z']) | Section Z | - | - | -- -- -- | -- -- -- | -- -- --
| [Sengoku](['https://en.wikipedia.org/wiki/Devil_Kings']) | Sengoku Basara | - | - | -- -- -- | -- -- -- | -- -- --
| [Sengoku2](['https://en.wikipedia.org/wiki/Sengoku_Basara_2']) | Sengoku Basara 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [Sengoku:BH](['https://en.wikipedia.org/wiki/Sengoku_Basara_Battle_Heroes']) | Sengoku Basara Battle Heroes | - | - | -- -- -- | -- -- -- | -- -- --
| [Sengoku:CH](['https://en.wikipedia.org/wiki/Sengoku_Basara_Chronicle_Heroes']) | Sengoku Basara Chronicle Heroes | - | - | -- -- -- | -- -- -- | -- -- --
| [Sengoku:X](['https://en.wikipedia.org/wiki/Sengoku_Basara_X']) | Sengoku Basara X | - | - | -- -- -- | -- -- -- | -- -- --
| [Sengoku:SH](['https://en.wikipedia.org/wiki/Sengoku_Basara:_Samurai_Heroes']) | Sengoku Basara: Samurai Heroes | - | - | -- -- -- | -- -- -- | -- -- --
| [Sengoku4](['https://en.wikipedia.org/wiki/Sengoku_Basara_4']) | Sengoku Basara 4 | - | - | -- -- -- | -- -- -- | -- -- --
| [Sengoku:SYD](['https://en.wikipedia.org/wiki/Sengoku_Basara']) | Sengoku Basara: Sanada Yukimura-Den | - | - | -- -- -- | -- -- -- | -- -- --
| [Shadow of Rome](['https://en.wikipedia.org/wiki/Shadow_of_Rome']) | Shadow of Rome | - | - | -- -- -- | -- -- -- | -- -- --
| [Shantae](['https://en.wikipedia.org/wiki/Shantae_(video_game)']) | Shantae | - | - | -- -- -- | -- -- -- | -- -- --
| [ShichiseiToushin:G](['https://en.wikipedia.org/wiki/Seven_Star_Fighting_God_Guyferd']) | Shichisei Toushin: Guyferd | - | - | -- -- -- | -- -- -- | -- -- --
| [ShiritsuJusticeGakuen:NSN2](['https://en.wikipedia.org/wiki/Rival_Schools:_United_By_Fate#Nekketsu_Seisyun_Nikki_2']) | Shiritsu Justice Gakuen: Nekketsu Seisyun Nikki 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [SideArms](['https://en.wikipedia.org/wiki/Side_Arms_(video_game)']) | Side Arms | - | - | -- -- -- | -- -- -- | -- -- --
| [SideArms:HD](['https://en.wikipedia.org/wiki/Side_Arms_Hyper_Dyne']) | Side Arms Hyper Dyne | - | - | -- -- -- | -- -- -- | -- -- --
| [SideArms:S]([]) | Side Arms Special | - | - | -- -- -- | -- -- -- | -- -- --
| [Slipstream]([]) | Slipstream | - | - | -- -- -- | -- -- -- | -- -- --
| [SmurfsVillage](['https://en.wikipedia.org/wiki/The_Smurfs']) | Smurfs' Village | - | - | -- -- -- | -- -- -- | -- -- --
| [SmurfsGrabber](['https://en.wikipedia.org/wiki/The_Smurfs']) | Smurfs' Grabber | - | - | -- -- -- | -- -- -- | -- -- --
| [SmurfLife](['https://en.wikipedia.org/wiki/The_Smurfs']) | Smurf Life | - | - | -- -- -- | -- -- -- | -- -- --
| [SnowBrothers](['https://en.wikipedia.org/wiki/Snow_Brothers']) | Snow Brothers | - | - | -- -- -- | -- -- -- | -- -- --
| [SonSon](['https://en.wikipedia.org/wiki/SonSon']) | SonSon | - | - | -- -- -- | -- -- -- | -- -- --
| [SonSon2]([]) | SonSon 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [Spawn:ItDH](['https://en.wikipedia.org/wiki/Spawn:_In_the_Demon%27s_Hand']) | Spawn: In the Demon's Hand | - | - | -- -- -- | -- -- -- | -- -- --
| [Spyborgs](['https://en.wikipedia.org/wiki/Spyborgs']) | Spyborgs | - | - | -- -- -- | -- -- -- | -- -- --
| [StarGladiator](['https://en.wikipedia.org/wiki/Star_Gladiator']) | Star Gladiator | - | - | -- -- -- | -- -- -- | -- -- --
| [StartlingAdventures:K3]([]) | Startling Adventures Kuso 3 נDaiboken | - | - | -- -- -- | -- -- -- | -- -- --
| [SteelBattalion](['https://en.wikipedia.org/wiki/Steel_Battalion']) | Steel Battalion | - | - | -- -- -- | -- -- -- | -- -- --
| [SteelBattalion:HA](['https://en.wikipedia.org/wiki/Steel_Battalion:_Heavy_Armor']) | Steel Battalion: Heavy Armor | - | - | -- -- -- | -- -- -- | -- -- --
| [SteelBattalion:LoC](['https://en.wikipedia.org/wiki/Steel_Battalion:_Line_of_Contact']) | Steel Battalion: Line of Contact | - | - | -- -- -- | -- -- -- | -- -- --
| [SteelFang]([]) | Steel Fang | - | - | -- -- -- | -- -- -- | -- -- --
| [Stocker](['https://en.wikipedia.org/wiki/Stocker_(video_game)']) | Stocker | - | - | -- -- -- | -- -- -- | -- -- --
| [SF](['https://en.wikipedia.org/wiki/Street_Fighter_(video_game)']) | Street Fighter | - | - | -- -- -- | -- -- -- | -- -- --
| [SF:30AC](['https://store.steampowered.com/app/586200', 'https://en.wikipedia.org/wiki/Street_Fighter_30th_Anniversary_Collection']) | Street Fighter 30th Anniversary Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [SF2010:TFF](['https://en.wikipedia.org/wiki/Street_Fighter_2010:_The_Final_Fight']) | Street Fighter 2010: The Final Fight | - | - | -- -- -- | -- -- -- | -- -- --
| [SFA](['https://en.wikipedia.org/wiki/Street_Fighter_Alpha']) | Street Fighter Alpha | - | - | -- -- -- | -- -- -- | -- -- --
| [SFA2](['https://en.wikipedia.org/wiki/Street_Fighter_Alpha_2']) | Street Fighter Alpha 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [SFA3](['https://en.wikipedia.org/wiki/Street_Fighter_Alpha_3']) | Street Fighter Alpha 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [SFA3:M](['https://en.wikipedia.org/wiki/Street_Fighter_Alpha_3']) | Street Fighter Alpha 3 Max | - | - | -- -- -- | -- -- -- | -- -- --
| [SFA:A](['https://en.wikipedia.org/wiki/Street_Fighter_Alpha_Anthology']) | Street Fighter Alpha Anthology | - | - | -- -- -- | -- -- -- | -- -- --
| [SF:AC](['https://en.wikipedia.org/wiki/Street_Fighter_Anniversary_Collection']) | Street Fighter Anniversary Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [SF:C](['https://en.wikipedia.org/wiki/Street_Fighter_Collection']) | Street Fighter Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [SFX](['https://en.wikipedia.org/wiki/Street_Fighter_EX']) | Street Fighter EX | - | - | -- -- -- | -- -- -- | -- -- --
| [SFX2](['https://en.wikipedia.org/wiki/Street_Fighter_EX2']) | Street Fighter EX2 | - | - | -- -- -- | -- -- -- | -- -- --
| [SFX2+](['https://en.wikipedia.org/wiki/Street_Fighter_EX2']) | Street Fighter EX 2 Plus | - | - | -- -- -- | -- -- -- | -- -- --
| [SFX3](['https://en.wikipedia.org/wiki/Street_Fighter_EX3']) | Street Fighter EX3 | - | - | -- -- -- | -- -- -- | -- -- --
| [SFX:A](['https://en.wikipedia.org/wiki/Street_Fighter_EX']) | Street Fighter EX Alpha | - | - | -- -- -- | -- -- -- | -- -- --
| [SFX+](['https://en.wikipedia.org/wiki/Street_Fighter_EX_Plus']) | Street Fighter EX Plus | - | - | -- -- -- | -- -- -- | -- -- --
| [SF2:TWW](['https://en.wikipedia.org/wiki/Street_Fighter_II:_The_World_Warrior']) | Street Fighter II: The World Warrior | - | - | -- -- -- | -- -- -- | -- -- --
| [SF2:C](['https://en.wikipedia.org/wiki/Street_Fighter_II_Champion_Edition']) | Street Fighter II Champion Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [SF2:M](['https://en.wikipedia.org/wiki/List_of_Street_Fighter_games#Other_games']) | Street Fighter II Movie | - | - | -- -- -- | -- -- -- | -- -- --
| [SF2:SC](['https://en.wikipedia.org/wiki/Street_Fighter_II_Champion_Edition']) | Street Fighter II: Special Champion Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [SF2:HF](['https://en.wikipedia.org/wiki/Street_Fighter_II:_Hyper_Fighting']) | Street Fighter II: Hyper Fighting | - | - | -- -- -- | -- -- -- | -- -- --
| [SF3:3S](['https://en.wikipedia.org/wiki/Street_Fighter_III:_3rd_Strike']) | Street Fighter III: 3rd Strike | - | - | -- -- -- | -- -- -- | -- -- --
| [SF3:3SO](['https://en.wikipedia.org/wiki/Street_Fighter_III:_3rd_Strike_Online_Edition']) | Street Fighter III: 3rd Strike Online Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [SF3:DI](['https://en.wikipedia.org/wiki/Street_Fighter_III:_Double_Impact']) | Street Fighter III: Double Impact | - | - | -- -- -- | -- -- -- | -- -- --
| [SF3:NG](['https://en.wikipedia.org/wiki/Street_Fighter_III']) | Street Fighter III: New Generation | - | - | -- -- -- | -- -- -- | -- -- --
| [SF3:2I](['https://en.wikipedia.org/wiki/Street_Fighter_III:_2nd_Impact']) | Street Fighter III: 2nd Impact | - | - | -- -- -- | -- -- -- | -- -- --
| [SF4](['https://en.wikipedia.org/wiki/Street_Fighter_IV']) | Street Fighter IV | - | - | -- -- -- | -- -- -- | -- -- --
| [SF4:V]([]) | Street Fighter IV: Volt | - | - | -- -- -- | -- -- -- | -- -- --
| [SF5](['https://store.steampowered.com/app/310950', 'https://en.wikipedia.org/wiki/Street_Fighter_V']) | Street Fighter V | - | - | -- -- -- | -- -- -- | -- -- --
| [SF5:AV](['https://en.wikipedia.org/wiki/Street_Fighter_V']) | Street Fighter V: Arcade Version | - | - | -- -- -- | -- -- -- | -- -- --
| [SF6](['https://en.wikipedia.org/wiki/Street_Fighter_6']) | Street Fighter 6 | - | - | -- -- -- | -- -- -- | -- -- --
| [SFX:MM](['https://en.wikipedia.org/wiki/Street_Fighter_X_Mega_Man']) | Street Fighter X Mega Man | - | - | -- -- -- | -- -- -- | -- -- --
| [SFX:T](['https://en.wikipedia.org/wiki/Street_Fighter_X_Tekken']) | Street Fighter X Tekken | - | - | -- -- -- | -- -- -- | -- -- --
| [SFZ](['https://en.wikipedia.org/wiki/Street_Fighter_Zero']) | Street Fighter Zero | - | - | -- -- -- | -- -- -- | -- -- --
| [SFZ2:A](['https://en.wikipedia.org/wiki/Street_Fighter_Zero_2_Alpha']) | Street Fighter Zero 2 Alpha | - | - | -- -- -- | -- -- -- | -- -- --
| [SFZ2:D](['https://en.wikipedia.org/wiki/Street_Fighter_Alpha_2']) | Street Fighter Zero 2 Dash | - | - | -- -- -- | -- -- -- | -- -- --
| [SFZ3](['https://en.wikipedia.org/wiki/Street_Fighter_Zero_3']) | Street Fighter Zero 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [SFZ3:U](['https://en.wikipedia.org/wiki/Street_Fighter_Zero_3_Upper']) | Street Fighter Zero 3 Upper | - | - | -- -- -- | -- -- -- | -- -- --
| [SF:TMa](['https://en.wikipedia.org/wiki/Street_Fighter:_The_Movie_(arcade_game)']) | Street Fighter: The Movie (arcade game) | - | - | -- -- -- | -- -- -- | -- -- --
| [SF:TMc](['https://en.wikipedia.org/wiki/Street_Fighter:_The_Movie_(console_video_game)']) | Street Fighter: The Movie (console video game) | - | - | -- -- -- | -- -- -- | -- -- --
| [Strider:A]([]) | Strider | - | - | -- -- -- | -- -- -- | -- -- --
| [Strider](['https://store.steampowered.com/app/235210', 'https://en.wikipedia.org/wiki/Strider_(2014_video_game)']) | Strider (2014 video game) | - | - | -- -- -- | -- -- -- | -- -- --
| [Strider:NES](['https://en.wikipedia.org/wiki/Strider_(1989_NES_video_game)']) | Strider (1989 NES video game) | - | - | -- -- -- | -- -- -- | -- -- --
| [Strider:HD](['https://en.wikipedia.org/wiki/Strider_(2014_video_game)']) | Strider HD | - | - | -- -- -- | -- -- -- | -- -- --
| [Strider2](['https://en.wikipedia.org/wiki/Strider_2_(1999_video_game)']) | Strider 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [StriderII](['https://en.wikipedia.org/wiki/Strider_2_(1999_video_game)']) | Strider II | - | - | -- -- -- | -- -- -- | -- -- --
| [SuperAdventureRockman](['https://en.wikipedia.org/wiki/Super_Adventure_Rockman']) | Super Adventure Rockman | - | - | -- -- -- | -- -- -- | -- -- --
| [SuperBusterBros](['https://en.wikipedia.org/wiki/Super_Buster_Bros.']) | Super Buster Bros. | - | - | -- -- -- | -- -- -- | -- -- --
| [SuperGhoulsGhosts](['https://en.wikipedia.org/wiki/Super_Ghouls_%27n_Ghosts']) | Super Ghouls 'n Ghosts | - | - | -- -- -- | -- -- -- | -- -- --
| [SuperPang](['https://en.wikipedia.org/wiki/Super_Pang']) | Super Pang | - | - | -- -- -- | -- -- -- | -- -- --
| [SPF2:T](['https://en.wikipedia.org/wiki/Super_Puzzle_Fighter_II_Turbo']) | Super Puzzle Fighter II Turbo | - | - | -- -- -- | -- -- -- | -- -- --
| [SPF2:THD](['https://en.wikipedia.org/wiki/Super_Puzzle_Fighter_II_Turbo_HD_Remix']) | Super Puzzle Fighter II Turbo HD Remix | - | - | -- -- -- | -- -- -- | -- -- --
| [SPF2:X](['https://en.wikipedia.org/wiki/Super_Puzzle_Fighter_II_Turbo']) | Super Puzzle Fighter II X for Matching Service | - | - | -- -- -- | -- -- -- | -- -- --
| [SSF2](['https://en.wikipedia.org/wiki/Super_Street_Fighter_II']) | Super Street Fighter II | - | - | -- -- -- | -- -- -- | -- -- --
| [SSF2:T](['https://en.wikipedia.org/wiki/Super_Street_Fighter_II_Turbo']) | Super Street Fighter II Turbo | - | - | -- -- -- | -- -- -- | -- -- --
| [SSF2:THD](['https://en.wikipedia.org/wiki/Super_Street_Fighter_II_Turbo_HD_Remix']) | Super Street Fighter II Turbo HD Remix | - | - | -- -- -- | -- -- -- | -- -- --
| [SSF2:XGM]([]) | Super Street Fighter II X Grand Master Challenge for Matching Service | - | - | -- -- -- | -- -- -- | -- -- --
| [SSF2:TR](['https://en.wikipedia.org/wiki/Super_Street_Fighter_II_Turbo#Game_Boy_Advance']) | Super Street Fighter II: Turbo Revival | - | - | -- -- -- | -- -- -- | -- -- --
| [SSF4](['https://en.wikipedia.org/wiki/Super_Street_Fighter_IV']) | Super Street Fighter IV | - | - | -- -- -- | -- -- -- | -- -- --
| [SSF4:3D](['https://en.wikipedia.org/wiki/Super_Street_Fighter_IV:_3D_Edition']) | Super Street Fighter IV: 3D Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [SSF4:A](['https://en.wikipedia.org/wiki/Super_Street_Fighter_IV:_Arcade_Edition']) | Super Street Fighter IV: Arcade Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [SuzuMonogatari]([]) | Suzu Monogatari | - | - | -- -- -- | -- -- -- | -- -- --
| [SweetHome](['https://en.wikipedia.org/wiki/Sweet_Home_(video_game)']) | Sweet Home | - | - | -- -- -- | -- -- -- | -- -- --
| [Sydney2000](['https://en.wikipedia.org/wiki/Sydney_2000_(video_game)']) | Sydney 2000 | - | - | -- -- -- | -- -- -- | -- -- --
| **Capcom+T-Z** | **None**
| [TNGCP:AS]([]) | Taisen Net Gimmick Capcom & Psikyo All Stars | - | - | -- -- -- | -- -- -- | -- -- --
| [TaleSpin](['https://en.wikipedia.org/wiki/TaleSpin_(Capcom_video_game)']) | TaleSpin | - | - | -- -- -- | -- -- -- | -- -- --
| [Talisman](['https://en.wikipedia.org/wiki/Talisman_(video_game)']) | Talisman | - | - | -- -- -- | -- -- -- | -- -- --
| [TVC:UAS](['https://en.wikipedia.org/wiki/Tatsunoko_vs._Capcom:_Ultimate_All-Stars']) | Tatsunoko vs. Capcom: Ultimate All-Stars | - | - | -- -- -- | -- -- -- | -- -- --
| [TechRomancer](['https://en.wikipedia.org/wiki/Tech_Romancer']) | Tech Romancer | - | - | -- -- -- | -- -- -- | -- -- --
| [TGAA:A](['https://en.wikipedia.org/wiki/The_Great_Ace_Attorney:_Adventures']) | The Great Ace Attorney: Adventures | - | - | -- -- -- | -- -- -- | -- -- --
| [TGAA2:R](['https://en.wikipedia.org/wiki/The_Great_Ace_Attorney_2:_Resolve']) | The Great Ace Attorney 2: Resolve | - | - | -- -- -- | -- -- -- | -- -- --
| [TheKingofDragons](['https://en.wikipedia.org/wiki/The_King_of_Dragons']) | The King of Dragons | - | - | -- -- -- | -- -- -- | -- -- --
| [Zelda:LTP](['https://en.wikipedia.org/wiki/The_Legend_of_Zelda:_A_Link_to_the_Past_and_Four_Swords']) | The Legend of Zelda: A Link to the Past and Four Swords | - | - | -- -- -- | -- -- -- | -- -- --
| [Zelda:OoA](['https://en.wikipedia.org/wiki/The_Legend_of_Zelda:_Oracle_of_Seasons_and_Oracle_of_Ages']) | The Legend of Zelda: Oracle of Ages | - | - | -- -- -- | -- -- -- | -- -- --
| [Zelda:OoS](['https://en.wikipedia.org/wiki/The_Legend_of_Zelda:_Oracle_of_Seasons_and_Oracle_of_Ages']) | The Legend of Zelda: Oracle of Seasons | - | - | -- -- -- | -- -- -- | -- -- --
| [Zelda:TMC](['https://en.wikipedia.org/wiki/The_Legend_of_Zelda:_The_Minish_Cap']) | The Legend of Zelda: The Minish Cap | - | - | -- -- -- | -- -- -- | -- -- --
| [TheLittleMermaid](['https://en.wikipedia.org/wiki/The_Little_Mermaid_(video_game)']) | The Little Mermaid | - | - | -- -- -- | -- -- -- | -- -- --
| [TheMagicalNinja:JK]([]) | The Magical Ninja: Jiraiya Kenzan! | - | - | -- -- -- | -- -- -- | -- -- --
| [TMOTB](['https://en.wikipedia.org/wiki/The_Misadventures_of_Tron_Bonne']) | The Misadventures of Tron Bonne | - | - | -- -- -- | -- -- -- | -- -- --
| [TheMaw](['https://en.wikipedia.org/wiki/The_Maw_(video_game)']) | The Maw | - | - | -- -- -- | -- -- -- | -- -- --
| [TNBC:OR](['https://en.wikipedia.org/wiki/The_Nightmare_Before_Christmas:_Oogie%27s_Revenge']) | The Nightmare Before Christmas: Oogie's Revenge | - | - | -- -- -- | -- -- -- | -- -- --
| [ThePunisher](['https://en.wikipedia.org/wiki/The_Punisher_(1993_video_game)']) | The Punisher | - | - | -- -- -- | -- -- -- | -- -- --
| [TheSpeedRumbler](['https://en.wikipedia.org/wiki/The_Speed_Rumbler']) | The Speed Rumbler | - | - | -- -- -- | -- -- -- | -- -- --
| [ThreeWonders](['https://en.wikipedia.org/wiki/Three_Wonders']) | Three Wonders | - | - | -- -- -- | -- -- -- | -- -- --
| [TigerRoad](['https://en.wikipedia.org/wiki/Tiger_Road']) | Tiger Road | - | - | -- -- -- | -- -- -- | -- -- --
| [TokiTori](['https://en.wikipedia.org/wiki/Toki_Tori']) | Toki Tori | - | - | -- -- -- | -- -- -- | -- -- --
| [TombRaider:TLR](['https://en.wikipedia.org/wiki/Tomb_Raider:_The_Last_Revelation']) | Tomb Raider: The Last Revelation | - | - | -- -- -- | -- -- -- | -- -- --
| [ToyStory](['https://en.wikipedia.org/wiki/Toy_Story_(video_game)']) | Toy Story | - | - | -- -- -- | -- -- -- | -- -- --
| [Snowboard:T](['https://en.wikipedia.org/wiki/Trick%27N_Snowboarder']) | Trick'N Snowboarder | - | - | -- -- -- | -- -- -- | -- -- --
| [Trojan](['https://en.wikipedia.org/wiki/Trojan_(video_game)']) | Trojan | - | - | -- -- -- | -- -- -- | -- -- --
| [Trouballs](['https://en.wikipedia.org/wiki/Trouballs']) | Trouballs | - | - | -- -- -- | -- -- -- | -- -- --
| [Turok](['https://en.wikipedia.org/wiki/Turok_(video_game)']) | Turok | - | - | -- -- -- | -- -- -- | -- -- --
| [UNSquadron](['https://en.wikipedia.org/wiki/U.N._Squadron']) | U.N. Squadron | - | - | -- -- -- | -- -- -- | -- -- --
| [UFC](['https://en.wikipedia.org/wiki/Ultimate_Fighting_Championship_(video_game)']) | Ultimate Fighting Championship | - | - | -- -- -- | -- -- -- | -- -- --
| [UGhostsGoblins](['https://en.wikipedia.org/wiki/Ultimate_Ghosts_%27n_Goblins']) | Ultimate Ghosts 'n Goblins | - | - | -- -- -- | -- -- -- | -- -- --
| [UMVC3](['https://store.steampowered.com/app/357190', 'https://en.wikipedia.org/wiki/Ultimate_Marvel_vs._Capcom_3']) | Ultimate Marvel vs. Capcom 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [UmbrellaCorps](['https://store.steampowered.com/app/390340', 'https://en.wikipedia.org/wiki/Umbrella_Corps']) | Umbrella Corps | - | - | -- -- -- | -- -- -- | -- -- --
| [UnderTheSkin](['https://en.wikipedia.org/wiki/Under_the_Skin_(video_game)']) | Under the Skin | - | - | -- -- -- | -- -- -- | -- -- --
| [Vampire:C](['https://en.wikipedia.org/wiki/Darkstalkers_Chronicle:_The_Chaos_Tower']) | Vampire Chronicles for Matching Service | - | - | -- -- -- | -- -- -- | -- -- --
| [Vampire:H2](['https://en.wikipedia.org/wiki/Darkstalkers_3#Updates']) | Vampire Hunter 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [Vampire:S](['https://en.wikipedia.org/wiki/Darkstalkers_3']) | Vampire Savior | - | - | -- -- -- | -- -- -- | -- -- --
| [Vampire:S2](['https://en.wikipedia.org/wiki/Darkstalkers_3#Updates']) | Vampire Savior 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [Vampire:DC](['https://en.wikipedia.org/wiki/Vampire:_Darkstalkers_Collection']) | Vampire: Darkstalkers Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [Varth:OT](['https://en.wikipedia.org/wiki/Varth:_Operation_Thunderstorm']) | Varth: Operation Thunderstorm | - | - | -- -- -- | -- -- -- | -- -- --
| [ViewtifulJoe](['https://en.wikipedia.org/wiki/Viewtiful_Joe_(video_game)']) | Viewtiful Joe | - | - | -- -- -- | -- -- -- | -- -- --
| [ViewtifulJoe2](['https://en.wikipedia.org/wiki/Viewtiful_Joe_2']) | Viewtiful Joe 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [ViewtifulJoe:DT](['https://en.wikipedia.org/wiki/Viewtiful_Joe:_Double_Trouble!']) | Viewtiful Joe: Double Trouble! | - | - | -- -- -- | -- -- -- | -- -- --
| [ViewtifulJoe:RHR](['https://en.wikipedia.org/wiki/Viewtiful_Joe:_Red_Hot_Rumble']) | Viewtiful Joe: Red Hot Rumble | - | - | -- -- -- | -- -- -- | -- -- --
| [Vulgus](['https://en.wikipedia.org/wiki/Vulgus']) | Vulgus | - | - | -- -- -- | -- -- -- | -- -- --
| [Wantame:DDS]([]) | Wantame Music Channel: Doko Demo Style | - | - | -- -- -- | -- -- -- | -- -- --
| [WOTF](['https://en.wikipedia.org/wiki/War_of_the_Grail']) | War of the Grail | - | - | -- -- -- | -- -- -- | -- -- --
| [Warauinu:SGL]([]) | Warauinu no Bouken GB: Silly Go Lucky! | - | - | -- -- -- | -- -- -- | -- -- --
| [WOF](['https://en.wikipedia.org/wiki/Warriors_of_Fate']) | Warriors of Fate | - | - | -- -- -- | -- -- -- | -- -- --
| [WOF2](['https://en.wikipedia.org/wiki/Warriors_of_Fate']) | Warriors of Fate II | - | - | -- -- -- | -- -- -- | -- -- --
| [WOTS2](['https://en.wikipedia.org/wiki/Way_of_the_Samurai_2']) | Way of the Samurai 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [Golf:WL](['https://en.wikipedia.org/wiki/We_Love_Golf!']) | We Love Golf! | - | - | -- -- -- | -- -- -- | -- -- --
| [WFRR](['https://en.wikipedia.org/wiki/Who_Framed_Roger_Rabbit_(1991_video_game)']) | Who Framed Roger Rabbit | - | - | -- -- -- | -- -- -- | -- -- --
| [WWTBAM?]([]) | Who Wants to Be a Millionaire? | - | - | -- -- -- | -- -- -- | -- -- --
| [Willow](['https://en.wikipedia.org/wiki/Willow_(video_game)']) | Willow | - | - | -- -- -- | -- -- -- | -- -- --
| [WilyRight:NRTP](['https://en.wikipedia.org/wiki/Wily_%26_Right_no_RockBoard:_That%27s_Paradise']) | Wily & Right no RockBoard: That's Paradise | - | - | -- -- -- | -- -- -- | -- -- --
| [WithoutWarning](['https://en.wikipedia.org/wiki/Without_Warning_(video_game)']) | Without Warning | - | - | -- -- -- | -- -- -- | -- -- --
| [WizardryV](['https://en.wikipedia.org/wiki/Wizardry']) | Wizardry V | - | - | -- -- -- | -- -- -- | -- -- --
| [WOTB:C3](['https://en.wikipedia.org/wiki/Wolf_of_the_Battlefield:_Commando_3']) | Wolf of the Battlefield: Commando 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [WorldGoneSour](['https://en.wikipedia.org/wiki/World_Gone_Sour']) | World Gone Sour | - | - | -- -- -- | -- -- -- | -- -- --
| [X-Men:VSF](['https://en.wikipedia.org/wiki/X-Men_vs._Street_Fighter']) | X-Men vs. Street Fighter | - | - | -- -- -- | -- -- -- | -- -- --
| [X-Men:VSFEX](['https://en.wikipedia.org/wiki/X-Men_vs._Street_Fighter']) | X-Men vs. Street Fighter EX Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [XMen:CotA](['https://en.wikipedia.org/wiki/X-Men:_Children_of_the_Atom_(video_game)']) | X-Men: Children of the Atom | - | - | -- -- -- | -- -- -- | -- -- --
| [XMen:MA](['https://en.wikipedia.org/wiki/X-Men:_Mutant_Apocalypse']) | X-Men: Mutant Apocalypse | - | - | -- -- -- | -- -- -- | -- -- --
| [X2:NR](['https://en.wikipedia.org/wiki/X2_(video_game)']) | X2: No Relief | - | - | -- -- -- | -- -- -- | -- -- --
| [YoNoid](['https://en.wikipedia.org/wiki/Yo!_Noid']) | Yo! Noid | - | - | -- -- -- | -- -- -- | -- -- --
| [ZackWiki:QfBT](['https://en.wikipedia.org/wiki/Zack_%26_Wiki:_Quest_for_Barbaros%27_Treasure']) | Zack & Wiki: Quest for Barbaros' Treasure | - | - | -- -- -- | -- -- -- | -- -- --
| [ZombieCafe](['https://en.wikipedia.org/wiki/Zombie_Cafe']) | Zombie Cafe | - | - | -- -- -- | -- -- -- | -- -- --
| **Capcom** | **Capcom**
| [BionicCommando](['https://store.steampowered.com/app/21670', 'https://en.wikipedia.org/wiki/Bionic_Commando_(2009_video_game)']) | Bionic Commando | - | - | -- -- -- | -- -- -- | -- -- --
| [BionicCommando:R](['https://store.steampowered.com/app/21680', 'https://en.wikipedia.org/wiki/Bionic_Commando_Rearmed']) | Bionic Commando Rearmed | - | - | -- -- -- | -- -- -- | -- -- --
| [Arcade:S](['https://store.steampowered.com/app/1755910', 'https://en.wikipedia.org/wiki/Capcom_Arcade_Stadium#Capcom_Arcade_2nd_Stadium']) | Capcom Arcade 2nd Stadium | - | - | -- -- -- | -- -- -- | -- -- --
| [Arcade](['https://store.steampowered.com/app/1515950']) | Capcom Arcade Stadium | - | - | -- -- -- | -- -- -- | -- -- --
| [BEU:B](['https://store.steampowered.com/app/885150']) | Capcom Beat 'Em Up Bundle | - | - | -- -- -- | -- -- -- | -- -- --
| [Fighting:C](['https://store.steampowered.com/app/1685750']) | Capcom Fighting Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [DV](['https://store.steampowered.com/app/45710', 'https://en.wikipedia.org/wiki/Dark_Void']) | Dark Void | - | - | -- -- -- | -- -- -- | -- -- --
| [DV:Z](['https://store.steampowered.com/app/45730/Dark_Void_Zero/', 'https://en.wikipedia.org/wiki/Dark_Void_Zero']) | Dark Void Zero | - | - | -- -- -- | -- -- -- | -- -- --
| [DR](['https://store.steampowered.com/app/427190', 'https://en.wikipedia.org/wiki/Dead_Rising_(video_game)']) | Dead Rising | - | - | -- -- -- | -- -- -- | -- -- --
| [DR2](['https://store.steampowered.com/app/45740', 'https://en.wikipedia.org/wiki/Dead_Rising_2']) | Dead Rising 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [DR2:OtR](['https://store.steampowered.com/app/45770', 'https://en.wikipedia.org/wiki/Dead_Rising_2:_Off_the_Record']) | Dead Rising 2: Off the Record | - | - | -- -- -- | -- -- -- | -- -- --
| [DR3](['https://store.steampowered.com/app/265550', 'https://en.wikipedia.org/wiki/Dead_Rising_3']) | Dead Rising 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [DR4](['https://store.steampowered.com/app/543460', 'https://en.wikipedia.org/wiki/Dead_Rising_4']) | Dead Rising 4 | - | - | -- -- -- | -- -- -- | -- -- --
| [DMC3:S](['https://store.steampowered.com/app/6550', 'https://en.wikipedia.org/wiki/Devil_May_Cry_3:_Dante%27s_Awakening#Special_Edition']) | Devil May Cry 3: Special Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [DMC4:S](['https://store.steampowered.com/app/329050', 'https://en.wikipedia.org/wiki/Devil_May_Cry_4:_Special_Edition']) | Devil May Cry 4: Special Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [DMC5](['https://store.steampowered.com/app/601150', 'https://en.wikipedia.org/wiki/Devil_May_Cry_5']) | Devil May Cry 5 | - | - | -- -- -- | -- -- -- | -- -- --
| [DMC:HD](['https://store.steampowered.com/app/631510', 'https://en.wikipedia.org/wiki/Devil_May_Cry']) | Devil May Cry: HD Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [Disney:AC](['https://store.steampowered.com/app/525040']) | The Disney Afternoon Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [DMC:DMC](['https://store.steampowered.com/app/220440', 'https://en.wikipedia.org/wiki/DmC:_Devil_May_Cry']) | DmC: Devil May Cry | - | - | -- -- -- | -- -- -- | -- -- --
| [Dragon](['https://store.steampowered.com/app/367500', 'https://en.wikipedia.org/wiki/Dragon%27s_Dogma']) | Dragon's Dogma | - | - | -- -- -- | -- -- -- | -- -- --
| [DT:R](['https://store.steampowered.com/app/237630', 'https://en.wikipedia.org/wiki/DuckTales:_Remastered']) | DuckTales: Remastered | - | - | -- -- -- | -- -- -- | -- -- --
| [Flock](['https://store.steampowered.com/app/21640', 'https://en.wikipedia.org/wiki/Flock!']) | Flock! | - | - | -- -- -- | -- -- -- | -- -- --
| [GNG:R](['https://store.steampowered.com/app/1375400']) | Ghosts 'n Goblins Resurrection | - | - | -- -- -- | -- -- -- | -- -- --
| [TGAA:C](['https://store.steampowered.com/app/1158850']) | The Great Ace Attorney Chronicles | - | - | -- -- -- | -- -- -- | -- -- --
| [LP:EC](['https://store.steampowered.com/app/6510', 'https://en.wikipedia.org/wiki/Lost_Planet:_Extreme_Condition']) | Lost Planet: Extreme Condition | - | - | -- -- -- | -- -- -- | -- -- --
| [LP3](['https://store.steampowered.com/app/226720', 'https://en.wikipedia.org/wiki/Lost_Planet_3']) | Lost Planet 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [MVC:I](['https://store.steampowered.com/app/493840', 'https://en.wikipedia.org/wiki/Marvel_vs._Capcom:_Infinite']) | Marvel vs. Capcom: Infinite | - | - | -- -- -- | -- -- -- | -- -- --
| [MM11](['https://store.steampowered.com/app/742300', 'https://en.wikipedia.org/wiki/Mega_Man_11']) | Mega Man 11 | - | - | -- -- -- | -- -- -- | -- -- --
| [MM:LC](['https://store.steampowered.com/app/363440']) | Mega Man Legacy Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [MM:LC2](['https://store.steampowered.com/app/495050']) | Mega Man Legacy Collection 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [MM:XD](['https://store.steampowered.com/app/1582620']) | Mega Man X DiVE | - | - | -- -- -- | -- -- -- | -- -- --
| [MMX:LC](['https://store.steampowered.com/app/743890', 'https://en.wikipedia.org/wiki/Mega_Man_X_Legacy_Collection']) | Mega Man X Legacy Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [MMX:LC2](['https://store.steampowered.com/app/743900', 'https://en.wikipedia.org/wiki/Mega_Man_X_Legacy_Collection_2']) | Mega Man X Legacy Collection 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [MMZX:LC](['https://store.steampowered.com/app/999020']) | Mega Man Zero/ZX Legacy Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [MH:R](['https://store.steampowered.com/app/1446780']) | Monster Hunter Rise | - | - | -- -- -- | -- -- -- | -- -- --
| [MH:S2](['https://store.steampowered.com/app/1277400']) | Monster Hunter Stories 2: Wings of Ruin | - | - | -- -- -- | -- -- -- | -- -- --
| [MH:W](['https://store.steampowered.com/app/582010', 'https://en.wikipedia.org/wiki/Monster_Hunter:_World']) | Monster Hunter: World | - | - | -- -- -- | -- -- -- | -- -- --
| [Okami:HD](['https://store.steampowered.com/app/587620', 'https://en.wikipedia.org/wiki/%C5%8Ckami_HD']) | Ōkami HD | - | - | -- -- -- | -- -- -- | -- -- --
| [O:W](['https://store.steampowered.com/app/761600', 'https://en.wikipedia.org/wiki/Onimusha:_Warlords']) | Onimusha: Warlords | - | - | -- -- -- | -- -- -- | -- -- --
| [PWAA:T](['https://store.steampowered.com/app/787480']) | Phoenix Wright: Ace Attorney Trilogy | - | - | -- -- -- | -- -- -- | -- -- --
| [RDR2](['https://store.steampowered.com/app/1174180']) | Red Dead Redemption 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [RememberMe](['https://store.steampowered.com/app/228300', 'https://en.wikipedia.org/wiki/Remember_Me_(video_game)']) | Remember Me | - | - | -- -- -- | -- -- -- | -- -- --
| [RE](['https://store.steampowered.com/app/304240', 'https://en.wikipedia.org/wiki/Resident_Evil_(1996_video_game)']) | Resident Evil | - | - | -- -- -- | -- -- -- | -- -- --
| [REZ](['https://store.steampowered.com/app/339340', 'https://en.wikipedia.org/wiki/Resident_Evil_Zero']) | Resident Evil Zero | - | - | -- -- -- | -- -- -- | -- -- --
| [RE2](['https://store.steampowered.com/app/883710', 'https://en.wikipedia.org/wiki/Resident_Evil_2']) | Resident Evil 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [RE3](['https://store.steampowered.com/app/952060', 'https://en.wikipedia.org/wiki/Resident_Evil_3_(2020_video_game)']) | Resident Evil 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [RE4](['https://store.steampowered.com/app/254700', 'https://en.wikipedia.org/wiki/Resident_Evil_4']) | Resident Evil 4 | - | - | -- -- -- | -- -- -- | -- -- --
| [RE5](['https://store.steampowered.com/app/21690', 'https://en.wikipedia.org/wiki/Resident_Evil_5']) | Resident Evil 5 | - | - | -- -- -- | -- -- -- | -- -- --
| [RE6](['https://store.steampowered.com/app/221040', 'https://en.wikipedia.org/wiki/Resident_Evil_6']) | Resident Evil 6 | - | - | -- -- -- | -- -- -- | -- -- --
| [RE7](['https://store.steampowered.com/app/418370', 'https://en.wikipedia.org/wiki/Resident_Evil_7:_Biohazard']) | Resident Evil 7: Biohazard | - | - | -- -- -- | -- -- -- | -- -- --
| [RER](['https://store.steampowered.com/app/952070']) | Resident Evil Resistance | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:R](['https://store.steampowered.com/app/222480', 'https://en.wikipedia.org/wiki/Resident_Evil:_Revelations']) | Resident Evil: Revelations | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:R2](['https://store.steampowered.com/app/287290', 'https://en.wikipedia.org/wiki/Resident_Evil:_Revelations_2']) | Resident Evil: Revelations 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:RV](['https://store.steampowered.com/app/1236300']) | Resident Evil Re:Verse | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:V](['https://store.steampowered.com/app/1196590', 'https://en.wikipedia.org/wiki/Resident_Evil_Village']) | Resident Evil Village | - | - | -- -- -- | -- -- -- | -- -- --
| [SF:30AC](['https://store.steampowered.com/app/586200', 'https://en.wikipedia.org/wiki/Street_Fighter_30th_Anniversary_Collection']) | Street Fighter 30th Anniversary Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [SF5](['https://store.steampowered.com/app/310950', 'https://en.wikipedia.org/wiki/Street_Fighter_V']) | Street Fighter V | - | - | -- -- -- | -- -- -- | -- -- --
| [Strider](['https://store.steampowered.com/app/235210', 'https://en.wikipedia.org/wiki/Strider_(2014_video_game)']) | Strider (2014 video game) | - | - | -- -- -- | -- -- -- | -- -- --
| [UMVC3](['https://store.steampowered.com/app/357190', 'https://en.wikipedia.org/wiki/Ultimate_Marvel_vs._Capcom_3']) | Ultimate Marvel vs. Capcom 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [USF4](['https://store.steampowered.com/app/45760']) | Ultra Street Fighter IV | - | - | -- -- -- | -- -- -- | -- -- --
| [UmbrellaCorps](['https://store.steampowered.com/app/390340', 'https://en.wikipedia.org/wiki/Umbrella_Corps']) | Umbrella Corps | - | - | -- -- -- | -- -- -- | -- -- --
| **Cig** | **Roberts Space Industries**
| [StarCitizen](['https://robertsspaceindustries.com/playstarcitizen']) | Star Citizen | - | - | -- -- -- | -- -- -- | -- -- --
| **Cryptic** | **Cryptic**
| [COH](['https://en.wikipedia.org/wiki/City_of_Heroes']) | City of Heroes | - | - | -- -- -- | -- -- -- | -- -- --
| [CO](['https://store.steampowered.com/app/9880']) | Champions Online | open | read | -- -- -- | -- -- -- | -- -- --
| [STO](['https://store.steampowered.com/app/9900']) | Star Trek Online | open | read | -- -- -- | -- -- -- | -- -- --
| [NVW](['https://store.steampowered.com/app/109600']) | Neverwinter | open | read | -- -- -- | -- -- -- | -- -- --
| [MTG](['https://en.wikipedia.org/wiki/Magic:_Legends']) | Magic: The Gathering | - | - | -- -- -- | -- -- -- | -- -- --
| **Crytek** | **Crytek**
| [ArcheAge](['https://store.steampowered.com/app/304030']) | ArcheAge | - | - | -- -- -- | -- -- -- | -- -- --
| [Hunt](['https://store.steampowered.com/app/594650']) | Hunt: Showdown | - | - | -- -- -- | -- -- -- | -- -- --
| [MWO](['https://store.steampowered.com/app/342200']) | MechWarrior Online | - | - | -- -- -- | -- -- -- | -- -- --
| [Warface](['https://store.steampowered.com/app/291480']) | Warface | - | - | -- -- -- | -- -- -- | -- -- --
| [Wolcen](['https://store.steampowered.com/app/424370']) | Wolcen: Lords of Mayhem | - | - | -- -- -- | -- -- -- | -- -- --
| [Crysis](['https://store.steampowered.com/app/1715130']) | Crysis Remastered | - | - | -- -- -- | -- -- -- | -- -- --
| [Ryse](['https://store.steampowered.com/app/302510']) | Ryse: Son of Rome | - | - | -- -- -- | -- -- -- | -- -- --
| [Robinson](['https://store.steampowered.com/app/579820']) | Robinson: The Journey | - | - | -- -- -- | -- -- -- | -- -- --
| [Snow](['https://store.steampowered.com/app/244930']) | SNOW - The Ultimate Edition | - | - | -- -- -- | -- -- -- | -- -- --
| **Cyanide** | **Cyanide**
| [TC](['https://store.steampowered.com/app/287630', 'https://www.gog.com/en/game/the_council']) | The Council | - | - | -- -- -- | -- -- -- | -- -- --
| [Werewolf:TA](['https://store.steampowered.com/app/679110']) | Werewolf: The Apocalypse - Earthblood | - | - | -- -- -- | -- -- -- | -- -- --
| **Frictional** | **HPL Engine**
| [P:O](['https://store.steampowered.com/app/22180']) | Penumbra: Overture | - | - | -- -- -- | -- -- -- | -- -- --
| [P:BP](['https://store.steampowered.com/app/22120']) | Penumbra: Black Plague | - | - | -- -- -- | -- -- -- | -- -- --
| [P:R](['https://store.steampowered.com/app/22140']) | Penumbra: Requiem | - | - | -- -- -- | -- -- -- | -- -- --
| [A:TDD](['https://store.steampowered.com/app/57300']) | Amnesia: The Dark Descent | - | - | -- -- -- | -- -- -- | -- -- --
| [A:J]([]) | Amnesia: Justine | - | - | -- -- -- | -- -- -- | -- -- --
| [A:AMFP](['https://store.steampowered.com/app/239200']) | Amnesia: A Machine for Pigs | - | - | -- -- -- | -- -- -- | -- -- --
| [SOMA](['https://store.steampowered.com/app/282140']) | SOMA | - | - | -- -- -- | -- -- -- | -- -- --
| [A:R](['https://store.steampowered.com/app/999220']) | Amnesia: Rebirth | - | - | -- -- -- | -- -- -- | -- -- --
| **Frontier** | **Frontier Developments**
| [FE](['https://en.wikipedia.org/wiki/Frontier:_First_Encounters']) | Frontier: First Encounters | - | - | -- -- -- | -- -- -- | -- -- --
| [DX](['https://en.wikipedia.org/wiki/Darxide']) | Darxide | - | - | -- -- -- | -- -- -- | -- -- --
| [V2K](['https://en.wikipedia.org/wiki/Zarch#V2000']) | V2000 | - | - | -- -- -- | -- -- -- | -- -- --
| [IF]([]) | Infestation | - | - | -- -- -- | -- -- -- | -- -- --
| [DX:EMP](['https://en.wikipedia.org/wiki/Darxide']) | Darxide EMP | - | - | -- -- -- | -- -- -- | -- -- --
| [RTX](['https://en.wikipedia.org/wiki/RollerCoaster_Tycoon_(video_game)']) | RollerCoaster Tycoon (Xbox port) | - | - | -- -- -- | -- -- -- | -- -- --
| [WG:PZ](['https://en.wikipedia.org/wiki/Wallace_%26_Gromit_in_Project_Zoo']) | Wallace & Gromit in Project Zoo | - | - | -- -- -- | -- -- -- | -- -- --
| [DL](['https://en.wikipedia.org/wiki/Dog%27s_Life']) | Dog's Life | - | - | -- -- -- | -- -- -- | -- -- --
| [RT2:WW](['https://en.wikipedia.org/wiki/RollerCoaster_Tycoon_2']) | RollerCoaster Tycoon 2: Wacky Worlds | - | - | -- -- -- | -- -- -- | -- -- --
| [RT2:TT](['https://en.wikipedia.org/wiki/RollerCoaster_Tycoon_2']) | RollerCoaster Tycoon 2: Time Twister | - | - | -- -- -- | -- -- -- | -- -- --
| [RT3](['https://en.wikipedia.org/wiki/RollerCoaster_Tycoon_3']) | RollerCoaster Tycoon 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [RT3:S](['https://en.wikipedia.org/wiki/RollerCoaster_Tycoon_3']) | RollerCoaster Tycoon 3: Soaked! | - | - | -- -- -- | -- -- -- | -- -- --
| [RT3:W](['https://en.wikipedia.org/wiki/RollerCoaster_Tycoon_3']) | RollerCoaster Tycoon 3: Wild! | - | - | -- -- -- | -- -- -- | -- -- --
| [WG:TCotWR](['https://en.wikipedia.org/wiki/Wallace_%26_Gromit:_The_Curse_of_the_Were-Rabbit_(video_game)']) | Wallace & Gromit: The Curse of the Were-Rabbit | - | - | -- -- -- | -- -- -- | -- -- --
| [TV](['https://en.wikipedia.org/wiki/Thrillville']) | Thrillville | - | - | -- -- -- | -- -- -- | -- -- --
| [TV:OtR](['https://en.wikipedia.org/wiki/Thrillville:_Off_the_Rails']) | Thrillville: Off the Rails | - | - | -- -- -- | -- -- -- | -- -- --
| [LW](['https://store.steampowered.com/app/447780', 'https://en.wikipedia.org/wiki/LostWinds']) | LostWinds | - | - | -- -- -- | -- -- -- | -- -- --
| [LW2](['https://store.steampowered.com/app/447800', 'https://en.wikipedia.org/wiki/LostWinds_2:_Winter_of_the_Melodias']) | LostWinds 2: Winter of the Melodias | - | - | -- -- -- | -- -- -- | -- -- --
| [KT](['https://en.wikipedia.org/wiki/Kinectimals']) | Kinectimals | - | - | -- -- -- | -- -- -- | -- -- --
| [KT:NwB](['https://en.wikipedia.org/wiki/Kinectimals']) | Kinectimals: Now with Bears! | - | - | -- -- -- | -- -- -- | -- -- --
| [KT:DA](['https://en.wikipedia.org/wiki/Kinect:_Disneyland_Adventures']) | Kinect: Disneyland Adventures | - | - | -- -- -- | -- -- -- | -- -- --
| [CC]([]) | Coaster Crazy | - | - | -- -- -- | -- -- -- | -- -- --
| [ZT](['https://en.wikipedia.org/wiki/Zoo_Tycoon_(2013_video_game)']) | Zoo Tycoon | - | - | -- -- -- | -- -- -- | -- -- --
| [CCD]([]) | Coaster Crazy Deluxe | - | - | -- -- -- | -- -- -- | -- -- --
| [TFDS]([]) | Tales from Deep Space | - | - | -- -- -- | -- -- -- | -- -- --
| [ED](['https://store.steampowered.com/app/359320', 'https://en.wikipedia.org/wiki/Elite_Dangerous']) | Elite: Dangerous | - | - | -- -- -- | -- -- -- | -- -- --
| [SR](['https://en.wikipedia.org/wiki/Screamride']) | Screamride | - | - | -- -- -- | -- -- -- | -- -- --
| [ED:H](['https://en.wikipedia.org/wiki/Elite_Dangerous#Horizons_season_of_expansions']) | Elite Dangerous: Horizons | - | - | -- -- -- | -- -- -- | -- -- --
| [ED:A](['https://en.wikipedia.org/wiki/Elite_Dangerous#Elite_Dangerous:_Arena']) | Elite Dangerous: Arena | - | - | -- -- -- | -- -- -- | -- -- --
| [PC](['https://store.steampowered.com/app/493340', 'https://en.wikipedia.org/wiki/Planet_Coaster']) | Planet Coaster | - | - | -- -- -- | -- -- -- | -- -- --
| [JW](['https://store.steampowered.com/app/648350', 'https://en.wikipedia.org/wiki/Jurassic_World_Evolution']) | Jurassic World Evolution | - | - | -- -- -- | -- -- -- | -- -- --
| [PZ](['https://store.steampowered.com/app/703080', 'https://en.wikipedia.org/wiki/Planet_Zoo']) | Planet Zoo | - | - | -- -- -- | -- -- -- | -- -- --
| [JW2](['https://store.steampowered.com/app/1244460', 'https://en.wikipedia.org/wiki/Jurassic_World_Evolution_2']) | Jurassic World Evolution 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [F1:22](['https://store.steampowered.com/app/1708520', 'https://en.wikipedia.org/wiki/F1_Manager_2022']) | F1 Manager 2022 | - | - | -- -- -- | -- -- -- | -- -- --
| [W4K:CG](['https://store.steampowered.com/app/1611910', 'https://en.wikipedia.org/wiki/Warhammer_40,000:_Chaos_Gate_-_Daemonhunters']) | Warhammer 40,000: Chaos Gate - Daemonhunters | - | - | -- -- -- | -- -- -- | -- -- --
| [F1:23](['https://store.steampowered.com/app/2287220', 'https://en.wikipedia.org/wiki/F1_Manager_2023']) | F1 Manager 2023 | - | - | -- -- -- | -- -- -- | -- -- --
| [W4K:AoS:RoR](['https://en.wikipedia.org/wiki/Warhammer_Age_of_Sigmar']) | Warhammer Age of Sigmar: Realms of Ruin | - | - | -- -- -- | -- -- -- | -- -- --
| **Id** | **Id**
| [CK:IOTV](['https://en.wikipedia.org/wiki/Commander_Keen_in_Invasion_of_the_Vorticons']) | Commander Keen in Invasion of the Vorticons | - | - | -- -- -- | -- -- -- | -- -- --
| [SK](['https://en.wikipedia.org/wiki/Shadow_Knights']) | Shadow Knights | - | - | -- -- -- | -- -- -- | -- -- --
| [HT3D](['https://en.wikipedia.org/wiki/Hovertank_3D']) | Hovertank 3D | - | - | -- -- -- | -- -- -- | -- -- --
| [DD:THM](['https://en.wikipedia.org/wiki/Dangerous_Dave_in_the_Haunted_Mansion']) | Dangerous Dave in the Haunted Mansion | - | - | -- -- -- | -- -- -- | -- -- --
| [RR](['https://en.wikipedia.org/wiki/Rescue_Rover']) | Rescue Rover | - | - | -- -- -- | -- -- -- | -- -- --
| [CK:KD](['https://en.wikipedia.org/wiki/Commander_Keen_in_Keen_Dreams']) | Commander Keen in Keen Dreams | - | - | -- -- -- | -- -- -- | -- -- --
| [RR2]([]) | Rescue Rover 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [C3D](['https://en.wikipedia.org/wiki/Catacomb_3-D']) | Catacomb 3-D | - | - | -- -- -- | -- -- -- | -- -- --
| [CK:GG](['https://en.wikipedia.org/wiki/Commander_Keen_in_Goodbye,_Galaxy']) | Commander Keen in Goodbye, Galaxy | - | - | -- -- -- | -- -- -- | -- -- --
| [CK:AAMB](['https://en.wikipedia.org/wiki/Commander_Keen_in_Aliens_Ate_My_Babysitter']) | Commander Keen in Aliens Ate My Babysitter | - | - | -- -- -- | -- -- -- | -- -- --
| [W3D](['https://en.wikipedia.org/wiki/Wolfenstein_3D']) | Wolfenstein 3D | - | - | -- -- -- | -- -- -- | -- -- --
| [TOTD](['https://en.wikipedia.org/wiki/Tiles_of_the_Dragon']) | Tiles of the Dragon | - | - | -- -- -- | -- -- -- | -- -- --
| [D1](['https://en.wikipedia.org/wiki/Doom_(1993_video_game)']) | Doom | - | - | -- -- -- | -- -- -- | -- -- --
| [D2](['https://en.wikipedia.org/wiki/Doom_II']) | Doom II | - | - | -- -- -- | -- -- -- | -- -- --
| [Q](['https://store.steampowered.com/app/2310', 'https://en.wikipedia.org/wiki/Quake_(video_game)']) | Quake | - | - | -- -- -- | -- -- -- | -- -- --
| [Q2](['https://store.steampowered.com/app/2320', 'https://en.wikipedia.org/wiki/Quake_II']) | Quake II | - | - | -- -- -- | -- -- -- | -- -- --
| [Q3:A](['https://store.steampowered.com/app/0', 'https://en.wikipedia.org/wiki/Quake_III_Arena']) | Quake III Arena | - | - | -- -- -- | -- -- -- | -- -- --
| [D3](['https://store.steampowered.com/app/9050', 'https://en.wikipedia.org/wiki/Doom_3']) | Doom 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [D:RPG](['https://en.wikipedia.org/wiki/Doom_RPG']) | Doom RPG | - | - | -- -- -- | -- -- -- | -- -- --
| [OE](['https://en.wikipedia.org/wiki/Orcs_%26_Elves']) | Orcs & Elves | - | - | -- -- -- | -- -- -- | -- -- --
| [QE2](['https://doomwiki.org/wiki/Orcs_%26_Elves_II']) | Orcs & Elves II | - | - | -- -- -- | -- -- -- | -- -- --
| [W:RPG](['https://en.wikipedia.org/wiki/Wolfenstein_RPG']) | Wolfenstein RPG | - | - | -- -- -- | -- -- -- | -- -- --
| [D2:RPG](['https://en.wikipedia.org/wiki/Doom_II_RPG']) | Doom II RPG | - | - | -- -- -- | -- -- -- | -- -- --
| [Q:L](['https://store.steampowered.com/app/282440', 'https://en.wikipedia.org/wiki/Quake_Live']) | Quake Live | - | - | -- -- -- | -- -- -- | -- -- --
| [R:MBT](['https://en.wikipedia.org/wiki/Rage:_Mutant_Bash_TV']) | Rage: Mutant Bash TV | - | - | -- -- -- | -- -- -- | -- -- --
| [R](['https://store.steampowered.com/app/0', 'https://en.wikipedia.org/wiki/Rage_(video_game)']) | Rage | - | - | -- -- -- | -- -- -- | -- -- --
| [D](['https://store.steampowered.com/app/0', 'https://en.wikipedia.org/wiki/Doom_(2016_video_game)']) | Doom | - | - | -- -- -- | -- -- -- | -- -- --
| [D:VFR](['https://store.steampowered.com/app/650000', 'https://en.wikipedia.org/wiki/Doom_VFR']) | Doom VFR | - | - | -- -- -- | -- -- -- | -- -- --
| [R2](['https://store.steampowered.com/app/0', 'https://en.wikipedia.org/wiki/Rage_2']) | Rage 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [D:E](['https://store.steampowered.com/app/0', 'https://en.wikipedia.org/wiki/Doom_Eternal']) | Doom Eternal | - | - | -- -- -- | -- -- -- | -- -- --
| [Q:C](['https://store.steampowered.com/app/611500', 'https://en.wikipedia.org/wiki/Quake_Champions']) | Quake Champions | - | - | -- -- -- | -- -- -- | -- -- --
| **IW** | **Infinity Ward**
| [COD](['https://store.steampowered.com/app/2620', 'https://store.steampowered.com/app/2640']) | Call of Duty | - | - | -- -- -- | -- -- -- | -- -- --
| [COD2](['https://store.steampowered.com/app/2630']) | Call of Duty 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [COD3]([]) | Call of Duty 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [COD4](['https://store.steampowered.com/app/7940']) | Call of Duty 4: Modern Warfare | - | - | -- -- -- | -- -- -- | -- -- --
| [007:QoS]([]) | 007: Quantum of Solace | - | - | -- -- -- | -- -- -- | -- -- --
| [WaW](['https://store.steampowered.com/app/10090']) | Call of Duty: World at War | - | - | -- -- -- | -- -- -- | -- -- --
| [MW2](['https://store.steampowered.com/app/10180']) | Call of Duty: Modern Warfare 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [BO](['https://store.steampowered.com/app/42700']) | Call of Duty: Black Ops | - | - | -- -- -- | -- -- -- | -- -- --
| [MW3](['https://store.steampowered.com/app/42680']) | Call of Duty: Modern Warfare 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [BO2](['https://store.steampowered.com/app/202970']) | Call of Duty: Black Ops II | - | - | -- -- -- | -- -- -- | -- -- --
| [Ghosts](['https://store.steampowered.com/app/209160']) | Call of Duty: Ghosts | - | - | -- -- -- | -- -- -- | -- -- --
| [AW](['https://store.steampowered.com/app/209650']) | Call of Duty: Advanced Warfare | - | - | -- -- -- | -- -- -- | -- -- --
| [BO3](['https://store.steampowered.com/app/311210']) | Call of Duty: Black Ops III | - | - | -- -- -- | -- -- -- | -- -- --
| [IW](['https://store.steampowered.com/app/292730']) | Call of Duty: Infinite Warfare | - | - | -- -- -- | -- -- -- | -- -- --
| [WWII](['https://store.steampowered.com/app/476600']) | Call of Duty: WWII | - | - | -- -- -- | -- -- -- | -- -- --
| [BO4](['https://us.shop.battle.net/en-us/product/call-of-duty-black-ops-4']) | Call of Duty: Black Ops 4 | - | - | -- -- -- | -- -- -- | -- -- --
| [MW](['https://store.steampowered.com/app/393080']) | Call of Duty: Modern Warfare | - | - | -- -- -- | -- -- -- | -- -- --
| [BOCW](['https://us.shop.battle.net/en-us/product/call-of-duty-black-ops-cold-war']) | Call of Duty: Black Ops Cold War | - | - | -- -- -- | -- -- -- | -- -- --
| [Vanguard](['https://us.shop.battle.net/en-us/product/call-of-duty-vanguard']) | Call of Duty: Vanguard | - | - | -- -- -- | -- -- -- | -- -- --
| [COD:MW2](['https://store.steampowered.com/app/1938090']) | Call of Duty: Modern Warfare II | - | - | -- -- -- | -- -- -- | -- -- --
| **Monolith** | **MonolithTech**
| [FEAR](['https://store.steampowered.com/app/21090']) | F.E.A.R. | - | - | -- -- -- | -- -- -- | -- -- --
| [FEAR:EP](['https://store.steampowered.com/app/21110']) | F.E.A.R.: Extraction Point | - | - | -- -- -- | -- -- -- | -- -- --
| [FEAR:PM](['https://store.steampowered.com/app/21120']) | F.E.A.R.: Perseus Mandate | - | - | -- -- -- | -- -- -- | -- -- --
| [FEAR2](['https://store.steampowered.com/app/16450']) | F.E.A.R. 2: Project Origin | - | - | -- -- -- | -- -- -- | -- -- --
| [FEAR3](['https://store.steampowered.com/app/21100']) | F.E.A.R. 3 | - | - | -- -- -- | -- -- -- | -- -- --
| **Origin** | **Origin Systems**
| [UO](['https://uo.com/client-download/']) | Ultima Online | - | - | -- -- -- | -- -- -- | -- -- --
| [U9](['https://www.gog.com/en/game/ultima_9_ascension']) | Ultima IX | - | - | -- -- -- | -- -- -- | -- -- --
| **Red** | **REDengine**
| [Witcher](['https://www.gog.com/en/game/the_witcher']) | The Witcher Enhanced Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [Witcher2](['https://www.gog.com/en/game/the_witcher_2']) | The Witcher 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [Witcher3](['https://www.gog.com/en/game/the_witcher_3_wild_hunt', 'https://www.gog.com/en/game/the_witcher_3_wild_hunt_game_of_the_year_edition']) | The Witcher 3: Wild Hunt | - | - | -- -- -- | -- -- -- | -- -- --
| [CP77](['https://store.steampowered.com/app/1091500']) | Cyberpunk 2077 | - | - | -- -- -- | -- -- -- | -- -- --
| [Witcher4](['https://en.wikipedia.org/wiki/The_Witcher_(video_game_series)']) | The Witcher 4 Polaris (future) | - | - | -- -- -- | -- -- -- | -- -- --
| **Unity** | **Unity**
| [AmongUs](['https://store.steampowered.com/app/945360']) | Among Us | - | - | -- -- -- | -- -- -- | -- -- --
| [Cities](['https://store.steampowered.com/app/255710']) | Cities: Skylines | - | - | -- -- -- | -- -- -- | -- -- --
| [Tabletop](['https://store.steampowered.com/app/286160']) | Tabletop Simulator | - | - | -- -- -- | -- -- -- | -- -- --
| [UBoat](['https://store.steampowered.com/app/1272010']) | Destroyer: The U-Boat Hunter | - | - | -- -- -- | -- -- -- | -- -- --
| [7D2D](['https://store.steampowered.com/app/251570']) | 7 Days to Die | - | - | -- -- -- | -- -- -- | -- -- --
| **Unknown** | **Unknown**
| [ACO](['https://store.steampowered.com/app/582160']) | Assassin's Creed Origins | - | - | -- -- -- | -- -- -- | -- -- --
| [APP]([]) | Application | - | - | -- -- -- | -- -- -- | -- -- --
| [CAT]([]) | Catalog | - | - | -- -- -- | -- -- -- | -- -- --
| **Unreal** | **Unreal**
| [UE1](['https://oldgamesdownload.com/unreal']) | Unreal | - | - | -- -- -- | -- -- -- | -- -- --
| [TWoT](['https://www.gog.com/en/game/the_wheel_of_time']) | The Wheel of Time | - | - | -- -- -- | -- -- -- | -- -- --
| [DeusEx](['https://www.gog.com/en/game/deus_ex']) | Deus Ex™ GOTY Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [DeusEx:MD](['https://www.gog.com/en/game/deus_ex_mankind_divided']) | Deus Ex: Mankind Divided | - | - | -- -- -- | -- -- -- | -- -- --
| [DeusEx2:IW](['https://www.gog.com/en/game/deus_ex_invisible_war']) | Deus Ex 2: Invisible War | - | - | -- -- -- | -- -- -- | -- -- --
| [DeusEx:HR](['https://www.gog.com/en/game/deus_ex_human_revolution_directors_cut']) | Deus Ex: Human Revolution - Director’s Cut | - | - | -- -- -- | -- -- -- | -- -- --
| [Rune](['https://www.gog.com/en/game/rune_classic']) | Rune | - | - | -- -- -- | -- -- -- | -- -- --
| [Undying](['https://www.gog.com/en/game/clive_barkers_undying']) | Clive Barker's Undying | - | - | -- -- -- | -- -- -- | -- -- --
| [UT2K](['https://oldgamesdownload.com/unreal-tournament-2003/']) | Unreal Tournament 2003 | - | - | -- -- -- | -- -- -- | -- -- --
| [UE2](['https://store.steampowered.com/app/13200/Unreal_2_The_Awakening/']) | Unreal II: The Awakening | - | - | -- -- -- | -- -- -- | -- -- --
| [BioShock](['https://store.steampowered.com/app/7670']) | BioShock | - | - | -- -- -- | -- -- -- | -- -- --
| [BioShockR](['https://store.steampowered.com/app/409710']) | BioShock Remastered | - | - | -- -- -- | -- -- -- | -- -- --
| [BioShock2](['https://store.steampowered.com/app/8850']) | BioShock 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [BioShock2R](['https://store.steampowered.com/app/409720']) | BioShock 2 Remastered | - | - | -- -- -- | -- -- -- | -- -- --
| [BioShock:Inf](['https://store.steampowered.com/app/8870']) | BioShock Infinite | - | - | -- -- -- | -- -- -- | -- -- --
| **Valve** | **Valve**
| [HL](['https://store.steampowered.com/app/70']) | Half-Life | open | read | gl -- -- | -- -- -- | -- -- --
| [TF](['https://store.steampowered.com/app/20']) | Team Fortress Classic | open | read | gl -- -- | -- -- -- | -- -- --
| [HL:OF](['https://store.steampowered.com/app/20']) | Half-Life: Opposing Force | open | read | -- -- -- | -- -- -- | -- -- --
| [CS](['https://store.steampowered.com/app/10']) | Counter-Strike | open | read | -- -- -- | -- -- -- | -- -- --
| [Ricochet](['https://store.steampowered.com/app/60']) | Ricochet | open | read | -- -- -- | -- -- -- | -- -- --
| [DM](['https://store.steampowered.com/app/40']) | Deathmatch Classic | open | read | -- -- -- | -- -- -- | -- -- --
| [HL:BS](['https://store.steampowered.com/app/130']) | Half-Life: Blue Shift | open | read | -- -- -- | -- -- -- | -- -- --
| [DOD](['https://store.steampowered.com/app/30']) | Day of Defeat | open | read | -- -- -- | -- -- -- | -- -- --
| [CS:CZ](['https://store.steampowered.com/app/80']) | Counter-Strike: Condition Zero | open | read | -- -- -- | -- -- -- | -- -- --
| [HL:Src](['https://store.steampowered.com/app/280']) | Half-Life: Source | open | read | -- -- -- | -- -- -- | -- -- --
| [CS:Src](['https://store.steampowered.com/app/240']) | Counter-Strike: Source | open | read | -- -- -- | -- -- -- | -- -- --
| [HL2](['https://store.steampowered.com/app/220']) | Half-Life 2 | open | read | -- -- -- | -- -- -- | -- -- --
| [HL2:DM](['https://store.steampowered.com/app/320']) | Half-Life 2: Deathmatch | open | read | -- -- -- | -- -- -- | -- -- --
| [HL:DM:Src](['https://store.steampowered.com/app/360']) | Half-Life Deathmatch: Source | open | read | -- -- -- | -- -- -- | -- -- --
| [HL2:E1](['https://store.steampowered.com/app/380']) | Half-Life 2: Episode One | open | read | -- -- -- | -- -- -- | -- -- --
| [Portal](['https://store.steampowered.com/app/400']) | Portal | open | read | -- -- -- | -- -- -- | -- -- --
| [HL2:E2](['https://store.steampowered.com/app/420']) | Half-Life 2: Episode Two | open | read | -- -- -- | -- -- -- | -- -- --
| [TF2](['https://store.steampowered.com/app/440']) | Team Fortress 2 | open | read | -- -- -- | -- -- -- | -- -- --
| [L4D](['https://store.steampowered.com/app/500']) | Left 4 Dead | open | read | -- -- -- | -- -- -- | -- -- --
| [L4D2](['https://store.steampowered.com/app/550']) | Left 4 Dead 2 | open | read | -- -- -- | -- -- -- | -- -- --
| [DOD:Src](['https://store.steampowered.com/app/300']) | Day of Defeat: Source | open | read | -- -- -- | -- -- -- | -- -- --
| [Portal2](['https://store.steampowered.com/app/620']) | Portal 2 | open | read | -- -- -- | -- -- -- | -- -- --
| [CS:GO](['https://store.steampowered.com/app/730']) | Counter-Strike: Global Offensive | open | read | -- -- -- | -- -- -- | -- -- --
| [D2](['https://store.steampowered.com/app/570']) | Dota 2 | open | read | gl -- -- | -- -- -- | -- -- --
| [TheLab:RR](['https://store.steampowered.com/app/450390']) | The Lab: Robot Repair | open | read | gl -- -- | gl -- -- | -- -- --
| [TheLab:SS](['https://store.steampowered.com/app/450390']) | The Lab: Secret Shop | - | - | -- -- -- | -- -- -- | -- -- --
| [TheLab:TL](['https://store.steampowered.com/app/450390']) | The Lab: The Lab | - | - | -- -- -- | -- -- -- | -- -- --
| [HL:Alyx](['https://store.steampowered.com/app/546560']) | Half-Life: Alyx | open | read | gl -- -- | gl -- -- | -- -- --
| **WbB** | **WB Games Boston**
| [AC](['https://en.wikipedia.org/wiki/Asheron%27s_Call', 'https://emulator.ac/how-to-play/', 'http://content.turbine.com/sites/clientdl/ac1/ac1install.exe', 'http://www.thwargle.com/']) | Asheron's Call | open | read | gl -- -- | -- -- -- | -- -- --
| [AC2](['https://en.wikipedia.org/wiki/Asheron%27s_Call_2:_Fallen_Kings']) | Asheron's Call 2: Fallen Kings | - | - | -- -- -- | -- -- -- | -- -- --
| [DDO](['https://en.wikipedia.org/wiki/Dungeons_%26_Dragons_Online']) | Dungeons & Dragons Online | - | - | -- -- -- | -- -- -- | -- -- --
| [LotRO](['https://en.wikipedia.org/wiki/The_Lord_of_the_Rings_Online']) | The Lord of the Rings Online | - | - | -- -- -- | -- -- -- | -- -- --
| [IC](['https://en.wikipedia.org/wiki/Infinite_Crisis_(video_game)']) | Infinite Crisis | - | - | -- -- -- | -- -- -- | -- -- --
| [B:AU](['https://en.wikipedia.org/wiki/Asheron%27s_Call_2:_Fallen_Kings']) | Batman: Arkham Underworld | - | - | -- -- -- | -- -- -- | -- -- --
| [GoT:C](['https://warnerbrosgames.com/game/game-of-thrones-conquest']) | Game of Thrones: Conquest | - | - | -- -- -- | -- -- -- | -- -- --
