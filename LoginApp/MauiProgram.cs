using LoginApp.Services;
using LoginApp.Views;
using Microsoft.Extensions.Logging;

namespace LoginApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>() // חשוב מאוד: App עצמו נרשם
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");

                });

            // ✅ רישום שירותים
            builder.Services.AddSingleton<IUserSessionService, UserSessionService>();

            // ✅ רישום עמודים
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<UsersListPage>();
            builder.Services.AddTransient<MainPage>();

            // ✅ רישום AppShell דרך DI
            builder.Services.AddSingleton<AppShell>();

            // ✅ רישום App עצמו עם גישה ל־IServiceProvider
            builder.Services.AddSingleton<App>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
