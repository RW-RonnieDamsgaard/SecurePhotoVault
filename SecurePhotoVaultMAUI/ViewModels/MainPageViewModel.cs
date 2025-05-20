using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SecurePhotoVaultMAUI.SecureStorage;
using SecurePhotoVaultMAUI.Services;
using System.Collections.ObjectModel;

namespace SecurePhotoVaultMAUI.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private bool isBusy;
        public ICommand EncryptCommand { get; }
        public ICommand DecryptCommand { get; }
        public ICommand LogoutCommand { get; }

        private bool isLoggedIn;
        public bool IsLoggedIn
        {
            get => isLoggedIn;
            set
            {
                isLoggedIn = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoggedIn)));
            }
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

        public MainPageViewModel()
        {
            EncryptCommand = new Command(async () => await EncryptImageAsync());
            DecryptCommand = new Command(async () => await DecryptImageAsync());
            LogoutCommand = new Command(async () => await LogoutAsync());

            CheckLoginStatus();
            LoadImagesAsync();
        }

        private async Task EncryptImageAsync()
        {
            if (!await AuthService.IsLoggedInAsync())
            {
                await Shell.Current.DisplayAlert("Adgang nægtet", "Du skal være logget ind for at se billeder.", "OK");
                return;
            }

            var file = await FilePicker.PickAsync(new PickOptions { FileTypes = FilePickerFileType.Images });
            if (file != null)
            {
                var encryptedPath = Path.Combine(FileSystem.AppDataDirectory, "encrypted.img");
                await ImageCryptoService.EncryptImageAsync(file.FullPath, encryptedPath);
            }
        }

        private async Task DecryptImageAsync()
        {
            if (!await AuthService.IsLoggedInAsync())
            {
                await Shell.Current.DisplayAlert("Adgang nægtet", "Du skal være logget ind for at se billeder.", "OK");
                return;
            }

            var encryptedPath = Path.Combine(FileSystem.AppDataDirectory, "encrypted.img");
            var decryptedPath = Path.Combine(FileSystem.AppDataDirectory, "decrypted.jpg");

            await ImageCryptoService.DecryptImageAsync(encryptedPath, decryptedPath);
            await Task.Delay(100);
            await LoadImagesAsync();
        }

        private async Task LogoutAsync()
        {
            IsBusy = true;
            await AuthService.LogoutAsync();
            await Task.Delay(200);
            await Shell.Current.GoToAsync("//LoginPage");
            IsBusy = false;
        }

        private async void CheckLoginStatus()
        {
            IsLoggedIn = await AuthService.IsLoggedInAsync();
        }


        private async Task LoadImagesAsync()
        {
            if (!await AuthService.IsLoggedInAsync())
                return;

            Images.Clear();

            var dir = FileSystem.AppDataDirectory;
            var imageFiles = Directory.GetFiles(dir, "*.jpg"); // eller dekrypterede billeder

            foreach (var file in imageFiles)
            {
                Images.Add(new ImageItem { FilePath = CopyImageToTemp(file) });

            }


        }
        private string CopyImageToTemp(string sourcePath)
        {
            var tempPath = Path.Combine(FileSystem.CacheDirectory, Path.GetFileName(sourcePath));
            File.Copy(sourcePath, tempPath, true);
            return tempPath;
        }

        public ObservableCollection<ImageItem> Images { get; set; } = new();

        public event PropertyChangedEventHandler PropertyChanged;
    }
    public class ImageItem
    {
        public string FilePath { get; set; }
    }

}
