import os, re, pathlib
from typing import Any

class MetadataManager(object):
    def __init__(self, folderIcon: Any=None, packageIcon: Any=None):
        self.folderIcon = folderIcon
        self.packageIcon = packageIcon
    def getIcon(self, name: str) -> Any: pass
    def getImage(self, name: str) -> Any: pass
        
class MetadataContent(object):
    def __init__(self, type: str, name: str, value: Any=None, tag: Any=None, maxWidth: int=None, maxHeight: int=None, dispose: Any=None, engineType: Any=None):
        self.type = type
        self.name = name
        self.value = value
        self.tag = tag
        self.maxWidth = maxWidth
        self.maxHeight = maxHeight
        self.dispose = dispose
        self.engineType = engineType

class MetadataInfo(object):
    def __init__(self, name: str, tag: Any=None, items: list[Any]=None, clickable: bool=False):
        self.name = name
        self.tag = tag
        self.items = items if items else []
        self.clickable = clickable

class MetadataItem(object):
    class Filter(object):
        def __init__(self, name: str, description: str=None):
            self.name = name
            self.description = description
    def __init__(self, source: Any, name: str, icon: Any=None, tag: Any=None, items: list[Any]=None, pakFile: Any=None):
        self.source = source
        self.name = name
        self.icon = icon
        self.tag = tag
        self.items = items if items else []
        self.pakFile = pakFile
    def findByPath(self, path: str, manager: MetadataManager):
        paths = re.split('\\|/|:', path, 2)
        node = next([x for x in self.items if x.name == paths[0]], None)
        if node and node.source.pak: node.source.pak.open(node.items, manager)
        return node if node or len(paths) == 1 else node.findByPath(paths[1], manager)

class StandardMetadataItem(object):
    @staticmethod
    def getPakFiles(manager: MetadataManager, pakFile: Any) -> list[MetadataItem]:
        root = []
        if not pakFile.files: return root
        currentPath = None
        currentFolder = None