import os
from typing import Callable
from enum import Enum
from .pak import PakFile
from openstk.gfx import IObjectManager, IMaterialManager, IShaderManager, ITextureManager, PlatformStats
from openstk.gfx_render import IMaterial
from openstk.gfx_texture import ITexture

# typedefs
class Shader: pass
class Texture: pass
class Material: pass

# TextureBuilderBase
class TextureBuilderBase:
    maxTextureMaxAnisotropy: int = PlatformStats.maxTextureMaxAnisotropy
    defaultTexture: Texture
    def buildTexture(info: ITexture, rng: range = None) -> Texture: pass
    def buildSolidTexture(width: int, height: int, rgba: list[float]) -> Texture: pass
    def buildNormalMap(source: Texture, strength: float): pass
    def deleteTexture(texture: Texture): pass

# TextureManager
class TextureManager(ITextureManager):
    _pakFile: PakFile
    _builder: TextureBuilderBase
    _cachedTextures: dict[object, (Texture, dict[str, object])] = {}
    _preloadTasks: dict[object, ITexture] = {}

    def __init__(self, pakFile: PakFile, builder: TextureBuilderBase):
        self._pakFile = pakFile
        self._builder = builder

    def buildSolidTexture(self, width: int, height: int, rgba: list[float] = None) -> Texture: return self._builder.buildSolidTexture(width, height, rgba)

    def buildNormalMap(self, source: Texture, strength: float) -> Texture: return self._builder.buildNormalMap(source, strength)

    @property
    def defaultTexture(self) -> Texture: return self._builder.defaultTexture

    def loadTexture(self, key: object, rng: range = None) -> (Texture, dict[str, object]):
        if key in self._cachedTextures: return self._cachedTextures[key]
        # Load & cache the texture.
        info = key if isinstance(key, ITexture) else self.loadTexture(key)
        texture = self._builder.buildTexture(info, rng) if info else self._builder.defaultTexture
        data = info.data if info else None
        self._cachedTextures[key] = (texture, data)
        return (texture, data)

    def preloadTexture(self, path: str) -> None:
        if path in self._cachedTextures: return
        # Start loading the texture file asynchronously if we haven't already started.
        if not path in self._preloadTasks: self._preloadTasks[path] = self._pakFile.loadFileObject(path)

    def deleteTexture(self, key: object) -> None:
        if not key in self._cachedTextures: return
        self._builder.deleteTexture(self._cachedTextures[0])
        self._cachedTextures.remove(key)

    async def _loadTexture(self, key: object) -> ITexture:
        assert(not key in self._cachedTextures)
        match key:
            case s if isinstance(key, str):
                self.preloadTexture(s)
                info = await self._preloadTasks[key]
                self._preloadTasks.remove(key)
                return info
            case _: raise Exception(f'Unknown {key}')

# ShaderBuilderBase
class ShaderBuilderBase:
    def buildShader(self, path: str, args: dict[str, bool]) -> Shader: pass
    def buildPlaneShader(self, path: str, args: dict[str, bool]) -> Shader: pass

# ShaderManager
class ShaderManager(IShaderManager):
    emptyArgs: dict[str, bool] = {}

    _pakFile: PakFile
    _builder: ShaderBuilderBase

    def __init__(self, pakFile: PakFile, builder: ShaderBuilderBase):
        self._pakFile = pakFile
        self._builder = builder
    
    def loadShader(self, path: str, args: dict[str, bool] = None) -> Shader:
        return self._builder.buildShader(path, args or self.emptyArgs)

    def loadPlaneShader(self, path: str, args: dict[str, bool] = None) -> Shader:
        return self._builder.buildPlaneShader(path, args or self.emptyArgs)

# ObjectBuilderBase
class ObjectBuilderBase:
    def createObject(prefab: object) -> object: pass
    def ensurePrefabContainerExists() -> None: pass
    def buildObject(source: object, materialManager: IMaterialManager) -> object: pass

# ObjectManager
class ObjectManager(IObjectManager):
    _pakFile: PakFile
    _materialManager: IMaterialManager
    _builder: ObjectBuilderBase
    _cachedPrefabs: dict[str, object] = {}
    _preloadTasks: dict[str, object] = {}

    def __init__(self, pakFile: PakFile, materialManager: IMaterialManager, builder: ObjectBuilderBase):
        self._pakFile = pakFile
        self._materialManager = materialManager
        self._builder = builder

    def createObject(self, path: str) -> (object, dict[str, object]):
        data = None
        self._builder.ensurePrefabContainerExists()
        # Load & cache the NIF prefab.
        if not path in self._cachedPrefabs: prefab = self._cachedPrefabs[path] = self.loadPrefabDontAddToPrefabCache(path)
        else: prefab = self._cachedPrefabs[path]
        # Instantiate the prefab.
        return self._builder.createObject(prefab)
 
    def preloadObject(self, path: str) -> None:
        if path in self._cachedPrefabs: return
        # Start loading the object asynchronously if we haven't already started.
        if not path in self._preloadTasks: self._preloadTasks[path] = self._pakFile.loadFileObject(object, path)

    async def loadPrefabDontAddToPrefabCache(path: str) -> object:
        assert(not path in self._cachedPrefabs)
        self.preloadObject(path)
        source = await self._preloadTasks[path]
        self._preloadTasks.remove(path)
        return self._builder.buildObject(source, self._materialManager)

# MaterialBuilderBase
class MaterialBuilderBase:
    textureManager : ITextureManager
    normalGeneratorIntensity: float = 0.75
    defaultMaterial: Material

    def __init__(self, textureManager: ITextureManager): self.TextureManager = textureManager
    def buildMaterial(self, key: object) -> Material: pass

# MaterialManager
class MaterialManager(IMaterialManager):
    _pakFile: PakFile
    _builder: MaterialBuilderBase
    _cachedMaterials: dict[object, (Material, dict[str, object])] = {}
    _preloadTasks: dict[object, IMaterial] = {}

    textureManager: ITextureManager

    def __init__(self, pakFile: PakFile, textureManager: ITextureManager, builder: MaterialBuilderBase):
        self._pakFile = pakFile
        self._textureManager = textureManager
        self._builder = builder

    def loadMaterial(self, key: object) -> (Material, dict[str, object]):
        if key in self._cachedMaterials: return self._cachedMaterials[key]
        # Load & cache the material.
        info = key if isinstance(key, IMaterial) else self.loadMaterialInfo(key)
        material = self._builder.buildMaterial(info) if info else self._builder.defaultMaterial
        data = info.data if info else None
        self._cachedMaterials[key] = (material, data)
        return material

    def preloadMaterial(self, path: str) -> None:
        if path in self._cachedMaterials: return
        # Start loading the material file asynchronously if we haven't already started.
        if not path in self._preloadTasks: self._preloadTasks[path] = self._pakFile.loadFileObject(path)

    async def loadMaterialInfo(self, key: object) -> IMaterial:
        assert(not key in self._cachedMaterials)
        match key:
            case path if isinstance(key, str):
                self.preloadMaterial(path)
                info = await self.preloadTasks[key]
                self.preloadTasks.remove(key)
                return info
            case _: raise Exception(f'Unknown {key}')

# typedefs
# class Type(Enum): pass
# class OS(Enum): pass

# Platform
class Platform:
    class Type(Enum):
        Unknown = 0
        OpenGL = 1
        Unity = 2
        Vulken = 3
        Test = 4
        Other = 5

    class OS(Enum):
        Windows = 1
        OSX = 1
        Linux = 1
        Android = 1

    platformType: Type = None
    platformTag: str = None
    platformOS: OS = OS.Windows
    graphicFactory: Callable = None
    startups: list[object] = []
    inTestHost: bool = False
    logFunc: Callable = None

    class Stats:
        maxTextureMaxAnisotropy: int

    # startup
    @staticmethod
    def startup() -> None:
        if Platform.inTestHost and len(Platform.startups) == 0: Platform.startups.append(TestPlatform.startup)
        for startup in Platform.startups:
            if startup(): return
        Platform.platformType = Platform.Type.Unknown
        Platform.graphicFactory = lambda source: None
        Platform.logFunc = lambda a: print(a)

# TestGraphic
class TestGraphic:
    def __init__(self, source):
        self._source = source

# TestPlatform
class TestPlatform:
    def startup() -> bool:
        Platform.platformType = Platform.Type.Test
        Platform.graphicFactory = lambda source: TestGraphic(source)
        Platform.logFunc = lambda a: print(a)
        return True
