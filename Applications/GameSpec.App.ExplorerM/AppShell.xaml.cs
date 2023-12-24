using GameSpec.App.Explorer.Views;

namespace GameSpec.App.Explorer
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Application.Current.MainPage = this;
        }

        internal Task OnFirstLoad()
        {
            var openPage = new OpenPage();
            Navigation.PushAsync(openPage);
            return Task.CompletedTask;
        }
    }
}