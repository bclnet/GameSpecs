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
    b = ['''The following are the current games:\n
| ID | Name | Open | Read | Tex | Mdl | Lvl | OpenGl | Unity | Unreal
| -- | --   | --   | --   | --  | --  | --  | --     | --    | --
''']
    for f in families:
        b.append(f'| **{f["id"]}** | **{f["name"]}**\n')
        for g in f['games']:
            open = "open" if "open" in g["status"] else "-"
            read = "read" if "read" in g["status"] else "-"
            tex = "tex" if "tex" in g["status"] else "-"
            mdl = "mdl" if "mdl" in g["status"] else "-"
            lvl = "lvl" if "lvl" in g["status"] else "-"
            opengl = "opengl" if "opengl" in g["status"] else "-"
            unity = "unity" if "unity" in g["status"] else "-"
            unreal = "unreal" if "unreal" in g["status"] else "-"
            b.append(f'| [{g["id"]}]({g["url"]}) | {g["name"]} | {open} | {read} | {tex} | {mdl} | {lvl} | {opengl} | {unity} | {unreal}\n')
    return ''.join(b)

body = GamesBody(Families())
# print(body)
Readme(body)