import os, json, glob, re
from typing import Any
from urllib.parse import urlparse
from importlib import resources
from .openstk_poly import findType
from .pakfile import PakFile, ManyPakFile, MultiPakFile
from .filesys import FileSystem, HostFileSystem, createFileSystem
from .filemgr import FileManager

def _value(val: dict[str, Any], key: str, default: Any = None) -> Any:
    return val[key] if key in val else default

def _list(val: dict[str, Any], key: str, default: Any = None) -> Any:
    return (val[key] if isinstance(val[key], list) else [val[key]]) if key in val else default

def _method(method: Any, val: dict[str, Any], key: str, default: Any = None) -> Any:
    return method(val[key]) if key in val else default

class FamilyEngine: pass
class FamilyGame: pass
class Resource: pass
class Family: pass

# parse key
@staticmethod
def parseKey(key: str) -> Any:
    if not key: return None
    elif key.startswith('hex:'): return bytes.fromhex(key[4:].replace('/x', ''))
    elif key.startswith('txt:'): return key[4:].encode('ascii')
    else: raise Exception(f'Unknown key: {key}')

# create Family
@staticmethod
def createFamily(path: str, loader: Any) -> Family:
    val = loader(path)
    familyType = _value(val, 'familyType')
    family = findType(familyType)(val) if familyType else \
        Family(val)
    if family.specs:
        for spec in family.specs:
            family.merge(createFamily(spec, loader))
    return family

# create FamilyEngine
@staticmethod
def createFamilyEngine(family: Family, id: str, val: dict[str, Any]) -> FamilyEngine:
    engineType = _value(val, 'engineType')
    engine = findType(engineType)(family, id, val) if engineType else \
        FamilyEngine(family, id, val)
    return engine

# create FamilyGame
@staticmethod
def createFamilyGame(family: Family, id: str, val: dict[str, Any], dgame: FamilyGame, paths: dict[str, Any]) -> FamilyGame:
    gameType = _value(val, 'gameType', dgame.gameType)
    game = findType(gameType)(family, id, val, dgame) if gameType else \
        FamilyGame(family, id, val, dgame)
    game.gameType = gameType
    game.found = id in paths if paths else False
    return game

# create FileManager
@staticmethod
def createFileManager(val: dict[str, Any]) -> FileManager:
    return FileManager(val)

class Family:
    def __init__(self, val: dict[str, Any]):
        self.id = val['id']
        self.name = _value(val, 'name')
        self.studio = _value(val, 'studio')
        self.description = _value(val, 'description')
        self.urls = _list(val, 'url')
        self.specs = _list(val, 'specs')
        # file manager
        self.fileManager = _method(createFileManager, val, 'fileManager')
        paths = self.fileManager.paths if self.fileManager else None
        # engines
        self.engines = engines = {}
        if 'engines' in val:
            for (id, val) in val['engines'].items():
                engines[id] = createFamilyEngine(self, id, val)
        # games
        self.games = games = {}
        dgame = FamilyGame(self, None, None, None)
        if 'games' in val:
            for (id, val) in val['games'].items():
                game = createFamilyGame(self, id, val, dgame, paths)
                if id.startswith('*'): dgame = game
                else: games[id] = game
        
    def __repr__(self): return f'''
{self.id}: {self.name}
engines: {[x for x in self.engines.values()] if self.engines else None}
games: {[x for x in self.games.values()] if self.games else None}
fileManager: {self.fileManager if self.fileManager else None}'''

    # merge
    def merge(self, source) -> None:
        if not source: return
        self.engines.update(source.engines)
        self.games.update(source.games)
        if self.fileManager: self.fileManager.merge(source.fileManager)
        else: self.fileManager = source.fileManager

    # get Game
    def getGame(self, id: str, throwOnError: bool = True) -> FamilyGame:
        game = self.games[id] if id in self.games else None
        if not game and throwOnError: raise Exception(f'Unknown game: {id}')
        return game

    # parse Resource
    def parseResource(self, uri: str, throwOnError: bool = True) -> Resource:
        if uri is None or not (uri := urlparse(uri)).fragment:
            return Resource(Game = FamilyGame(self, None, None, None))
        game = self.getGame(uri.fragment)
        searchPattern = '' if uri.scheme == 'file' else uri.path[1:]
        paths = self.fileManager.paths
        fileSystemType = game.fileSystemType
        fileSystem = \
            (createFileSystem(fileSystemType, paths[game.id][0]) if game.id in paths and paths[game.id] else None) if uri.scheme == 'game' else \
            (createFileSystem(fileSystemType, uri.path) if uri.path else None) if uri.scheme == 'file' else \
            (createFileSystem(fileSystemType, None, uri) if uri.netloc else None) if uri.scheme.startswith('http') else None
        if not fileSystem:
            if throwOnError: raise Exception(f'Not located: {game.id}')
            else: return None
        return Resource(fileSystem, game, searchPattern)

    # open PakFile
    def openPakFile(self, res: Resource | str, throwOnError: bool = True) -> PakFile:
        resource = res if isinstance(res, Resource) else \
            self.parseResource(res) if isinstance(res, str) else \
            None
        if not resource:
            if throwOnError: raise Exception(f'Unknown res: {res}')
            else: return None
        if not resource.game: raise Exception(f'Undefined Game')
        return (pak := resource.game.createPakFile(resource.fileSystem, resource.searchPattern, throwOnError)) and pak.open()

class FamilyEngine:
    def __init__(self, family: Family, id: str, val: dict[str, Any]):
        self.family = family
        self.id = id
        self.name = _value(val, 'name')
    def __repr__(self): return f'\n  {self.id}: {self.name}'

class FamilyGame:
    class Edition:
        def __init__(self, id: str, val: dict[str, Any]):
            self.id = id
            self.name = _value(val, 'name')
            self.key = _method(parseKey, val, 'key')
        def __repr__(self): return f'{self.id}: {self.name}'
    class DownloadableContent:
        def __init__(self, id: str, val: dict[str, Any]):
            self.id = id
            self.name = _value(val, 'name')
            self.path = _value(val, 'path')
        def __repr__(self): return f'{self.id}: {self.name}'
    class Locale:
        def __init__(self, id: str, val: dict[str, Any]):
            self.id = id
            self.name = _value(val, 'name')
        def __repr__(self): return f'{self.id}: {self.name}'
    def __init__(self, family: Family, id: str, val: dict[str, Any], dgame: FamilyGame):
        self.family = family
        self.id = id
        if not dgame: self.ignore = False; self.gameType = self.engine = \
            self.paths = self.key = self.fileSystemType = \
            self.searchBy = self.pakFileType = self.pakExts = None; return
        self.ignore = _value(val, 'n/a', dgame.ignore)
        self.name = _value(val, 'name')
        self.engine = _value(val, 'engine', dgame.engine)
        self.urls = _list(val, 'url')
        self.date = _value(val, 'date')
        #self.option = _list(val, 'option', dgame.option)
        #self.paks = _list(val, 'paks', dgame.paks)
        #self.dats = _list(val, 'dats', dgame.dats)
        self.paths = _list(val, 'path', dgame.paths)
        self.key = _method(parseKey, val, 'key', dgame.key)
        self.status = _value(val, 'status')
        self.tags = _value(val, 'tags')
        # interface
        self.fileSystemType = _value(val, 'fileSystemType', dgame.fileSystemType)
        self.searchBy = _value(val, 'searchBy', dgame.searchBy)
        self.pakFileType = _value(val, 'pakFileType', dgame.pakFileType)
        self.pakExts = _list(val, 'pakExt', dgame.pakExts) 
        # related
        self.editions = editions = {}
        if 'editions' in val:
            for (id, val) in val['editions'].items():
                editions[id] = FamilyGame.Edition(id, val)
        self.dlcs = dlcs = {}
        if 'dlcs' in val:
            for (id, val) in val['dlcs'].items():
                dlcs[id] = FamilyGame.DownloadableContent(id, val)
        self.locales = locales = {}
        if 'locales' in val:
            for (id, val) in val['locales'].items():
                locales[id] = FamilyGame.Locale(id, val)
    def __repr__(self): return f'''
   {self.id}: {self.name} - {self.found}'''
#     def __repr__(self): return f'''
#   {self.id}: {self.name} - {self.status}
#   - editions: {self.editions if self.editions else None}
#   - dlcs: {self.dlcs if self.dlcs else None}
#   - locales: {self.locales if self.locales else None}'''

    # create SearchPatterns
    def createSearchPatterns(self, searchPattern: str) -> str:
        if searchPattern: return searchPattern
        elif not self.searchBy: return '*'
        elif self.searchBy == 'Pak': return '' if not self.pakExts else \
            f'*{self.pakExts[0]}' if len(self.pakExts) == 1 else f'({"*:".join(self.pakExts)})'
        elif self.searchBy == 'TopDir': return '*'
        elif self.searchBy == 'TwoDir': return '*/*'
        elif self.searchBy == 'AllDir': return '**/*'
        else: raise Exception(f'Unknown searchBy: {self.searchBy}')

    # create PakFile
    def createPakFile(self, fileSystem: FileSystem, searchPattern: str, throwOnError: bool) -> PakFile:
        if isinstance(fileSystem, HostFileSystem): raise Exception('HostFileSystem not supported')
        searchPattern = self.createSearchPatterns(searchPattern)
        pakFiles = []
        for p in self.findPaths(fileSystem, searchPattern):
            if self.searchBy == 'Pak':
                for path in p[1]:
                    if self.isPakFile(path): pakFiles.append(self.createPakFileObj(fileSystem, path))
            else: pakFiles.append(self.createPakFileObj(fileSystem, p))
        return self.createPakFileObj(fileSystem, pakFiles)

    # create createPakFileObj
    def createPakFileObj(self, fileSystem: FileSystem, value: Any, tag: Any = None) -> PakFile:
        if isinstance(value, str):
            if self.isPakFile(value): return self.createPakFileType(fileSystem, value, tag)
            else: raise Exception(f'{self.id} missing {value}')
        elif isinstance(value, tuple):
            p, l = value
            return self.createPakFileObj(fileSystem, l[0], tag) if len(l) == 1 and self.isPakFile(l[0]) \
                else ManyPakFile(
                    self.createPakFileType(fileSystem, 'Base', tag), self, \
                        p if len(p) > 0 else 'Many', fileSystem, l, visualPathSkip = len(p) + 1 if len(p) > 0 else 0
                    )
        elif isinstance(value, list):
            return value[0] if len(value) == 1 else \
                MultiPakFile(self, 'Multi', fileSystem, value, tag)
        elif value is None: return None
        else: raise Exception(f'Unknown: {value}')

    # create PakFileType
    def createPakFileType(self, fileSystem: FileSystem, path: str, tag: Any = None) -> PakFile:
        if not self.pakFileType: raise Exception(f'{self.id} missing PakFileType')
        return findType(self.pakFileType)(self, fileSystem, path, tag)

    # find Paths
    def findPaths(self, fileSystem: FileSystem, searchPattern: str):
        ignores = self.family.fileManager.ignores
        gameIgnores = ignores[self.id] if self.id in ignores else None
        for path in self.paths or ['']:
            fileSearch = fileSystem.findPaths(path, searchPattern)
            if gameIgnores: fileSearch = [x for x in fileSearch if not os.path.basename(x) in gameIgnores]
            yield (path, list(fileSearch))

    # is a PakFile
    def isPakFile(self, path: str) -> bool:
        return any([x for x in self.pakExts if path.endswith(x)])

class Resource:
    def __init__(self, fileSystem: FileSystem, game: FamilyGame, searchPattern: str):
        self.fileSystem = fileSystem
        self.game = game
        self.searchPattern = searchPattern
    def __repr__(self): return f'res:/{self.searchPattern}#{self.game}'

familyKeys = ["Arkane", "Bethesda", "Bioware", "Black", "Blizzard", "Capcom", "Cig", "Cryptic", "Crytek", "Cyanide", "Epic", "Frictional", "Frontier", "Id", "IW", "Monolith", "Origin", "Red", "Unity", "Unknown", "Valve", "WbB"]

@staticmethod
def init():
    def commentRemover(text: str) -> str:
        def replacer(match): s = match.group(0); return ' ' if s.startswith('/') else s
        pattern = re.compile(r'//.*?$|/\*.*?\*/|\'(?:\\.|[^\\\'])*\'|"(?:\\.|[^\\"])*"', re.DOTALL | re.MULTILINE)
        return re.sub(pattern, replacer, text)
    def loadJson(path: str) -> dict[str, Any]:
        body = resources.files('Specs').joinpath(path).read_text(encoding='utf-8')
        return json.loads(commentRemover(body).encode().decode('utf-8-sig'))
    families = {}
    for path in [f'{x}Family.json' for x in familyKeys]:
        family = createFamily(path, loadJson)
        families[family.id] = family
    return families

@staticmethod
def getFamily(id: str, throwOnError: bool = True) -> Family:
    family = families[id] if id in families else None
    if not family and throwOnError: raise Exception(f'Unknown family: {id}')
    return family

families = init()
unknown = getFamily('Unknown')
unknownPakFile = unknown.openPakFile('game:/#APP', throwOnError=False)
# print(families)
