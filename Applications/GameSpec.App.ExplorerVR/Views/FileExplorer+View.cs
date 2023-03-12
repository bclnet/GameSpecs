using GameSpec.App.ExplorerVR.Controls;

namespace GameSpec.App.Explorer.Views
{
    partial class FileExplorer
    {
        private Picker NodeFilter;
        private TreeView Node;
        private FileInfo FileInfo;

        void InitializeComponent()
        {
            NodeFilter = NameScopeExtensions.FindByName<Picker>(this, "NodeFilter");
            Node = NameScopeExtensions.FindByName<TreeView>(this, "Node");
            FileInfo = NameScopeExtensions.FindByName<FileInfo>(this, "FileInfo");
        }
    }
}
