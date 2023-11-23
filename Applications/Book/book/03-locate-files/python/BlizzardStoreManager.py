import os, platform, BlizzardProtoDatabase_pb2

def init():
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
    root = getPath()
    if root == None: return
    dbPath = os.path.join(root, 'product.db')
    if not os.path.exists(dbPath): return
    productDb = BlizzardProtoDatabase_pb2.Database()
    with open(dbPath, 'rb') as f:
        bytes = f.read()
        productDb.ParseFromString(bytes)
        #try: database.ParseFromString(bytes)
        #except InvalidProtocolBufferException: return None
        for app in productDb.ProductInstall:
            appPath = app.Settings.InstallPath
            if os.path.isdir(appPath): blizzardAppPaths[app.Uid] = appPath

blizzardAppPaths = {}
init()
print(blizzardAppPaths)
