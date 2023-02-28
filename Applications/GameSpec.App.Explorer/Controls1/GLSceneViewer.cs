using OpenStack;
using OpenStack.Graphics;
using OpenStack.Graphics.Controls;
using OpenStack.Graphics.OpenGL.Renderer1;
using OpenStack.Graphics.OpenGL.Renderer1.Renderers;
using OpenStack.Graphics.Renderer1;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows;

namespace GameSpec.App.Explorer.Controls1
{
    //was:Renderer/GLSceneViewer
    public abstract class GLSceneViewer : GLViewerControl
    {
        public Scene Scene { get; private set; }
        public Scene SkyboxScene { get; protected set; }

        public bool ShowBaseGrid { get; set; } = true;
        public bool ShowSkybox { get; set; } = true;

        protected float SkyboxScale { get; set; } = 1.0f;
        protected Vector3 SkyboxOrigin { get; set; } = Vector3.Zero;

        bool _showStaticOctree = false;
        bool _showDynamicOctree = false;
        Frustum _cullFrustum;

        //ComboBox _renderModeComboBox;
        ParticleGridRenderer _baseGrid;
        Camera _skyboxCamera = new GLDebugCamera();
        OctreeDebugRenderer<SceneNode> _staticOctreeRenderer;
        OctreeDebugRenderer<SceneNode> _dynamicOctreeRenderer;

        protected GLSceneViewer(Frustum cullFrustum = null)
        {
            _cullFrustum = cullFrustum;

            InitializeControl();

            //AddCheckBox("Show Grid", ShowBaseGrid, (v) => ShowBaseGrid = v);
            //AddCheckBox("Show Static Octree", _showStaticOctree, (v) => _showStaticOctree = v);
            //AddCheckBox("Show Dynamic Octree", _showDynamicOctree, (v) => _showDynamicOctree = v);
            //AddCheckBox("Lock Cull Frustum", false, (v) => { _lockedCullFrustum = v ? Scene.MainCamera.ViewFrustum.Clone() : null; });

            Unloaded += (a, b) => { GLPaint -= OnPaint; };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public static readonly DependencyProperty GraphicProperty = DependencyProperty.Register(nameof(Graphic), typeof(object), typeof(GLSceneViewer),
            new PropertyMetadata((d, e) => (d as GLSceneViewer).OnProperty()));
        public IOpenGraphic Graphic
        {
            get => GetValue(GraphicProperty) as IOpenGraphic;
            set => SetValue(GraphicProperty, value);
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source), typeof(object), typeof(GLSceneViewer),
            new PropertyMetadata((d, e) => (d as GLSceneViewer).OnProperty()));
        public object Source
        {
            get => GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        void OnProperty()
        {
            if (Graphic == null || Source == null) return;

            var graphic = Graphic as IOpenGLGraphic;

            Scene = new Scene(graphic, MeshBatchRenderer.Render);
            _baseGrid = new ParticleGridRenderer(20, 5, graphic);

            Camera.SetViewportSize((int)ActualWidth, (int)ActualHeight); //: HandleResize()
            Camera.SetLocation(new Vector3(256));
            Camera.LookAt(new Vector3(0));

            LoadScene(Source);

            if (Scene.AllNodes.Any())
            {
                var bbox = Scene.AllNodes.First().BoundingBox;
                var location = new Vector3(bbox.Max.Z, 0, bbox.Max.Z) * 1.5f;

                Camera.SetLocation(location);
                Camera.LookAt(bbox.Center);
            }

            _staticOctreeRenderer = new OctreeDebugRenderer<SceneNode>(Scene.StaticOctree, Graphic as IOpenGLGraphic, false);
            _dynamicOctreeRenderer = new OctreeDebugRenderer<SceneNode>(Scene.DynamicOctree, Graphic as IOpenGLGraphic, true);

            //if (_renderModeComboBox != null)
            //{
            //    var supportedRenderModes = Scene.AllNodes
            //        .SelectMany(r => r.GetSupportedRenderModes())
            //        .Distinct();
            //    SetAvailableRenderModes(supportedRenderModes);
            //}

            GLPaint += OnPaint;
        }

        protected abstract void InitializeControl();

        protected abstract void LoadScene(object source);

        void OnPaint(object sender, RenderEventArgs e)
        {
            Scene.MainCamera = e.Camera;
            Scene.Update(e.FrameTime);

            if (ShowBaseGrid) _baseGrid.Render(e.Camera, RenderPass.Both);

            if (ShowSkybox && SkyboxScene != null)
            {
                _skyboxCamera.CopyFrom(e.Camera);
                _skyboxCamera.SetLocation(e.Camera.Location - SkyboxOrigin);
                _skyboxCamera.SetScale(SkyboxScale);

                SkyboxScene.MainCamera = _skyboxCamera;
                SkyboxScene.Update(e.FrameTime);
                SkyboxScene.RenderWithCamera(_skyboxCamera);

                GL.Clear(ClearBufferMask.DepthBufferBit);
            }

            Scene.RenderWithCamera(e.Camera, _cullFrustum);

            if (_showStaticOctree) _staticOctreeRenderer.Render(e.Camera, RenderPass.Both);
            if (_showDynamicOctree) _dynamicOctreeRenderer.Render(e.Camera, RenderPass.Both);
        }

        protected void SetEnabledLayers(HashSet<string> layers)
        {
            Scene.SetEnabledLayers(layers);
            _staticOctreeRenderer = new OctreeDebugRenderer<SceneNode>(Scene.StaticOctree, Graphic as IOpenGLGraphic, false);
        }

        //protected void AddRenderModeSelectionControl()
        //{
        //    if (_renderModeComboBox == null)
        //        _renderModeComboBox = AddSelection("Render Mode", (renderMode, _) =>
        //        {
        //            foreach (var node in Scene.AllNodes)
        //                node.SetRenderMode(renderMode);

        //            if (SkyboxScene != null)
        //                foreach (var node in SkyboxScene.AllNodes)
        //                    node.SetRenderMode(renderMode);
        //        });
        //}

        //void SetAvailableRenderModes(IEnumerable<string> renderModes)
        //{
        //    _renderModeComboBox.Items.Clear();
        //    if (renderModes.Any())
        //    {
        //        _renderModeComboBox.Enabled = true;
        //        _renderModeComboBox.Items.Add("Default Render Mode");
        //        _renderModeComboBox.Items.AddRange(renderModes.ToArray());
        //        _renderModeComboBox.SelectedIndex = 0;
        //    }
        //    else
        //    {
        //        _renderModeComboBox.Items.Add("(no render modes available)");
        //        _renderModeComboBox.SelectedIndex = 0;
        //        _renderModeComboBox.Enabled = false;
        //    }
        //}
    }
}
