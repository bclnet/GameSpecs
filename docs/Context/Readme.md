Context
===

Context provides the interface for interacting with this service

* Resource - a uri formated resource with a path and game component
* Family - represents a family of games by a shared aspect
* FamilyGame - represents a single game
* FamilyManager - a static interface for the service
* FamilyPlatform - represents the current platform
* PakFile - represents a games collection of assets

### Loading an asset:
**service locates all installed games**
* windows searches registry for installed games
* linux searches installed clients for installed games: Steam, GOG, BattleNet
* a fixed location of `/Games` folder is searched for games

**(*optional*) initiate a game platform: `UnityPlatform.Startup()`**
* initiates the game platform, uses a default platform if none specified

**get a family reference: `var family = FamilyManager.GetFamily("ID")`**
* returns the specified family

**open a game specific archive file: `var pakFile = family.OpenPakFile("game:/Archive#ID")`**
* opens the specified pakfile
* urls have the following parts

#### URL


**load a game specific asset: `var obj = await pakFile.LoadFileObjectAsync<object>("Path")`**
* a
* b
* c

**service parses game objects for the specifed resource: textures, models, levels, etc**
* a
* b
* c

**service adapts the game objects to the current platform: unity, unreal, etc**
* a
* b
* c

**platform now contains the specified game asset**
* a
* b
* c

**additionally the service provides a collection of applications**
* a
* b
* c

---

### Family

| type          | name          | description
| ---           | ---           | ---   
| **class**     | **Empty**     | An empty family.
| void          | Bootstrap     | Touches this instance.
| void          | FileFilters   | Gets the file filters.
| string        | Id            | Gets or sets the family identifier.
| string        | Name          | Gets or sets the family name.
| string        | Engine        | Gets or sets the family engine.
| string        | Studio        | Gets or sets the family studio.
| string        | Description   | Gets or sets the family description.
| Type          | PakFileType   | Gets or sets the pakFile type.
| PakOption     | PakOptions    | Gets or sets the pak options.
| Type          | Pak2FileType  | Gets or sets the pakFile2 type.
| PakOption     | Pak2Options   | Gets or sets the pak2 options.
| IDictionary<string, FamilyGame>| Games | Gets the family games.
| IDictionary<string, FamilyGame>| OtherGames | Gets the family other games.
| FileManager   | FileManager   | Gets or sets the family file manager.
| Type          | FileSystemType| Gets or sets the file system type.
| FamilyGame    | GetGame(string id) | Gets the specified family game.
| Resource      | ParseResource(Uri uri, bool throwOnError = true) | Parses the family resource uri.
| PakFile       | OpenPakFile(FamilyGame game, string[] paths, int index = 0, bool throwOnError = true) | Opens the family pak file.
| PakFile       | OpenPakFile(Resource resource, int index = 0, bool throwOnError = true) | Opens the family pak file.
| PakFile       | OpenPakFile(Uri uri, int index = 0, bool throwOnError = true) | Opens the family pak file.

### FamilyGame

| type          | name          | description
| ---           | ---           | ---   
| **class**     | **Edition**   | The game edition.
| **class**     | **DownloadableContent** | The game DLC.
| **class**     | **Locale**    | The game locale.
| Family        | Family        | Gets or sets the family.
| string        | Id            | Gets or sets the game identifier.
| string        | Name          | Gets or sets the game name.
| string        | Engine        | Gets or sets the game engine.
| PakOption     | PakOptions    | Gets or sets the pak options.
| PakOption     | Pak2Options   | Gets or sets the pak2 options.
| IList<Uri>    | Paks          | Gets or sets the paks.
| IList<Uri>    | Dats          | Gets or sets the dats.
| IList<string> | Paths         | Gets or sets the Paths.
| object        | Key           | Gets or sets the key.
| bool          | Found         | Determines if the game has been found.
| Type          | FileSystemType| Gets or sets the file system type.
| IDictionary<string, Edition>| Editions | Gets the game editions.
| IDictionary<string, DownloadableContent>| Dlc | Gets the game dlcs.
| IDictionary<string, Locale>| Locales | Gets the game locales.
| string        | DisplayedName | Gets the displayed game name.
| FamilyGame    | Ensure()      | Ensures this instance.
| FileManager.IFileSystem | CreateFileSystem() | Creates the game file system.

### FamilyPlatform

| type          | name          | description
| ---           | ---           | ---   
| **class**     | **Stats**     | The platform stats.
| **enum**      | **Type**      | The platform types.
| **enum**      | **OS**        | The platform OS.
| Type          | Platform      | Gets or sets the platform.
| string        | PlatformTag   | Gets or sets the platform tag.
| OS            | PlatformOS    | Gets the platform os.
| Func<PakFile, IOpenGraphic>| GraphicFactory | Gets or sets the platform graphics factory.
| List<Func<bool>>| Startups    | Gets the platform startups.
| bool          | InTestHost    | Determines if in a test host.

### FamilyManager

| type          | name          | description
| ---           | ---           | ---   
| **class**     | **DefaultOptions** | Default Options for Applications.
| IDictionary<string, Family>| Families | Gets the families.
| Family        | GetFamily(string familyName, bool throwOnError = true) | Gets the specified family.
| Family        | ParseFamily(string json) | Parses the family.

### PakFile

| type          | name          | description
| ---           | ---           | ---   
| Family        | Family        | Gets the pak family.
| FamilyGame    | Game          | Gets the pak family game.
| string        | Name          | Gets the pak name.
| IDictionary<Type, Func<string, string>>| PathFinders | Gets the pak path finders.
| bool          | Valid         | Determines whether this instance is valid.
| void          | Dispose       | Closes this instance.
| void          | Close         | Closes this instance.
| bool          | Contains(string path) | Determines whether this instance contains the item.
| bool          | Contains(int fileId) | Determines whether this instance contains the item.
| int           | Count         | Gets the pak item count.
| string        | FindPath<T>(string path) | Finds the path.
| Task<Stream>  | LoadFileDataAsync(string path, DataOption option = 0, Action<FileMetadata, string> exception = null) | Loads the file data asynchronous.
| Task<Stream>  | LoadFileDataAsync(int fileId, DataOption option = 0, Action<FileMetadata, string> exception = null) | Loads the file data asynchronous.
| Task<Stream>  | LoadFileDataAsync(FileMetadata file, DataOption option = 0, Action<FileMetadata, string> exception = null) | Loads the file data asynchronous.
| Task<T>       | LoadFileObjectAsync<T>(string path, Action<FileMetadata, string> exception = null) | Loads the object asynchronous.
| Task<T>       | LoadFileObjectAsync<T>(int fileId, Action<FileMetadata, string> exception = null) | Loads the object asynchronous.
| Task<T>       | LoadFileObjectAsync<T>(FileMetadata file, Action<FileMetadata, string> exception = null) | Loads the object asynchronous.
| Task<T>       | LoadFileObjectAsync<T>(string path, PakFile transformTo, Action<FileMetadata, string> exception = null) | Loads the object transformed asynchronous.
| Task<T>       | LoadFileObjectAsync<T>(int fileId, PakFile transformTo, Action<FileMetadata, string> exception = null) | Loads the object transformed asynchronous.
| Task<T>       | LoadFileObjectAsync<T>(FileMetadata fileId, PakFile transformTo, Action<FileMetadata, string> exception = null) | Loads the object transformed asynchronous.
| IOpenGraphic  | Graphic       | Gets the graphic.
| Task<List<MetaItem>>| GetMetaItemsAsync(MetaManager manager) | Gets the metadata items.
| Task<List<MetaItem.Filter>>| GetMetaItemFiltersAsync(MetaManager manager) | Gets the metadata item filters.
| Task<List<MetaInfo>>| GetMetaInfosAsync(MetaManager manager, MetaItem item) | Gets the metadata infos.

### Resource

| type          | name          | description
| ---           | ---           | ---   
| **class**     | **PakOption** | Pak options.
| PakOption     | Options       | The options.
| Uri           | Host          | The host.
| string[]      | Paths         | The paths.
| FamilyGame    | Game          | The game.
