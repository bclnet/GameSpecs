import sys, os, re
sys.path.append('../..')
from base import getFamilies

def WriteBody(path, marker, body):
    f = open(path, 'r')
    text = f.read()
    f.close()
    head, sep, tail = text.partition(marker)
    text = head + sep + '\n' + body
    f = open(path, 'w')
    f.write(text)
    f.close()

def GameFamily(f):
    b = ['''The following are the current games:\n
| ID | Name | Open | Read | Texure | Model | Level
| -- | --   | --   | --   | --     | --    | --
''']
    b.append(f'{f.id}\n')
    b.append(f'name: {f.name}\n')
    b.append(f'Games:\n')
    for g in f.games:
        b.append(f'{g.id}\n')
        b.append(f'name: {g.name}\n')
        b.append(f'engine: {g.engine}\n')
        b.append(f'url: {g.url}\n')
        b.append(f'date: {g.date}\n')
        b.append(f'key: {g.key}\n')
        b.append(f'pakExt: {g.pakExt}\n')
        b.append(f'\n')
    b.append(f'Other-Games:\n')
    for g in f.otherGames:
        b.append(f'{g.id}\n')
        b.append(f'name: {g.name}\n')
        b.append(f'engine: {g.engine}\n')
        b.append(f'url: {g.url}\n')
        b.append(f'date: {g.date}\n')
        b.append(f'\n')
    return ''.join(b)

for f in getFamilies('../../'):
    body = GameFamily(f)
    print(body)
    Write(f'02-game-families/{f.id}.asc', body, '=== Table')