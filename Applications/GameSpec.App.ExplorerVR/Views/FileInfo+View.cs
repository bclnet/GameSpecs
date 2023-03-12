using GameSpec.App.ExplorerVR.Controls;

namespace GameSpec.App.Explorer.Views
{
    partial class FileInfo
    {
        private TreeView Node;

        void InitializeComponent()
        {
            Node = NameScopeExtensions.FindByName<TreeView>(this, "Node");
        }
    }
}
