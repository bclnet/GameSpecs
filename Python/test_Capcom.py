from gamespecs import familymgr

# get family
family = familymgr.getFamily('Capcom')
print(f'studio: {family.studio}')

# file = ('game:/re_chunk_000.pak#Arcade', 'File0001.tex')
file = ('game:/arc/pc/game.arc#Fighting:C', 'common/pause_blur.sdl')

# get pak with game:/uri
pakFile = family.openPakFile(file[0])
print(f'pak: {pakFile}')

# get file
data = pakFile.loadFileData(file[1])
print(f'dat: {data}')