using Microsoft.Maui.Controls;

namespace GameX.App.Explorer.Views
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
