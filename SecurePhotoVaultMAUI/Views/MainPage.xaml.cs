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
    }

}
