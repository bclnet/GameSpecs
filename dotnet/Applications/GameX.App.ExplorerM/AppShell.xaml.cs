using GameX.App.Explorer.Views;

namespace GameX.App.Explorer
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Application.Current.MainPage = this;
        }

        internal Task OnReady()
        {
            var openPage = new OpenPage();
            openPage.OnReady();
            Navigation.PushAsync(openPage);
            return Task.CompletedTask;
        }
    }
}