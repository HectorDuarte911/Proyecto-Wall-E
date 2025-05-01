using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WALLE;
namespace PixelWallE.ViewModels;
public partial class DrawCanvasViewModel : ObservableObject
{
    public int MaxCanvasDimension => 300;
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ResizeCanvasCommand))]
    private int _canvasDimension;
    public event EventHandler? CanvasNeedsRedraw;
    public DrawCanvasViewModel()
    {
        Canva.InitCanvas();
        Walle.Spawn(Canva.GetCanvasSize() / 2, Canva.GetCanvasSize() / 2);
        Walle.Color("Transparent");
        Walle.Size(1);
        _canvasDimension = Canva.GetCanvasSize();
    }
    [RelayCommand(CanExecute = nameof(CanResizeCanvas))]
    private void ResizeCanvas()
    {
        int newDim = Math.Clamp(CanvasDimension, 1, MaxCanvasDimension);
        if (newDim != CanvasDimension) CanvasDimension = newDim;
        Canva.RedimensionCanvas(newDim);
    }
    private bool CanResizeCanvas()
    {
        return CanvasDimension >= 2 && CanvasDimension <= MaxCanvasDimension;
    }
    public void SignalCanvasUpdate()
    {
        CanvasNeedsRedraw?.Invoke(this, EventArgs.Empty);
    }
    public int GetBackendCanvasDimension() => Canva.GetCanvasSize();
}