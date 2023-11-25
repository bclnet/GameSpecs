import os, json, glob, re

class Family:
    def __init__(s, d):
        s.id = d['id']
        s.name = d['name'] if 'name' in d else None
        s.studio = d['studio'] if 'studio' in d else None
        s.description = d['description'] if 'description' in d else None
        s.url = (d['url'] if isinstance(d['url'], list) else [d['url']]) if 'url' in d else []
        # games
        s.games = games = []
        dgame = Game(None, s, None, None)
        if 'games' in d:
            for id in d['games']:
                game = Game(dgame, s, id, d['games'][id])
                if id.startswith('*'): dgame = game
                else: games.append(game)
        # fileManager
        s.fileManager = FileManager(d['fileManager']) if 'fileManager' in d else None
    def __repr__(s): return f'''
{s.id}: {s.name}
Games: {[x for x in s.games]}
Applications: {[x for x in s.fileManager.applications] if s.fileManager else None}
'''

class Game:
    def __init__(s, dgame, family, id, d):
        s.family = family
        s.id = id
        if not dgame: s.ignore = False; s.engine = s.pakExt = None; return
        s.ignore = d['n/a'] if 'n/a' in d else dgame.ignore
        s.name = d['name'] if 'name' in d else None
        s.engine = d['engine'] if 'engine' in d else dgame.engine
        s.url = (d['url'] if isinstance(d['url'], list) else [d['url']]) if 'url' in d else []
        s.date = d['date'] if 'date' in d else None
        s.key = d['key'] if 'key' in d else None
        s.pakExt = d['pakExt'] if 'pakExt' in d else dgame.pakExt
        s.status = d['status'] if 'status' in d else []
        s.tags = d['tags'] if 'tags' in d else None
    def __repr__(s): return f'\n  {s.id}: {s.name} {s.status}'

class FileManager:
    def __init__(s, d):
        # applications
        s.applications = applications = []
        if 'application' in d:
            for id in d['application']:
                applications.append(FMApplication(id, d['application'][id]))
        # ignores
        s.ignores = ignores = []
        if 'ignores' in d:
            for id in d['ignores']:
                ignores.append(FMIgnore(id, d['ignores'][id]))
    def __repr__(s): return f'\n  {s.id}'

class FMApplication:
    def __init__(s, id, d):
        s.id = id
        s.dir = (', '.join(d['dir']) if isinstance(d['dir'], list) else d['dir']) if 'dir' in d else None
        s.key = (', '.join(d['key']) if isinstance(d['key'], list) else d['key']) if 'key' in d else None
        s.reg = (', '.join(d['reg']) if isinstance(d['reg'], list) else d['reg']) if 'reg' in d else None
        s.path = (', '.join(d['path']) if isinstance(d['path'], list) else d['path']) if 'path' in d else None
    def __repr__(s): return f'\n  {s.id}'

class FMIgnore:
    def __init__(s, id, d):
        s.id = id
        s.path = (', '.join(d['path']) if isinstance(d['path'], list) else d['path']) if 'path' in d else None
    def __repr__(s): return f'\n  {s.id}'

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
