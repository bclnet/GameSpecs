from gamex import family

# get family
family = family.getFamily('Capcom')
print(f'studio: {family.studio}')

# file = ('game:/re_chunk_000.pak#Arcade', 'File0001.tex')
file = ('game:/arc/pc/game.arc#Fighting:C', 'common/pause_blur.sdl')

# get pak with game:/uri
pakFile = family.openPakFile(file[0])
sample = pakFile.game.getSample(file[1][7:]).path if file[1].startswith('sample') else file[1]
print(f'pak: {pakFile}, {sample}')

# get file
data = pakFile.loadFileData(sample)
print(f'dat: {data}')