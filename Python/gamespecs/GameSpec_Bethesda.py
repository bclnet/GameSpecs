import os
from .familymgr import Family, FamilyGame
from .pakfile import FileSource, BinaryPakFile
from .Base.binary_dds import Binary_Dds
from .Bethesda.pakbinary_bsa import PakBinary_Bsa
from .Bethesda.pakbinary_ba2 import PakBinary_Ba2
from .util import _pathExtension

class BethesdaFamily(Family):
    def __init__(self, elem):
        super().__init__(elem)

class BethesdaGame(FamilyGame):
    def __init__(self, family, id, elem, dgame):
        super().__init__(family, id, elem, dgame)

class BethesdaPakFile(BinaryPakFile):
    def __init__(self, game, fileSystem, filePath, tag):
        super().__init__(game, fileSystem, filePath, self.getPakBinary(game, _pathExtension(filePath).lower()), tag)
        self.objectFactoryFactoryMethod = self.objectFactoryFactory

    #region Factories
    @staticmethod
    def getPakBinary(game: FamilyGame, extension: str) -> object:
        match extension:
            case '': return PakBinary_Bsa()
            case '.bsa': return PakBinary_Bsa()
            case '.ba2': return PakBinary_Ba2()
            case _: raise Exception(f'Unknown: {extension}')

    # @staticmethod
    # def NiFactory(r: Reader, f: FileSource, s: PakFile): file = NiFile(Path.GetFileNameWithoutExtension(f.Path)); file.Read(r); return file

    @staticmethod
    def objectFactoryFactory(source: FileSource, game: FamilyGame) -> (int, object):
        match _pathExtension(source.path).lower():
            case '.dds': return (0, Binary_Dds.factory)
            # case '.nif': return (0, NiFactory)
            case _: return (0, None)
    #endregion