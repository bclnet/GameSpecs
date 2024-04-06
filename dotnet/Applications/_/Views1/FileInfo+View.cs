using Microsoft.Maui.Controls;

namespace GameX.App.Explorer.Views
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
