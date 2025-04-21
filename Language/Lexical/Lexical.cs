namespace WALLE;
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
        case '|' : if(Match('|'))AddToken(TokenTypes.OR);
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
        case '=' : if(Match('='))AddToken(TokenTypes.EQUAL_EQUAL);else  errors.Add(new Error(line , "Unexpected character , maybe you mean '==' , '<=' or '>='"));break;
        case ' ' :
        case '\r':
        case '\t':break;
        case '\n':line++ ;break;
        case '"':StringRead();break;
        default:
        if(ISDigit(c))NumberRead();
        else if(IsAlpha(c))
        {
          if(!IsKeyword())
          {
            current = start;
          if(!IsDeclaration())
          {
            current  = start;
            labelview();
          } 
         } 
        }
        else errors.Add(new Error(line , "Unexpected character"));
        break;
    }
  }
  private bool IsAlpha(char c)
  {
    return (c >= 'a' && c <= 'z')|| (c >= 'A' && c <= 'Z') || c == '-';
  }
  private bool IsAlphaNumeric(char c)
  {
    return IsAlpha(c) || ISDigit(c);
  }
  private void StringRead()
  {
    bool flag = false;
    while(LookAfter() != '"')
    {
      if(EOF())
      {
         errors.Add(new Error(line, "Unfinish string detected"));
         flag = true;
         break;
      }
      Advance();
    }
    if(!flag)Advance();
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
    if(current <= source.Length)
    {
      if((IsAlpha(LookAfter()) || LookAfter() == '"' ) && LookAfter() != '-')
      {
        while(IsAlpha(LookAfter()))Advance();
        errors.Add(new Error(line,"Unexpected character"));
      }
    }
    AddTokenHelper(TokenTypes.NUMBER,source.Substring(start,current-start));
  }
  private bool IsDeclaration()
  {
    while(IsAlphaNumeric(LookAfter()))Advance();
    string text = source.Substring(start , current - start );
      if(IsLineUP())return false;
    if(LookAfter() != '<' && LookAfter() != ' ')return false;
    tokens.Add(new Token(TokenTypes.IDENTIFIER,text,text,line));
    return true;
  }
  private void labelview()
  {
    bool flag = true;
    while(IsAlphaNumeric(LookAfter()))Advance();
    string text = source.Substring(start , current - start);
    if( tokens.Count != 0 && tokens[tokens.Count - 1].type == TokenTypes.ASSIGNED)
    {
      for (int i = 0; i < text.Length; i++)
      {
        if(!IsAlpha(text[i]) || text[i] == '-' )
        {
           flag =false;
           break;
        }
      }
      if(flag)AddToken(TokenTypes.IDENTIFIER);
      else
      {
        errors.Add(new Error(line,"Label can't have a value to assing"));
      }
    }
    else
    {
    AddToken(TokenTypes.LABEL);
    labels.Add(new Label (new Token (TokenTypes.LABEL ,text,null!,line)));
    }
   
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
  private bool IsLineUP()
  {
    while(!EOF())
    {
      if(LookAfter() != '\n' && LookAfter() != ' ')return false;
      Advance();
    }
    return true;
  }
  private bool IsKeyword()
  {
    while(IsAlpha(LookAfter()) && LookAfter() != '-')Advance();
    string text = source.Substring(start , current - start);
    switch (text)
    {
      case "true":AddTokenHelper(TokenTypes.TRUE,true);return true;
      case "false":AddTokenHelper(TokenTypes.FALSE,false);return true;
      case "GoTo":AddToken(TokenTypes.GOTO);return true;
      case "Spawn":AddToken(TokenTypes.SPAWN);return true;
      case "GetActualX":AddToken(TokenTypes.GETACTUALX);return true;
      case "GetActualY":AddToken(TokenTypes.GETACTUALY);return true;
      case "Size":AddToken(TokenTypes.SIZE);return true;
      case "Color":AddToken(TokenTypes.COLOR);return true;
      case "DrawLine":AddToken(TokenTypes.DRAWLINE);return true;
      case "DrawCircle":AddToken(TokenTypes.DRAWCIRCLE);return true;
      case "DrawRectangle":AddToken(TokenTypes.DRAWRECTANGLE);return true;
      case "Fill":AddToken(TokenTypes.FILL);return true;
      case "GetCanvasSize":AddToken(TokenTypes.GETCANVASSIZE);return true;
      case "IsBrushColor":AddToken(TokenTypes.ISBRUSHCOLOR);return true;
      case "IsBrushSize":AddToken(TokenTypes.ISBRUSHSIZE);return true;
      case "GetColorCount":AddToken(TokenTypes.GETCOLORCOUNT);return true;
      case "IsCanvasColor":AddToken(TokenTypes.ISCANVASCOLOR);return true;
      default:return false;
    }
  }
}