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
    for s in f.games:
        b.append(f'{s.id}\n')
        b.append(f'name: {s.name}\n')
        b.append(f'engine: {s.engine}\n')
        b.append(f'url: {s.url}\n')
        b.append(f'date: {s.date}\n')
        b.append(f'key: {s.key}\n')
        b.append(f'pakExt: {s.pakExt}\n')
        b.append(f'\n')
    b.append(f'\n')
    b.append(f'=== Other-Games\n\n')
    for s in f.otherGames:
        b.append(f'{s.id}\n')
        b.append(f'name: {s.name}\n')
        b.append(f'engine: {s.engine}\n')
        b.append(f'url: {s.url}\n')
        b.append(f'date: {s.date}\n')
        b.append(f'\n')
    return ''.join(b)

    
def LocateFiles(fm):
    b = ['\n']
    b.append(f'=== Files\n\n')
    for s in fm.applications:
         b.append(f'{s.id}\n')
         b.append(f'dir: {s.dir}\n')
         b.append(f'key: {s.key}\n')
         b.append(f'reg: {s.reg}\n')
         b.append(f'path: {s.path}\n')
         b.append(f'\n')
    return ''.join(b)


for f in getFamilies('../../'):
    print(f.id)
    body = GameFamily(f)
    if not f.id.startswith('Capcom'):
        writeFile(f'book/02-game-families/{f.id}.asc', '=== Table\n', body)
    if f.fileManager != None:
        body = LocateFiles(f.fileManager)
        writeFile(f'book/03-locate-files/{f.id}.asc', '=== Table\n', body)