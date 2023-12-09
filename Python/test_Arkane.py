from game_specs import familymgr

# get family
family = familymgr.getFamily('Arkane')
print(f'studio: {family.studio}')

# get pak with game:/uri
# pakFile = family.openPakFile('game:/data.pak#AF')
pakFile = family.openPakFile('game:/data.pak#AF')
print(f'pak: {pakFile}')

# get file
# data = pakFile.loadFileData('GRAPH/particles/DEFAULT.jpg')
data = pakFile.loadFileData('GRAPH/particles/DEFAULT.jpg')
print(f'dat: {data}')