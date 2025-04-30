namespace WALLE;

public class Parser
{
  public List<Error> errors { get; private set; }
  private readonly List<Token> tokens;
  private int current = 0;
  private Dictionary<string, Token> definedLabels = new Dictionary<string, Token>();
  public Parser(List<Token> tokens, List<Error> errors)
  {
    this.tokens = tokens;
    this.errors = errors ?? new List<Error>();
    this.definedLabels.Clear();
  }
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
        }
      }
      catch (ParseError)
      {
        Synchronize();
      }
      if (errors.Count > 10)
      {
        errors.Add(new Error(Peek()?.line ?? 0, "Too many errors. Parsing aborted."));
        break;
      }
    }
    return statementslist;
  }
  private class ParseError : Exception { }
  private Stmt? DeclarationOrStatement()
  {
    if (Check(TokenTypes.IDENTIFIER))
    {
      Token potentialLabelToken = Peek()!;
      Token? nextToken = PeekNext();
      if (nextToken == null || nextToken.line > potentialLabelToken.line)
      {
        return LabelDefinition();
      }
    }
    return Statement();
  }
  private Stmt Statement()
  {
    if (Match(TokenTypes.GOTO)) return GoToStatement(Previous());
    if (Match(TokenTypes.LABEL)) return LabelReferenceOrError();
    if (Match(TokenTypes.SPAWN)) return SpawnStatement(Previous());
    if (Match(TokenTypes.SIZE)) return SizeStatement(Previous());
    if (Match(TokenTypes.COLOR)) return ColorStatement(Previous());
    if (Match(TokenTypes.DRAWLINE)) return DrawLineStatement(Previous());
    if (Match(TokenTypes.DRAWCIRCLE)) return DrawCircleStatement(Previous());
    if (Match(TokenTypes.DRAWRECTANGLE)) return DrawRectangleStatement(Previous());
    if (Match(TokenTypes.FILL)) return FillStatement(Previous());
    return ExpressionStatement();
  }
  private Stmt LabelDefinition()
  {
    Token tag = Consume(TokenTypes.IDENTIFIER, "Expected label name.");
    string labelName = tag.writing;
    if (definedLabels.ContainsKey(labelName))
    {
      Error(tag, $"Label '{labelName}' is already defined on line {definedLabels[labelName].line}.");
    }
    else
    {
      definedLabels.Add(labelName, tag);
    }
    return new Label(tag);
  }
  private Stmt LabelReferenceOrError()
  {
    Token labelToken = Previous();
    Error(labelToken, "Invalid label usage or definition placement. Labels must be defined alone on their line.");
    return new Expression(new Literal($"INVALID_LABEL_PLACEMENT:{labelToken.writing}"));
  }
  private Stmt GoToStatement(Token startToken)
  {
    Token leftBrace = consumeSameLine(TokenTypes.LEFT_BRACE, "after 'GoTo'", startToken.line);
    if (!Check(TokenTypes.LABEL))
    {
      throw Error(Peek() ?? leftBrace, "Expected label name inside '[' ']'.");
    }
    Token labelToken = Advance();
    Stmt labelRef = new Label(labelToken);
    Token rightBrace = consumeSameLine(TokenTypes.RIGHT_BRACE, "after the label name in 'GoTo'", startToken.line);
    Token leftParen = consumeSameLine(TokenTypes.LEFT_PAREN, "after ']'", startToken.line);
    Expresion condition = Expression();
    Token rightParen = consumeSameLine(TokenTypes.RIGHT_PAREN, "after the condition in 'GoTo'", startToken.line);
    return new GoTo(condition, labelRef as Label);
  }
  private Stmt SpawnStatement(Token startToken)
  {
    Token leftParen = consumeSameLine(TokenTypes.LEFT_PAREN, "after 'Spawn'", startToken.line);
    Expresion x = ParseArgumentExpressionSameLine("Spawn X", leftParen.line);
    ConsumeCommaSameLine(leftParen.line);
    Expresion y = ParseArgumentExpressionSameLine("Spawn Y", leftParen.line);
    ConsumeRightParenSameLine(leftParen.line);
    return new Spawn(x, y);
  }
  private Stmt SizeStatement(Token startToken)
  {
    Token leftParen = consumeSameLine(TokenTypes.LEFT_PAREN, "after 'Size'", startToken.line);
    Expresion size = ParseArgumentExpressionSameLine("Size", leftParen.line);
    ConsumeRightParenSameLine(leftParen.line);
    return new Size(size);
  }
  private Stmt ColorStatement(Token startToken)
  {
    Token leftParen = consumeSameLine(TokenTypes.LEFT_PAREN, "after 'Color'", startToken.line);
    Expresion color = ParseArgumentExpressionSameLine("Color", leftParen.line);
    ConsumeRightParenSameLine(leftParen.line);
    return new Color(color);
  }
  private Stmt DrawLineStatement(Token startToken)
  {
    Token leftParen = consumeSameLine(TokenTypes.LEFT_PAREN, "after 'DrawLine'", startToken.line);
    Expresion dirx = ParseDrawArgumentSameLine("DrawLine direction X", leftParen.line);
    ConsumeCommaSameLine(leftParen.line);
    Expresion diry = ParseDrawArgumentSameLine("DrawLine direction Y", leftParen.line);
    ConsumeCommaSameLine(leftParen.line);
    Expresion distance = ParseArgumentExpressionSameLine("DrawLine distance", leftParen.line);
    ConsumeRightParenSameLine(leftParen.line);
    return new DrawLine(dirx, diry, distance);
  }
  private Stmt DrawCircleStatement(Token startToken)
  {
    Token leftParen = consumeSameLine(TokenTypes.LEFT_PAREN, "after 'DrawCircle'", startToken.line);
    Expresion dirx = ParseDrawArgumentSameLine("DrawCircle center offset X", leftParen.line);
    ConsumeCommaSameLine(leftParen.line);
    Expresion diry = ParseDrawArgumentSameLine("DrawCircle center offset Y", leftParen.line);
    ConsumeCommaSameLine(leftParen.line);
    Expresion radius = ParseArgumentExpressionSameLine("DrawCircle radius", leftParen.line);
    ConsumeRightParenSameLine(leftParen.line);
    return new DrawCircle(dirx, diry, radius);
  }
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
    ConsumeRightParenSameLine(leftParen.line);
    return new DrawRectangle(dirx, diry, distance, width, height);
  }
  private Stmt FillStatement(Token startToken)
  {
    Token leftParen = consumeSameLine(TokenTypes.LEFT_PAREN, "after 'Fill'", startToken.line);
    ConsumeRightParenSameLine(leftParen.line);
    return new Fill();
  }
  private Stmt ExpressionStatement()
  {
    Expresion expr = Expression();
    return new Expression(expr);
  }
  private Expresion Expression()
  {
    return Assignment();
  }
  private Expresion Assignment()
  {
    Expresion expr = Or();
    if (Match(TokenTypes.ASSIGNED))
    {
      Token assignOp = Previous();
      if (IsAtEnd() || Peek()!.line != assignOp.line)
      {
        throw Error(assignOp, "Assignment value must start on the same line as '<-'.");
      }
      Expresion value = Assignment();
      if (expr is Variable variable)
      {
        return new Assign(variable.name, value);
      }
      else
      {
        Error(assignOp, "Invalid assignment target.");
        return expr;
      }
    }
    return expr;
  }
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
  private Expresion Factor()
  {
    Expresion expr = Unary();
    while (Match(TokenTypes.DIVIDE, TokenTypes.PRODUCT, TokenTypes.MODUL))
    {
      Token op = Previous();
      Expresion right = Unary();
      expr = new Binary(expr, op, right);
    }
    return expr;
  }
  private Expresion Unary()
  {
    if (Match(TokenTypes.BANG, TokenTypes.MINUS, TokenTypes.POW))
    {
      Token op = Previous();
      Expresion right = Unary();
      return new Unary(op, right);
    }
    return Primary();
  }
  private Expresion Primary()
  {
    if (Match(TokenTypes.FALSE)) return new Literal(false);
    if (Match(TokenTypes.TRUE)) return new Literal(true);
    if (Match(TokenTypes.NUMBER)) return new Literal(Previous().literal);
    if (Match(TokenTypes.STRING)) return new Literal(Previous().literal);
    if (Match(TokenTypes.IDENTIFIER)) return new Variable(Previous());
    if (Match(TokenTypes.GETACTUALX)) return ParseGetActualX(Previous());
    if (Match(TokenTypes.GETACTUALY)) return ParseGetActualY(Previous());
    if (Match(TokenTypes.ISBRUSHCOLOR)) return ParseIsBrushColor(Previous());
    if (Match(TokenTypes.ISBRUSHSIZE)) return ParseIsBrushSize(Previous());
    if (Match(TokenTypes.ISCANVASCOLOR)) return ParseIsCanvasColor(Previous());
    if (Match(TokenTypes.GETCOLORCOUNT)) return ParseGetColorCount(Previous());
    if (Match(TokenTypes.GETCANVASSIZE)) return ParseGetCanvasSize(Previous());
    if (Match(TokenTypes.LEFT_PAREN))
    {
      Token leftParen = Previous();
      Expresion expr = Expression();
      ConsumeRightParenSameLine(leftParen.line);
      return new Grouping(expr);
    }
    throw Error(Peek(), "Expected expression.");
  }
  private Expresion ParseGetActualX(Token startToken)
  {
    Token leftParen = consumeSameLine(TokenTypes.LEFT_PAREN, "after 'GetActualX'", startToken.line);
    ConsumeRightParenSameLine(leftParen.line);
    return new GetActualX();
  }
  private Expresion ParseGetActualY(Token startToken)
  {
    Token leftParen = consumeSameLine(TokenTypes.LEFT_PAREN, "after 'GetActualY'", startToken.line);
    ConsumeRightParenSameLine(leftParen.line);
    return new GetActualY();
  }
  private Expresion ParseIsBrushColor(Token startToken)
  {
    Token leftParen = consumeSameLine(TokenTypes.LEFT_PAREN, "after 'IsBrushColor'", startToken.line);
    Expresion color = ParseArgumentExpressionSameLine("IsBrushColor", leftParen.line);
    ConsumeRightParenSameLine(leftParen.line);
    return new IsBrushColor(color);
  }
  private Expresion ParseIsBrushSize(Token startToken)
  {
    Token leftParen = consumeSameLine(TokenTypes.LEFT_PAREN, "after 'IsBrushSize'", startToken.line);
    Expresion size = ParseArgumentExpressionSameLine("IsBrushSize", leftParen.line);
    ConsumeRightParenSameLine(leftParen.line);
    return new IsBrushSize(size);
  }
  private Expresion ParseIsCanvasColor(Token startToken)
  {
    Token leftParen = consumeSameLine(TokenTypes.LEFT_PAREN, "after 'IsCanvasColor'", startToken.line);
    Expresion color = ParseArgumentExpressionSameLine("IsCanvasColor color", leftParen.line);
    ConsumeCommaSameLine(leftParen.line);
    Expresion vertical = ParseArgumentExpressionSameLine("IsCanvasColor vertical", leftParen.line);
    ConsumeCommaSameLine(leftParen.line);
    Expresion horizontal = ParseArgumentExpressionSameLine("IsCanvasColor horizontal", leftParen.line);
    ConsumeRightParenSameLine(leftParen.line);
    return new IsCanvasColor(color, vertical, horizontal);
  }
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
    ConsumeRightParenSameLine(leftParen.line);
    return new GetColorCount(color, x1, y1, x2, y2);
  }
  private Expresion ParseGetCanvasSize(Token startToken)
  {
    Token leftParen = consumeSameLine(TokenTypes.LEFT_PAREN, "after 'GetCanvasSize'", startToken.line);
    ConsumeRightParenSameLine(leftParen.line);
    return new GetCanvasSize();
  }
  private Expresion ParseArgumentExpressionSameLine(string context, int expectedLine)
  {
    CheckLine(context, expectedLine);
    return Expression();
  }
  private Expresion ParseDrawArgumentSameLine(string context, int expectedLine) // Cambiado de object a Expresion
  {
    CheckLine(context, expectedLine);
    if (Match(TokenTypes.MINUS))
    {
      Token minus = Previous();
      Token numberToken = consumeSameLine(TokenTypes.NUMBER, $"number after '-' in {context}", expectedLine);
      object literalValue = numberToken.literal; // O intenta parsear a int/double si es string
      if (literalValue is string s && int.TryParse(s, System.Globalization.CultureInfo.InvariantCulture, out int d)) literalValue = d;
      return new Unary(minus, new Literal(literalValue));
    }
    else if (Check(TokenTypes.NUMBER))
    {
      Token numberToken = consumeSameLine(TokenTypes.NUMBER, $"number for {context}", expectedLine);
      object literalValue = numberToken.literal;
      if (literalValue is string s && int.TryParse(s, System.Globalization.CultureInfo.InvariantCulture, out int d))literalValue = d;
      return new Literal(literalValue); 
    }
    else
    {
      Expresion exprArg = Expression();
      return exprArg;
    }
  }
  private void ConsumeCommaSameLine(int expectedLine)
  {
    consumeSameLine(TokenTypes.COMMA, "',' between arguments", expectedLine);
  }
  private void ConsumeRightParenSameLine(int expectedLine)
  {
    consumeSameLine(TokenTypes.RIGHT_PAREN, "')' at the end of argument list or expression", expectedLine);
  }
  private void CheckLine(string context, int expectedLine)
  {
    if (!IsAtEnd() && Peek()!.line != expectedLine)
    {
      throw Error(Peek(), $"Unexpected line break before {context}. Must be on line {expectedLine}.");
    }
  }
  private Token consumeSameLine(TokenTypes type, string contextMsg, int expectedLine)
  {
    if (!IsAtEnd() && Peek()!.line != expectedLine)
    {
      throw Error(Peek(), $"Unexpected line break {contextMsg}. Must be on line {expectedLine}. Found on line {Peek()!.line}");
    }
    if (Check(type))
    {
      return Advance();
    }
    throw Error(Peek(), $"Expected '{type}' {contextMsg}, but got '{Peek()?.writing ?? "EOF"}' on line {expectedLine}.");
  }
  private Token Consume(TokenTypes type, string message)
  {
    if (Check(type)) return Advance();
    throw Error(Peek(), message);
  }
  private bool Match(params TokenTypes[] types)
  {
    foreach (TokenTypes type in types)
    {
      if (Check(type))
      {
        Advance();
        return true;
      }
    }
    return false;
  }
  private bool Check(TokenTypes type)
  {
    if (IsAtEnd()) return false;
    return Peek()!.type == type;
  }
  private Token Advance()
  {
    if (!IsAtEnd()) current++;
    return Previous();
  }
  private bool IsAtEnd()
  {
    return current >= tokens.Count;
  }
  private Token? Peek()
  {
    if (IsAtEnd()) return null;
    return tokens[current];
  }
  private Token? PeekNext()
  {
    if (current + 1 >= tokens.Count) return null;
    return tokens[current + 1];
  }
  private Token Previous()
  {
    if (current == 0) return null!;
    return tokens[current - 1];
  }
  private ParseError Error(Token? token, string message)
  {
    int line = token?.line ?? tokens.LastOrDefault()?.line ?? 0;
    string where = token?.writing ?? "EOF";
    errors.Add(new Error(line, $"Error near '{where}': {message}"));
    return new ParseError();
  }
  private void Synchronize()
  {
    while (!IsAtEnd())
    {
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
        case TokenTypes.LABEL:
          return;
      }
      Advance();
    }
  }
}