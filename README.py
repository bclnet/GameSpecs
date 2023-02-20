import os, json, glob, re

def Readme(gamesBody):
    # read
    f = open('README.md', 'r')
    text = f.read()
    f.close()
    # body
    head, sep, tail = text.partition('''## Games
---''')
    body = head + sep + '\n' + gamesBody
    # write
    f = open('README.md', 'w')
    f.write(body)
    f.close()

def Families():
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
    for file in glob.glob('*/*.json'):
        data = jsonLoad(file)
        games = []
        for id in data['games']:
            s = data['games'][id]
            games.append({
                'id': id,
                'name': s['name'],
                'url': s['url'][0] if isinstance(s['url'], list) else s['url'],
                'status': s['status'] if 'status' in s else []
            })
        families.append({
            'id': data['id'],
            'name': data['name'],
            'games': games
        })
    return families

def GamesBody(families):
    def single(set, value): return value if value in set else '-'
    def platform(set, value):
        values = [i[len(value) + 1:].split('/') for i in g['status'] if i.startswith(value)]
        values = values[0] if len(values) > 0 else values
        gl = 'gl' if 'GL' in values else '--'
        un = 'un' if 'UN' in values else '--'
        ur = 'ur' if 'UR' in values else '--'
        vk = 'vk' if 'VK' in values else '--'
        return f'{gl} {un} {ur}'
    b = ['''The following are the current games:\n
| ID | Name | Open | Read | Texure | Model | Level
| -- | --   | --   | --   | --     | --    | --
''']
    for f in families:
        b.append(f'| **{f["id"]}** | **{f["name"]}**\n')
        for g in f['games']:
            b.append(f'| [{g["id"]}]({g["url"]}) | {g["name"]} | {single(g["status"], "open")} | {single(g["status"], "read")} | {platform(g["status"], "texture")} | {platform(g["status"], "model")} | {platform(g["status"], "level")}\n')
    return ''.join(b)
body = GamesBody(Families())
#print(body)
Readme(body)