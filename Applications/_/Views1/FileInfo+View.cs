using Microsoft.Maui.Controls;

namespace GameSpec.App.Explorer.Views
{
    partial class FileInfo
    {
        private global::TreeView.Maui.Controls.TreeView Node;

        void InitializeComponent()
        {
            Node = NameScopeExtensions.FindByName<global::TreeView.Maui.Controls.TreeView>(this, "Node");
        }
    }
}
