@staticmethod
def getPathByKey(key):
    (k,v) = key.split(':', 2)
    match k:
        case 'Steam':
            from SteamStoreManager import steamAppPaths
            return steamAppPaths[v] if v in steamAppPaths else None
        case 'GOG':
            from GogStoreManager import gogAppPaths
            return gogAppPaths[v] if v in gogAppPaths else None
        case 'Blizzard':
            from BlizzardStoreManager import blizzardAppPaths
            return blizzardAppPaths[v] if v in blizzardAppPaths else None
        case 'Epic':
            from EpicStoreManager import epicAppPaths
            return epicAppPaths[v] if v in epicAppPaths else None
        case 'Unknown': return None
        case _: raise Exception(f'Unknown key: {key}')

# print(getPathByKey('Steam:1755910'))
