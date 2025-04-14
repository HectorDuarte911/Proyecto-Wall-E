using System.Text;
public class Language
{
  private static Interpreter?
   interpreter;
  /// <summary>
  /// Report if is an error
  /// </summary>
  public static void Main(string[] args)
  {
    if (args.Length > 1)
    {
      Environment.Exit(64);
    }
    else if (args.Length == 1)
    {
      RunFile(args[0]);
    }
    else RunPromt();
  }
  /// <summary>
  /// Read the text that was writing
  /// </summary>
  /// <param name="path"></param>
  private static void RunFile(string path)
  {
    byte[] bytes = File.ReadAllBytes(path);
    Run(Encoding.Default.GetString(bytes));
  }
  /// <summary>
  /// Put the cursor in the place to write the code
  /// </summary>
  private static void RunPromt()
  {
    string line;
    while (true)
    {
      Console.Write("> ");
      line = Console.ReadLine()!;
      if (line == null) break;
      Run(line);
    }
  }
  /// <summary>
  /// Comunicate the writing whith the sintaxix and the Lexical
  /// </summary>
  /// <param name="source"></param>
  private static void Run(string source)
  {
    Lexical scaner = new Lexical(source);
    List<Token> tokens = scaner.TokensSearch();
    foreach (Token item in tokens)
    {
      Console.WriteLine(item.type.ToString());
    }
    if(scaner.errors.Count > 0)
    {
     foreach (Error error in scaner.errors)
     {
      Console.WriteLine($"[{error.Location}] " + error.Argument);
      return;
     }
    }
    else
    {
    Parser parser = new Parser(tokens,scaner.errors);
    List<Stmt> statements = parser.parse();
    if(parser.errors.Count > 0)
    {
      foreach (Error error in parser.errors)
     {
      Console.WriteLine($"[{error.Location}] " + error.Argument);
      return;
     }
    }
    else
    {
    interpreter = new Interpreter(parser.errors);
    interpreter.interpret(statements,0);
    if(interpreter.errors.Count > 0)
    {
      foreach (Error error in interpreter.errors)
     {
      Console.WriteLine($"[{error.Location}] " + error.Argument);
      return;
     }
    }
    }
  }
  }
}