import sys, os, json, glob, re
from urllib.parse import urlparse
sys.path.append('../../03-locate-files/python')
import FileManager

class Resource:
    def __init__(s, fileSystem, game, searchPattern):
        s.fileSystem = fileSystem
        s.game = game
        s.searchPattern = searchPattern
    def __repr__(s): return f'resource:/{s.searchPattern}#{s.game}'

class Family:
    def __init__(s, d):
        s.id = d['id']
        s.name = d['name'] if 'name' in d else None
        s.studio = d['studio'] if 'studio' in d else None
        s.description = d['description'] if 'description' in d else None
        s.urls = (d['url'] if isinstance(d['url'], list) else [d['url']]) if 'url' in d else []
        # engines
        s.engines = engines = {}
        if 'engines' in d:
            for id in d['engines']:
                engines[id] = FamilyEngine(s, id, d['engines'][id])
        # games
        s.games = games = {}
        dgame = FamilyGame(None, s, None, None)
        if 'games' in d:
            for id in d['games']:
                game = FamilyGame(dgame, s, id, d['games'][id])
                if id.startswith('*'): dgame = game
                else: games[id] = game
        # file manager
        s.fileManager = FileManager.FileManager(d['fileManager']) if 'fileManager' in d else None
    def __repr__(s): return f'''
{s.id}: {s.name}
engines: {[x for x in s.engines.values()] if s.engines else None}
games: {[x for x in s.games.values()] if s.games else None}
fileManager: {s.fileManager if s.fileManager else None}'''
    def getGame(s, id, throwOnError = True):
        game = s.games[id] if id in s.games else None
        if not game and throwOnError: raise Exception(f'Unknown game: {id}')
        return game
    # parse Resource
    def parseResource(s, uri, throwOnError = True):
        if uri is None or not (uri := urlparse(uri)).fragment: return Resource(Game = FamilyGame(None, s, None, None))
        game = s.getGame(uri.fragment)
        searchPattern = '' if uri.scheme == 'file' else uri.path[1:]
        # var fileSystem =
        #     // game-scheme
        #     string.Equals(uri.Scheme, "game", StringComparison.OrdinalIgnoreCase) ? Paths.TryGetValue(game.Id, out var z) ? game.CreateFileSystem(z.Single()) : (throwOnError ? throw new ArgumentOutOfRangeException(nameof(uri), $"{game.Id}: unable to locate game resources") : (IFileSystem)null)
        #     // file-scheme
        #     : uri.IsFile ? !string.IsNullOrEmpty(uri.LocalPath) ? game.CreateFileSystem(uri.LocalPath) : (throwOnError ? throw new ArgumentOutOfRangeException(nameof(uri), $"{game.Id}: unable to locate file resources") : (IFileSystem)null)
        #     // network-scheme
        #     : !string.IsNullOrEmpty(uri.Host) ? new HostFileSystem(uri) : (throwOnError ? throw new ArgumentOutOfRangeException(nameof(uri), $"{game.Id}: unable to locate network resources") : (IFileSystem)null);
        return Resource(game, s, searchPattern)
    # open PakFile
    def openPakFile(s, res, throwOnError = True):
        resource = res if isinstance(res, Resource) else \
            s.parseResource(res) if isinstance(res, str) else None
        if not resource and throwOnError: raise Exception(f'Unknown res: {res}')
        return 'PAK'

class FamilyEngine:
    def __init__(s, family, id, d):
        s.family = family
        s.id = id
        s.name = d['name'] if 'name' in d else None
    def __repr__(s): return f'\n  {s.id}: {s.name}'

class FamilyGame:
    class Edition:
        def __init__(s, id, d):
            s.id = id
            s.name = d['name'] if 'name' in d else None
            s.key = d['key'] if 'key' in d else None
        def __repr__(s): return f'{s.id}: {s.name}'
    class DownloadableContent:
        def __init__(s, id, d):
            s.id = id
            s.name = d['name'] if 'name' in d else None
            s.path = d['path'] if 'path' in d else None
        def __repr__(s): return f'{s.id}: {s.name}'
    class Locale:
        def __init__(s, id, d):
            s.id = id
            s.name = d['name'] if 'name' in d else None
        def __repr__(s): return f'{s.id}: {s.name}'
    def __init__(s, dgame, family, id, d):
        s.family = family
        s.id = id
        if not dgame: s.ignore = False; s.engine = s.searchBy = s.pakExts = None; return
        s.ignore = d['n/a'] if 'n/a' in d else dgame.ignore
        s.name = d['name'] if 'name' in d else None
        s.engine = d['engine'] if 'engine' in d else dgame.engine
        s.urls = (d['url'] if isinstance(d['url'], list) else [d['url']]) if 'url' in d else None
        s.date = d['date'] if 'date' in d else None
        s.pakExts = (d['pakExt'] if isinstance(d['pakExt'], list) else [d['pakExt']]) if 'pakExt' in d else dgame.pakExts
        s.key = d['key'] if 'key' in d else None
        s.searchBy = d['searchBy'] if 'searchBy' in d else dgame.searchBy
        s.status = d['status'] if 'status' in d else []
        s.tags = d['tags'] if 'tags' in d else None
        # editions
        s.editions = editions = {}
        if 'editions' in d:
            for id in d['editions']:
                editions[id] = FamilyGame.Edition(id, d['editions'][id])
        # dlc
        s.dlc = dlc = {}
        if 'dlc' in d:
            for id in d['dlc']:
                dlc[id] = FamilyGame.DownloadableContent(id, d['dlc'][id])
        # locales
        s.locales = locales = {}
        if 'locales' in d:
            for id in d['locales']:
                locales[id] = FamilyGame.Locale(id, d['locales'][id])
    def __repr__(s): return f'''
  {s.id}: {s.name} {s.status}
  - editions: {s.editions if s.editions else None}
  - dlc: {s.dlc if s.dlc else None}
  - locales: {s.locales if s.locales else None}'''

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

families = init('../../../../../')
# print(families)
