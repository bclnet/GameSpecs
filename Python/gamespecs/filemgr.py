import os, platform, psutil, winreg
from . import storemgr
from .util import _list

GAMESPATH = 'Games'

class FileManager:
    ApplicationPath = os.getcwd()
    # get locale games
    gameRoots = [os.path.join(x.mountpoint, GAMESPATH) for x in psutil.disk_partitions()]
    if platform.system() == 'Android': gameRoots.append(os.path.join('/sdcard', GAMESPATH))
    games = {x:os.path.join(r,x) for r in gameRoots if os.path.isdir(r) for x in os.listdir(r)}
    def __init__(self, elem):
        self.filters = {}
        self.paths = {}
        self.ignores = {}
        # applications
        if 'application' in elem:
            for k,v in elem['application'].items():
                if not k in self.paths: self.addApplication(k, v)
        # direct
        if 'direct' in elem:
            for k,v in elem['direct'].items():
                if 'path' in v:
                    for key in _list(v, 'path'):
                        self.addPath(k, elem, key, False)
        # ignores
        if 'ignores' in elem:
            for k,v in elem['ignores'].items():
                self.addIgnore(k, _list(v, 'path'))
        # filters
        if 'filters' in elem:
            for (id, val) in elem['filters'].items():
                self.addFilter(id, val)
    def __repr__(self): return f'''
- paths: {list(self.paths.keys()) if self.paths else None}
- ignores: {list(self.ignores.keys()) if self.ignores else None}
- filters: {list(self.filters.keys()) if self.filters else None}'''

    def merge(self, source) -> None:
        self.paths.update(source.paths)
        self.ignores.update(source.ignores)
        self.filters.update(source.filters)

    def addApplication(self, id, elem):
        system = platform.system()
        if system == 'Windows' and 'reg' in elem:
            for key in _list(elem, 'reg'):
                if not id in self.paths and (z := self.getPathByRegistryKey(key, elem)): self.addPath(id, elem, z)
        if 'key' in elem:
            for key in _list(elem, 'key'):
                if not id in self.paths and (z := storemgr.getPathByKey(key)): self.addPath(id, elem, z)
        if 'dir' in elem:
            for key in _list(elem, 'dir'):
                if not id in self.paths and key in FileManager.games: self.addPath(id, elem, games[key])
    
    def addFilter(self, id, elem):
        if not id in self.filters: self.filters[id] = []
        self.filters[id].append(elem)

    def addIgnore(self, id, paths):
        if not id in self.ignores: self.ignores[id] = set()
        for v in paths: self.ignores[id].add(v)

    def addPath(self, id, elem, path, usePath = True):
        if path is None or not os.path.isdir(path := FileManager.getPathWithSpecialFolders(path, '')): return
        paths = _list(elem, 'path') if usePath and 'path' in elem else [path]
        for p in [os.path.join(path, x) for x in paths]:
            if not os.path.isdir(p): continue
            if not id in self.paths: self.paths[id] = []
            self.paths[id].append(p)

    @staticmethod
    def getPathWithSpecialFolders(path, rootPath):
        return f'{rootPath}{path[6:]}' if path.startswith('%Path%') else \
        f'{FileManager.ApplicationPath}{path[9:]}' if path.startswith('%AppPath%') else \
        f'{os.getenv("APPDATA")}{path[9:]}' if path.startswith('%AppData%') else \
        f'{os.getenv("LOCALAPPDATA")}{path[14:]}' if path.startswith('%LocalAppData%') else \
        path

    @staticmethod
    def findRegistryPath(paths):
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
    def getPathByRegistryKey(key, elem):
        path = FileManager.findRegistryPath([f'Wow6432Node\\{key}', key])
        if elem is None: return path
        #if 'path' in elem: elem['path'] { path = Path.GetFullPath(GetPathWithSpecialFolders(path2.GetString(), path)); return !string.IsNullOrEmpty(path); }
        # else if (keyElem.Value.TryGetProperty("xml", out var xml)
        #     && keyElem.Value.TryGetProperty("xmlPath", out var xmlPath)
        #     && TryGetSingleFileValue(GetPathWithSpecialFolders(xml.GetString(), path), "xml", xmlPath.GetString(), out path))
        #     return !string.IsNullOrEmpty(path)
        return path

