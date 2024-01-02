import os, re, pathlib
from typing import Any

class MetaManager: pass
class MetaItem: pass
        
class MetaContent:
    def __init__(self, type: str, name: str, value: Any=None, tag: Any=None, maxWidth: int=None, maxHeight: int=None, dispose: Any=None, engineType: Any=None):
        self.type = type
        self.name = name
        self.value = value
        self.tag = tag
        self.maxWidth = maxWidth
        self.maxHeight = maxHeight
        self.dispose = dispose
        self.engineType = engineType

class MetaInfo:
    def __init__(self, name: str, tag: Any=None, items: list[Any]=None, clickable: bool=False):
        self.name = name
        self.tag = tag
        self.items = items if items else []
        self.clickable = clickable

class MetaItem:
    class Filter:
        def __init__(self, name: str, description: str=None):
            self.name = name
            self.description = description
    def __init__(self, source: Any, name: str, icon: Any=None, tag: Any=None, pakFile: Any=None, items: list[Any]=None):
        self.source = source
        self.name = name
        self.icon = icon
        self.tag = tag
        self.pakFile = pakFile
        self.items = items if items else []
    def findByPath(self, path: str, manager: MetaManager) -> MetaItem:
        paths = re.split('\\\\|/|:', path, 1)
        node = next(iter([x for x in self.items if x.name == paths[0]]), None)
        if node and node.source.pak: node.source.pak.open(node.items, manager)
        return node if not node or len(paths) == 1 else node.findByPath(paths[1], manager)
    @staticmethod
    def findByPathForNodes(nodes: list[MetaItem], path: str, manager: MetaManager) -> MetaItem:
        paths = re.split('\\\\|/|:', path, 1)
        node = next(iter([x for x in nodes if x.name == paths[0]]), None)
        if node and node.source.pak: node.source.pak.open(node.items, manager)
        return node if not node or len(paths) == 1 else node.findByPath(paths[1], manager)

class MetaManager:
    def __init__(self, folderIcon: Any=None, packageIcon: Any=None):
        self.folderIcon = folderIcon
        self.packageIcon = packageIcon
    def getIcon(self, name: str) -> Any: pass
    def getImage(self, name: str) -> Any: pass

    @staticmethod
    def guessStringOrBytes(pakFile: Any, stream: object) -> Any:
        return None

    @staticmethod
    def getMetaInfos(manager: MetaManager, pakFile: Any, file: Any) -> list[MetaInfo]:
        nodes = []
        obj = pakFile.loadFileObject(object, file)
        print('a:', obj)
        # nodes.append(MetaInfo(None, MetaContent(type='Hex',name='TEST',value=BytesIO())))
        return nodes

    @staticmethod
    def getMetaItems(manager: MetaManager, pakFile: Any) -> list[MetaItem]:
        root = []
        if not pakFile.files: return root
        currentPath = None; currentFolder = None

        # parse paths
        for file in sorted(pakFile.files, key=lambda x:x.path):
            # next path, skip empty
            path = file.path[pakFile.pathSkip:]
            if not path: continue
            # folder
            fileFolder = os.path.dirname(path)
            if currentPath != fileFolder:
                currentPath = fileFolder
                currentFolder = root
                if fileFolder:
                    for folder in fileFolder.split('/'):
                        found = next(iter([x for x in currentFolder if x.name == folder and not x.pakFile]), None)
                        if found: currentFolder = found.items
                        else:
                            found = MetaItem(file, folder, manager.folderIcon)
                            currentFolder.append(found)
                            currentFolder = found.items
            # pakfile
            if file.pak:
                items = MetaManager.getPakFiles(manager, file.pak)
                currentFolder.append(MetaItem(file, os.path.basename(file.path), manager.packageIcon, pakFile=pakFile, items=items))
                continue
            # file
            fileName = os.path.basename(path)
            fileNameForIcon = pakFile.fileMask(fileName) or fileName if pakFile.fileMask else fileName
            _, extentionForIcon = os.path.splitext(fileNameForIcon)
            if extentionForIcon: extentionForIcon = extentionForIcon[1:]
            currentFolder.append(MetaItem(file, fileName, manager.getIcon(extentionForIcon), pakFile=pakFile))
        return root