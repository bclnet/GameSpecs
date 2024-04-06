//using GameX.Graphics.Scenes;
//using GameX.Valve.Formats;
//using GameX.Valve.Graphics.OpenGL.Scenes;
//using OpenStack.Graphics;
//using OpenStack.Graphics.Renderer1;

//namespace GameX.App.Explorer.Controls1
//{
//    //was:Renderer/GLModelViewer
//    public class GLModelViewer : GLSceneViewer
//    {
//        //ComboBox _animationComboBox;
//        //CheckedListBox _meshGroupListBox;
//        ModelSceneNode _modelSceneNode;
//        MeshSceneNode _meshSceneNode;

//        public GLModelViewer() : base(Frustum.CreateEmpty()) { }

//        protected override void InitializeControl()
//        {
//            //AddRenderModeSelectionControl();
//            //_animationComboBox = ViewerControl.AddSelection("Animation", (animation, _) => { _modelSceneNode?.SetAnimation(animation); });
//        }

//        protected override void LoadScene(object source)
//        {
//            var model = source is IModel z1 ? z1
//                : source is IRedirected<IModel> y1 ? y1.Value
//                : null;
//            if (model != null)
//            {
//                _modelSceneNode = new ModelSceneNode(Scene, model as IValveModel);
//                SetAvailableAnimations(_modelSceneNode.GetSupportedAnimationNames());
//                Scene.Add(_modelSceneNode, false);

//                var meshGroups = _modelSceneNode.GetMeshGroups();
//                if (meshGroups.Count() > 1)
//                {
//                    //    _meshGroupListBox = ViewerControl.AddMultiSelection("Mesh Group", selectedGroups => { _modelSceneNode.SetActiveMeshGroups(selectedGroups); });
//                    //    _meshGroupListBox.Items.AddRange(_modelSceneNode.GetMeshGroups().ToArray<object>());
//                    //    foreach (var group in _modelSceneNode.GetActiveMeshGroups()) _meshGroupListBox.SetItemChecked(_meshGroupListBox.FindStringExact(group), true);
//                }
//            }
//            else SetAvailableAnimations(Enumerable.Empty<string>());

//            var mesh = source is IMesh z2 ? z2
//                : source is IRedirected<IMesh> y2 ? y2.Value
//                : null;
//            if (mesh != null)
//            {
//                _meshSceneNode = new MeshSceneNode(Scene, mesh, 0);
//                Scene.Add(_meshSceneNode, false);
//            }
//        }

//        void SetAvailableAnimations(IEnumerable<string> animations)
//        {
//            //_animationComboBox.BeginUpdate();
//            //_animationComboBox.Items.Clear();

//            //var count = animations.Count();
//            //if (count > 0)
//            //{
//            //    _animationComboBox.Enabled = true;
//            //    _animationComboBox.Items.Add($"({count} animations available)");
//            //    _animationComboBox.Items.AddRange(animations.ToArray());
//            //    _animationComboBox.SelectedIndex = 0;
//            //}
//            //else
//            //{
//            //    _animationComboBox.Items.Add("(no animations available)");
//            //    _animationComboBox.SelectedIndex = 0;
//            //    _animationComboBox.Enabled = false;
//            //}
//            //_animationComboBox.EndUpdate();
//        }
//    }
//}
