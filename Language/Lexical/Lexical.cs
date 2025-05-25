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
    switch (c)
    {
      case '(': AddToken(TokenTypes.LEFT_PAREN); break;
      case ')': AddToken(TokenTypes.RIGHT_PAREN); break;
      case '[': AddToken(TokenTypes.LEFT_BRACE); break;
      case ']': AddToken(TokenTypes.RIGHT_BRACE); break;
      case '/': AddToken(TokenTypes.DIVIDE); break;
      case '|':
        if (Match('|')) AddToken(TokenTypes.OR);
        else errors.Add(new Error(line, "Unexpected character '|' , maybe you want to use ||")); break;
      case '&': AddToken(Match('&') ? TokenTypes.AND : TokenTypes.AND); break;
      case '%': AddToken(TokenTypes.MODUL); break;
      case ',': AddToken(TokenTypes.COMMA); break;
      case '+': AddToken(TokenTypes.PLUS); break;
      case '-': AddToken(TokenTypes.MINUS); break;
      case '_': errors.Add(new Error(line, "Unexpected character '_' can't initialisade a label or variable name")); break;
      case '*': AddToken(Match('*') ? TokenTypes.POW : TokenTypes.PRODUCT); break;
      case '!': AddToken(Match('=') ? TokenTypes.BANG_EQUAL : TokenTypes.BANG); break;
      case '>': AddToken(Match('=') ? TokenTypes.GREATER_EQUAL : TokenTypes.GREATER); break;
      case '<': AddToken(Match('=') ? TokenTypes.LESS_EQUAL : Match('-') ? TokenTypes.ASSIGNED : TokenTypes.LESS); break;
      case '=': if (Match('=')) AddToken(TokenTypes.EQUAL_EQUAL); else errors.Add(new Error(line, "Unexpected character '=', maybe you mean '=='? Assignment is '<-'")); break; // Modified error message
      case ' ': case '\r': case '\t': break;
      case '\n': line++; break;
      case '"': StringRead(); break;
      default:
        if (ISDigit(c)) NumberRead();
        else if (IsAlpha(c)) IdentifierOrKeyword();
        else errors.Add(new Error(line, "Unexpected character"));
        break;
    }
  }
  /// <summary>
  /// Determinate is is a keyword or a identifer token
  /// </summary>
  private void IdentifierOrKeyword()
  {
    while (IsAlphaNumeric(LookAfter())) Advance();
    string text = source.Substring(start, current - start);
    TokenTypes type;
    object? literal = null;
    switch (text)
    {
      case "GoTo": type = TokenTypes.GOTO; break;
      case "Spawn": type = TokenTypes.SPAWN; break;
      case "GetActualX": type = TokenTypes.GETACTUALX; break;
      case "GetActualY": type = TokenTypes.GETACTUALY; break;
      case "Size": type = TokenTypes.SIZE; break;
      case "Color": type = TokenTypes.COLOR; break;
      case "DrawLine": type = TokenTypes.DRAWLINE; break;
      case "DrawCircle": type = TokenTypes.DRAWCIRCLE; break;
      case "DrawRectangle": type = TokenTypes.DRAWRECTANGLE; break;
      case "Fill": type = TokenTypes.FILL; break;
      case "GetCanvasSize": type = TokenTypes.GETCANVASSIZE; break;
      case "IsBrushColor": type = TokenTypes.ISBRUSHCOLOR; break;
      case "IsBrushSize": type = TokenTypes.ISBRUSHSIZE; break;
      case "GetColorCount": type = TokenTypes.GETCOLORCOUNT; break;
      case "IsCanvasColor": type = TokenTypes.ISCANVASCOLOR; break;
      case "true": type = TokenTypes.TRUE; literal = true; break;
      case "false": type = TokenTypes.FALSE; literal = false; break;
      default: type = TokenTypes.IDENTIFIER; literal = text; break;
    }
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
  private void AddToken(TokenTypes type) => AddTokenHelper(type, null!);
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