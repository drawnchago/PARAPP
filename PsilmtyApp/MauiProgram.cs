using Microsoft.Extensions.Logging;
using PsilmtyApp.Services;

namespace PsilmtyApp
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
                });

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            // ── API HTTP Client (typed) ───────────────────────────
            builder.Services.AddSingleton<SessionStore>();
            builder.Services.AddSingleton<SessionService>();
            builder.Services.AddHttpClient<ApiService>(client =>
            {
                // Cambia por la IP/URL de tu API en producción
                client.BaseAddress = new Uri("https://api.sanisidrolabradormty.com/");
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            return builder.Build();
        }
    }
}
