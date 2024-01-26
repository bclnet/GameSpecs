import os, platform, sqlite3
from contextlib import closing

def getPath():
    system = platform.system()
    if system == 'Windows':
        # windows paths
        home = os.getenv('ALLUSERSPROFILE')
        search = ['GOG.com/Galaxy']
        paths = [os.path.join(home, path, 'storage') for path in search]
    elif system == 'Linux':
        # linux paths
        home = os.path.expanduser('~')
        search = ['??']
        paths = [os.path.join(home, path, 'Storage') for path in search]
    elif system == 'Darwin':
        # mac paths
        home = '/Users/Shared'
        search = ['GOG.com/Galaxy']
        paths = [os.path.join(home, path, 'Storage') for path in search]
    else: raise Exception(f'Unknown platform: {system}')
    return next(iter(s for s in paths if os.path.isdir(s)), None)
    
# get gogPaths
gogPaths = {}
root = getPath()
if root and os.path.exists(dbPath := os.path.join(root, 'galaxy-2.0.db')):
    # query games
    with closing(sqlite3.connect(dbPath)) as connection:
        with closing(connection.cursor()) as cursor:
            for s in cursor.execute('SELECT productId, installationPath FROM InstalledBaseProducts').fetchall():
                # add appPath if exists
                appPath = s[1]
                if os.path.isdir(appPath): gogPaths[s[0]] = appPath

# print(gogPaths)
