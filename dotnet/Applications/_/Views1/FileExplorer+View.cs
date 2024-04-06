using Microsoft.Maui.Controls;

namespace GameX.App.Explorer.Views
{
    partial class FileExplorer
    {
        private Picker NodeFilter;
        private global::TreeView.Maui.Controls.TreeView Node;
        private FileInfo FileInfo;

        void InitializeComponent()
        {
            NodeFilter = NameScopeExtensions.FindByName<Picker>(this, "NodeFilter");
            Node = NameScopeExtensions.FindByName<global::TreeView.Maui.Controls.TreeView>(this, "Node");
            FileInfo = NameScopeExtensions.FindByName<FileInfo>(this, "FileInfo");
        }
    }
}
