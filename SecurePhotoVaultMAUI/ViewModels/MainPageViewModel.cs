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
        public ICommand AddImageCommand { get; }

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
            AddImageCommand = new Command(async () => await AddImageAsync());
            EncryptCommand = new Command<ImageItem>(async item => await EncryptImageAsync(item));
            DecryptCommand = new Command<ImageItem>(async item => await DecryptImageAsync(item));
            //EncryptCommand = new Command(async () => await EncryptImageAsync());
            //DecryptCommand = new Command(async () => await DecryptImageAsync());
            LogoutCommand = new Command(async () => await LogoutAsync());

            CheckLoginStatus();
            LoadImagesAsync();
        }

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


        private async Task EncryptImageAsync(ImageItem item)
        {
            if (!string.IsNullOrEmpty(item.DecryptedFilePath) && File.Exists(item.DecryptedFilePath))
            {
                await ImageCryptoService.EncryptImageAsync(item.DecryptedFilePath);
                File.Delete(item.DecryptedFilePath);
                item.DecryptedFilePath = null;
                item.IsDecrypted = false;
                item.OnPropertyChanged(nameof(item.DecryptedFilePath));
                item.OnPropertyChanged(nameof(item.IsDecrypted));
                await LoadImagesAsync(); // Opdater listen
            }
        }

        private async Task DecryptImageAsync(ImageItem item)
        {
            var decryptedFile = Path.Combine(
                FileSystem.AppDataDirectory,
                $"plain_{Guid.NewGuid()}.jpg"
            );

            await ImageCryptoService.DecryptImageAsync(item.EncryptedFilePath, decryptedFile);

            // Slet den krypterede fil
            File.Delete(item.EncryptedFilePath);

            // Opdater item til at være et dekrypteret billede
            item.EncryptedFilePath = null;
            item.DecryptedFilePath = decryptedFile;
            item.IsDecrypted = true;
            item.EncryptCommand = new Command(async () => await EncryptImageAsync(item));
            item.DecryptCommand = null;
            item.OnPropertyChanged(nameof(item.EncryptedFilePath));
            item.OnPropertyChanged(nameof(item.DecryptedFilePath));
            item.OnPropertyChanged(nameof(item.IsDecrypted));

            await LoadImagesAsync(); // Opdater listen, så visningen matcher filsystemet
        }

        // Fjern denne metode, da den ikke længere matcher filnavnene
        // private async Task DecryptImageAsync() { ... }

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

            // Indlæs krypterede billeder
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

            // Indlæs ukrypterede billeder (plain)
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
        private string CopyImageToTemp(string sourcePath)
        {
            var tempPath = Path.Combine(FileSystem.CacheDirectory, Path.GetFileName(sourcePath));
            File.Copy(sourcePath, tempPath, true);
            return tempPath;
        }

        public ObservableCollection<ImageItem> Images { get; set; } = new();

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
