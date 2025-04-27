using Avalonia;
using System.Diagnostics;
namespace PixelWallE;

internal class Program
{
    public static void Main(string[] args) => BuildAvaloniaApp()
    .StartWithClassicDesktopLifetime(args); 
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .WithInterFont() 
        .LogToTrace(); 
}
