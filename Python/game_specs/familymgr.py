import os, json, glob, re
from urllib.parse import urlparse
import pakfile, filemgr, filesys

class Family:
    def __init__(self, d):
        self.id = d['id']
        self.name = d['name'] if 'name' in d else None
        self.studio = d['studio'] if 'studio' in d else None
        self.description = d['description'] if 'description' in d else None
        self.urls = (d['url'] if isinstance(d['url'], list) else [d['url']]) if 'url' in d else []
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
        self.fileManager = filemgr.FileManager(d['fileManager']) if 'fileManager' in d else None
    def __repr__(self): return f'''
{self.id}: {self.name}
engines: {[x for x in self.engines.values()] if self.engines else None}
games: {[x for x in self.games.values()] if self.games else None}
fileManager: {self.fileManager if self.fileManager else None}'''

    # get Game
    def getGame(self, id, throwOnError = True):
        game = self.games[id] if id in self.games else None
        if not game and throwOnError: raise Exception(f'Unknown game: {id}')
        return game

    # parse Resource
    def parseResource(self, uri, throwOnError = True):
        if uri is None or not (uri := urlparse(uri)).fragment: return Resource(Game = FamilyGame(None, self, None, None))
        game = self.getGame(uri.fragment)
        searchPattern = '' if uri.scheme == 'file' else uri.path[1:]
        paths = self.fileManager.paths
        fileSystem = \
            (game.createFileSystem(paths[game.id][0]) if game.id in paths and paths[game.id] else None) if uri.scheme == 'game' else \
            (game.createFileSystem(uri.path) if uri.path else None) if uri.scheme == 'file' else \
            (game.createFileSystem(None, uri) if uri.netloc else None) if uri.scheme.startswith('http') else None
        if not fileSystem:
            if throwOnError: raise Exception(f'Unknown schema: {uri}')
            else: return None
        return Resource(fileSystem, game, searchPattern)

    # open PakFile
    def openPakFile(self, res, throwOnError = True):
        resource = res if isinstance(res, Resource) else \
            self.parseResource(res) if isinstance(res, str) else None
        if not resource:
            if throwOnError: raise Exception(f'Unknown res: {res}')
            else: return None
        if not resource.game: raise Exception(f'Undefined Game')
        return (pak := resource.game.createPakFile(resource.fileSystem, resource.searchPattern, throwOnError)) and pak.open()

class FamilyEngine:
    def __init__(self, family, id, d):
        self.family = family
        self.id = id
        self.name = d['name'] if 'name' in d else None
    def __repr__(self): return f'\n  {self.id}: {self.name}'

class FamilyGame:
    class Edition:
        def __init__(self, id, d):
            self.id = id
            self.name = d['name'] if 'name' in d else None
            self.key = d['key'] if 'key' in d else None
        def __repr__(self): return f'{self.id}: {self.name}'
    class DownloadableContent:
        def __init__(self, id, d):
            self.id = id
            self.name = d['name'] if 'name' in d else None
            self.path = d['path'] if 'path' in d else None
        def __repr__(self): return f'{self.id}: {self.name}'
    class Locale:
        def __init__(self, id, d):
            self.id = id
            self.name = d['name'] if 'name' in d else None
        def __repr__(self): return f'{self.id}: {self.name}'
    def __init__(self, dgame, family, id, d):
        self.family = family
        self.id = id
        if not dgame: self.ignore = False; self.engine = self.paths = self.key = self.fileSystemType = self.searchBy = self.pakFileType = self.pakExts = None; return
        self.ignore = d['n/a'] if 'n/a' in d else dgame.ignore
        self.name = d['name'] if 'name' in d else None
        self.engine = d['engine'] if 'engine' in d else dgame.engine
        self.urls = (d['url'] if isinstance(d['url'], list) else [d['url']]) if 'url' in d else None
        self.date = d['date'] if 'date' in d else None
        #self.option
        #self.paks
        #self.dats
        self.paths = (d['path'] if isinstance(d['path'], list) else [d['path']]) if 'path' in d else dgame.paths
        self.key = d['key'] if 'key' in d else dgame.key
        self.status = d['status'] if 'status' in d else None
        self.tags = d['tags'] if 'tags' in d else None
        # interface
        self.fileSystemType = d['fileSystemType'] if 'fileSystemType' in d else dgame.fileSystemType
        self.searchBy = d['searchBy'] if 'searchBy' in d else dgame.searchBy
        self.pakFileType = d['pakFileType'] if 'pakFileType' in d else dgame.pakFileType
        self.pakExts = (d['pakExt'] if isinstance(d['pakExt'], list) else [d['pakExt']]) if 'pakExt' in d else dgame.pakExts
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
  
    # create FileSystem
    def createFileSystem(self, root, host = None):
        return filesys.HostFileSystem(host) if host else \
            dynamicType(self.fileSystemType)(self, root) if self.fileSystemType else \
            filesys.StandardFileSystem(root)

    # create PakFile
    def createPakFile(self, fileSystem, searchPattern, throwOnError):
        if isinstance(fileSystem, filesys.HostFileSystem): raise Exception('HostFileSystem not supported')
        searchPattern = self.createSearchPatterns(searchPattern)
        pakFiles = []
        for p in self.findPaths(fileSystem, searchPattern):
            if self.searchBy == 'Pak':
                for path in p[1]:
                    if self.isPakFile(path): pakFiles.append(self.createPakFileObj(fileSystem, path))
            else: pakFiles.append(self.createPakFileObj(fileSystem, p))
        return self.createPakFileObj(fileSystem, pakFiles)

    # create createPakFileObj
    def createPakFileObj(self, fileSystem, value, tag = None):
        if isinstance(value, str):
            if self.isPakFile(value): return self.createPakFileType(fileSystem, value, tag)
            else: raise Exception(f'{self.id} missing {value}')
        elif isinstance(value, tuple):
            p, l = value
            return self.createPakFileObj(fileSystem, l[0], tag) if len(l) == 1 and self.isPakFile(l[0]) \
                else PakFile.ManyPakFile(self.createPakFileType(fileSystem, '', tag), self, v.Item1 if len(p) > 0 else 'Many', fileSystem, l, visualPathSkip = len(p) + 1 if len(p) > 0 else 0)
        elif isinstance(value, list):
            return value[0] if len(value) == 1 \
                else PakFile.MultiPakFile(self, 'Multi', fileSystem, v, tag)
        elif value is None: return None
        else: raise Exception(f'Unknown: {value}')

    # create PakFileType
    def createPakFileType(self, fileSystem, path, tag = None):
        if not self.pakFileType: raise Exception(f'{self.id} missing PakFileType')
        return dynamicType(self.pakFileType)(self, fileSystem, path, tag)

    def isPakFile(self, path):
        return any([x for x in self.pakExts if x.endswith(x)])

    # find Paths
    def findPaths(self, fileSystem, searchPattern):
        ignores = self.family.fileManager.ignores
        gameIgnores = ignores[self.id] if self.id in ignores else None
        for path in self.paths or ['']:
            fileSearch = filesys.findPaths(fileSystem, path, searchPattern)
            if gameIgnores: fileSearch = [x for x in fileSearch if not os.path.filename(x) in gameIgnores]
            yield (path, list(fileSearch))

    # create SearchPatterns
    def createSearchPatterns(self, searchPattern):
        if searchPattern: return searchPattern
        elif not self.searchBy: return '*'
        elif self.searchBy == 'Pak': return '' if not self.pakExts else f'*{self.pakExts[0]}' if self.pakExts.length == 1 else f'({'*:'.join(self.pakExts)})'
        elif self.searchBy == 'TopDir': return '*'
        elif self.searchBy == 'TwoDir': return '*/*'
        elif self.searchBy == 'AllDir': return '**/*'
        else: raise Exception(f'Unknown searchBy: {self.searchBy}')

class Resource:
    def __init__(self, fileSystem, game, searchPattern):
        self.fileSystem = fileSystem
        self.game = game
        self.searchPattern = searchPattern
    def __repr__(self): return f'resource:/{self.searchPattern}#{self.game}'

@staticmethod
def init(root):
    def commentRemover(text):
        def replacer(match): s = match.group(0); return ' ' if s.startswith('/') else s
        pattern = re.compile(r'//.*?$|/\*.*?\*/|\'(?:\\.|[^\\\'])*\'|"(?:\\.|[^\\"])*"', re.DOTALL | re.MULTILINE)
        return re.sub(pattern, replacer, text)
    def jsonLoad(file):
        with open(file, encoding='utf8') as f:
            return json.loads(commentRemover(f.read()).encode().decode('utf-8-sig'))
    families = {}
    for file in glob.glob(f'{root}*/*.json'):
        family = Family(jsonLoad(file))
        families[family.id] = family
    return families

@staticmethod
def getFamily(id, throwOnError = True):
    family = families[id] if id in families else None
    if not family and throwOnError: raise Exception(f'Unknown family: {id}')
    return family

@staticmethod
def dynamicType(klass):
    # print(f'create: {klass}')
    from importlib import import_module
    klass, modulePath = klass.rsplit(',', 1)
    try:
        _, className = klass.rsplit('.', 1)
        module = import_module(modulePath.strip().replace('.', '_'))
        return getattr(module, className)
    except (ImportError, AttributeError) as e: raise ImportError(klass)

families = init('../../')
# print(families)
