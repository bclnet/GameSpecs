using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace OpenStack.Graphics.Renderer1
{
    //was:Render/Scene
    public class Scene
    {
        public class UpdateContext
        {
            public float Timestep { get; }
            public UpdateContext(float timestep) => Timestep = timestep;
        }

        public class RenderContext
        {
            public Camera Camera { get; set; }
            public Vector3? LightPosition { get; set; }
            public RenderPass RenderPass { get; set; }
            public Shader ReplacementShader { get; set; }
            public bool ShowDebug { get; set; }
        }

        public Camera MainCamera { get; set; }
        public Vector3? LightPosition { get; set; }
        public IOpenGraphic Graphic { get; }
        public Octree<SceneNode> StaticOctree { get; }
        public Octree<SceneNode> DynamicOctree { get; }

        public bool ShowDebug { get; set; } = true;

        public IEnumerable<SceneNode> AllNodes => StaticNodes.Concat(DynamicNodes);

        readonly List<SceneNode> StaticNodes = new List<SceneNode>();
        readonly List<SceneNode> DynamicNodes = new List<SceneNode>();
        readonly Action<List<MeshBatchRequest>, RenderContext> MeshBatchRenderer;

        public Scene(IOpenGraphic graphic, Action<List<MeshBatchRequest>, RenderContext> meshBatchRenderer, float sizeHint = 32768)
        {
            Graphic = graphic ?? throw new ArgumentNullException(nameof(graphic));
            MeshBatchRenderer = meshBatchRenderer ?? throw new ArgumentNullException(nameof(meshBatchRenderer));
            StaticOctree = new Octree<SceneNode>(sizeHint);
            DynamicOctree = new Octree<SceneNode>(sizeHint);
        }

        public void Add(SceneNode node, bool dynamic)
        {
            if (dynamic)
            {
                DynamicNodes.Add(node);
                DynamicOctree.Insert(node, node.BoundingBox);
                node.Id = (uint)DynamicNodes.Count * 2 - 1;
            }
            else
            {
                StaticNodes.Add(node);
                StaticOctree.Insert(node, node.BoundingBox);
                node.Id = (uint)StaticNodes.Count * 2;
            }
        }

        public SceneNode Find(uint id)
        {
            if (id == 0) return null;
            else if (id % 2 == 1)
            {
                var index = ((int)id + 1) / 2 - 1;
                return index >= DynamicNodes.Count ? null : DynamicNodes[index];
            }
            else
            {
                var index = (int)id / 2 - 1;
                return index >= StaticNodes.Count ? null : StaticNodes[index];
            }
        }

        public void Update(float timestep)
        {
            var updateContext = new UpdateContext(timestep);
            foreach (var node in StaticNodes) node.Update(updateContext);
            foreach (var node in DynamicNodes) { var oldBox = node.BoundingBox; node.Update(updateContext); DynamicOctree.Update(node, oldBox, node.BoundingBox); }
        }

        public void RenderWithCamera(Camera camera, Frustum cullFrustum = null)
        {
            var allNodes = StaticOctree.Query(cullFrustum ?? camera.ViewFrustum);
            allNodes.AddRange(DynamicOctree.Query(cullFrustum ?? camera.ViewFrustum));

            // Collect mesh calls
            var opaqueDrawCalls = new List<MeshBatchRequest>();
            var blendedDrawCalls = new List<MeshBatchRequest>();
            var looseNodes = new List<SceneNode>();
            foreach (var node in allNodes)
            {
                if (node is IMeshCollection meshCollection)
                    foreach (var mesh in meshCollection.RenderableMeshes)
                    {
                        foreach (var call in mesh.DrawCallsOpaque)
                            opaqueDrawCalls.Add(new MeshBatchRequest
                            {
                                Transform = node.Transform,
                                Mesh = mesh,
                                Call = call,
                                DistanceFromCamera = (node.BoundingBox.Center - camera.Location).LengthSquared(),
                                NodeId = node.Id,
                                MeshId = (uint)mesh.MeshIndex,
                            });

                        foreach (var call in mesh.DrawCallsBlended)
                            blendedDrawCalls.Add(new MeshBatchRequest
                            {
                                Transform = node.Transform,
                                Mesh = mesh,
                                Call = call,
                                DistanceFromCamera = (node.BoundingBox.Center - camera.Location).LengthSquared(),
                                NodeId = node.Id,
                                MeshId = (uint)mesh.MeshIndex,
                            });
                    }
                else looseNodes.Add(node);
            }

            // Sort loose nodes by distance from camera
            looseNodes.Sort((a, b) =>
            {
                var aLength = (a.BoundingBox.Center - camera.Location).LengthSquared();
                var bLength = (b.BoundingBox.Center - camera.Location).LengthSquared();
                return bLength.CompareTo(aLength);
            });

            // Opaque render pass
            var renderContext = new RenderContext
            {
                Camera = camera,
                LightPosition = LightPosition,
                RenderPass = RenderPass.Opaque,
                ShowDebug = ShowDebug,
            };

            // Blended render pass, back to front for loose nodes
            if (camera.Picker != null)
                if (camera.Picker.IsActive) { camera.Picker.Render(); renderContext.ReplacementShader = camera.Picker.Shader; }
                else if (camera.Picker.Debug) renderContext.ReplacementShader = camera.Picker.DebugShader;
            MeshBatchRenderer(opaqueDrawCalls, renderContext);
            foreach (var node in looseNodes) node.Render(renderContext);
            if (camera.Picker != null && camera.Picker.IsActive)
            {
                camera.Picker.Finish();
                RenderWithCamera(camera, cullFrustum);
            }
        }

        public void SetEnabledLayers(HashSet<string> layers)
        {
            foreach (var renderer in AllNodes) renderer.LayerEnabled = layers.Contains(renderer.LayerName);
            StaticOctree.Clear();
            DynamicOctree.Clear();
            foreach (var node in StaticNodes) if (node.LayerEnabled) StaticOctree.Insert(node, node.BoundingBox);
            foreach (var node in DynamicNodes) if (node.LayerEnabled) DynamicOctree.Insert(node, node.BoundingBox);
        }
    }
}
