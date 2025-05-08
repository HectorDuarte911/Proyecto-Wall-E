using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WALLE;
using System.IO;
using System.Text.Json;
using Avalonia.Platform.Storage;
using Avalonia.Controls;
using System.Threading.Tasks;
using PixelWallE.Models;

namespace PixelWallE.ViewModels;

public partial class DrawCanvasViewModel : ObservableObject
{
    public int MaxCanvasDimension => 300;
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ResizeCanvasCommand))]
    private int _canvasDimension;
    public event EventHandler? CanvasNeedsRedraw;
    public static Func<IStorageProvider?> GetStorageProvider { get; set; } = () => null;
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
        Walle.Spawn(newDim / 2, newDim / 2); Walle.Spawn(newDim / 2, newDim / 2);
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
    [RelayCommand]
    private async Task SaveCanvasAsync()
    {
        var storageProvider = GetStorageProvider?.Invoke();
        if (storageProvider == null) return;
        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Canvas Drawing",
            SuggestedFileName = "my_drawing",
            DefaultExtension = "json",
            FileTypeChoices = new[]{ new FilePickerFileType("JSON Canvas") { Patterns = new[] { "*.json" } }}
            
        });
        if (file != null)
        {
            try
            {
                var saveData = new CanvasSaveData{Dimension = Canva.GetCanvasSize()};
                saveData.CellColors = new string[saveData.Dimension][];
                for (int i = 0; i < saveData.Dimension; i++)
                {
                    saveData.CellColors[i] = new string[saveData.Dimension];
                    for (int j = 0; j < saveData.Dimension; j++)saveData.CellColors[i][j] = Canva.GetCellColor(j, i);
                }
                string jsonString = JsonSerializer.Serialize(saveData, new JsonSerializerOptions { WriteIndented = true });
                await using var stream = await file.OpenWriteAsync();
                await using var writer = new StreamWriter(stream);
                await writer.WriteAsync(jsonString);
                System.Diagnostics.Debug.WriteLine($"Canvas saved to: {file.Path.AbsolutePath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving canvas: {ex.Message}");
            }
        }
    }
    [RelayCommand]
    private async Task LoadCanvasAsync()
    {
        var storageProvider = GetStorageProvider?.Invoke();
        if (storageProvider == null) return;
        var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Load Canvas Drawing",
            AllowMultiple = false,
            FileTypeFilter = new[]{new FilePickerFileType("JSON Canvas") { Patterns = new[] { "*.json" } }}
        });
        if (files != null && files.Count > 0)
        {
            var file = files[0];
            try
            {
                await using var stream = await file.OpenReadAsync();
                using var reader = new StreamReader(stream);
                string jsonString = await reader.ReadToEndAsync();
                var loadedData = JsonSerializer.Deserialize<CanvasSaveData>(jsonString);
                if (loadedData != null && loadedData.CellColors != null)
                {
                    int newDim = Math.Clamp(loadedData.Dimension, 1, MaxCanvasDimension);
                    CanvasDimension = newDim;
                    Canva.RedimensionCanvas(newDim);

                    for (int i = 0; i < newDim; i++)
                    {
                        for (int j = 0; j < newDim; j++)
                        {
                            if (i < loadedData.CellColors.Length && j < loadedData.CellColors[i].Length)Canva.SetCellColor(j, i, loadedData.CellColors[i][j]);
                            else Canva.SetCellColor(j, i, "White");
                        }
                    }
                    Walle.Spawn(newDim / 2, newDim / 2);
                    Walle.Color("Transparent");
                    Walle.Size(1);
                    SignalCanvasUpdate(); 
                    System.Diagnostics.Debug.WriteLine($"Canvas loaded from: {file.Path.AbsolutePath}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading canvas: {ex.Message}");
            }
        }
    }
}