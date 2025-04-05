public class Gramatic
{
    /// <summary>
    /// Actual line that is ispected
    /// </summary>
    private string source;
    /// <summary>
    /// Start position of the inspection
    /// </summary>
    private int start = 0;
    /// <summary>
    /// Current postion of the insoection 
    /// </summary>
    private int current = 0;
    /// <summary>
    /// Actual line of the inspection
    /// </summary>
    private int line =1;
    /// <summary>
    /// Colection of tokens in the source
    /// </summary>
    private List <Token> tokens = new List<Token>();
    /// <summary>
    /// Constructor of Gramatic
    /// </summary>
    /// <param name="source"></param>
    public Gramatic (string? source)
    {
        this.source = source;
    }
    /// <summary>
    /// Inspect the posible tokens in yhe line  
    /// </summary>
    /// <returns>Colection of all the tokens</returns>
    public List<Token> TokensSearch()
    {
        while(!IsTheEnd())
        {
            start= current;
            InspectTokens();
        }
        tokens.Add(new Token(TokenTypes.EOF," ",null,line));
        return tokens;
   }
   /// <summary>
   /// Inform if is the end of the line
   /// </summary>
   /// <returns></returns>
   private bool IsTheEnd()
   {
    return current >= source?.Length;
   }
   /// <summary>
   /// Advance one character in the inspection
   /// </summary>
   /// <returns>The current character before this method was called </returns>
   private char Advance()
   {
    current ++;
    return source[current - 1];
   }
   /// <summary>
   /// Inspect the character and see if is valid or not 
   /// </summary>
   private void InspectTokens()
   {
    char c = Advance();
    switch(c)
    {
        case ';' : AddToken(TokenTypes.SEMICOLON);break;
        case '(' : AddToken(TokenTypes.LEFT_PAREN);break;
        case ')' : AddToken(TokenTypes.RIGHT_PAREN);break;
        case '[' : AddToken(TokenTypes.LEFT_BRACE);break;
        case ']' : AddToken(TokenTypes.RIGHT_BRACE);break;
        case '/' : AddToken(TokenTypes.DIVIDE);break;
        case '|' : current++;if(Match('|'))AddToken(TokenTypes.OR);
        else Language.Error(line , "Unexpected character '|' ,maybe you want to use ||");
        break;
        case '&' : current++;if(Match('&'))AddToken(TokenTypes.OR);
        else Language.Error(line , "Unexpected character '&' ,maybe you want to use &&");
        break;
        case '%' : AddToken(TokenTypes.MODUL);break;
        case ',' : AddToken(TokenTypes.COMMA);break;
        case '+' : AddToken(TokenTypes.PLUS);break;
        case '-' : AddToken(TokenTypes.MINUS);break;
        case '*' : AddToken(Match('*') ? TokenTypes.POW : TokenTypes.PRODUCT);break;
        case '!' : AddToken(Match('=') ? TokenTypes.BANG_EQUAL : TokenTypes.BANG);break;
        case '>' : AddToken(Match('=') ? TokenTypes.GREATER_EQUAL : TokenTypes.GREATER);break;
        case '<' : AddToken(Match('=') ? TokenTypes.LESS_EQUAL : Match('-') ? TokenTypes.ASSIGNED : TokenTypes.LESS);break;
        case ' ' :
        case '\r':
        case '\t':break;
        case '\n':line++ ;break;
        case '"':StringRead();break;
        default:
        if(ISDigit(c))NumberRead();
        else Language.Error(line , "Unexpected character");
        break;
    } 
  }
  /// <summary>
  /// Call the auxiliar to add a neutral token whith no literal
  /// </summary>
  /// <param name="type"></param>
  private void AddToken(TokenTypes type)
  {
    AddTokenHelper(type, null);
  }
  /// <summary>
  /// Add the token to the list of tokens in the line
  /// </summary>
  /// <param name="type">Type of the token added</param>
  /// <param name="literal">literal of the token added</param>
  private void AddTokenHelper(TokenTypes type , object literal)
  {
    string text = source.Substring(start , current);
    tokens.Add(new Token(type,text,literal,line));
  }
  /// <summary>
  /// See if the next character match the actual character to form a unique simbol
  /// </summary>
  /// <param name="value"></param>
  /// <returns></returns>
  private bool Match(char value)
  {
    if(IsTheEnd())return false;
    if(source?[current] != value)return false;
    current++;
    return true;
  }
  /// <summary>
  /// Look the current position
  /// </summary>
  /// <returns>the character in the current position of the line</returns>
  private char LookAfter()
  {
    if(IsTheEnd())return '\0';
    return source[current];
  }
  /// <summary>
  /// Inspect the validation and when the string finish
  /// </summary>
  private void StringRead()
  {
    while(LookAfter() != '"' && !IsTheEnd())
    {
        if(LookAfter() == '\n')line++;
        Advance();
    }
    if(IsTheEnd())Language.Error(line, "Unfinish string detected");
    Advance();
    string value = source.Substring(start, current - 1);
    AddTokenHelper(TokenTypes.STRING,value);
  }
  /// <summary>
  /// See if the character if a number
  /// </summary>
  /// <param name="c">Posible number</param>
  /// <returns>True if is a number</returns>
  private bool ISDigit(char c)
  {
    return c >= '0' && c <= '9';
  }
  /// <summary>
  /// Inspect how large if the detected number
  /// </summary>
  private void NumberRead()
  {
    while(ISDigit(LookAfter()))Advance();
    AddTokenHelper(TokenTypes.NUMBER,source.Substring(start,current));
  }


}