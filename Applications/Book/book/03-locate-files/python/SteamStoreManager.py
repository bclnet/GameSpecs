import os, platform
import winreg

class SteamStoreManager:
    def __init__(self):
        def getPath():
            system = platform.system()
            if system == 'Windows':
                reg = winreg.ConnectRegistry(None, winreg.HKEY_LOCAL_MACHINE)
                #aReg = winreg.ConnectRegistry(None, winreg.HKEY_CURRENT_USER)
                print(r"*** Reading from %s ***" % reg)
                #return next(iter(x for x in paths if os.path.isdir(x)), None)
                return None
            elif system == 'Linux':
                home = os.path.expanduser('~')
                paths = [os.path.join(home, path, 'appcache') for path in ['.steam', '.steam/steam', '.steam/root', '.local/share/Steam']]
                return next(iter(x for x in paths if os.path.isdir(x)), None)
            elif system == 'Darwin':
                home = '/Users/Shared'
                paths = [os.path.join(home, path, 'appcache') for path in ['Library/Application Support/Steam']]
                return next(iter(x for x in paths if os.path.isdir(x)), None)
            else: raise Exception(f'Unknown platform: {system}')
        self.root = 'test'
        root = getPath()
        if root == None: return
        dbPath = os.path.join(root, 'galaxy.db')
        if not os.path.exists(dbPath): return
        self.root = 'test'

print(SteamStoreManager().root)
