import sys, os
from OpenGL import GL as gl
from PyQt6.QtWidgets import QWidget
from PyQt6.QtCore import Qt
from PyQt6.QtOpenGLWidgets import QOpenGLWidget
from PyQt6.QtOpenGL import QOpenGLBuffer, QOpenGLShader, QOpenGLShaderProgram, QOpenGLTexture
from openstk.gfx_texture import ITexture
from openstk.gl_view import OpenGLView
# from openstk.gl_render import TextureRenderer

class TextureRenderer: pass

# typedefs
class IOpenGLGraphic: pass

# TextureView
class TextureView(OpenGLView):
    background: bool
    range: range = range(0)
    renderers: list[TextureRenderer] = []

    def __init__(self, parent, tab):
        super().__init__()
        self.parent = parent
        self.graphic: IOpenGLGraphic = parent.graphic
        self.source: ITexture = tab
        self.onProperty()

    def onProperty(self):
        if not self.graphic or not self.source: return
        graphic = self.graphic
        source = self.source if isinstance(self.source, ITexture) else None
        if not source: return

        self.handleResize()
        self.camera.setLocation(np.array([200., 200., 200.]))
        self.camera.lookAt(np.zeros(3))

        graphic.textureManager.deleteTexture(source)
        texture, _ = graphic.textureManager.loadTexture(source, range)
        self.renderers.clear()
        # self.renderers.append(renderer := TextureRenderer(graphic, texture))
        # renderer.background = background

    def paintGL(self):
        super().paintGL()

        # self.handleInput(Keyboard.GetState())
        for renderer in self.renderers:
            print('HERE')
            renderer.render(self.camera, RenderPass.Both)
