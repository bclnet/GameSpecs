import os
from openstk.gfx import IFixedMaterial, IParamMaterial
from openstk.gl import IOpenGLGraphic, QuadIndexBuffer, GLMeshBufferCache
from openstk.gl_shader import ShaderDebugLoader
from openstk.gl_render import GLRenderMaterial
from .platform import ObjectBuilderBase, ObjectManager, MaterialBuilderBase, MaterialManager, ShaderBuilderBase, ShaderManager, TextureManager, TextureBuilderBase, Platform

# typedefs
class PakFile: pass
class Shader: pass
class ShaderLoader: pass
class IMaterialManager: pass
class ITexture: pass

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

# OpenGLTextureBuilder
class OpenGLTextureBuilder(TextureBuilderBase):
    _defaultTexture: int = 0

    @property
    def defaultTexture(self) -> int: return self._defaultTexture if self._defaultTexture != 0 else (_defaultTexture := self._buildAutoTexture())

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
        0.9, 0.2, 0.8, 1.0,
        ])

    def buildTexture(self, info: ITexture, range: range = None) -> int:
        id = gl.glGenTexture()
        numMipMaps = math.max(1, info.mipMaps)
        start = range.start or 0 if range else 0
        end = numMipMaps - 1

        gl.glBindTexture(gl.GL_TEXTURE_2D, id)
        gl.glTexParameter(gl.GL_TEXTURE_2D, gl.GL_TEXTURE_MAX_LEVEL, end - start)
        bytes, fmt, ranges = info.begin(Platform.Type.OpenGL)
        # TODO
        info.end()

        if self.maxTextureMaxAnisotropy >= 4:
            gl.glTexParameter(gl.GL_TEXTURE_2D, gl.GL_TEXTURE_MAX_ANISOTROPY_EXT, self.maxTextureMaxAnisotropy)
            gl.glTexParameter(gl.GL_TEXTURE_2D, gl.GL_TEXTURE_MIN_FILTER, gl.GL_LINEAR_MIPMAP_LINEAR)
            gl.glTexParameter(gl.GL_TEXTURE_2D, gl.GL_TEXTURE_MAG_FILTER, gl.GL_LINEAR)
        else:
            gl.glTexParameter(gl.GL_TEXTURE_2D, gl.GL_TEXTURE_MIN_FILTER, gl.GL_NEAREST)
            gl.glTexParameter(gl.GL_TEXTURE_2D, gl.GL_TEXTURE_MAG_FILTER, gl.GL_NEAREST)
        gl.glTexParameter(gl.GL_TEXTURE_2D, gl.GL_TEXTURE_WRAP_S, gl.GL_CLAMP if info.flags & TextureFlags.SUGGEST_CLAMPS else gl.GL_REPEAT)
        gl.glTexParameter(gl.GL_TEXTURE_2D, gl.GL_TEXTURE_WRAP_T, gl.GL_CLAMP if info.flags & TextureFlags.SUGGEST_CLAMPT else gl.GL_REPEAT)
        gl.glBindTexture(gl.GL_TEXTURE_2D, 0)
        return id

    def buildSolidTexture(self, width: int, height: int, pixels: list[float]) -> int:
        id = gl.glGenTexture()
        gl.glBindTexture(gl.GL_TEXTURE_2D, id)
        gl.glTexImage2D(gl.GL_TEXTURE_2D, 0, gl.GL_RGBA32F, width, height, 0, gl.GL_RGBA, gl.GL_FLOAT, pixels)
        gl.glTexParameter(gl.GL_TEXTURE_2D, gl.GL_TEXTURE_MAX_LEVEL, 0)
        gl.glTexParameter(gl.GL_TEXTURE_2D, gl.GL_TEXTURE_MIN_FILTER, gl.GL_NEAREST)
        gl.glTexParameter(gl.GL_TEXTURE_2D, gl.GL_TEXTURE_MAG_FILTER, gl.GL_NEAREST)
        gl.glTexParameter(gl.GL_TEXTURE_2D, gl.GL_TEXTURE_WRAP_S, gl.GL_REPEAT)
        gl.glTexParameter(gl.GL_TEXTURE_2D, gl.GL_TEXTURE_WRAP_T, gl.GL_REPEAT)
        gl.glBindTexture(gl.GL_TEXTURE_2D, 0)
        return id

    def buildNormalMap(source: int, strength: float) -> int: raise NotImplementedError()

    def deleteTexture(id: int) -> None: gl.glDeleteTexture(id)

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

    def buildMaterial(key: object) -> GLRenderMaterial:
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