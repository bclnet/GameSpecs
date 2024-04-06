import math, numpy as np
from OpenGL.GL import *
from openstk.gfx_camera import Camera

CAMERASPEED = 300 # Per second

# GLCamera
class GLCamera(Camera):
    def setViewport(self, x: int, y: int, width: int, height: int) -> None:
        return glViewport(0, 0, width, height)

# GLDebugCamera
class GLDebugCamera(GLCamera):
    mouseOverRenderArea: bool = False # Set from outside this class by forms code
    mouseDragging: bool = False
    mouseDelta: np.ndarray = [0, 0]
    mousePreviousPosition: np.ndarray = [0, 0]
    keyboardState: object = None #KeyboardState
    mouseState: object = None #MouseState
    scrollWheelDelta: int = 0

    def tick(self, deltaTime: float) -> None:
        # if not self.mouseOverRenderArea: return

        # Use the keyboard state to update position
        # self.handleInputTick(deltaTime)

        # Full width of the screen is a 1 PI (180deg)
        self.yaw -= math.pi * self.mouseDelta[0] / self.windowSize[0]
        self.pitch -= math.pi / self.aspectRatio * self.mouseDelta[1] / self.windowSize[1]
        self.clampRotation()
        self.recalculateMatrices()

    def handleInput(self, mouseState: object, keyboardState: object): # MouseState, KeyboardState
        self.scrollWheelDelta += mouseState.scrollWheelValue - self.mouseState.scrollWheelValue
        self.mouseState = mouseState
        self.keyboardState = keyboardState
        if self.mouseOverRenderArea or mouseState.leftButton == ButtonState.Released:
            self.mouseDragging = False
            self.mouseDelta = 0
            if not self.mouseOverRenderArea: return

        # drag
        if mouseState.leftButton == ButtonState.Pressed:
            if not self.mouseDragging:
                self.mouseDragging = True
                self.mousePreviousPosition = np.array([mouseState[0], mouseState[1]])
            mouseNewCoords = np.array([mouseState[0], mouseState[1]])
            self.mouseDelta[0] = mouseNewCoords[0] - self.mousePreviousPosition[0]
            self.mouseDelta[1] = mouseNewCoords[1] - self.mousePreviousPosition[1]
            self.mousePreviousPosition = mouseNewCoords

    def handleInputTick(self, deltaTime: float):
        speed = CAMERASPEED * deltaTime

        # double speed if shift is pressed
        if self.keyboardState.IsKeyDown(Key.ShiftLeft): speed *= 2
        elif self.keyboardState.IsKeyDown(Key.F): speed *= 10

        if self.keyboardState.IsKeyDown(Key[3]): self.location += self.getForwardVector() * speed
        if self.keyboardState.IsKeyDown(Key.S): self.location -= self.getForwardVector() * speed
        if self.keyboardState.IsKeyDown(Key.D): self.location += self.getRightVector() * speed
        if self.keyboardState.IsKeyDown(Key.A): self.location -= self.getRightVector() * speed
        if self.keyboardState.IsKeyDown(Key[2]): self.location += np.array([0., 0., -speed])
        if self.keyboardState.IsKeyDown(Key.Q): self.location += np.array([0., 0., speed])

        # scroll
        if self.scrollWheelDelta:
            self.location += self.getForwardVector() * self.scrollWheelDelta * speed
            self.scrollWheelDelta = 0