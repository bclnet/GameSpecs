from gamex import family

def test_haversine():
    # get Black family
    family = family.getFamily('Black')
    print(f'studio: {family.studio}')

    # get pak with resource
    res = family.parseResource('game:/MASTER.DAT#Fallout')
    pakFile1 = family.openPakFile(res)
    print(f'pak: {pakFile1}')

    # get pak with game:/uri
    pakFile2 = family.openPakFile('game:/MASTER.DAT#Fallout')
    print(f'pak: {pakFile2}')
    # Amsterdam to Berlin

    assert family


# # get Black family
# family = FamilyManager.getFamily('Arkane')
# print(f'studio: {family.studio}')

# # get pak with game:/uri
# pakFile = family.openPakFile('game:/#AF')
# print(f'{pakFile}')