import sys, os
from OpenGL import GL as gl
from PyQt6.QtWidgets import QWidget
from PyQt6.QtCore import Qt
from PyQt6.QtOpenGLWidgets import QOpenGLWidget
from PyQt6.QtOpenGL import QOpenGLBuffer, QOpenGLShader, QOpenGLShaderProgram, QOpenGLTexture
from openstk.opengl_view import OpenGLView

class TextureView(OpenGLView):
    def __init__(self, parent, tab):
        super().__init__()
        self.parent = parent
        self.graphic:IOpenGLGraphic = None
        self.source:ITexture = tab
        self.onProperty()

    def onProperty(self):
        print(self.source)
        pass