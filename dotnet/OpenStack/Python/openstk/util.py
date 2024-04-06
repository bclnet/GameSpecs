import math, numpy as np

def _throw(message: str) -> None:
    raise Exception(message)

#ref https://github.com/microsoft/referencesource/blob/master/System.Numerics/System/Numerics/Vector3.cs
def _np_normalize(vector: np.ndarray) -> np.ndarray:
    norm = np.linalg.norm(vector)
    return vector / norm if norm else np.zeros(vector.shape[0])

#ref https://github.com/microsoft/referencesource/blob/master/System.Numerics/System/Numerics/Matrix4x4.cs
def _np_createScale4x4(scale: float) -> np.ndarray:
    return np.array([
        [scale, 0., 0., 0.],
        [0., scale, 0., 0.],
        [0., 0., scale, 0.],
        [0., 0., 0., 1.]])

#ref https://github.com/microsoft/referencesource/blob/master/System.Numerics/System/Numerics/Matrix4x4.cs
def _np_createLookAt4x4(cameraPosition: np.ndarray, cameraTarget: np.ndarray, cameraUpVector: np.ndarray) -> np.ndarray:
    zaxis = _np_normalize(cameraPosition - cameraTarget)
    xaxis = _np_normalize(np.cross(cameraUpVector, zaxis))
    yaxis = np.cross(zaxis, xaxis)
    return np.array([
        [xaxis[0], yaxis[0], zaxis[0], 0.],
        [xaxis[1], yaxis[1], zaxis[1], 0.],
        [xaxis[2], yaxis[2], zaxis[2], 0.],
        [-np.matmul(xaxis, cameraPosition), -np.matmul(yaxis, cameraPosition), -np.matmul(zaxis, cameraPosition), 1.]])

#ref https://github.com/microsoft/referencesource/blob/master/System.Numerics/System/Numerics/Matrix4x4.cs
def _np_createPerspectiveFieldOfView4x4(fieldOfView: float, aspectRatio: float, nearPlaneDistance: float, farPlaneDistance: float) -> np.ndarray:
    if fieldOfView <= 0. or fieldOfView >= math.pi: raise Exception('fieldOfView')
    if nearPlaneDistance <= 0.: raise Exception('nearPlaneDistance')
    if farPlaneDistance <= 0.: raise Exception('farPlaneDistance')
    if nearPlaneDistance >= farPlaneDistance: raise Exception('nearPlaneDistance')
    yScale = 1. / math.tan(fieldOfView * 0.5)
    xScale = yScale / aspectRatio
    return np.array([
        [xScale, 0., 0., 0.],
        [0., yScale, 0., 0.],
        [0., 0., farPlaneDistance / (nearPlaneDistance - farPlaneDistance), -1.],
        [0., 0., nearPlaneDistance * farPlaneDistance / (nearPlaneDistance - farPlaneDistance), 0.]])
