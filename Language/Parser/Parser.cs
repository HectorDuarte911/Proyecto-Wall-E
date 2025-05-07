using System.Globalization;
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
      int lineBeforeStatement = (current > 0) ? tokens[current - 1].line : 1;
      if (!IsAtEnd() && Peek()!.line > lineBeforeStatement) lineBeforeStatement = Peek()!.line;
      try
      {
        Stmt? statement = DeclarationOrStatement();
        if (statement != null)
        {
          statementslist.Add(statement);
          CheckForTrailingTokens(statement);
        }
      }
      catch (ParseError)
      {
        Synchronize();
      }
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
  private class ParseError : Exception { }
  private void CheckForTrailingTokens(Stmt parsedStatement)
  {
    if (!IsAtEnd())
    {
      Token lastTokenOfStatement = Previous();
      Token nextToken = Peek()!;
      if (nextToken.line == lastTokenOfStatement.line) throw Error(nextToken, "Unexpected token found after the end of a statement on the same line.");
    }
  }
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
    if (definedLabels.ContainsKey(labelName)) Error(tag, $"Label '{labelName}' is already defined on line {definedLabels[labelName].line}.");
    else definedLabels.Add(labelName, tag);
    return new Label(tag);
  }
  private Stmt GoToStatement(Token startToken)
  {
    Token leftBrace = consumeSameLine(TokenTypes.LEFT_BRACE, "after 'GoTo'", startToken.line);
    Token labelIdentifierToken = Consume(TokenTypes.IDENTIFIER, "Expected label name (identifier) inside '[' ']' for 'GoTo'.");
    Stmt labelRef = new Label(labelIdentifierToken);
    Token rightBrace = consumeSameLine(TokenTypes.RIGHT_BRACE, "after the label name in 'GoTo'", startToken.line);
    Token leftParen = consumeSameLine(TokenTypes.LEFT_PAREN, "after ']'", startToken.line);
    Expresion condition = Expression();
    Token rightParen = consumeSameLine(TokenTypes.RIGHT_PAREN, "after the condition in 'GoTo'", startToken.line);
    return new GoTo(condition, labelRef as Label);
  }
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
  private Stmt SpawnStatement(Token startToken)
  {
    Token leftParen = consumeSameLine(TokenTypes.LEFT_PAREN, "after 'Spawn'", startToken.line);
    Expresion x = ParseArgumentExpressionSameLine("Spawn X", leftParen.line);
    ConsumeCommaSameLine(leftParen.line);
    Expresion y = ParseArgumentExpressionSameLine("Spawn Y", leftParen.line);
    ConsumeRightParenSameLine(leftParen.line);
    return new Spawn(startToken,x, y);
  }
  private Stmt SizeStatement(Token startToken)
  {
    Token leftParen = consumeSameLine(TokenTypes.LEFT_PAREN, "after 'Size'", startToken.line);
    Expresion size = ParseArgumentExpressionSameLine("Size", leftParen.line);
    ConsumeRightParenSameLine(leftParen.line);
    return new Size(startToken,size);
  }
  private Stmt ColorStatement(Token startToken)
  {
    Token leftParen = consumeSameLine(TokenTypes.LEFT_PAREN, "after 'Color'", startToken.line);
    Expresion color = ParseArgumentExpressionSameLine("Color", leftParen.line);
    ConsumeRightParenSameLine(leftParen.line);
    return new Color(startToken,color);
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
    return new DrawLine(startToken,dirx, diry, distance);
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
    return new DrawCircle(startToken,dirx, diry, radius);
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
    return new DrawRectangle(startToken,dirx, diry, distance, width, height);
  }
  private Stmt FillStatement(Token startToken)
  {
    Token leftParen = consumeSameLine(TokenTypes.LEFT_PAREN, "after 'Fill'", startToken.line);
    ConsumeRightParenSameLine(leftParen.line);
    return new Fill(startToken);
  }
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
      if (IsAtEnd() || Peek()!.line != assignOp.line) throw Error(assignOp, "Assignment value must start on the same line as '<-'.");
      Expresion value = Assignment();
      if (expr is Variable variable) return new Assign(variable.name, value);
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
        Expresion expr = Power();
        while (Match(TokenTypes.DIVIDE, TokenTypes.PRODUCT, TokenTypes.MODUL))
        {
            Token op = Previous();
            Expresion right = Power();
            expr = new Binary(expr, op, right);
        }
        return expr;
    }
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
      if (literalValue is string s && int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intValue))
      {
        return new Literal(intValue);
      }
      if (!(literalValue is int)) { }
      return new Literal(literalValue);
    }
    if (Match(TokenTypes.LEFT_PAREN))
    {
      Token leftParen = Previous();
      Expresion expr = Expression();
      ConsumeRightParenSameLine(leftParen.line);
      return new Grouping(expr);
    }
    if (IsAtEnd()) throw Error(Previous(), "Expected expression, found end of file.");
    else throw Error(Peek(), "Expected expression.");
  }
  private Expresion ParseGetActualX(Token startToken)
  {
    Token leftParen = consumeSameLine(TokenTypes.LEFT_PAREN, "after 'GetActualX'", startToken.line);
    ConsumeRightParenSameLine(leftParen.line);
    return new GetActualX(startToken);
  }
  private Expresion ParseGetActualY(Token startToken)
  {
    Token leftParen = consumeSameLine(TokenTypes.LEFT_PAREN, "after 'GetActualY'", startToken.line);
    ConsumeRightParenSameLine(leftParen.line);
    return new GetActualY(startToken);
  }
  private Expresion ParseIsBrushColor(Token startToken)
  {
    Token leftParen = consumeSameLine(TokenTypes.LEFT_PAREN, "after 'IsBrushColor'", startToken.line);
    Expresion color = ParseArgumentExpressionSameLine("IsBrushColor", leftParen.line);
    ConsumeRightParenSameLine(leftParen.line);
    return new IsBrushColor(startToken,color);
  }
  private Expresion ParseIsBrushSize(Token startToken)
  {
    Token leftParen = consumeSameLine(TokenTypes.LEFT_PAREN, "after 'IsBrushSize'", startToken.line);
    Expresion size = ParseArgumentExpressionSameLine("IsBrushSize", leftParen.line);
    ConsumeRightParenSameLine(leftParen.line);
    return new IsBrushSize(startToken,size);
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
    return new IsCanvasColor(startToken,color, vertical, horizontal);
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
    return new GetColorCount(startToken,color, x1, y1, x2, y2);
  }
  private Expresion ParseGetCanvasSize(Token startToken)
  {
    Token leftParen = consumeSameLine(TokenTypes.LEFT_PAREN, "after 'GetCanvasSize'", startToken.line);
    ConsumeRightParenSameLine(leftParen.line);
    return new GetCanvasSize(startToken);
  }
  private Expresion ParseArgumentExpressionSameLine(string context, int expectedLine)
  {
    CheckLine($"start of argument '{context}'", expectedLine);
    Expresion arg = Expression();
    return arg;
  }
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
  private void ConsumeCommaSameLine(int expectedLine)
  {
    consumeSameLine(TokenTypes.COMMA, "',' between arguments", expectedLine);
  }
  private void ConsumeRightParenSameLine(int startParenLine)
  {
    int expectedLine = Previous().line;
    consumeSameLine(TokenTypes.RIGHT_PAREN, "')' to close argument list or expression group", expectedLine);
  }
  private void CheckLine(string contextMsg, int expectedLine)
  {
    if (!IsAtEnd() && Peek()!.line != expectedLine)
      throw Error(Peek(), $"Unexpected line break before {contextMsg}. Expected on line {expectedLine}, but found on line {Peek()!.line}.");
  }
  private Token consumeSameLine(TokenTypes type, string contextMsg, int expectedLine)
  {
    if (IsAtEnd()) throw Error(Previous(), $"Expected '{GetTokenTypeString(type)}' {contextMsg} on line {expectedLine}, but reached end of file.");
    Token nextToken = Peek()!;
    if (nextToken.line != expectedLine) throw Error(nextToken, $"Expected '{GetTokenTypeString(type)}' {contextMsg} on line {expectedLine}, but found '{nextToken.writing}' on line {nextToken.line}.");
    if (Check(type)) return Advance();
    throw Error(nextToken, $"Expected '{GetTokenTypeString(type)}' {contextMsg} on line {expectedLine}, but got '{nextToken.writing}' ({GetTokenTypeString(nextToken.type)}).");
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
    int line = token?.line ?? ((current > 0) ? Previous().line : (tokens.Count > 0 ? tokens[0].line : 0));
    string where = token?.writing ?? (IsAtEnd() ? "end of file" : "unknown token");
    errors.Add(new Error(line, $"Parse Error near '{where}': {message}"));
    return new ParseError();
  }
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
        case TokenTypes.IDENTIFIER:
          return;
      }
      Advance();
    }
  }
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
  private Token? FindTokenForExpression(Expresion expr)
  {
    if (expr is Variable var) return var.name;
    if (expr is Assign ass) return ass.name;
    if (expr is Binary bin) return bin.Operator ?? FindTokenForExpression(bin.Leftside!);
    if (expr is Unary un) return un.Operator ?? FindTokenForExpression(un.Rightside!);
    if (expr is Logical log) return log.Operator ?? FindTokenForExpression(log.left);
    if (expr is Grouping g) return FindTokenForExpression(g.expresion!);
    return null;
  }
}
