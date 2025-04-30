
namespace WALLE;
public class Void { }
public class Interpreter : Expresion.IVisitor<object>, Stmt.IVisitor<Void>
{
  public List<Error> errors { get; private set; }
  public Enviroment enviroment;
  public Interpreter(List<Error> errors)
  {
    this.errors = errors;
    enviroment = new Enviroment(errors);
  }
  public void interpret(List<Stmt> statements)
  {
    if (statements == null || statements.Count == 0) return;
    int current = 0;
    var labelIndexMap = new Dictionary<string, int>();
    for (int i = 0; i < statements.Count; i++)
    {
      if (statements[i] is Label labelStmt)
      {
        if (!labelIndexMap.TryAdd(labelStmt.tag.writing, i))
        {
          errors.Add(new Error(labelStmt.tag.line, $"Internal Error: Duplicate label '{labelStmt.tag.writing}' detected during interpretation setup."));
          return;
        }
      }
    }
    while (current >= 0 && current < statements.Count)
    {
      Stmt currentStatement = statements[current];
      if (currentStatement is GoTo goToStmt)
      {
        object conditionResult = evaluate(goToStmt.condition);
        if (IsTrue(conditionResult))
        {
          string targetLabelName = goToStmt.label!.tag.writing;
          if (labelIndexMap.TryGetValue(targetLabelName, out int targetIndex))
          {
            current = targetIndex;
            continue;
          }
          else
          {
            errors.Add(new Error(goToStmt.label!.tag.line, $"Runtime Error: Label '{targetLabelName}' not found."));
            break;
          }
        }
      }
      else
      {
        execute(currentStatement);
        if (errors.Count > 0) break;
      }
      current++;
    }
  }
  private void execute(Stmt stmt)
  {
    stmt.accept(this);
  }
  public Void VisitExpressionStmt(Expression stmt)
  {
    evaluate(stmt.expresion);
    return new Void();
  }
  public Void VisitGoToStmt(GoTo stmt)
  {
    return default!;
  }
  public Void VisitLabelStmt(Label stmt)
  {
    return new Void();
  }
  public Void VisitSpawnStmt(Spawn stmt)
  {
    Token xContext = FindToken(stmt.x) ?? new Token(TokenTypes.IDENTIFIER, "?", null!, 0);
    Token yContext = FindToken(stmt.y) ?? xContext;
    if (!TryEvaluateAndConvert<int>(stmt.x, xContext, "Spawn X", out int x) || !TryEvaluateAndConvert<int>(stmt.y, yContext, "Spawn Y", out int y)) return new Void();
    if (Canva.IsOutRange(x, y)) errors.Add(new Error(xContext.line, $"Runtime Error: Spawn position ({x}, {y}) is out of the canvas bounds."));
    else Walle.Spawn(x, y);
    return new Void();
  }
  public Void VisitSizeStmt(Size stmt)
  {
    Token context = FindToken(stmt.number) ?? new Token(TokenTypes.IDENTIFIER, "?", null!, 0);
    if (!TryEvaluateAndConvert<int>(stmt.number, context, "Size", out int size)) return new Void();
    if (size <= 0) errors.Add(new Error(context.line, $"Runtime Error: Size ({size}) must be a positive integer."));
    else Walle.Size(size);
    return new Void();
  }
  public Void VisitColorStmt(Color stmt)
  {
    Token context = FindToken(stmt.color) ?? new Token(TokenTypes.IDENTIFIER, "?", null!, 0);
    if (!TryEvaluateAndConvert<string>(stmt.color, context, "Color", out string colorValue)) return new Void();
    if (!IsValidColor(colorValue)) errors.Add(new Error(context.line, $"Runtime Error: '{colorValue}' is not a valid color name."));
    else Walle.Color(colorValue);
    return new Void();
  }
  public Void VisitDrawLineStmt(DrawLine stmt)
  {
    Token dirXContext = FindToken(stmt.dirx as Expresion) ?? new Token(TokenTypes.IDENTIFIER, "?", null!, 0);
    Token dirYContext = FindToken(stmt.diry as Expresion) ?? dirXContext;
    Token distContext = FindToken(stmt.distance) ?? dirYContext;
    if (!TryEvaluateAndConvert<int>(stmt.dirx, dirXContext, "DrawLine direction X", out int dirX, IsValidDir, "Direction must be -1, 0, or 1") ||
            !TryEvaluateAndConvert<int>(stmt.diry, dirYContext, "DrawLine direction Y", out int dirY, IsValidDir, "Direction must be -1, 0, or 1") ||
            !TryEvaluateAndConvert<int>(stmt.distance, distContext, "DrawLine distance", out int distance)) return new Void();
    Walle.DrawLine(dirX, dirY, distance);
    return new Void();
  }
  public Void VisitDrawCircleStmt(DrawCircle stmt)
  {
    Token dirXContext = FindToken(stmt.dirx as Expresion) ?? new Token(TokenTypes.IDENTIFIER, "?", null!, 0);
    Token dirYContext = FindToken(stmt.diry as Expresion) ?? dirXContext;
    Token radiusContext = FindToken(stmt.radius) ?? dirYContext;

    if (!TryEvaluateAndConvert<int>(stmt.dirx, dirXContext, "DrawCircle center offset X", out int dirX, IsValidDir, "Direction must be -1, 0, or 1") ||
        !TryEvaluateAndConvert<int>(stmt.diry, dirYContext, "DrawCircle center offset Y", out int dirY, IsValidDir, "Direction must be -1, 0, or 1") ||
        !TryEvaluateAndConvert<int>(stmt.radius, radiusContext, "DrawCircle radius", out int radius, r => r > 0, "Radius must be positive")) return new Void();
    if (radius <= 0)
    {
      errors.Add(new Error(radiusContext.line, $"Runtime Error: DrawCircle radius ({radius}) must be positive."));
      return new Void();
    }

    Walle.DrawCircle(dirX, dirY, radius);
    return new Void();
  }
  public Void VisitDrawRectangleStmt(DrawRectangle stmt)
  {
    Token dirXContext = FindToken(stmt.dirx as Expresion) ?? new Token(TokenTypes.IDENTIFIER, "?", null!, 0);
    Token dirYContext = FindToken(stmt.diry as Expresion) ?? dirXContext;
    Token distContext = FindToken(stmt.distance) ?? dirYContext;
    Token widthContext = FindToken(stmt.width) ?? distContext;
    Token heightContext = FindToken(stmt.height) ?? widthContext;
    if (!TryEvaluateAndConvert<int>(stmt.dirx, dirXContext, "DrawRectangle corner offset X", out int dirX, IsValidDir, "Direction must be -1, 0, or 1") ||
        !TryEvaluateAndConvert<int>(stmt.diry, dirYContext, "DrawRectangle corner offset Y", out int dirY, IsValidDir, "Direction must be -1, 0, or 1") ||
        !TryEvaluateAndConvert<int>(stmt.distance, distContext, "DrawRectangle distance", out int distance) ||
        !TryEvaluateAndConvert<int>(stmt.width, widthContext, "DrawRectangle width", out int width, w => w > 0, "Width must be positive") ||
        !TryEvaluateAndConvert<int>(stmt.height, heightContext, "DrawRectangle height", out int height, h => h > 0, "Height must be positive")) return new Void();
    if (width <= 0 || height <= 0)
    {
      errors.Add(new Error(widthContext.line, $"Runtime Error: DrawRectangle width ({width}) and height ({height}) must be positive."));
      return new Void();
    }
    Walle.DrawRectangle(dirX, dirY, distance, width, height);
    return new Void();
  }
  public Void VisitFillStmt(Fill stmt)
  {
    Walle.Fill();
    return new Void();
  }
  public object VisitGetActualX(GetActualX expresion)
  {
    return Walle.GetActualX();
  }
  public object VisitGetActualY(GetActualY expresion)
  {
    return Walle.GetActualY();
  }
  public object VisitIsBrushColor(IsBrushColor expresion)
  {
    Token context = FindToken(expresion.color) ?? new Token(TokenTypes.IDENTIFIER, "?", null!, 0);
    if (!TryEvaluateAndConvert<string>(expresion.color, context, "IsBrushColor color", out string colorValue, IsValidColor, $"Value is not a valid color name")) return false;
    return Walle.IsBrushColor(colorValue);
  }
  public object VisitIsBrushSize(IsBrushSize expresion)
  {
    Token context = FindToken(expresion.size) ?? new Token(TokenTypes.IDENTIFIER, "?", null!, 0);
    if (!TryEvaluateAndConvert<int>(expresion.size, context, "IsBrushSize size", out int sizeValue, s => s > 0, "Size must be a positive integer")) return false;
    return Walle.IsBrushSize(sizeValue);
  }
  public object VisitGetColorCount(GetColorCount expresion)
  {
    Token colorCtx = FindToken(expresion.color) ?? new Token(TokenTypes.IDENTIFIER, "?", null!, 0);
    Token x1Ctx = FindToken(expresion.x1) ?? colorCtx;
    Token y1Ctx = FindToken(expresion.y1) ?? x1Ctx;
    Token x2Ctx = FindToken(expresion.x2) ?? y1Ctx;
    Token y2Ctx = FindToken(expresion.y2) ?? x2Ctx;
    if (!TryEvaluateAndConvert<string>(expresion.color, colorCtx, "GetColorCount color", out string colorValue, IsValidColor, "Invalid color name") ||
        !TryEvaluateAndConvert<int>(expresion.x1, x1Ctx, "GetColorCount x1", out int x1Value) ||
        !TryEvaluateAndConvert<int>(expresion.y1, y1Ctx, "GetColorCount y1", out int y1Value) ||
        !TryEvaluateAndConvert<int>(expresion.x2, x2Ctx, "GetColorCount x2", out int x2Value) ||
        !TryEvaluateAndConvert<int>(expresion.y2, y2Ctx, "GetColorCount y2", out int y2Value)) return 0;
    if (Canva.IsOutRange(x1Value, y1Value) || Canva.IsOutRange(x2Value, y2Value))
    {
      errors.Add(new Error(x1Ctx.line, $"Runtime Error: Coordinates provided to GetColorCount are out of canvas bounds."));
      return 0;
    }
    return Canva.GetColorCount(colorValue, x1Value, y1Value, x2Value, y2Value);
  }
  public object VisitIsCanvasColor(IsCanvasColor expresion)
  {
    Token colorCtx = FindToken(expresion.color) ?? new Token(TokenTypes.IDENTIFIER, "?", null!, 0);
    Token vertCtx = FindToken(expresion.vertical) ?? colorCtx;
    Token horzCtx = FindToken(expresion.horizontal) ?? vertCtx;
    if (!TryEvaluateAndConvert<string>(expresion.color, colorCtx, "IsCanvasColor color", out string colorValue, IsValidColor, "Invalid color name") ||
        !TryEvaluateAndConvert<int>(expresion.vertical, vertCtx, "IsCanvasColor vertical offset", out int verticalValue) ||
        !TryEvaluateAndConvert<int>(expresion.horizontal, horzCtx, "IsCanvasColor horizontal offset", out int horizontalValue)) return false;
    int targetX = Walle.GetActualX() + verticalValue;
    int targetY = Walle.GetActualY() + horizontalValue;
    if (Canva.IsOutRange(targetX, targetY))
    {
      errors.Add(new Error(vertCtx.line, $"Runtime Error: Calculated position ({targetX}, {targetY}) for IsCanvasColor is out of canvas bounds."));
      return false;
    }
    return Canva.IsCanvasColor(colorValue, verticalValue, horizontalValue);
  }
  public object VisitGetCanvasSize(GetCanvasSize expresion)
  {
    return Canva.GetCanvasSize();
  }
  public object visitAssign(Assign expresion)
  {
    object value = evaluate(expresion.value!);
    enviroment.assign(expresion.name!, value);
    return value;
  }
  public object visitLogical(Logical expresion)
  {
    if (SameTypeValue(evaluate(expresion.right), evaluate(expresion.left)))
    {
      object left = evaluate(expresion.left);
      if (expresion.Operator.type == TokenTypes.OR)
      {
        if (IsTrue(left)) return left;
      }
      else if (!IsTrue(left)) return left;
    }
    else errors.Add(new Error(expresion.Operator.line, "A logical conected can only apply to a expresions of the same type"));

    return evaluate(expresion.right);
  }
  public object visitLiteral(Literal expresion)
  {
    if (expresion.Value == "true") return true;
    if (expresion.Value == "false") return false;
    return expresion.Value;
  }
  public object visitGrouping(Grouping expresion)
  {
    return evaluate(expresion.expresion!);
  }
  public object visitUnary(Unary expresion)
  {
    object right = evaluate(expresion.Rightside!);
    switch (expresion.Operator!.type)
    {
      case TokenTypes.MINUS:
        NumberOperand(expresion.Operator, right);
        Console.WriteLine(-Convert.ToInt32(right));
        return -Convert.ToInt32(right);
      case TokenTypes.BANG:
        return !IsTrue(right);
      case TokenTypes.POW:
        NumberOperand(expresion.Operator, right);
        return Convert.ToInt32(right) * Convert.ToInt32(right);
    }
    return null!;
  }
  public object visitVariable(Variable expresion)
  {
    return enviroment.get(expresion.name!);
  }
  public object visitBinary(Binary expresion)
  {
    object left = evaluate(expresion.Leftside!);
    object right = evaluate(expresion.Rightside!);
    switch (expresion.Operator!.type)
    {
      case TokenTypes.MINUS:
        NumberOperands(expresion.Operator, left, right);
        return Convert.ToInt32(left) - Convert.ToInt32(right);
      case TokenTypes.PLUS:
        NumberOperands(expresion.Operator, left, right);
        return Convert.ToInt32(left) + Convert.ToInt32(right);
      case TokenTypes.MODUL:
        NumberOperands(expresion.Operator, left, right);
        return Convert.ToInt32(left) % Convert.ToInt32(right);
      case TokenTypes.DIVIDE:
        NumberOperands(expresion.Operator, left, right);
        return Convert.ToInt32(left) / Convert.ToInt32(right);
      case TokenTypes.PRODUCT:
        NumberOperands(expresion.Operator, left, right);
        return Convert.ToInt32(left) * Convert.ToInt32(right);
      case TokenTypes.GREATER:
        NumberOperands(expresion.Operator, left, right);
        return Convert.ToInt32(left) > Convert.ToInt32(right);
      case TokenTypes.GREATER_EQUAL:
        NumberOperands(expresion.Operator, left, right);
        return Convert.ToInt32(left) >= Convert.ToInt32(right);
      case TokenTypes.LESS:
        NumberOperands(expresion.Operator, left, right);
        return Convert.ToInt32(left) < Convert.ToInt32(right);
      case TokenTypes.LESS_EQUAL:
        NumberOperands(expresion.Operator, left, right);
        return Convert.ToInt32(left) <= Convert.ToInt32(right);
      case TokenTypes.BANG_EQUAL:
        return !IsEqual(left, right);
      case TokenTypes.EQUAL_EQUAL:
        return IsEqual(left, right);
    }
    return null!;
  }
  private bool IsEqual(object left, object right)
  {
    if (left == null && right == null) return true;
    if (left == null) return false;
    string? leftstr = left.ToString(), rightstr = right.ToString();
    return leftstr == rightstr;
  }
  private bool IsTrue(object ObjectConcrete)
  {
    if (ObjectConcrete == null) return false;
    if (ObjectConcrete is bool booleanValue) return booleanValue;
    return true;
  }
  private object evaluate(Expresion expresion)
  {
    return expresion.accept(this);
  }
  private void NumberOperand(Token Operator, object operand)
  {
    if (operand is string operandtring && int.TryParse(operandtring, out int operandInt)) return;
    errors.Add(new Error(Operator.line, "Operand must be a number"));
  }
  private void NumberOperands(Token Operator, object left, object right)
  {
    if (!IsAritmetic(left) || !IsAritmetic(right)) errors.Add(new Error(Operator.line, "Operands must be a numbers"));
  }
  private bool IsAritmetic(object expresion)
  {
    if (expresion is bool) return false;
    return true;
  }
  private bool IsVoid(List<Stmt> statements)
  {
    foreach (Stmt stmt in statements)
    {
      if (stmt is Spawn) return true;
      if (stmt is GoTo) return true;
      if (stmt is Size) return true;
      if (stmt is Color) return true;
      if (stmt is DrawLine) return true;
      if (stmt is DrawCircle) return true;
      if (stmt is DrawRectangle) return true;
      if (stmt is Fill) return true;
    }
    return false;
  }
  private bool IsValidColor(string color)
  {
    string newcolor = "";
    bool flag = false;
    for (int i = 0; i < color.Length; i++)
    {
      if (color[i] == ' ')
      {
        if (flag) break;
      }
      else
      {
        newcolor += color[i];
        flag = true;
      }
    }
    switch (newcolor)
    {
      case "Red":
      case "Blue":
      case "Green":
      case "Yellow":
      case "Orange":
      case "Purple":
      case "Black":
      case "White":
      case "Transparent": return true;
      default: return false;
    }
  }
  private bool IsValidDir(int dir)
  {
    return dir == 1 || dir == -1 || dir == 0;
  }
  private string? NegativeComprove(object dir)
  {
    if (dir is Literal)
    {
      Literal? lit = dir as Literal;
      return lit!.Value as string;
    }
    else
    {
      Unary? una = dir as Unary;
      Literal? lit = una!.Rightside as Literal;
      return una?.Operator!.writing + lit!.Value as string;
    }
  }
  private bool SameTypeValue(object left, object right)
  {
    if (left is bool && right is bool) return true;
    if (left is int && right is int) return true;
    return false;
  }
  private bool TryEvaluateAndConvert<T>(Expresion expr, Token contextToken, string argName, out T result, Func<T, bool>? validator = null, string? validationErrorMsg = null)
  {
    result = default(T)!;
    object evaluatedValue = evaluate(expr);
    int errorLine = contextToken?.line ?? 0;
    if (evaluatedValue is T typedValue) result = typedValue;
    else if (typeof(T) == typeof(int))
    {
      if (evaluatedValue is string s && int.TryParse(s, out int i)) result = (T)(object)i;
      else
      {
        errors.Add(new Error(errorLine, $"Runtime Error: Argument '{argName}' requires an integer value, but got '{evaluatedValue?.GetType().Name ?? "null"}'."));
        return false;
      }
    }
    else if (typeof(T) == typeof(string))
    {
      if (evaluatedValue != null) result = (T)(object)evaluatedValue.ToString()!;
      else
      {
        errors.Add(new Error(errorLine, $"Runtime Error: Argument '{argName}' requires a '{typeof(T).Name}' value, but got null."));
        return false;
      }
    }
    else
    {
      errors.Add(new Error(errorLine, $"Runtime Error: Argument '{argName}' requires a '{typeof(T).Name}' value, but got '{evaluatedValue?.GetType().Name ?? "null"}'."));
      return false;
    }
    if (validator != null && !validator(result))
    {
      string specificError = validationErrorMsg ?? $"Validation failed for argument '{argName}'";
      errors.Add(new Error(errorLine, $"Runtime Error: {specificError} (Value: '{result}')"));
      return false;
    }
    return true;
  }
  private Token? FindToken(Expresion? expr)
  {
    if (expr is Binary bin) return bin.Operator ?? FindToken(bin.Leftside);
    if (expr is Unary un) return un.Operator ?? FindToken(un.Rightside);
    if (expr is Variable var) return var.name;
    if (expr is Assign ass) return ass.name ?? FindToken(ass.value);
    if (expr is Literal lit) return null;
    if (expr is Logical log) return log.Operator ?? FindToken(log.left);
    return null;
  }
}
