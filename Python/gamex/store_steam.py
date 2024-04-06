import os, platform, winreg

# AcfStruct
class AcfStruct:
    def read(path):
        if not os.path.exists(path): return None
        with open(path, 'r') as f: return AcfStruct(f.read())

    def __init__(self, region):
        def nextEndOf(str, open, close, startIndex):
            if open == close:
                raise Exception('"Open" and "Close" char are equivalent!')
            openItem = 0; closeItem = 0
            for i in range(startIndex, len(str)):
                if str[i] == open: openItem += 1
                if str[i] == close:
                    closeItem += 1
                    if closeItem > openItem: return i
            raise Exception('Not enough closing characters!')
        self.get = {}
        self.value = {}
        lengthOfRegion = len(region); index = 0
        while (lengthOfRegion > index):
            firstStart = region.find('"', index)
            if firstStart == -1: break
            firstEnd = region.find('"', firstStart + 1)
            index = firstEnd + 1
            first = region[firstStart + 1:firstEnd]
            secondStart = region.find('"', index)
            secondOpen = region.find('{', index)
            if secondStart == -1:
                self.get[first] = None
            elif secondOpen == -1 or secondStart < secondOpen:
                secondEnd = region.find('"', secondStart + 1)
                index = secondEnd + 1
                second = region[secondStart + 1:secondEnd]
                self.value[first] = second.replace('\\\\', '\\')
            else:
                secondClose = nextEndOf(region, '{', '}', secondOpen + 1)
                acfs = AcfStruct(region[secondOpen + 1:secondClose])
                index = secondClose + 1
                self.get[first] = acfs

    def repr(self, depth):
        b = []
        for (k,v) in self.value.items():
            b.append(f'{"  "*depth}"{k}": "{v}"\n')
        for (k,v) in self.get.items():
            b.append(f'{"  "*depth}"{k}" {{\n{"  "*depth}')
            if not v is None: b.append(v.repr(depth + 1))
            b.append(f'}}\n')
        return ''.join(b)
    def __repr__(self): return self.repr(0)

def getPath():
    system = platform.system()
    if system == 'Windows':
        # windows paths
        try: key = winreg.OpenKey(winreg.HKEY_CURRENT_USER, 'SOFTWARE\\Valve\\Steam', 0, winreg.KEY_READ)
        except FileNotFoundError:
            try: key = winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, 'SOFTWARE\\Valve\\Steam', 0, winreg.KEY_READ | winreg.KEY_WOW64_32KEY)
            except FileNotFoundError: return None
        return winreg.QueryValueEx(key, 'SteamPath')[0]
    elif system == 'Linux':
        # linux paths
        home = os.path.expanduser('~')
        search = ['.steam', '.steam/steam', '.steam/root', '.local/share/Steam']
        paths = [os.path.join(home, path, 'appcache') for path in search]
    elif system == 'Darwin':
        # mac paths
        home = '/Users/Shared'
        search = ['Library/Application Support/Steam']
        paths = [os.path.join(home, path, 'appcache') for path in search]
    else: raise Exception(f'Unknown platform: {system}')
    return next(iter(x for x in paths if os.path.isdir(x)), None)
    
# get steamPaths
steamPaths = {}
root = getPath()
if root:
    # query games
    libraryFolders = AcfStruct.read(os.path.join(root, 'steamapps', 'libraryfolders.vdf'))
    for folder in libraryFolders.get['libraryfolders'].get.values():
        path = folder.value['path']
        if not os.path.isdir(path): continue
        for appId in folder.get['apps'].value.keys():
            appManifest = AcfStruct.read(os.path.join(path, 'steamapps', f'appmanifest_{appId}.acf'))
            if appManifest is None: continue
            # add appPath if exists
            appPath = os.path.join(path, 'steamapps', 'common', appManifest.get['AppState'].value['installdir'])
            if os.path.isdir(appPath): steamPaths[appId] = appPath

# print(steamPaths)
