Game Specs
===

Game Specs is an open-source, cross-platform solution for delivering game assets as a service.

### High level steps:
1. locate installed games.
2. open game specific archive files.
3. parse game objects from specific file formats for textures, models, levels.
4. adapt game objects to a gaming platform like unity.

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
Family Context

## [Families](docs/Families/Readme.md)
Families are the primary grouping mechanism for interacting with the asset services.

Usually file formats center around the game developer or game engine being used, and are modified, instead of replaced, as the studio releases new versions.

The following are the current familes:

| ID                                               | Name                      | Sample Game       | Status
| --                                               | --                        | --                | --
| [AC](Documents/Families/AC/Readme.md)            | Asheron's Call            | Asheron's Call    | In Development
| [Arkane](Documents/Families/Arkane/Readme.md)    | Arkane Studios            | Dishonored 2      | In Development
| [Bioware](Documents/Families/Bioware/Readme.md)  | BioWare Bioware           | Neverwinter Nights| In Development
| [Blizzard](Documents/Families/Blizzard/Readme.md)| Blizzard                  | StarCraft         | In Development
| [Cry](Documents/Families/Cry/Readme.md)          | Crytek                    | MechWarrior Online| In Development
| [Cryptic](Documents/Families/Cryptic/Readme.md)  | Cryptic                   | Star Trek Online  | In Development
| [Cyanide](Documents/Families/Cyanide/Readme.md)  | Cyanide Formats           | The Council       | In Development
| [Hpl](Documents/Families/Hpl/Readme.md)          | Frictional Games          | SOMA              | In Development
| [Id](Documents/Families/Id/Readme.md)            | id Software               | Doom              | In Development
| [IW](Documents/Families/IW/Readme.md)            | Infinity Ward             | Call of Duty      | In Development
| [Lith](Documents/Families/Lith/Readme.md)        | Monolith                  | F.E.A.R.          | In Development
| [Origin](Documents/Families/Origin/Readme.md)    | Origin Systems            | Ultima Online     | In Development
| [Red](Documents/Families/Red/Readme.md)          | REDengine                 | The Witcher 3: Wild Hunt | In Development
| [Rsi](Documents/Families/Rsi/Readme.md)          | Roberts Space Industries  | Star Citizen      | In Development
| [Tes](Documents/Families/Tes/Readme.md)          | The Elder Scrolls         | Skyrim            | In Development
| [Unity](Documents/Families/Unity/Readme.md)      | Unity                     | AmongUs           | In Development
| [Unknown](Documents/Families/Unknown/Readme.md)  | Unknown                   | N/A               | In Development
| [Unreal](Documents/Families/Unreal/Readme.md)    | Unreal                    | BioShock          | In Development
| [Valve](Documents/Families/Valve/Readme.md)      | Valve                     | Dota 2            | In Development

## [Platforms](Documents/Platforms/Readme.md)
Platforms provide the interface to each gaming platform.

## Games
---
The following are the current games:

| ID | Name | Open | Read | Texure | Model | Level
| -- | --   | --   | --   | --     | --    | --
| **AC** | **Asheron's Call**
| [AC](https://emulator.ac/how-to-play/) | Asheron's Call | open | read | -- -- -- | -- -- -- | -- -- --
| **Arkane** | **Arkane Studios**
| [AF](https://www.gog.com/en/game/arx_fatalis) | Arx Fatalis | open | read | -- -- -- | -- -- -- | -- -- --
| [DOM](https://store.steampowered.com/app/2100) | Dark Messiah of Might and Magic | open | read | -- -- -- | -- -- -- | -- -- --
| [D](https://www.gog.com/en/game/dishonored_definitive_edition) | Dishonored | - | - | -- -- -- | -- -- -- | -- -- --
| [D2](https://www.gog.com/index.php/game/dishonored_2) | Dishonored 2 | open | read | -- -- -- | -- -- -- | -- -- --
| [P](https://www.gog.com/en/game/prey) | Prey | open | read | -- -- -- | -- -- -- | -- -- --
| [D:DOTO](https://www.gog.com/en/game/dishonored_death_of_the_outsider) | Dishonored: Death of the Outsider | - | - | -- -- -- | -- -- -- | -- -- --
| [W:YB](https://store.steampowered.com/app/1056960) | Wolfenstein: Youngblood | - | - | -- -- -- | -- -- -- | -- -- --
| [W:CP](https://store.steampowered.com/app/1056970) | Wolfenstein: Cyberpilot | - | - | -- -- -- | -- -- -- | -- -- --
| [DL](https://store.steampowered.com/app/1252330) | Deathloop | - | - | -- -- -- | -- -- -- | -- -- --
| [RF](https://bethesda.net/en/game/redfall) | Redfall (future) | - | - | -- -- -- | -- -- -- | -- -- --
| **Bioware** | **BioWare**
| [SS](https://www.gog.com/en/game/shattered_steel) | Shattered Steel | - | - | -- -- -- | -- -- -- | -- -- --
| [BG](https://www.gog.com/en/game/baldurs_gate_enhanced_edition) | Baldur's Gate | - | - | -- -- -- | -- -- -- | -- -- --
| [MDK2](https://www.gog.com/en/game/mdk_2) | MDK2 | - | - | -- -- -- | -- -- -- | -- -- --
| [BG2](https://www.gog.com/en/game/baldurs_gate_2_enhanced_edition) | Baldur's Gate II: Shadows of Amn | - | - | -- -- -- | -- -- -- | -- -- --
| [NWN](https://store.steampowered.com/app/704450) | Neverwinter Nights | - | - | -- -- -- | -- -- -- | -- -- --
| [KotOR](https://store.steampowered.com/app/32370) | Star Wars: Knights of the Old Republic | - | - | -- -- -- | -- -- -- | -- -- --
| [JE](https://www.gog.com/en/game/jade_empire_special_edition) | Jade Empire | - | - | -- -- -- | -- -- -- | -- -- --
| [ME](https://store.steampowered.com/app/1328670) | Mass Effect | - | - | -- -- -- | -- -- -- | -- -- --
| [NWN2](https://www.gog.com/en/game/neverwinter_nights_2_complete) | Neverwinter Nights 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [DA:O](https://store.steampowered.com/app/47810) | Dragon Age: Origins | - | - | -- -- -- | -- -- -- | -- -- --
| [ME2](https://store.steampowered.com/app/24980) | Mass Effect 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [DA2](https://store.steampowered.com/app/1238040) | Dragon Age II | - | - | -- -- -- | -- -- -- | -- -- --
| [SWTOR](https://store.steampowered.com/app/1286830) | Star Wars: The Old Republic | - | - | -- -- -- | -- -- -- | -- -- --
| [ME3](https://store.steampowered.com/app/1238020) | Mass Effect 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [DA:I](https://store.steampowered.com/app/1222690) | Dragon Age: Inquisition | - | - | -- -- -- | -- -- -- | -- -- --
| [ME:A](https://store.steampowered.com/app/1238000) | Mass Effect: Andromeda | - | - | -- -- -- | -- -- -- | -- -- --
| [A](https://www.ea.com/games/anthem/buy/pc) | Anthem | - | - | -- -- -- | -- -- -- | -- -- --
| [ME:LE](https://store.steampowered.com/app/1328670) | Mass Effect: Legendary Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [DA:D](https://www.ea.com/en-gb/games/dragon-age/dragon-age-dreadwolf) | Dragon Age: Dreadwolf (future) | - | - | -- -- -- | -- -- -- | -- -- --
| [ME5](https://en.wikipedia.org/wiki/Mass_Effect) | Mass Effect 5 (future) | - | - | -- -- -- | -- -- -- | -- -- --
| **Blizzard** | **Blizzard**
| [SC](https://us.shop.battle.net/en-us/product/starcraft) | StarCraft | - | - | -- -- -- | -- -- -- | -- -- --
| [D2R](https://us.shop.battle.net/en-us/product/diablo_ii_resurrected) | Diablo II: Resurrected | - | - | -- -- -- | -- -- -- | -- -- --
| [W3](https://us.shop.battle.net/en-us/product/warcraft-iii-reforged) | Warcraft III: Reign of Chaos | - | - | -- -- -- | -- -- -- | -- -- --
| [WOW](https://us.shop.battle.net/en-us/family/world-of-warcraft) | World of Warcraft | - | - | -- -- -- | -- -- -- | -- -- --
| [WOWC](https://us.shop.battle.net/en-us/family/world-of-warcraft-classic) | World of Warcraft: Classic | - | - | -- -- -- | -- -- -- | -- -- --
| [SC2](https://us.shop.battle.net/en-us/product/starcraft-ii) | StarCraft II: Wings of Liberty | - | - | -- -- -- | -- -- -- | -- -- --
| [D3](https://us.shop.battle.net/en-us/product/diablo-iii) | Diablo III | - | - | -- -- -- | -- -- -- | -- -- --
| [HS](https://us.shop.battle.net/en-us/product/hearthstone-heroes-of-warcraft) | Hearthstone | - | - | -- -- -- | -- -- -- | -- -- --
| [HOTS]() | Heroes of the Storm | - | - | -- -- -- | -- -- -- | -- -- --
| [CB](https://us.shop.battle.net/en-us/family/crash-bandicoot-4) | Crash Bandicoot� 4: It�s About Time | - | - | -- -- -- | -- -- -- | -- -- --
| [DI](https://diabloimmortal.blizzard.com/en-us/) | Diablo Immortal | - | - | -- -- -- | -- -- -- | -- -- --
| [OW2](https://us.shop.battle.net/en-us/product/overwatch) | Overwatch 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [D4](https://diablo4.blizzard.com/en-us/) | Diablo IV | - | - | -- -- -- | -- -- -- | -- -- --
| **Cry** | **Crytek**
| [ArcheAge](https://store.steampowered.com/app/304030) | ArcheAge | - | - | -- -- -- | -- -- -- | -- -- --
| [Hunt](https://store.steampowered.com/app/594650) | Hunt: Showdown | - | - | -- -- -- | -- -- -- | -- -- --
| [MWO](https://store.steampowered.com/app/342200) | MechWarrior Online | - | - | -- -- -- | -- -- -- | -- -- --
| [Warface](https://store.steampowered.com/app/291480) | Warface | - | - | -- -- -- | -- -- -- | -- -- --
| [Wolcen](https://store.steampowered.com/app/424370) | Wolcen: Lords of Mayhem | - | - | -- -- -- | -- -- -- | -- -- --
| [Crysis](https://store.steampowered.com/app/1715130) | Crysis Remastered | - | - | -- -- -- | -- -- -- | -- -- --
| [Ryse](https://store.steampowered.com/app/302510) | Ryse: Son of Rome | - | - | -- -- -- | -- -- -- | -- -- --
| [Robinson](https://store.steampowered.com/app/579820) | Robinson: The Journey | - | - | -- -- -- | -- -- -- | -- -- --
| [Snow](https://store.steampowered.com/app/244930) | SNOW - The Ultimate Edition | - | - | -- -- -- | -- -- -- | -- -- --
| **Cryptic** | **Cryptic**
| [CO](https://store.steampowered.com/app/9880) | Champions Online | open | read | -- -- -- | -- -- -- | -- -- --
| [STO](https://store.steampowered.com/app/9900) | Star Trek Online | open | read | -- -- -- | -- -- -- | -- -- --
| [NVW](https://store.steampowered.com/app/109600) | Neverwinter | open | read | -- -- -- | -- -- -- | -- -- --
| **Cyanide** | **Cyanide**
| [TC](https://store.steampowered.com/app/287630) | The Council | - | - | -- -- -- | -- -- -- | -- -- --
| [Werewolf:TA](https://store.steampowered.com/app/679110) | Werewolf: The Apocalypse - Earthblood | - | - | -- -- -- | -- -- -- | -- -- --
| **HPL** | **HPL Engine**
| [P:O](https://store.steampowered.com/app/22180) | Penumbra: Overture | - | - | -- -- -- | -- -- -- | -- -- --
| [P:BP](https://store.steampowered.com/app/22120) | Penumbra: Black Plague | - | - | -- -- -- | -- -- -- | -- -- --
| [P:R](https://store.steampowered.com/app/22140) | Penumbra: Requiem | - | - | -- -- -- | -- -- -- | -- -- --
| [A:TDD](https://store.steampowered.com/app/57300) | Amnesia: The Dark Descent | - | - | -- -- -- | -- -- -- | -- -- --
| [A:AMFP](https://store.steampowered.com/app/239200) | Amnesia: A Machine for Pigs | - | - | -- -- -- | -- -- -- | -- -- --
| [SOMA](https://store.steampowered.com/app/282140) | SOMA | - | - | -- -- -- | -- -- -- | -- -- --
| [A:R](https://store.steampowered.com/app/999220) | Amnesia: Rebirth | - | - | -- -- -- | -- -- -- | -- -- --
| **Id** | **Id**
| [Q](https://store.steampowered.com/app/2310) | Quake | - | - | -- -- -- | -- -- -- | -- -- --
| [Q2](https://store.steampowered.com/app/2320) | Quake II | - | - | -- -- -- | -- -- -- | -- -- --
| [Q3:A](https://store.steampowered.com/app/0) | Quake III Arena | - | - | -- -- -- | -- -- -- | -- -- --
| [D3](https://store.steampowered.com/app/9050) | Doom 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [Q:L](https://store.steampowered.com/app/0) | Quake Live | - | - | -- -- -- | -- -- -- | -- -- --
| [R](https://store.steampowered.com/app/0) | Rage | - | - | -- -- -- | -- -- -- | -- -- --
| [D](https://store.steampowered.com/app/0) | Doom | - | - | -- -- -- | -- -- -- | -- -- --
| [D:VFR](https://store.steampowered.com/app/650000) | Doom VFR | - | - | -- -- -- | -- -- -- | -- -- --
| [R2](https://store.steampowered.com/app/0) | Rage 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [D:E](https://store.steampowered.com/app/0) | Doom Eternal | - | - | -- -- -- | -- -- -- | -- -- --
| [Q:C](https://store.steampowered.com/app/0) | Quake Champions | - | - | -- -- -- | -- -- -- | -- -- --
| **IW** | **Infinity Ward**
| [COD](https://store.steampowered.com/app/2620) | Call of Duty | - | - | -- -- -- | -- -- -- | -- -- --
| [COD2](https://store.steampowered.com/app/2630) | Call of Duty 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [COD4](https://store.steampowered.com/app/7940) | Call of Duty 4: Modern Warfare | - | - | -- -- -- | -- -- -- | -- -- --
| [WaW](https://store.steampowered.com/app/10090) | Call of Duty: World at War | - | - | -- -- -- | -- -- -- | -- -- --
| [MW2](https://store.steampowered.com/app/10180) | Call of Duty: Modern Warfare 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [BO](https://store.steampowered.com/app/42700) | Call of Duty: Black Ops | - | - | -- -- -- | -- -- -- | -- -- --
| [MW3](https://store.steampowered.com/app/42680) | Call of Duty: Modern Warfare 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [BO2](https://store.steampowered.com/app/202970) | Call of Duty: Black Ops II | - | - | -- -- -- | -- -- -- | -- -- --
| [Ghosts](https://store.steampowered.com/app/209160) | Call of Duty: Ghosts | - | - | -- -- -- | -- -- -- | -- -- --
| [AW](https://store.steampowered.com/app/209650) | Call of Duty: Advanced Warfare | - | - | -- -- -- | -- -- -- | -- -- --
| [BO3](https://store.steampowered.com/app/311210) | Call of Duty: Black Ops III | - | - | -- -- -- | -- -- -- | -- -- --
| [IW](https://store.steampowered.com/app/292730) | Call of Duty: Infinite Warfare | - | - | -- -- -- | -- -- -- | -- -- --
| [WWII](https://store.steampowered.com/app/476600) | Call of Duty: WWII | - | - | -- -- -- | -- -- -- | -- -- --
| [BO4](https://us.shop.battle.net/en-us/product/call-of-duty-black-ops-4) | Call of Duty: Black Ops 4 | - | - | -- -- -- | -- -- -- | -- -- --
| [MW](https://store.steampowered.com/app/393080) | Call of Duty: Modern Warfare | - | - | -- -- -- | -- -- -- | -- -- --
| [BOCW](https://us.shop.battle.net/en-us/product/call-of-duty-black-ops-cold-war) | Call of Duty: Black Ops Cold War | - | - | -- -- -- | -- -- -- | -- -- --
| [Vanguard](https://us.shop.battle.net/en-us/product/call-of-duty-vanguard) | Call of Duty: Vanguard | - | - | -- -- -- | -- -- -- | -- -- --
| [COD:MW2](https://store.steampowered.com/app/1938090) | Call of Duty: Modern Warfare II | - | - | -- -- -- | -- -- -- | -- -- --
| **Lith** | **LithTech**
| [FEAR](https://store.steampowered.com/app/21090) | F.E.A.R. | - | - | -- -- -- | -- -- -- | -- -- --
| [FEAR:EP](https://store.steampowered.com/app/21110) | F.E.A.R.: Extraction Point | - | - | -- -- -- | -- -- -- | -- -- --
| [FEAR:PM](https://store.steampowered.com/app/21120) | F.E.A.R.: Perseus Mandate | - | - | -- -- -- | -- -- -- | -- -- --
| [FEAR2](https://store.steampowered.com/app/16450) | F.E.A.R. 2: Project Origin | - | - | -- -- -- | -- -- -- | -- -- --
| [FEAR3](https://store.steampowered.com/app/21100) | F.E.A.R. 3 | - | - | -- -- -- | -- -- -- | -- -- --
| **Origin** | **Origin Systems**
| [UO](https://uo.com/client-download/) | Ultima Online | - | - | -- -- -- | -- -- -- | -- -- --
| [UltimaIX](https://www.gog.com/en/game/ultima_9_ascension) | Ultima IX | - | - | -- -- -- | -- -- -- | -- -- --
| **Red** | **REDengine**
| [Witcher](https://www.gog.com/en/game/the_witcher) | The Witcher Enhanced Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [Witcher2](https://www.gog.com/en/game/the_witcher_2) | The Witcher 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [Witcher3](https://www.gog.com/en/game/the_witcher_3_wild_hunt) | The Witcher 3: Wild Hunt | - | - | -- -- -- | -- -- -- | -- -- --
| [CP77](https://store.steampowered.com/app/1091500) | Cyberpunk 2077 | - | - | -- -- -- | -- -- -- | -- -- --
| [Witcher4](https://en.wikipedia.org/wiki/The_Witcher_(video_game_series)) | The Witcher 4 Polaris (future) | - | - | -- -- -- | -- -- -- | -- -- --
| **Rsi** | **Roberts Space Industries**
| [StarCitizen](https://robertsspaceindustries.com/playstarcitizen) | Star Citizen | - | - | -- -- -- | -- -- -- | -- -- --
| **Tes** | **The Elder Scrolls**
| [Fallout2](https://store.steampowered.com/app/38410) | Fallout 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [Morrowind](https://store.steampowered.com/app/22320) | The Elder Scrolls III: Morrowind | - | - | -- -- -- | -- -- -- | -- -- --
| [Oblivion](https://store.steampowered.com/app/22330) | The Elder Scrolls IV: Oblivion | - | - | -- -- -- | -- -- -- | -- -- --
| [Fallout3](https://store.steampowered.com/app/22370) | Fallout 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [FalloutNV](https://store.steampowered.com/app/22380) | Fallout New Vegas | - | - | -- -- -- | -- -- -- | -- -- --
| [Skyrim](https://store.steampowered.com/app/72850) | The Elder Scrolls V: Skyrim | - | - | -- -- -- | -- -- -- | -- -- --
| [Fallout4](https://store.steampowered.com/app/377160) | Fallout 4 | - | - | -- -- -- | -- -- -- | -- -- --
| [SkyrimSE](https://store.steampowered.com/app/489830) | The Elder Scrolls V: Skyrim � Special Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [Fallout:S](https://store.steampowered.com/app/588430) | Fallout Shelter | - | - | -- -- -- | -- -- -- | -- -- --
| [Fallout4VR](https://store.steampowered.com/app/611660) | Fallout 4 VR | - | - | -- -- -- | -- -- -- | -- -- --
| [SkyrimVR](https://store.steampowered.com/app/611670) | The Elder Scrolls V: Skyrim VR | - | - | -- -- -- | -- -- -- | -- -- --
| [Fallout76](https://store.steampowered.com/app/1151340) | Fallout 76 | - | - | -- -- -- | -- -- -- | -- -- --
| [Starfield](https://store.steampowered.com/app/1716740) | Starfield (future) | - | - | -- -- -- | -- -- -- | -- -- --
| [Unknown1](https://en.wikipedia.org/wiki/The_Elder_Scrolls) | The Elder Scrolls VI (future) | - | - | -- -- -- | -- -- -- | -- -- --
| [Fallout5](https://en.wikipedia.org/wiki/Fallout_(series)) | Fallout 5 (future) | - | - | -- -- -- | -- -- -- | -- -- --
| **Unity** | **Unity**
| [AmongUs](https://store.steampowered.com/app/945360) | Among Us | - | - | -- -- -- | -- -- -- | -- -- --
| [Cities](https://store.steampowered.com/app/255710) | Cities: Skylines | - | - | -- -- -- | -- -- -- | -- -- --
| [Tabletop](https://store.steampowered.com/app/286160) | Tabletop Simulator | - | - | -- -- -- | -- -- -- | -- -- --
| [UBoat](https://store.steampowered.com/app/1272010) | Destroyer: The U-Boat Hunter | - | - | -- -- -- | -- -- -- | -- -- --
| [7D2D](https://store.steampowered.com/app/251570) | 7 Days to Die | - | - | -- -- -- | -- -- -- | -- -- --
| **Unknown** | **Unknown**
| [ACO](https://store.steampowered.com/app/582160) | Assassin's Creed Origins | - | - | -- -- -- | -- -- -- | -- -- --
| [CAT]() | Catalog | - | - | -- -- -- | -- -- -- | -- -- --
| **Unreal** | **Unreal**
| [BioShock](https://store.steampowered.com/app/7670) | BioShock | - | - | -- -- -- | -- -- -- | -- -- --
| [BioShockR](https://store.steampowered.com/app/409710) | BioShock Remastered | - | - | -- -- -- | -- -- -- | -- -- --
| [BioShock2](https://store.steampowered.com/app/8850) | BioShock 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [BioShock2R](https://store.steampowered.com/app/409720) | BioShock 2 Remastered | - | - | -- -- -- | -- -- -- | -- -- --
| [BioShock:Inf](https://store.steampowered.com/app/8870) | BioShock Infinite | - | - | -- -- -- | -- -- -- | -- -- --
| **Valve** | **Valve**
| [HL](https://store.steampowered.com/app/70) | Half-Life | open | read | gl -- -- | -- -- -- | -- -- --
| [TF](https://store.steampowered.com/app/20) | Team Fortress Classic | open | read | gl -- -- | -- -- -- | -- -- --
| [HL:OF](https://store.steampowered.com/app/20) | Half-Life: Opposing Force | open | read | -- -- -- | -- -- -- | -- -- --
| [CS](https://store.steampowered.com/app/10) | Counter-Strike | open | read | -- -- -- | -- -- -- | -- -- --
| [Ricochet](https://store.steampowered.com/app/60) | Ricochet | open | read | -- -- -- | -- -- -- | -- -- --
| [DM](https://store.steampowered.com/app/40) | Deathmatch Classic | open | read | -- -- -- | -- -- -- | -- -- --
| [HL:BS](https://store.steampowered.com/app/130) | Half-Life: Blue Shift | open | read | -- -- -- | -- -- -- | -- -- --
| [DOD](https://store.steampowered.com/app/30) | Day of Defeat | open | read | -- -- -- | -- -- -- | -- -- --
| [CS:CZ](https://store.steampowered.com/app/80) | Counter-Strike: Condition Zero | open | read | -- -- -- | -- -- -- | -- -- --
| [HL:Src](https://store.steampowered.com/app/280) | Half-Life: Source | open | read | -- -- -- | -- -- -- | -- -- --
| [CS:Src](https://store.steampowered.com/app/240) | Counter-Strike: Source | open | read | -- -- -- | -- -- -- | -- -- --
| [HL2](https://store.steampowered.com/app/220) | Half-Life 2 | open | read | -- -- -- | -- -- -- | -- -- --
| [HL2:DM](https://store.steampowered.com/app/320) | Half-Life 2: Deathmatch | open | read | -- -- -- | -- -- -- | -- -- --
| [HL:DM:Src](https://store.steampowered.com/app/360) | Half-Life Deathmatch: Source | open | read | -- -- -- | -- -- -- | -- -- --
| [HL2:E1](https://store.steampowered.com/app/380) | Half-Life 2: Episode One | open | read | -- -- -- | -- -- -- | -- -- --
| [Portal](https://store.steampowered.com/app/400) | Portal | open | read | -- -- -- | -- -- -- | -- -- --
| [HL2:E2](https://store.steampowered.com/app/420) | Half-Life 2: Episode Two | open | read | -- -- -- | -- -- -- | -- -- --
| [TF2](https://store.steampowered.com/app/440) | Team Fortress 2 | open | read | -- -- -- | -- -- -- | -- -- --
| [L4D](https://store.steampowered.com/app/500) | Left 4 Dead | open | read | -- -- -- | -- -- -- | -- -- --
| [L4D2](https://store.steampowered.com/app/550) | Left 4 Dead 2 | open | read | -- -- -- | -- -- -- | -- -- --
| [DOD:Src](https://store.steampowered.com/app/300) | Day of Defeat: Source | open | read | -- -- -- | -- -- -- | -- -- --
| [Portal2](https://store.steampowered.com/app/620) | Portal 2 | open | read | -- -- -- | -- -- -- | -- -- --
| [CS:GO](https://store.steampowered.com/app/730) | Counter-Strike: Global Offensive | open | read | -- -- -- | -- -- -- | -- -- --
| [D2](https://store.steampowered.com/app/570) | Dota 2 | open | - | -- -- -- | -- -- -- | -- -- --
| [TheLab:RR](https://store.steampowered.com/app/450390) | The Lab: Robot Repair | open | read | -- -- -- | -- -- -- | -- -- --
| [TheLab:SS](https://store.steampowered.com/app/450390) | The Lab: Secret Shop | - | - | -- -- -- | -- -- -- | -- -- --
| [TheLab:TL](https://store.steampowered.com/app/450390) | The Lab: The Lab | - | - | -- -- -- | -- -- -- | -- -- --
| [HL:Alyx](https://store.steampowered.com/app/546560) | Half-Life: Alyx | open | read | -- -- -- | -- -- -- | -- -- --
