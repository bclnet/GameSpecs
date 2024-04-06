//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Numerics;

//namespace namespace GameX.App.Explorer.Controls
//{
//    /// <summary>
//    /// GL Render control with world controls (render mode, camera selection).
//    /// </summary>
//#pragma warning disable CA1001 // Types that own disposable fields should be disposable
//was:Renderer/GLWorldViewer
//    public class GLWorldViewer : GLSceneViewer
//#pragma warning restore CA1001 // Types that own disposable fields should be disposable
//    {
//        readonly World _world;
//        readonly WorldNode _worldNode;
//        CheckedListBox _worldLayersComboBox;
//        ComboBox _cameraComboBox;
//        SavedCameraPositionsControl _savedCameraPositionsControl;

//        public GLWorldViewer(GuiContext guiContext, World world) : base(guiContext)
//        {
//            _world = world;
//        }

//        public GLWorldViewer(GuiContext guiContext, WorldNode worldNode) : base(guiContext)
//        {
//            _worldNode = worldNode;
//        }

//        protected override void InitializeControl()
//        {
//            AddRenderModeSelectionControl();

//            _worldLayersComboBox = ViewerControl.AddMultiSelection("World Layers", (worldLayers) =>
//            {
//                SetEnabledLayers(new HashSet<string>(worldLayers));
//            });

//            _savedCameraPositionsControl = new SavedCameraPositionsControl();
//            _savedCameraPositionsControl.SaveCameraRequest += OnSaveCameraRequest;
//            _savedCameraPositionsControl.RestoreCameraRequest += OnRestoreCameraRequest;
//            ViewerControl.AddControl(_savedCameraPositionsControl);
//        }

//        void OnRestoreCameraRequest(object sender, string e)
//        {
//            if (Settings.Config.SavedCameras.TryGetValue(e, out var savedFloats))
//                if (savedFloats.Length == 5)
//                    Scene.MainCamera.SetLocationPitchYaw(new Vector3(savedFloats[0], savedFloats[1], savedFloats[2]), savedFloats[3], savedFloats[4]);
//        }

//        void OnSaveCameraRequest(object sender, EventArgs e)
//        {
//            var cam = Scene.MainCamera;
//            var saveName = string.Format("Saved Camera #{0}", Settings.Config.SavedCameras.Count + 1);

//            Settings.Config.SavedCameras.Add(saveName, new[] { cam.Location.X, cam.Location.Y, cam.Location.Z, cam.Pitch, cam.Yaw });
//            Settings.Save();

//            _savedCameraPositionsControl.RefreshSavedPositions();
//        }

//        protected override void LoadScene()
//        {
//            if (_world != null)
//            {
//                var loader = new WorldLoader(GuiContext, _world);
//                var result = loader.Load(Scene);

//                if (result.Skybox != null)
//                {
//                    SkyboxScene = new Scene(GuiContext);
//                    var skyboxLoader = new WorldLoader(GuiContext, result.Skybox);
//                    var skyboxResult = skyboxLoader.Load(SkyboxScene);

//                    SkyboxScale = skyboxResult.SkyboxScale;
//                    SkyboxOrigin = skyboxResult.SkyboxOrigin;

//                    ViewerControl.AddCheckBox("Show Skybox", ShowSkybox, (v) => ShowSkybox = v);
//                }

//                var worldLayers = Scene.AllNodes
//                    .Select(r => r.LayerName)
//                    .Distinct();
//                SetAvailableLayers(worldLayers);

//                if (worldLayers.Any())
//                {
//                    // TODO: Since the layers are combined, has to be first in each world node?
//                    _worldLayersComboBox.SetItemCheckState(0, CheckState.Checked);

//                    foreach (var worldLayer in result.DefaultEnabledLayers)
//                        _worldLayersComboBox.SetItemCheckState(_worldLayersComboBox.FindStringExact(worldLayer), CheckState.Checked);
//                }

//                if (result.CameraMatrices.Any())
//                {
//                    if (_cameraComboBox == default)
//                    {
//                        _cameraComboBox = ViewerControl.AddSelection("Camera", (cameraName, index) =>
//                        {
//                            if (index > 0)
//                            {
//                                if (result.CameraMatrices.TryGetValue(cameraName, out var cameraMatrix))
//                                    Scene.MainCamera.SetFromTransformMatrix(cameraMatrix);

//                                _cameraComboBox.SelectedIndex = 0;
//                            }
//                        });

//                        _cameraComboBox.Items.Add("Set view to camera...");
//                        _cameraComboBox.SelectedIndex = 0;
//                    }

//                    _cameraComboBox.Items.AddRange(result.CameraMatrices.Keys.ToArray<object>());
//                }
//            }

//            if (_worldNode != null)
//            {
//                var loader = new WorldNodeLoader(GuiContext, _worldNode);
//                loader.Load(Scene);

//                var worldLayers = Scene.AllNodes
//                    .Select(r => r.LayerName)
//                    .Distinct()
//                    .ToList();
//                SetAvailableLayers(worldLayers);

//                for (var i = 0; i < _worldLayersComboBox.Items.Count; i++)
//                    _worldLayersComboBox.SetItemChecked(i, true);
//            }

//            ShowBaseGrid = false;

//            ViewerControl.Invoke((Action)_savedCameraPositionsControl.RefreshSavedPositions);
//        }

//        void SetAvailableLayers(IEnumerable<string> worldLayers)
//        {
//            _worldLayersComboBox.Items.Clear();
//            if (worldLayers.Any())
//            {
//                _worldLayersComboBox.Enabled = true;
//                _worldLayersComboBox.Items.AddRange(worldLayers.ToArray());
//            }
//            else
//                _worldLayersComboBox.Enabled = false;
//        }
//    }
//}
