import sys, os, re
sys.path.append('../..')
from base import getFamilies

def writeFile(path, marker, body):
    f = open(path, 'r')
    text = f.read()
    f.close()
    head, sep, tail = text.partition(marker)
    text = head + sep + body
    f = open(path, 'w')
    f.write(text)
    f.close()

def GameFamily(f):
    b = ['\n']
    b.append(f'{f.id}\n')
    b.append(f'name: {f.name}\n')
    b.append(f'studio: {f.studio}\n')
    b.append(f'description: {f.description}\n')
    b.append(f'url: {f.url}\n')
    b.append(f'\n')
    b.append(f'=== Games\n\n')
    for g in f.games:
        b.append(f'{g.id}\n')
        b.append(f'name: {g.name}\n')
        b.append(f'engine: {g.engine}\n')
        b.append(f'url: {g.url}\n')
        b.append(f'date: {g.date}\n')
        b.append(f'key: {g.key}\n')
        b.append(f'pakExt: {g.pakExt}\n')
        b.append(f'\n')
    b.append(f'\n')
    b.append(f'=== Other-Games\n\n')
    for g in f.otherGames:
        b.append(f'{g.id}\n')
        b.append(f'name: {g.name}\n')
        b.append(f'engine: {g.engine}\n')
        b.append(f'url: {g.url}\n')
        b.append(f'date: {g.date}\n')
        b.append(f'\n')
    return ''.join(b)

    
def LocateFiles(f):
    b = ['\n']
    b.append(f'{f.id}\n')
    b.append(f'name: {f.name}\n')
    b.append(f'\n')
    b.append(f'=== Files\n\n')
    # for g in f.games:
    #     b.append(f'{g.id}\n')
    #     b.append(f'name: {g.name}\n')
    #     b.append(f'engine: {g.engine}\n')
    #     b.append(f'url: {g.url}\n')
    #     b.append(f'date: {g.date}\n')
    #     b.append(f'key: {g.key}\n')
    #     b.append(f'pakExt: {g.pakExt}\n')
    #     b.append(f'\n')
    return ''.join(b)


for f in getFamilies('../../'):
    print(f.id)
    #body = GameFamily(f)
    #if not f.id.startswith('Capcom'): writeFile(f'book/02-game-families/{f.id}.asc', '=== Table\n', body)
    body = LocateFiles(f)
    if not f.id.startswith('Capcom'): writeFile(f'book/03-locate-files/{f.id}.asc', '=== Table\n', body)