using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using Avalonia.Media;
namespace PixelWallE.EditorHelpers;
    public class MyCompletionData : ICompletionData
    {
        public MyCompletionData(string text) => Text = text;
        /// <summary>
        /// Save the image of the text to completed
        /// </summary>
        public IImage? Image => null;
        /// <summary>
        /// Save the text to completed
        /// </summary>
        public string Text { get; }
        /// <summary>
        /// Save the conted (text) of the text to completed
        /// </summary>
        public object Content => Text;
        /// <summary>
        /// Put a description to the completetion 
        /// </summary>
        public object Description => $"Insert '{Text}'";
        public double Priority => 0;
        /// <summary>
        /// Do the action of complete
        /// </summary>
        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs e) => textArea.Document.Replace(completionSegment, Text);
    }
