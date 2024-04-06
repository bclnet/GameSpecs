from gamex import family

# get family
family = family.getFamily('Arkane')
print(f'studio: {family.studio}')

file = ('game:/#AF', 'sample:0')
# file = ('game:/master.index#D2', 'strings/english_m.lang')

# get pak with game:/uri
pakFile = family.openPakFile(file[0])
sample = pakFile.game.getSample(file[1][7:]).path if file[1].startswith('sample') else file[1]
print(f'pak: {pakFile}, {sample}')

# get file
data = pakFile.loadFileData(sample)
print(f'dat: {data}')