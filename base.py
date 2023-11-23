import os, json, glob, re

class Family:
  def __init__(t, id, name, studio, description, url, games, fileManager):
    t.id = id
    t.name = name
    t.studio = studio
    t.description = description
    t.url = url
    t.games = games
    t.fileManager = fileManager
  def __repr__(self): return f'''
{self.id}: {self.name}
Games: {[x for x in self.games]}
OtherGames: {[x for x in self.otherGames]}
FileManager.Applications: {[x for x in self.fileManager.applications]}
'''

class Game:
  def __init__(t, id, ignore, name, engine, url, date, key, pakExt, status, tags):
    t.id = id
    t.ignore = name
    t.name = name
    t.engine = engine
    t.url = url
    t.date = date
    t.key = key
    t.pakExt = pakExt
    t.status = status
    t.tags = tags
  def __repr__(self): return f'\n  {self.id}: {self.name}'

class FileManager:
  def __init__(t, applications, ignores):
    t.applications = applications
    t.ignores = ignores
  def __repr__(self): return f'\n  {self.id}'

class FMApplication:
  def __init__(t, id, dir, key, reg, path):
    t.id = id
    t.dir = dir
    t.key = key
    t.reg = reg
    t.path = path
  def __repr__(self): return f'\n  {self.id}'

class FMIgnore:
  def __init__(t, id, path):
    t.id = id
    t.path = path
  def __repr__(self): return f'\n  {self.id}'

def getFamilies(root):
    def commentRemover(text):
        def replacer(match):
            s = match.group(0)
            return ' ' if s.startswith('/') else s # note: a space and not an empty string
        pattern = re.compile(r'//.*?$|/\*.*?\*/|\'(?:\\.|[^\\\'])*\'|"(?:\\.|[^\\"])*"', re.DOTALL | re.MULTILINE)
        return re.sub(pattern, replacer, text)
    def jsonLoad(file):
      with open(file, encoding='utf8') as f:
        return json.loads(commentRemover(f.read()).encode().decode('utf-8-sig'))

    families = []
    for file in glob.glob(root + '*/*.json'):
        d = jsonLoad(file)
        # games
        default_ = Game(id=None,ignore=False,name=None,engine=None,url=None,date=None,key=None,pakExt=None,status=None,tags=None)
        games = []
        if 'games' in d:
            for id in d['games']:
                s = d['games'][id]
                game = Game(
                    id = id,
                    ignore = s['n/a'] if 'n/a' in s else default_.ignore,
                    name = s['name'] if 'name' in s else None,
                    engine = s['engine'] if 'engine' in s else default_.engine,
                    url = (s['url'] if isinstance(s['url'], list) else [s['url']]) if 'url' in s else [],
                    date = s['date'] if 'date' in s else None,
                    key = s['key'] if 'key' in s else None,
                    pakExt = s['pakExt'] if 'pakExt' in s else default_.pakExt,
                    status = s['status'] if 'status' in s else [],
                    tags = s['tags'] if 'tags' in s else None)
                if id.startswith('*'): default_ = game
                else: games.append(game)
        # fileManager
        fileManager = None
        if 'fileManager' in d:
            df = d['fileManager']
            # fm-applications
            fmApplications = []
            if 'application' in df:
                for id in df['application']:
                    s = df['application'][id]
                    fmApplications.append(FMApplication(
                        id = id,
                        dir = (', '.join(s['dir']) if isinstance(s['dir'], list) else s['dir']) if 'dir' in s else None,
                        key = (', '.join(s['key']) if isinstance(s['key'], list) else s['key']) if 'key' in s else None,
                        reg = (', '.join(s['reg']) if isinstance(s['reg'], list) else s['reg']) if 'reg' in s else None,
                        path = (', '.join(s['path']) if isinstance(s['path'], list) else s['path']) if 'path' in s else None))
            # fm-ignores
            fmIgnores = []
            if 'ignores' in df:
                for id in df['ignores']:
                    s = df['ignores'][id]
                    fmIgnores.append(FMIgnore(
                        id = id,
                        path = (', '.join(s['path']) if isinstance(s['path'], list) else s['path']) if 'path' in s else None))
            # file manager
            fileManager = FileManager(
                applications = fmApplications,
                ignores = fmIgnores)
        # add
        families.append(Family(
            id = d['id'],
            name = d['name'] if 'name' in d else None,
            studio = d['studio'] if 'studio' in d else None,
            description = d['description'] if 'description' in d else None,
            url = (d['url'] if isinstance(d['url'], list) else [d['url']]) if 'url' in d else [],
            games = games,
            fileManager = fileManager))
    return families
