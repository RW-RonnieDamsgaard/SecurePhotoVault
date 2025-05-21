using SecurePhotoVaultMAUI.ViewModels;

namespace SecurePhotoVaultMAUI.Views
{
    public partial class MainPage : ContentPage
    {

        public MainPage(MainPageViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is MainPageViewModel vm)
            {
                vm.CheckLoginStatusOnAppearing(); // ny metode du tilføjer
            }
        }
    }

}
