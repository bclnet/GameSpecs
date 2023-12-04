import os, platform, pathlib

class StandardFileSystem:
    def __init__(self, root): self.root = root; self.skip = len(root) + 1
    def glob(self, path, searchPattern):
        g = pathlib.Path(os.path.join(self.root, path)).glob(searchPattern)
        return [str(x)[self.skip:] for x in g]
    def open(self, path, mode):
        return open(os.path.join(self.root, path), mode) 

class HostFileSystem:
    def __init__(self, uri):
        self.uri = uri
    def glob(self, path, searchPattern):
        raise Exception('Not Implemented')
    def open(self, path, mode):
        raise Exception('Not Implemented')

@staticmethod
def findPaths(fileSystem, path, searchPattern):
    if (expandStartIdx := searchPattern.find('(')) != -1 and \
        (expandMidIdx := searchPattern.find(':', expandStartIdx)) != -1 and \
        (expandEndIdx := searchPattern.find(')', expandMidIdx)) != -1 and \
        expandStartIdx < expandEndIdx:
        for expand in searchPattern[expandStartIdx + 1: expandEndIdx].split(':'):
            for found in findPaths(fileSystem, path, searchPattern[:expandStartIdx] + expand + searchPattern[expandEndIdx+1:]): yield found
        return
    for path in fileSystem.glob(path, searchPattern): yield path



# def getDirectories(self, path, searchPattern, recursive):
#     print('HERE1')
#     exit(1)
#     p = pathlib.Path(os.path.join(self.root, path))
#     g = p.rglob(searchPattern) if recursive else p.glob(searchPattern)
#     print(list(g))
#     exit(1)
#     return [x[self.skip:] for x in g if x.is_dir()]
# def getFiles(self, path, searchPattern):
#     p = pathlib.Path(os.path.join(self.root, path))
#     g = p.glob(searchPattern)
#     print([x for x in g])
#     exit(1)
#     return [x[self.skip:] for x in g if not x.is_dir()]
# def fileExists(self, path):
# # folder
# directoryPattern = os.path.dirname(searchPattern)
# if '*' in directoryPattern:
#     for directory in fileSystem.getDirectories(path, directoryPattern, '**' in directoryPattern):
#         for found in fileSystem.findPaths(directory, os.path.filename(directoryPattern)):
#             yield found
#     searchPattern = os.path.filename(searchPattern)
# # file
# if '*' not in searchPattern: yield fileSystem.getFile(os.path.join(path, searchPattern))
# else:
#     for file in fileSystem.getFiles(path, searchPattern): yield file
