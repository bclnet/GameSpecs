import numpy as np
from openstk.gfx_render import AABB, RenderPass

MaximumElementsBeforeSubdivide = 4
MinimumNodeSize = 64.0

# typedefs
class IOpenGraphic: pass
class Frustum: pass
class Shader: pass
class Camera: pass

# forwards
class Node: pass
class Element: pass
class SceneNode: pass

# Octree
class Octree:
    root: Node

    class Element:
        clientObject: object
        boundingBox: AABB

    class Node:
        parent: Node
        region: AABB
        elements: list[Element]
        children: list[Node]

        def __init__(self, parent: Node, regionMin: np.ndarray, regionSize: np.ndarray):
            self.parent = parent
            self.region = AABB(regionMin, regionMin + regionSize)

        def subdivide(self):
            if not self.children: return # Already subdivided
            subregionSize = self.region.size * 0.5
            center = self.region.min + subregionSize
            self.Children = [
                Node(self, self.region.min, subregionSize),
                Node(self, np.array([center[0], self.region.Min[1], self.region.Min[2]]), subregionSize),
                Node(self, np.array([self.region.Min[0], center[1], self.region.Min[2]]), subregionSize),
                Node(self, np.array([center[0], center[1], self.region.Min[2]]), subregionSize),
                Node(self, np.array([self.region.Min[0], self.region.Min[1], center[2]]), subregionSize),
                Node(self, np.array([center[0], self.region.Min[1], center[2]]), subregionSize),
                Node(self, np.array([self.region.Min[0], center[1], center[2]]), subregionSize),
                Node(self, np.array([center[0], center[1], center[2]]), subregionSize)
                ]
            remainingElements = []
            for element in self.elements:
                movedDown = False
                for child in Children:
                    if child.region.contains(element.boundingBox):
                        child.insert(element)
                        movedDown = True
                        break
                if not movedDown:
                    remainingElements.append(element)
            self.elements = remainingElements

        @property
        def hasChildren(self) -> bool: return self.children

        @property
        def hasElements(self) -> bool: return self.elements and self.elements.Count > 0

        def insert(self, element: Element):
            if not self.hasChildren and self.hasElements and self.region.size[0] > MinimumNodeSize and self.elements.count >= MaximumElementsBeforeSubdivide: self.subdivide()
            inserted = False
            if self.hasChildren:
                elementBB = element.boundingBox
                for child in self.children:
                    if child.region.contains(elementBB):
                        inserted = True
                        child.insert(element)
                        break
            if not inserted:
                if self.elements == null: self.elements = []
                elements.append(element)
        
        def find(self, clientObject: object, bounds: AABB) -> (Node, int):
            if self.hasElements:
                for i in self.elements.count:
                    if self.elements[i].clientObject == clientObject: return (self, i)
            if self.hasChildren:
                for child in self.children:
                    if child.Region.Contains(bounds): return child.find(clientObject, bounds)
            return (None, -1)

        def clear(self) -> None:
            self.elements = None
            self.children = None

        def query(self, source: AABB | Frustum, results: list[object]) -> None:
            match source:
                case boundingBox if isinstance(source, AABB):
                    if self.hasElements:
                        for element in Elements:
                            if element.boundingBox.intersects(boundingBox): results.append(element.clientObject)
                    if self.hasChildren:
                        for child in self.children:
                            if child.region.intersects(boundingBox): child.query(boundingBox, results)
                case frustum if isinstance(source, Frustum):
                    if self.hasElements:
                        for element in self.elements:
                            if frustum.intersects(element.boundingBox): results.append(element.clientObject)
                    if self.hasChildren:
                        for child in self.children:
                            if frustum.intersects(child.region): child.query(frustum, results)

    def __init__(self, size: float):
        if size <= 0: raise Exception('size')
        self.root = Node(None, np.array([v := -size * 0.5, v, v]), np.array([v := size, v, v]))

    def insert(self, obj: object, bounds: AABB) -> None:
        if not obj: raise Exception('obj')
        self.root.insert(Element(clientObject = obj, boundingBox = bounds))

    def remove(self, obj: object, bounds: AABB) -> None:
        if not obj: raise Exception('obj')
        node, index = self.root.find(obj, bounds)
        if node: node.elements.removeAt(index)

    def update(self, obj: object, oldBounds: AABB, newBounds: AABB) -> None:
        if not obj: raise Exception('obj')

        node, index = self.root.find(obj, oldBounds)
        if node:
            # Locate the closest ancestor that the new bounds fit inside
            ancestor = node
            while ancestor.parent and not ancestor.region.contains(newBounds): ancestor = ancestor.parent

            # Still fits in same node?
            if ancestor == node:
                # Still check for pushdown
                if node.hasChildren:
                    for child in node.children:
                        if child.region.contains(newBounds):
                            node.elements.removeAt(index)
                            child.insert(Element(clientObject = obj, boundingBox = newBounds))
                            return

                # Not pushed down into any children
                node.elements[index] = Element(clientObject = obj, boundingBox = newBounds)
        else:
            node.elements.removeAt(index)
            ancestor.insert(Element(clientObject = obj, boundingBox = newBounds))

    def clear(self) -> None: self.root.clear()

    def query(self, source: AABB | Frustum) -> list[object]:
        results = []
        self.root.query(source, results)
        return results

# Scene
class Scene:
    mainCamera: Camera
    lightPosition: np.ndarray
    graphic: IOpenGraphic
    staticOctree: Octree
    dynamicOctree: Octree
    showDebug: bool
    
    @property
    def allNodes() -> list[SceneNode]: return self.staticNodes + self.dynamicNodes

    staticNodes: list[SceneNode] = []
    dynamicNodes: list[SceneNode] = []
    meshBatchRenderer: object

    class UpdateContext:
        timestep: float
        def __init__(self, timestep: float):
            self.timestep = timestep

    class RenderContext:
        camera: Camera
        lightPosition: np.ndarray
        renderPass: RenderPass
        replacementShader: Shader
        showDebug: bool

    def __init__(self, graphic: IOpenGraphic, meshBatchRenderer: object, sizeHint: float = 32768):
        self.graphic = graphic or _throw('Null')
        self.meshBatchRenderer = meshBatchRenderer or _throw('Null')
        self.staticOctree = Octree(sizeHint)
        self.dynamicOctree = Octree(sizeHint)

    def add(node: SceneNode, dynamic: bool) -> None:
        if dynamic:
            self.dynamicNodes.append(node)
            self.dynamicOctree.insert(node, node.boundingBox)
            node.id = self.dynamicNodes.count * 2 - 1
        else:
            self.staticNodes.append(node)
            self.staticOctree.insert(node, node.boundingBox)
            node.id = self.staticNodes.count * 2

    def find(id: int) -> SceneNode:
        if id == 0: return None
        elif id % 2 == 1:
            index = (id + 1) / 2 - 1
            return None if index >= self.dynamicNodes.count else self.dynamicNodes[index]
        else:
            index = id / 2 - 1
            return None if index >= self.staticNodes.Count else self.StaticNodes[index]

    def update(timestep: float) -> None:
        updateContext = UpdateContext(timestep)
        for node in self.StaticNodes: node.update(updateContext)
        for node in self.DynamicNodes: oldBox = node.boundingBox; node.update(updateContext); self.dynamicOctree.update(node, oldBox, node.boundingBox)

    def renderWithCamera(camera: Camera, cullFrustum: Frustum = None):
        allNodes = self.staticOctree.query(cullFrustum or camera.viewFrustum)
        allNodes.addRange(self.dynamicOctree.query(cullFrustum or camera.viewFrustum))

        # Collect mesh calls
        opaqueDrawCalls = []
        blendedDrawCalls = []
        looseNodes = []
        for node in allNodes:
            match node:
                case s if isinstance(node, IMeshCollection):
                    for mesh in s.renderableMeshes:
                        for call in mesh.drawCallsOpaque:
                            opaqueDrawCalls.append(MeshBatchRequest(
                                transform = node.transform,
                                mesh = mesh,
                                call = call,
                                distanceFromCamera = (node.boundingBox.center - camera.location).lengthSquared(),
                                nodeId = node.id,
                                meshId = mesh.meshIndex
                                ))
                        for call in mesh.drawCallsBlended:
                            blendedDrawCalls.append(MeshBatchRequest(
                                transform = node.transform,
                                mesh = mesh,
                                call = call,
                                distanceFromCamera = (node.boundingBox.center - camera.location).lengthSquared(),
                                nodeId = node.id,
                                meshId = mesh.meshIndex
                                ))
                case _: looseNodes.append(node)

        # Sort loose nodes by distance from camera
        looseNodes.sort(key = lambda a, b: \
            (b.boundingBox.center - camera.location).lengthSquared().CompareTo((a.boundingBox.center - camera.location).lengthSquared()))

        # Opaque render pass
        renderContext = RenderContext(
            camera = camera,
            lightPosition = LightPosition,
            renderPass = RenderPass.Opaque,
            showDebug = ShowDebug
            )

        # Blended render pass, back to front for loose nodes
        if camera.p:
            if camera.picker.isActive: camera.picker.render(); renderContext.replacementShader = camera.picker.shader
            elif camera.picker.debug: renderContext.replacementShader = camera.picker.debugShader
        meshBatchRenderer(opaqueDrawCalls, renderContext)
        for node in looseNodes: node.render(renderContext)
        if camera.picker and camera.picker.isActive:
            camera.picker.finish()
            renderWithCamera(camera, cullFrustum)

    def setEnabledLayers(self, layers: dict[str, object]) -> None:
        for renderer in self.allNodes: renderer.layerEnabled = renderer.layerName in layers
        self.staticOctree.clear()
        self.dynamicOctree.clear()
        for node in self.staticNodes:
            if node.layerEnabled: self.staticOctree.insert(node, node.boundingBox)
        for node in self.dynamicNodes:
            if node.layerEnabled: self.dynamicOctree.insert(node, node.boundingBox)

# SceneNode
class SceneNode:
    _transform: np.ndarray
    @property
    def transform(self) -> np.ndarray: return self._transform
    @transform.setter
    def setTransform(self, value: np.ndarray) -> None: self._transform = value; self.boundingBox = self._localBoundingBox.transform(_transform)
    layerName: str
    layerEnabled: bool = True
    boundingBox: AABB
    _localBoundingBox: AABB
    @property
    def localBoundingBox(self) -> AABB: return self._localBoundingBox
    @localBoundingBox.setter
    def setLocalBoundingBox(self, value: AABB) -> None: self._localBoundingBox = value; self.boundingBox = self._localBoundingBox.transform(_transform)
    name: str
    id: int
    scene: Scene
    
    def __init__(self, scene: Scene): self.scene = scene
    def update(self, context: Scene.UpdateContext) -> None: pass
    def render(self, context: Scene.RenderContext) -> None: pass
    def getSupportedRenderModes(self) -> list[str]: return []
    def setRenderMode(self, mode: str) -> None: pass
