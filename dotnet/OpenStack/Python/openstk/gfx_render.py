import numpy as np
from typing import Callable
from enum import Enum, Flag
from openstk.util import _throw
from .util import _np_normalize

# typedefs
class Camera: pass
class IMaterial: pass
class IModel: pass

# forwards
class AABB: pass
class Frustum: pass
class IVBIB: pass
class Attribute: pass
class RenderSlotType: pass

# Shader
class Shader:
    _getUniformLocation: Callable
    _uniforms: dict[str, int] = {}
    name: str
    program: int
    parameters: dict[str, bool]
    renderModes: list[str]

    def __init__(self, getUniformLocation: Callable, name: str = None, program: int = None, parameters: dict[str, bool] = None, renderModes: list[str] = None):
        self._getUniformLocation = getUniformLocation or _throw('Null')
        self.name = name
        self.program = program
        self.parameters = parameters
        self.renderModes = renderModes
    
    def getUniformLocation(self, name: str) -> int:
        if name in self._uniforms: return self._uniforms[name]
        value = self._getUniformLocation(self.program, name)
        self._uniforms[name] = value
        return value

# IPickingTexture
class IPickingTexture:
    isActive: bool
    debug: bool
    shader: Shader
    debugShader: Shader
    def render() -> None: pass
    def resize(width: int, height: int) -> None: pass
    def finish() -> None: pass

# OnDiskBufferData
class OnDiskBufferData:
    elementCount: int
    elementSizeInBytes : int # stride for vertices. Type for indices
    attributes: list[Attribute] # Vertex attribs. Empty for index buffers
    data: bytes

    class RenderSlotType(Enum):
        RENDER_SLOT_INVALID = -1
        RENDER_SLOT_PER_VERTEX = 0
        RENDER_SLOT_PER_INSTANCE = 1

    class Attribute:
        semanticName: str
        semanticIndex: int
        format: Callable
        offset: int
        slot: int
        slotType: RenderSlotType
        instanceStepRate: int

# IVBIB
class IVBIB:
    vertexBuffers: list[OnDiskBufferData]
    indexBuffers: list[OnDiskBufferData]
    def remapBoneIndices(remapTable: list[int]) -> IVBIB: pass

# AABB
class AABB:
    min: np.ndarray
    max: np.ndarray
    def size(self) -> np.ndarray: return self.max - self.min
    def center(self) -> np.ndarray: return (self.min + self.max) * 0.5

    def __init__(self, min: np.ndarray, max: np.ndarray):
        self.min = min
        self.max = max

    def __str__(self):
        return f'AABB [({self.min[0]},{self.min[1]},{self.min[2]}) -> ({self.max[0]},{self.max[1]},{self.max[2]}))'
    
    def contains(self, point: np.ndarray | AABB) -> bool:
        match point:
            case p if isinstance(point, np.ndarray):
                return p[0] >= self.min[0] and p[0] < self.max[0] and \
                    p[1] >= self.min[1] and p[1] < self.max[1] and \
                    p[2] >= self.min[2] and p[2] < self.max[2]
            case o if isinstance(point, AABB):
                return o.min[0] >= self.min[0] and o.max[0] <= self.max[0] and \
                    o.min[1] >= self.min[1] and o.max[1] <= self.max[1] and \
                    o.min[2] >= self.min[2] and o.max[2] <= self.max[2]
    
    def intersects(self, other: AABB) -> bool:
        return other.max[0] >= self.min[0] and other.min[0] < self.max[0] and \
            other.max[1] >= self.min[1] and other.min[1] < self.max[1] and \
            other.max[2] >= self.min[2] and other.min[2] < self.max[2]
    
    def union(self, other: AABB) -> AABB:
        return AABB(np.min(self.min, other.min), np.max(self.max, other.max))
    
    def translate(self, offset: np.ndarray) -> AABB:
        return AABB(self.min + offset, self.max + offset)

    # Note: Since we're dealing with AABBs here, the resulting AABB is likely to be bigger than the original if rotation
    # and whatnot is involved. This problem compounds with multiple transformations. Therefore, endeavour to premultiply matrices
    # and only use this at the last step.
    def transform(transform: np.ndarray) -> AABB:
        points = [
            Vector4.Transform(Vector4(Min[0], Min[1], Min[2], 1.), transform),
            Vector4.Transform(Vector4(Max[0], Min[1], Min[2], 1.), transform),
            Vector4.Transform(Vector4(Max[0], Max[1], Min[2], 1.), transform),
            Vector4.Transform(Vector4(Min[0], Max[1], Min[2], 1.), transform),
            Vector4.Transform(Vector4(Min[0], Max[1], Max[2], 1.), transform),
            Vector4.Transform(Vector4(Min[0], Min[1], Max[2], 1.), transform),
            Vector4.Transform(Vector4(Max[0], Min[1], Max[2], 1.), transform),
            Vector4.Transform(Vector4(Max[0], Max[1], Max[2], 1.), transform)
            ]
        min = points[0]
        max = points[0]
        for i in range(1, points.Length):
            min = np.min(min, points[i])
            max = np.max(max, points[i])
        return AABB(Vector3(min[0], min[1], min[2]), Vector3(max[0], max[1], max[2]))

# Frustum
class Frustum:
   planes: list[np.ndarray] = [None]*6

   @staticmethod
   def createEmpty() -> Frustum:
      r = Frustum()
      return r

   def update(self, viewProjectionMatrix: np.ndarray) -> None:
      self.planes[0] = _np_normalize(np.array([
         viewProjectionMatrix[0,3] + viewProjectionMatrix[0,0],
         viewProjectionMatrix[1,3] + viewProjectionMatrix[1,0],
         viewProjectionMatrix[2,3] + viewProjectionMatrix[2,0],
         viewProjectionMatrix[3,3] + viewProjectionMatrix[3,0]]))
      self.planes[1] = _np_normalize(np.array([
         viewProjectionMatrix[0,3] - viewProjectionMatrix[0,0],
         viewProjectionMatrix[1,3] - viewProjectionMatrix[1,0],
         viewProjectionMatrix[2,3] - viewProjectionMatrix[2,0],
         viewProjectionMatrix[3,3] - viewProjectionMatrix[3,0]]))
      self.planes[2] = _np_normalize(np.array([
         viewProjectionMatrix[0,3] - viewProjectionMatrix[0,1],
         viewProjectionMatrix[1,3] - viewProjectionMatrix[1,1],
         viewProjectionMatrix[2,3] - viewProjectionMatrix[2,1],
         viewProjectionMatrix[3,3] - viewProjectionMatrix[3,1]]))
      self.planes[3] = _np_normalize(np.array([
         viewProjectionMatrix[0,3] + viewProjectionMatrix[0,1],
         viewProjectionMatrix[1,3] + viewProjectionMatrix[1,1],
         viewProjectionMatrix[2,3] + viewProjectionMatrix[2,1],
         viewProjectionMatrix[3,3] + viewProjectionMatrix[3,1]]))
      self.planes[4] = _np_normalize(np.array([
         viewProjectionMatrix[0,2],
         viewProjectionMatrix[1,2],
         viewProjectionMatrix[2,2],
         viewProjectionMatrix[3,2]]))
      self.planes[5] = _np_normalize(np.array([
         viewProjectionMatrix[0,3] - viewProjectionMatrix[0,2],
         viewProjectionMatrix[1,3] - viewProjectionMatrix[1,2],
         viewProjectionMatrix[2,3] - viewProjectionMatrix[2,2],
         viewProjectionMatrix[3,3] - viewProjectionMatrix[3,2]]))

   def clone() -> Frustum:
      rv = Frustum()
      self.planes.copyTo(rv.planes, 0)
      return rv

   def intersects(box: AABB) -> bool:
      for i in range(self.planes.length):
         closest = np.array(
            box.min[0] if self.planes[i][0] < 0 else box.max[0],
            box.min[1] if self.planes[i][1] < 0 else box.max[1],
            box.min[2] if self.planes[i][2] < 0 else box.max[2])
         if Vector3.Dot(np.array(self.planes[i][0], self.planes[i][1], self.planes[i][2]), closest) + self.planes[i][3] < 0: return False
      return True

# IMesh
class IMesh:
    data: dict[str, object]
    vbib: IVBIB
    def getBounds() -> None: pass
    minBounds: np.ndarray
    maxBounds: np.ndarray

# RenderMaterial
class RenderMaterial:
    material: IMaterial
    textures: dict[str, int] = {}
    isBlended: bool
    isToolsMaterial: bool
    _alphaTestReference: float
    _isAdditiveBlend: bool
    _isRenderBackfaces: bool

    def __init__(self, material: IMaterial):
        self.material = material
        match material:
            case s if isinstance(material, IFixedMaterial):
                pass
            case p if isinstance(material, IParamMaterial):
                # FIX: Valve specific
                if 'F_ALPHA_TEST' in p.intParams and p.intParams['F_ALPHA_TEST'] == 1 and 'g_flAlphaTestReference' in p.floatParams: self._alphaTestReference = p.floatParams['g_flAlphaTestReference']
                self.isToolsMaterial = 'tools.toolsmaterial' in p.intAttributes
                self.isBlended = ('F_TRANSLUCENT' in p.intParams and p.IntParams['F_TRANSLUCENT'] == 1) or 'mapbuilder.water' in p.intAttributes or material.shaderName == 'vr_glass.vfx' or material.shaderName == 'tools_sprite.vfx'
                self._isAdditiveBlend = 'F_ADDITIVE_BLEND' in p.intParams and p.intParams['F_ADDITIVE_BLEND'] == 1
                self._isRenderBackfaces = 'F_RENDER_BACKFACES' in p.intParams and p.intParams['F_RENDER_BACKFACES'] == 1
            case _: raise Exception(f'Unknown {material}')
    def render(shader: Shader) -> None: pass
    def postRender() -> None: pass

# DrawCall
class DrawCall:
    primitiveType: int
    shader: Shader
    baseVertex: int
    startIndex: int
    indexCount: int
    tintColor: np.ndarray
    material: RenderMaterial
    vertexArrayObject: int
    vertexBuffer: (int, int)
    indexType: int
    indexBuffer: (int, int)

    class RenderMeshDrawPrimitiveFlags(Flag):
        None_ = 0x0
        UseShadowFastPath = 0x1
        UseCompressedNormalTangent = 0x2
        IsOccluder = 0x4
        InputLayoutIsNotMatchedToMaterial = 0x8
        HasBakedLightingFromVertexStream = 0x10
        HasBakedLightingFromLightmap = 0x20
        CanBatchWithDynamicShaderConstants = 0x40
        DrawLast = 0x80
        HasPerInstanceBakedLightingData = 0x100

    @staticmethod
    def isCompressedNormalTangent(drawCall: dict[str, object]) -> bool:
        if 'm_bUseCompressedNormalTangent' in drawCall: return bool(drawCall['m_bUseCompressedNormalTangent'])
        if 'm_nFlags' not in drawCall: return False
        flags = drawCall['m_nFlags']
        match flags:
            case s if isinstance(flags, str): return 'MESH_DRAW_FLAGS_USE_COMPRESSED_NORMAL_TANGENT' in s.upper()
            case i if isinstance(flags, int): return i & RenderMeshDrawPrimitiveFlags.UseCompressedNormalTangent != 0
            case _: False

# RenderableMesh
class RenderableMesh:
    boundingBox: AABB
    tint: np.ndarray = np.ones(4)
    drawCallsAll: list[DrawCall] = []
    drawCallsOpaque: list[DrawCall] = []
    drawCallsBlended: list[DrawCall] = []
    animationTexture: int
    animationTextureSize: int
    time: float = 0.
    meshIndex: int
    _mesh: IMesh
    _vbib: IVBIB

    def __init__(self, action: Callable, mesh: IMesh, meshIndex: int, skinMaterials: dict[str, str] = None, model: IModel = None):
        action(self)
        self._mesh = mesh
        self._vbib = model.remapBoneIndices(mesh.vbib, meshIndex) if model else mesh.vbib
        mesh.getBounds()
        self.boundingBox = AABB(mesh.minBounds, mesh.maxBounds)
        self.meshIndex = meshIndex
        self._configureDrawCalls(skinMaterials, True)
    def getSupportedRenderModes() -> list[str]: return None #self.drawCallsAll.SelectMany(drawCall => drawCall.Shader.RenderModes).Distinct()
    def setRenderMode(renderMode: str) -> None: pass
    def setAnimationTexture(texture: int, animationTextureSize: int) -> None:
        self.animationTexture = texture
        self.animationTextureSize = animationTextureSize
    def update(timeStep: float) -> None: self.time += timeStep
    def setSkin(skinMaterials: dict[str, str]) -> None: self._configureDrawCalls(skinMaterials, False)
    def  _configureDrawCalls(skinMaterials: dict[str, str], firstSetup: bool) -> None: pass

# MeshBatchRequest
class MeshBatchRequest:
    transform: np.ndarray
    mesh: RenderableMesh
    call: DrawCall
    distanceFromCamera: float
    nodeId: int
    meshId: int

# RenderPass
class RenderPass(Enum):
    Both = 0,
    Opaque = 1,
    Translucent = 2 # Blended

# IRenderer
class IRenderer:
    boundingBox: AABB
    def render(camera: Camera, renderPass: RenderPass) -> None: pass
    def update(frameTime: float) -> None: pass

# IMeshCollection
class IMeshCollection:
    renderableMeshes: list[RenderableMesh]
