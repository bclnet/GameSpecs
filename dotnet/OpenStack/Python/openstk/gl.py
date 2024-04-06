import numpy as np
from OpenGL.GL import *
from openstk.gfx import IOpenGraphicAny

#ref https://pyopengl.sourceforge.net/documentation/manual-3.0/glGetProgram.html

# typedefs
class GLMeshBufferCache: pass
class QuadIndexBuffer: pass

# IOpenGLGraphic
class IOpenGLGraphic(IOpenGraphicAny):
    # cache
    meshBufferCache: GLMeshBufferCache
    quadIndices: QuadIndexBuffer