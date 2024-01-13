import os
from typing import Any
# from .platform import Startups, Type, PlatformType, GraphicFactory, LogFunc, startup
from openstk.gl import IOpenGLGraphic, QuadIndexBuffer, GLMeshBufferCache
from openstk.gl_shader import ShaderDebugLoader
from .platform import ObjectBuilderBase, ObjectManager, MaterialManager, ShaderBuilderBase, ShaderManager, TextureManager, Platform

# typedefs
class PakFile: pass
class Shader: pass
class ShaderLoader: pass
class IMaterialManager: pass

# OpenGLGraphic
class OpenGLGraphic(IOpenGLGraphic):
    source: PakFile
    textureManager: TextureManager
    materialManager: MaterialManager
    objectManager: ObjectManager
    shaderManager: ShaderManager

    def __init__(self, source: PakFile):
        self.source = source
        self.textureManager = TextureManager(source, OpenGLTextureBuilder())
        self.materialManager = MaterialManager(source, self.textureManager, OpenGLMaterialBuilder(self.textureManager))
        self.objectManager = ObjectManager(source, self.materialManager, OpenGLObjectBuilder())
        self.shaderManager = ShaderManager(source, OpenGLShaderBuilder())
        self._meshBufferCache = GLMeshBufferCache()

    def loadTexture(self, path: str, range: range = None) -> (int, dict[str, object]): return self.textureManager.loadTexture(path, range)
    def preloadTexture(self, path: str) -> None: self.textureManager.preloadTexture(path)
    def createObject(self, path: str) -> (object, dict[str, object]): return self.objectManager.createObject(path)
    def preloadObject(self, path: str) -> None: self.objectManager.preloadObject(path)
    def loadShader(self, path: str, args: dict[str, bool] = None) -> Shader: return self.shaderManager.loadShader(path, args)
    def loadFileObject(path: str) -> object: return self.source.loadFileObject(path)

    # cache
    _quadIndices: QuadIndexBuffer
    @property
    def quadIndices(self) -> QuadIndexBuffer: return self._quadIndices if self._quadIndices else (_quadIndices := QuadIndexBuffer(65532))
    meshBufferCache: GLMeshBufferCache

# OpenGLObjectBuilder
class OpenGLObjectBuilder(ObjectBuilderBase):
    def ensurePrefabContainerExists() -> None: pass
    def createObject(prefab: object) -> object: raise NotImplementedError()
    def buildObject(source: object, materialManager: IMaterialManager) -> object: raise NotImplementedError()

# OpenGLShaderBuilder
class OpenGLShaderBuilder(ShaderBuilderBase):
    _loader: ShaderLoader = ShaderDebugLoader()
    def buildShader(path: str, args: dict[str, bool]) -> Shader: return self._loader.loadShader(path, args)
    def buildPlaneShader(path: str, args: dict[str, bool]) -> Shader: return self._loader.loadPlaneShader(path, args)

# OpenGLObjectBuilder
class OpenGLObjectBuilder(ObjectBuilderBase):
    def ensurePrefabContainerExists() -> None: pass
    def createObject(prefab: object) -> object: raise NotImplementedError()
    def buildObject(source: object, materialManager: IMaterialManager) -> object: raise NotImplementedError()

# OpenGLPlatform
class OpenGLPlatform:
    def startup() -> bool:
        PlatformType = Platform.Type.OpenGL
        GraphicFactory = lambda source: OpenGLGraphic(source)
        LogFunc = lambda a: print(a)
        return True

# Startups.append(OpenGLPlatform.startup)
# startup()