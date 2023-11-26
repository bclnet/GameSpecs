import FamilyManager

# get Arkane family
family = FamilyManager.getFamily('Arkane')
print(family.studio)

# get pak with resource
res = family.parseResource('file:/path/#AF')
pakFile1 = family.openPakFile(res)

# get pak with game:/uri
pakFile2 = family.openPakFile('file:/path/#AF')

# get pak with game:/uri
pakFile = family.openPakFile('game:/#AF')
print(pakFile)