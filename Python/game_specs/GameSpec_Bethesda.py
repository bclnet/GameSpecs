class BethesdaPakFile:
    def __init__(self, game, fileSystem, filePath, tag):
        self.game = game
        self.fileSystem = fileSystem
    def __repr__(self): return f'BethesdaPakFile:{self.game}'
    def open(self): return f'OPEN'