import math, numpy as np
from openstk.gfx_render import Frustum, IPickingTexture
from .util import _np_normalize, _np_createScale4x4, _np_createLookAt4x4, _np_createPerspectiveFieldOfView4x4

PiOver2 = 1.570796
FOV = 0.7853982 # MathX.PiOver4

class Camera: pass

# Camera
class Camera:
    location: np.ndarray = np.ones(3)
    pitch: float
    yaw: float
    scale: float = 1.
    projectionMatrix: np.ndarray = np.zeros(4)
    cameraViewMatrix: np.ndarray
    viewProjectionMatrix: np.ndarray
    viewFrustum: Frustum = Frustum()
    picker: IPickingTexture = None
    windowSize: np.ndarray
    aspectRatio: float

    def __init__(self):
        self.lookAt(np.zeros(3))

    def recalculateMatrices(self) -> None:
        self.cameraViewMatrix = _np_createScale4x4(self.scale) * _np_createLookAt4x4(self.location, self.location + self.getForwardVector(), np.array([0., 0., 1.]))
        self.viewProjectionMatrix = self.cameraViewMatrix * self.projectionMatrix
        self.viewFrustum.update(self.viewProjectionMatrix)

    def getForwardVector(self) -> np.ndarray: return np.array([(math.cos(self.yaw) * math.cos(self.pitch)), (math.sin(self.yaw) * math.cos(self.pitch)), math.sin(self.pitch)])

    def getRightVector(self) -> np.ndarray: return np.array([math.cos(self.yaw - PiOver2), math.sin(self.yaw - PiOver2), 0.])

    def setViewportSize(self, viewportWidth: int, viewportHeight: int):
        # store window size and aspect ratio
        self.aspectRatio = viewportWidth / viewportHeight
        self.windowSize = np.array([viewportWidth, viewportHeight])

        # calculate projection matrix
        self.projectionMatrix = _np_createPerspectiveFieldOfView4x4(FOV, self.aspectRatio, 1., 40000.)
        self.recalculateMatrices()

        # setup viewport
        self.setViewport(0, 0, viewportWidth, viewportHeight)

        if self.picker: self.picker.resize(viewportWidth, viewportHeight)

    def setViewport(self, x: int, y: int, width: int, height: int) -> None: pass

    def copyFrom(self, fromOther: Camera) -> None:
        self.aspectRatio = fromOther.aspectRatio
        self.windowSize = fromOther.windowSize
        self.location = fromOther.location
        self.pitch = fromOther.pitch
        self.yaw = fromOther.yaw
        self.projectionMatrix = fromOther.projectionMatrix
        self.cameraViewMatrix = fromOther.cameraViewMatrix
        self.viewProjectionMatrix = fromOther.viewProjectionMatrix
        self.viewFrustum.Update(self.viewProjectionMatrix)
    
    def setLocation(self, location: np.ndarray) -> None:
        self.location = location
        self.recalculateMatrices()

    def setLocationPitchYaw(self, location: np.ndarray, pitch: float, yaw: float) -> None:
        self.location = location
        self.pitch = pitch
        self.yaw = yaw
        self.recalculateMatrices()

    def lookAt(self, target: np.ndarray) -> None:
        dir = _np_normalize(target - self.location)
        self.yaw = math.atan2(dir[1], dir[0])
        self.pitch = math.asin(dir[2])
        self.clampRotation()
        self.recalculateMatrices()

    def setFromTransformMatrix(self, matrix: np.ndarray) -> None:
        self.location = matrix.translation

        # extract view direction from view matrix and use it to calculate pitch and yaw
        dir = np.ndarray([matrix[0,0], matrix[0,1], matrix[0,2]])
        self.yaw = math.atan2(dir[1], dir[0])
        self.pitch = math.asin(dir[2])

        self.recalculateMatrices()

    def setScale(self, scale: float) -> None:
        self.scale = scale
        self.recalculateMatrices()

    def tick(self, deltaTime: float) -> None: pass

    # prevent camera from going upside-down
    def clampRotation(self) -> None:
        if self.pitch >= PiOver2: self.pitch = PiOver2 - 0.001
        elif self.pitch <= -PiOver2: self.pitch = -PiOver2 + 0.001
