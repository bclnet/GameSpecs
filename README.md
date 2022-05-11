Game Specs
===============

Game Specs is an open-source, cross-platform solution for delivering game assets as a service.


# Families
Families are the primary grouping mechanism for interacting with the asset services.

Usually file formats center around the game developer or game engine being used, and are modified, instead of replaced, as the studio releases new versions.

---

The following are the current familes:

| ID                                               | Name                      | Sample Game       | Status
| ---                                              | ---                       | ---               | ---
| [AC](Documents/Families/AC/Readme.md)            | Asheron's Call            | Asheron's Call    | In Development
| [Arkane](Documents/Families/Arkane/Readme.md)    | Arkane Studios            | Dishonored 2      | In Development
| [Aurora](Documents/Families/Aurora/Readme.md)    | BioWare Aurora            | Neverwinter Nights| In Development
| [Cry](Documents/Families/Cry/Readme.md)          | Crytek                    | MechWarrior Online| In Development
| [Cyanide](Documents/Families/Cyanide/Readme.md)  | Cyanide Formats           | The Council       | In Development
| [Origin](Documents/Families/Origin/Readme.md)    | Origin Systems            | Ultima Online     | In Development
| [Red](Documents/Families/Red/Readme.md)          | REDengine                 | The Witcher 3: Wild Hunt | In Development
| [Rsi](Documents/Families/Rsi/Readme.md)          | Roberts Space Industries  | Star Citizen      | In Development
| [Tes](Documents/Families/Tes/Readme.md)          | The Elder Scrolls         | Skyrim            | In Development
| [Unity](Documents/Unity/Readme.md)               | TBD                       | TBD               | In Development
| [Unreal](Documents/Unreal/Readme.md)             | TBD                       | TBD               | In Development
| [Valve](Documents/Families/Valve/Readme.md)      | Valve                     | Dota 2            | In Development


### Game Specs Benefits
* Portable (windows, apple, linux, mobile, intel, arm)
* Loads textures, models, animations, sounds, and levels
* Avaliable with streaming assets (cached)
* References assets with a uniform resource location (url)
* Loaders for Unreal and Unity
* Locates installed games
* Family centric context
* Includes a desktop app to explore assets
* Includes a command line interface to export assets (list, unpack, shred)
* *future:* Usage tracking (think Spotify)
* *future:* Entitlement (think drm)

### Context
    Context

### Location (find installed games)
    First step is locating installed games
    Location definition by platform. For instance windows usually uses registration entries.

### Runtime (c++ vs .net)
    dotnet runtime
    Hosted manage for unreal or native

### Uniform Resource Location (url)
    TBD

## [Applications](Documents/Applications/Readme.md)
Multiple applications are included in GameSpecs to make it easier to work with the game assets.

## [Context](Documents/Context/Readme.md)
Family Context

## [Families](docs/Families/Readme.md)
Families are the primary grouping mechanism for interacting with the asset services.

## [Platforms](Documents/Platforms/Readme.md)
Platforms provide the interface to each platform.
