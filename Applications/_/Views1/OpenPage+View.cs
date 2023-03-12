//using GameSpec.App.ExplorerVR.Controls;

using Microsoft.Maui.Controls;

namespace GameSpec.App.Explorer.Views
{
    partial class OpenPage
    {
        private Picker Family;
        private Picker FamilyGame;

        void InitializeComponent()
        {
            Family = NameScopeExtensions.FindByName<Picker>(this, "Family");
            FamilyGame = NameScopeExtensions.FindByName<Picker>(this, "FamilyGame");
        }
    }
}
