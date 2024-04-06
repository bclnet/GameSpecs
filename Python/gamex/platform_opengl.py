import os
import numpy as np
from OpenGL.GL import *
from openstk.gfx import IFixedMaterial, IParamMaterial
from openstk.gl import IOpenGLGraphic, QuadIndexBuffer, GLMeshBufferCache
from openstk.gl_shader import ShaderDebugLoader
from openstk.gl_render import GLRenderMaterial
from openstk.gfx_texture import TextureGLFormat, TextureFlags
from openstk.poly import IDisposable
from .platform import ObjectBuilderBase, ObjectManager, MaterialBuilderBase, MaterialManager, ShaderBuilderBase, ShaderManager, TextureManager, TextureBuilderBase, Platform

# typedefs
class PakFile: pass
class Shader: pass
class ShaderLoader: pass
class IMaterialManager: pass
class ITexture: pass

# OpenGLObjectBuilder
class OpenGLObjectBuilder(ObjectBuilderBase):
    def ensurePrefabContainerExists(self) -> None: pass
    def createObject(self, prefab: object) -> object: raise NotImplementedError()
    def buildObject(self, source: object, materialManager: IMaterialManager) -> object: raise NotImplementedError()

# OpenGLShaderBuilder
class OpenGLShaderBuilder(ShaderBuilderBase):
    _loader: ShaderLoader = ShaderDebugLoader()
    def buildShader(self, path: str, args: dict[str, bool]) -> Shader: return self._loader.loadShader(path, args)
    def buildPlaneShader(self, path: str, args: dict[str, bool]) -> Shader: return self._loader.loadPlaneShader(path, args)

# OpenGLTextureBuilder
class OpenGLTextureBuilder(TextureBuilderBase):
    _defaultTexture: int = -1

    @property
    def defaultTexture(self) -> int:
        if self._defaultTexture > -1: return self._defaultTexture
        self._defaultTexture = self._buildAutoTexture()
        return self._defaultTexture

    def _buildAutoTexture(self) -> int: return self.buildSolidTexture(4, 4, [
        0.9, 0.2, 0.8, 1.0,
        0.0, 0.9, 0.0, 1.0,
        0.9, 0.2, 0.8, 1.0,
        0.0, 0.9, 0.0, 1.0,

        0.0, 0.9, 0.0, 1.0,
        0.9, 0.2, 0.8, 1.0,
        0.0, 0.9, 0.0, 1.0,
        0.9, 0.2, 0.8, 1.0,

        0.9, 0.2, 0.8, 1.0,
        0.0, 0.9, 0.0, 1.0,
        0.9, 0.2, 0.8, 1.0,
        0.0, 0.9, 0.0, 1.0,

        0.0, 0.9, 0.0, 1.0,
        0.9, 0.2, 0.8, 1.0,
        0.0, 0.9, 0.0, 1.0,
        0.9, 0.2, 0.8, 1.0
        ])

    def buildTexture(self, info: ITexture, rng: range = None) -> int:
        return self.defaultTexture

        id = glGenTextures(1)
        numMipMaps = max(1, info.mipMaps)
        start = rng[0] or 0 if rng else 0
        end = numMipMaps - 1

        glBindTexture(GL_TEXTURE_2D, id)
        glTexParameter(GL_TEXTURE_2D, GL_TEXTURE_MAX_LEVEL, end - start)
        bytes, fmt, ranges = info.begin(Platform.Type.OpenGL)
        pixels = []

        def compressedTexImage2D(info: ITexture, i: int, internalFormat: int) -> bool:
            nonlocal pixels
            rng = ranges[i] if ranges else None
            if rng and rng[0] == -1: return False
            width = info.width >> i
            height = info.height >> i
            pixels = bytes[rng[0]:rng[1]] if rng else bytes
            arrayType = GLbyte * len(pixels)
            glCompressedTexImage2D(GL_TEXTURE_2D, i, internalFormat, width, height, 0, len(pixels), arrayType(*pixels))
            return True

        def texImage2D(info: ITexture, i: int, internalFormat: int, format: int, type: int) -> bool:
            nonlocal pixels
            rng = ranges[i] if ranges else None
            if rng and rng[0] == -1: return False
            width = info.width >> i
            height = info.height >> i
            pixels = bytes[rng[0]:rng[1]] if rng else bytes
            arrayType = GLbyte * len(pixels)
            abc = glTexImage2D(GL_TEXTURE_2D, i, internalFormat, width, height, 0, format, type, arrayType(*pixels))
            return True

        match fmt:
            case glFormat if isinstance(fmt, TextureGLFormat):
                internalFormat = glFormat.value
                if not internalFormat: print('Unsupported texture, using default'); return self.defaultTexture
                for i in range(start, end):
                    if not compressedTexImage2D(info, i, internalFormat): return self.defaultTexture
            case glPixelFormat if isinstance(fmt, tuple):
                internalFormat, format, type = glPixelFormat[0].value, glPixelFormat[1].value, glPixelFormat[2].value
                if not internalFormat: print('Unsupported texture, using default'); return self.defaultTexture
                for i in range(start, numMipMaps):
                    if not texImage2D(info, i, internalFormat, format, type): return self.defaultTexture
            case _: raise Exception(f'Uknown {fmt}')

        if isinstance(info, IDisposable): info.dispose()
        info.end()

        if self.maxTextureMaxAnisotropy >= 4:
            glTexParameter(GL_TEXTURE_2D, GL_TEXTURE_MAX_ANISOTROPY_EXT, self.maxTextureMaxAnisotropy)
            glTexParameter(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR_MIPMAP_LINEAR)
            glTexParameter(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR)
        else:
            glTexParameter(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST)
            glTexParameter(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST)
        glTexParameter(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP if (info.flags & TextureFlags.SUGGEST_CLAMPS.value) != 0 else GL_REPEAT)
        glTexParameter(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP if (info.flags & TextureFlags.SUGGEST_CLAMPT.value) != 0 else GL_REPEAT)
        glBindTexture(GL_TEXTURE_2D, 0)
        return id

    def buildSolidTexture(self, width: int, height: int, pixels: list[float]) -> int:
        pixels = np.array(pixels, dtype = np.float32)
        id = glGenTextures(1)
        glBindTexture(GL_TEXTURE_2D, id)
        glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA32F, width, height, 0, GL_RGBA, GL_FLOAT, pixels)
        # glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA32F, width, height, 0, GL_RGBA, GL_FLOAT, (GLfloat * len(pixels))(*pixels))
        glTexParameter(GL_TEXTURE_2D, GL_TEXTURE_MAX_LEVEL, 0)
        glTexParameter(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST)
        glTexParameter(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST)
        glTexParameter(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT)
        glTexParameter(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT)
        glBindTexture(GL_TEXTURE_2D, 0)
        return id

    def buildNormalMap(self, source: int, strength: float) -> int: raise NotImplementedError()

    def deleteTexture(self, id: int) -> None: glDeleteTexture(id)

# OpenGLMaterialBuilder
class OpenGLMaterialBuilder(MaterialBuilderBase):
    _defaultMaterial: GLRenderMaterial

    def __init__(self, textureManager: TextureManager):
        super().__init__(textureManager)

    @property
    def defaultMaterial(self) -> int: return self._defaultMaterial if self._defaultMaterial else (_defaultMaterial := self._buildAutoMaterial())

    def _buildAutoMaterial(type: int) -> GLRenderMaterial:
        m = GLRenderMaterial(None)
        m.textures['g_tColor'] = self.textureManager.defaultTexture
        m.material.shaderName = 'vrf.error'
        return m

    def buildMaterial(self, key: object) -> GLRenderMaterial:
        match key:
            case s if isinstance(key, IMaterial):
                match s:
                    case m if isinstance(key, IFixedMaterial): return m
                    case p if isinstance(key, IMaterial):
                        for tex in p.textureParams: m.textures[tex.key], _ = self.textureManager.loadTexture(f'{tex.Value}_c')
                        if 'F_SOLID_COLOR' in p.intParams and p.intParams['F_SOLID_COLOR'] == 1:
                            a = p.vectorParams['g_vColorTint']
                            m.textures['g_tColor'] = self.textureManager.buildSolidTexture(1, 1, a[0], a[1], a[2], a[3])
                        if not 'g_tColor' in m.textures: m.textures['g_tColor'] = self.textureManager.defaultTexture

                        # Since our shaders only use g_tColor, we have to find at least one texture to use here
                        if m.textures['g_tColor'] == self.textureManager.defaultTexture:
                            for name in ['g_tColor2', 'g_tColor1', 'g_tColorA', 'g_tColorB', 'g_tColorC']:
                                if name in m.textures:
                                    m.textures['g_tColor'] = m.textures[name]
                                    break

                        # Set default values for scale and positions
                        if not 'g_vTexCoordScale' in p.vectorParams: p.vectorParams['g_vTexCoordScale'] = np.ones(4)
                        if not 'g_vTexCoordOffset' in p.vectorParams: p.vectorParams['g_vTexCoordOffset'] = np.zeros(4)
                        if not 'g_vColorTint' in p.vectorParams: p.vectorParams['g_vColorTint'] = np.ones(4)
                        return m
                    case _: raise Exception(f'Unknown: {s}')
            case _: raise Exception(f'Unknown: {key}')

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
        self.meshBufferCache = GLMeshBufferCache()

    def loadTexture(self, path: str, rng: range = None) -> (int, dict[str, object]): return self.textureManager.loadTexture(path, rng)
    def preloadTexture(self, path: str) -> None: self.textureManager.preloadTexture(path)
    def createObject(self, path: str) -> (object, dict[str, object]): return self.objectManager.createObject(path)
    def preloadObject(self, path: str) -> None: self.objectManager.preloadObject(path)
    def loadShader(self, path: str, args: dict[str, bool] = None) -> Shader: return self.shaderManager.loadShader(path, args)
    def loadFileObject(self, path: str) -> object: return self.source.loadFileObject(path)

    # cache
    _quadIndices: QuadIndexBuffer
    @property
    def quadIndices(self) -> QuadIndexBuffer: return self._quadIndices if self._quadIndices else (_quadIndices := QuadIndexBuffer(65532))
    meshBufferCache: GLMeshBufferCache

# OpenGLPlatform
class OpenGLPlatform:
    def startup() -> bool:
        Platform.platformType = Platform.Type.OpenGL
        Platform.graphicFactory = lambda source: OpenGLGraphic(source)
        Platform.logFunc = lambda a: print(a)
        return True

# OpenGL:startup
Platform.startups.append(OpenGLPlatform.startup)
Platform.startup()