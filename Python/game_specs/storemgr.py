@staticmethod
def getPathByKey(key):
    (k,v) = key.split(':', 2)
    match k:
        case 'Steam':
            from . import storemgr_steam
            return storemgr_steam.steamPaths[v] if v in storemgr_steam.steamPaths else None
        case 'GOG':
            from . import storemgr_gog
            return storemgr_gog.gogPaths[v] if v in storemgr_gog.gogPaths else None
        case 'Blizzard':
            from . import storemgr_blizzard
            return storemgr_blizzard.blizzardPaths[v] if v in storemgr_blizzard.blizzardPaths else None
        case 'Epic':
            from . import storemgr_epic
            return storemgr_epic.epicPaths[v] if v in storemgr_epic.epicPaths else None
        case 'Unknown': return None
        case _: raise Exception(f'Unknown key: {key}')

# print(getPathByKey('Steam:1755910'))
