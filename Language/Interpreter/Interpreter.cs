namespace WALLE;

public class Interpreter : Expresion.IVisitor<object>, Stmt.IVisitor<object?>
{
  internal class JumpRequest 
  {
    public int TargetIndex { get; } 
    public int OriginalGoToIndex { get; }
    public JumpRequest(int targetIndex, int originalGoToIndex)
    {
      TargetIndex = targetIndex;
      OriginalGoToIndex = originalGoToIndex;
    }
  }
  public List<Error> errors { get; private set; }
  private Enviroment enviroment;
  private Dictionary<string, int> labelMap = new Dictionary<string, int>();
  private const int MAXSteps = 10000;
  private int executionSteps = 0;
  private int executingStmtIndex = -1;
  public Interpreter(List<Error> errors)
  {
    this.errors = errors;
    enviroment = new Enviroment(errors);
  }
  public void interpret(List<Stmt> statements)
  {
    labelMap.Clear();
    executionSteps = 0;
    executingStmtIndex = -1;
    try
    {
      PreprocessLabels(statements);
      if (errors.Count > 0) return;
    }
    catch (RuntimeError error)
    {
      return;
    }
    int currentStatementIndex = 0;
    while (currentStatementIndex < statements.Count)
    {
      executionSteps++;
      if (executionSteps > MAXSteps)
      {
        RuntimeError(GetCurrentToken(statements, currentStatementIndex), "Maximum execution steps exceeded. Possible infinite loop detected.");
        return;
      }
      executingStmtIndex = currentStatementIndex;
      Stmt currentStmt = statements[executingStmtIndex];
      object? result = null;
      try
      {
        result = execute(currentStmt);
      }
      catch (RuntimeError error)
      {
        return;
      }
      if (result is JumpRequest jump)
      {
        if (jump.TargetIndex < 0 || jump.TargetIndex > statements.Count)
        {
          RuntimeError(GetCurrentToken(statements, jump.OriginalGoToIndex),
          $"Internal error: Jump target index {jump.TargetIndex} is out of bounds (valid range 0-{statements.Count}).");
          return;
        }
        currentStatementIndex = jump.TargetIndex;
      }
      else currentStatementIndex++;
    }
  }
  private void PreprocessLabels(List<Stmt> statements)
  {
    labelMap.Clear();
    for (int i = 0; i < statements.Count; i++)
    {
      if (statements[i] is Label labelStmt)
      {
        string labelName = labelStmt.tag.writing;
        if (labelMap.ContainsKey(labelName))
        {
          RuntimeError(labelStmt.tag, $"Duplicate label '{labelName}' found during interpretation preprocessing. Original definition was on line {GetLabelOriginalLine(labelName)}.");
        }
        else labelMap.Add(labelName, i + 1);
      }
    }
  }
  private int GetLabelOriginalLine(string labelName)
  {
    return 0;
  }
  private Token? GetCurrentToken(List<Stmt> statements, int index)
  {
    if (index < 0 || index >= statements.Count) return null;
    Stmt stmt = statements[index];
    if (stmt is GoTo gt && gt.label != null) return gt.label.tag;
    if (stmt is Label lbl) return lbl.tag;
    if (stmt is DrawLine dl) return dl.KeywordToken;
    if (stmt is Spawn sp) return sp.KeywordToken;
    if (stmt is Fill fi) return fi.KeywordToken;
    if (stmt is Expression exprStmt)
    {
      if (exprStmt.expresion is Assign ass && ass.name != null) return ass.name;
      return FindToken(exprStmt.expresion);
    }
    return FindTokenFromAnywhere(stmt);
  }
  private Token? FindTokenFromAnywhere(Stmt stmt)
  {
    return null;
  }
  private object? execute(Stmt stmt)
  {
    return stmt.accept(this);
  }
  private object evaluate(Expresion expresion)
  {
    return expresion.accept(this);
  }
  public object VisitExpressionStmt(Expression stmt)
  {
    evaluate(stmt.expresion);
    return null!;
  }
  public object? VisitGoToStmt(GoTo stmt)
  {
    object? conditionResult = null;
    try
    {
      conditionResult = evaluate(stmt.condition);
    }
    catch (RuntimeError err)
    {
      RuntimeError(stmt.label?.tag ?? GetCurrentToken(new List<Stmt> { stmt }, 0), $"Error evaluating GoTo condition: {err.Message}");
      return null;
    }
    if (IsTrue(conditionResult))
    {
      if (stmt.label == null || stmt.label.tag == null)
      {
        RuntimeError(GetCurrentToken(new List<Stmt> { stmt }, 0), "Internal Error: GoTo statement has missing label information.");
        return null;
      }
      string labelName = stmt.label.tag.writing;
      if (labelMap.TryGetValue(labelName, out int targetIndex)) return new JumpRequest(targetIndex, executingStmtIndex);
      else
      {
        RuntimeError(stmt.label.tag, $"Undefined label '{labelName}' referenced in GoTo statement.");
        return null;
      }
    }
    return null;
  }
  public object? VisitLabelStmt(Label stmt)
  {
    return null;
  }
  public object? VisitSpawnStmt(Spawn stmt)
  {
    Token fallbackToken = stmt.KeywordToken;
    Token xContext = FindToken(stmt.x) ?? fallbackToken;
    Token yContext = FindToken(stmt.y) ?? fallbackToken;
    if (!TryEvaluateAndConvert<int>(stmt.x, xContext, "Spawn X", out int x) || !TryEvaluateAndConvert<int>(stmt.y, yContext, "Spawn Y", out int y)) return null;
    if (Canva.IsOutRange(x, y)) errors.Add(new Error(xContext.line, $"Runtime Error: Spawn position ({x}, {y}) is out of the canvas bounds."));
    else Walle.Spawn(x, y);
    return null;
  }
  public object? VisitSizeStmt(Size stmt)
  {
    Token fallbackToken = stmt.KeywordToken;
    Token context = FindToken(stmt.number) ?? fallbackToken;
    if (!TryEvaluateAndConvert<int>(stmt.number, context, "Size", out int size)) return null;
    if (size <= 0) errors.Add(new Error(context.line, $"Runtime Error: Size ({size}) must be a positive integer."));
    else Walle.Size(size);
    return null;
  }
  public object? VisitColorStmt(Color stmt)
  {
    Token fallbackToken = stmt.KeywordToken;
    Token context = FindToken(stmt.color) ?? fallbackToken;
    if (!TryEvaluateAndConvert<string>(stmt.color, context, "Color", out string colorValue)) return null;
    if (!IsValidColor(colorValue)) errors.Add(new Error(context.line, $"Runtime Error: '{colorValue}' is not a valid color name."));
    else Walle.Color(colorValue);
    return null;
  }
  public object? VisitDrawLineStmt(DrawLine stmt)
  {
    Token fallbackToken = stmt.KeywordToken;
    Token dirXContext = FindToken(stmt.dirx) ?? fallbackToken;
    Token dirYContext = FindToken(stmt.diry) ?? fallbackToken;
    Token distContext = FindToken(stmt.distance) ?? fallbackToken;
    if (!TryEvaluateAndConvert<int>(stmt.dirx, dirXContext, "DrawLine direction X", out int dirX, IsValidDir, "Direction must be -1, 0, or 1") ||
            !TryEvaluateAndConvert<int>(stmt.diry, dirYContext, "DrawLine direction Y", out int dirY, IsValidDir, "Direction must be -1, 0, or 1") ||
            !TryEvaluateAndConvert<int>(stmt.distance, distContext, "DrawLine distance", out int distance)) return null;
    Walle.DrawLine(dirX, dirY, distance);
    return null;
  }
  public object? VisitDrawCircleStmt(DrawCircle stmt)
  {
    Token fallbackToken = stmt.KeywordToken;
    Token dirXContext = FindToken(stmt.dirx) ?? fallbackToken;
    Token dirYContext = FindToken(stmt.diry) ?? fallbackToken;
    Token radiusContext = FindToken(stmt.radius) ?? fallbackToken;
    if (!TryEvaluateAndConvert<int>(stmt.dirx, dirXContext, "DrawCircle center offset X", out int dirX, IsValidDir, "Direction must be -1, 0, or 1") ||
        !TryEvaluateAndConvert<int>(stmt.diry, dirYContext, "DrawCircle center offset Y", out int dirY, IsValidDir, "Direction must be -1, 0, or 1") ||
        !TryEvaluateAndConvert<int>(stmt.radius, radiusContext, "DrawCircle radius", out int radius, r => r > 0, "Radius must be positive")) return null;
    if (radius <= 0)
    {
      errors.Add(new Error(radiusContext.line, $"Runtime Error: DrawCircle radius ({radius}) must be positive."));
      return null;
    }
    Walle.DrawCircle(dirX, dirY, radius);
    return null;
  }
  public object? VisitDrawRectangleStmt(DrawRectangle stmt)
  {
    Token fallbackToken = stmt.KeywordToken;
    Token dirXContext = FindToken(stmt.dirx) ?? fallbackToken;
    Token dirYContext = FindToken(stmt.diry) ?? fallbackToken;
    Token distContext = FindToken(stmt.distance) ?? fallbackToken;
    Token widthContext = FindToken(stmt.width) ?? fallbackToken;
    Token heightContext = FindToken(stmt.height) ?? fallbackToken;
    if (!TryEvaluateAndConvert<int>(stmt.dirx, dirXContext, "DrawRectangle corner offset X", out int dirX, IsValidDir, "Direction must be -1, 0, or 1") ||
        !TryEvaluateAndConvert<int>(stmt.diry, dirYContext, "DrawRectangle corner offset Y", out int dirY, IsValidDir, "Direction must be -1, 0, or 1") ||
        !TryEvaluateAndConvert<int>(stmt.distance, distContext, "DrawRectangle distance", out int distance) ||
        !TryEvaluateAndConvert<int>(stmt.width, widthContext, "DrawRectangle width", out int width, w => w > 0, "Width must be positive") ||
        !TryEvaluateAndConvert<int>(stmt.height, heightContext, "DrawRectangle height", out int height, h => h > 0, "Height must be positive")) return null;
    if (width <= 0 || height <= 0)
    {
      errors.Add(new Error(widthContext.line, $"Runtime Error: DrawRectangle width ({width}) and height ({height}) must be positive."));
      return null;
    }
    Walle.DrawRectangle(dirX, dirY, distance, width, height);
    return null;
  }
  public object? VisitFillStmt(Fill stmt)
  {
    Walle.Fill();
    return null;
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
    Token fallbackToken = expresion.KeywordToken;
    Token context = FindToken(expresion.color) ?? fallbackToken;
    if (!TryEvaluateAndConvert<string>(expresion.color, context, "IsBrushColor color", out string colorValue, IsValidColor, $"Value is not a valid color name")) return false;
    return Walle.IsBrushColor(colorValue);
  }
  public object VisitIsBrushSize(IsBrushSize expresion)
  {
    Token fallbackToken = expresion.KeywordToken;
    Token context = FindToken(expresion.size) ?? fallbackToken;
    if (!TryEvaluateAndConvert<int>(expresion.size, context, "IsBrushSize size", out int sizeValue, s => s > 0, "Size must be a positive integer")) return false;
    return Walle.IsBrushSize(sizeValue);
  }
  public object VisitGetColorCount(GetColorCount expresion)
  {
    Token fallbackToken = expresion.KeywordToken;
    Token colorCtx = FindToken(expresion.color) ?? fallbackToken;
    Token x1Ctx = FindToken(expresion.x1) ?? fallbackToken;
    Token y1Ctx = FindToken(expresion.y1) ?? fallbackToken;
    Token x2Ctx = FindToken(expresion.x2) ?? fallbackToken;
    Token y2Ctx = FindToken(expresion.y2) ?? fallbackToken;
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
    Token fallbackToken = expresion.KeywordToken;
    Token colorCtx = FindToken(expresion.color) ?? fallbackToken;
    Token vertCtx = FindToken(expresion.vertical) ?? fallbackToken;
    Token horzCtx = FindToken(expresion.horizontal) ?? fallbackToken;
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
    if (expresion.Value is bool b) return b;
    if (expresion.Value is string s)
    {
      if (int.TryParse(s, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int intValue)) return intValue;
      if (s.Equals("true", StringComparison.OrdinalIgnoreCase)) return true;
      if (s.Equals("false", StringComparison.OrdinalIgnoreCase)) return false;
      return s;
    }
    if (expresion.Value is int i) return i;
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
        return -(int)right;
      case TokenTypes.BANG:
        return !IsTrue(right);
      case TokenTypes.POW:
        NumberOperand(expresion.Operator, right);
        int num = (int)right;
        return num * num;
    }
    throw new RuntimeError(expresion.Operator, "Unknown unary operator.");
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
        return (int)left - (int)right;
      case TokenTypes.PLUS:
        if (left is string || right is string) return left?.ToString() + right?.ToString();
        NumberOperands(expresion.Operator, left, right);
        return (int)left + (int)right;
      case TokenTypes.DIVIDE:
        NumberOperands(expresion.Operator, left, right);
        if ((int)right == 0)
          throw new RuntimeError(expresion.Operator, "Division by zero.");
        return (int)left / (int)right;
      case TokenTypes.PRODUCT:
        NumberOperands(expresion.Operator, left, right);
        return (int)left * (int)right;
      case TokenTypes.MODUL:
        NumberOperands(expresion.Operator, left, right);
        if ((int)right == 0)
          throw new RuntimeError(expresion.Operator, "Modulo by zero.");
        return (int)left % (int)right;
      case TokenTypes.GREATER:
        NumberOperands(expresion.Operator, left, right);
        return (int)left > (int)right;
      case TokenTypes.GREATER_EQUAL:
        NumberOperands(expresion.Operator, left, right);
        return (int)left >= (int)right;
      case TokenTypes.LESS:
        NumberOperands(expresion.Operator, left, right);
        return (int)left < (int)right;
      case TokenTypes.LESS_EQUAL:
        NumberOperands(expresion.Operator, left, right);
        return (int)left <= (int)right;
      case TokenTypes.BANG_EQUAL:
        return !IsEqual(left, right);
      case TokenTypes.EQUAL_EQUAL:
        return IsEqual(left, right);
    }
    throw new RuntimeError(expresion.Operator, "Unknown binary operator.");
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
  private void NumberOperand(Token Operator, object operand)
  {
    if (operand is int) return;
    throw new RuntimeError(Operator, "Operand must be a number.");
  }
  private void NumberOperands(Token Operator, object left, object right)
  {
    if (left is int && right is int) return;
    throw new RuntimeError(Operator, "Operands must be numbers.");
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
  private bool SameTypeValue(object left, object right)
  {
    if (left is bool && right is bool) return true;
    if (left is int && right is int) return true;
    return false;
  }
  private bool TryEvaluateAndConvert<T>(Expresion expr, Token contextToken, string argName, out T result, Func<T, bool>? validator = null, string? validationErrorMsg = null)
  {
    result = default(T)!;
    object evaluatedValue;
    try
    {
      evaluatedValue = evaluate(expr);
    }
    catch (RuntimeError err)
    {
      errors.Add(new Error(contextToken.line, $"Runtime Error evaluating argument '{argName}': {err.Message}"));
      return false;
    }
    int errorLine = contextToken.line;
    if (evaluatedValue is T typedValue) result = typedValue;
    else if (typeof(T) == typeof(int))
    {
      int intResult;
      if (evaluatedValue is int i) intResult = i;
      else if (evaluatedValue is string s && int.TryParse(s, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int parsedInt))
        intResult = parsedInt;
      else
      {
        errors.Add(new Error(errorLine, $"Runtime Error: Argument '{argName}' requires an integer value, but got type '{evaluatedValue?.GetType().Name ?? "null"}' (value: '{evaluatedValue}')."));
        return false;
      }
      result = (T)(object)intResult;
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
      errors.Add(new Error(errorLine, $"Runtime Error: Argument '{argName}' requires a '{typeof(T).Name}' value, but got type '{evaluatedValue?.GetType().Name ?? "null"}' (value: '{evaluatedValue}')."));
      return false;
    }
    if (validator != null && !validator(result))
    {
      string specificError = validationErrorMsg ?? $"Validation failed for argument '{argName}'";
      errors.Add(new Error(errorLine, $"Runtime Error: {specificError} (Actual value: '{result}')"));
      return false;
    }
    return true;
  }
  private Token? FindToken(Expresion? expr)
  {
    if (expr is Binary bin) return bin.Operator ?? FindToken(bin.Leftside);
    if (expr is GetActualX gx) return gx.KeywordToken;
    if (expr is GetActualY gy) return gy.KeywordToken;
    if (expr is IsBrushColor ibc) return ibc.KeywordToken ?? FindToken(ibc.color);
    if (expr is Unary un) return un.Operator ?? FindToken(un.Rightside);
    if (expr is Variable var) return var.name;
    if (expr is Assign ass) return ass.name ?? FindToken(ass.value);
    if (expr is Literal lit) return null;
    if (expr is Logical log) return log.Operator ?? FindToken(log.left);
    if (expr is Grouping grp) return FindToken(grp.expresion);
    return null;
  }
  private void RuntimeError(Token? token, string message)
  {
    int line = token?.line ?? -1;
    string errorMessage = $"[Runtime Error] ";
    if (line > 0) errorMessage += $"Line {line}: ";
    if (token != null) errorMessage += $"Near '{token.writing}': ";
    else errorMessage += "Near unknown token: ";
    errorMessage += message;
    errors.Add(new Error(line, errorMessage));
    throw new RuntimeError(token, message);
  }
}
public class RuntimeError : Exception
{
  public Token? token { get; }
  public RuntimeError(Token? token, string message) : base(message)
  {
    this.token = token;
  }
}