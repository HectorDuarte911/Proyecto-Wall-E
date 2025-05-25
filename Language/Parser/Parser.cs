using System.Globalization;
namespace WALLE;
/// <summary>///Detected all the sintaxis errors in the scanned tokens /// </summary>
public class Parser
{
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
      if (errors.Count > 10)
      {
        int errorLine = Peek()?.line ?? tokens.LastOrDefault()?.line ?? 0;
        errors.Add(new Error(errorLine, "Too many errors. Parsing aborted."));
        break;
      }
    }
    CheckLabelReferences(statementslist);
    return statementslist;
  }
  ///<summary>///Determinate if the actual token is a statement or a posible Label ///</summary>
  private Stmt DeclarationOrStatement()
  {
    if (Check(TokenTypes.IDENTIFIER))
    {
      Token potentialLabelToken = Peek()!;
      Token? nextToken = PeekNext();
      if (nextToken == null || nextToken.line > potentialLabelToken.line) return LabelDefinition();
    }
    return Statement();
  }
  /// <summary>///Determinate the type of statement of the token/// </summary>
  private Stmt Statement()
  {
    if (Match(TokenTypes.GOTO)) return GoToStatement(Previous());
    if (Match(TokenTypes.SPAWN)) return SpawnStatement(Previous());
    if (Match(TokenTypes.SIZE)) return SizeStatement(Previous());
    if (Match(TokenTypes.COLOR)) return ColorStatement(Previous());
    if (Match(TokenTypes.DRAWLINE)) return DrawLineStatement(Previous());
    if (Match(TokenTypes.DRAWCIRCLE)) return DrawCircleStatement(Previous());
    if (Match(TokenTypes.DRAWRECTANGLE)) return DrawRectangleStatement(Previous());
    if (Match(TokenTypes.FILL)) return FillStatement(Previous());
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
    Expresion condition = Expression();
    consumeSameLine(TokenTypes.RIGHT_PAREN, "after the condition in 'GoTo'", startToken.line);
    return new GoTo(condition, labelRef as Label);
  }
  /// <summary>///Detected the sintax error of a Spaw statement /// </summary>
  private Stmt SpawnStatement(Token startToken)
  {
    Token leftParen = consumeSameLine(TokenTypes.LEFT_PAREN, "after 'Spawn'", startToken.line);
    Expresion x = ParseArgumentExpressionSameLine("Spawn X", leftParen.line);
    ConsumeCommaSameLine(leftParen.line);
    Expresion y = ParseArgumentExpressionSameLine("Spawn Y", leftParen.line);
    ConsumeRightParenSameLine();
    return new Spawn(startToken, x, y);
  }
  /// <summary>///Detected the sintax error of a Size statement /// </summary>
  private Stmt SizeStatement(Token startToken)
  {
    Token leftParen = consumeSameLine(TokenTypes.LEFT_PAREN, "after 'Size'", startToken.line);
    Expresion size = ParseArgumentExpressionSameLine("Size", leftParen.line);
    ConsumeRightParenSameLine();
    return new Size(startToken, size);
  }
  /// <summary>///Detected the sintax error of a Color statement /// </summary>
  private Stmt ColorStatement(Token startToken)
  {
    Token leftParen = consumeSameLine(TokenTypes.LEFT_PAREN, "after 'Color'", startToken.line);
    Expresion color = ParseArgumentExpressionSameLine("Color", leftParen.line);
    ConsumeRightParenSameLine();
    return new Color(startToken, color);
  }
  /// <summary>///Detected the sintax error of a DrawLine statement /// </summary>
  private Stmt DrawLineStatement(Token startToken)
  {
    Token leftParen = consumeSameLine(TokenTypes.LEFT_PAREN, "after 'DrawLine'", startToken.line);
    Expresion dirx = ParseDrawArgumentSameLine("DrawLine direction X", leftParen.line);
    ConsumeCommaSameLine(leftParen.line);
    Expresion diry = ParseDrawArgumentSameLine("DrawLine direction Y", leftParen.line);
    ConsumeCommaSameLine(leftParen.line);
    Expresion distance = ParseArgumentExpressionSameLine("DrawLine distance", leftParen.line);
    ConsumeRightParenSameLine();
    return new DrawLine(startToken, dirx, diry, distance);
  }
  /// <summary>///Detected the sintax error of a DrawCircle statement /// </summary>
  private Stmt DrawCircleStatement(Token startToken)
  {
    Token leftParen = consumeSameLine(TokenTypes.LEFT_PAREN, "after 'DrawCircle'", startToken.line);
    Expresion dirx = ParseDrawArgumentSameLine("DrawCircle center offset X", leftParen.line);
    ConsumeCommaSameLine(leftParen.line);
    Expresion diry = ParseDrawArgumentSameLine("DrawCircle center offset Y", leftParen.line);
    ConsumeCommaSameLine(leftParen.line);
    Expresion radius = ParseArgumentExpressionSameLine("DrawCircle radius", leftParen.line);
    ConsumeRightParenSameLine();
    return new DrawCircle(startToken, dirx, diry, radius);
  }
  /// <summary>///Detected the sintax error of a DrawRectangle statement /// </summary>
  private Stmt DrawRectangleStatement(Token startToken)
  {
    Token leftParen = consumeSameLine(TokenTypes.LEFT_PAREN, "after 'DrawRectangle'", startToken.line);
    Expresion dirx = ParseDrawArgumentSameLine("DrawRectangle corner offset X", leftParen.line);
    ConsumeCommaSameLine(leftParen.line);
    Expresion diry = ParseDrawArgumentSameLine("DrawRectangle corner offset Y", leftParen.line);
    ConsumeCommaSameLine(leftParen.line);
    Expresion distance = ParseArgumentExpressionSameLine("DrawRectangle distance", leftParen.line);
    ConsumeCommaSameLine(leftParen.line);
    Expresion width = ParseArgumentExpressionSameLine("DrawRectangle width", leftParen.line);
    ConsumeCommaSameLine(leftParen.line);
    Expresion height = ParseArgumentExpressionSameLine("DrawRectangle height", leftParen.line);
    ConsumeRightParenSameLine();
    return new DrawRectangle(startToken, dirx, diry, distance, width, height);
  }
  /// <summary>///Detected the sintax error of a Fill statement /// </summary>
  private Stmt FillStatement(Token startToken)
  {
    consumeSameLine(TokenTypes.LEFT_PAREN, "after 'Fill'", startToken.line);
    ConsumeRightParenSameLine();
    return new Fill(startToken);
  }
  /// <summary>///Detected the sintax error of an Expresion statement /// </summary>
  private Stmt ExpressionStatement()
  {
    Expresion expr = Expression();
    if (!(expr is Assign))
    {
      Token errorToken = FindTokenForExpression(expr) ?? Previous() ?? Peek()!;
      if (expr is Variable || expr is Literal)
        throw Error(errorToken, $"Unexpected '{errorToken.writing}'. Variables and literals cannot stand alone as statements. Use assignment '<-' or a command like Spawn, Draw, etc.");
      else throw Error(errorToken, "This expression cannot stand alone as a statement. Only assignments ('<-') or commands (Spawn, Draw, etc.) are allowed.");
    }
    return new Expression(expr);
  }
  /// <summary>///Detected the sintax error of an expresion /// </summary>
  private Expresion Expression() => Assignment();
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
  public Expresion Or()
  {
    Expresion expr = And();
    while (Match(TokenTypes.OR))
    {
      Token op = Previous();
      Expresion right = And();
      expr = new Logical(expr, op, right);
    }
    return expr;
  }
  /// <summary>///Detected the sintax error of an and expresion /// </summary>
  private Expresion And()
  {
    Expresion expr = Equality();
    while (Match(TokenTypes.AND))
    {
      Token op = Previous();
      Expresion right = Equality();
      expr = new Logical(expr, op, right);
    }
    return expr;
  }
  /// <summary>///Detected the sintax error of an equality expresion /// </summary>
  private Expresion Equality()
  {
    Expresion expr = Comparison();
    while (Match(TokenTypes.BANG_EQUAL, TokenTypes.EQUAL_EQUAL))
    {
      Token op = Previous();
      Expresion right = Comparison();
      expr = new Binary(expr, op, right);
    }
    return expr;
  }
  /// <summary>///Detected the sintax error of a comparison expresion /// </summary>
  private Expresion Comparison()
  {
    Expresion expr = Term();
    while (Match(TokenTypes.GREATER, TokenTypes.GREATER_EQUAL, TokenTypes.LESS, TokenTypes.LESS_EQUAL))
    {
      Token op = Previous();
      Expresion right = Term();
      expr = new Binary(expr, op, right);
    }
    return expr;
  }
  /// <summary>///Detected the sintax error of a term expresion /// </summary>
  private Expresion Term()
  {
    Expresion expr = Factor();
    while (Match(TokenTypes.MINUS, TokenTypes.PLUS))
    {
      Token op = Previous();
      Expresion right = Factor();
      expr = new Binary(expr, op, right);
    }
    return expr;
  }
  /// <summary>///Detected the sintax error of a factor expresion /// </summary>
  private Expresion Factor()
  {
    Expresion expr = Power();
    while (Match(TokenTypes.DIVIDE, TokenTypes.PRODUCT, TokenTypes.MODUL))
    {
      Token op = Previous();
      Expresion right = Power();
      expr = new Binary(expr, op, right);
    }
    return expr;
  }
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
    if (Match(TokenTypes.BANG, TokenTypes.MINUS))
    {
      Token op = Previous();
      Expresion right = Unary();
      return new Unary(op, right);
    }
    return Primary();
  }
  /// <summary>///Detected the sintax error of a primary expresion /// </summary>
  private Expresion Primary()
  {
    if (Match(TokenTypes.FALSE)) return new Literal(false);
    if (Match(TokenTypes.TRUE)) return new Literal(true);
    if (Match(TokenTypes.STRING)) return new Literal(Previous().literal);
    if (Match(TokenTypes.IDENTIFIER)) return new Variable(Previous());
    if (Match(TokenTypes.GETACTUALX)) return ParseGetActualX(Previous());
    if (Match(TokenTypes.GETACTUALY)) return ParseGetActualY(Previous());
    if (Match(TokenTypes.ISBRUSHCOLOR)) return ParseIsBrushColor(Previous());
    if (Match(TokenTypes.ISBRUSHSIZE)) return ParseIsBrushSize(Previous());
    if (Match(TokenTypes.ISCANVASCOLOR)) return ParseIsCanvasColor(Previous());
    if (Match(TokenTypes.GETCOLORCOUNT)) return ParseGetColorCount(Previous());
    if (Match(TokenTypes.GETCANVASSIZE)) return ParseGetCanvasSize(Previous());
    if (Match(TokenTypes.NUMBER))
    {
      object literalValue = Previous().literal;
      if (literalValue is string s && int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intValue)) return new Literal(intValue);
      return new Literal(literalValue);
    }
    if (Match(TokenTypes.LEFT_PAREN))
    {
      Expresion expr = Expression();
      ConsumeRightParenSameLine();
      return new Grouping(expr);
    }
    if (IsAtEnd()) throw Error(Previous(), "Expected expression, found end of file.");
    else throw Error(Peek(), "Expected expression.");
  }
  /// <summary>///Detected the sintax error of a GetActualX expresion /// </summary>
  private Expresion ParseGetActualX(Token startToken)
  {
    consumeSameLine(TokenTypes.LEFT_PAREN, "after 'GetActualX'", startToken.line);
    ConsumeRightParenSameLine();
    return new GetActualX(startToken);
  }
  /// <summary>///Detected the sintax error of a GetActualY expresion /// </summary>
  private Expresion ParseGetActualY(Token startToken)
  {
    consumeSameLine(TokenTypes.LEFT_PAREN, "after 'GetActualY'", startToken.line);
    ConsumeRightParenSameLine();
    return new GetActualY(startToken);
  }
  /// <summary>///Detected the sintax error of an IsBrushColor expresion /// </summary>
  private Expresion ParseIsBrushColor(Token startToken)
  {
    Token leftParen = consumeSameLine(TokenTypes.LEFT_PAREN, "after 'IsBrushColor'", startToken.line);
    Expresion color = ParseArgumentExpressionSameLine("IsBrushColor", leftParen.line);
    ConsumeRightParenSameLine();
    return new IsBrushColor(startToken, color);
  }
  /// <summary>///Detected the sintax error of an IsBrushSize expresion /// </summary>
  private Expresion ParseIsBrushSize(Token startToken)
  {
    Token leftParen = consumeSameLine(TokenTypes.LEFT_PAREN, "after 'IsBrushSize'", startToken.line);
    Expresion size = ParseArgumentExpressionSameLine("IsBrushSize", leftParen.line);
    ConsumeRightParenSameLine();
    return new IsBrushSize(startToken, size);
  }
  /// <summary>///Detected the sintax error of an IsCanvasColor expresion /// </summary>
  private Expresion ParseIsCanvasColor(Token startToken)
  {
    Token leftParen = consumeSameLine(TokenTypes.LEFT_PAREN, "after 'IsCanvasColor'", startToken.line);
    Expresion color = ParseArgumentExpressionSameLine("IsCanvasColor color", leftParen.line);
    ConsumeCommaSameLine(leftParen.line);
    Expresion vertical = ParseArgumentExpressionSameLine("IsCanvasColor vertical", leftParen.line);
    ConsumeCommaSameLine(leftParen.line);
    Expresion horizontal = ParseArgumentExpressionSameLine("IsCanvasColor horizontal", leftParen.line);
    ConsumeRightParenSameLine();
    return new IsCanvasColor(startToken, color, vertical, horizontal);
  }
  /// <summary>///Detected the sintax error of a GetColorCount expresion /// </summary>
  private Expresion ParseGetColorCount(Token startToken)
  {
    Token leftParen = consumeSameLine(TokenTypes.LEFT_PAREN, "after 'GetColorCount'", startToken.line);
    Expresion color = ParseArgumentExpressionSameLine("GetColorCount color", leftParen.line);
    ConsumeCommaSameLine(leftParen.line);
    Expresion x1 = ParseArgumentExpressionSameLine("GetColorCount x1", leftParen.line);
    ConsumeCommaSameLine(leftParen.line);
    Expresion y1 = ParseArgumentExpressionSameLine("GetColorCount y1", leftParen.line);
    ConsumeCommaSameLine(leftParen.line);
    Expresion x2 = ParseArgumentExpressionSameLine("GetColorCount x2", leftParen.line);
    ConsumeCommaSameLine(leftParen.line);
    Expresion y2 = ParseArgumentExpressionSameLine("GetColorCount y2", leftParen.line);
    ConsumeRightParenSameLine();
    return new GetColorCount(startToken, color, x1, y1, x2, y2);
  }
  /// <summary>///Detected the sintax error of a GetCanvasSize expresion /// </summary>
  private Expresion ParseGetCanvasSize(Token startToken)
  {
    consumeSameLine(TokenTypes.LEFT_PAREN, "after 'GetCanvasSize'", startToken.line);
    ConsumeRightParenSameLine();
    return new GetCanvasSize(startToken);
  }
  /// <summary>///Advance the parsing to the next line of statement/// </summary>
  private void Synchronize()
  {
    Advance();
    while (!IsAtEnd())
    {
      if (current > 0 && Previous().line < Peek()!.line) return;
      switch (Peek()?.type)
      {
        case TokenTypes.GOTO:
        case TokenTypes.SPAWN:
        case TokenTypes.SIZE:
        case TokenTypes.COLOR:
        case TokenTypes.DRAWLINE:
        case TokenTypes.DRAWCIRCLE:
        case TokenTypes.DRAWRECTANGLE:
        case TokenTypes.FILL:
        case TokenTypes.IDENTIFIER:return;
      }
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
  /// <summary>///Auxiliar than comprove is the token is in the correct line and return the result expresion/// </summary>
  private Expresion ParseArgumentExpressionSameLine(string context, int expectedLine)
  {
    CheckLine($"start of argument '{context}'", expectedLine);
    Expresion arg = Expression();
    return arg;
  }
  /// <summary>///Auxiliar thean comprove is the direction introduced is a valid direction to a draw statement /// </summary>
  private Expresion ParseDrawArgumentSameLine(string context, int expectedLine)
  {
    CheckLine($"start of draw argument '{context}'", expectedLine);
    if (Match(TokenTypes.MINUS))
    {
      Token minusToken = Previous();
      if (Check(TokenTypes.NUMBER) && Peek()!.line == minusToken.line)
      {
        Token numberToken = Advance();
        object literalValue = numberToken.literal;
        if (literalValue is string s && int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int val)) return new Literal(-val);
        else throw Error(numberToken, $"Internal error: Expected number literal after '-' in {context}, got '{literalValue}'.");
      }
      else
      {
        current--;
        return Expression();
      }
    }
    else return Expression();
  }
  /// <summary>///Check if is the actual token is in the line introduced /// </summary>
  private void CheckLine(string contextMsg, int expectedLine)
  {
    if (!IsAtEnd() && Peek()!.line != expectedLine) throw Error(Peek(), $"Unexpected line break before {contextMsg}. Expected on line {expectedLine}, but found on line {Peek()!.line}.");
  }
  /// <summary>///Return the string writing of the diferents types of tokens/// </summary> 
  private string GetTokenTypeString(TokenTypes type)
  {
    return type switch
    {
      TokenTypes.LEFT_PAREN => "'('",
      TokenTypes.RIGHT_PAREN => "')'",
      TokenTypes.LEFT_BRACE => "'['",
      TokenTypes.RIGHT_BRACE => "']'",
      TokenTypes.COMMA => "','",
      TokenTypes.ASSIGNED => "'<-'",
      TokenTypes.IDENTIFIER => "identifier",
      TokenTypes.NUMBER => "number",
      TokenTypes.STRING => "string",
      _ => type.ToString()
    };
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
  /// <summary>///Consume in the same line comma token /// </summary>
  private void ConsumeCommaSameLine(int expectedLine) => consumeSameLine(TokenTypes.COMMA, "',' between arguments", expectedLine);
  /// <summary>///Consume in the same line a right paren token/// </summary>
  private void ConsumeRightParenSameLine()
  {
    int expectedLine = Previous().line;
    consumeSameLine(TokenTypes.RIGHT_PAREN, "')' to close argument list or expression group", expectedLine);
  }
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
