// PixelWallE.ViewModels/DrawCanvasViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WALLE;
using System; // Add this

namespace PixelWallE.ViewModels;

public partial class DrawCanvasViewModel : ObservableObject
{
    public int MaxCanvasDimension => 300;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ResizeCanvasCommand))]
    private int _canvasDimension;

    // Event to notify the View that the canvas content needs refreshing
    public event EventHandler? CanvasNeedsRedraw; // <-- ADD THIS

    public DrawCanvasViewModel()
    {
        Canva.InitCanvas(); // Initialize the backend canvas
        Walle.Spawn(Canva.GetCanvasSize() / 2, Canva.GetCanvasSize() / 2); // Start Walle somewhere reasonable
        Walle.Color("Transparent"); // Default color
        Walle.Size(1);       // Default size
        _canvasDimension = Canva.GetCanvasSize();
    }

    [RelayCommand(CanExecute = nameof(CanResizeCanvas))]
    private void ResizeCanvas()
    {
        int newDim = Math.Clamp(CanvasDimension, 1, MaxCanvasDimension);
        if (newDim != CanvasDimension)
        {
            CanvasDimension = newDim; // This setter will trigger PropertyChanged
        }
        Canva.RedimensionCanvas(newDim);
        // No need to explicitly call DrawGrid here, the View handles CanvasDimension changes
    }

    private bool CanResizeCanvas()
    {
        // Allow resizing even if the script hasn't run yet
        return CanvasDimension >= 2 && CanvasDimension <= MaxCanvasDimension;
    }

    // Method for the MainWindowViewModel to call after interpretation
    public void SignalCanvasUpdate() // <-- ADD THIS METHOD
    {
        // Trigger the event to tell the View to redraw
        CanvasNeedsRedraw?.Invoke(this, EventArgs.Empty);
    }

    // Expose the backend canvas dimension if needed elsewhere,
    // but the UI primarily uses CanvasDimension property
    public int GetBackendCanvasDimension() => Canva.GetCanvasSize();
}