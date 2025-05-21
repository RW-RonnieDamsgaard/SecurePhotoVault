using SecurePhotoVaultMAUI.ViewModels;

namespace SecurePhotoVaultMAUI.Views;

public partial class LoginPage : ContentPage
{
	public LoginPage()
	{
		InitializeComponent();
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is LoginViewModel vm)
        {
            vm.Password = string.Empty;
            vm.ConfirmPassword = string.Empty;
            await vm.CheckIfRegisteredAsync();
        }
    }
}