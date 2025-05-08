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
            var mainWindow = new MainWindow 
            {
                DataContext = new MainWindowViewModel()
            };
            desktop.MainWindow = mainWindow;
            DrawCanvasViewModel.GetStorageProvider = () => TopLevel.GetTopLevel(mainWindow)?.StorageProvider;
        }
        base.OnFrameworkInitializationCompleted();
    }
}