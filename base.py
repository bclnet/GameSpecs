import os, json, glob, re

class Family:
  def __init__(t, id, name, games, otherGames):
    t.id = id
    t.name = name
    t.games = games
    t.otherGames = otherGames

  def __repr__(self):
    return f'''

{self.id}: {self.name}
Games: {[x for x in self.games]}
OtherGames: {[x for x in self.otherGames]}
'''

class Game:
  def __init__(t, id, name, engine, url, date, key, pakExt, status):
    t.id = id
    t.name = name
    t.engine = engine
    t.url = url
    t.date = date
    t.key = key
    t.pakExt = pakExt
    t.status = status

  def __repr__(self):
    return f'\n  {self.id}: {self.name}'

def getFamilies(root):
    def commentRemover(text):
        def replacer(match):
            s = match.group(0)
            return ' ' if s.startswith('/') else s # note: a space and not an empty string
        pattern = re.compile(r'//.*?$|/\*.*?\*/|\'(?:\\.|[^\\\'])*\'|"(?:\\.|[^\\"])*"', re.DOTALL | re.MULTILINE)
        return re.sub(pattern, replacer, text)
    def jsonLoad(file):
        f = open(file, encoding='utf8')
        data = json.loads(commentRemover(f.read()).encode().decode('utf-8-sig'))
        f.close()
        return data

    families = []
    for file in glob.glob(root + '*/*.json'):
        data = jsonLoad(file)
        default_ = Game(id=None,name=None,engine=None,url=None,date=None,key=None,pakExt=None,status=None)
        games = []
        if 'games' in data:
            for id in data['games']:
                s = data['games'][id]
                game = Game(
                    id = id,
                    name = s['name'] if 'name' in s else None,
                    engine = s['engine'] if 'engine' in s else default_.engine,
                    url = (', '.join(s['url']) if isinstance(s['url'], list) else s['url']) if 'url' in s else [],
                    date = s['date'] if 'date' in s else None,
                    key = s['key'] if 'key' in s else None,
                    pakExt = s['pakExt'] if 'pakExt' in s else default_.pakExt,
                    status = s['status'] if 'status' in s else []
                )
                if id.startswith('*'): default_ = game
                else: games.append(game)
        default_ = Game(id=None,name=None,engine=None,url=None,date=None,key=None,pakExt=None,status=None)
        otherGames = []
        if 'other-games' in data:
            for id in data['other-games']:
                s = data['other-games'][id]
                game = Game(
                    id = id,
                    name = s['name'] if 'name' in s else None,
                    engine = s['engine'] if 'engine' in s else default_.engine,
                    url = (', '.join(s['url']) if isinstance(s['url'], list) else s['url']) if 'url' in s else [],
                    date = s['date'] if 'date' in s else None,
                    key = s['key'] if 'key' in s else None,
                    pakExt = s['pakExt'] if 'pakExt' in s else default_.pakExt,
                    status = s['status'] if 'status' in s else []
                )
                if id.startswith('*'): default_ = game
                else: otherGames.append(game)
        families.append(Family(
            id = data['id'],
            name = data['name'] if 'name' in data else None,
            games = games,
            otherGames = otherGames
        ))
    return families
