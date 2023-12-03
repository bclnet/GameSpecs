import os, json, glob, re

import baseFM as FileManager

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
    # open PakFile
    def openPakFile(res, throwOnError = True):
        resource = res if isinstance(res, Resource) else \
        FileManager.parseResource(res) if isinstance(res, str) else None
        if not resource and throwOnError: raise Exception(f'Unknown res: {res}')

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
  - editions: {[x for x in s.editions.values()] if s.editions else None}
  - dlc: {[x for x in s.dlc.values()] if s.dlc else None}
  - locales: {[x for x in s.locales.values()] if s.locales else None}'''

@staticmethod
def init(root):
    def commentRemover(text):
        def replacer(match): s = match.group(0); return ' ' if s.startswith('/') else s
        pattern = re.compile(r'//.*?$|/\*.*?\*/|\'(?:\\.|[^\\\'])*\'|"(?:\\.|[^\\"])*"', re.DOTALL | re.MULTILINE)
        return re.sub(pattern, replacer, text)
    def jsonLoad(path):
        with open(path, encoding='utf8') as f:
            return json.loads(commentRemover(f.read()).encode().decode('utf-8-sig'))
    families = {}
    for path in glob.glob(f'{root}Specs/*.json'):
        family = Family(jsonLoad(path))
        families[family.id] = family
    return families

@staticmethod
def getFamily(familyName, throwOnError = True):
    family = families[familyName] if familyName in families else None
    if not family and throwOnError: raise Exception(f'Unknown family: {familyName}')
    return family

