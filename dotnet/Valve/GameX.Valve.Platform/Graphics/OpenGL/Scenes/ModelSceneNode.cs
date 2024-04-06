using GameX.Valve.Formats;
using GameX.Valve.Formats.Animations;
using GameX.Valve.Formats.Blocks;
using OpenStack;
using OpenStack.Graphics.OpenGL.Renderer1;
using OpenStack.Graphics.Renderer1;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace GameX.Valve.Graphics.OpenGL.Scenes
{
    //was:Renderer/ModelSceneNode
    public class ModelSceneNode : SceneNode, IMeshCollection
    {
        IValveModel Model { get; }

        public Vector4 Tint
        {
            get => MeshRenderers.Count > 0 ? MeshRenderers[0].Tint : Vector4.One;
            set { foreach (var renderer in MeshRenderers) renderer.Tint = value; }
        }

        public readonly AnimationController AnimationController;
        public IEnumerable<RenderableMesh> RenderableMeshes => ActiveMeshRenderers;
        public string ActiveSkin;

        readonly List<RenderableMesh> MeshRenderers = new List<RenderableMesh>();
        readonly List<Animation> Animations = new List<Animation>();
        Dictionary<string, string> SkinMaterials;

        int AnimationTexture = -1;
        ICollection<string> ActiveMeshGroups = new HashSet<string>();
        ICollection<RenderableMesh> ActiveMeshRenderers = new HashSet<RenderableMesh>();

        bool LoadedAnimations;

        public ModelSceneNode(Scene scene, IValveModel model, string skin = null, bool loadAnimations = true) : base(scene)
        {
            Model = model;
            AnimationController = new AnimationController(model.Skeleton);

            if (skin != null) SetSkin(skin);

            LoadMeshes();
            UpdateBoundingBox();

            if (loadAnimations) LoadAnimations();
        }

        public override void Update(Scene.UpdateContext context)
        {
            if (!AnimationController.Update(context.Timestep)) return;

            UpdateBoundingBox(); // Reset back to the mesh bbox

            var newBoundingBox = LocalBoundingBox;

            // Update animation matrices
            var skeleton = Model.Skeleton;
            var matrices = AnimationController.GetAnimationMatrices(skeleton);
            var animationMatrices = matrices.Flatten();

            // Update animation texture
            GL.BindTexture(TextureTarget.Texture2D, AnimationTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, 4, skeleton.Bones.Length, 0, PixelFormat.Rgba, PixelType.Float, animationMatrices);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            var first = true;
            foreach (var matrix in matrices)
            {
                var bbox = LocalBoundingBox.Transform(matrix);
                newBoundingBox = first ? bbox : newBoundingBox.Union(bbox);
                first = false;
            }

            LocalBoundingBox = newBoundingBox;
        }

        public override void Render(Scene.RenderContext context) { } // This node does not render itself; it uses the batching system via IRenderableMeshCollection

        public override IEnumerable<string> GetSupportedRenderModes() => MeshRenderers.SelectMany(renderer => renderer.GetSupportedRenderModes()).Distinct();

        public override void SetRenderMode(string renderMode)
        {
            foreach (var renderer in MeshRenderers) renderer.SetRenderMode(renderMode);
        }

        void SetSkin(string skin)
        {
            ActiveSkin = skin;

            string[] defaultMaterials = null;
            foreach (var materialGroup in Model.Data.Get<IDictionary<string, object>[]>("m_materialGroups"))
            {
                // The first item needs to match the default materials on the model
                defaultMaterials ??= materialGroup.Get<string[]>("m_materials");
                if (materialGroup.Get<string>("m_name") == skin)
                {
                    var materials = materialGroup.Get<string[]>("m_materials");
                    SkinMaterials = new Dictionary<string, string>();
                    for (var i = 0; i < defaultMaterials.Length; i++) SkinMaterials[defaultMaterials[i]] = materials[i];
                    break;
                }
            }

            foreach (var mesh in MeshRenderers) mesh.SetSkin(SkinMaterials);
        }

        public void LoadAnimations()
        {
            if (LoadedAnimations) return;
            LoadedAnimations = true;
            Animations.AddRange(Model.GetAllAnimations(null));
            if (Animations.Any()) SetupAnimationTextures();
        }

        void LoadMeshes()
        {
            // Get embedded meshes
            foreach (var embeddedMesh in Model.GetEmbeddedMeshesAndLoD().Where(m => (m.LoDMask & 1) != 0))
                MeshRenderers.Add(new GLRenderableMesh(Scene.Graphic as IOpenGLGraphic, embeddedMesh.Mesh, embeddedMesh.MeshIndex, SkinMaterials, Model));

            // Load referred meshes from file (only load meshes with LoD 1)
            foreach (var refMesh in GetLod1RefMeshes())
            {
                var newResource = Scene.Graphic.LoadFileObject<Binary_Pak>($"{refMesh.MeshName}_c").Result;
                if (newResource == null) continue;

                if (!newResource.ContainsBlockType<VBIB>()) { Console.WriteLine("Old style model, no VBIB!"); continue; }

                MeshRenderers.Add(new GLRenderableMesh(Scene.Graphic as IOpenGLGraphic, (DATAMesh)newResource.DATA, refMesh.MeshIndex, SkinMaterials, Model));
            }

            // Set active meshes to default
            SetActiveMeshGroups(Model.GetDefaultMeshGroups());
        }

        void SetupAnimationTextures()
        {
            if (AnimationTexture == -1)
            {
                // Create animation texture
                AnimationTexture = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, AnimationTexture);
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

        public IEnumerable<string> GetSupportedAnimationNames() => Animations.Select(a => a.Name);

        public void SetAnimation(string animationName)
        {
            var activeAnimation = Animations.FirstOrDefault(a => a.Name == animationName);
            AnimationController.SetAnimation(activeAnimation);
            UpdateBoundingBox();

            if (activeAnimation != default) for (var i = 0; i < MeshRenderers.Count; i++) MeshRenderers[i].SetAnimationTexture(AnimationTexture, Model.Skeleton.Bones.Length);
            else foreach (var renderer in MeshRenderers) renderer.SetAnimationTexture(null, 0);
        }

        public IEnumerable<(int MeshIndex, string MeshName, long LoDMask)> GetLod1RefMeshes() => Model.GetReferenceMeshNamesAndLoD().Where(m => (m.LoDMask & 1) != 0);

        public IEnumerable<string> GetMeshGroups() => Model.GetMeshGroups();

        public ICollection<string> GetActiveMeshGroups() => ActiveMeshGroups;

        public void SetActiveMeshGroups(IEnumerable<string> meshGroups)
        {
            ActiveMeshGroups = new HashSet<string>(GetMeshGroups().Intersect(meshGroups));

            var groups = GetMeshGroups();
            if (groups.Count() > 1)
            {
                ActiveMeshRenderers.Clear();
                foreach (var group in ActiveMeshGroups)
                {
                    var meshMask = Model.GetActiveMeshMaskForGroup(group).ToArray();
                    foreach (var meshRenderer in MeshRenderers) if (meshMask[meshRenderer.MeshIndex]) ActiveMeshRenderers.Add(meshRenderer);
                }
            }
            else ActiveMeshRenderers = new HashSet<RenderableMesh>(MeshRenderers);
        }

        void UpdateBoundingBox()
        {
            var first = true;
            foreach (var mesh in MeshRenderers)
            {
                LocalBoundingBox = first ? mesh.BoundingBox : BoundingBox.Union(mesh.BoundingBox);
                first = false;
            }
        }
    }
}
