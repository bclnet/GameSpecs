import numpy as np
from typing import Callable
from enum import Enum
from OpenGL.GL import *
from openstk.gfx_render import DrawCall, MeshBatchRequest, RenderMaterial, RenderableMesh, IPickingTexture
from openstk.gfx_scene import Scene

# typedefs
class Camera: pass
class Shader: pass
class IVBIB: pass
class IMaterial: pass
class IMesh: pass
class IModel: pass

# forwards
class IOpenGLGraphic: pass
class PickingIntent: pass
class PixelInfo: pass

# GLMeshBuffers
class GLMeshBuffers:
    vertexBuffers: list[bytes]
    indexBuffers: list[bytes]

    class Buffer:
        handle: int
        size: int

    def __init__(self, vbib: IVBIB):
        self.vertexBuffers = [None] * vbib.vertexBuffers.Count
        self.indexBuffers = [None] * vbib.indexBuffers.Count
        for i in range(vbib.vertexBuffers.count):
            self.vertexBuffers[i].handle = glGenBuffer()
            glBindBuffer(GL_ARRAY_BUFFER, VertexBuffers[i].Handle)
            glBufferData(GL_ARRAY_BUFFER, vbib.VertexBuffers[i].ElementCount * vbib.vertexBuffers[i].elementSizeInBytes, vbib.vertexBuffers[i].data, GL_STATIC_DRAW)
            vertexBuffers[i].size = glGetBufferParameter(GL_ARRAY_BUFFER, GL_BUFFER_SIZE)
        for i in range(vbib.indexBuffers.count):
            self.indexBuffers[i].handle = glGenBuffer()
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, self.indexBuffers[i].handle)
            glBufferData(GL_ELEMENT_ARRAY_BUFFER, vbib.indexBuffers[i].elementCount * vbib.indexBuffers[i].elementSizeInBytes, vbib.indexBuffers[i].data, GL_STATIC_DRAW)
            self.indexBuffers[i].size = glGetBufferParameter(GL_ELEMENT_ARRAY_BUFFER, GL_BUFFER_SIZE)

# GLMeshBufferCache
class GLMeshBufferCache:
    class VAOKey:
        vbib: GLMeshBuffers
        shader: Shader
        vertexIndex: int
        indexIndex: int
        baseVertex: int

    def getVertexIndexBuffers(vbib: IVBIB) -> GLMeshBuffers:
        if vbib in self._gpuBuffers: return self._gpuBuffers[vbib]
        else:
            newGpuVbib = GLMeshBuffers(vbib)
            self._gpuBuffers.append(vbib, newGpuVbib)
            return newGpuVbib

# MeshBatchRenderer
class MeshBatchRenderer:
    @staticmethod
    def render(requests: list[MeshBatchRequest], context: Scene.RenderContext) -> None:
            # Opaque: Grouped by material
            if context.renderPass == RenderPass.Both or context.renderPass == RenderPass.Opaque: self.drawBatch(requests, context)
            # Blended: In reverse order
            if context.renderPass == RenderPass.Both or context.enderPass == renderPass.Translucent:
                holder = [MeshBatchRequest()] # Holds the one request we render at a time
                requests.sort(lambda a, b: -a.distanceFromCamera.compareTo(b.distanceFromCamera))
                for request in requests: holder[0] = request; self.drawBatch(holder, context)

    def drawBatch(drawCalls: list[MeshBatchRequest], context: Scene.RenderContext) -> None:
        glEnable(GL_DEPTH_TEST)

        viewProjectionMatrix = context.camera.viewProjectionMatrix
        cameraPosition = context.camera.location
        lightPosition = cameraPosition # (context.LightPosition ?? context.Camera.Location)

        groupedDrawCalls = drawCalls.groupBy(lambda a: a.call.shader) if not context.replacementShader else \
            drawCalls.groupBy(lambda a: context.replacementShader)

        for shaderGroup in groupedDrawCalls:
            shader = shaderGroup.key
            uniformLocationAnimated = shader.getUniformLocation('bAnimated')
            uniformLocationAnimationTexture = shader.getUniformLocation('animationTexture')
            uniformLocationNumBones = shader.getUniformLocation('fNumBones')
            uniformLocationTransform = shader.getUniformLocation('transform')
            uniformLocationTint = shader.getUniformLocation('m_vTintColorSceneObject')
            uniformLocationTintDrawCall = shader.getUniformLocation('m_vTintColorDrawCall')
            uniformLocationTime = shader.getUniformLocation('g_flTime')
            uniformLocationObjectId = shader.getUniformLocation('sceneObjectId')
            uniformLocationMeshId = shader.getUniformLocation('meshId')

            glUseProgram(shader.program)

            glUniform3(shader.getUniformLocation('vLightPosition'), cameraPosition)
            glUniform3(shader.getUniformLocation('vEyePosition'), cameraPosition)
            glUniformMatrix4(shader.getUniformLocation('uProjectionViewMatrix'), False) # ref viewProjectionMatrix

            for materialGroup in shaderGroup.groupBy(lambda a: a.call.material):
                material = materialGroup.key
                if not context.showDebug and material.isToolsMaterial: continue
                material.render(shader)

                for request in materialGroup:
                    transform = request.transform
                    glUniformMatrix4(uniformLocationTransform, False) # ref transformTk
                    if uniformLocationObjectId != 1: glUniform1(uniformLocationObjectId, request.nodeId)
                    if uniformLocationMeshId != 1: glUniform1(uniformLocationMeshId, request.meshId)
                    if uniformLocationTime != 1: glUniform1(uniformLocationTime, request.mesh.time)
                    if uniformLocationAnimated != -1: glUniform1(uniformLocationAnimated, 1. if request.mesh.animationTexture.hasValue else 0.)

                    # push animation texture to the shader (if it supports it)
                    if request.mesh.animationTexture.hasValue:
                        if uniformLocationAnimationTexture != -1: glActiveTexture(GL_TEXTURE0); glBindTexture(GL_TEXTURE_2D, request.mesh.animationTexture.value); glUniform1(uniformLocationAnimationTexture, 0)
                        if uniformLocationNumBones != -1: v = math.max(1, request.mesh.animationTextureSize - 1); glUniform1(uniformLocationNumBones, v)

                    if uniformLocationTint > -1: tint = request.mesh.tint; glUniform4(uniformLocationTint, tint)
                    if uniformLocationTintDrawCall > -1: glUniform3(uniformLocationTintDrawCall, request.call.tintColor)

                    glBindVertexArray(request.call.vertexArrayObject)
                    glDrawElements(request.call.primitiveType, request.call.indexCount, request.call.IndexType, request.call.startIndex)

                material.postRender()

        glDisable(GL_DEPTH_TEST)

# QuadIndexBuffer
class QuadIndexBuffer:
    glHandle: int

    def __init__(self, size: int):
        self.glHandle = glGenBuffer()
        glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, self.glHandle)
        indices = []*size
        for i in range(size / 6):
            indices[(i * 6) + 0] = ((i * 4) + 0)
            indices[(i * 6) + 1] = ((i * 4) + 1)
            indices[(i * 6) + 2] = ((i * 4) + 2)
            indices[(i * 6) + 3] = ((i * 4) + 0)
            indices[(i * 6) + 4] = ((i * 4) + 2)
            indices[(i * 6) + 5] = ((i * 4) + 3)
        glBufferData(GL_ELEMENT_ARRAY_BUFFER, size * 2, indices, GL_STATIC_DRAW)

# GLPickingTexture
class GLPickingTexture(IPickingTexture):
    class PixelInfo:
        objectId: int
        meshId: int
        unused2: int

    class PickingIntent(Enum):
        Select = 1,
        Open = 2

    class PickingRequest:
        activeNextFrame: bool
        cursorPositionX: int
        cursorPositionY: int
        intent: PickingIntent
        def nextFrame(self, x: int, y: int, intent: PickingIntent):
            self.activeNextFrame = True
            self.cursorPositionX = x
            self.cursorPositionY = y
            self.intent = intent

    class PickingResponse:
        intent: PickingIntent
        pixelInfo: PixelInfo
        
    def __init__(self, graphic: IOpenGLGraphic, onPicked: list[Callable]):
        self.shader = graphic.loadShader('vrf.picking', {})
        self.debugShader = graphic.loadShader('vrf.picking', { 'F_DEBUG_PICKER': True })
        # self.onPicked += onPicked
        self.setup()

    def setup(self) -> None:
        fboHandle = glGenFramebuffers(1)
        glBindFramebuffer(GL_FRAMEBUFFER, fboHandle)

        colorHandle = glGenTextures(1)
        glBindTexture(GL_TEXTURE_2D, colorHandle)
        glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA32UI, width, height, 0, GL_RGBA_INTEGER, GL_UNSIGNED_INT, None)
        glTexParameter(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST)
        glTexParameter(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST)
        glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, colorHandle, 0)

        depthHandle = glGenTextures(1)
        glBindTexture(GL_TEXTURE_2D, depthHandle)
        glTexImage2D(GL_TEXTURE_2D, 0, GL_DEPTH_COMPONENT, width, height, 0, GL_DEPTH_COMPONENT, GL_FLOAT, None)
        glFramebufferTexture2D(GL_FRAMEBUFFER, GL_DEPTH_ATTACHMENT, GL_TEXTURE_2D, depthHandle, 0)

        status = glCheckFramebufferStatus(GL_FRAMEBUFFER)
        if status != GL_FRAMEBUFFER_COMPLETE: raise Exception(f'Framebuffer failed to bind with error: {status}')

        glBindTexture(GL_TEXTURE_2D, 0)
        glBindFramebuffer(GL_FRAMEBUFFER, 0)

    def render(self) -> None:
        glBindFramebuffer(GL_DRAW_FRAMEBUFFER, fboHandle)
        glClearColor(0., 0., 0., 0.)
        glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT)

    def finish(self) -> None:
        glBindFramebuffer(dl.GL_DRAW_FRAMEBUFFER, 0)
        if self.request.activeNextFrame:
            self.request.activeNextFrame = False
            pixelInfo = ReadPixelInfo(self.request.cursorPositionX, self.request.cursorPositionY)
            if self.onPicked:
                self.onPicked.Invoke(self, PickingResponse(
                    intent = Request.Intent,
                    pixelInfo = pixelInfo
                    ))

    def resize(self, width: int, height: int) -> None:
        self.width = width
        self.height = height
        glBindTexture(GL_TEXTURE_2D, colorHandle)
        glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA32UI, width, height, 0, GL_RGBA_INTEGER, GL_UNSIGNED_INT, IntPtr.Zero)
        glBindTexture(GL_TEXTURE_2D, depthHandle)
        glTexImage2D(GL_TEXTURE_2D, 0, GL_DEPTH_COMPONENT, width, height, 0, GL_DEPTH_COMPONENT, GL_FLOAT, IntPtr.Zero)

    def readPixelInfo(self, width: int, height: int) -> PixelInfo:
        glFlush()
        glFinish()
        glBindFramebuffer(GL_READ_FRAMEBUFFER, fboHandle)
        glReadBuffer(GL_COLOR_ATTACHMENT0)
        pixelInfo = PixelInfo()
        glReadPixels(width, self.height - height, 1, 1, GL_RGBA_INTEGER, GL_UNSIGNED_INT) # , ref pixelInfo
        glReadBuffer(GL_NONE)
        glBindFramebuffer(GL_READ_FRAMEBUFFER, 0)
        return pixelInfo

    def dispose(self):
        self.onPicked = None
        glDeleteTexture(colorHandle)
        glDeleteTexture(depthHandle)
        glDeleteFramebuffer(fboHandle)

# GLRenderMaterial
class GLRenderMaterial(RenderMaterial):
    def __init__(self, material: IMaterial):
        super().__init__(material)

    def render(self, shader: Shader) -> None:
        # Start at 1, texture unit 0 is reserved for the animation texture
        textureUnit: int = 1
        uniformLocation: int
        for texture in self.textures:
            uniformLocation = shader.getUniformLocation(texture.key)
            if uniformLocation > -1:
                glActiveTexture(GL_TEXTURE0 + textureUnit)
                glBindTexture(GL_TEXTURE_2D, texture.Value)
                glUniform1(uniformLocation, textureUnit)
                textureUnit += 1
        match self.material:
            case p if isinstance(self.material, IParamMaterial):
                for param in p.floatParams:
                    uniformLocation = shader.getUniformLocation(param.key)
                    if uniformLocation > -1: glUniform1(uniformLocation, param.value)
                for param in p.vectorParams:
                    uniformLocation = shader.getUniformLocation(param.key)
                    if uniformLocation > -1: glUniform4(uniformLocation, np.array([param.value[0], param.value[1], param.value[2], param.value[3]]))
        alphaReference: int = shader.getUniformLocation('g_flAlphaTestReference')
        if alphaReference > -1: glUniform1(alphaReference, alphaTestReference)
        if self.isBlended:
            glDepthMask(False)
            glEnable(GL_BLEND)
            glBlendFunc(GL_SRC_ALPHA, GL_ONE if self.isAdditiveBlend else GL_ONE_MINUS_SRC_ALPHA)
        if self.isRenderBackfaces: glDisable(GL_CULL_FACE)

    def postRender(self) -> None:
        if self.isBlended: glDepthMask(True); glDisable(GL_BLEND)
        if self.isRenderBackfaces: glEnable(GL_CULL_FACE)

# GLRenderableMesh
class GLRenderableMesh(RenderableMesh):
    graphic: IOpenGLGraphic

    def __init__(self, graphic: IOpenGLGraphic, mesh: IMesh, meshIndex: int, skinMaterials: dict[str, str] = None, model: IModel = None):
        # super().__init__(lambda t: t.graphic = graphic, mesh, meshIndex, skinMaterials, model)
        pass # SKY

    def setRenderMode(self, renderMode: str):
        for call in (self.drawCallsOpaque + self.drawCallsBlended):
            # recycle old shader parameters that are not render modes since we are scrapping those anyway
            parameters = { x.key:x.value for x in call.shader.parameters if x.key.startsWith('renderMode') }
            if renderMode and call.shader.renderModes.contains(renderMode):
                parameters.append(f'renderMode_{renderMode}', True)
            call.shader = self.graphic.loadShader(call.shader.name, parameters)
            call.vertexArrayObject = self.graphic.meshBufferCache.getVertexArrayObject(self.mesh.vbib, call.shader, call.vertexBuffer.id, call.indexBuffer.id, call.baseVertex)

    def _configureDrawCalls(skinMaterials: dict[str, str], firstSetup: bool) -> None:
        data = mesh.data
        if firstSetup: self.graphic.meshBufferCache.getVertexIndexBuffers(self.vbib)  # This call has side effects because it uploads to gpu

        # Prepare drawcalls
        i = 0
        for sceneObject in data['m_sceneObjects']:
            for objectDrawCall in sceneObject['m_drawCalls']:
                materialName = objectDrawCall['m_material'] or objectDrawCall['m_pMaterial']
                if skinMaterials and materialName in skinMaterials: materialName = skinMaterials[materialName]

                material, _ = self.graphic.materialManager.loadMaterial(f'{materialName}_c')
                isOverlay = isinstance(material.material, IParamMaterial) and 'F_OVERLAY' in material.material.intParams

                # Ignore overlays for now
                if isOverlay: continue

                shaderArgs: dict[str, bool] = {}
                if drawCall.isCompressedNormalTangent(objectDrawCall): shaderArgs['fulltangent'] = False

                if firstSetup:
                    # TODO: Don't pass around so much shit
                    drawCall = self.createDrawCall(objectDrawCall, shaderArgs, material)
                    self.drawCallsAll.append(drawCall)
                    if drawCall.material.isBlended: self.drawCallsBlended.append(drawCall)
                    else: self.drawCallsOpaque.append(drawCall)
                    continue

                self.setupDrawCallMaterial(self.drawCallsAll[i], shaderArgs, material); i += 1

    def _createDrawCall(objectDrawCall: dict[str, object], shaderArgs: dict[str, bool], material: GLRenderMaterial) -> DrawCall:
        drawCall = DrawCall()
        primitiveType = objectDrawCall['m_nPrimitiveType']
        match primitiveType:
            case i if isinstance(primitiveType, int):
                if i == RenderPrimitiveType.RENDER_PRIM_TRIANGLES: drawCall.primitiveType = GL_TRIANGLES
            case s if isinstance(primitiveType, str):
                if s == 'RENDER_PRIM_TRIANGLES': drawCall.primitiveType = GL_TRIANGLES
        if drawCall.primitiveType != GL_TRIANGLES: raise Exception(f'Unknown PrimitiveType in drawCall! {primitiveType})')

        self.setupDrawCallMaterial(drawCall, shaderArgs, material)

        indexBufferObject = objectDrawCall['m_indexBuffer']
        drawCall.indexBuffer = (indexBufferObject['m_hBuffer'], indexBufferObject['m_nBindOffsetBytes'])

        vertexElementSize = self.vbib.vertexBuffers[drawCall.vertexBuffer.id].elementSizeInBytes
        drawCall.baseVertex = objectDrawCall['m_nBaseVertex'] * vertexElementSize

        indexElementSize = self.vbib.indexBuffers[drawCall.indexBuffer.id].elementSizeInBytes
        drawCall.startIndex = objectDrawCall['m_nStartIndex'] * indexElementSize
        drawCall.indexCount = objectDrawCall['m_nIndexCount']

        if 'm_vTintColor' in objectDrawCall: drawCall.tintColor = objectDrawCall['m_vTintColor']

        if indexElementSize == 2: drawCall.indexType = GL_UNSIGNED_SHORT; # shopkeeper_vr
        elif indexElementSize == 4: drawCall.indexType = GL_UNSIGNED_INT; # glados
        else: raise Exception(f'Unsupported index type {indexElementSize}')

        vertexBuffer = objectDrawCall['m_vertexBuffers'][0]
        drawCall.vertexBuffer = (vertexBuffer['m_hBuffer'], vertexBuffer['m_nBindOffsetBytes'])
        drawCall.vertexArrayObject = self.graphic.meshBufferCache.getVertexArrayObject(self.vbib, drawCall.shader, drawCall.vertexBuffer.id, drawCall.indexBuffer.id, drawCall.baseVertex)
        return drawCall

    def _setupDrawCallMaterial(drawCall: DrawCall, shaderArgs: dict[str, bool], material: RenderMaterial) -> None:
        drawCall.material = material

        # add shader parameters from material to the shader parameters from the draw call
        combinedShaderArgs = { x.key:x.value for x in shaderArgs + material.material.getShaderArgs() }

        # load shader
        drawCall.shader = self.graphic.loadShader(drawCall.material.material.shaderName, combinedShaderArgs)

        # bind and validate shader
        glUseProgram(drawCall.shader.program)

        if 'g_tTintMask' not in drawCall.material.textures: drawCall.material.textures.append('g_tTintMask', self.graphic.textureManager.buildSolidTexture(1, 1, 1., 1., 1., 1.))
        if 'g_tNormal' not in drawCall.material.textures: drawCall.material.textures.append('g_tNormal', self.graphic.textureManager.buildSolidTexture(1, 1, 0.5, 1, 0.5, 1))