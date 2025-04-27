using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
namespace PixelWallE.ViewModels;
public partial class TextEditorViewModel : ObservableObject
{
    [ObservableProperty]
    private TextDocument _document;
    public TextEditorViewModel()
    {
        _document = new TextDocument();
    }
}