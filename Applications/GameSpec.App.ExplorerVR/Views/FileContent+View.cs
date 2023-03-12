using GameSpec.App.ExplorerVR.Controls;

namespace GameSpec.App.Explorer.Views
{
    partial class FileContent
    {
        private ContentView @this;
        private HorizontalStackLayout ContentTab;
        private ContentView ContentTabContent;

        void InitializeComponent()
        {
            @this = NameScopeExtensions.FindByName<ContentView>(this, "this");
            ContentTab = NameScopeExtensions.FindByName<HorizontalStackLayout>(this, "ContentTab");
            ContentTabContent = NameScopeExtensions.FindByName<ContentView>(this, "ContentTabContent");
        }
    }
}
