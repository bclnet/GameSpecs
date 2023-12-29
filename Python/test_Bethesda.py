from gamespecs import familymgr

# get family
family = familymgr.getFamily('Bethesda')
print(f'studio: {family.studio}')

file = ('game:/Morrowind.bsa#Morrowind', 'bookart/boethiah_256.dds')
# file = ('game:/Oblivion - Meshes.bsa#Oblivion', 'GRAPH/particles/DEFAULT.jpg')
# file = ('game:/Fallout - Meshes.bsa#Fallout3', 'strings/english_m.lang')
# file = ('game:/Fallout4 - Meshes.ba2#Fallout4', 'Meshes/Marker_Error.NIF')

# get pak with game:/uri
pakFile = family.openPakFile(file[0])
print(f'pak: {pakFile}')

# get file
data = pakFile.loadFileData(file[1])
print(f'dat: {data}')