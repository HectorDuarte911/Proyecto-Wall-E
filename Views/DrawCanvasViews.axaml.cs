using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using PixelWallE.ViewModels;
using System;
using System.ComponentModel;  
namespace PixelWallE.Views;

public partial class DrawCanvasView : UserControl
{
    private DrawCanvasViewModel? _viewModel;

    public DrawCanvasView()
    {
        InitializeComponent();
        this.FindControl<Canvas>("DrawCanvas")!.SizeChanged += OnCanvasSizeChanged;
        this.DataContextChanged += OnDataContextChanged;
    }
    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
        }
        _viewModel = DataContext as DrawCanvasViewModel;
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            DrawGrid();
        }
        else
        {
            ClearGrid();
        }
    }
    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DrawCanvasViewModel.CanvasDimension))
        {
            DrawGrid();
        }
    }
    private void OnCanvasSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        DrawGrid();
    }

    private void ClearGrid()
    {
        var canvas = this.FindControl<Canvas>("DrawCanvas");
        if (canvas != null)
        {
            canvas.Children.Clear();
        }
    }
    private void DrawGrid()
    {
        var canvas = this.FindControl<Canvas>("DrawCanvas");
        if (canvas == null || _viewModel == null || _viewModel.CanvasDimension <= 0)
        {
            ClearGrid();
            return;
        }
        canvas.Children.Clear();
        int dimension = _viewModel.CanvasDimension; 
        double canvasWidth = canvas.Bounds.Width;
        double canvasHeight = canvas.Bounds.Height;

        if (canvasWidth <= 0 || canvasHeight <= 0) return;
        double maxDrawableSize = Math.Min(canvasWidth, canvasHeight);
        double cellSize = maxDrawableSize / dimension;
        double offsetX = (canvasWidth - (cellSize * dimension)) / 2.0;
        double offsetY = (canvasHeight - (cellSize * dimension)) / 2.0;
        for (int i = 0; i < dimension; i++) 
        {
            for (int j = 0; j < dimension; j++)
            {
                var rect = new Rectangle();
                rect.Width = cellSize;
                rect.Height = cellSize;
                rect.Stroke = Brushes.LightGray;
                rect.StrokeThickness = 0.5;
                Canvas.SetLeft(rect, offsetX + (j * cellSize));
                Canvas.SetTop(rect, offsetY + (i * cellSize));
                canvas.Children.Add(rect);
            }
        }
    }
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
        }
        var canvas = this.FindControl<Canvas>("DrawCanvas");
        if (canvas != null)
        {
            canvas.SizeChanged -= OnCanvasSizeChanged;
        }
        this.DataContextChanged -= OnDataContextChanged;

        base.OnDetachedFromVisualTree(e);
    }
}