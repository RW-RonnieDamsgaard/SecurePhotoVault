using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SecurePhotoVaultMAUI.SecureStorage
{
    public class ImageCryptoService
    {
        public static async Task EncryptImageAsync(string inputPath, string outputPath)
        {
            var key = await CryptoService.GetOrCreateKeyAsync();

            using var aes = Aes.Create();
            aes.Key = key;
            aes.GenerateIV();

            await using var fileStream = new FileStream(outputPath, FileMode.Create);
            await fileStream.WriteAsync(aes.IV); // Write IV first

            await using var inputFileStream = new FileStream(inputPath, FileMode.Open);

            // CryptoStream must be disposed before fileStream
            using (var cryptoStream = new CryptoStream(fileStream, aes.CreateEncryptor(), CryptoStreamMode.Write, leaveOpen: true))
            {
                await inputFileStream.CopyToAsync(cryptoStream);
            }
        }


        public static async Task DecryptImageAsync(string encryptedPath, string outputPath)
        {
            var key = await CryptoService.GetOrCreateKeyAsync();

            await using var inputFile = new FileStream(encryptedPath, FileMode.Open, FileAccess.Read);
            byte[] iv = new byte[16];
            int read = await inputFile.ReadAsync(iv, 0, iv.Length);

            if (read != 16)
                throw new InvalidOperationException("Could not read IV from encrypted file.");

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            await using var outputFile = new FileStream(outputPath, FileMode.Create, FileAccess.Write);

            try
            {
                using (var cryptoStream = new CryptoStream(inputFile, aes.CreateDecryptor(), CryptoStreamMode.Read, leaveOpen: true))
                {
                    await cryptoStream.CopyToAsync(outputFile);
                }
            }
            catch (CryptographicException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Decryption failed: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Key: {Convert.ToBase64String(key)}");
                System.Diagnostics.Debug.WriteLine($"IV: {Convert.ToBase64String(iv)}");
                System.Diagnostics.Debug.WriteLine($"Encrypted file size: {inputFile.Length}");
                throw;
            }
        }
    }
}
