Context
===============

Family Context

---

## FamilyManager

| type          | name          | description
| ---           | ---           | ---   
| IDictionary<string, Family> | Families | The familes.
| Family        | FamilyEstate(string estateName) | Gets the specified family.


## Family

| type          | name          | description
| ---           | ---           | ---   
| string        | Id            | The estate identifier.
| string        | Name          | The estate name.
| string        | Studio        | The estate studio.
| string        | Description   | The estate description.
| Type          | PakFileType   | The type of the pak file.
| PakMultiType  | PakMulti      | The multi-pak.
| Type          | Pak2FileType  | The type of the pak file.
| PakMultiType  | Pak2Multi     | The multi-pak.
| IDictionary<string, FamilyGame> | Games | Gets the games.
| FileManager   | FileManager   | Gets the file manager.
| (string id, FamilyGame game)  | GetGame(string id) | Gets the game.
| Resource      | ParseResource(Uri uri) | Parses the resource.
| PakFile | OpenPakFile(string[] filePaths, string game) | Opens the pak file.
| PakFile | OpenPakFile(Resource resource) | Opens the pak file.
| PakFile | OpenPakFile(Uri uri) | Opens the pak file.

## struct Resource
* bool StreamPak
* Uri Host
* string[] Paths
* string Game

## class FamilyGame
* string Game
* string Name
* IList<Uri> DefaultPaks
* bool Found


## Debug
