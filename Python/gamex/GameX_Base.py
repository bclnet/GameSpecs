from gamex import Family
from gamex.pak import PakFile

# typedefs
class FamilyGame: pass
class PakState: pass

# UnknownFamily
class UnknownFamily(Family):
    def __init__(self, elem: dict[str, object]):
        super().__init__(elem)

# UnknownPakFile
class UnknownPakFile(PakFile):
    def __init__(self, state: PakState):
        super().__init__(state)
        self.name = 'Unknown'