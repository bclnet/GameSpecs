import os, platform, psutil, winreg, StoreManager
GAMESPATH = 'Games'

class FileManager:
    ApplicationPath = os.getcwd()
    # get locale games
    gameRoots = [os.path.join(x.mountpoint, GAMESPATH) for x in psutil.disk_partitions()]
    if platform.system() == 'Android': gameRoots.append(os.path.join('/sdcard', GAMESPATH))
    games = {x:os.path.join(r,x) for r in gameRoots if os.path.isdir(r) for x in os.listdir(r)}
    
    def addApplication(s, id, d):
        system = platform.system()
        if system == 'Windows' and 'reg' in d:
            for key in d['reg'] if isinstance(d['reg'], list) else [d['reg']]:
                if not id in s.paths and (z := s.getPathByRegistryKey(key, d)): s.addPath(id, d, z)
        if 'key' in d:
            for key in d['key'] if isinstance(d['key'], list) else [d['key']]:
                if not id in s.paths and (z := StoreManager.getPathByKey(key)): s.addPath(id, d, z)
        if 'dir' in d:
            for key in d['dir'] if isinstance(d['dir'], list) else [d['dir']]:
                if not id in s.paths and key in FileManager.games: s.addPath(id, d, games[key])
    
    def addFilter(s, id, d):
        if not id in s.filters: s.filters[id] = []
        s.filters[id].append(d)

    def addIgnore(s, id, d):
        if not id in s.ignores: s.ignores[id] = []
        s.ignores[id].append(d)

    def addPath(s, id, d, path, usePath = True):
        if path is None or not os.path.isdir(path := FileManager.getPathWithSpecialFolders(path, '')): return
        paths = [os.path.join(path, x) for x in (d['path'] if isinstance(d['path'], list) else [d['path']])] if usePath and 'path' in d else [path]
        for p in paths:
            if not os.path.isdir(p): continue
            if not id in s.paths: s.paths[id] = []
            s.paths[id].append(p)

    @staticmethod
    def getPathWithSpecialFolders(path, rootPath):
        return f'{rootPath}{path[6:]}' if path.startswith('%Path%') else \
        f'{FileManager.ApplicationPath}{path[9:]}' if path.startswith('%AppPath%') else \
        f'{os.getenv('APPDATA')}{path[9:]}' if path.startswith('%AppData%') else \
        f'{os.getenv('LOCALAPPDATA')}{path[14:]}' if path.startswith('%LocalAppData%') else \
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

    def __init__(s, d):
        s.filters = {}
        s.paths = {}
        s.ignores = {}
        # applications
        if 'application' in d:
            for id in d['application']:
                if not id in s.paths: s.addApplication(id, d['application'][id])
        # direct
        if 'direct' in d:
            for id in d['direct']:
                if 'path' in d['direct'][id]:
                    val = d['direct'][id]['path']
                    for key in val if isinstance(val, list) else [val]:
                        s.addPath(id, d, key, False)
        # ignores
        if 'ignores' in d:
            for id in d['ignores']:
                s.addIgnore(id, d['ignores'][id])
        # filters
        if 'filters' in d:
            for id in d['filters']:
                s.addFilter(id, d['filters'][id])
    def __repr__(s): return f'''
- paths: {list(s.paths.keys()) if s.paths else None}
- ignores: {list(s.ignores.keys()) if s.ignores else None}
- filters: {list(s.filters.keys()) if s.filters else None}'''
