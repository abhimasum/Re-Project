using Microsoft.Extensions.Logging;
using Rework.Desktop.Services;

namespace Rework.Desktop;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();
        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // Typed client for the shared API. The base address is stored in preferences, with a local default.
        var apiBase = Preferences.Default.Get("api.baseUrl", "https://localhost:7180");
        builder.Services.AddHttpClient<ReworkApiClient>(c => c.BaseAddress = new Uri(apiBase));

        return builder.Build();
    }
}
