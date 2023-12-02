import sys; sys.path.append('../../04-decode-archives/python')
import FamilyManager

# get Black family
family = FamilyManager.getFamily('Arkane')
print(f'studio: {family.studio}')

# get pak with game:/uri
pakFile = family.openPakFile('game:/#AF')
print(f'{pakFile}')
