import sys, os, time
from OpenGL.GL import *
from OpenGL.raw.GL.EXT.texture_filter_anisotropic import GL_MAX_TEXTURE_MAX_ANISOTROPY_EXT
from PyQt6.QtWidgets import QWidget
from PyQt6.QtCore import Qt
from PyQt6.QtOpenGLWidgets import QOpenGLWidget
from PyQt6.QtOpenGL import QOpenGLBuffer, QOpenGLShader, QOpenGLShaderProgram, QOpenGLTexture, QOpenGLDebugLogger, QOpenGLDebugMessage
from openstk.gfx import PlatformStats
from openstk.gl_camera import GLDebugCamera

# https://forum.qt.io/topic/137468/a-few-basic-changes-in-pyqt6-and-pyside6-regarding-shader-based-opengl-graphics
# https://github.com/8Observer8/falling-collada-cube-bullet-physics-opengl33-pyqt6/blob/master/main.py

def debuggerMessage(msg: QOpenGLDebugMessage) -> None:
    if msg.severity().value >= QOpenGLDebugMessage.Severity.LowSeverity.value: return
    print(f'OpenGL: {msg.type().name[:-4]}:{msg.severity().name[:-8]} - {msg.message()}')

# OpenGLView
class OpenGLView(QOpenGLWidget):
    def __init__(self):
        super().__init__()
        self.debugger = QOpenGLDebugLogger(self)

    def checkGL(self):
        print(f'OpenGL version: {glGetString(GL_VERSION).decode()}')
        print(f'OpenGL vendor: {glGetString(GL_VENDOR).decode()}')
        if self.debugger.initialize():
            print(f'OpenGL debugger: installed')
            self.debugger.messageLogged.connect(debuggerMessage)
            self.debugger.startLogging()
        print(f'GLSL version: {glGetString(GL_SHADING_LANGUAGE_VERSION).decode()}')

        extensions = {}
        for i in range(glGetInteger(GL_NUM_EXTENSIONS)):
            extension = glGetStringi(GL_EXTENSIONS, i).decode()
            if extension not in extensions: extensions[extension] = None

        if 'GL_EXT_texture_filter_anisotropic' in extensions:
            maxTextureMaxAnisotropy = glGetInteger(GL_MAX_TEXTURE_MAX_ANISOTROPY_EXT)
            PlatformStats.maxTextureMaxAnisotropy = maxTextureMaxAnisotropy
            print(f'MaxTextureMaxAnisotropyExt: {maxTextureMaxAnisotropy}')
        else:
            print(f'GL_EXT_texture_filter_anisotropic is not supported')

    def initializeGL(self):
        super().initializeGL()
        self.checkGL()
        self.camera = GLDebugCamera()
        self.elapsedTime = time.time()
        self.handleResize()
    
    def resizeGL(self, width: int, height: int):
        self.handleResize()

    def enterEvent(self, event):
        self.camera.mouseOverRenderArea = True
        return super().enterEvent(event)

    def leaveEvent(self, event):
        self.camera.mouseOverRenderArea = False
        return super().leaveEvent(event)

    def paintGL(self):
        elapsedTime = time.time()
        self.elapsedTime = elapsedTime - self.elapsedTime
        frameTime = self.elapsedTime / 1000.
        self.elapsedTime = elapsedTime

        self.camera.tick(frameTime)
        # self.camera.handleInput(None, None) #OpenTK.Input.Mouse.GetState(), OpenTK.Input.Keyboard.GetState()

        glClearColor(0.2, 0.3, 0.3, 1.)
        glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT)

        # for paint in self.paints:
        #     paint(frameTime, self.camera)
        # self.update()

    def handleResize(self):
        self.camera.setViewportSize(self.width(), self.height())
        self.recalculatePositions()

    def recalculatePositions(self): pass


# from PyQt6.QtGui import QSurfaceFormat
# fmt = QSurfaceFormat()
# fmt.setVersion(4, 6)
# fmt.setProfile(QSurfaceFormat.OpenGLContextProfile.CoreProfile)
# fmt.setOption(QSurfaceFormat.FormatOption.DebugContext) # | QSurfaceFormat.FormatOption.DeprecatedFunctions)
# self.setFormat(fmt)
# message = QOpenGLDebugMessage.createApplicationMessage(QStringLiteral()'Custom message'))
# self.debugger.logMessage(message)
# glDebugMessageInsert(GL_DEBUG_SOURCE_APPLICATION, GL_DEBUG_TYPE_ERROR, 0, GL_DEBUG_SEVERITY_NOTIFICATION, -1, 'Vary dangerous error')
