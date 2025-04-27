// PixelWallE.Views/DrawCanvasView.cs
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using PixelWallE.ViewModels;
using System;
using System.ComponentModel;
using WALLE; // Needed for Canva.GetCellColor

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
            _viewModel.CanvasNeedsRedraw -= OnCanvasNeedsRedraw; // <-- UNSUBSCRIBE
        }

        _viewModel = DataContext as DrawCanvasViewModel;

        if (_viewModel != null)
        {
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            _viewModel.CanvasNeedsRedraw += OnCanvasNeedsRedraw; // <-- SUBSCRIBE
            // Initial draw when DataContext is set
            DrawGrid();
        }
        else
        {
            ClearGrid();
        }
    }

    // Handler for the ViewModel's PropertyChanged event
    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Redraw if the dimension changes
        if (e.PropertyName == nameof(DrawCanvasViewModel.CanvasDimension))
        {
            DrawGrid();
        }
    }

    // Handler for the ViewModel's custom event
    private void OnCanvasNeedsRedraw(object? sender, EventArgs e) // <-- ADD HANDLER
    {
        // The ViewModel signaled that the backend data changed, so redraw
        DrawGrid();
    }


    private void OnCanvasSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        // Redraw when the physical canvas control resizes
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
        if (canvas == null || _viewModel == null)
        {
            ClearGrid();
            return;
        }

        // Use the dimension from the ViewModel, which should be kept in sync with the backend
        int dimension = _viewModel.CanvasDimension;
        if (dimension <= 0)
        {
             ClearGrid();
             return;
        }


        canvas.Children.Clear(); // Clear previous drawings

        double canvasWidth = canvas.Bounds.Width;
        double canvasHeight = canvas.Bounds.Height;

        if (canvasWidth <= 0 || canvasHeight <= 0) return; // Nothing to draw on yet

        // Calculate cell size based on the smaller dimension of the canvas panel
        double maxDrawableSize = Math.Min(canvasWidth, canvasHeight);
        double cellSize = maxDrawableSize / dimension;

        // Calculate offsets to center the grid within the canvas panel
        double totalGridWidth = cellSize * dimension;
        double totalGridHeight = cellSize * dimension;
        double offsetX = (canvasWidth - totalGridWidth) / 2.0;
        double offsetY = (canvasHeight - totalGridHeight) / 2.0;

        // Ensure offsets are not negative if canvas is somehow smaller than grid
        offsetX = Math.Max(0, offsetX);
        offsetY = Math.Max(0, offsetY);


        for (int i = 0; i < dimension; i++) // Represents Row (Y)
        {
            for (int j = 0; j < dimension; j++) // Represents Column (X)
            {
                var rect = new Rectangle
                {
                    Width = cellSize,
                    Height = cellSize,
                    // Subtle stroke for the grid lines
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 0.5
                };

                // Get color from the backend static canvas
                // Assuming Canva.GetCellColor uses (column, row) or (x, y)
                string colorString = "White"; // Default
                try
                {
                    // Prevent accessing out of bounds if backend dimension mismatches UI
                    if (!Canva.IsOutRange(j, i))
                    {
                        colorString = Canva.GetCellColor(j, i);
                    }
                }
                catch(IndexOutOfRangeException)
                {
                    // Handle cases where backend canvas might not be ready or dimension mismatch
                     colorString = "Pink"; // Error color
                }
                 catch(NullReferenceException)
                {
                    // Handle cases where backend canvas might not be initialized
                     colorString = "Pink"; // Error color
                }


                // Set fill color based on the string
                rect.Fill = GetBrushFromString(colorString); // <-- SET FILL COLOR

                // Position the rectangle on the Avalonia Canvas
                Canvas.SetLeft(rect, offsetX + (j * cellSize));
                Canvas.SetTop(rect, offsetY + (i * cellSize));

                canvas.Children.Add(rect);
            }
        }
    }

    // Helper to convert color names to Avalonia Brushes
    private IBrush GetBrushFromString(string? colorString) // <-- ADD HELPER
    {
        // Normalize input
        string normalizedColor = colorString?.Trim() ?? "Transparent";

        switch (normalizedColor.ToLowerInvariant()) // Case-insensitive comparison
        {
            case "red": return Brushes.Red;
            case "blue": return Brushes.Blue;
            case "green": return Brushes.Green;
            case "yellow": return Brushes.Yellow;
            case "orange": return Brushes.Orange;
            case "purple": return Brushes.Purple;
            case "black": return Brushes.Black;
            case "white": return Brushes.White;
            case "transparent": return Brushes.Transparent; // Or White if you prefer no transparency

            // Add other colors your language supports if any
            // case "cyan": return Brushes.Cyan;
            // case "magenta": return Brushes.Magenta;
            // case "gray": return Brushes.Gray; // etc.

            default:
                // Fallback for unknown colors - could be white, black, or a distinct error color
                return Brushes.Pink; // Or Brushes.White / Brushes.Black
        }
    }


    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        // Clean up subscriptions when the control is removed
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            _viewModel.CanvasNeedsRedraw -= OnCanvasNeedsRedraw; // <-- UNSUBSCRIBE
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