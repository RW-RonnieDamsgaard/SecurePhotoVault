using SecurePhotoVaultMAUI.Views;

namespace SecurePhotoVaultMAUI
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            Routing.RegisterRoute("MainPage", typeof(MainPage));
            Routing.RegisterRoute("LoginPage", typeof(LoginPage));

            MainPage = new AppShell();
        }
    }
}
