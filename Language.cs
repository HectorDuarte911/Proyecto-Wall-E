using System.Text;

public class Language
{
    /// <summary>
    /// Report if is an error
    /// </summary>
    private static  bool HadError = false;
    /// <summary>
    /// Comunicate the error the error
    /// </summary>
    /// <param name="line">Line off the error</param>
    /// <param name="message">type of error</param>
    public static void Error(int line, string message)
    {
        Console.WriteLine("[Line " + line + "] Error : " + message);
        HadError = true;
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
    private static void Run(string content)
    {
        Console.WriteLine("Ejecuting content of the file");
        Console.WriteLine("Content");
    }
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