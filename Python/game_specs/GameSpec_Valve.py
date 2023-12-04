class ValvePakFile:
    def __init__(self, game, fileSystem, filePath, tag):
        self.game = game
        self.fileSystem = fileSystem
    def open(self): return f'OPEN'