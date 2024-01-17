import os
from typing import Callable
from gamespecs.pakfile import BinaryPakFile
from .Base.pakbinary_zip import PakBinary_Zip
from .Capcom.pakbinary_arc import PakBinary_Arc
from .Capcom.pakbinary_big import PakBinary_Big
from .Capcom.pakbinary_bundle import PakBinary_Bundle
from .Capcom.pakbinary_kpka import PakBinary_Kpka
from .Capcom.pakbinary_plist import PakBinary_Plist
from .Unity.pakbinary_unity import PakBinary_Unity
from .util import _pathExtension

# typedefs
class FamilyGame: pass
class PakBinary: pass
class IFileSystem: pass
class FileSource: pass
class FileOption: pass

# CapcomPakFile
class CapcomPakFile(BinaryPakFile):
    def __init__(self, game: FamilyGame, fileSystem: IFileSystem, filePath: str, tag: object = None):
        super().__init__(game, fileSystem, filePath, self.getPakBinary(game, _pathExtension(filePath).lower()), tag)

    #region Factories
    @staticmethod
    def getPakBinary(game: FamilyGame, extension: str) -> PakBinary:
        if not extension: return None
        elif extension == '.pie': return PakBinary_Zip()
        match game.engine:
            case 'Unity': return PakBinary_Unity()
        match extension:
            case '.pak': return PakBinary_Kpka()
            case '.arc': return PakBinary_Arc()
            case '.big': return PakBinary_Big()
            case '.bundle': return PakBinary_Bundle()
            case '.mbundle': return PakBinary_Plist()
            case _: raise Exception(f'Unknown: {extension}')
    #endregion
