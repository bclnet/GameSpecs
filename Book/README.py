import sys, os, re, qrcode
sys.path.append('..')
import base

def writeFile(z, path, marker, body):
    with open(path, 'r', encoding='utf-8') as f: text = f.read()
    head, sep, tail = text.partition(marker)
    text = head + sep + body
    with open(path, 'w', encoding='utf-8') as f: f.write(text)

def getUrl(url):
    if url == '': return ''
    file = f'{url.replace(':', '').replace('_', '').replace('/', '_').replace('.', '').replace('&', '+').replace("'", '+')}.png'
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
        b.append(f'==== List of Engines\n')
        b.append(f'\n')
        b.append('[cols="1,1"]\n')
        b.append('|===\n')
        b.append(f'|ID |Name\n')
        for s in f.engines.values():
            b.append('\n')
            b.append(f'|{s.id}\n')
            b.append(f'|{s.name}\n')
        b.append('|===\n')
        b.append(f'\n')
    if f.games:
        b.append(f'==== List of Games\n')
        b.append(f'\n')
        b.append('[cols="1,1,1,1,1,1a"]\n')
        b.append('|===\n')
        b.append(f'|ID |Name |Engine |Date |Extension(s) |Url\n')
        for s in f.games.values():
            s_fa = f.fileManager.applications[s.id] if f.fileManager and s.id in f.fileManager.applications else None
            s_fi = f.fileManager.ignores[s.id] if f.fileManager and s.id in f.fileManager.ignores else None
            s_ff = f.fileManager.filters[s.id] if f.fileManager and s.id in f.fileManager.filters else None
            multi = s_fa or s.key or s.editions or s.dlcs or s.locales
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
                b.append(f'5+a|\n')
                # s.key
                if s.key: b.append(f'{s.key}\n')
                # editions
                if s.editions:
                    b.append(f'Editions:\n')
                    b.append('[cols="1,1"]\n')
                    b.append('!===\n')
                    b.append(f'!ID !Name\n')
                    for t in s.editions.values():
                        b.append('\n')
                        b.append(f'!{t.id}\n')
                        b.append(f'!{t.name}\n')
                    b.append('!===\n')
                    b.append(f'\n')
                # dlcs
                if s.dlcs:
                    b.append(f'DLCs:\n')
                    b.append('[cols="1,1,1"]\n')
                    b.append('!===\n')
                    b.append(f'!ID !Name !Path\n')
                    for t in s.dlcs.values():
                        b.append('\n')
                        b.append(f'!{t.id}\n')
                        b.append(f'!{t.name}\n')
                        b.append(f'!{t.path}\n')
                    b.append('!===\n')
                    b.append(f'\n')
                # locales
                if s.locales:
                    b.append(f'Locales:\n')
                    b.append('[cols="1,1"]\n')
                    b.append('!===\n')
                    b.append(f'!ID !Name\n')
                    for t in s.locales.values():
                        b.append('\n')
                        b.append(f'!{t.id}\n')
                        b.append(f'!{t.name}\n')
                    b.append('!===\n')
                    b.append(f'\n')
                # fileManager.Application
                if s_fa:
                    b.append('[cols="1,1,1"]\n')
                    b.append('!===\n')
                    b.append(f'!Dir !Key !Path\n')
                    if s_fa:
                        b.append('\n')
                        b.append(f'!{', '.join(s_fa.dir) if s_fa.dir else None}\n')
                        b.append(f'!{', '.join(s_fa.key) if s_fa.key else None}\n')
                        b.append(f'!{', '.join(s_fa.path) if s_fa.path else None}\n')
                        b.append('\n')
                        b.append(f'3+!{', '.join(s_fa.reg) if s_fa.reg else None}\n')
                    b.append('!===\n')
                    b.append(f'\n')
        b.append('|===\n')
        b.append(f'\n')
    return ''.join(b)

for f in base.init('../python').values():
    print(f.id)
    body = GameFamily(f)
    # print(body)
    writeFile(f, f'book/A-families/{f.id}.asc', '==== Family Info\n', body)