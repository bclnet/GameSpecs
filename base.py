import os, json, glob, re
from typing import Any

def _value(elem: dict[str, Any], key: str, default: Any = None) -> Any:
    return elem[key] if key in elem else default
def _list(elem: dict[str, Any], key: str, default: Any = None) -> Any:
    return (elem[key] if isinstance(elem[key], list) else [elem[key]]) if key in elem else default
def _method(elem: dict[str, Any], key: str, method: Any, default: Any = None) -> Any:
    return method(elem[key]) if key in elem else default
def _related(elem: dict[str, Any], key: str, method: Any, default: Any = None) -> Any:
    return { k:method(k, v) for k,v in elem[key].items() } if key in elem else {}
def _relatedTrim(elem: dict[str, Any], key: str, method: Any, default: Any = None) -> Any:
    return { k:v for k,v in { k:method(k, v) for k,v in elem[key].items() }.items() if v } if key in elem else {}

class FileManager:
    def __init__(self, elem):
        # related
        self.applications = _related(elem, 'application', lambda k,v:FileManager.Application(k, v))
        self.directs = _related(elem, 'directs', lambda k,v:FileManager.Direct(k, v))
        self.ignores = _related(elem, 'ignores', lambda k,v:FileManager.Ignore(k, v))
        self.filters = _related(elem, 'filters', lambda k,v:FileManager.Filter(k, v))
    def __repr__(self): return f'''
- applications: {list(self.applications.keys()) if self.applications else None}
- directs: {list(self.directs.keys()) if self.directs else None}
- ignores: {list(self.ignores.keys()) if self.ignores else None}
- filters: {list(self.filters.keys()) if self.filters else None}'''
    def merge(self, source):
        self.ignores.update(source.ignores)
        self.filters.update(source.filters)
    class Application:
        def __init__(self, id, elem):
            self.id = id
            self.dir = _list(elem, 'dir')
            self.key = _list(elem, 'key')
            self.reg = _list(elem, 'ref')
            self.path = _list(elem, 'path')
        def __repr__(self): return f'{self.id}'
    class Direct:
        def __init__(self, id, elem):
            self.id = id
            self.path = _list(elem, 'path')
        def __repr__(self): return f'{self.id}'
    class Ignore:
        def __init__(self, id, elem):
            self.id = id
            self.path = _list(elem, 'path')
        def __repr__(self): return f'{self.id}'
    class Filter:
        def __init__(self, id, elem):
            self.id = id
            self.v = elem
        def __repr__(self): return f'{self.id}'

class Family:
    def __init__(self, elem):
        self.id = _value(elem, 'id')
        self.name = _value(elem, 'name')
        self.studio = _value(elem, 'studio')
        self.description = _value(elem, 'description')
        self.urls = _list(elem, 'url', [])
        self.specs = _list(elem, 'specs')
        # file manager
        self.fileManager = _method(elem, 'fileManager', FileManager)
        # related
        dgame = FamilyGame(None, self, None, None)
        def gameMethod(k, v):
            nonlocal dgame
            game = FamilyGame(self, k, v, dgame)
            if k.startswith('*'): dgame = game; return None
            return game
        self.engines = _related(elem, 'engines', lambda k,v:FamilyEngine(self, k, v))
        self.games = _relatedTrim(elem, 'games', gameMethod)
    def __repr__(self): return f'''
{self.id}: {self.name}
engines: {[x for x in self.engines.values()]}
games: {[x for x in self.games.values()]}
fileManager: {self.fileManager if self.fileManager else None}'''
    # merge
    def merge(self, source) -> None:
        if not source: return
        self.engines.update(source.engines)
        self.games.update(source.games)
        if self.fileManager: self.fileManager.merge(source.fileManager)
        else: self.fileManager = source.fileManager
    # open PakFile
    def openPakFile(res, throwOnError = True):
        resource = res if isinstance(res, Resource) else \
        FileManager.parseResource(res) if isinstance(res, str) else None
        if not resource and throwOnError: raise Exception(f'Unknown res: {res}')

class FamilyEngine:
    def __init__(self, family, id, elem):
        self.family = family
        self.id = id
        self.name = _value(elem, 'name')
    def __repr__(self): return f'\n  {self.id}: {self.name}'

class FamilyGame:
    class Edition:
        def __init__(self, id, elem):
            self.id = id
            self.name = _value(elem, 'name')
            self.key = _value(elem, 'key')
        def __repr__(self): return f'{self.id}: {self.name}'
    class DownloadableContent:
        def __init__(self, id, elem):
            self.id = id
            self.name = _value(elem, 'name')
            self.path = _value(elem, 'path')
        def __repr__(self): return f'{self.id}: {self.name}'
    class Locale:
        def __init__(self, id, elem):
            self.id = id
            self.name = _value(elem, 'name')
        def __repr__(self): return f'{self.id}: {self.name}'
    def __init__(self, family, id, elem, dgame):
        self.family = family
        self.id = id
        if not dgame:
            self.ignore = False; self.searchBy = 'Pak'; self.paks = ['game:/']
            self.engine = self.resource = self.searchBy = self.pakExts = None
            return
        self.ignore = _value(elem, 'n/a', dgame.ignore)
        self.name = _value(elem, 'name')
        self.engine = _value(elem, 'engine', dgame.engine)
        self.resource = _value(elem, 'resource', dgame.resource)
        self.urls = _list(elem, 'url')
        self.date = _value(elem, 'date')
        self.key = _value(elem, 'key')
        self.status = _value(elem, 'status', [])
        self.tags = _value(elem, 'tags')
        # interface
        self.searchBy = _value(elem, 'searchBy', dgame.searchBy)
        self.pakExts = _list(elem, 'pakExt', dgame.pakExts) 
        # related
        self.editions = _related(elem, 'editions', lambda k,v:FamilyGame.Edition(k, v))
        self.dlcs = _related(elem, 'dlcs', lambda k,v:FamilyGame.DownloadableContent(k, v))
        self.locales = _related(elem, 'locales', lambda k,v:FamilyGame.Locale(k, v))
    def __repr__(self): return f'''
  {self.id}: {self.name} {self.status}
  - editions: {[x for x in self.editions.values()] if self.editions else None}
  - dlc: {[x for x in self.dlc.values()] if self.dlc else None}
  - locales: {[x for x in self.locales.values()] if self.locales else None}'''

# create Family
@staticmethod
def createFamily(path: str, loader: Any) -> Family:
    elem = loader(path)
    family = Family(elem)
    if family.specs:
        for spec in family.specs:
            family.merge(createFamily(spec, loader))
    return family

@staticmethod
def init(root):
    rootPath = f'{root}/gamex/Specs'
    def commentRemover(text):
        def replacer(match): self = match.group(0); return ' ' if self.startswith('/') else self
        pattern = re.compile(r'//.*?$|/\*.*?\*/|\'(?:\\.|[^\\\'])*\'|"(?:\\.|[^\\"])*"', re.DOTALL | re.MULTILINE)
        return re.sub(pattern, replacer, text)
    def loadJson(path):
        with open(os.path.join(rootPath, path), encoding='utf8') as f:
            return json.loads(commentRemover(f.read()).encode().decode('utf-8-sig'))
    families = {}
    for path in glob.glob(os.path.join(rootPath, '*.json')):
        basePath = os.path.basename(path)
        if basePath.find('+') >= 0: continue
        family = createFamily(basePath, loadJson)
        families[family.id] = family
    return families

@staticmethod
def getFamily(id, throwOnError = True):
    family =  _value(families, id)
    if not family and throwOnError: raise Exception(f'Unknown family: {id}')
    return family
