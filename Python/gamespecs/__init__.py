from ._config import __title__, __version__, appDefaultOptions as config, familyKeys
from .familymgr import *
from .platform import Platform
from .util import _value

init()
# print(familymgr.families)

unknown = getFamily('Unknown')
unknownPakFile = unknown.openPakFile('game:/#APP', throwOnError = False)
