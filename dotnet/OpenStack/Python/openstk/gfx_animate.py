import quaternion, numpy as np
from typing import Callable
from enum import Enum

# forwards
class Bone: pass
class FrameCache: pass

# bone
class Bone:
    index: int
    parent: Bone
    children: list[Bone] = []
    name: str
    position: np.ndarray
    angle: quaternion.quaternion
    bindPose: np.ndarray
    inverseBindPose: np.ndarray

    def __init__(self, index: int, name: str, position: np.ndarray, rotation: quaternion.quaternion):
        self.index = index
        self.name = name
        self.position = position
        self.angle = rotation
        # Calculate matrices
        # self.bindPose = Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(position)
        # Matrix4x4.Invert(BindPose, out var inverseBindPose)
        self.inverseBindPose = inverseBindPose

    def setParent(parent: Bone) -> None:
        if self.children.contains(parent):
            self.parent = parent
            parent.children.append(self)

# ISkeleton
class ISkeleton:
    roots: list[Bone]
    bones: list[Bone]

# ChannelAttribute
class ChannelAttribute(Enum):
    Position = 1
    Angle = 2
    Scale = 3
    Unknown = 4

# FrameBone
class FrameBone:
    position: np.ndarray
    angle: quaternion.quaternion
    scale: float

# FrameBone
class Frame:
    bones: list[FrameBone]

    def __init__(self, skeleton: ISkeleton):
        self.bones = [None]*[skeleton.bones.length]
        self.clear(skeleton)

    def setAttribute(bone: int, attribute: ChannelAttribute, data: np.ndarray | quaternion.quaternion | float) -> None:
        match data:
            case p if isinstance(data, np.ndarray):
                match attribute:
                    case ChannelAttribute.Position: self.bones[bone].position = p
#if DEBUG
                    case _: print(f"Unknown frame attribute '{p}' encountered with Vector3 data")
#endif
            case q if isinstance(data, quaternion.quaternion):
                match attribute:
                    case ChannelAttribute.Angle: self.bones[bone].angle = q
#if DEBUG
                    case _: print(f"Unknown frame attribute '{q}' encountered with Quaternion data")
#endif
            case f if isinstance(data, float):
                match attribute:
                    case ChannelAttribute.Scale: self.bones[bone].scale = f
#if DEBUG
                    case _: print(f"Unknown frame attribute '{f}' encountered with float data")
#endif

    def clear(self, skeleton: ISkeleton) -> None:
        for i in range(bones.Length):
            self.bones[i].position = skeleton.bones[i].position
            self.bones[i].angle = skeleton.bones[i].angle
            self.bones[i].scale = 1

# IAnimation
class IAnimation:
    name: str
    fps: float
    frameCount: int
    def decodeFrame(index: int, outFrame: Frame) -> None: pass
    def getAnimationMatrices(frameCache: FrameCache, index: int | float, skeleton: ISkeleton) -> np.ndarray: pass

# FrameCache
class FrameCache:
    @staticmethod
    def frameFactory() -> object: lambda skeleton: Frame(skeleton)
    previousFrame: (int, Frame)
    nextFrame: (int, Frame)
    interpolatedFrame: Frame
    skeleton: ISkeleton

    def __init__(self, skeleton: ISkeleton):
        self.previousFrame = (-1, frameFactory(skeleton))
        self.nextFrame = (-1, frameFactory(skeleton))
        self.interpolatedFrame = frameFactory(skeleton)
        self.skeleton = skeleton
        self.clear()

    def getFrame(anim: IAnimation, time: float) -> Frame:
        # Calculate the index of the current frame
        frameIndex = (time * anim.fps) % anim.frameCount
        t = (time * anim.fps - frameIndex) % 1

        # Get current and next frame
        frame1 = self.getFrame(anim, frameIndex)
        frame2 = self.getFrame(anim, (frameIndex + 1) % anim.frameCount)

        # Interpolate bone positions, angles and scale
        for i in range(frame1.bones.length):
            frame1Bone = frame1.bones[i]
            frame2Bone = frame2.bones[i]
            self.interpolatedFrame.bones[i].position = Vector3.lerp(frame1Bone.position, frame2Bone.position, t)
            self.interpolatedFrame.bones[i].angle = quaternion.slerp(frame1Bone.angle, frame2Bone.angle, t)
            self.interpolatedFrame.bones[i].scale = frame1Bone.scale + (frame2Bone.scale - frame1Bone.scale) * t

        return self.interpolatedFrame

# AnimationController
class AnimationController:
    frameCache: FrameCache
    updateHandler: Callable = lambda a, b: None
    activeAnimation: IAnimation
    time: float
    shouldUpdate: bool
    @property
    def activeAnimation(self) -> IAnimation: return self.activeAnimation
    isPaused: bool
    @property
    def frame(self) -> int:
        return round(self.time * activeAnimation.fps) % activeAnimation.frameCount if self.activeAnimation and self.activeAnimation.frameCount != 0 else 0
    @frame.setter
    def setFrame(self, value: int) -> None:
        if activeAnimation:
            self.time = value / activeAnimation.fps if activeAnimation.fps != 0 else 0.
            self.shouldUpdate = True

    def __init__(self, skeleton: ISkeleton):
        self.frameCache = FrameCache(skeleton)

    def update(self, timeStep: float) -> bool:
        if not self.activeAnimation: return False
        if self.isPaused:
            res = self.shouldUpdate
            self.shouldUpdate = False
            return res
        self.time += timeStep
        self.updateHandler(activeAnimation, self.frame)
        self.shouldUpdate = False
        return True

    def setAnimation(self, animation: IAnimation) -> None:
        self.frameCache.clear()
        self.activeAnimation = animation
        self.time = 0.
        self.updateHandler(activeAnimation, -1)

    def pauseLastFrame(self) -> None:
        self.isPaused = True
        self.frame = 0 if not self.activeAnimation else activeAnimation.frameCount - 1

    def getAnimationMatrices(self, skeleton: ISkeleton) -> np.ndarray:
        return activeAnimation.getAnimationMatrices(self.frameCache, self.frame, skeleton) if self.isPaused else \
            activeAnimation.getAnimationMatrices(self.frameCache, self.time, skeleton)

    def registerUpdateHandler(self, handler: Callable) -> None: self.updateHandler = handler
