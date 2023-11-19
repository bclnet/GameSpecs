import sys, os, re
sys.path.append('../..')
from base import getFamilies

def writeFile(z, path, marker, body):
    f = open(path, 'r', encoding='utf-8')
    text = f.read()
    f.close()
    head, sep, tail = text.partition(marker)
    text = head + sep + body
    f = open(path, 'w', encoding='utf-8')
    f.write(text)
    f.close()

def GameFamily(f):
    b = ['\n']
    b.append('[cols="1"]\n')
    b.append('|===\n')
    b.append(f'|{f.id}\n')
    b.append(f'|name: {f.name}\n')
    b.append(f'|studio: {f.studio}\n')
    b.append(f'|description: {f.description}\n')
    b.append(f'|url: {f.url}\n')
    b.append('|===\n')
    b.append(f'\n')
    if f.games:
        b.append(f'==== Games\n\n')
        for s in f.games:
            b.append('[cols="1"]\n')
            b.append('|===\n')
            b.append(f'|{s.id}\n')
            b.append(f'|name: {s.name}\n')
            b.append(f'|engine: {s.engine}\n')
            b.append(f'|url: {s.url}\n')
            b.append(f'|date: {s.date}\n')
            b.append(f'|key: {s.key}\n')
            b.append(f'|pakExt: {s.pakExt}\n')
            b.append('|===\n')
            b.append(f'\n')
        b.append(f'\n')
    if f.otherGames:
        b.append(f'==== Other-Games\n\n')
        for s in f.otherGames:
            b.append('[cols="1"]\n')
            b.append('|===\n')
            b.append(f'|{s.id}\n')
            b.append(f'|name: {s.name}\n')
            b.append(f'|engine: {s.engine}\n')
            b.append(f'|url: {s.url}\n')
            b.append(f'|date: {s.date}\n')
            b.append('|===\n')
            b.append(f'\n')
    return ''.join(b)
    
def LocateFiles(fm):
    b = ['\n']
    b.append(f'==== Files\n\n')
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
    writeFile(f, f'book/02-game-families/{f.id}.asc', '==== Family Info\n', body)
    if f.fileManager != None:
        body = LocateFiles(f.fileManager)
        writeFile(f, f'book/03-locate-files/{f.id}.asc', '=== Table\n', body)