import os, platform, sqlite3
from contextlib import closing

@staticmethod
def init():
    def getPath():
        system = platform.system()
        if system == 'Windows':
            # windows paths
            home = os.getenv('ALLUSERSPROFILE')
            search = ['GOG.com/Galaxy']
            paths = [os.path.join(home, path, 'storage') for path in search]
            return next(iter(s for s in paths if os.path.isdir(s)), None)
        elif system == 'Linux':
            # linux paths
            home = os.path.expanduser('~')
            search = ['??']
            paths = [os.path.join(home, path, 'Storage') for path in search]
            return next(iter(s for s in paths if os.path.isdir(s)), None)
        elif system == 'Darwin':
            # mac paths
            home = '/Users/Shared'
            paths = [os.path.join(home, path, 'Storage') for path in ['GOG.com/Galaxy']]
            return next(iter(s for s in paths if os.path.isdir(s)), None)
        else: raise Exception(f'Unknown platform: {system}')
    
    # get dbPath
    root = getPath()
    if root is None: return
    dbPath = os.path.join(root, 'galaxy-2.0.db')
    if not os.path.exists(dbPath): return
    # query games
    with closing(sqlite3.connect(dbPath)) as connection:
        with closing(connection.cursor()) as cursor:
            for s in cursor.execute('SELECT productId, installationPath FROM InstalledBaseProducts').fetchall():
                # add appPath if exists
                appPath = s[1]
                if os.path.isdir(appPath): gogAppPaths[s[0]] = appPath

gogAppPaths = {}
init()
# print(gogAppPaths)
