using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PsilmtyApp.Services;

namespace PsilmtyApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            using var settingsStream = typeof(MauiProgram).Assembly
                .GetManifestResourceStream("PsilmtyApp.appsettings.json")
                ?? throw new InvalidOperationException("The embedded appsettings.json file was not found.");
            builder.Configuration.AddJsonStream(settingsStream);
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
                var baseUrl = builder.Configuration["Api:BaseUrl"]
                    ?? throw new InvalidOperationException("Api:BaseUrl is missing from appsettings.json.");
                var timeoutSeconds = builder.Configuration.GetValue("Api:TimeoutSeconds", 30);
                client.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
                client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            });

            return builder.Build();
        }
    }
}
