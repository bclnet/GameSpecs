import os, json, glob, re
from ._config import familyKeys
from typing import Any
from urllib.parse import urlparse
from importlib import resources
from openstk.poly import findType
from .pakfile import PakFile, ManyPakFile, MultiPakFile
from .filesys import FileSystem, HostFileSystem, createFileSystem
from .filemgr import FileManager
from .util import _throw, _value, _list, _method, _related, _relatedTrim
from .platform import Startups, Type, InTestHost, TestPlatform

class FamilyApp: pass
class FamilyEngine: pass
class FamilyGame: pass
class Resource: pass
class Family: pass

# parse key
@staticmethod
def parseKey(key: str) -> Any:
    if not key: return None
    elif key.startswith('b64:'): return base64.b64decode(key[4:].encode('ascii')) 
    elif key.startswith('hex:'): return bytes.fromhex(key[4:].replace('/x', ''))
    elif key.startswith('txt:'): return key[4:]
    else: raise Exception(f'Unknown key: {key}')

# create Family
@staticmethod
def createFamily(path: str, loader: Any) -> Family:
    elem = loader(path)
    familyType = _value(elem, 'familyType')
    family = findType(familyType)(elem) if familyType else \
        Family(elem)
    if family.specs:
        for spec in family.specs:
            family.merge(createFamily(spec, loader))
    return family

# create FamilyEngine
@staticmethod
def createFamilyEngine(family: Family, id: str, elem: dict[str, Any]) -> FamilyEngine:
    engineType = _value(elem, 'engineType')
    engine = findType(engineType)(family, id, elem) if engineType else \
        FamilyEngine(family, id, elem)
    return engine

# create FamilyGame
@staticmethod
def createFamilyGame(family: Family, id: str, elem: dict[str, Any], dgame: FamilyGame, paths: dict[str, Any]) -> FamilyGame:
    gameType = _value(elem, 'gameType', dgame.gameType)
    game = findType(gameType)(family, id, elem, dgame) if gameType else \
        FamilyGame(family, id, elem, dgame)
    game.gameType = gameType
    game.found = id in paths if paths else False
    return game

# create FamilyApp
@staticmethod
def createFamilyApp(family: Family, id: str, elem: dict[str, Any]) -> FamilyApp:
    appType = _value(elem, 'appType')
    app = findType(appType)(family, id, elem) if appType else \
        FamilyApp(family, id, elem)
    return app

# create FileManager
@staticmethod
def createFileManager(elem: dict[str, Any]) -> FileManager:
    return FileManager(elem)

class Family:
    def __init__(self, elem: dict[str, Any]):
        self.id = _value(elem, 'id')
        self.name = _value(elem, 'name')
        self.studio = _value(elem, 'studio')
        self.description = _value(elem, 'description')
        self.urls = _list(elem, 'url')
        self.specs = _list(elem, 'specs')
        # file manager
        self.fileManager = _method(elem, 'fileManager', createFileManager)
        paths = self.fileManager.paths if self.fileManager else None
        # related
        dgame = FamilyGame(self, None, None, None)
        def gameMethod(k, v):
            nonlocal dgame
            game = createFamilyGame(self, k, v, dgame, paths)
            if k.startswith('*'): dgame = game; return None
            return game
        self.engines = _related(elem, 'engines', lambda k,v:createFamilyEngine(self, k, v))
        self.games = _relatedTrim(elem, 'games', gameMethod)
        self.apps = _related(elem, 'apps', lambda k,v:createFamilyApp(self, k, v))
    def __repr__(self): return f'''
{self.id}: {self.name}
engines: {[x for x in self.engines.values()]}
games: {[x for x in self.games.values()]}
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
        ids = id.rsplit('.', 1)
        game = _value(self.games, ids[0]) or (throwOnError and _throw(f'Unknown game: {id}'))
        # if not game and throwOnError: raise Exception(f'Unknown game: {id}')
        edition = _value(self.games.editions, ids[1]) if len(ids) > 1 else None
        return (game, edition)

    # parse Resource
    def parseResource(self, uri: str, throwOnError: bool = True) -> Resource:
        if uri is None or not (uri := urlparse(uri)).fragment:
            return Resource(Game = FamilyGame(self, None, None, None))
        game, edition = self.getGame(uri.fragment)
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
        return Resource(
            fileSystem = fileSystem,
            game = game,
            edition = edition,
            searchPattern = searchPattern
            )

    # open PakFile
    def openPakFile(self, res: Resource | str, throwOnError: bool = True) -> PakFile:
        resource = res if isinstance(res, Resource) else \
            self.parseResource(res) if isinstance(res, str) else \
            None
        if not resource:
            if throwOnError: raise Exception(f'Unknown res: {res}')
            else: return None
        if not resource.game: raise Exception(f'Undefined Game')
        return (pak := resource.game.createPakFile(resource.fileSystem, resource.edition, resource.searchPattern, throwOnError)) and pak.open()

class FamilyApp:
    def __init__(self, family: Family, id: str, elem: dict[str, Any]):
        self.family = family
        self.id = id
        self.name = _value(elem, 'name') or id
    def __repr__(self): return f'\n  {self.id}: {self.name}'

class FamilyEngine:
    def __init__(self, family: Family, id: str, elem: dict[str, Any]):
        self.family = family
        self.id = id
        self.name = _value(elem, 'name') or id
    def __repr__(self): return f'\n  {self.id}: {self.name}'

class FamilyGame:
    class Edition:
        def __init__(self, id: str, elem: dict[str, Any]):
            self.id = id
            self.name = _value(elem, 'name') or id
            self.key = _method(elem, 'key', parseKey)
        def __repr__(self): return f'{self.id}: {self.name}'
    class DownloadableContent:
        def __init__(self, id: str, elem: dict[str, Any]):
            self.id = id
            self.name = _value(elem, 'name') or id
            self.path = _value(elem, 'path')
        def __repr__(self): return f'{self.id}: {self.name}'
    class Locale:
        def __init__(self, id: str, elem: dict[str, Any]):
            self.id = id
            self.name = _value(elem, 'name') or id
        def __repr__(self): return f'{self.id}: {self.name}'
    def __init__(self, family: Family, id: str, elem: dict[str, Any], dgame: FamilyGame):
        self.family = family
        self.id = id
        if not dgame:
            self.ignore = False; self.searchBy = 'Pak'; self.paks = ['game:/']
            self.gameType = self.engine = self.resource = \
            self.paths = self.key = self.fileSystemType = \
            self.pakFileType = self.pakExts = None
            return
        self.ignore = _value(elem, 'n/a', dgame.ignore)
        self.name = _value(elem, 'name')
        self.engine = _value(elem, 'engine', dgame.engine)
        self.resource = _value(elem, 'resource', dgame.resource)
        self.urls = _list(elem, 'url')
        self.date = _value(elem, 'date')
        #self.option = _list(elem, 'option', dgame.option)
        self.paks = _list(elem, 'pak', dgame.paks)
        #self.dats = _list(elem, 'dats', dgame.dats)
        self.paths = _list(elem, 'path', dgame.paths)
        self.key = _method(elem, 'key', parseKey, dgame.key)
        # self.status = _value(elem, 'status')
        self.tags = _value(elem, 'tags', '').split(' ')
        # interface
        self.fileSystemType = _value(elem, 'fileSystemType', dgame.fileSystemType)
        self.searchBy = _value(elem, 'searchBy', dgame.searchBy)
        self.pakFileType = _value(elem, 'pakFileType', dgame.pakFileType)
        self.pakExts = _list(elem, 'pakExt', dgame.pakExts) 
        # related
        self.editions = _related(elem, 'editions', lambda k,v:FamilyGame.Edition(k, v))
        self.dlcs = _related(elem, 'dlcs', lambda k,v:FamilyGame.DownloadableContent(k, v))
        self.locales = _related(elem, 'locales', lambda k,v:FamilyGame.Locale(k, v))
    def __repr__(self): return f'''
   {self.id}: {self.name} - {self.found}'''
#     def __repr__(self): return f'''
#   {self.id}: {self.name} - {self.status}
#   - editions: {self.editions if self.editions else None}
#   - dlcs: {self.dlcs if self.dlcs else None}
#   - locales: {self.locales if self.locales else None}'''

    # create Pak
    def toPaks(self) -> list[str]:
        return [f'{x}#{self.id}' for x in self.paks] if self.paks else []

    # create SearchPatterns
    def createSearchPatterns(self, searchPattern: str) -> str:
        if searchPattern: return searchPattern
        elif not self.searchBy: return '*'
        elif self.searchBy == 'None': return None
        elif self.searchBy == 'Pak': return '' if not self.pakExts else \
            f'*{self.pakExts[0]}' if len(self.pakExts) == 1 else f'(*{":*".join(self.pakExts)})'
        elif self.searchBy == 'TopDir': return '*'
        elif self.searchBy == 'TwoDir': return '*/*'
        elif self.searchBy == 'AllDir': return '**/*'
        else: raise Exception(f'Unknown searchBy: {self.searchBy}')

    # create PakFile
    def createPakFile(self, fileSystem: FileSystem, edition: Edition, searchPattern: str, throwOnError: bool) -> PakFile:
        if isinstance(fileSystem, HostFileSystem): raise Exception('HostFileSystem not supported')
        searchPattern = self.createSearchPatterns(searchPattern)
        if not searchPattern: return None
        pakFiles = []
        for key in [None]+list(self.dlcs.keys()):
            for p in self.findPaths(fileSystem, edition, self.dlcs[key] if key else None, searchPattern):
                if self.searchBy == 'Pak':
                    for path in p[1]:
                        if self.isPakFile(path): pakFiles.append(self.createPakFileObj(fileSystem, path))
                else: pakFiles.append(self.createPakFileObj(fileSystem, p))
        return self.createPakFileObj(fileSystem, pakFiles)

    # create createPakFileObj
    def createPakFileObj(self, fileSystem: FileSystem, value: Any, tag: Any = None) -> PakFile:
        match value:
            case s if isinstance(value, str):
                if self.isPakFile(s): return self.createPakFileType(fileSystem, s, tag)
                else: raise Exception(f'{self.id} missing {s}')
            case p, l if isinstance(value, tuple):
                return self.createPakFileObj(fileSystem, l[0], tag) if len(l) == 1 and self.isPakFile(l[0]) \
                    else ManyPakFile(
                        self.createPakFileType(fileSystem, None, tag), self, \
                            p if len(p) > 0 else 'Many', fileSystem, l, visualPathSkip = len(p) + 1 if len(p) > 0 else 0
                        )
            case s if isinstance(value, list):
                return value[0] if len(s) == 1 else \
                    MultiPakFile(self, 'Multi', fileSystem, s, tag)
            case None: return None
            case _: raise Exception(f'Unknown: {value}')

    # create PakFileType
    def createPakFileType(self, fileSystem: FileSystem, path: str, tag: Any = None) -> PakFile:
        if not self.pakFileType: raise Exception(f'{self.id} missing PakFileType')
        return findType(self.pakFileType)(self, fileSystem, path, tag)

    # find Paths
    def findPaths(self, fileSystem: FileSystem, edition: Edition, dlc: DownloadableContent, searchPattern: str):
        ignores = self.family.fileManager.ignores
        gameIgnores = _value(ignores, self.id)
        for path in self.paths or ['']:
            dlcPath = os.path.join(path, dlc.path) if dlc and dlc.path else path
            fileSearch = fileSystem.findPaths(dlcPath, searchPattern)
            if gameIgnores: fileSearch = [x for x in fileSearch if not os.path.basename(x) in gameIgnores]
            yield (path, list(fileSearch))

    # is a PakFile
    def isPakFile(self, path: str) -> bool:
        return any([x for x in self.pakExts if path.endswith(x)])

class Resource:
    def __init__(self, fileSystem: FileSystem, game: FamilyGame, edition: FamilyGame.Edition, searchPattern: str):
        self.fileSystem = fileSystem
        self.game = game
        self.edition = edition
        self.searchPattern = searchPattern
    def __repr__(self): return f'res:/{self.searchPattern}#{self.game}'

@staticmethod
def init():
    def commentRemover(text: str) -> str:
        def replacer(match): s = match.group(0); return ' ' if s.startswith('/') else s
        pattern = re.compile(r'//.*?$|/\*.*?\*/|\'(?:\\.|[^\\\'])*\'|"(?:\\.|[^\\"])*"', re.DOTALL | re.MULTILINE)
        return re.sub(pattern, replacer, text)
    def loadJson(path: str) -> dict[str, Any]:
        body = resources.files().joinpath('Specs', path).read_text(encoding='utf-8')
        return json.loads(commentRemover(body).encode().decode('utf-8-sig'))
    families = {}
    for path in [f'{x}Family.json' for x in familyKeys]:
        family = createFamily(path, loadJson)
        families[family.id] = family
    return families

@staticmethod
def getFamily(id: str, throwOnError: bool = True) -> Family:
    family = _value(families, id)
    if not family and throwOnError: raise Exception(f'Unknown family: {id}')
    return family

families = init()
unknown = getFamily('Unknown')
unknownPakFile = unknown.openPakFile('game:/#APP', throwOnError=False)
# print(families)
