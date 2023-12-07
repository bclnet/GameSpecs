
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
