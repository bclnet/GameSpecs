import sys, os, re
sys.path.append('../..')
from base import getFamilies

def writeFile(z, path, marker, body):
    with open(path, 'r', encoding='utf-8') as f: text = f.read()
    head, sep, tail = text.partition(marker)
    text = head + sep + body
    with open(path, 'w', encoding='utf-8') as f: f.write(text)

def getUrl(url):
    file = f'{url.replace(':', '').replace('/', '_')}.png'
    path = os.path.join('.', 'qrcodes', file)
    if not os.path.exists(path):
        print(file)
        qrcode.make(url).save(path)
    return f'image::qrcodes/{file}'


.Local version control diagram
image::images/local.png[Local version control diagram]


def GameFamily(f):
    b = ['\n']
    b.append('[cols="1"]\n')
    b.append('|===\n')
    b.append(f'|{f.id}\n')
    b.append(f'|name: {f.name}\n')
    b.append(f'|studio: {f.studio}\n')
    b.append(f'|description: {f.description}\n')
    b.append(f'|url: {[getUrl(x) for x in f.url]}\n')
    b.append('|===\n')
    b.append(f'\n')
    if f.games:
        b.append(f'==== Games\n\n')
        b.append(f'\n')
        b.append('[cols="1,1,1,1,1,1,1"]\n')
        b.append('|===\n')
        b.append(f'|Id\n')
        b.append(f'|Name\n')
        b.append(f'|Engine\n')
        b.append(f'|Date\n')
        b.append(f'|Key\n')
        b.append(f'|PakExt\n')
        b.append(f'|Url\n')
        for s in f.games:
            b.append('\n')
            b.append(f'|{s.id}\n')
            b.append(f'|{s.name}\n')
            b.append(f'|{s.engine}\n')
            b.append(f'|{s.date}\n')
            b.append(f'|{s.key}\n')
            b.append(f'|{s.pakExt}\n')
            b.append(f'|{[getUrl(x) for x in s.url]}\n')
        b.append('|===\n')
        b.append(f'\n')
    return ''.join(b)
    
def LocateFiles(fm):
    b = ['\n']
    b.append(f'==== Files\n\n')
    b.append(f'\n')
    b.append('[cols="1,1,1,1,1"]\n')
    b.append('|===\n')
    b.append(f'|Id\n')
    b.append(f'|Dir\n')
    b.append(f'|Key\n')
    b.append(f'|Reg\n')
    b.append(f'|Path\n')
    for s in fm.applications:
        b.append('\n')
        b.append(f'|{s.id}\n')
        b.append(f'|{s.dir}\n')
        b.append(f'|{s.key}\n')
        b.append(f'|{s.reg}\n')
        b.append(f'|{s.path}\n')
    b.append('|===\n')
    b.append(f'\n')
    return ''.join(b)

for f in getFamilies('../../'):
    print(f.id)
    body = GameFamily(f)
    #print(body)
    # writeFile(f, f'book/02-game-families/{f.id}.asc', '==== Family Info\n', body)
    # if f.fileManager != None:
    #     body = LocateFiles(f.fileManager)
    #     writeFile(f, f'book/03-locate-files/{f.id}.asc', '=== Table\n', body)