import os, platform

class BlizzardStoreManager:
    def __init__(self):
        def getPath():
            system = platform.system()
            if system == 'Windows':
                return os.path.join(os.getenv('ALLUSERSPROFILE'), 'Battle.net', 'Agent')
            elif system == 'Linux':
                home = os.path.expanduser('~')
                paths = [os.path.join(home, path, 'appcache') for path in ['.steam', '.steam/steam', '.steam/root', '.local/share/Steam']]
                return next(iter(x for x in paths if os.path.isdir(x)), None)
            elif system == 'Darwin':
                home = '/Users/Shared'
                paths = [os.path.join(home, path, 'data') for path in ['Battle.net/Agent']]
                return next(iter(x for x in paths if os.path.isdir(x)), None)
            else: raise Exception(f'Unknown platform: {system}')
        self.root = 'test'
        root = getPath()
        if root == None: return
        dbPath = os.path.join(root, 'product.db')
        if not os.path.exists(dbPath): return

print(BlizzardStoreManager().root)
