using Microsoft.Extensions.Logging;
using SecurePhotoVaultMAUI.ViewModels;
using SecurePhotoVaultMAUI.Views;

namespace SecurePhotoVaultMAUI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });


#if DEBUG
    		builder.Logging.AddDebug();
#endif

            // Dependency injection (valgfrit men anbefales)
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<MainPageViewModel>();

            return builder.Build();
        }
    }
}
