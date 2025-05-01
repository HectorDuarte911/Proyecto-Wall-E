using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using PixelWallE.ViewModels;

namespace PixelWallE;
public partial class App : Application
{
    public override void Initialize()
    { 
        AvaloniaXamlLoader.Load(this);
    }
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindowViewModel = new MainWindowViewModel();
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainWindowViewModel
            };
            desktop.MainWindow.WindowState = WindowState.Maximized;
        }
        base.OnFrameworkInitializationCompleted();
    }
}