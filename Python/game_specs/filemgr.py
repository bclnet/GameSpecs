import os, platform, psutil, winreg
from . import storemgr
GAMESPATH = 'Games'

def _list(d, key, default = None): return (d[key] if isinstance(d[key], list) else [d[key]]) if key in d else default

class FileManager:
    ApplicationPath = os.getcwd()
    # get locale games
    gameRoots = [os.path.join(x.mountpoint, GAMESPATH) for x in psutil.disk_partitions()]
    if platform.system() == 'Android': gameRoots.append(os.path.join('/sdcard', GAMESPATH))
    games = {x:os.path.join(r,x) for r in gameRoots if os.path.isdir(r) for x in os.listdir(r)}
    
    def addApplication(self, id, d):
        system = platform.system()
        if system == 'Windows' and 'reg' in d:
            for key in _list(d, 'reg'):
                if not id in self.paths and (z := self.getPathByRegistryKey(key, d)): self.addPath(id, d, z)
        if 'key' in d:
            for key in _list(d, 'key'):
                if not id in self.paths and (z := storemgr.getPathByKey(key)): self.addPath(id, d, z)
        if 'dir' in d:
            for key in _list(d, 'dir'):
                if not id in self.paths and key in FileManager.games: self.addPath(id, d, games[key])
    
    def addFilter(self, id, d):
        if not id in self.filters: self.filters[id] = []
        self.filters[id].append(d)

    def addIgnore(self, id, paths):
        if not id in self.ignores: self.ignores[id] = set()
        for v in paths: self.ignores[id].add(v)

    def addPath(self, id, d, path, usePath = True):
        if path is None or not os.path.isdir(path := FileManager.getPathWithSpecialFolders(path, '')): return
        paths = _list(d, 'path') if usePath and 'path' in d else [path]
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
    def getPathByRegistryKey(key, d):
        path = FileManager.findRegistryPath([f'Wow6432Node\\{key}', key])
        if d is None: return path
        #if 'path' in d: d['path'] { path = Path.GetFullPath(GetPathWithSpecialFolders(path2.GetString(), path)); return !string.IsNullOrEmpty(path); }
        # else if (keyElem.Value.TryGetProperty("xml", out var xml)
        #     && keyElem.Value.TryGetProperty("xmlPath", out var xmlPath)
        #     && TryGetSingleFileValue(GetPathWithSpecialFolders(xml.GetString(), path), "xml", xmlPath.GetString(), out path))
        #     return !string.IsNullOrEmpty(path)
        return path

    def __init__(self, d):
        self.filters = {}
        self.paths = {}
        self.ignores = {}
        # applications
        if 'application' in d:
            for (id, val) in d['application'].items():
                if not id in self.paths: self.addApplication(id, val)
        # direct
        if 'direct' in d:
            for (id, val) in d['direct'].items():
                if 'path' in val:
                    for key in _list(val, 'path'):
                        self.addPath(id, d, key, False)
        # ignores
        if 'ignores' in d:
            for (id, val) in d['ignores'].items():
                self.addIgnore(id, _list(val, 'path'))
        # filters
        if 'filters' in d:
            for (id, val) in d['filters'].items():
                self.addFilter(id, val)
    def __repr__(self): return f'''
- paths: {list(self.paths.keys()) if self.paths else None}
- ignores: {list(self.ignores.keys()) if self.ignores else None}
- filters: {list(self.filters.keys()) if self.filters else None}'''
