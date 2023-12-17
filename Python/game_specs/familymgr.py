import os, json, glob, re
from typing import Any
from urllib.parse import urlparse
from importlib import resources
from .openstack_poly import findType
from .pakfile import PakFile, ManyPakFile, MultiPakFile
from .filesys import FileSystem, HostFileSystem, createFileSystem
from .filemgr import FileManager

class FamilyGame: pass
class Resource: pass

def _value(d, key, default = None): return d[key] if key in d else default
def _list(d, key, default = None): return (d[key] if isinstance(d[key], list) else [d[key]]) if key in d else default
def _method(method, d, key, default = None): return method(d[key]) if key in d else default

class Family:
    def __init__(self, d):
        self.id = d['id']
        self.name = _value(d, 'name')
        self.studio = _value(d, 'studio')
        self.description = _value(d, 'description')
        self.urls = _list(d, 'url')
        # engines
        self.engines = engines = {}
        if 'engines' in d:
            for (id, val) in d['engines'].items():
                engines[id] = FamilyEngine(self, id, val)
        # games
        self.games = games = {}
        dgame = FamilyGame(None, self, None, None)
        if 'games' in d:
            for (id, val) in d['games'].items():
                game = FamilyGame(dgame, self, id, val)
                if id.startswith('*'): dgame = game
                else: games[id] = game
        # file manager
        self.fileManager = _method(FileManager, d, 'fileManager')
    def __repr__(self): return f'''
{self.id}: {self.name}
engines: {[x for x in self.engines.values()] if self.engines else None}
games: {[x for x in self.games.values()] if self.games else None}
fileManager: {self.fileManager if self.fileManager else None}'''

    # get Game
    def getGame(self, id: str, throwOnError: bool = True) -> FamilyGame:
        game = self.games[id] if id in self.games else None
        if not game and throwOnError: raise Exception(f'Unknown game: {id}')
        return game

    # parse Resource
    def parseResource(self, uri: str, throwOnError: bool = True) -> Resource:
        if uri is None or not (uri := urlparse(uri)).fragment:
            return Resource(Game = FamilyGame(None, self, None, None))
        game = self.getGame(uri.fragment)
        searchPattern = '' if uri.scheme == 'file' else uri.path[1:]
        paths = self.fileManager.paths
        fileSystemType = game.fileSystemType
        fileSystem = \
            (createFileSystem(fileSystemType, paths[game.id][0]) if game.id in paths and paths[game.id] else None) if uri.scheme == 'game' else \
            (createFileSystem(fileSystemType, uri.path) if uri.path else None) if uri.scheme == 'file' else \
            (createFileSystem(fileSystemType, None, uri) if uri.netloc else None) if uri.scheme.startswith('http') else None
        if not fileSystem:
            if throwOnError: raise Exception(f'Not located: {uri}')
            else: return None
        return Resource(fileSystem, game, searchPattern)

    # open PakFile
    def openPakFile(self, res, throwOnError: bool = True) -> PakFile:
        resource = res if isinstance(res, Resource) else \
            self.parseResource(res) if isinstance(res, str) else \
            None
        if not resource:
            if throwOnError: raise Exception(f'Unknown res: {res}')
            else: return None
        if not resource.game: raise Exception(f'Undefined Game')
        return (pak := resource.game.createPakFile(resource.fileSystem, resource.searchPattern, throwOnError)) and pak.open()

class FamilyEngine:
    def __init__(self, family: Family, id: str, d):
        self.family = family
        self.id = id
        self.name = _value(d, 'name')
    def __repr__(self): return f'\n  {self.id}: {self.name}'

# parse key
@staticmethod
def parseKey(key) -> Any:
    if not key: return None
    elif key.startswith('hex:'): return bytes.fromhex(key[4:].replace('/x', ''))
    elif key.startswith('txt:'): return key[4:].encode('ascii')
    else: raise Exception(f'Unknown key: {key}')

class FamilyGame:
    class Edition:
        def __init__(self, id: str, d):
            self.id = id
            self.name = _value(d, 'name')
            self.key = _method(parseKey, d, 'key')
        def __repr__(self): return f'{self.id}: {self.name}'
    class DownloadableContent:
        def __init__(self, id: str, d):
            self.id = id
            self.name = _value(d, 'name')
            self.path = _value(d, 'path')
        def __repr__(self): return f'{self.id}: {self.name}'
    class Locale:
        def __init__(self, id: str, d):
            self.id = id
            self.name = _value(d, 'name')
        def __repr__(self): return f'{self.id}: {self.name}'
    def __init__(self, dgame, family: Family, id: str, d):
        self.family = family
        self.id = id
        if not dgame: self.ignore = False; self.engine = \
            self.paths = self.key = self.fileSystemType = \
            self.searchBy = self.pakFileType = self.pakExts = None; return
        self.ignore = _value(d, 'n/a', dgame.ignore)
        self.name = _value(d, 'name')
        self.engine = _value(d, 'engine', dgame.engine)
        self.urls = _list(d, 'url')
        self.date = _value(d, 'date')
        #self.option = _list(d, 'option', dgame.option)
        #self.paks = _list(d, 'paks', dgame.paks)
        #self.dats = _list(d, 'dats', dgame.dats)
        self.paths = _list(d, 'path', dgame.paths)
        self.key = _method(parseKey, d, 'key', dgame.key)
        self.status = _value(d, 'status')
        self.tags = _value(d, 'tags')
        # interface
        self.fileSystemType = _value(d, 'fileSystemType', dgame.fileSystemType)
        self.searchBy = _value(d, 'searchBy', dgame.searchBy)
        self.pakFileType = _value(d, 'pakFileType', dgame.pakFileType)
        self.pakExts = _list(d, 'pakExt', dgame.pakExts) 
        # related
        self.editions = editions = {}
        if 'editions' in d:
            for (id, val) in d['editions'].items():
                editions[id] = FamilyGame.Edition(id, val)
        self.dlcs = dlcs = {}
        if 'dlcs' in d:
            for (id, val) in d['dlcs'].items():
                dlcs[id] = FamilyGame.DownloadableContent(id, val)
        self.locales = locales = {}
        if 'locales' in d:
            for (id, val) in d['locales'].items():
                locales[id] = FamilyGame.Locale(id, val)
    def __repr__(self): return f'''
  {self.id}: {self.name} {self.status}
  - editions: {self.editions if self.editions else None}
  - dlcs: {self.dlcs if self.dlcs else None}
  - locales: {self.locales if self.locales else None}'''
  
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
        if isinstance(fileSystem, HostFileSystem):
            raise Exception('HostFileSystem not supported')
        searchPattern = self.createSearchPatterns(searchPattern)
        pakFiles = []
        for p in self.findPaths(fileSystem, searchPattern):
            if self.searchBy == 'Pak':
                for path in p[1]:
                    if self.isPakFile(path): pakFiles.append(self.createPakFileObj(fileSystem, path))
            else: pakFiles.append(self.createPakFileObj(fileSystem, p))
        return self.createPakFileObj(fileSystem, pakFiles)

    # create createPakFileObj
    def createPakFileObj(self, fileSystem: FileSystem, value, tag = None) -> PakFile:
        if isinstance(value, str):
            if self.isPakFile(value): return self.createPakFileType(fileSystem, value, tag)
            else: raise Exception(f'{self.id} missing {value}')
        elif isinstance(value, tuple):
            p, l = value
            return self.createPakFileObj(fileSystem, l[0], tag) if len(l) == 1 and self.isPakFile(l[0]) \
                else ManyPakFile(self.createPakFileType(fileSystem, 'Base', tag), self, p if len(p) > 0 else 'Many', fileSystem, l, visualPathSkip = len(p) + 1 if len(p) > 0 else 0)
        elif isinstance(value, list):
            return value[0] if len(value) == 1 \
                else MultiPakFile(self, 'Multi', fileSystem, value, tag)
        elif value is None: return None
        else: raise Exception(f'Unknown: {value}')

    # create PakFileType
    def createPakFileType(self, fileSystem: FileSystem, path: str, tag = None) -> PakFile:
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
        return any([x for x in self.pakExts if x.endswith(x)])

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
    def jsonLoad(path: str):
        body = resources.files('Specs').joinpath(path).read_text(encoding='utf-8')
        return json.loads(commentRemover(body).encode().decode('utf-8-sig'))
    families = {}
    for path in [f'{x}Family.json' for x in familyKeys]:
        family = Family(jsonLoad(path))
        families[family.id] = family
    return families

@staticmethod
def getFamily(id, throwOnError: bool = True) -> Family:
    family = families[id] if id in families else None
    if not family and throwOnError: raise Exception(f'Unknown family: {id}')
    return family

families = init()
# print(families)
