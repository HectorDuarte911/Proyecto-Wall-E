using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using Avalonia.Media;
namespace PixelWallE.EditorHelpers;
    public class MyCompletionData : ICompletionData
    {
        public MyCompletionData(string text) => Text = text;
         public IImage? Image => null;
        public string Text { get; }
        public object Content => Text;
        public object Description => $"Insert '{Text}'";
        public double Priority => 0;
        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs e) => textArea.Document.Replace(completionSegment, Text);
    }
