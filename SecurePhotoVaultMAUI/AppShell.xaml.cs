using SecurePhotoVaultMAUI.Services;

namespace SecurePhotoVaultMAUI
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            if (!await AuthService.IsLoggedInAsync())
                await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
