using GameX.Scenes;
using GameX.Valve.Formats.Blocks;
using GameX.Valve.Graphics.OpenGL.Scenes;
using OpenStack;
using OpenStack.Graphics.Renderer1;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace GameX.Valve.Formats.OpenGL
{
    //was:Renderer/WorldLoader
    public class WorldNodeLoader
    {
        readonly DATAWorldNode Node;
        readonly IOpenGLGraphic Graphic;

        public WorldNodeLoader(IOpenGLGraphic graphic, DATAWorldNode node)
        {
            Node = node;
            Graphic = graphic;
        }

        public void Load(Scene scene)
        {
            var data = Node.Data;

            var worldLayers = data.ContainsKey("m_layerNames") ? data.Get<string[]>("m_layerNames") : Array.Empty<string>();
            var sceneObjectLayerIndices = data.ContainsKey("m_sceneObjectLayerIndices") ? data.GetInt64Array("m_sceneObjectLayerIndices") : null;
            var sceneObjects = data.GetArray("m_sceneObjects");
            var i = 0;

            // Output is WorldNode_t we need to iterate m_sceneObjects inside it
            foreach (var sceneObject in sceneObjects)
            {
                var layerIndex = sceneObjectLayerIndices?[i++] ?? -1;

                // sceneObject is SceneObject_t
                var renderableModel = sceneObject.Get<string>("m_renderableModel");
                var matrix = sceneObject.GetArray("m_vTransform").ToMatrix4x4();

                var tintColorVector = sceneObject.GetVector4("m_vTintColor");
                var tintColor = tintColorVector.W == 0 ? Vector4.One : tintColorVector;

                if (renderableModel != null)
                {
                    var newResource = Graphic.LoadFileObject<Binary_Pak>($"{renderableModel}_c").Result;
                    if (newResource == null) continue;
                    var modelNode = new ModelSceneNode(scene, (IValveModel)newResource.DATA, null, false)
                    {
                        Transform = matrix,
                        Tint = tintColor,
                        LayerName = worldLayers[layerIndex],
                        Name = renderableModel,
                    };
                    scene.Add(modelNode, false);
                }

                var renderable = sceneObject.Get<string>("m_renderable");
                if (renderable != null)
                {
                    var newResource = Graphic.LoadFileObject<Binary_Pak>($"{renderable}_c").Result;
                    if (newResource == null) continue;
                    var meshNode = new MeshSceneNode(scene, new DATAMesh(newResource), 0)
                    {
                        Transform = matrix,
                        Tint = tintColor,
                        LayerName = worldLayers[layerIndex],
                        Name = renderable,
                    };
                    scene.Add(meshNode, false);
                }
            }

            if (!data.ContainsKey("m_aggregateSceneObjects")) return;

            var aggregateSceneObjects = data.GetArray("m_aggregateSceneObjects");
            foreach (var sceneObject in aggregateSceneObjects)
            {
                var renderableModel = sceneObject.Get<string>("m_renderableModel");
                if (renderableModel != null)
                {
                    var newResource = Graphic.LoadFileObject<Binary_Pak>($"{renderableModel}_c").Result;
                    if (newResource == null) continue;

                    var layerIndex = sceneObject.Get<int>("m_nLayer");
                    var modelNode = new ModelSceneNode(scene, (IValveModel)newResource.DATA, null, false)
                    {
                        LayerName = worldLayers[layerIndex],
                        Name = renderableModel,
                    };
                    scene.Add(modelNode, false);
                }
            }
        }
    }
}
