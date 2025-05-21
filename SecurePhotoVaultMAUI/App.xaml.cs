using SecurePhotoVaultMAUI.Helper;
using SecurePhotoVaultMAUI.Services;
using SecurePhotoVaultMAUI.Views;
using System.Timers;


namespace SecurePhotoVaultMAUI
{
    public partial class App : Application
    {
        private static DateTime _lastInteractionTime = DateTime.Now;
        private static readonly TimeSpan _sessionTimeout = TimeSpan.FromMinutes(2);
        private static System.Timers.Timer _sessionTimer;

        public App()
        {
            InitializeComponent();

            Routing.RegisterRoute("MainPage", typeof(MainPage));
            Routing.RegisterRoute("LoginPage", typeof(LoginPage));

            MainPage = new AppShell();
            StartSessionTimer();

        }

        public static void TouchSession()
        {
            _lastInteractionTime = DateTime.Now;
        }

        private void StartSessionTimer()
        {
            _sessionTimer = new System.Timers.Timer(10000);
            _sessionTimer.Elapsed += async (sender, args) =>
            {
                var now = DateTime.Now;
                if ((now - _lastInteractionTime) > _sessionTimeout)
                {
                    _sessionTimer.Stop();

                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await SessionSecurityHelper.RekrypterAllePlainBillederAsync();
                        await AuthService.LogoutAsync();
                        await Shell.Current.GoToAsync("//LoginPage");
                    });
                }
            };
            _sessionTimer.Start();
        }

    }
}
