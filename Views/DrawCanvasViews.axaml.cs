using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using PixelWallE.ViewModels;
using System.ComponentModel;
using WALLE;
namespace PixelWallE.Views;

public partial class DrawCanvasView : UserControl
{
    private DrawCanvasViewModel? _viewModel;
    private readonly IBrush _walleMarkerBrush = Brushes.Brown;
    public DrawCanvasView()
    {
        InitializeComponent();
        this.FindControl<Canvas>("DrawCanvas")!.SizeChanged += OnCanvasSizeChanged;
        DataContextChanged += OnDataContextChanged;
    }
    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            _viewModel.CanvasNeedsRedraw -= OnCanvasNeedsRedraw;
        }
        _viewModel = DataContext as DrawCanvasViewModel;
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            _viewModel.CanvasNeedsRedraw += OnCanvasNeedsRedraw;
            DrawGrid();
        }
        else ClearGrid();
    }
    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DrawCanvasViewModel.CanvasDimension)) DrawGrid();
    }
    private void OnCanvasNeedsRedraw(object? sender, EventArgs e)
    {
        DrawGrid();
    }
    private void OnCanvasSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        DrawGrid();
    }
    private void ClearGrid()
    {
        var canvas = this.FindControl<Canvas>("DrawCanvas");
        if (canvas != null) canvas.Children.Clear();
    }
    private void DrawGrid()
    {
        var canvas = this.FindControl<Canvas>("DrawCanvas");
        if (canvas == null || _viewModel == null)
        {
            ClearGrid();
            return;
        }
        int dimension = _viewModel.CanvasDimension;
        if (dimension <= 0)
        {
            ClearGrid();
            return;
        }
        canvas.Children.Clear();

        double canvasWidth = canvas.Bounds.Width;
        double canvasHeight = canvas.Bounds.Height;

        if (canvasWidth <= 0 || canvasHeight <= 0) return;
        double maxDrawableSize = Math.Min(canvasWidth, canvasHeight);
        double cellSize = maxDrawableSize / dimension;
        double totalGridWidth = cellSize * dimension;
        double totalGridHeight = cellSize * dimension;
        double offsetX = (canvasWidth - totalGridWidth) / 2.0;
        double offsetY = (canvasHeight - totalGridHeight) / 2.0;
        offsetX = Math.Max(0, offsetX);
        offsetY = Math.Max(0, offsetY);
        int walleX = -1;
        int walleY = -1;
        try
        {
            walleX = Walle.GetActualX();
            walleY = Walle.GetActualY();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting Walle position: {ex.Message}");
        }
        for (int i = 0; i < dimension; i++)
        {
            for (int j = 0; j < dimension; j++)
            {
                var rect = new Rectangle
                {
                    Width = cellSize,
                    Height = cellSize,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 0.5
                };
                IBrush cellFillBrush;
                if (j == walleX && i == walleY) cellFillBrush = _walleMarkerBrush;
                else
                {
                    string colorString = "White";
                    try
                    {
                        if (!Canva.IsOutRange(j, i)) colorString = Canva.GetCellColor(j, i);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        colorString = "Pink";
                    }
                    catch (NullReferenceException)
                    {
                        colorString = "Pink";
                    }
                    cellFillBrush = GetBrushFromString(colorString);
                }
                rect.Fill = cellFillBrush;
                Canvas.SetLeft(rect, offsetX + (j * cellSize));
                Canvas.SetTop(rect, offsetY + (i * cellSize));
                canvas.Children.Add(rect);
            }
        }
    }
    private IBrush GetBrushFromString(string? colorString)
    {
        string normalizedColor = colorString?.Trim() ?? "Transparent";
        switch (normalizedColor.ToLowerInvariant())
        {
            case "red": return Brushes.Red;
            case "blue": return Brushes.Blue;
            case "green": return Brushes.Green;
            case "yellow": return Brushes.Yellow;
            case "orange": return Brushes.Orange;
            case "purple": return Brushes.Purple;
            case "black": return Brushes.Black;
            case "white": return Brushes.White;
            case "transparent": return Brushes.Transparent;
            default: return Brushes.Pink;
        }
    }
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            _viewModel.CanvasNeedsRedraw -= OnCanvasNeedsRedraw;
        }
        var canvas = this.FindControl<Canvas>("DrawCanvas");
        if (canvas != null) canvas.SizeChanged -= OnCanvasSizeChanged;
        DataContextChanged -= OnDataContextChanged;
        base.OnDetachedFromVisualTree(e);
    }
    
}