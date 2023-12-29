from gamespecs import familymgr

# get family
family = familymgr.getFamily('Arkane')
print(f'studio: {family.studio}')

file = ('game:/data.pak#AF', 'GRAPH/particles/DEFAULT.jpg')
# file = ('game:/master.index#D2', 'strings/english_m.lang')

# get pak with game:/uri
pakFile = family.openPakFile(file[0])
print(f'pak: {pakFile}')

# get file
data = pakFile.loadFileData(file[1])
print(f'dat: {data}')