using Microsoft.Extensions.Logging;
using Sonara.App.ViewModels;

namespace Sonara.App
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

            builder.Services.AddSonaraServices();                 // сервисы (заглушки)
            builder.Services.AddTransient<LibraryViewModel>();    // ViewModel
            builder.Services.AddSingleton<PlayerViewModel>();
            builder.Services.AddTransient<LibraryPage>();         // страница
            builder.Services.AddTransient<SettingsViewModel>();
            builder.Services.AddTransient<SettingsPage>();
            
            return builder.Build();
        }
    }
}
