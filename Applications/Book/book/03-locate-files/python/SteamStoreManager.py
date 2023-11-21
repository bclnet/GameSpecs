import os, platform
import winreg

class SteamStoreManager:
    def __init__(self):
        def getPath():
            system = platform.system()
            if system == 'Windows':
                try: key = winreg.OpenKey(winreg.HKEY_CURRENT_USER, 'SOFTWARE\Valve\Steam', 0, winreg.KEY_READ)
                except FileNotFoundError:
                    try: key = winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, 'SOFTWARE\Valve\Steam', 0, winreg.KEY_READ | winreg.KEY_WOW64_32KEY)
                    except FileNotFoundError: return None
                return winreg.QueryValueEx(key, 'SteamPath')[0]
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
        print(root)
        if root == None: return
        dbPath = os.path.join(root, 'galaxy.db')
        if not os.path.exists(dbPath): return
        self.root = 'test'

print(SteamStoreManager().root)
