# import sys; sys.path.append('../../04-decode-archives/python')
from game_specs import familymgr

# get Black family
family = familymgr.getFamily('Black')
print(f'studio: {family.studio}')

# get pak with game:/uri
# pakFile = family.openPakFile('game:/MASTER.DAT#Fallout')
# pakFile = family.openPakFile('game:/master.dat#Fallout2')
print(f'pak: {pakFile}')
