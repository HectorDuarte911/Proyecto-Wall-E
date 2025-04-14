public class Keywords
{
public static readonly Dictionary<string , TokenTypes> keywords ;
static Keywords(){keywords = new Dictionary<string ,TokenTypes>{{"GoTo", TokenTypes.GOTO},};}
}
public class Lexical
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
  public  List<Error> errors {get;private set;}
  public static List<Label> labels = new List<Label>();
  /// <summary>
    /// Constructor of Lexical
    /// </summary>
    /// <param name="source"></param>
  public Lexical (string source)
  {
    this.source = source;
    errors = new List<Error>();
  }
  /// <summary>
  /// Inspect the posible tokens in yhe line  
  /// </summary>
  /// <returns>Colection of all the tokens</returns>
  public List<Token> TokensSearch()
    {
      while(!EOF())
      {
        start= current;
        InspectTokens();
      }
      return tokens;
   }
  private void InspectTokens()
   {
    char c = Advance();
    switch(c)
    {
        case '(' : AddToken(TokenTypes.LEFT_PAREN);break;
        case ')' : AddToken(TokenTypes.RIGHT_PAREN);break;
        case '[' : AddToken(TokenTypes.LEFT_BRACE);break;
        case ']' : AddToken(TokenTypes.RIGHT_BRACE);break;
        case '/' : AddToken(TokenTypes.DIVIDE);break;
        case '|' : current++;if(Match('|'))AddToken(TokenTypes.OR);
        else errors.Add(new Error(line , "Unexpected character '|' ,maybe you want to use ||"));break;
        case '&' : AddToken(Match('&') ? TokenTypes.AND : TokenTypes.AND);break;
        case '%' : AddToken(TokenTypes.MODUL);break;
        case ',' : AddToken(TokenTypes.COMMA);break;
        case '+' : AddToken(TokenTypes.PLUS);break;
        case '-' : AddToken(TokenTypes.MINUS);break;
        case '*' : AddToken(Match('*') ? TokenTypes.POW : TokenTypes.PRODUCT);break;
        case '!' : AddToken(Match('=') ? TokenTypes.BANG_EQUAL : TokenTypes.BANG);break;
        case '>' : AddToken(Match('=') ? TokenTypes.GREATER_EQUAL : TokenTypes.GREATER);break;
        case '<' : AddToken(Match('=') ? TokenTypes.LESS_EQUAL : Match('-') ? TokenTypes.ASSIGNED : TokenTypes.LESS);break;
        case '=' : if(Match('='))AddToken(TokenTypes.EQUAL_EQUAL);
        else  errors.Add(new Error(line , "Unexpected character , maybe you mean '==' , '<=' or '>='"));break;
        case ' ' :
        case '\r':
        case '\t':break;
        case '\n':line++ ;break;
        case '"':StringRead();break;
        default:
        if(ISDigit(c))NumberRead();
        else if(c == '_' && current == 1)errors.Add(new Error (line , "The character '_' can't initialisade an expresion or label"));
        else if(IsAlpha(c))identifier();
        else errors.Add(new Error(line , "Unexpected character"));
        break;
    } 
  }
  private bool IsAlpha(char c)
  {
    return (c >= 'a' && c <= 'z')|| (c >= 'A' && c <= 'Z') || c == '_';
  }
  private bool IsAlphaNumeric(char c)
  {
    return IsAlpha(c) || ISDigit(c);
  }
  private void StringRead()
  {
    while(LookAfter() != '"' && !EOF())
    {
        if(LookAfter() == '\n')line++;
        Advance();
    }
    if(EOF())errors.Add(new Error(line, "Unfinish string detected"));
    Advance();
    string value = source.Substring(start + 1, current - 2- start);
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
    AddTokenHelper(TokenTypes.NUMBER,source.Substring(start,current-start));
  }
  private void identifier()
  {
    int actual = current;
    bool label = false;
    while(IsAlpha(LookAfter()) && LookAfter() != '_'){
      if(ISDigit(LookAfter())){
        label = true;
        current = actual;
        labelview();break;
      }
      Advance();
    }
    if(!label){
      while(true){
        if(LookAfter() == '<' && Match('-')){label = true ;break;}
        else if(LookAfter() != ' ')break;
        if(EOF())
        {
          current = actual;
          labelview();break;
        }
        Advance();
      }
      if(label){
      string text = source.Substring(start , current - start);
      TokenTypes type = TokenTypes.IDENTIFIER;
      if(Keywords.keywords.ContainsKey(text))type = Keywords.keywords[text];
      AddToken(type);
      }
    }
  }
  private void labelview()
  {
    while(IsAlphaNumeric(LookAfter()))Advance();
    Console.WriteLine(source.Substring(start , current));
    string text = source.Substring(start , current);
    AddToken(TokenTypes.LABEL);
    labels.Add(new Label (new Token (TokenTypes.LABEL ,text,null!,line)));
  }
  /// <summary>
  /// Call the auxiliar to add a neutral token whith no literal
  /// </summary>
  /// <param name="type"></param>
  private void AddToken(TokenTypes type)
  {
    AddTokenHelper(type, null!);
  }
  /// <summary>
  /// Add the token to the list of tokens in the line
  /// </summary>
  /// <param name="type">Type of the token added</param>
  /// <param name="literal">literal of the token added</param>
  private void AddTokenHelper(TokenTypes type , object literal)
  {
    string text = source.Substring(start , current - start);
    tokens.Add(new Token(type,text,literal,line));
  }
  /// <summary>
  /// See if the next character match the actual character to form a unique simbol
  /// </summary>
  /// <param name="value"></param>
  /// <returns></returns>
  private bool Match(char value)
  {
    if(EOF())return false;
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
    if(EOF())return '\0';
    return source[current];
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
  private bool EOF()
  {
    return current >= source.Length ;
  }
}