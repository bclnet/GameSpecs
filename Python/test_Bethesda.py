from game_specs import familymgr

# get family
family = familymgr.getFamily('Bethesda')
print(f'studio: {family.studio}')

# get pak with game:/uri
# pakFile = family.openPakFile('game:/Morrowind.bsa#Morrowind')
# pakFile = family.openPakFile('game:/Oblivion - Meshes.bsa#Oblivion')
# pakFile = family.openPakFile('game:/Fallout - Meshes.bsa#Fallout3')
pakFile = family.openPakFile('game:/Fallout4 - Meshes.ba2#Fallout4')
print(f'pak: {pakFile}')

# get file
# data = pakFile.loadFileData('GRAPH/particles/DEFAULT.jpg')
# data = pakFile.loadFileData('strings/english_m.lang')
data = pakFile.loadFileData('Meshes/Marker_Error.NIF')
print(f'dat: {data}')