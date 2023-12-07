from game_specs import familymgr

# get family
family = familymgr.getFamily('Arkane')
print(f'studio: {family.studio}')

# get pak with game:/uri
pakFile = family.openPakFile('game:/#AF')
print(f'pak: {pakFile}')

