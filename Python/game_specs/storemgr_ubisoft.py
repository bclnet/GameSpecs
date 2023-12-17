import os, platform, json

@staticmethod
def init():
    def getPath():
        system = platform.system()
        if system == 'Windows':
            # windows paths
            home = os.getenv('LOCALAPPDATA')
            search = ['Ubisoft Game Launcher']
            paths = [os.path.join(home, path) for path in search]
        elif system == 'Linux':
            # linux paths
            home = os.path.expanduser('~')
            search = ['??']
            paths = [os.path.join(home, path) for path in search]
        elif system == 'Darwin':
            # mac paths
            home = '/Users/Shared'
            search = ['??']
            paths = [os.path.join(home, path) for path in search]
        else: raise Exception(f'Unknown platform: {system}')
        return next(iter(x for x in paths if os.path.isdir(x)), None)
    
    # get dbPath
    root = getPath()
    if root is None: return
    dbPath = os.path.join(root, 'settings.yaml')
    if not os.path.exists(dbPath): return
    with open(dbPath, 'r') as f: body = f.read()
    gamePath = body[body.index('game_installation_path:') + 23:body.index('installer_cache_path')].strip()

    # query games
    for s in [s for s in os.listdir(gamePath)]:
        print(s)
        
    #     with open(os.path.join(dbPath, s), 'r') as f:
    #         # add appPath if exists
    #         appPath = json.loads(f.read())['InstallLocation']
    #         if os.path.isdir(appPath): ubisoftPaths[s[:-5]] = appPath

ubisoftPaths = {}
init()
print(ubisoftPaths)
