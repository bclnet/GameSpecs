import os
from typing import Callable
from gamex import Family, FamilyGame
from gamex.pak import BinaryPakFile
from .Base.binary import Binary_Dds
from .Bethesda.pakbinary_bsa import PakBinary_Bsa
from .Bethesda.pakbinary_ba2 import PakBinary_Ba2
from .util import _pathExtension

# typedefs
class PakBinary: pass
class PakState: pass
class FileSource: pass
class FileOption: pass

# BethesdaFamily
class BethesdaFamily(Family):
    def __init__(self, elem: dict[str, object]):
        super().__init__(elem)

# BethesdaGame
class BethesdaGame(FamilyGame):
    def __init__(self, family: Family, id: str, elem: dict[str, object], dgame: FamilyGame):
        super().__init__(family, id, elem, dgame)

# BethesdaPakFile
class BethesdaPakFile(BinaryPakFile):
    def __init__(self, state: PakState):
        super().__init__(state, self.getPakBinary(state.game, _pathExtension(state.path).lower()))
        self.objectFactoryFactoryMethod = self.objectFactoryFactory

    #region Factories
    @staticmethod
    def getPakBinary(game: FamilyGame, extension: str) -> PakBinary:
        match extension:
            case '': return PakBinary_Bsa()
            case '.bsa': return PakBinary_Bsa()
            case '.ba2': return PakBinary_Ba2()
            case _: raise Exception(f'Unknown: {extension}')

    # @staticmethod
    # def NiFactory(r: Reader, f: FileSource, s: PakFile): file = NiFile(Path.GetFileNameWithoutExtension(f.Path)); file.Read(r); return file

    @staticmethod
    def objectFactoryFactory(source: FileSource, game: FamilyGame) -> (FileOption, Callable):
        match _pathExtension(source.path).lower():
            case '.dds': return (0, Binary_Dds.factory)
            # case '.nif': return (0, NiFactory)
            case _: return (0, None)
    #endregion