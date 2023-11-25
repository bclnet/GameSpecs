from BlizzardStoreManager import blizzardAppPaths
from EpicStoreManager import epicAppPaths
from GogStoreManager import gogAppPaths
from SteamStoreManager import steamAppPaths

@staticmethod
def getPathByKey(key):
    (k,v) = key.split(':', 2)
    match k:
        case 'Steam': return steamAppPaths[v] if v in steamAppPaths else None
        case 'GOG': return gogAppPaths[v] if v in gogAppPaths else None
        case 'Blizzard': return blizzardAppPaths[v] if v in blizzardAppPaths else None
        case 'Epic': return epicAppPaths[v] if v in epicAppPaths else None
        case 'Unknown': return None
        case _: raise Exception(f'Unknown key: {key}')

# print(getPathByKey('Steam:1755910'))
