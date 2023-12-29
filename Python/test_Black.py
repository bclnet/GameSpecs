from gamespecs import familymgr

# get family
family = familymgr.getFamily('Black')
print(f'studio: {family.studio}')

file = ('game:/MASTER.DAT#Fallout', 'COLOR.PAL')
# file = ('game:/master.dat#Fallout2', 'art/backgrnd/BACK1.FRM')

# get pak with game:/uri
pakFile = family.openPakFile(file[0])
print(f'pak: {pakFile}')

# get file
data = pakFile.loadFileData(file[1])
print(f'dat: {data}')