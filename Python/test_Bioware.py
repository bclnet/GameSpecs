from gamespecs import familymgr

# get family
family = familymgr.getFamily('Bioware')
print(f'studio: {family.studio}')

file = ('game:/#SWTOR', 'swtor_en-us_alliance_1.tor:resources/en-us/fxe/cnv/alliance/alderaan/lokin/lokin.fxeL')

# get pak with game:/uri
pakFile = family.openPakFile(file[0])
print(f'pak: {pakFile}')

# get file
data = pakFile.loadFileData(file[1])
print(f'dat: {data}')