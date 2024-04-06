from gamex import family

# get family
family = family.getFamily('Bethesda')
print(f'studio: {family.studio}')

file = ('game:/Morrowind.bsa#Morrowind', 'bookart/boethiah_256.dds')
# file = ('game:/Oblivion - Meshes.bsa#Oblivion', 'GRAPH/particles/DEFAULT.jpg')
# file = ('game:/Fallout - Meshes.bsa#Fallout3', 'strings/english_m.lang')
# file = ('game:/Fallout4 - Meshes.ba2#Fallout4', 'Meshes/Marker_Error.NIF')

# get pak with game:/uri
pakFile = family.openPakFile(file[0])
sample = pakFile.game.getSample(file[1][7:]).path if file[1].startswith('sample') else file[1]
print(f'pak: {pakFile}, {sample}')

# get file
data = pakFile.loadFileData(sample)
print(f'dat: {data}')