using GameSpec.App.ExplorerVR.Controls;

namespace GameSpec.App.Explorer.Views
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
