using Avalonia.Controls;
using Avalonia.Input;
using AvaloniaEdit;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Editing;
using PixelWallE.EditorHelpers; 
using Avalonia; 
using AvaloniaEdit.Document; 
namespace PixelWallE.Views
{
    public partial class TextEditorView : UserControl
    {
        private TextEditor? _textEditor;
        private CompletionWindow? _completionWindow;
        private readonly List<MyCompletionData> _allCompletions = new List<MyCompletionData>
        {
            new MyCompletionData("Color"), new MyCompletionData("Size"),new MyCompletionData("Spawn"),
            new MyCompletionData("DrawLine"),new MyCompletionData("DrawCircle"),new MyCompletionData("DrawRectangle"),
            new MyCompletionData("GetCanvasSize"),new MyCompletionData("Fill"),new MyCompletionData("IsBrushColor"),
            new MyCompletionData("GetCellColor"),new MyCompletionData("IsBrushSize"),new MyCompletionData("GetColorCount"),
            new MyCompletionData("GetActualX"), new MyCompletionData("GetActualY"),new MyCompletionData("IsCanvasColor"),
            new MyCompletionData("Red"), new MyCompletionData("Blue"), new MyCompletionData("Green"),
            new MyCompletionData("Yellow"), new MyCompletionData("Orange"), new MyCompletionData("Purple"),
            new MyCompletionData("Black"), new MyCompletionData("White"), new MyCompletionData("Transparent"),
            new MyCompletionData("Goldenrod"), new MyCompletionData("DarkSlateGray"),new MyCompletionData("GoTo"),
            new MyCompletionData("LightSkyBlue"), new MyCompletionData("DimGray"),new MyCompletionData("true"),
            new MyCompletionData("SaddleBrown"), new MyCompletionData("false")
        
        };
        public TextEditorView()
        {
            InitializeComponent();
            _textEditor = this.FindControl<TextEditor>("CodeEditor"); 
            if (_textEditor != null)
            {
                _textEditor.TextArea.TextEntering += TextArea_TextEntering;
                _textEditor.TextArea.TextEntered += TextArea_TextEntered;
                _textEditor.KeyDown += TextEditor_KeyDown;
            }
        }
        private void TextEditor_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space && e.KeyModifiers == KeyModifiers.Control)
            {
                if (_textEditor != null)
                {
                    ShowCompletionWindow(_textEditor.TextArea);
                    e.Handled = true;
                }
            }
        }
        private void TextArea_TextEntered(object? sender, TextInputEventArgs e)
        {
            if (_textEditor == null || string.IsNullOrEmpty(e.Text)) return;
            char typedChar = e.Text[0];
            if (char.IsLetter(typedChar) || typedChar == '_') ShowCompletionWindow(_textEditor.TextArea);
        }
        private void TextArea_TextEntering(object? sender, TextInputEventArgs e)
        {
            if (e.Text != null && e.Text.Length > 0 && _completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]) && e.Text[0] != '_') _completionWindow.CompletionList.RequestInsertion(e);
            }
        }
        private void ShowCompletionWindow(TextArea textArea)
        {
            if (_completionWindow != null)  return;
            _completionWindow = new CompletionWindow(textArea);
            _completionWindow.Closed += (o, args) => _completionWindow = null;
            PopulateCompletionData(_completionWindow, textArea);
            if (_completionWindow.CompletionList.CompletionData.Any()) _completionWindow.Show();
            else _completionWindow.Close();
        }
        private void PopulateCompletionData(CompletionWindow completionWindow, TextArea textArea)
        {
            string wordBeforeCaret = GetWordBeforeCaret(textArea);
            var suggestions = _allCompletions
                .Where(item => item.Text.StartsWith(wordBeforeCaret, StringComparison.OrdinalIgnoreCase))
                .ToList();
            foreach (var suggestion in suggestions)completionWindow.CompletionList.CompletionData.Add(suggestion);
            if (!string.IsNullOrEmpty(wordBeforeCaret) && suggestions.Any())
            {
                completionWindow.StartOffset = textArea.Caret.Offset - wordBeforeCaret.Length;
                completionWindow.EndOffset = textArea.Caret.Offset;
            }
        }
        private string GetWordBeforeCaret(TextArea textArea)
        {
            int offset = textArea.Caret.Offset;
            if (offset == 0) return string.Empty;
            int wordStartOffset = TextUtilities.GetNextCaretPosition(textArea.Document, offset, LogicalDirection.Backward, CaretPositioningMode.WordStart);
            while (wordStartOffset < offset && !char.IsLetterOrDigit(textArea.Document.GetCharAt(wordStartOffset)))wordStartOffset++;
            if (wordStartOffset < offset) 
            {
                string text = textArea.Document.GetText(wordStartOffset, offset - wordStartOffset);
                return new string(text.TakeWhile(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
            }
            
            return string.Empty;
        }
        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            if (_textEditor != null)
            {
                _textEditor.TextArea.TextEntering -= TextArea_TextEntering;
                _textEditor.TextArea.TextEntered -= TextArea_TextEntered;
                _textEditor.KeyDown -= TextEditor_KeyDown;
            }
            if (_completionWindow != null)
            {
                _completionWindow.Close(); 
            }
            base.OnDetachedFromVisualTree(e);
        }
    }
}