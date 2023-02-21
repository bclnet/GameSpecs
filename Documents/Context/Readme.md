Context
===

Family Context

---


### Location (find installed games)
    First step is locating installed games
    Location definition by platform. For instance windows usually uses registration entries.

### Uniform Resource Location (url)
    TBD

### Runtime (c++ vs .net)
    dotnet runtime
    Hosted manage for unreal or native

### Family

| type          | name          | description
| ---           | ---           | ---   
| **class**     | **Empty**     | An empty family.
| **class**     | **ByteKey**   | A ByteKey Container.
| void          | Bootstrap     | Touches this instance.
| void          | Ensure        | Ensures this instance.
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
| IList<Uri>    | Paks          | Gets or sets the paks.
| IList<Uri>    | Dats          | Gets or sets the dats.
| IList<string> | Paths         | Gets or sets the Paths.
| object        | Key           | Gets or sets the key.
| bool          | Found         | Determines if the game has been found.
| PakOption     | Pak2Options   | Gets or sets the file system type.
| IDictionary<string, Edition>| Editions | Gets the game editions.
| IDictionary<string, DownloadableContent>| Dlc | Gets the game dlcs.
| IDictionary<string, Locale>| Locales | Gets the game locales.
| string        | DisplayedName | Gets the displayed game name.
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
| Task<List<MetadataItem>>| GetMetadataItemsAsync(MetadataManager manager) | Gets the metadata items.
| Task<List<MetadataItem.Filter>>| GetMetadataItemFiltersAsync(MetadataManager manager) | Gets the metadata item filters.
| Task<List<MetadataInfo>>| GetMetadataInfosAsync(MetadataManager manager, MetadataItem item) | Gets the metadata infos.

### Resource

| type          | name          | description
| ---           | ---           | ---   
| **class**     | **PakOption** | Pak options.
| PakOption     | Options       | The options.
| Uri           | Host          | The host.
| string[]      | Paths         | The paths.
| FamilyGame    | Game          | The game.
