using GameSpec.Graphics.Scenes;
using GameSpec.Valve.Formats.Blocks;
using GameSpec.Valve.Graphics.OpenGL.Scenes;
using OpenStack;
using OpenStack.Graphics.Renderer;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace GameSpec.Valve.Formats.OpenGL
{
    public class WorldNodeLoader
    {
        readonly DATAWorldNode _node;
        readonly IOpenGLGraphic _graphic;

        public WorldNodeLoader(IOpenGLGraphic graphic, DATAWorldNode node)
        {
            _node = node;
            _graphic = graphic;
        }

        public void Load(Scene scene)
        {
            var data = _node.Data;

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

                var tintColorWrongVector = sceneObject.GetVector4("m_vTintColor");
                var tintColor = tintColorWrongVector.W == 0
                    ? Vector4.One // Ignoring tintColor, it will fuck things up.
                    : new Vector4(tintColorWrongVector.X, tintColorWrongVector.Y, tintColorWrongVector.Z, tintColorWrongVector.W);

                if (renderableModel != null)
                {
                    var newResource = _graphic.LoadFileObjectAsync<BinaryPak>(renderableModel).Result;
                    if (newResource == null) continue;
                    var modelNode = new DebugModelSceneNode(scene, (IValveModelInfo)newResource.DATA, null, false)
                    {
                        Transform = matrix,
                        Tint = tintColor,
                        LayerName = worldLayers[layerIndex],
                    };
                    scene.Add(modelNode, false);
                }

                var renderable = sceneObject.Get<string>("m_renderable");
                if (renderable != null)
                {
                    var newResource = _graphic.LoadFileObjectAsync<BinaryPak>(renderable).Result;
                    if (newResource == null) continue;
                    var meshNode = new DebugMeshSceneNode(scene, new DATAMesh(newResource))
                    {
                        Transform = matrix,
                        Tint = tintColor,
                        LayerName = worldLayers[layerIndex],
                    };
                    scene.Add(meshNode, false);
                }
            }
        }
    }
}
