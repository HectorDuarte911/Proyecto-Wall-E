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
    private string _compilerOutput = "Listo.";

    public MainWindowViewModel()
    {
        _canvasViewModel = new DrawCanvasViewModel();
        _editorViewModel = new TextEditorViewModel();
        // Ensure the ViewModels CanvasDimension matches the backend initially
        _canvasViewModel.CanvasDimension = _canvasViewModel.GetBackendCanvasDimension();
    }
    [RelayCommand]
    private void RunScript()
    {
        string codeToExecute = EditorViewModel.Document.Text;
        CompilerOutput = "Compilando y ejecutando...";
        List<Error> errors = new List<Error>();
        Canva.InitCanvas(); // Reinitialize to default size/state
        Canva.RedimensionCanvas(CanvasViewModel.CanvasDimension); // Apply current UI dimension
        Walle.Spawn(CanvasViewModel.CanvasDimension / 2, CanvasViewModel.CanvasDimension / 2); // Reset Walle position
        Walle.Color("Transparent"); // Reset color
        Walle.Size(1);       // Reset size
        try
        {
            Lexical lexer = new Lexical(codeToExecute);
            List<Token> tokens = lexer.TokensSearch();
            errors.AddRange(lexer.errors);
            if (errors.Count > 0)
            {
                ShowErrors(errors);
                CompilerOutput += "\nEjecución cancelada debido a errores léxicos.";
                return;
            }

            // Check if tokens were generated
            if (tokens.Count == 0 && !string.IsNullOrWhiteSpace(codeToExecute))
            {
                 // If there's code but no tokens, likely only whitespace/comments
                 CompilerOutput = "No se encontraron comandos ejecutables.";
                 // Optionally trigger a redraw to show the cleared canvas
                 CanvasViewModel.SignalCanvasUpdate();
                 return;
            }
            else if (tokens.Count == 0)
            {
                 CompilerOutput = "Editor vacío. No hay nada que ejecutar.";
                 // Optionally trigger a redraw to show the cleared canvas
                 CanvasViewModel.SignalCanvasUpdate();
                 return;
            }
            Parser parser = new Parser(tokens, errors);
            List<Stmt> statements = parser.Parse();
            errors.AddRange(parser.errors);
            if (errors.Count > 0)
            {
                ShowErrors(errors);
                 CompilerOutput += "\nEjecución cancelada debido a errores de sintaxis.";
                return;
            }

            Interpreter interpreter = new Interpreter(errors);
            interpreter.interpret(statements);
            errors.AddRange(interpreter.errors);
            if (errors.Count > 0)
            {
                ShowErrors(errors);
                CanvasViewModel.SignalCanvasUpdate(); // <-- UPDATE UI EVEN WITH RUNTIME ERRORS
            }
            else
            {
                CompilerOutput = "Ejecución completada sin errores.";
                CanvasViewModel.SignalCanvasUpdate(); // <-- UPDATE UI ON SUCCESS
            }
        }
        catch (Exception ex)
        {
            CompilerOutput = $"Error inesperado durante la ejecución:\n{ex.Message}\n{ex.StackTrace}";
            // Attempt to update the canvas view even if an unexpected error occurred
             CanvasViewModel.SignalCanvasUpdate();
        }
    }
    private void ShowErrors(List<Error> errors)
    {
        StringBuilder errorBuilder = new StringBuilder();
        errorBuilder.AppendLine("Errores encontrados:");
        foreach (var error in errors)
        {
            errorBuilder.AppendLine($"- Line {error.Location}: {error.Argument}");
        }
        CompilerOutput = errorBuilder.ToString();
    }
}