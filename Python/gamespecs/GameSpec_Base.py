from .familymgr import Family
from .pakfile import PakFile

class UnknownFamily(Family):
    def __init__(self, elem):
        super().__init__(elem)

class UnknownPakFile(PakFile):
    def __init__(self, game, fileSystem, filePath, tag):
        super().__init__(game, 'Unknown')