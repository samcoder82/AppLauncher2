using Microsoft.Extensions.Configuration;

namespace AppLauncher;

static class Program
{

    /**
     * <summary>The Configuration</summary>
     */
    public static IConfiguration? Configuration;
    
    /**
     * <summary>The main entry point for the application</summary>
     */
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        
        var builder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        Configuration = builder.Build();
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        
        Application.Run(new AppLauncherContext());
    }
}