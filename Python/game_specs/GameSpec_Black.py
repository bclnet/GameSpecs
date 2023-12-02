# import sys; sys.path.append('../../02-game-families/python')
# import FamilyManager

class BlackPakFile:
    def __init__(s, game, fileSystem, filePath, tag):
        s.game = game
        s.fileSystem = fileSystem
    def __repr__(s): return f'BlackPakFile:{s.game}'
    def open(s): return f'OPEN'