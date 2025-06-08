namespace WALLE;
public class Lexical
{
  /// <summary>
  /// Text in scanning
  /// </summary>
  private string source;
  /// <summary>
  /// Start position of the scann
  /// </summary>
  private int start = 0;
  /// <summary>
  /// Current postion of the scann
  /// </summary>
  private int current = 0;
  /// <summary>
  /// Actual line of the scann
  /// </summary>
  private int line = 1;
  /// <summary>
  /// Tokens whith their matches
  /// </summary>
  public static Dictionary<char, (TokenTypes, List<(char, TokenTypes)>)> MatchTokens = new Dictionary<char, (TokenTypes, List<(char, TokenTypes)>)>()
  {
    {'>',(TokenTypes.GREATER,new List<(char, TokenTypes)>(){('=',TokenTypes.GREATER_EQUAL)})},
    {'<',(TokenTypes.LESS,new List<(char, TokenTypes)>(){('=',TokenTypes.LESS_EQUAL),('-',TokenTypes.ASSIGNED)})},
    {'!',(TokenTypes.BANG,new List<(char, TokenTypes)>(){('=',TokenTypes.BANG_EQUAL) })},{'&',(TokenTypes.AND,new List<(char, TokenTypes)>(){('&',TokenTypes.AND)})},
    {'|',(TokenTypes.TRASH,new List<(char, TokenTypes)>(){('|',TokenTypes.OR) })},{'=',(TokenTypes.TRASH,new List<(char, TokenTypes)>(){('=',TokenTypes.EQUAL_EQUAL) })},
    {'*',(TokenTypes.PRODUCT,new List<(char, TokenTypes)>(){('*',TokenTypes.POW) })},{'(',(TokenTypes.LEFT_PAREN,new List<(char, TokenTypes)>(){})},
    {')',(TokenTypes.RIGHT_PAREN,new List<(char, TokenTypes)>(){})},{'[',(TokenTypes.LEFT_BRACE,new List<(char, TokenTypes)>(){})},
    {']',(TokenTypes.RIGHT_BRACE,new List<(char, TokenTypes)>(){})},{'/',(TokenTypes.DIVIDE,new List<(char, TokenTypes)>(){})},
    {',',(TokenTypes.COMMA,new List<(char, TokenTypes)>(){})},{'%',(TokenTypes.MODUL,new List<(char, TokenTypes)>(){})},
    {'+',(TokenTypes.PLUS,new List<(char, TokenTypes)>(){})},{'-',(TokenTypes.MINUS,new List<(char, TokenTypes)>(){})},
  };
  /// <summary>
  /// Colection of scanning tokens in the source
  /// </summary>
  private List<Token> tokens = new List<Token>();
  /// <summary>
  /// Colection of errors in the scanning
  /// </summary>
  public List<Error> errors { get; private set; }
  /// <summary>
  /// Constructor of Lexical
  /// </summary>
  public Lexical(string source)
  {
    this.source = source;
    errors = new List<Error>();
  }
  /// <summary>
  /// Scann the tokens of the source  
  /// </summary>
  public List<Token> TokensSearch()
  {
    while (!EOF())
    {
      start = current;
      InspectTokens();
    }
    return tokens;
  }
  /// <summary>
  /// Determinate the typr of token in the actual position of the source
  /// </summary>
  private void InspectTokens()
  {
    char c = Advance();
    if (MatchTokens.ContainsKey(c))
    {
      (TokenTypes, List<(char, TokenTypes)>) matches = MatchTokens[c];
      int count = matches.Item2.Count;
      if (count == 0) AddToken(MatchTokens[c].Item1);
      else
      {
        foreach ((char matchtype, TokenTypes returnMatch) pair in matches.Item2)
        {
          count--;
          MatchScan(matches.Item1, pair.matchtype, pair.returnMatch, count);
        }
      }
    }
    else if (c == '"') StringRead();
    else if (c == '\n') line++;
    else if (c == ' ' || c == '\r' || c == '\t') { }
    else if (c == '_') errors.Add(new Error(line, "Unexpected character '_' can't initialisade an expresion"));
    else if (ISDigit(c)) NumberRead();
    else if (IsAlpha(c)) IdentifierOrKeyword();
    else errors.Add(new Error(line, "Unexpected character"));
  }
  /// <summary>
  /// Add a Match Token
  /// </summary>
  private void MatchScan(TokenTypes defauldMatch, char matchtype, TokenTypes ReturnMatch, int count)
  {
    TokenTypes token = TokenTypes.TRASH;
    if (Match(matchtype)) token = ReturnMatch;
    else if (count == 0)
    {
      if (defauldMatch == TokenTypes.TRASH) errors.Add(new Error(line, $"Unexpected character '{source[current - 1]}' maybe you mean '{source[current - 1]}{source[current -1 ]}'"));
      else token = defauldMatch;
    }
    if (token != TokenTypes.TRASH) AddToken(token);
  } 
  /// <summary>
  /// Existed keywords
  /// </summary>
  public static Dictionary<string, TokenTypes> Keywords = new Dictionary<string, TokenTypes>()
  {
   {"GoTo",TokenTypes.GOTO},{"Spawn",TokenTypes.SPAWN},{"GetActualX",TokenTypes.GETACTUALX},{"GetActualY",TokenTypes.GETACTUALY},
   {"Size",TokenTypes.SIZE},{"Color",TokenTypes.COLOR},{"DrawLine",TokenTypes.DRAWLINE},{"DrawCircle",TokenTypes.DRAWCIRCLE},
   {"DrawRectangle",TokenTypes.DRAWRECTANGLE},{"Fill",TokenTypes.FILL},{"GetCanvasSize",TokenTypes.GETCANVASSIZE},
   {"IsBrushColor",TokenTypes.ISBRUSHCOLOR},{"IsBrushSize",TokenTypes.ISBRUSHSIZE},{ "GetColorCount",TokenTypes.GETCOLORCOUNT},
   {"IsCanvasColo",TokenTypes.ISCANVASCOLOR},{ "true",TokenTypes.TRUE},{ "false",TokenTypes.FALSE},
  };
  private void IdentifierOrKeyword()
  {
    while (IsAlphaNumeric(LookAfter())) Advance();
    string text = source.Substring(start, current - start);
    TokenTypes type;
    object? literal = null;
        if (Keywords.ContainsKey(text))
        {
            type = Keywords[text];
            if (text == "true") literal = true;
            if (text == "false") literal = false;
        }
        else { type = TokenTypes.IDENTIFIER; literal = text; }
    AddTokenHelper(type, literal!);
  }
  /// <summary>
  /// Determinate if is a character type of source
  /// </summary>
  private bool IsAlpha(char c) => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';
  /// <summary>
  /// Determinate if is a number or a character type of source
  /// </summary>
  private bool IsAlphaNumeric(char c) => IsAlpha(c) || ISDigit(c);
  /// <summary>
  /// Determinate if is a valible string type of token
  /// </summary>
  private void StringRead()
  {
    bool flag = false;
    while (LookAfter() != '"')
    {
      if (EOF())
      {
        errors.Add(new Error(line, "Unfinish string detected"));
        flag = true; break;
      }
      Advance();
    }
    if (!flag) Advance();
    string value = source.Substring(start + 1, current - 2 - start);
    AddTokenHelper(TokenTypes.STRING, value);
  }
  /// <summary>
  /// Determinate if is a number source
  /// </summary>
  private bool ISDigit(char c) => c >= '0' && c <= '9';
  /// <summary>
  /// Determinate if is a valid number declaration token
  /// </summary>
  private void NumberRead()
  {
    while (ISDigit(LookAfter())) Advance();
    if (!EOF() && IsAlpha(LookAfter()))
    {
      while (!EOF() && IsAlphaNumeric(LookAfter())) Advance();
      errors.Add(new Error(line, "Invalid number format: unexpected characters after number."));
    }
    else AddTokenHelper(TokenTypes.NUMBER, source.Substring(start, current - start));
  }
  /// <summary>
  /// Predeterminate token added
  /// </summary>
  private void AddToken(TokenTypes type)=>AddTokenHelper(type, null!);
  /// <summary>
  /// General token added to the list
  /// </summary>
  private void AddTokenHelper(TokenTypes type, object literal)
  {
    string text = source.Substring(start, current - start);
    tokens.Add(new Token(type, text, literal, line));
  }
  /// <summary>
  /// See if the next character match the actual character to form a unique simbol
  /// </summary>
  private bool Match(char value)
  {
    if (EOF()) return false;
    if (source?[current] != value) return false;
    current++;
    return true;
  }
  /// <summary>
  /// Look the current position
  /// </summary>
  private char LookAfter()
  {
    if (EOF()) return '\0';
    return source[current];
  }
  /// <summary>
  /// Advance one character in the inspection
  /// </summary>
  private char Advance()
  {
    current++;
    return source[current - 1];
  }
  /// <summary>
  /// Determinate if is the end of a source
  /// </summary>
  private bool EOF() => current >= source.Length;
}