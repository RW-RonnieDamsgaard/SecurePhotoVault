using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Isopoh.Cryptography.Argon2;


namespace SecurePhotoVaultMAUI.Services
{
    public static class AuthService
    {
        private const string KeyHash = "user-password-hash";

        public static async Task<bool> RegisterAsync(string password)
        {
            var saltBytes = new byte[16];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            var salt = Convert.ToBase64String(saltBytes);
            var hash = Argon2.Hash(password);
            await Microsoft.Maui.Storage.SecureStorage.SetAsync(KeyHash, hash);
            await Microsoft.Maui.Storage.SecureStorage.SetAsync("user-salt", salt);
            return true;
        }

        public static async Task<bool> LoginAsync(string password)
        {
            var storedHash = await Microsoft.Maui.Storage.SecureStorage.GetAsync(KeyHash);
            if (string.IsNullOrEmpty(storedHash))
                return false;

            return Argon2.Verify(storedHash, password);
        }

        public static async Task SetLoginStatusAsync(bool isLoggedIn)
        {
            await Microsoft.Maui.Storage.SecureStorage.SetAsync("is-logged-in", isLoggedIn.ToString());
        }

        public static async Task<bool> IsLoggedInAsync()
        {
            var result = await Microsoft.Maui.Storage.SecureStorage.GetAsync("is-logged-in");
            return bool.TryParse(result, out bool val) && val;
        }

        public static async Task LogoutAsync()
        {
            await Microsoft.Maui.Storage.SecureStorage.SetAsync("is-logged-in", "false");
        }
        public static void ClearUserDataAsync()
        {
            Microsoft.Maui.Storage.SecureStorage.Default.Remove("user-salt");
            Microsoft.Maui.Storage.SecureStorage.Default.Remove("user-password-hash");
        }
    }
}
