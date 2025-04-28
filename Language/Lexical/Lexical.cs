namespace WALLE;

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
  private int line = 1;
  /// <summary>
  /// Colection of tokens in the source
  /// </summary>
  private List<Token> tokens = new List<Token>();
  public List<Error> errors { get; private set; }
  public List<Label> labels = new List<Label>();
  /// <summary>
  /// Constructor of Lexical
  /// </summary>
  /// <param name="source"></param>
  public Lexical(string source)
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
    while (!EOF())
    {
      start = current;
      InspectTokens();
    }
    return tokens;
  }
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
      case '*': AddToken(Match('*') ? TokenTypes.POW : TokenTypes.PRODUCT); break;
      case '!': AddToken(Match('=') ? TokenTypes.BANG_EQUAL : TokenTypes.BANG); break;
      case '>': AddToken(Match('=') ? TokenTypes.GREATER_EQUAL : TokenTypes.GREATER); break;
      case '<': AddToken(Match('=') ? TokenTypes.LESS_EQUAL : Match('-') ? TokenTypes.ASSIGNED : TokenTypes.LESS); break;
      case '=': if (Match('=')) AddToken(TokenTypes.EQUAL_EQUAL); else errors.Add(new Error(line, "Unexpected character '=', maybe you mean '=='? Assignment is '<-'")); break; // Modified error message
      case ' ':
      case '\r':
      case '\t': break;
      case '\n': line++; break;
      case '"': StringRead(); break;
      default:
        if (ISDigit(c)) NumberRead();
        else if (IsAlpha(c)) IdentifierOrKeyword();
        else errors.Add(new Error(line, "Unexpected character"));
        break;
    }
  }
  private bool IsAlpha(char c)
  {
    return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '-';
  }
  private bool IsAlphaNumeric(char c)
  {
    return IsAlpha(c) || ISDigit(c);
  }
  private void StringRead()
  {
    bool flag = false;
    while (LookAfter() != '"')
    {
      if (EOF())
      {
        errors.Add(new Error(line, "Unfinish string detected"));
        flag = true;
        break;
      }
      Advance();
    }
    if (!flag) Advance();
    string value = source.Substring(start + 1, current - 2 - start);
    AddTokenHelper(TokenTypes.STRING, value);
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
    while (ISDigit(LookAfter())) Advance();
    if (!EOF() && IsAlpha(LookAfter()))
    {
    while (!EOF() && IsAlphaNumeric(LookAfter())) Advance();
    errors.Add(new Error(line, "Invalid number format: unexpected characters after number."));
    }
    else
    {
    AddTokenHelper(TokenTypes.NUMBER, source.Substring(start, current - start));
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
  private void AddTokenHelper(TokenTypes type, object literal)
  {
    string text = source.Substring(start, current - start);
    tokens.Add(new Token(type, text, literal, line));
  }
  /// <summary>
  /// See if the next character match the actual character to form a unique simbol
  /// </summary>
  /// <param name="value"></param>
  /// <returns></returns>
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
  /// <returns>the character in the current position of the line</returns>
  private char LookAfter()
  {
    if (EOF()) return '\0';
    return source[current];
  }
  /// <summary>
  /// Advance one character in the inspection
  /// </summary>
  /// <returns>The current character before this method was called </returns>
  private char Advance()
  {
    current++;
    return source[current - 1];
  }
  private bool EOF()
  {
    return current >= source.Length;
  }
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
      default:
      type = TokenTypes.IDENTIFIER;
      literal = text;
      break;
    }
    AddTokenHelper(type, literal!);
}

}