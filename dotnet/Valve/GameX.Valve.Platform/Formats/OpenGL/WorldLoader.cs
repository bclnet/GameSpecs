using GameX.Valve.Formats.Blocks;
using GameX.Valve.Formats.Extras;
using GameX.Valve.Graphics.OpenGL.Scenes;
using OpenStack;
using OpenStack.Graphics.Renderer1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using static GameX.Valve.Formats.Blocks.DATAEntityLump;
using static System.Numerics.Polyfill;

namespace GameX.Valve.Formats.OpenGL
{
    //was:Renderer/WorldLoader
    public class WorldLoader
    {
        readonly DATAWorld World;
        readonly IOpenGLGraphic Graphic;

        // Contains metadata that can't be captured by manipulating the scene itself. Returned from Load().
        public class LoadResult
        {
            public readonly HashSet<string> DefaultEnabledLayers = new HashSet<string>();
            public readonly IDictionary<string, Matrix4x4> CameraMatrices = new Dictionary<string, Matrix4x4>();
            public Vector3? GlobalLightPosition;
            public DATAWorld Skybox;
            public float SkyboxScale = 1.0f;
            public Vector3 SkyboxOrigin = Vector3.Zero;
        }

        public WorldLoader(IOpenGLGraphic graphic, DATAWorld world)
        {
            World = world;
            Graphic = graphic;
        }

        public LoadResult Load(Scene scene)
        {
            var result = new LoadResult();
            result.DefaultEnabledLayers.Add("Entities");

            // Output is World_t we need to iterate m_worldNodes inside it.
            foreach (var worldNode in World.GetWorldNodeNames())
                if (worldNode != null)
                {
                    var newResource = Graphic.LoadFileObject<Binary_Pak>($"{worldNode}.vwnod_c").Result;
                    if (newResource == null) continue;
                    var subloader = new WorldNodeLoader(Graphic, (DATAWorldNode)newResource.DATA);
                    subloader.Load(scene);
                }

            foreach (var lumpName in World.GetEntityLumpNames())
            {
                if (lumpName == null) continue;

                var newResource = Graphic.LoadFileObject<Binary_Pak>("{lumpName}_c").Result;
                if (newResource == null) continue;

                var entityLump = (DATAEntityLump)newResource.DATA;
                LoadEntitiesFromLump(scene, result, entityLump, "world_layer_base");
            }
            return result;
        }

        void LoadEntitiesFromLump(Scene scene, LoadResult result, DATAEntityLump entityLump, string layerName = null)
        {
            foreach (var childEntityName in entityLump.GetChildEntityNames())
            {
                var newResource = Graphic.LoadFileObject<Binary_Pak>(childEntityName).Result;
                if (newResource == null) continue;

                var childLump = (DATAEntityLump)newResource.DATA;
                var childName = childLump.Data.Get<string>("m_name");

                LoadEntitiesFromLump(scene, result, childLump, childName);
            }

            foreach (var entity in entityLump.GetEntities())
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

                    var skyboxWorldPath = $"maps/{Path.GetFileNameWithoutExtension(targetmapname)}/world.vwrld_c";
                    var skyboxPackage = Graphic.LoadFileObject<Binary_Pak>(skyboxWorldPath).Result;
                    if (skyboxPackage != null) result.Skybox = (DATAWorld)skyboxPackage.DATA;
                }

                var scale = entity.Get<string>("scales");
                var position = entity.Get<string>("origin");
                var angles = entity.Get<string>("angles");
                var model = entity.Get<string>("model");
                var skin = entity.Get<string>("skin");
                var particle = entity.Get<string>("effect_name");
                var animation = entity.Get<string>("defaultanim");
                if (scale == null || position == null || angles == null) continue;

                var isGlobalLight = classname == "env_global_light" || classname == "light_environment";
                var isCamera = classname == "sky_camera" || classname == "point_devshot_camera" || classname == "point_camera";
                var isTrigger = classname.Contains("trigger", StringComparison.InvariantCulture) || classname == "post_processing_volume";

                var origin = ParseVector3(position);
                var transformationMatrix = ConvertToTransformationMatrix(scale, position, angles);

                if (classname == "sky_camera")
                {
                    result.SkyboxScale = entity.Get<ulong>("scale");
                    result.SkyboxOrigin = origin;
                }

                if (particle != null)
                {
                    var particleResource = Graphic.LoadFileObject<Binary_Pak>(particle).Result;
                    if (particleResource != null)
                    {
                        var particleSystem = (DATAParticleSystem)particleResource.DATA;
                        try
                        {
                            var particleNode = new ParticleSceneNode(scene, particleSystem)
                            {
                                Transform = Matrix4x4.CreateTranslation((Vector3)origin),
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
                    var cameraName = string.IsNullOrEmpty(name) ? classname : name;
                    result.CameraMatrices.Add(cameraName, transformationMatrix);
                    continue;
                }
                else if (isGlobalLight) { result.GlobalLightPosition = origin; continue; }
                else if (model == null) continue;

                var objColor = Vector4.One;

                // Parse color if present
                var color = entity.Get("rendercolor");

                // HL Alyx has an entity that puts rendercolor as a string instead of color255
                if (color != default && color.Type == EntityFieldType.Color32)
                {
                    var bytes = (byte[])color.Data;
                    objColor.X = bytes[0] / 255.0f;
                    objColor.Y = bytes[1] / 255.0f;
                    objColor.Z = bytes[2] / 255.0f;
                    objColor.W = bytes[3] / 255.0f;
                }

                if (!isTrigger && model == null)
                {
                    AddToolModel(scene, classname, transformationMatrix, origin);
                    continue;
                }

                var newEntity = Graphic.LoadFileObject<Binary_Pak>($"{model}_c").Result;
                if (newEntity == null)
                {
                    var errorModelResource = Graphic.LoadFileObject<Binary_Pak>("models/dev/error.vmdl_c").Result;
                    if (errorModelResource != null)
                    {
                        var errorModel = new ModelSceneNode(scene, (IValveModel)errorModelResource.DATA, skin, false)
                        {
                            Transform = transformationMatrix,
                            LayerName = layerName,
                        };
                        scene.Add(errorModel, false);
                    }
                    else Console.WriteLine("Unable to load error.vmdl_c. Did you add \"core/pak_001.dir\" to your game paths?");
                    continue;
                }

                var newModel = (IValveModel)newEntity.DATA;
                var modelNode = new ModelSceneNode(scene, newModel, skin, false)
                {
                    Transform = transformationMatrix,
                    Tint = objColor,
                    LayerName = layerName,
                    Name = model,
                };

                if (animation != default)
                {
                    modelNode.LoadAnimations(); // Load only this animation
                    modelNode.SetAnimation(animation);
                    if (entity.Get<bool>("holdanimation")) modelNode.AnimationController.PauseLastFrame();
                }

                var bodyHash = StringToken.Get("body");
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

                var phys = newModel.GetEmbeddedPhys();
                if (phys == null)
                {
                    var refPhysicsPaths = newModel.GetReferencedPhysNames();
                    if (refPhysicsPaths.Any())
                    {
                        var newResource = Graphic.LoadFileObject<Binary_Pak>($"{refPhysicsPaths.First()}_c").Result;
                        if (newResource != null) phys = (DATAPhysAggregateData)newResource.DATA;
                    }
                }

                if (phys != null)
                {
                    var physSceneNode = new PhysSceneNode(scene, phys)
                    {
                        Transform = transformationMatrix,
                        IsTrigger = isTrigger,
                        LayerName = layerName
                    };
                    scene.Add(physSceneNode, false);
                }
            }
        }

        void AddToolModel(Scene scene, string classname, Matrix4x4 transformationMatrix, Vector3 position)
        {
            var filename = HammerEntities.GetToolModel(classname);
            var resource = Graphic.LoadFileObject<Binary_Pak>($"{filename}_c").Result;
            if (resource == null)
            {
                // TODO: Create a 16x16x16 box to emulate how Hammer draws them
                resource = Graphic.LoadFileObject<Binary_Pak>("materials/editor/obsolete.vmat_c").Result;
                if (resource == null) return;
            }

            if (resource.DATA is DATAModel model)
            {
                var modelNode = new ModelSceneNode(scene, (IValveModel)model, null, false)
                {
                    Transform = transformationMatrix,
                    LayerName = "Entities",
                };
                scene.Add(modelNode, false);
            }
            else if (resource.DATA is DATAMaterial)
            {
                var spriteNode = new SpriteSceneNode(scene, resource, position)
                {
                    LayerName = "Entities",
                };
                scene.Add(spriteNode, false);
            }
            else throw new ArgumentOutOfRangeException(nameof(resource), $"Got resource {resource} for class \"{classname}\"");
        }
    }
}
