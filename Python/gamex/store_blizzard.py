import os, platform
from .Blizzard_pb2 import Database

def getPath() -> None:
    system = platform.system()
    if system == 'Windows':
        # windows paths
        home = os.getenv('ALLUSERSPROFILE')
        paths = [os.path.join(home, 'Battle.net', 'Agent')]
    elif system == 'Linux':
        # linux paths
        home = os.path.expanduser('~')
        search = ['.steam', '.steam/steam', '.steam/root', '.local/share/Steam']
        paths = [os.path.join(home, path, 'appcache') for path in search]
    elif system == 'Darwin':
        # mac paths
        home = '/Users/Shared'
        search = ['Battle.net/Agent']
        paths = [os.path.join(home, path, 'data') for path in search]
    else: raise Exception(f'Unknown platform: {system}')
    return next(iter(x for x in paths if os.path.isdir(x)), None)

# get blizzardPaths
blizzardPaths = {}
root = getPath()
if root and os.path.exists(dbPath := os.path.join(root, 'product.db')):
    # query games
    productDb = Database()
    with open(dbPath, 'rb') as f:
        bytes = f.read()
        productDb.ParseFromString(bytes)
        #try: database.ParseFromString(bytes)
        #except InvalidProtocolBufferException: return None
        for app in productDb.ProductInstall:
            # add appPath if exists
            appPath = app.Settings.InstallPath
            if os.path.isdir(appPath): blizzardPaths[app.Uid] = appPath

# print(blizzardPaths)