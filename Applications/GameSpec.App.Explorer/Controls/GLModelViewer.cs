using GameSpec.Graphics.Scenes;
using GameSpec.Valve.Formats;
using GameSpec.Valve.Graphics.OpenGL.Scenes;
using OpenStack.Graphics;
using OpenStack.Graphics.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameSpec.Metadata.View
{
    public class GLModelViewer : GLSceneViewer
    {
        //ComboBox _animationComboBox;
        //CheckedListBox _meshGroupListBox;
        DebugModelSceneNode _modelSceneNode;
        DebugMeshSceneNode _meshSceneNode;

        public GLModelViewer() : base(Frustum.CreateEmpty()) { }

        protected override void InitializeControl()
        {
            //AddRenderModeSelectionControl();
            //_animationComboBox = ViewerControl.AddSelection("Animation", (animation, _) => { _modelSceneNode?.SetAnimation(animation); });
        }

        protected override void LoadScene(object source)
        {
            var model = source is IModelInfo z1 ? z1
                : source is IRedirected<IModelInfo> y1 ? y1.Value
                : null;
            if (model != null)
            {
                _modelSceneNode = new DebugModelSceneNode(Scene, model as IValveModelInfo);
                SetAvailableAnimations(_modelSceneNode.GetSupportedAnimationNames());
                Scene.Add(_modelSceneNode, false);

                var meshGroups = _modelSceneNode.GetMeshGroups();
                if (meshGroups.Count() > 1)
                {
                    //    _meshGroupListBox = ViewerControl.AddMultiSelection("Mesh Group", selectedGroups => { _modelSceneNode.SetActiveMeshGroups(selectedGroups); });
                    //    _meshGroupListBox.Items.AddRange(_modelSceneNode.GetMeshGroups().ToArray<object>());
                    //    foreach (var group in _modelSceneNode.GetActiveMeshGroups()) _meshGroupListBox.SetItemChecked(_meshGroupListBox.FindStringExact(group), true);
                }
            }
            else SetAvailableAnimations(Enumerable.Empty<string>());

            var mesh = source is IMeshInfo z2 ? z2
                : source is IRedirected<IMeshInfo> y2 ? y2.Value
                : null;
            if (mesh != null)
            {
                _meshSceneNode = new DebugMeshSceneNode(Scene, mesh);
                Scene.Add(_meshSceneNode, false);
            }
        }

        void SetAvailableAnimations(IEnumerable<string> animations)
        {
            //_animationComboBox.BeginUpdate();
            //_animationComboBox.Items.Clear();

            //var count = animations.Count();
            //if (count > 0)
            //{
            //    _animationComboBox.Enabled = true;
            //    _animationComboBox.Items.Add($"({count} animations available)");
            //    _animationComboBox.Items.AddRange(animations.ToArray());
            //    _animationComboBox.SelectedIndex = 0;
            //}
            //else
            //{
            //    _animationComboBox.Items.Add("(no animations available)");
            //    _animationComboBox.SelectedIndex = 0;
            //    _animationComboBox.Enabled = false;
            //}
            //_animationComboBox.EndUpdate();
        }
    }
}
