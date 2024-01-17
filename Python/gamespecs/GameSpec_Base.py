from gamespecs import Family
from gamespecs.pakfile import PakFile

# typedefs
class FamilyGame: pass
class IFileSystem: pass

# UnknownFamily
class UnknownFamily(Family):
    def __init__(self, elem: dict[str, object]):
        super().__init__(elem)

# UnknownPakFile
class UnknownPakFile(PakFile):
    def __init__(self, game: FamilyGame, fileSystem: IFileSystem, filePath: str, tag: object = None):
        super().__init__(game, 'Unknown')