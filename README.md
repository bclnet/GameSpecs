GameX
===

GameX is an open-source, cross-platform solution for delivering game assets as a service.

### GameX Benefits:
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


## [Applications](docs/Applications/Readme.md)
Multiple applications are included in GameX to make it easier to work with the game assets.

The following are the current applications:

| ID                                               | Name
| --                                               | --  
| [Command Line Interface](docs/Applications/Command%20Line%20Interface/Readme.md)| A CLI tool.
| [Explorer](docs/Applications/Explorer/Readme.md)                   | An application explorer.
| [Unity Plugin](docs/Applications/Unity%20Plugin/Readme.md)         | A Unity plugin.
| [Unreal Plugin](docs/Applications/Unreal%20Plugin/Readme.md)       | A Unreal plugin.

## [Context](docs/Context/Readme.md)
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
| [Arkane](docs/Families/Arkane/Readme.md)    | Arkane Studios            | Dishonored 2      | In Development
| [Bethesda](docs/Families/Bethesda/Readme.md)| The Elder Scrolls         | Skyrim            | In Development
| [Bioware](docs/Families/Bioware/Readme.md)  | Bioware                   | Neverwinter Nights| In Development
| [Black](docs/Families/Black/Readme.md)      | Black Isle Studios        | Fallout 2         | In Development
| [Blizzard](docs/Families/Blizzard/Readme.md)| Blizzard                  | StarCraft         | In Development
| [Capcom](docs/Families/Capcom/Readme.md)    | Capcom                    | Resident Evil     | In Development
| [Cig](docs/Families/Cig/Readme.md)          | Cloud Imperium Games      | Star Citizen      | In Development
| [Cryptic](docs/Families/Cryptic/Readme.md)  | Cryptic                   | Star Trek Online  | In Development
| [Crytek](docs/Families/Cry/Readme.md)       | Crytek                    | MechWarrior Online| In Development
| [Cyanide](docs/Families/Cyanide/Readme.md)  | Cyanide Formats           | The Council       | In Development
| [Epic](docs/Families/Epic/Readme.md)        | Epic                      | BioShock          | In Development
| [Frictional](docs/Families/Frictional/Readme.md)| Frictional Games      | SOMA              | In Development
| [Frontier](docs/Families/Frontier/Readme.md)| Frontier Developments     | Elite: Dangerous  | In Development
| [Id](docs/Families/Id/Readme.md)            | id Software               | Doom              | In Development
| [IW](docs/Families/IW/Readme.md)            | Infinity Ward             | Call of Duty      | In Development
| [Monolith](docs/Families/Monolith/Readme.md)| Monolith                  | F.E.A.R.          | In Development
| [Origin](docs/Families/Origin/Readme.md)    | Origin Systems            | Ultima Online     | In Development
| [Red](docs/Families/Red/Readme.md)          | REDengine                 | The Witcher 3: Wild Hunt | In Development
| [Unity](docs/Families/Unity/Readme.md)      | Unity                     | AmongUs           | In Development
| [Unknown](docs/Families/Unknown/Readme.md)  | Unknown                   | N/A               | In Development
| [Valve](docs/Families/Valve/Readme.md)      | Valve                     | Dota 2            | In Development
| [WbB](docs/Families/WbB/Readme.md)          | Asheron's Call            | Asheron's Call    | In Development

## [Platforms](docs/Platforms/Readme.md)
Platforms provide the interface to each gaming platform.

## Games
---
The following are the current games:

| ID | Name | Open | Read | Texure | Model | Level
| -- | --   | --   | --   | --     | --    | --
| **Arkane** | **Arkane Studios**
| [AF](https://www.gog.com/en/game/arx_fatalis) | Arx Fatalis | open | read | gl -- -- | -- -- -- | -- -- --
| [DOM](https://store.steampowered.com/app/2100) | Dark Messiah of Might and Magic | open | read | -- -- -- | -- -- -- | -- -- --
| [D](https://www.gog.com/en/game/dishonored_definitive_edition) | Dishonored | - | - | -- -- -- | -- -- -- | -- -- --
| [D2](https://www.gog.com/index.php/game/dishonored_2) | Dishonored 2 | open | read | -- -- -- | -- -- -- | -- -- --
| [P](https://www.gog.com/en/game/prey) | Prey | open | read | -- -- -- | -- -- -- | -- -- --
| [D:DOTO](https://www.gog.com/en/game/dishonored_death_of_the_outsider) | Dishonored: Death of the Outsider | - | - | -- -- -- | -- -- -- | -- -- --
| [W:YB](https://store.steampowered.com/app/1056960) | Wolfenstein: Youngblood | - | - | -- -- -- | -- -- -- | -- -- --
| [W:CP](https://store.steampowered.com/app/1056970) | Wolfenstein: Cyberpilot | - | - | -- -- -- | -- -- -- | -- -- --
| [DL](https://store.steampowered.com/app/1252330) | Deathloop | - | - | -- -- -- | -- -- -- | -- -- --
| [RF](https://bethesda.net/en/game/redfall) | Redfall (future) | - | - | -- -- -- | -- -- -- | -- -- --
| **Bethesda** | **Bethesda Game Studios**
| [Morrowind](https://store.steampowered.com/app/22320) | The Elder Scrolls III: Morrowind | - | - | -- -- -- | -- -- -- | -- -- --
| [Oblivion](https://store.steampowered.com/app/22330) | The Elder Scrolls IV: Oblivion | - | - | -- -- -- | -- -- -- | -- -- --
| [Fallout3](https://store.steampowered.com/app/22370) | Fallout 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [FalloutNV](https://store.steampowered.com/app/22380) | Fallout New Vegas | - | - | -- -- -- | -- -- -- | -- -- --
| [Skyrim](https://store.steampowered.com/app/72850) | The Elder Scrolls V: Skyrim | - | - | -- -- -- | -- -- -- | -- -- --
| [Fallout4](https://store.steampowered.com/app/377160) | Fallout 4 | - | - | -- -- -- | -- -- -- | -- -- --
| [SkyrimSE](https://store.steampowered.com/app/489830) | The Elder Scrolls V: Skyrim – Special Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [Fallout:S](https://store.steampowered.com/app/588430) | Fallout Shelter | - | - | -- -- -- | -- -- -- | -- -- --
| [Fallout4VR](https://store.steampowered.com/app/611660) | Fallout 4 VR | - | - | -- -- -- | -- -- -- | -- -- --
| [SkyrimVR](https://store.steampowered.com/app/611670) | The Elder Scrolls V: Skyrim VR | - | - | -- -- -- | -- -- -- | -- -- --
| [Fallout76](https://store.steampowered.com/app/1151340) | Fallout 76 | - | - | -- -- -- | -- -- -- | -- -- --
| [Starfield](https://store.steampowered.com/app/1716740) | Starfield | - | - | -- -- -- | -- -- -- | -- -- --
| [Unknown1](https://en.wikipedia.org/wiki/The_Elder_Scrolls) | The Elder Scrolls VI (future) | - | - | -- -- -- | -- -- -- | -- -- --
| [Fallout5](https://en.wikipedia.org/wiki/Fallout_(series)) | Fallout 5 (future) | - | - | -- -- -- | -- -- -- | -- -- --
| **Bioware** | **BioWare**
| [SS](https://www.gog.com/en/game/shattered_steel) | Shattered Steel | - | - | -- -- -- | -- -- -- | -- -- --
| [BG](https://www.gog.com/en/game/baldurs_gate_enhanced_edition) | Baldur's Gate | - | - | -- -- -- | -- -- -- | -- -- --
| [MDK2](https://www.gog.com/en/game/mdk_2) | MDK2 | - | - | -- -- -- | -- -- -- | -- -- --
| [BG2](https://www.gog.com/en/game/baldurs_gate_2_enhanced_edition) | Baldur's Gate II: Shadows of Amn | - | - | -- -- -- | -- -- -- | -- -- --
| [NWN](https://store.steampowered.com/app/704450) | Neverwinter Nights | - | - | -- -- -- | -- -- -- | -- -- --
| [KotOR](https://store.steampowered.com/app/32370) | Star Wars: Knights of the Old Republic | - | - | -- -- -- | -- -- -- | -- -- --
| [JE](https://www.gog.com/en/game/jade_empire_special_edition) | Jade Empire | - | - | -- -- -- | -- -- -- | -- -- --
| [ME](https://store.steampowered.com/app/17460) | Mass Effect | - | - | -- -- -- | -- -- -- | -- -- --
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
| **Black** | **Black Isle Studios**
| [Fallout](https://store.steampowered.com/app/38400) | Fallout | - | - | -- -- -- | -- -- -- | -- -- --
| [Fallout2](https://store.steampowered.com/app/38410) | Fallout 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [P:T](https://en.wikipedia.org/wiki/Planescape:_Torment) | Planescape: Torment | - | - | -- -- -- | -- -- -- | -- -- --
| [ID](https://en.wikipedia.org/wiki/Icewind_Dale) | Icewind Dale | - | - | -- -- -- | -- -- -- | -- -- --
| [ID:HoW](https://en.wikipedia.org/wiki/Icewind_Dale:_Heart_of_Winter) | Icewind Dale: Heart of Winter | - | - | -- -- -- | -- -- -- | -- -- --
| [ID2](https://en.wikipedia.org/wiki/Icewind_Dale_II) | Icewind Dale II | - | - | -- -- -- | -- -- -- | -- -- --
| [BG:DA2](https://en.wikipedia.org/wiki/Baldur%27s_Gate:_Dark_Alliance_II) | Baldur's Gate: Dark Alliance II | - | - | -- -- -- | -- -- -- | -- -- --
| **Blizzard** | **Blizzard Entertainment**
| [SC](https://us.shop.battle.net/en-us/product/starcraft) | StarCraft | - | - | -- -- -- | -- -- -- | -- -- --
| [D2R](https://us.shop.battle.net/en-us/product/diablo_ii_resurrected) | Diablo II: Resurrected | - | - | -- -- -- | -- -- -- | -- -- --
| [W3](https://us.shop.battle.net/en-us/product/warcraft-iii-reforged) | Warcraft III: Reign of Chaos | - | - | -- -- -- | -- -- -- | -- -- --
| [WOW](https://us.shop.battle.net/en-us/family/world-of-warcraft) | World of Warcraft | - | - | -- -- -- | -- -- -- | -- -- --
| [WOWC](https://us.shop.battle.net/en-us/family/world-of-warcraft-classic) | World of Warcraft: Classic | - | - | -- -- -- | -- -- -- | -- -- --
| [SC2](https://us.shop.battle.net/en-us/product/starcraft-ii) | StarCraft II: Wings of Liberty | - | - | -- -- -- | -- -- -- | -- -- --
| [D3](https://us.shop.battle.net/en-us/product/diablo-iii) | Diablo III | - | - | -- -- -- | -- -- -- | -- -- --
| [HOTS](https://us.shop.battle.net/en-us/family/heroes-of-the-storm) | Heroes of the Storm | - | - | -- -- -- | -- -- -- | -- -- --
| [HS](https://us.shop.battle.net/en-us/family/hearthstone) | Hearthstone | - | - | -- -- -- | -- -- -- | -- -- --
| [CB](https://us.shop.battle.net/en-us/family/crash-bandicoot-4) | Crash Bandicoot™ 4: It’s About Time | - | - | -- -- -- | -- -- -- | -- -- --
| [DI](https://diabloimmortal.blizzard.com/en-us/) | Diablo Immortal | - | - | -- -- -- | -- -- -- | -- -- --
| [OW2](https://us.shop.battle.net/en-us/product/overwatch) | Overwatch 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [D4](https://diablo4.blizzard.com/en-us/) | Diablo IV | - | - | -- -- -- | -- -- -- | -- -- --
| **Capcom** | **Capcom**
| [Arcade](https://store.steampowered.com/app/1515950) | Capcom Arcade Stadium | - | - | -- -- -- | -- -- -- | -- -- --
| [Fighting:C](https://store.steampowered.com/app/1685750) | Capcom Fighting Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [GNG:R](https://store.steampowered.com/app/1375400) | Ghosts 'n Goblins Resurrection | - | - | -- -- -- | -- -- -- | -- -- --
| [MM:LC](https://store.steampowered.com/app/363440) | Mega Man Legacy Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [MM:LC2](https://store.steampowered.com/app/495050) | Mega Man Legacy Collection 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [MM:XD](https://store.steampowered.com/app/1582620) | Mega Man X DiVE | - | - | -- -- -- | -- -- -- | -- -- --
| [MMZX:LC](https://store.steampowered.com/app/999020) | Mega Man Zero/ZX Legacy Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [MH:R](https://store.steampowered.com/app/1446780) | Monster Hunter Rise | - | - | -- -- -- | -- -- -- | -- -- --
| [MH:S2](https://store.steampowered.com/app/1277400) | Monster Hunter Stories 2: Wings of Ruin | - | - | -- -- -- | -- -- -- | -- -- --
| [PWAA:T](https://store.steampowered.com/app/787480) | Phoenix Wright: Ace Attorney Trilogy | - | - | -- -- -- | -- -- -- | -- -- --
| [RDR2](https://store.steampowered.com/app/1174180) | Red Dead Redemption 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [RER](https://store.steampowered.com/app/952070) | Resident Evil Resistance | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:RV](https://store.steampowered.com/app/1236300) | Resident Evil Re:Verse | - | - | -- -- -- | -- -- -- | -- -- --
| [Disney:AC](https://store.steampowered.com/app/525040) | The Disney Afternoon Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [TGAA:C](https://store.steampowered.com/app/1158850) | The Great Ace Attorney Chronicles | - | - | -- -- -- | -- -- -- | -- -- --
| [USF4](https://store.steampowered.com/app/45760) | Ultra Street Fighter IV | - | - | -- -- -- | -- -- -- | -- -- --
| [BionicCommando](https://store.steampowered.com/app/21670) | Bionic Commando (2009 video game) | - | - | -- -- -- | -- -- -- | -- -- --
| [BionicCommando:R](https://store.steampowered.com/app/21680) | Bionic Commando Rearmed | - | - | -- -- -- | -- -- -- | -- -- --
| [Arcade:S](https://store.steampowered.com/app/1755910) | Capcom Arcade 2nd Stadium | - | - | -- -- -- | -- -- -- | -- -- --
| [BEU:B](https://store.steampowered.com/app/885150) | Capcom Beat 'Em Up Bundle | - | - | -- -- -- | -- -- -- | -- -- --
| [DV](https://store.steampowered.com/app/45710) | Dark Void | - | - | -- -- -- | -- -- -- | -- -- --
| [DV:Z](https://store.steampowered.com/app/45730) | Dark Void Zero | - | - | -- -- -- | -- -- -- | -- -- --
| [DR](https://store.steampowered.com/app/427190) | Dead Rising | - | - | -- -- -- | -- -- -- | -- -- --
| [DR2](https://store.steampowered.com/app/45740) | Dead Rising 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [DR2:OtR](https://store.steampowered.com/app/45770) | Dead Rising 2: Off the Record | - | - | -- -- -- | -- -- -- | -- -- --
| [DR3](https://store.steampowered.com/app/265550) | Dead Rising 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [DR4](https://store.steampowered.com/app/543460) | Dead Rising 4 | - | - | -- -- -- | -- -- -- | -- -- --
| [DMC3:S](https://store.steampowered.com/app/6550) | Devil May Cry 3: Special Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [DMC4:S](https://store.steampowered.com/app/329050) | Devil May Cry 4: Special Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [DMC5](https://store.steampowered.com/app/601150) | Devil May Cry 5 | - | - | -- -- -- | -- -- -- | -- -- --
| [DMC:HD](https://store.steampowered.com/app/631510) | Devil May Cry: HD Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [DMC:DMC](https://store.steampowered.com/app/220440) | DmC: Devil May Cry | - | - | -- -- -- | -- -- -- | -- -- --
| [Dragon](https://store.steampowered.com/app/367500) | Dragon's Dogma | - | - | -- -- -- | -- -- -- | -- -- --
| [DT:R](https://store.steampowered.com/app/237630) | DuckTales: Remastered | - | - | -- -- -- | -- -- -- | -- -- --
| [Flock](https://store.steampowered.com/app/21640) | Flock! | - | - | -- -- -- | -- -- -- | -- -- --
| [LP:EC](https://store.steampowered.com/app/6510) | Lost Planet: Extreme Condition | - | - | -- -- -- | -- -- -- | -- -- --
| [LP3](https://store.steampowered.com/app/226720) | Lost Planet 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [MVC:I](https://store.steampowered.com/app/493840) | Marvel vs. Capcom: Infinite | - | - | -- -- -- | -- -- -- | -- -- --
| [MM11](https://store.steampowered.com/app/742300) | Mega Man 11 | - | - | -- -- -- | -- -- -- | -- -- --
| [MMX:LC](https://store.steampowered.com/app/743890) | Mega Man X Legacy Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [MMX:LC2](https://store.steampowered.com/app/743900) | Mega Man X Legacy Collection 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [MH:W](https://store.steampowered.com/app/582010) | Monster Hunter: World | - | - | -- -- -- | -- -- -- | -- -- --
| [Okami:HD](https://store.steampowered.com/app/587620) | Ōkami HD | - | - | -- -- -- | -- -- -- | -- -- --
| [O:W](https://store.steampowered.com/app/761600) | Onimusha: Warlords | - | - | -- -- -- | -- -- -- | -- -- --
| [RememberMe](https://store.steampowered.com/app/228300) | Remember Me | - | - | -- -- -- | -- -- -- | -- -- --
| [RE](https://store.steampowered.com/app/304240) | Resident Evil | - | - | -- -- -- | -- -- -- | -- -- --
| [RE2](https://store.steampowered.com/app/883710) | Resident Evil 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [RE3](https://store.steampowered.com/app/952060) | Resident Evil 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [RE4](https://store.steampowered.com/app/254700) | Resident Evil 4 | - | - | -- -- -- | -- -- -- | -- -- --
| [RE5](https://store.steampowered.com/app/21690) | Resident Evil 5 | - | - | -- -- -- | -- -- -- | -- -- --
| [RE6](https://store.steampowered.com/app/221040) | Resident Evil 6 | - | - | -- -- -- | -- -- -- | -- -- --
| [RE7](https://store.steampowered.com/app/418370) | Resident Evil 7: Biohazard | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:R](https://store.steampowered.com/app/222480) | Resident Evil: Revelations | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:R2](https://store.steampowered.com/app/287290) | Resident Evil: Revelations 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [RE:V](https://store.steampowered.com/app/1196590) | Resident Evil Village | - | - | -- -- -- | -- -- -- | -- -- --
| [REZ](https://store.steampowered.com/app/339340) | Resident Evil Zero | - | - | -- -- -- | -- -- -- | -- -- --
| [SF:30AC](https://store.steampowered.com/app/586200) | Street Fighter 30th Anniversary Collection | - | - | -- -- -- | -- -- -- | -- -- --
| [SF5](https://store.steampowered.com/app/310950) | Street Fighter V | - | - | -- -- -- | -- -- -- | -- -- --
| [Strider](https://store.steampowered.com/app/235210) | Strider (2014 video game) | - | - | -- -- -- | -- -- -- | -- -- --
| [UMVC3](https://store.steampowered.com/app/357190) | Ultimate Marvel vs. Capcom 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [UmbrellaCorps](https://store.steampowered.com/app/390340) | Umbrella Corps | - | - | -- -- -- | -- -- -- | -- -- --
| **Cig** | **Cloud Imperium Games**
| [StarCitizen](https://robertsspaceindustries.com/playstarcitizen) | Star Citizen | - | - | -- -- -- | -- -- -- | -- -- --
| **Cryptic** | **Cryptic**
| [CO](https://store.steampowered.com/app/9880) | Champions Online | open | read | -- -- -- | -- -- -- | -- -- --
| [STO](https://store.steampowered.com/app/9900) | Star Trek Online | open | read | -- -- -- | -- -- -- | -- -- --
| [NVW](https://store.steampowered.com/app/109600) | Neverwinter | open | read | -- -- -- | -- -- -- | -- -- --
| **Crytek** | **Crytek**
| [ArcheAge](https://store.steampowered.com/app/304030) | ArcheAge | - | - | -- -- -- | -- -- -- | -- -- --
| [Hunt](https://store.steampowered.com/app/594650) | Hunt: Showdown | - | - | -- -- -- | -- -- -- | -- -- --
| [MWO](https://store.steampowered.com/app/342200) | MechWarrior Online | - | - | -- -- -- | -- -- -- | -- -- --
| [Warface](https://store.steampowered.com/app/291480) | Warface | - | - | -- -- -- | -- -- -- | -- -- --
| [Wolcen](https://store.steampowered.com/app/424370) | Wolcen: Lords of Mayhem | - | - | -- -- -- | -- -- -- | -- -- --
| [Crysis](https://store.steampowered.com/app/1715130) | Crysis Remastered | - | - | -- -- -- | -- -- -- | -- -- --
| [Ryse](https://store.steampowered.com/app/302510) | Ryse: Son of Rome | - | - | -- -- -- | -- -- -- | -- -- --
| [Robinson](https://store.steampowered.com/app/579820) | Robinson: The Journey | - | - | -- -- -- | -- -- -- | -- -- --
| [Snow](https://store.steampowered.com/app/244930) | SNOW - The Ultimate Edition | - | - | -- -- -- | -- -- -- | -- -- --
| **Cyanide** | **Cyanide**
| [TC](https://store.steampowered.com/app/287630) | The Council | - | - | -- -- -- | -- -- -- | -- -- --
| [Werewolf:TA](https://store.steampowered.com/app/679110) | Werewolf: The Apocalypse - Earthblood | - | - | -- -- -- | -- -- -- | -- -- --
| **Epic** | **Epic**
| [UE1](https://oldgamesdownload.com/unreal) | Unreal | - | - | -- -- -- | -- -- -- | -- -- --
| [TWoT](https://www.gog.com/en/game/the_wheel_of_time) | The Wheel of Time | - | - | -- -- -- | -- -- -- | -- -- --
| [DeusEx](https://www.gog.com/en/game/deus_ex) | Deus Ex™ GOTY Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [DeusEx:MD](https://www.gog.com/en/game/deus_ex_mankind_divided) | Deus Ex: Mankind Divided | - | - | -- -- -- | -- -- -- | -- -- --
| [DeusEx2:IW](https://www.gog.com/en/game/deus_ex_invisible_war) | Deus Ex 2: Invisible War | - | - | -- -- -- | -- -- -- | -- -- --
| [DeusEx:HR](https://www.gog.com/en/game/deus_ex_human_revolution_directors_cut) | Deus Ex: Human Revolution - Director’s Cut | - | - | -- -- -- | -- -- -- | -- -- --
| [Rune](https://www.gog.com/en/game/rune_classic) | Rune | - | - | -- -- -- | -- -- -- | -- -- --
| [Undying](https://www.gog.com/en/game/clive_barkers_undying) | Clive Barker's Undying | - | - | -- -- -- | -- -- -- | -- -- --
| [UT2K](https://oldgamesdownload.com/unreal-tournament-2003/) | Unreal Tournament 2003 | - | - | -- -- -- | -- -- -- | -- -- --
| [UE2](https://store.steampowered.com/app/13200/Unreal_2_The_Awakening/) | Unreal II: The Awakening | - | - | -- -- -- | -- -- -- | -- -- --
| [BioShock](https://store.steampowered.com/app/7670) | BioShock | - | - | -- -- -- | -- -- -- | -- -- --
| [BioShockR](https://store.steampowered.com/app/409710) | BioShock Remastered | - | - | -- -- -- | -- -- -- | -- -- --
| [BioShock2](https://store.steampowered.com/app/8850) | BioShock 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [BioShock2R](https://store.steampowered.com/app/409720) | BioShock 2 Remastered | - | - | -- -- -- | -- -- -- | -- -- --
| [BioShock:Inf](https://store.steampowered.com/app/8870) | BioShock Infinite | - | - | -- -- -- | -- -- -- | -- -- --
| **Frictional** | **HPL Engine**
| [P:O](https://store.steampowered.com/app/22180) | Penumbra: Overture | - | - | -- -- -- | -- -- -- | -- -- --
| [P:BP](https://store.steampowered.com/app/22120) | Penumbra: Black Plague | - | - | -- -- -- | -- -- -- | -- -- --
| [P:R](https://store.steampowered.com/app/22140) | Penumbra: Requiem | - | - | -- -- -- | -- -- -- | -- -- --
| [A:TDD](https://store.steampowered.com/app/57300) | Amnesia: The Dark Descent | - | - | -- -- -- | -- -- -- | -- -- --
| [A:AMFP](https://store.steampowered.com/app/239200) | Amnesia: A Machine for Pigs | - | - | -- -- -- | -- -- -- | -- -- --
| [SOMA](https://store.steampowered.com/app/282140) | SOMA | - | - | -- -- -- | -- -- -- | -- -- --
| [A:R](https://store.steampowered.com/app/999220) | Amnesia: Rebirth | - | - | -- -- -- | -- -- -- | -- -- --
| **Frontier** | **Frontier Developments**
| [LW](https://store.steampowered.com/app/447780) | LostWinds | - | - | -- -- -- | -- -- -- | -- -- --
| [LW2](https://store.steampowered.com/app/447800) | LostWinds 2: Winter of the Melodias | - | - | -- -- -- | -- -- -- | -- -- --
| [ED](https://store.steampowered.com/app/359320) | Elite: Dangerous | - | - | -- -- -- | -- -- -- | -- -- --
| [PC](https://store.steampowered.com/app/493340) | Planet Coaster | - | - | -- -- -- | -- -- -- | -- -- --
| [JW](https://store.steampowered.com/app/648350) | Jurassic World Evolution | - | - | -- -- -- | -- -- -- | -- -- --
| [PZ](https://store.steampowered.com/app/703080) | Planet Zoo | - | - | -- -- -- | -- -- -- | -- -- --
| [JW2](https://store.steampowered.com/app/1244460) | Jurassic World Evolution 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [F1:22](https://store.steampowered.com/app/1708520) | F1 Manager 2022 | - | - | -- -- -- | -- -- -- | -- -- --
| [W4K:CG](https://store.steampowered.com/app/1611910) | Warhammer 40,000: Chaos Gate - Daemonhunters | - | - | -- -- -- | -- -- -- | -- -- --
| [F1:23](https://store.steampowered.com/app/2287220) | F1 Manager 2023 | - | - | -- -- -- | -- -- -- | -- -- --
| [W4K:AoS:RoR](https://en.wikipedia.org/wiki/Warhammer_Age_of_Sigmar) | Warhammer Age of Sigmar: Realms of Ruin | - | - | -- -- -- | -- -- -- | -- -- --
| **Id** | **Id**
| [Q](https://store.steampowered.com/app/2310) | Quake | - | - | -- -- -- | -- -- -- | -- -- --
| [Q2](https://store.steampowered.com/app/2320) | Quake II | - | - | -- -- -- | -- -- -- | -- -- --
| [Q3:A](https://store.steampowered.com/app/0) | Quake III Arena | - | - | -- -- -- | -- -- -- | -- -- --
| [D3](https://store.steampowered.com/app/9050) | Doom 3 | - | - | -- -- -- | -- -- -- | -- -- --
| [Q:L](https://store.steampowered.com/app/282440) | Quake Live | - | - | -- -- -- | -- -- -- | -- -- --
| [R](https://store.steampowered.com/app/0) | Rage | - | - | -- -- -- | -- -- -- | -- -- --
| [D](https://store.steampowered.com/app/0) | Doom (2016) | - | - | -- -- -- | -- -- -- | -- -- --
| [D:VFR](https://store.steampowered.com/app/650000) | Doom VFR | - | - | -- -- -- | -- -- -- | -- -- --
| [R2](https://store.steampowered.com/app/0) | Rage 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [D:E](https://store.steampowered.com/app/0) | Doom Eternal | - | - | -- -- -- | -- -- -- | -- -- --
| [Q:C](https://store.steampowered.com/app/611500) | Quake Champions | - | - | -- -- -- | -- -- -- | -- -- --
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
| **Monolith** | **MonolithTech**
| [FEAR](https://store.steampowered.com/app/21090) | F.E.A.R. | - | - | -- -- -- | -- -- -- | -- -- --
| [FEAR:EP](https://store.steampowered.com/app/21110) | F.E.A.R.: Extraction Point | - | - | -- -- -- | -- -- -- | -- -- --
| [FEAR:PM](https://store.steampowered.com/app/21120) | F.E.A.R.: Perseus Mandate | - | - | -- -- -- | -- -- -- | -- -- --
| [FEAR2](https://store.steampowered.com/app/16450) | F.E.A.R. 2: Project Origin | - | - | -- -- -- | -- -- -- | -- -- --
| [FEAR3](https://store.steampowered.com/app/21100) | F.E.A.R. 3 | - | - | -- -- -- | -- -- -- | -- -- --
| **Origin** | **Origin Systems**
| [UO](https://uo.com/client-download/) | Ultima Online | - | - | -- -- -- | -- -- -- | -- -- --
| [U9](https://www.gog.com/en/game/ultima_9_ascension) | Ultima IX | - | - | -- -- -- | -- -- -- | -- -- --
| **Red** | **REDengine**
| [Witcher](https://www.gog.com/en/game/the_witcher) | The Witcher Enhanced Edition | - | - | -- -- -- | -- -- -- | -- -- --
| [Witcher2](https://www.gog.com/en/game/the_witcher_2) | The Witcher 2 | - | - | -- -- -- | -- -- -- | -- -- --
| [Witcher3](https://www.gog.com/en/game/the_witcher_3_wild_hunt) | The Witcher 3: Wild Hunt | - | - | -- -- -- | -- -- -- | -- -- --
| [CP77](https://store.steampowered.com/app/1091500) | Cyberpunk 2077 | - | - | -- -- -- | -- -- -- | -- -- --
| [Witcher4](https://en.wikipedia.org/wiki/The_Witcher_(video_game_series)) | The Witcher 4 Polaris (future) | - | - | -- -- -- | -- -- -- | -- -- --
| **Ubisoft** | **Ubisoft**
| **Unity** | **Unity**
| [AmongUs](https://store.steampowered.com/app/945360) | Among Us | - | - | -- -- -- | -- -- -- | -- -- --
| [Cities](https://store.steampowered.com/app/255710) | Cities: Skylines | - | - | -- -- -- | -- -- -- | -- -- --
| [Tabletop](https://store.steampowered.com/app/286160) | Tabletop Simulator | - | - | -- -- -- | -- -- -- | -- -- --
| [UBoat](https://store.steampowered.com/app/1272010) | Destroyer: The U-Boat Hunter | - | - | -- -- -- | -- -- -- | -- -- --
| [7D2D](https://store.steampowered.com/app/251570) | 7 Days to Die | - | - | -- -- -- | -- -- -- | -- -- --
| **Unknown** | **Unknown**
| [APP]() | Application | - | - | -- -- -- | -- -- -- | -- -- --
| [CAT]() | Catalog | - | - | -- -- -- | -- -- -- | -- -- --
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
| [D2](https://store.steampowered.com/app/570) | Dota 2 | open | read | gl -- -- | -- -- -- | -- -- --
| [TheLab:RR](https://store.steampowered.com/app/450390) | The Lab: Robot Repair | open | read | gl -- -- | gl -- -- | -- -- --
| [TheLab:SS](https://store.steampowered.com/app/450390) | The Lab: Secret Shop | - | - | -- -- -- | -- -- -- | -- -- --
| [TheLab:TL](https://store.steampowered.com/app/450390) | The Lab: The Lab | - | - | -- -- -- | -- -- -- | -- -- --
| [HL:Alyx](https://store.steampowered.com/app/546560) | Half-Life: Alyx | open | read | gl -- -- | gl -- -- | -- -- --
| **WbB** | **WB Games Boston**
| [AC](https://en.wikipedia.org/wiki/Asheron%27s_Call) | Asheron's Call | open | read | gl -- -- | -- -- -- | -- -- --
| [AC2](https://en.wikipedia.org/wiki/Asheron%27s_Call_2:_Fallen_Kings) | Asheron's Call 2: Fallen Kings | - | - | -- -- -- | -- -- -- | -- -- --
| [DDO](https://en.wikipedia.org/wiki/Dungeons_%26_Dragons_Online) | Dungeons & Dragons Online | - | - | -- -- -- | -- -- -- | -- -- --
| [LotRO](https://en.wikipedia.org/wiki/The_Lord_of_the_Rings_Online) | The Lord of the Rings Online | - | - | -- -- -- | -- -- -- | -- -- --
| [IC](https://en.wikipedia.org/wiki/Infinite_Crisis_(video_game)) | Infinite Crisis | - | - | -- -- -- | -- -- -- | -- -- --
