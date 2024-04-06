using Microsoft.Maui.Controls;

namespace GameX.App.Explorer.Views
{
    partial class AudioPlayer
    {
        private Button PlayButton;

        void InitializeComponent()
        {
            PlayButton = NameScopeExtensions.FindByName<Button>(this, "PlayButton");
        }
    }
}
