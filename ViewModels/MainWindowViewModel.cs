 using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PixelWallE.ViewModels; 
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
    }
    [RelayCommand]
    private void RunScript()
    {
        string codeToExecute = EditorViewModel.Document.Text;
        CompilerOutput = "Compilando...";
        List<Error> errors = new List<Error>();

        try
        {
            Lexical lexer = new Lexical(codeToExecute);
            List<Token> tokens = lexer.TokensSearch();
            errors.AddRange(lexer.errors); 
            if (errors.Count > 0)
            {
                ShowErrors(errors);
                return; 
            }
            Parser parser = new Parser(tokens, errors); 
            List<Stmt> statements = parser.parse();
            errors.AddRange(parser.errors);
            if (errors.Count > 0)
            {
                ShowErrors(errors);
                return; 
            }
            Interpreter interpreter = new Interpreter(errors);
            interpreter.interpret(statements, 0); 
            errors.AddRange(interpreter.errors); 
            if (errors.Count > 0)
            {
                ShowErrors(errors);
            }
            else
            {
                CompilerOutput = "Ejecución completada sin errores.";
            }
        }
        catch (Exception ex)
        {
            CompilerOutput = $"Error inesperado:\n{ex.Message}\n{ex.StackTrace}";
        }
    }
    private void ShowErrors(List<Error> errors)
    {
        StringBuilder errorBuilder = new StringBuilder();
        errorBuilder.AppendLine("Errores encontrados:");
        foreach (var error in errors)
        {
            errorBuilder.AppendLine($"- Línea {error.Location}: {error.Argument}");
        }
        CompilerOutput = errorBuilder.ToString();
    }
}