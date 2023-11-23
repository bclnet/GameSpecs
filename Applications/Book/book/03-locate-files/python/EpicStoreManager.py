import os, platform, json

def init():
    def getPath():
        system = platform.system()
        if system == 'Windows':
            home = os.getenv('ALLUSERSPROFILE')
            paths = [os.path.join(home, path, 'Data') for path in ['Epic/EpicGamesLauncher']]
            return next(iter(x for x in paths if os.path.isdir(x)), None)
        elif system == 'Linux':
            home = os.path.expanduser('~')
            paths = [os.path.join(home, path, 'Data') for path in ['?GOG?']]
            return next(iter(x for x in paths if os.path.isdir(x)), None)
        elif system == 'Darwin':
            home = '/Users/Shared'
            paths = [os.path.join(home, path, 'Data') for path in ['Epic/EpicGamesLauncher']]
            return next(iter(x for x in paths if os.path.isdir(x)), None)
        else: raise Exception(f'Unknown platform: {system}')
    
    root = getPath()
    if root == None: return
    dbPath = os.path.join(root, 'Manifests')
    for s in [s for s in os.listdir(dbPath) if s.endswith('.item')]:
        with open(os.path.join(dbPath, s), 'r') as f:
            epicAppPaths[s[:-5]] = json.loads(f.read())['InstallLocation']

epicAppPaths = {}
init()
print(epicAppPaths)
