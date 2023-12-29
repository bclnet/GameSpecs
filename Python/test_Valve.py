from gamespecs import familymgr

# get family
family = familymgr.getFamily('Valve')
print(f'studio: {family.studio}')

file = ('game:/#HL', 'COLOR.PAL')

# get pak with game:/uri
pakFile = family.openPakFile(file[0])
print(f'pak: {pakFile}')

# get file
data = pakFile.loadFileData(file[1])
print(f'dat: {data}')