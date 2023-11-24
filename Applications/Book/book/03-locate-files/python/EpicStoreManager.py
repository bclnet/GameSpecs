import os, platform, json

def init():
    def getPath():
        system = platform.system()
        if system == 'Windows':
            # windows paths
            home = os.getenv('ALLUSERSPROFILE')
            search = ['Epic/EpicGamesLauncher']
            paths = [os.path.join(home, path, 'Data') for path in search]
        elif system == 'Linux':
            # linux paths
            home = os.path.expanduser('~')
            search = ['?GOG?']
            paths = [os.path.join(home, path, 'Data') for path in search]
        elif system == 'Darwin':
            # mac paths
            home = '/Users/Shared'
            search = ['Epic/EpicGamesLauncher']
            paths = [os.path.join(home, path, 'Data') for path in search]
        else: raise Exception(f'Unknown platform: {system}')
        return next(iter(x for x in paths if os.path.isdir(x)), None)
    
    # get dbPath
    root = getPath()
    if root == None: return
    dbPath = os.path.join(root, 'Manifests')
    # query games
    for s in [s for s in os.listdir(dbPath) if s.endswith('.item')]:
        with open(os.path.join(dbPath, s), 'r') as f:
            # add appPath if exists
            appPath = json.loads(f.read())['InstallLocation']
            if os.path.isdir(appPath): epicAppPaths[s[:-5]] = appPath

epicAppPaths = {}
init()
# print(epicAppPaths)
