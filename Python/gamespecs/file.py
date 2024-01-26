import os, pathlib, platform, psutil, winreg
from openstk.poly import Reader, findType
from . import store
from .util import _list

GAMESPATH = 'Games'

# forwards
class FileManager: pass

# FileManager
class FileManager:
    applicationPath = os.getcwd()
    filters: dict[str, object] = {}
    paths: dict[str, object] = {}
    ignores: dict[str, object] = {}

    # get locale games
    gameRoots = [os.path.join(x.mountpoint, GAMESPATH) for x in psutil.disk_partitions()]
    if platform.system() == 'Android': gameRoots.append(os.path.join('/sdcard', GAMESPATH))
    localGames = {x:os.path.join(r,x) for r in gameRoots if os.path.isdir(r) for x in os.listdir(r)}

    def __init__(self, elem: dict[str, object]):
        # applications
        if 'application' in elem:
            for k,v in elem['application'].items():
                if not k in self.paths:
                    self.addApplication(k, v)
        # direct
        if 'direct' in elem:
            for k,v in elem['direct'].items():
                if 'path' in v:
                    for path in _list(v, 'path'):
                        self.addPath(k, elem, path, False)
        # ignores
        if 'ignores' in elem:
            for k,v in elem['ignores'].items():
                self.addIgnore(k, _list(v, 'path'))
        # filters
        if 'filters' in elem:
            for k,v in elem['filters'].items():
                self.addFilter(k, v)
    def __repr__(self): return f'''
- paths: {list(self.paths.keys()) if self.paths else None}
- ignores: {list(self.ignores.keys()) if self.ignores else None}
- filters: {list(self.filters.keys()) if self.filters else None}'''

    def merge(self, source: FileManager) -> None:
        self.paths.update(source.paths)
        self.ignores.update(source.ignores)
        self.filters.update(source.filters)

    def addApplication(self, id: str, elem: dict[str, object]) -> None:
        if platform.system() == 'Windows' and 'reg' in elem:
            for key in _list(elem, 'reg'):
                if not id in self.paths and (z := self.getPathByRegistryKey(key, elem[key] if key in elem else None)):
                    self.addPath(id, elem, z)
        if 'key' in elem:
            for key in _list(elem, 'key'):
                if not id in self.paths and (z := store.getPathByKey(key)):
                    self.addPath(id, elem, z)
        if 'dir' in elem:
            for key in _list(elem, 'dir'):
                if not id in self.paths and key in FileManager.localGames:
                    self.addPath(id, elem, FileManager.localGames[key])
    
    def addFilter(self, id, elem: dict[str, object]) -> None:
        if not id in self.filters: self.filters[id] = []
        self.filters[id].append(elem)

    def addIgnore(self, id: str, paths: list[str]) -> None:
        if not id in self.ignores: self.ignores[id] = set()
        z2 = self.ignores[id]
        for v in paths: z2.add(v)

    def addPath(self, id: str, elem: dict[str, object], path: str, usePath: bool = True) -> None:
        if path is None or not os.path.isdir(path := FileManager.getPathWithSpecialFolders(path, '')): return
        paths = _list(elem, 'path') if usePath and 'path' in elem else [path]
        for p in [os.path.join(path, x) for x in paths]:
            if not os.path.isdir(p): continue
            if not id in self.paths: self.paths[id] = []
            self.paths[id].append(p)

    @staticmethod
    def getPathWithSpecialFolders(path: str, rootPath: str = None) -> str:
        lowerPath = path.lower()
        return f'{rootPath}{path[6:]}' if lowerPath.startswith('%path%') else \
        f'{FileManager.applicationPath}{path[9:]}' if lowerPath.startswith('%apppath%') else \
        f'{os.getenv("APPDATA")}{path[9:]}' if lowerPath.startswith('%appdata%') else \
        f'{os.getenv("LOCALAPPDATA")}{path[14:]}' if lowerPath.startswith('%localappdata%') else \
        path

    @staticmethod
    def findRegistryPath(paths: list[str]) -> str:
        for p in paths:
            keyPath = p.replace('/', '\\')
            try: key = winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, f'SOFTWARE\\{keyPath}', 0, winreg.KEY_READ)
            except FileNotFoundError:
                try: key = winreg.OpenKey(winreg.HKEY_CURRENT_USER, f'SOFTWARE\\{keyPath}', 0, winreg.KEY_READ)
                except FileNotFoundError:
                    try: key = winreg.OpenKey(winreg.HKEY_CLASSES_ROOT, f'VirtualStore\\MACHINE\\SOFTWARE\\{keyPath}', 0, winreg.KEY_READ)
                    except FileNotFoundError: key = None
            if key is None: continue
            # search directories
            path = None
            for search in ['Path', 'Install Dir', 'InstallDir', 'InstallLocation']:
                try:
                    val = winreg.QueryValueEx(key, search)[0]
                    if os.path.isdir(val): path = val; break
                except FileNotFoundError: continue
            # search files
            if path is None:
                for search in ['Installed Path', 'ExePath', 'Exe']:
                    try:
                        val = winreg.QueryValueEx(key, search)[0]
                        if os.path.exists(val): path = val; break
                    except FileNotFoundError: continue
                if path is not None: path = os.path.dirname(path)
            if path is not None and os.path.isdir(path): return path
        return None

    @staticmethod
    def getPathByRegistryKey(key: str, elem: dict[str, object]) -> str:
        path = FileManager.findRegistryPath([f'Wow6432Node\\{key}', key])
        if not elem: return path
        elif 'path' in elem: return os.path.abspath(FileManager.getPathWithSpecialFolders(elem['path'], path))
        elif 'xml' in elem and 'xmlPath' in elem:
            return FileManager.getSingleFileValue(FileManager.getPathWithSpecialFolders(elem['xml'], path), 'xml', elem['xmlPath'])
        return None

    @staticmethod
    def getSingleFileValue(path: str, ext: str, select: str) -> str:
        if not os.fileExists(path): return None
        with open(path, 'r') as f: content = f.read()
        match ext:
            case 'xml': raise NotImplementedError() #return XDocument.Parse(content).XPathSelectElement(select)?.Value,
            case _: raise Exception(f'Unknown {ext}')
        return os.path.basename(value) if value else None
    
# IFileSystem
class IFileSystem:
    def findPaths(self, path: str, searchPattern: str) -> str:
        if (expandStartIdx := searchPattern.find('(')) != -1 and \
            (expandMidIdx := searchPattern.find(':', expandStartIdx)) != -1 and \
            (expandEndIdx := searchPattern.find(')', expandMidIdx)) != -1 and \
            expandStartIdx < expandEndIdx:
            for expand in searchPattern[expandStartIdx + 1: expandEndIdx].split(':'):
                for found in self.findPaths(path, searchPattern[:expandStartIdx] + expand + searchPattern[expandEndIdx+1:]): yield found
            return
        for path in self.glob(path, searchPattern): yield path

# StandardFileSystem
class StandardFileSystem(IFileSystem):
    def __init__(self, root: str): self.root = root; self.skip = len(root) + 1
    def glob(self, path: str, searchPattern: str) -> list[str]:
        g = pathlib.Path(os.path.join(self.root, path)).glob(searchPattern)
        return [str(x)[self.skip:] for x in g]
    def fileExists(self, path: str) -> bool: return os.path.exists(os.path.join(self.root, path))
    def fileInfo(self, path: str) -> object: return os.stat(os.path.join(self.root, path))
    def openReader(self, path: str, mode: str = 'rb') -> Reader: return Reader(open(os.path.join(self.root, path), mode))

# HostFileSystem
class HostFileSystem(IFileSystem):
    def __init__(self, uri: str): self.uri = uri
    def glob(self, path: str, searchPattern: str) -> list[str]: raise NotImplementedError()
    def fileExists(self, path: str) -> bool: raise NotImplementedError()
    def fileInfo(self, path: str) -> object: raise NotImplementedError()
    def openReader(self, path: str, mode: str = 'rb') -> Reader: raise NotImplementedError()
