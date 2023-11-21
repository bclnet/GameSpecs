import os, platform
import sqlite3
from contextlib import closing

class EpicStoreManager:
    def __init__(self):
        def getPath():
            system = platform.system()
            if system == 'Windows':
                home = os.getenv('ALLUSERSPROFILE')
                paths = [os.path.join(home, path, 'storage') for path in ['GOG.com\Galaxy']]
                return next(iter(x for x in paths if os.path.isdir(x)), None)
            elif system == 'Linux':
                home = os.path.expanduser('~')
                paths = [os.path.join(home, path, 'appcache') for path in ['?GOG?']]
                return next(iter(x for x in paths if os.path.isdir(x)), None)
            elif system == 'Darwin':
                home = '/Users/Shared'
                paths = [os.path.join(home, path, 'Storage') for path in ['GOG.com/Galaxy']]
                return next(iter(x for x in paths if os.path.isdir(x)), None)
            else: raise Exception(f'Unknown platform: {system}')
        self.appPaths = {}
        root = getPath()
        if root == None: return
        dbPath = os.path.join(root, 'galaxy.db')
        if not os.path.exists(dbPath): return
        with closing(sqlite3.connect(dbPath)) as connection:
            with closing(connection.cursor()) as cursor:
                rows = cursor.execute('SELECT productId, installationPath FROM InstalledBaseProducts').fetchall()
                print(rows)

print(EpicStoreManager().root)
