import os, json, glob, re

def Readme(games):
    # read
    f = open('README.md', 'r')
    text = f.read()
    f.close()
    # body
    head, sep, tail = text.partition('''## Games
---''')
    body = head + sep + games
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

def Body(families):
    header = '''The following are the current games:

    | ID                                               | Name                      | Sample Game       | Status
    | --                                               | --                        | --                | --'''
    breakx = '''
    | **ID**                                           |                           |                   |'''
    game = '''
    | game | gm|                   |'''

print(Families())
print('done')