using GameSpec.Valve.Formats.Blocks;
using GameSpec.Valve.Graphics.OpenGL.Scenes;
using OpenStack;
using OpenStack.Graphics.Renderer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using static GameSpec.Valve.Formats.Blocks.DATAEntityLump;

namespace GameSpec.Valve.Formats.OpenGL
{
    public class WorldLoader
    {
        readonly DATAWorld _data;
        readonly IOpenGLGraphic _graphic;

        // Contains metadata that can't be captured by manipulating the scene itself. Returned from Load().
        public class LoadResult
        {
            public HashSet<string> DefaultEnabledLayers { get; } = new HashSet<string>();

            public IDictionary<string, Matrix4x4> CameraMatrices { get; } = new Dictionary<string, Matrix4x4>();

            public Vector3? GlobalLightPosition { get; set; }

            public DATAWorld Skybox { get; set; }
            public float SkyboxScale { get; set; } = 1.0f;
            public Vector3 SkyboxOrigin { get; set; } = Vector3.Zero;
        }

        public WorldLoader(IOpenGLGraphic graphic, DATAWorld data)
        {
            _data = data;
            _graphic = graphic;
        }

        public LoadResult Load(Scene scene)
        {
            var result = new LoadResult();

            // Output is World_t we need to iterate m_worldNodes inside it.
            var worldNodes = _data.GetWorldNodeNames();
            foreach (var worldNode in worldNodes)
                if (worldNode != null)
                {
                    var newResource = _graphic.LoadFileObjectAsync<BinaryPak>($"{worldNode}.vwnod").Result ?? throw new Exception("WTF");
                    var subloader = new WorldNodeLoader(_graphic, (DATAWorldNode)newResource.DATA);
                    subloader.Load(scene);
                }

            foreach (var lumpName in _data.GetEntityLumpNames())
            {
                if (lumpName == null) return result;

                var newResource = _graphic.LoadFileObjectAsync<BinaryPak>(lumpName).Result;
                if (newResource == null) return result;

                var entityLump = (DATAEntityLump)newResource.DATA;
                LoadEntitiesFromLump(scene, result, entityLump, "world_layer_base"); // TODO
            }

            return result;
        }

        void LoadEntitiesFromLump(Scene scene, LoadResult result, DATAEntityLump entityLump, string layerName = null)
        {
            var childEntities = entityLump.GetChildEntityNames();

            foreach (var childEntityName in childEntities)
            {
                var newResource = _graphic.LoadFileObjectAsync<BinaryPak>(childEntityName).Result;
                if (newResource == null) continue;

                var childLump = (DATAEntityLump)newResource.DATA;
                var childName = childLump.Data.Get<string>("m_name");

                LoadEntitiesFromLump(scene, result, childLump, childName);
            }

            var worldEntities = entityLump.GetEntities();

            foreach (var entity in worldEntities)
            {
                var classname = entity.Get<string>("classname");

                if (classname == "info_world_layer")
                {
                    var spawnflags = entity.Get<uint>("spawnflags");
                    var layername = entity.Get<string>("layername");

                    // Visible on spawn flag
                    if ((spawnflags & 1) == 1) result.DefaultEnabledLayers.Add(layername);

                    continue;
                }
                else if (classname == "skybox_reference")
                {
                    var worldgroupid = entity.Get<string>("worldgroupid");
                    var targetmapname = entity.Get<string>("targetmapname");

                    var skyboxPackage = _graphic.LoadFileObjectAsync<BinaryPak>($"maps/{Path.GetFileNameWithoutExtension(targetmapname)}/world.vwrld").Result;
                    if (skyboxPackage != null) result.Skybox = (DATAWorld)skyboxPackage.DATA;
                }

                var scale = entity.Get<string>("scales");
                var position = entity.Get<string>("origin");
                var angles = entity.Get<string>("angles");
                var model = entity.Get<string>("model");
                var skin = entity.Get<string>("skin");
                var particle = entity.Get<string>("effect_name");
                //var animation = entity.GetProperty<string>("defaultanim");
                string animation = null;

                if (scale == null || position == null || angles == null) continue;

                var isGlobalLight = classname == "env_global_light";
                var isCamera =
                    classname == "sky_camera" ||
                    classname == "point_devshot_camera" ||
                    classname == "point_camera";

                var scaleMatrix = Matrix4x4.CreateScale(System.Numerics.Polyfill.ParseVector(scale));

                var positionVector = System.Numerics.Polyfill.ParseVector(position);
                var positionMatrix = Matrix4x4.CreateTranslation(positionVector);

                var pitchYawRoll = System.Numerics.Polyfill.ParseVector(angles);
                var rollMatrix = Matrix4x4.CreateRotationX(OpenTK.MathHelper.DegreesToRadians(pitchYawRoll.Z)); // Roll
                var pitchMatrix = Matrix4x4.CreateRotationY(OpenTK.MathHelper.DegreesToRadians(pitchYawRoll.X)); // Pitch
                var yawMatrix = Matrix4x4.CreateRotationZ(OpenTK.MathHelper.DegreesToRadians(pitchYawRoll.Y)); // Yaw

                var rotationMatrix = rollMatrix * pitchMatrix * yawMatrix;
                var transformationMatrix = scaleMatrix * rotationMatrix * positionMatrix;

                if (classname == "sky_camera")
                {
                    result.SkyboxScale = entity.Get<ulong>("scale");
                    result.SkyboxOrigin = positionVector;
                }

                if (particle != null)
                {
                    var particleResource = _graphic.LoadFileObjectAsync<BinaryPak>(particle).Result;

                    if (particleResource != null)
                    {
                        var particleSystem = (DATAParticleSystem)particleResource.DATA;
                        var origin = new Vector3(positionVector.X, positionVector.Y, positionVector.Z);

                        try
                        {
                            var particleNode = new DebugParticleSceneNode(scene, particleSystem)
                            {
                                Transform = Matrix4x4.CreateTranslation(origin),
                                LayerName = layerName,
                            };
                            scene.Add(particleNode, true);
                        }
                        catch (Exception e)
                        {
                            Console.Error.WriteLine($"Failed to setup particle '{particle}': {e.Message}");
                        }
                    }

                    continue;
                }

                if (isCamera)
                {
                    var name = entity.Get<string>("targetname") ?? string.Empty;
                    var cameraName = string.IsNullOrEmpty(name)
                        ? classname
                        : name;

                    result.CameraMatrices.Add(cameraName, transformationMatrix);
                    continue;
                }
                else if (isGlobalLight) { result.GlobalLightPosition = positionVector; continue; }
                else if (model == null) continue;

                var objColor = Vector4.One;

                // Parse color if present
                var color = entity.Get("rendercolor");

                // HL Alyx has an entity that puts rendercolor as a string instead of color255
                // TODO: Make an enum for these types
                if (color != default && color.Type == EntityFieldType.Color32)
                {
                    var colourBytes = (byte[])color.Data;
                    objColor.X = colourBytes[0] / 255.0f;
                    objColor.Y = colourBytes[1] / 255.0f;
                    objColor.Z = colourBytes[2] / 255.0f;
                    objColor.W = colourBytes[3] / 255.0f;
                }

                var newEntity = _graphic.LoadFileObjectAsync<BinaryPak>(model).Result;
                if (newEntity == null)
                {
                    var errorModelResource = _graphic.LoadFileObjectAsync<BinaryPak>("models/dev/error.vmdl").Result;
                    if (errorModelResource != null)
                    {
                        var errorModel = new DebugModelSceneNode(scene, (IValveModelInfo)errorModelResource.DATA, skin, false)
                        {
                            Transform = transformationMatrix,
                            LayerName = layerName,
                        };
                        scene.Add(errorModel, false);
                    }
                    else Console.WriteLine("Unable to load error.vmdl_c. Did you add \"core/pak_001.dir\" to your game paths?");
                    continue;
                }

                var newModel = (IValveModelInfo)newEntity.DATA;
                var modelNode = new DebugModelSceneNode(scene, newModel, skin, false)
                {
                    Transform = transformationMatrix,
                    Tint = objColor,
                    LayerName = layerName,
                };

                if (animation != default)
                {
                    modelNode.LoadAnimation(animation); // Load only this animation
                    modelNode.SetAnimation(animation);
                }

                var bodyHash = DATAEntityLump.StringToken.Get("body");
                if (entity.Properties.ContainsKey(bodyHash))
                {
                    var groups = modelNode.GetMeshGroups();
                    var body = entity.Properties[bodyHash].Data;
                    var bodyGroup = -1;

                    if (body is ulong bodyGroupLong) bodyGroup = (int)bodyGroupLong;
                    else if (body is string bodyGroupString)
                    {
                        if (!int.TryParse(bodyGroupString, out bodyGroup)) bodyGroup = -1;
                    }

                    modelNode.SetActiveMeshGroups(groups.Skip(bodyGroup).Take(1));
                }

                scene.Add(modelNode, false);
            }
        }
    }
}
