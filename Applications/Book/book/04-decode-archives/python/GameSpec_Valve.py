# import sys; sys.path.append('../../02-game-families/python')
# import FamilyManager

class ValvePakFile:
    def __init__(s, game, fileSystem, filePath, tag):
        s.game = game
        s.fileSystem = fileSystem
    def open(s): return f'OPEN'