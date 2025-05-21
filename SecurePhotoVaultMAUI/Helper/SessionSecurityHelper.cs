using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecurePhotoVaultMAUI.SecureStorage;

namespace SecurePhotoVaultMAUI.Helper
{
    public static class SessionSecurityHelper
    {
        public static async Task RekrypterAllePlainBillederAsync()
        {
            var appDir = FileSystem.AppDataDirectory;
            var plainFiles = Directory.GetFiles(appDir, "plain_*.*");

            foreach (var file in plainFiles)
            {
                try
                {
                    await ImageCryptoService.EncryptImageAsync(file);
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Fejl ved rekryptering af {file}: {ex.Message}");
                }
            }
        }
    }
}
