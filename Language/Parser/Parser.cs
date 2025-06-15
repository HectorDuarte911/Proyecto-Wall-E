namespace WALLE;
/// <summary>///Detected all the sintaxis errors in the scanned tokens /// </summary>
public class Parser
{
  /// <summary>/// To parse statements/// </summary>
  private delegate Stmt StatementParser(Token keyword);
  /// <summary>///All the posible statements to parse/// </summary>
  private readonly Dictionary<TokenTypes, StatementParser> StatementParsers;
  /// <summary>///All the posible primary expresion/// </summary>
  private readonly Dictionary<TokenTypes, Func<Expresion>> PrimaryParsers;
  /// <summary>///Save de sintaxis errors detected/// </summary>
  public List<Error> errors { get; private set; }
  /// <summary>///Colection of the scanned tokens/// </summary>
  private readonly List<Token> tokens;
  /// <summary>///Current token in the parse/// </summary>
  private int current = 0;
  /// <summary>///Colection of the Labels in the parse/// </summary>
  private Dictionary<string, Token> definedLabels = new Dictionary<string, Token>();
  /// <summary>///Especial type of parser errors /// </summary>
  private class ParseError : Exception { }
  public Parser(List<Token> tokens, List<Error> errors)
  {
    this.tokens = tokens;
    this.errors = errors ?? new List<Error>();
    definedLabels.Clear();
    StatementParsers = new Dictionary<TokenTypes, StatementParser>{
    { TokenTypes.GOTO, GoToStatement },{ TokenTypes.SPAWN, SpawnStatement },
    { TokenTypes.SIZE, SizeStatement },{ TokenTypes.COLOR, ColorStatement },
    { TokenTypes.DRAWLINE, DrawLineStatement },{ TokenTypes.DRAWCIRCLE, DrawCircleStatement },
    { TokenTypes.DRAWRECTANGLE, DrawRectangleStatement },{ TokenTypes.FILL, FillStatement }
    };
    PrimaryParsers = new Dictionary<TokenTypes, Func<Expresion>>
    {
        { TokenTypes.FALSE, () => { Advance(); return new Literal(false); } },
        { TokenTypes.TRUE, () => { Advance(); return new Literal(true); } },
        { TokenTypes.STRING, () => { Advance(); return new Literal(Previous().literal); } },
        { TokenTypes.IDENTIFIER, () => { Advance(); return new Variable(Previous()); } },
        { TokenTypes.NUMBER, () => {Advance();if (int.TryParse((string)Previous().literal, out int val)) return new Literal(val);throw Error(Previous(), "Invalid number format.");}},
        { TokenTypes.LEFT_PAREN, () => {Advance();Expresion expr = Assignment();ConsumeRightParenSameLine();return new Grouping(expr);}},
        { TokenTypes.GETACTUALX, () => ParseGetActualX(Advance()) },{ TokenTypes.GETACTUALY, () => ParseGetActualY(Advance()) },
        { TokenTypes.ISBRUSHCOLOR, () => ParseIsBrushColor(Advance()) },{ TokenTypes.ISBRUSHSIZE, () => ParseIsBrushSize(Advance()) },
        { TokenTypes.ISCANVASCOLOR, () => ParseIsCanvasColor(Advance()) },{ TokenTypes.GETCOLORCOUNT, () => ParseGetColorCount(Advance()) },
        { TokenTypes.GETCANVASSIZE, () => ParseGetCanvasSize(Advance()) }
    };
  }
  /// <summary>///Principal method that parse all the tokens and convert it to statements /// </summary>
  public List<Stmt> Parse()
  {
    List<Stmt> statementslist = new List<Stmt>();
    while (!IsAtEnd())
    {
      try
      {
        Stmt? statement = DeclarationOrStatement();
        if (statement != null)
        {
          statementslist.Add(statement);
          CheckForTrailingTokens();
        }
      }
      catch (ParseError) { Synchronize(); }
    }
    CheckLabelReferences(statementslist);
    return statementslist;
  }
  /// <summary>/// Encapsule the patron of the while for binarys expresions/// </summary>
  private Expresion ParseBinaryExpression(Func<Expresion> higherPrecedenceParser, params TokenTypes[] types)
  {
    Expresion expr = higherPrecedenceParser();
    while (Match(types))
    {
      Token op = Previous();
      Expresion right = higherPrecedenceParser();
      expr = new Binary(expr, op, right);
    }
    return expr;
  }
  /// <summary>///Encapsule the patron of the while for logicals expresions/// </summary>
  private Expresion ParseLogicalExpression(Func<Expresion> higherPrecedenceParser, params TokenTypes[] types)
  {
    Expresion expr = higherPrecedenceParser();
    while (Match(types))
    {
      Token op = Previous();
      Expresion right = higherPrecedenceParser();
      expr = new Logical(expr, op, right);
    }
    return expr;
  }
  /// <summary>/// Parser and valid the number of arguments to a function or comand./// </summary>
  private List<Expresion> ParseAndValidateArguments(Token keyword, int expectedCount)
  {
    var args = ParseArgumentList(keyword.writing, keyword.line);
    if (args.Count != expectedCount) throw Error(keyword, $"'{keyword.writing}' expect {expectedCount} arguments, but found {args.Count}.");
    return args;
  }
  /// <summary>/// Parse a list of arguments/// </summary>
  private List<Expresion> ParseArgumentList(string functionName, int expectedLine)
  {
    consumeSameLine(TokenTypes.LEFT_PAREN, $"after '{functionName}'", expectedLine);
    var arguments = new List<Expresion>();
    if (!Check(TokenTypes.RIGHT_PAREN))
    {
      do
      {
        if (arguments.Count >= 250) Error(Peek(), "Cannot have more than 250 arguments.");
        arguments.Add(Assignment());
      } while (Match(TokenTypes.COMMA));
    }
    ConsumeRightParenSameLine();
    return arguments;
  }
  ///<summary>///Determinate if the actual token is a statement or a posible Label ///</summary>
  private Stmt DeclarationOrStatement()
  {
    if (Check(TokenTypes.IDENTIFIER) && (PeekNext() == null || PeekNext()!.line > Peek()!.line)) return LabelDefinition();
    return Statement();
  }
  /// <summary>///Determinate the type of statement of the token/// </summary>
  private Stmt Statement()
  {
    Token currentToken = Peek()!;
    if (currentToken != null && StatementParsers.TryGetValue(currentToken.type, out var parser))
    {
      Advance();
      return parser(currentToken);
    }
    return ExpressionStatement();
  }
  /// <summary>///Determinate if is a valid label or not/// </summary>
  private Stmt LabelDefinition()
  {
    Token tag = Consume(TokenTypes.IDENTIFIER, "Expected label name.");
    string labelName = tag.writing;
    if (definedLabels.ContainsKey(labelName)) Error(tag, $"Label '{labelName}' is already defined on line {definedLabels[labelName].line}.");
    else definedLabels.Add(labelName, tag);
    return new Label(tag);
  }
  /// <summary>///Detected the sintax error of a GoTo statement /// </summary>
  private Stmt GoToStatement(Token startToken)
  {
    consumeSameLine(TokenTypes.LEFT_BRACE, "after 'GoTo'", startToken.line);
    Token labelIdentifierToken = Consume(TokenTypes.IDENTIFIER, "Expected label name (identifier) inside '[' ']' for 'GoTo'.");
    Stmt labelRef = new Label(labelIdentifierToken);
    consumeSameLine(TokenTypes.RIGHT_BRACE, "after the label name in 'GoTo'", startToken.line);
    consumeSameLine(TokenTypes.LEFT_PAREN, "after ']'", startToken.line);
    Expresion condition = Assignment();
    consumeSameLine(TokenTypes.RIGHT_PAREN, "after the condition in 'GoTo'", startToken.line);
    return new GoTo(condition, labelRef as Label);
  }
  /// <summary>///Detected the sintax error of a Spaw statement /// </summary>
  private Stmt SpawnStatement(Token t)
  {
    var args = ParseAndValidateArguments(t, 2);
    return new Spawn(t, args[0], args[1]);
  }
  /// <summary>///Detected the sintax error of a Size statement /// </summary>
  private Stmt SizeStatement(Token t)
  {
    var args = ParseAndValidateArguments(t, 1);
    return new Size(t, args[0]);
  }
  /// <summary>///Detected the sintax error of a Color statement /// </summary>
  private Stmt ColorStatement(Token t)
  {
    var args = ParseAndValidateArguments(t, 1);
    return new Color(t, args[0]);
  }
  /// <summary>///Detected the sintax error of a DrawLine statement /// </summary>
  private Stmt DrawLineStatement(Token t)
  {
    var args = ParseAndValidateArguments(t, 3);
    return new DrawLine(t, args[0], args[1], args[2]);
  }
  /// <summary>///Detected the sintax error of a DrawCircle statement /// </summary>
  private Stmt DrawCircleStatement(Token t)
  {
    var args = ParseAndValidateArguments(t, 3);
    return new DrawCircle(t, args[0], args[1], args[2]);
  }
  /// <summary>///Detected the sintax error of a DrawRectangle statement /// </summary>
  private Stmt DrawRectangleStatement(Token t)
  {
    var args = ParseAndValidateArguments(t, 5);
    return new DrawRectangle(t, args[0], args[1], args[2], args[3], args[4]);
  }
  /// <summary>///Detected the sintax error of a Fill statement /// </summary>
  private Stmt FillStatement(Token t)
  {
    ParseAndValidateArguments(t, 0);
    return new Fill(t);
  }
  /// <summary>///Detected the sintax error of an Expresion statement /// </summary>
  private Stmt ExpressionStatement()
  {
    Expresion expr = Assignment();
    if (!(expr is Assign))
    {
      Token errorToken = FindTokenForExpression(expr) ?? Previous() ?? Peek()!;
      if (expr is Variable || expr is Literal)
        throw Error(errorToken, $"Unexpected '{errorToken.writing}'. Variables and literals cannot stand alone as statements. Use assignment '<-' or a command like Spawn, Draw, etc.");
      else throw Error(errorToken, "This expression cannot stand alone as a statement. Only assignments ('<-') or commands (Spawn, Draw, etc.) are allowed.");
    }
    return new Expression(expr);
  }
  /// <summary>///Detected the sintax error of an assigment expresion /// </summary>
  private Expresion Assignment()
  {
    Expresion expr = Or();
    if (Match(TokenTypes.ASSIGNED))
    {
      Token assignOp = Previous();
      if (IsAtEnd() || Peek()!.line != assignOp.line) throw Error(assignOp, "Assignment value must start on the same line as '<-'.");
      Expresion value = Assignment();
      if (expr is Variable variable) return new Assign(variable.name, value);
      else throw Error(assignOp, "Invalid assignment target.");
    }
    return expr;
  }
  /// <summary>///Detected the sintax error of an or expresion /// </summary>
  public Expresion Or() => ParseLogicalExpression(And, TokenTypes.OR);
  /// <summary>///Detected the sintax error of an and expresion /// </summary>
  private Expresion And() => ParseLogicalExpression(Equality, TokenTypes.AND);
  /// <summary>///Detected the sintax error of an equality expresion /// </summary>
  private Expresion Equality() => ParseBinaryExpression(Comparison, TokenTypes.BANG_EQUAL, TokenTypes.EQUAL_EQUAL);
  /// <summary>///Detected the sintax error of a comparison expresion /// </summary>
  private Expresion Comparison() => ParseBinaryExpression(Term, TokenTypes.GREATER, TokenTypes.GREATER_EQUAL, TokenTypes.LESS, TokenTypes.LESS_EQUAL);
  /// <summary>///Detected the sintax error of a term expresion /// </summary>
  private Expresion Term() => ParseBinaryExpression(Factor, TokenTypes.MINUS, TokenTypes.PLUS);
  /// <summary>///Detected the sintax error of a factor expresion /// </summary>
  private Expresion Factor() => ParseBinaryExpression(Power, TokenTypes.DIVIDE, TokenTypes.PRODUCT, TokenTypes.MODUL);
  /// <summary>///Detected the sintax error of a pow expresion /// </summary>
  private Expresion Power()
  {
    Expresion expr = Unary();
    if (Match(TokenTypes.POW))
    {
      Token op = Previous();
      Expresion right = Power();
      expr = new Binary(expr, op, right);
    }
    return expr;
  }
  /// <summary>///Detected the sintax error of an unary expresion /// </summary>
  private Expresion Unary()
  {
    if (Match(TokenTypes.BANG, TokenTypes.MINUS)) return new Unary(Previous(), Unary());
    return Primary();
  }
  /// <summary>///Detected the sintax error of a primary expresion /// </summary>
  private Expresion Primary()
  {
    Token? currentToken = Peek();
    if (currentToken == null) throw Error(null, "Expected expression but found end of file.");
    if (PrimaryParsers.TryGetValue(currentToken.type, out var parserFunction)) return parserFunction();
    throw Error(currentToken, "Expected expression.");
  }
  /// <summary>///Detected the sintax error of a GetActualX expresion /// </summary>
  private Expresion ParseGetActualX(Token t)
  {
    ParseAndValidateArguments(t, 0);
    return new GetActualX(t);
  }
  /// <summary>///Detected the sintax error of a GetActualY expresion /// </summary>
  private Expresion ParseGetActualY(Token t)
  {
    ParseAndValidateArguments(t, 0);
    return new GetActualY(t);
  }
  /// <summary>///Detected the sintax error of an IsBrushColor expresion /// </summary>
  private Expresion ParseIsBrushColor(Token t) => new IsBrushColor(t, ParseAndValidateArguments(t, 1)[0]);
  /// <summary>///Detected the sintax error of an IsBrushSize expresion /// </summary>
  private Expresion ParseIsBrushSize(Token t) => new IsBrushSize(t, ParseAndValidateArguments(t, 1)[0]);
  /// <summary>///Detected the sintax error of an IsCanvasColor expresion /// </summary>
  private Expresion ParseIsCanvasColor(Token t)
  {
    var args = ParseAndValidateArguments(t, 3);
    return new IsCanvasColor(t, args[0], args[1], args[2]);
  }
  /// <summary>///Detected the sintax error of a GetColorCount expresion /// </summary>
  private Expresion ParseGetColorCount(Token t)
  {
    var args = ParseAndValidateArguments(t, 5);
    return new GetColorCount(t, args[0], args[1], args[2], args[3], args[4]);
  }
  /// <summary>///Detected the sintax error of a GetCanvasSize expresion /// </summary>
  private Expresion ParseGetCanvasSize(Token t)
  {
    ParseAndValidateArguments(t, 0);
    return new GetCanvasSize(t);
  }
  /// <summary>///Advance the parsing to the next line of statement/// </summary>
  private void Synchronize()
  {
    Advance();
    while (!IsAtEnd())
    {
      if (current > 0 && Previous().line < Peek()!.line) return;
      TokenTypes type = Peek()!.type;
      if (StatementParsers.ContainsKey(type) || type == TokenTypes.IDENTIFIER) return;
      Advance();
    }
  }
  /// <summary>///Comprove if isn't garbage tokens after a statement /// </summary>
  private void CheckForTrailingTokens()
  {
    if (!IsAtEnd())
    {
      Token lastTokenOfStatement = Previous();
      Token nextToken = Peek()!;
      if (nextToken.line == lastTokenOfStatement.line) throw Error(nextToken, "Unexpected token found after the end of a statement on the same line.");
    }
  }
  /// <summary>///Check if a label in a GoTO sttement is declarated /// </summary>
  private void CheckLabelReferences(List<Stmt> statements)
  {
    foreach (Stmt stmt in statements)
    {
      if (stmt is GoTo goToStmt)
      {
        string labelName = goToStmt.label!.tag.writing;
        if (!definedLabels.ContainsKey(labelName)) Error(goToStmt.label.tag, $"Undefined label '{labelName}' referenced in GoTo statement.");
      }
    }
  }
  /// <summary>///Return a token of the introduced expresion/// </summary>
  private Token? FindTokenForExpression(Expresion expr)
  {
    if (expr is Variable var) return var.name;
    if (expr is Assign ass) return ass.name;
    if (expr is Binary bin) return bin.Operator ?? FindTokenForExpression(bin.leftside!);
    if (expr is Unary un) return un.Operator ?? FindTokenForExpression(un.rightside!);
    if (expr is Logical log) return log.Operator ?? FindTokenForExpression(log.left);
    if (expr is Grouping g) return FindTokenForExpression(g.expresion!);
    return null;
  }
  /// <summary>///Return the string writing of the diferents types of tokens/// </summary> 
  private string GetTokenTypeString(TokenTypes type)
  {
    foreach (char c in Lexical.MatchTokens.Keys)
    {
      if (Lexical.MatchTokens[c].Item1 == type) return c.ToString();
    }
    return type.ToString();
  }
  /// <summary>///Consume a token in the same line and throw a ParseError if isn't right to consume of isn.t in the same line/// </summary> 
  private Token consumeSameLine(TokenTypes type, string contextMsg, int expectedLine)
  {
    if (IsAtEnd()) throw Error(Previous(), $"Expected '{GetTokenTypeString(type)}' {contextMsg} on line {expectedLine}, but reached end of file.");
    Token nextToken = Peek()!;
    if (nextToken.line != expectedLine) throw Error(nextToken, $"Expected '{GetTokenTypeString(type)}' {contextMsg} on line {expectedLine}, but found '{nextToken.writing}' on line {nextToken.line}.");
    if (Check(type)) return Advance();
    throw Error(nextToken, $"Expected '{GetTokenTypeString(type)}' {contextMsg} on line {expectedLine}, but got '{nextToken.writing}' ({GetTokenTypeString(nextToken.type)}).");
  }
  /// <summary>///Check if the actual token type is the introduced and if isn't throw a new ParseError/// </summary>
  private Token Consume(TokenTypes type, string message)
  {
    if (Check(type)) return Advance();
    throw Error(Peek(), message);
  }
  /// <summary>///Consume in the same line a right paren token/// </summary>
  private void ConsumeRightParenSameLine() => consumeSameLine(TokenTypes.RIGHT_PAREN, "')' to close argument list or expression group", Previous().line);
  /// <summary>///View if the nect token match one of the introduced types/// </summary>
  private bool Match(params TokenTypes[] types)
  {
    foreach (TokenTypes type in types)
    {
      if (Check(type))
      {
        Advance(); return true;
      }
    }
    return false;
  }
  /// <summary>///Check if the introduced type is the same type of the actual token's /// </summary>
  private bool Check(TokenTypes type)
  {
    if (IsAtEnd()) return false;
    return Peek()!.type == type;
  }
  /// <summary>///Return the actual token and advance to the next token/// </summary>
  private Token Advance()
  {
    if (!IsAtEnd()) current++;
    return Previous();
  }
  /// <summary>///Comprove if is the end of the tokens/// </summary>
  private bool IsAtEnd() => current >= tokens.Count;
  /// <summary>///Return the next token in the list/// </summary>
  private Token? PeekNext()
  {
    if (current + 1 >= tokens.Count) return null;
    return tokens[current + 1];
  }
  /// <summary>///Return the actual token in the list/// </summary>
  private Token? Peek()
  {
    if (IsAtEnd()) return null;
    return tokens[current];
  }
  /// <summary>///Return the previous token in the list/// </summary>
  private Token Previous()
  {
    if (current == 0) return null!;
    return tokens[current - 1];
  }
  /// <summary>///Throw a ParseError/// </summary>
  private ParseError Error(Token? token, string message)
  {
    int line = token?.line ?? ((current > 0) ? Previous().line : (tokens.Count > 0 ? tokens[0].line : 0));
    string where = token?.writing ?? (IsAtEnd() ? "end of file" : "unknown token");
    errors.Add(new Error(line, $"Parse Error near '{where}': {message}"));
    return new ParseError();
  }
}
