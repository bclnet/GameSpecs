from gamespecs import familymgr

# get family
family = familymgr.getFamily('Valve')
print(f'studio: {family.studio}')

# get pak with game:/uri
pakFile = family.openPakFile('game:/#HL')
print(f'pak: {pakFile}')

# get file
# data = pakFile.loadFileData('COLOR.PAL')
# print(f'dat: {data}')