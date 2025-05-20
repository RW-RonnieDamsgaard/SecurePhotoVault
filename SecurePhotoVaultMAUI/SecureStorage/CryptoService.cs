using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.Maui.Storage;

namespace SecurePhotoVaultMAUI.SecureStorage
{
    public class CryptoService
    {
        private const string KeyName = "aes-key";

        public static async Task<byte[]> GetOrCreateKeyAsync()
        {
            var existingKey = await Microsoft.Maui.Storage.SecureStorage.GetAsync(KeyName);
            if (existingKey != null)
                return Convert.FromBase64String(existingKey);

            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.GenerateKey();

            var base64Key = Convert.ToBase64String(aes.Key);
            await Microsoft.Maui.Storage.SecureStorage.SetAsync(KeyName, base64Key);

            return aes.Key;
        }
    }
}
