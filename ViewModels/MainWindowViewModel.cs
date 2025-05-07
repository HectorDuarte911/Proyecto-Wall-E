using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text;
using WALLE;
namespace PixelWallE.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private DrawCanvasViewModel _canvasViewModel;
    [ObservableProperty]
    private TextEditorViewModel _editorViewModel;
    [ObservableProperty]
    private string _compilerOutput = "Ready.";
    public MainWindowViewModel()
    {
        _canvasViewModel = new DrawCanvasViewModel();
        _editorViewModel = new TextEditorViewModel();
        _canvasViewModel.CanvasDimension = _canvasViewModel.GetBackendCanvasDimension();
    }
    [RelayCommand]
    private void RunScript()
    {
        string codeToExecute = EditorViewModel.Document.Text;
        CompilerOutput = "Compiling and executing...";
        List<Error> allErrors = new List<Error>();
        Canva.InitCanvas();
        Canva.RedimensionCanvas(CanvasViewModel.CanvasDimension);
        Walle.Spawn(CanvasViewModel.CanvasDimension / 2, CanvasViewModel.CanvasDimension / 2);
        Walle.Color("Transparent");
        Walle.Size(1);
        Interpreter? interpreter = null;
        try
        {
            Lexical lexer = new Lexical(codeToExecute);
            List<Token> tokens = lexer.TokensSearch();
            allErrors.AddRange(lexer.errors);
            if (allErrors.Count > 0)
            {
                CompilerOutput = "Execution cancelled due to lexical errors.";
                ShowErrors(allErrors);
                return;
            }
            if (tokens.Count == 0)
            {
                CompilerOutput = string.IsNullOrWhiteSpace(codeToExecute) ?"Editor is empty. Nothing to execute." :"No executable commands found.";
                CanvasViewModel.SignalCanvasUpdate();
                return;
            }
            Parser parser = new Parser(tokens, allErrors);
            List<Stmt> statements = parser.Parse();
            if (allErrors.Count > 0)
            {
                CompilerOutput = "Execution cancelled due to syntax errors.";
                ShowErrors(allErrors);
                return;
            }
            interpreter = new Interpreter(allErrors);
            interpreter.interpret(statements);
            if (allErrors.Count > 0)
            {
                CompilerOutput = "Execution completed with errors.";
                ShowErrors(allErrors);
            }
            else CompilerOutput = "Execution completed successfully.";
        }
        catch (RuntimeError rtError)
        {
            string specificErrorMessage = rtError.Message;
            int errorLine = rtError.token?.line ?? -1;
            CompilerOutput = "Runtime Error" + '\n' + $"(Line {errorLine}): {specificErrorMessage}";
            List<Error> otherErrors = allErrors
                .Where(e => !(e.Location == errorLine && e.Argument == specificErrorMessage))
                .ToList();
            if (otherErrors.Count > 0) ShowErrors(otherErrors);
        }
        catch (Exception ex)
        {
            CompilerOutput = $"Unexpected Error: {ex.Message}\n{ex.StackTrace}";
            if (allErrors.Count > 0)
            {
                StringBuilder sb = new StringBuilder(CompilerOutput);
                sb.AppendLine("\n Previous Errors (Lexical/Syntax) ");
                HashSet<string> reportedMessages = new HashSet<string>();
                foreach (var err in allErrors)
                {
                    string errorMsg = $"- Line {err.Location}: {err.Argument}";
                    if (reportedMessages.Add($"L{err.Location}:{err.Argument}")) sb.AppendLine(errorMsg);
                }
                CompilerOutput = sb.ToString();
            }
        }
        finally
        {
            CanvasViewModel.SignalCanvasUpdate();
        }
    }
    private void ShowErrors(List<Error> errorsToShow)
    {
        if (errorsToShow == null || errorsToShow.Count == 0) return;
        StringBuilder errorDetailsBuilder = new StringBuilder();
        errorDetailsBuilder.AppendLine("\n Error Details ");
        HashSet<string> reportedMessages = new HashSet<string>();
        bool detailsAdded = false;
        foreach (var error in errorsToShow)
        {
            string uniqueErrorKey = $"L{error.Location}:{error.Argument}";
            string errorMsg = $"- Line {error.Location}: {error.Argument}";
            if (reportedMessages.Add(uniqueErrorKey))
            {
                errorDetailsBuilder.AppendLine(errorMsg);
                detailsAdded = true;
            }
        }
        if (detailsAdded) CompilerOutput += errorDetailsBuilder.ToString();
    }
}