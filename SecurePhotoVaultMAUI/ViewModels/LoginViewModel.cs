using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SecurePhotoVaultMAUI.Services;

namespace SecurePhotoVaultMAUI.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private bool isBusy;
        private bool isAlreadyRegistered;
        private string password;
        private string confirmPassword;
        public bool IsNotRegistered => !IsAlreadyRegistered;

        

        public ICommand RegisterCommand { get; }
        public ICommand LoginCommand { get; }

        public LoginViewModel()
        {
            RegisterCommand = new Command(async () => await RegisterAsync());
            LoginCommand = new Command(async () => await LoginAsync());
            //CheckIfRegisteredAsync();
        }
        public string Password
        {
            get => password;
            set
            {
                if (password != value)
                {
                    password = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Password)));
                }
            }
        }
        public string ConfirmPassword
        {
            get => confirmPassword;
            set
            {
                if (confirmPassword != value)
                {
                    confirmPassword = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ConfirmPassword)));
                }
            }
        }
        public async Task CheckIfRegisteredAsync()
        {
            var existingSalt = await Microsoft.Maui.Storage.SecureStorage.GetAsync("user-salt");
            IsAlreadyRegistered = !string.IsNullOrEmpty(existingSalt);
        }

        public bool IsBusy
        {
            get => isBusy;
            set
            {
                isBusy = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBusy)));
            }
        }
        private async Task RegisterAsync()
        {
            if (Password != ConfirmPassword)
            {
                await Shell.Current.DisplayAlert("Fejl", "Adgangskoder matcher ikke", "OK");
                return;
            }

            if (!PasswordValidator.IsPasswordStrong(Password))
            {
                await Shell.Current.DisplayAlert("Svag adgangskode",
                    "Adgangskoden skal være mindst 8 tegn og indeholde både store og små bogstaver, tal og specialtegn.",
                    "OK");
                return;
            }

            await AuthService.RegisterAsync(Password);
            await AuthService.SetLoginStatusAsync(true);
            await Shell.Current.GoToAsync("//MainPage");
        }
        public bool IsAlreadyRegistered
        {
            get => isAlreadyRegistered;
            set
            {
                isAlreadyRegistered = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAlreadyRegistered)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsNotRegistered)));

            }
        }
        private async Task LoginAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            var success = await AuthService.LoginAsync(Password);
            if (success)
            {
                await AuthService.SetLoginStatusAsync(true);
                await Task.Delay(200);
                await Shell.Current.GoToAsync("//MainPage");
            }
            else
            {
                await Shell.Current.DisplayAlert("Fejl", "Forkert adgangskode", "OK");
            }
            IsBusy = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
