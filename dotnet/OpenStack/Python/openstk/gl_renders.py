import ctypes, numpy as np
from enum import Enum
from OpenGL.GL import *
from openstk.gfx_render import AABB, IRenderer, RenderPass

sizeof_float = ctypes.sizeof(GLfloat)

# typedefs
class IOpenGLGraphic: pass
class Shader: pass
class Camera: pass

# TextureRenderer
class TextureRenderer(IRenderer):
    graphic: IOpenGLGraphic
    texture: int
    shader: Shader
    quadVao: int
    background: bool
    boundingBox: AABB = AABB(np.array([-1., -1., -1.]), np.array([1., 1., 1.]))

    def __init__(self, graphic: IOpenGLGraphic, texture: int, background: bool = False):
        self.graphic = graphic
        self.texture = texture
        self.shader = graphic.shaderManager.loadPlaneShader('plane')
        self.quadVao = self.setupQuadBuffer()
        self.background = background

    def setupQuadBuffer(self) -> int:
        glUseProgram(self.shader.program)

        # create and bind vao
        vao = glGenVertexArrays(1)
        glBindVertexArray(vao)

        vbo = glGenBuffers(1)
        glBindBuffer(GL_ARRAY_BUFFER, vbo)

        vertices = np.array([
            # position      :normal        :texcoord  :tangent
            -1., -1., +0.,  +0., +0., 1.,  +0., +1.,  +1., +0., +0.,
            -1., +1., +0.,  +0., +0., 1.,  +0., +0.,  +1., +0., +0.,
            +1., -1., +0.,  +0., +0., 1.,  +1., +1.,  +1., +0., +0.,
            +1., +1., +0.,  +0., +0., 1.,  +1., +0.,  +1., +0., +0.
            ], dtype = np.float32)

        # arrayType = GLfloat * len(vertices)
        # glBufferData(GL_ARRAY_BUFFER, len(vertices) * sizeof_float, arrayType(*vertices), GL_STATIC_DRAW)
        # glBufferData(GL_ARRAY_BUFFER, len(vertices) * sizeof_float, (GLfloat * len(vertices))(*vertices), GL_STATIC_DRAW)
        glBufferData(GL_ARRAY_BUFFER, vertices.nbytes, vertices, GL_STATIC_DRAW)
        # print(vertices.nbytes, glGetBufferParameteriv(GL_ARRAY_BUFFER, GL_BUFFER_SIZE))

        glEnableVertexAttribArray(0)

        attributes = [
            ('vPOSITION', 3),
            ('vNORMAL', 3),
            ('vTEXCOORD', 2),
            ('vTANGENT', 3)
            ]
        stride = sizeof_float * sum([x[1] for x in attributes])
        offset = 0
        for name,size in attributes:
            attributeLocation = glGetAttribLocation(self.shader.program, name)
            if attributeLocation > -1:
                glEnableVertexAttribArray(attributeLocation)
                glVertexAttribPointer(attributeLocation, size, GL_FLOAT, False, stride, offset)
            offset += sizeof_float * size

        glBindVertexArray(0) # unbind vao
        return vao

    def render(self, camera: Camera, renderPass: RenderPass) -> None:
        if self.background:
            glClearColor(255, 255, 255, 255)
            glClear(GL_COLOR_BUFFER_BIT)

        glUseProgram(self.shader.program)
        glBindVertexArray(self.quadVao)
        glEnableVertexAttribArray(0)

        if self.texture > -1:
            glActiveTexture(GL_TEXTURE0)
            glBindTexture(GL_TEXTURE_2D, self.texture)

        glDrawArrays(GL_TRIANGLE_STRIP, 0, 4)

        glBindVertexArray(0)
        glUseProgram(0)

    def update(self, frameTime: float) -> None: pass
