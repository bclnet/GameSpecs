using GameSpec.Valve.Formats;
using GameSpec.Valve.Formats.Blocks;
using GameSpec.Valve.Formats.Blocks.Animation;
using GameSpec.Valve.Formats.Blocks.Animation.SegmentDecoders;
using GameSpec.Valve.Formats.OpenGL;
using OpenStack;
using OpenStack.Graphics.OpenGL;
using OpenStack.Graphics.Renderer;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace GameSpec.Valve.Graphics.OpenGL.Scenes
{
    public class DebugModelSceneNode : SceneNode, IMeshCollection
    {
        IValveModelInfo Model { get; }

        public Vector4 Tint
        {
            get => _meshRenderers.Count > 0 ? _meshRenderers[0].Tint : Vector4.One;
            set { foreach (var renderer in _meshRenderers) renderer.Tint = value; }
        }

        public IEnumerable<Mesh> Meshes => _activeMeshRenderers;

        readonly List<Mesh> _meshRenderers = new();
        readonly List<CCompressedAnimQuaternion> _animations = new();
        Dictionary<string, string> _skinMaterials;

        CCompressedAnimQuaternion _activeAnimation;
        int _animationTexture;
        Skeleton _skeleton;
        ICollection<string> _activeMeshGroups = new HashSet<string>();
        ICollection<Mesh> _activeMeshRenderers = new HashSet<Mesh>();

        float _time;

        public DebugModelSceneNode(Scene scene, IValveModelInfo model, string skin = null, bool loadAnimations = true) : base(scene)
        {
            Model = model;

            // Load required resources
            if (loadAnimations)
            {
                LoadSkeleton();
                LoadAnimations();
            }

            if (skin != null) SetSkin(skin);

            LoadMeshes();
            UpdateBoundingBox();
        }

        public override void Update(Scene.UpdateContext context)
        {
            if (_activeAnimation == null) return;

            // Update animation matrices
            var animationMatrices = new float[_skeleton.AnimationTextureSize * 16];
            for (var i = 0; i < _skeleton.AnimationTextureSize; i++)
            {
                // Default to identity matrices
                animationMatrices[i * 16] = 1.0f;
                animationMatrices[(i * 16) + 5] = 1.0f;
                animationMatrices[(i * 16) + 10] = 1.0f;
                animationMatrices[(i * 16) + 15] = 1.0f;
            }

            _time += context.Timestep;
            animationMatrices = _activeAnimation.GetAnimationMatricesAsArray(_time, _skeleton);

            // Update animation texture
            GL.BindTexture(TextureTarget.Texture2D, _animationTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, 4, _skeleton.AnimationTextureSize, 0, PixelFormat.Rgba, PixelType.Float, animationMatrices);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public override void Render(Scene.RenderContext context) { } // This node does not render itself; it uses the batching system via IRenderableMeshCollection

        public override IEnumerable<string> GetSupportedRenderModes() => _meshRenderers.SelectMany(renderer => renderer.GetSupportedRenderModes()).Distinct();

        public override void SetRenderMode(string renderMode)
        {
            foreach (var renderer in _meshRenderers) renderer.SetRenderMode(renderMode);
        }

        void SetSkin(string skin)
        {
            var materialGroups = Model.Data.Get<IDictionary<string, object>[]>("m_materialGroups");
            string[] defaultMaterials = null;

            foreach (var materialGroup in materialGroups)
            {
                // "The first item needs to match the default materials on the model"
                if (defaultMaterials == null) defaultMaterials = materialGroup.Get<string[]>("m_materials");

                if (materialGroup.Get<string>("m_name") == skin)
                {
                    var materials = materialGroup.Get<string[]>("m_materials");
                    _skinMaterials = new Dictionary<string, string>();
                    for (var i = 0; i < defaultMaterials.Length; i++) _skinMaterials[defaultMaterials[i]] = materials[i];
                    break;
                }
            }
        }

        void LoadMeshes()
        {
            // Get embedded meshes
            foreach (var embeddedMesh in Model.GetEmbeddedMeshesAndLoD().Where(m => (m.LoDMask & 1) != 0))  _meshRenderers.Add(new GLMesh(Scene.Graphic as IOpenGLGraphic, embeddedMesh.Mesh, _skinMaterials));

            // Load referred meshes from file (only load meshes with LoD 1)
            var referredMeshesAndLoDs = Model.GetReferenceMeshNamesAndLoD();
            foreach (var (MeshName, LoDMask) in referredMeshesAndLoDs.Where(m => (m.LoDMask & 1) != 0))
            {
                var newResource = Scene.Graphic.LoadFileObjectAsync<BinaryPak>(MeshName).Result;
                if (newResource == null) continue;

                if (!newResource.ContainsBlockType<VBIB>()) { Console.WriteLine("Old style model, no VBIB!"); continue; }

                _meshRenderers.Add(new GLMesh(Scene.Graphic as IOpenGLGraphic, new DATAMesh(newResource), _skinMaterials));
            }

            // Set active meshes to default
            SetActiveMeshGroups(Model.GetDefaultMeshGroups());
        }

        void LoadSkeleton() => _skeleton = Model.GetSkeleton();

        void SetupAnimationTexture()
        {
            if (_animationTexture == default)
            {
                // Create animation texture
                _animationTexture = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, _animationTexture);
                // Set clamping to edges
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                // Set nearest-neighbor sampling since we don't want to interpolate matrix rows
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Nearest);
                //Unbind texture again
                GL.BindTexture(TextureTarget.Texture2D, 0);
            }
        }

        void LoadAnimations()
        {
            var animGroupPaths = Model.GetReferencedAnimationGroupNames();
            var embeddedAnims = Model.GetEmbeddedAnimations();

            if (!animGroupPaths.Any() && !embeddedAnims.Any()) return;

            SetupAnimationTexture();

            // Load animations from referenced animation groups
            foreach (var animGroupPath in animGroupPaths)
            {
                var animGroup = Scene.Graphic.LoadFileObjectAsync<BinaryPak>(animGroupPath).Result;
                _animations.AddRange(AnimationGroupLoader.LoadAnimationGroup(Scene.Graphic as IOpenGLGraphic, animGroup));
            }

            // Get embedded animations
            _animations.AddRange(embeddedAnims);
        }

        public void LoadAnimation(string animationName)
        {
            var animGroupPaths = Model.GetReferencedAnimationGroupNames();
            var embeddedAnims = Model.GetEmbeddedAnimations();

            if (!animGroupPaths.Any() && !embeddedAnims.Any()) return;

            if (_skeleton == default)
            {
                LoadSkeleton();
                SetupAnimationTexture();
            }

            // Get embedded animations
            var embeddedAnim = embeddedAnims.FirstOrDefault(a => a.Name == animationName);
            if (embeddedAnim != default) { _animations.Add(embeddedAnim); return; }

            // Load animations from referenced animation groups
            foreach (var animGroupPath in animGroupPaths)
            {
                var animGroup = Scene.Graphic.LoadFileObjectAsync<BinaryPak>(animGroupPath).Result;
                var foundAnimations = AnimationGroupLoader.TryLoadSingleAnimationFileFromGroup(Scene.Graphic as IOpenGLGraphic, animGroup, animationName);
                if (foundAnimations != default) { _animations.AddRange(foundAnimations); return; }
            }
        }

        public IEnumerable<string> GetSupportedAnimationNames() => _animations.Select(a => a.Name);

        public void SetAnimation(string animationName)
        {
            _time = 0f;
            _activeAnimation = _animations.FirstOrDefault(a => a.Name == animationName);

            if (_activeAnimation != default) foreach (var renderer in _meshRenderers) renderer.SetAnimationTexture(_animationTexture, _skeleton.AnimationTextureSize);
            else foreach (var renderer in _meshRenderers) renderer.SetAnimationTexture(null, 0);
        }

        public IEnumerable<string> GetMeshGroups() => Model.GetMeshGroups();

        public ICollection<string> GetActiveMeshGroups() => _activeMeshGroups;

        public void SetActiveMeshGroups(IEnumerable<string> meshGroups)
        {
            _activeMeshGroups = new HashSet<string>(GetMeshGroups().Intersect(meshGroups));

            var groups = GetMeshGroups();
            if (groups.Count() > 1)
            {
                _activeMeshRenderers.Clear();
                foreach (var group in _activeMeshGroups)
                {
                    var meshMask = Model.GetActiveMeshMaskForGroup(group).ToArray();
                    for (var meshIndex = 0; meshIndex < _meshRenderers.Count; meshIndex++) if (meshMask[meshIndex] && !_activeMeshRenderers.Contains(_meshRenderers[meshIndex])) _activeMeshRenderers.Add(_meshRenderers[meshIndex]);
                }
            }
            else _activeMeshRenderers = new HashSet<Mesh>(_meshRenderers);
        }

        void UpdateBoundingBox()
        {
            var first = true;
            foreach (var mesh in _meshRenderers)
            {
                LocalBoundingBox = first ? mesh.BoundingBox : BoundingBox.Union(mesh.BoundingBox);
                first = false;
            }
        }
    }
}
