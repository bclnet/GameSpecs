import os, platform, json

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
    
# get ubisoftPaths
ubisoftPaths = {}
root = getPath()
if root and os.path.exists(dbPath := os.path.join(root, 'settings.yaml')): 
    with open(dbPath, 'r') as f: body = f.read()
    gamePath = body[body.index('game_installation_path:') + 23:body.index('installer_cache_path')].strip()

    # query games
    for s in [s for s in os.listdir(gamePath)]:
        # print(s)
        pass
        
    #     with open(os.path.join(dbPath, s), 'r') as f:
    #         # add appPath if exists
    #         appPath = json.loads(f.read())['InstallLocation']
    #         if os.path.isdir(appPath): ubisoftPaths[s[:-5]] = appPath

# print(ubisoftPaths)
