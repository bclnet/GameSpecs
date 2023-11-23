import os, platform, sqlite3
from contextlib import closing

def init():
    def getPath():
        system = platform.system()
        if system == 'Windows':
            home = os.getenv('ALLUSERSPROFILE')
            paths = [os.path.join(home, path, 'storage') for path in ['GOG.com\Galaxy']]
            return next(iter(s for s in paths if os.path.isdir(s)), None)
        elif system == 'Linux':
            home = os.path.expanduser('~')
            paths = [os.path.join(home, path, 'Storage') for path in ['?GOG?']]
            return next(iter(s for s in paths if os.path.isdir(s)), None)
        elif system == 'Darwin':
            home = '/Users/Shared'
            paths = [os.path.join(home, path, 'Storage') for path in ['GOG.com/Galaxy']]
            return next(iter(s for s in paths if os.path.isdir(s)), None)
        else: raise Exception(f'Unknown platform: {system}')
    
    root = getPath()
    if root == None: return
    dbPath = os.path.join(root, 'galaxy-2.0.db')
    if not os.path.exists(dbPath): return
    with closing(sqlite3.connect(dbPath)) as connection:
        with closing(connection.cursor()) as cursor:
            for s in cursor.execute('SELECT productId, installationPath FROM InstalledBaseProducts').fetchall():
                if os.path.isdir(s[1]): gogAppPaths[s[0]] = s[1]

gogAppPaths = {}
init()
print(gogAppPaths)
