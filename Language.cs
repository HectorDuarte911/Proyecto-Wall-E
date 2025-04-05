using System.Text;

public class Language
{
    /// <summary>
    /// Report if is an error
    /// </summary>
    private static  bool HadError = false;
   /// <summary>
   /// Intermade a type of errror
   /// </summary>
   /// <param name="line"></param>
   /// <param name="message"></param>
    public static void Error(int line, string message)
    {
     Report(line,"",message);   
    }
    /// <summary>
    /// Show to the user the error
    /// </summary>
    /// <param name="line"></param>
    /// <param name="where"></param>
    /// <param name="message"></param>
    private static void Report(int line , string where , string message)
    {
     Console.WriteLine("[Line " + line + "] Error : " + message);
     HadError = true;
    }
  /// <summary>
  /// Interference in the message of the type of error of a token
  /// </summary>
  /// <param name="token"></param>
  /// <param name="message"></param>
    public static void error(Token token,string message)
   {
    if(token.type == TokenTypes.EOF)
    {
      Report(token.line,"at end",message);
    }
    else
    {
        Report(token.line," at '" + token.writing + "'",message);
    }
   }
    public static void Main(string [] args)
    {
        if(args.Length > 1)
        {
         Environment.Exit(64);
        }
        else if(args.Length == 1)
        {
            RunFile(args[0]);
        }
        else RunPromt();
    }
   /// <summary>
   /// Read the text that was writing
   /// </summary>
   /// <param name="path"></param>
    static void RunFile(string path)
    {
     try
     {
      string content = File.ReadAllText(path,Encoding.Default);
      Run(content);
     }
     catch(IOException e)
     {
        Console.WriteLine($"Error reading the file: {e.Message}");
     }
    }
    /// <summary>
    /// Comunicate the writing whith the sintaxix and the gramatic
    /// </summary>
    /// <param name="source"></param>
    private static void Run(string source)
    {
        Gramatic scaner = new Gramatic(source);
        List<Token> tokens = scaner.TokensSearch();
        Parser parser = new Parser(tokens);
        Expresion expresion = parser.parse();
        if(HadError)return;
        Console.WriteLine(expresion);
    }
  /// <summary>
  /// Put the cursor in the place to write the code
  /// </summary>
    static void RunPromt()
    {
      string line;
      while(true)
      {
        Console.Write("> ");
        line = Console.ReadLine()!;
        if(line == null)break;
        Run(line);
      }
    }
}