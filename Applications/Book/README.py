import sys, os, re, qrcode
sys.path.append('../..')
import base

def writeFile(z, path, marker, body):
    with open(path, 'r', encoding='utf-8') as f: text = f.read()
    head, sep, tail = text.partition(marker)
    text = head + sep + body
    with open(path, 'w', encoding='utf-8') as f: f.write(text)

def getUrl(url):
    if url == '': return ''
    file = f'{url.replace(':', '').replace('/', '_').replace('.', '').replace('&', '+').replace("'", '+')}.png'
    path = os.path.join('.', 'qrcodes', file)
    if not os.path.exists(path):
        img = qrcode.make(url, box_size=5)
        img.save(path)
    return f'image:qrcodes/{file}[width=100,height=100]'

def GameFamily(f):
    b = ['\n']
    b.append('[cols="1a"]\n')
    b.append('|===\n')
    b.append(f'|{f.id}\n')
    b.append(f'|name: {f.name}\n')
    b.append(f'|studio: {f.studio}\n')
    b.append(f'|description: {f.description}\n')
    b.append(f'|{'\n\n'.join([getUrl(x) for x in f.urls]) if f.urls else None}\n')
    b.append('|===\n')
    b.append(f'\n')
    if f.engines:
        b.append(f'==== Engines\n')
        b.append(f'\n')
        b.append('[cols="1,1"]\n')
        b.append('|===\n')
        b.append(f'|Id |Name\n')
        for s in f.engines.values():
            b.append('\n')
            b.append(f'|{s.id}\n')
            b.append(f'|{s.name}\n')
        b.append('|===\n')
        b.append(f'\n')
    if f.games:
        b.append(f'==== Games\n')
        b.append(f'\n')
        b.append('[cols="1,1,1,1,1,1a"]\n')
        b.append('|===\n')
        b.append(f'|Id |Name |Engine |Date |Extension(s) |Url\n')
        for s in f.games.values():
            multi = s.key
            b.append('\n')
            if multi: b.append('.2+')
            b.append(f'|{s.id}\n')
            b.append(f'|{s.name}\n')
            b.append(f'|{s.engine}\n')
            b.append(f'|{s.date}\n')
            b.append(f'|{', '.join(s.pakExts) if s.pakExts else None}\n')
            b.append(f'|{'\n\n'.join([getUrl(x) for x in s.urls]) if s.urls else None}\n')
            if multi:
                b.append('\n')
                b.append(f'5+|Key:\n{s.key}\n')
        b.append('|===\n')
        b.append(f'\n')
    return ''.join(b)
    
def LocateFiles(fm):
    b = ['\n']
    b.append('[cols="1,1,1,1"]\n')
    b.append('|===\n')
    b.append(f'|Id |Dir |Key |Path\n')
    for s in fm.applications.values():
        b.append('\n')
        b.append(f'.2+|{s.id}\n')
        b.append(f'|{', '.join(s.dir)}\n')
        b.append(f'|{', '.join(s.key)}\n')
        b.append(f'|{', '.join(s.path)}\n')
        b.append('\n')
        b.append(f'3+|{', '.join(s.reg)}\n')
    b.append('|===\n')
    b.append(f'\n')
    return ''.join(b)

for f in base.init('../../').values():
    print(f.id)
    body = GameFamily(f)
    writeFile(f, f'book/02-game-families/{f.id}.asc', '==== Family Info\n', body)
    # if f.fileManager != None:
    #     body = LocateFiles(f.fileManager)
    #     writeFile(f, f'book/03-locate-files/{f.id}.asc', '==== File Info\n', body)