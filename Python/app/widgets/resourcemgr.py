import sys, os, glob
from PyQt6.QtGui import QIcon
from gamex.meta import MetaManager

# ResourceManager
class ResourceManager(MetaManager):
    def __init__(self):
        self._icons = {
            path.replace('\\', '.').replace('/', '.').rsplit('.', 2)[1]:QIcon(path) \
            for path in glob.glob('resources/icons/*.png')
            }
        self._defaultIcon = self._icons['_default']
        self.folderIcon = self._icons['_folder']
        self.packageIcon = self._icons['_package']

    def getIcon(self, name):
        return self._icons[name] if name in self._icons else self._defaultIcon