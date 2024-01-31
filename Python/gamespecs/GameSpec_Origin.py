import os
from typing import Callable
from gamespecs.pak import BinaryPakFile
from .Origin.UO.binary import Binary_Animdata, Binary_AsciiFont, Binary_BodyConverter
from .Origin.pakbinary_u8 import PakBinary_U8
from .Origin.pakbinary_uo import PakBinary_UO
from .Origin.pakbinary_u9 import PakBinary_U9
from .util import _pathExtension

# typedefs
class FamilyGame: pass
class PakBinary: pass
class PakState: pass
class FileSource: pass
class FileOption: pass

# OriginPakFile
class OriginPakFile(BinaryPakFile):
    def __init__(self, state: PakState):
        super().__init__(state, self.getPakBinary(state.game))
        self.objectFactoryFactoryMethod = self.objectFactoryFactory

    #region Factories

    @staticmethod
    def getPakBinary(game: FamilyGame) -> PakBinary:
        match game.id:
            case 'U8': return PakBinary_U8()
            case 'UO': return PakBinary_UO()
            case 'U9': return PakBinary_U9()
            case _: raise Exception(f'Unknown: {game.id}')
        
    @staticmethod
    def objectFactoryFactory(source: FileSource, game: FamilyGame) -> (FileOption, Callable):
        match game.id:
            case 'UO':
                match source.path.lower():
                    case 'animdata.mul': return (0, Binary_Animdata.factory)
                    case 'fonts.mul': return (0, Binary_AsciiFont.factory)
                    case 'bodyconv.def': return (0, Binary_BodyConverter.factory)
                    case 'body.def': return (0, Binary_BodyTable.factory)
                    case 'calibration.cfg': return (0, Binary_CalibrationInfo.factory)
                    case 'gump.def': return (0, Binary_GumpDef.factory)
                    case 'hues.mul': return (0, Binary_Hues.factory)
                    case 'mobtypes.txt': return (0, Binary_MobType.factory)
                    case x if x == 'multimap.rle' or x.startswith('facet') == 'facet': return (0, Binary_MultiMap.factory)
                    case 'music/digital/config.txt': return (0, Binary_MusicDef.factory)
                    case 'radarcol.mul': return (0, Binary_RadarColor.factory)
                    case 'skillgrp.mul': return (0, Binary_SkillGroups.factory)
                    case 'speech.mul': return (0, Binary_SpeechList.factory)
                    case 'tiledata.mul': return (0, Binary_TileData.factory)
                    case x if x.startswith('cliloc'): return (0, Binary_StringTable.factory)
                    case 'verdata.mul': return (0, Binary_Verdata.factory)
                    # server
                    case 'data/containers.cfg': return (0, ServerBinary_Container.factory)
                    case 'data/bodytable.cfg': return (0, ServerBinary_BodyTable.factory)
                    case _:
                        match _pathExtension(source.path).lower():
                            case '.anim': return (0, Binary_Anim.factory)
                            case '.tex': return (0, Binary_Gump.factory)
                            case '.land': return (0, Binary_Land.factory)
                            case '.light': return (0, Binary_Light.factory)
                            case '.art': return (0, Binary_Static.factory)
                            case '.multi': return (0, Binary_Multi.factory)
                            case _: (0, None)
            case _: raise Exception(f'Unknown: {game.engine}')

    #endregion