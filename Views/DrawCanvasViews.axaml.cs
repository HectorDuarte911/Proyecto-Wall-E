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
    private void OnCanvasNeedsRedraw(object? sender, EventArgs e) => DrawGrid();
    private void OnCanvasSizeChanged(object? sender, SizeChangedEventArgs e) => DrawGrid();
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
        int walleX = Walle.GetActualX();
        int walleY = Walle.GetActualY();
        for (int i = 0; i < dimension; i++)
        {
            for (int j = 0; j < dimension; j++)
            {
                double cellLeft = offsetX + (j * cellSize);
                double cellTop = offsetY + (i * cellSize);
                var rect = new Rectangle
                {
                    Width = cellSize,
                    Height = cellSize,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 0.5
                };
                Canvas.SetLeft(rect, cellLeft);
                Canvas.SetTop(rect, cellTop);
                if (j == walleX && i == walleY && !Canva.IsOutRange(j, i))
                {
                    rect.Fill = Brushes.White;
                    canvas.Children.Add(rect);
                    DrawWalleIcon(canvas, cellLeft, cellTop, cellSize);
                }
                else
                {
                    string colorString = "White";
                    try
                    {
                        if (!Canva.IsOutRange(j, i)) colorString = Canva.GetCellColor(j, i);
                    }
                    catch (IndexOutOfRangeException) { colorString = "Pink"; }
                    catch (NullReferenceException) { colorString = "Pink"; }
                    rect.Fill = GetBrushFromString(colorString);
                    canvas.Children.Add(rect);
                }
            }
        }
    }
    private void DrawWalleIcon(Canvas canvas, double cellX, double cellY, double cellSize)
    {
        IBrush headBrush = Brushes.Goldenrod;
        IBrush eyeSocketBrush = Brushes.DarkSlateGray;
        IBrush eyeLensBrush = Brushes.LightSkyBlue;
        IBrush antennaBrush = Brushes.DimGray;
        IBrush neckRingBrush = Brushes.SaddleBrown;
        IBrush mouthBrush = Brushes.DarkSlateGray;
        double neckRingHeight = cellSize * 0.15;
        double neckRingWidth = cellSize * 0.65;
        var neckRing = new Ellipse
        {
            Width = neckRingWidth,
            Height = neckRingHeight,
            Fill = neckRingBrush
        };
        Canvas.SetLeft(neckRing, cellX + (cellSize - neckRingWidth) / 2);
        Canvas.SetTop(neckRing, cellY + cellSize * 0.55);
        canvas.Children.Add(neckRing);
        double headWidth = cellSize * 0.6;
        double headHeight = cellSize * 0.5;
        var head = new Ellipse
        {
            Width = headWidth,
            Height = headHeight,
            Fill = headBrush
        };
        double headTop = cellY + cellSize * 0.2;
        double headLeft = cellX + (cellSize - headWidth) / 2;
        Canvas.SetLeft(head, cellX + (cellSize - headWidth) / 2);
        Canvas.SetTop(head, cellY + cellSize * 0.2);
        canvas.Children.Add(head);
        double mouthWidth = headWidth;
        double mouthHeight = cellSize * 0.04;
        var mouth = new Rectangle
        {
            Width = mouthWidth,
            Height = mouthHeight,
            Fill = mouthBrush
        };
        Canvas.SetLeft(mouth, headLeft + (headWidth - mouthWidth) / 2);
        Canvas.SetTop(mouth, headTop + headHeight * 0.65);
        canvas.Children.Add(mouth);
        double eyeSocketDiameter = cellSize * 0.3;
        double eyeLensDiameter = eyeSocketDiameter * 0.6;
        var leftEyeSocket = new Ellipse
        {
            Width = eyeSocketDiameter,
            Height = eyeSocketDiameter,
            Fill = eyeSocketBrush
        };
        Canvas.SetLeft(leftEyeSocket, cellX + cellSize * 0.15 - eyeSocketDiameter / 2);
        Canvas.SetTop(leftEyeSocket, cellY + cellSize * 0.25);
        canvas.Children.Add(leftEyeSocket);
        var leftEyeLens = new Ellipse
        {
            Width = eyeLensDiameter,
            Height = eyeLensDiameter,
            Fill = eyeLensBrush
        };
        Canvas.SetLeft(leftEyeLens, Canvas.GetLeft(leftEyeSocket) + (eyeSocketDiameter - eyeLensDiameter) / 2);
        Canvas.SetTop(leftEyeLens, Canvas.GetTop(leftEyeSocket) + (eyeSocketDiameter - eyeLensDiameter) / 2);
        canvas.Children.Add(leftEyeLens);
        var rightEyeSocket = new Ellipse
        {
            Width = eyeSocketDiameter,
            Height = eyeSocketDiameter,
            Fill = eyeSocketBrush
        };
        Canvas.SetLeft(rightEyeSocket, cellX + cellSize * 0.85 - eyeSocketDiameter / 2);
        Canvas.SetTop(rightEyeSocket, cellY + cellSize * 0.25);
        canvas.Children.Add(rightEyeSocket);
        var rightEyeLens = new Ellipse
        {
            Width = eyeLensDiameter,
            Height = eyeLensDiameter,
            Fill = eyeLensBrush
        };
        Canvas.SetLeft(rightEyeLens, Canvas.GetLeft(rightEyeSocket) + (eyeSocketDiameter - eyeLensDiameter) / 2);
        Canvas.SetTop(rightEyeLens, Canvas.GetTop(rightEyeSocket) + (eyeSocketDiameter - eyeLensDiameter) / 2);
        canvas.Children.Add(rightEyeLens);
        double antennaStalkWidth = cellSize * 0.08;
        double antennaStalkHeight = cellSize * 0.15;
        double antennaTopWidth = cellSize * 0.12;
        double antennaTopHeight = cellSize * 0.1;
        var antennaStalk = new Rectangle
        {
            Width = antennaStalkWidth,
            Height = antennaStalkHeight,
            Fill = antennaBrush
        };
        double antennaStalkLeft = cellX + (cellSize - antennaStalkWidth) / 2;
        double antennaStalkTop = headTop - antennaStalkHeight + (cellSize * 0.05);
        Canvas.SetLeft(antennaStalk, antennaStalkLeft);
        Canvas.SetTop(antennaStalk, antennaStalkTop);
        canvas.Children.Add(antennaStalk);
        var antennaTop = new Ellipse
        {
            Width = antennaTopWidth,
            Height = antennaTopHeight,
            Fill = antennaBrush
        };
        Canvas.SetLeft(antennaTop, cellX + (cellSize - antennaTopWidth) / 2);
        Canvas.SetTop(antennaTop, Canvas.GetTop(antennaStalk) - antennaTopHeight * 0.7);
        canvas.Children.Add(antennaTop);
    }
    private IBrush GetBrushFromString(string? colorString)
    {
        string normalizedColor = colorString?.Trim() ?? "Transparent";
        return normalizedColor.ToLowerInvariant() switch
        {
            "red" => Brushes.Red,
            "blue" => Brushes.Blue,
            "green" => Brushes.Green,
            "yellow" => Brushes.Yellow,
            "orange" => Brushes.Orange,
            "purple" => Brushes.Purple,
            "black" => Brushes.Black,
            "white" => Brushes.White,
            "transparent" => Brushes.Transparent,
            "goldenrod" => Brushes.Goldenrod,
            "darkslategray" => Brushes.DarkSlateGray,
            "lightskyblue" => Brushes.LightSkyBlue,
            "dimgray" => Brushes.DimGray,
            "saddlebrown" => Brushes.SaddleBrown,
            _ => Brushes.Pink,
        };
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