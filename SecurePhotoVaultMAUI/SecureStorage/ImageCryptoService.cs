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
        public static async Task EncryptImageAsync(string inputPath)
        {
            var key = await CryptoService.GetOrCreateKeyAsync();

            using var aes = Aes.Create();
            aes.Key = key;
            aes.GenerateIV();

            var outputFileName = $"encrypted_{Guid.NewGuid()}.img";
            var outputPath = Path.Combine(FileSystem.AppDataDirectory, outputFileName);

            // Write IV + ciphertext to a memory stream first
            using var ms = new MemoryStream();
            await ms.WriteAsync(aes.IV);

            await using (var inputFileStream = new FileStream(inputPath, FileMode.Open))
            using (var cryptoStream = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write, leaveOpen: true))
            {
                await inputFileStream.CopyToAsync(cryptoStream);
            }

            // Compute HMAC-SHA256 over IV + ciphertext
            using var hmac = new HMACSHA256(key);
            ms.Position = 0;
            var data = ms.ToArray();
            var mac = hmac.ComputeHash(data);

            // Write HMAC + IV + ciphertext to the output file
            await using (var fileStream = new FileStream(outputPath, FileMode.Create))
            {
                await fileStream.WriteAsync(mac);
                await fileStream.WriteAsync(data);
            }
        }


        public static async Task DecryptImageAsync(string encryptedPath, string outputPath)
        {
            var key = await CryptoService.GetOrCreateKeyAsync();

            await using var inputFile = new FileStream(encryptedPath, FileMode.Open, FileAccess.Read);

            // Read HMAC
            byte[] storedMac = new byte[32];
            int readMac = await inputFile.ReadAsync(storedMac, 0, storedMac.Length);
            if (readMac != 32)
                throw new InvalidOperationException("Could not read HMAC from encrypted file.");

            // Read IV + ciphertext
            byte[] rest = new byte[inputFile.Length - 32];
            int readRest = await inputFile.ReadAsync(rest, 0, rest.Length);
            if (readRest != rest.Length)
                throw new InvalidOperationException("Could not read encrypted data.");

            // Verify HMAC
            using (var hmac = new HMACSHA256(key))
            {
                var computedMac = hmac.ComputeHash(rest);
                if (!storedMac.SequenceEqual(computedMac))
                    await Shell.Current.DisplayAlert("Advarsel", "Filen er blevet ændret eller beskadiget (tampering opdaget).", "OK");
                    await Shell.Current.GoToAsync("//MainPage");
                    return;
            }

            // Extract IV
            byte[] iv = new byte[16];
            Array.Copy(rest, 0, iv, 0, 16);

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            await using var outputFile = new FileStream(outputPath, FileMode.Create, FileAccess.Write);

            // Decrypt ciphertext (rest[16..])
            using (var ms = new MemoryStream(rest, 16, rest.Length - 16))
            using (var cryptoStream = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
            {
                await cryptoStream.CopyToAsync(outputFile);
            }
        }
    }
}
