using System.ComponentModel;
using System.Windows.Input;
using SecurePhotoVaultMAUI.SecureStorage;
using SecurePhotoVaultMAUI.Services;
using System.Collections.ObjectModel;
using SecurePhotoVaultMAUI.Helper;

namespace SecurePhotoVaultMAUI.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private bool isBusy;
        public ICommand EncryptCommand { get; }
        public ICommand DecryptCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand AddImageCommand { get; }



        public MainPageViewModel()
        {
            AddImageCommand = new Command(async () => await AddImageAsync());
            EncryptCommand = new Command<ImageItem>(async item => await EncryptImageAsync(item));
            DecryptCommand = new Command<ImageItem>(async item => await DecryptImageAsync(item));
            LogoutCommand = new Command(async () => await LogoutAsync());

            CheckLoginStatus();
        }

        public ObservableCollection<ImageItem> Images { get; set; } = new();


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
        // User picks an image from the file system
        private async Task AddImageAsync()
        {
            var file = await FilePicker.PickAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Images,
                PickerTitle = "Vælg billede"
            });

            if (file == null)
                return;

            // Copy the selected image to the app's data directory
            var destFileName = $"plain_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var destPath = Path.Combine(FileSystem.AppDataDirectory, destFileName);

            using (var sourceStream = await file.OpenReadAsync())
            using (var destStream = File.Create(destPath))
            {
                await sourceStream.CopyToAsync(destStream);
            }

            // Add to Images collection as a plain (not encrypted) image
            var item = new ImageItem
            {
                DecryptedFilePath = destPath,
                IsDecrypted = true
            };
            item.EncryptCommand = new Command(async () => await EncryptImageAsync(item));
            item.DeleteCommand = new Command(() =>
            {
                File.Delete(destPath);
                Images.Remove(item);
            });
            Images.Add(item);
        }
        // Encrypt the image
        private async Task EncryptImageAsync(ImageItem item)
        {
            App.TouchSession();
            if (!string.IsNullOrEmpty(item.DecryptedFilePath) && File.Exists(item.DecryptedFilePath))
            {
                await ImageCryptoService.EncryptImageAsync(item.DecryptedFilePath);
                File.Delete(item.DecryptedFilePath);
                item.DecryptedFilePath = null;
                item.IsDecrypted = false;
                item.OnPropertyChanged(nameof(item.DecryptedFilePath));
                item.OnPropertyChanged(nameof(item.IsDecrypted));
                await LoadImagesAsync();
            }
        }
        // Decrypt the image
        private async Task DecryptImageAsync(ImageItem item)
        {
            App.TouchSession();
            var decryptedFile = Path.Combine(
                FileSystem.AppDataDirectory,
                $"plain_{Guid.NewGuid()}.jpg"
            );

            await ImageCryptoService.DecryptImageAsync(item.EncryptedFilePath, decryptedFile);

            File.Delete(item.EncryptedFilePath);

            item.EncryptedFilePath = null;
            item.DecryptedFilePath = decryptedFile;
            item.IsDecrypted = true;
            item.EncryptCommand = new Command(async () => await EncryptImageAsync(item));
            item.DecryptCommand = null;
            item.OnPropertyChanged(nameof(item.EncryptedFilePath));
            item.OnPropertyChanged(nameof(item.DecryptedFilePath));
            item.OnPropertyChanged(nameof(item.IsDecrypted));

            await LoadImagesAsync();
        }


        private async Task LogoutAsync()
        {
            IsBusy = true;

            await SessionSecurityHelper.RekrypterAllePlainBillederAsync();

            await Task.Delay(200);
            await Shell.Current.GoToAsync("//LoginPage");

            Images.Clear();

            IsBusy = false;
        }

        private async void CheckLoginStatus()
        {
            IsLoggedIn = await AuthService.IsLoggedInAsync();
        }

        public async void CheckLoginStatusOnAppearing()
        {
            IsLoggedIn = await AuthService.IsLoggedInAsync();

            if (IsLoggedIn)
            {
                await LoadImagesAsync();
            }
        }

        private async Task LoadImagesAsync()
        {
            if (!await AuthService.IsLoggedInAsync())
                return;

            Images.Clear();

            var encryptedFiles = Directory.GetFiles(FileSystem.AppDataDirectory, "encrypted_*.img");
            foreach (var file in encryptedFiles)
            {
                var item = new ImageItem
                {
                    EncryptedFilePath = file,
                    IsDecrypted = false
                };
                item.DecryptCommand = new Command(async () => await DecryptImageAsync(item));
                item.DeleteCommand = new Command(() =>
                {
                    File.Delete(file);
                    Images.Remove(item);
                });
                Images.Add(item);
            }

            var plainFiles = Directory.GetFiles(FileSystem.AppDataDirectory, "plain_*.*");
            foreach (var file in plainFiles)
            {
                var item = new ImageItem
                {
                    DecryptedFilePath = file,
                    IsDecrypted = true
                };
                item.EncryptCommand = new Command(async () => await EncryptImageAsync(item));
                item.DeleteCommand = new Command(() =>
                {
                    File.Delete(file);
                    Images.Remove(item);
                });
                Images.Add(item);
            }
        }

        public ICommand ResetCommand => new Command(async () =>
        {
            bool confirm = await Shell.Current.DisplayAlert("Nulstil", "Er du sikker på, at du vil nulstille appen? Alle billeder slettes ikke automatisk.", "Ja", "Nej");
            if (!confirm) return;

            AuthService.ClearUserDataAsync();
            await Shell.Current.GoToAsync("//LoginPage");
        });


        public event PropertyChangedEventHandler PropertyChanged;
    }
    public class ImageItem : INotifyPropertyChanged
    {
        public string EncryptedFilePath { get; set; }
        public string DecryptedFilePath { get; set; }
        public bool IsDecrypted { get; set; }
        public ICommand DecryptCommand { get; set; }
        public ICommand EncryptCommand { get; set; }
        public ICommand DeleteCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}
