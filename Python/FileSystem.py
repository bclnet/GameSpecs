import os, platform, pathlib

class StandardFileSystem:
    def __init__(s, root): s.root = root; s.skip = len(root) + 1
    def glob(s, path, searchPattern):
        g = pathlib.Path(os.path.join(s.root, path)).glob(searchPattern)
        return [str(x)[s.skip:] for x in g]

class HostFileSystem:
    def __init__(s, uri):
        s.uri = uri
    def glob(s, path, searchPattern):
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



# def getDirectories(s, path, searchPattern, recursive):
#     print('HERE1')
#     exit(1)
#     p = pathlib.Path(os.path.join(s.root, path))
#     g = p.rglob(searchPattern) if recursive else p.glob(searchPattern)
#     print(list(g))
#     exit(1)
#     return [x[s.skip:] for x in g if x.is_dir()]
# def getFiles(s, path, searchPattern):
#     p = pathlib.Path(os.path.join(s.root, path))
#     g = p.glob(searchPattern)
#     print([x for x in g])
#     exit(1)
#     return [x[s.skip:] for x in g if not x.is_dir()]
# def fileExists(s, path):
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
