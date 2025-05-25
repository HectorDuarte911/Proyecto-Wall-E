namespace WALLE;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
/// <summary>///Errors in run time///</summary>
public class RuntimeError : Exception
{
  public Token? token { get; private set; }
  public RuntimeError(Token? token, string message) : base(message) => this.token = token;
}
/// <summary>///Ejecute the statements/// </summary>
public class Interpreter : Expresion.IVisitor<object>, Stmt.IVisitor<object?>
{
  /// <summary>///Action of change a line whith a GoTo statement///</summary>
  internal class JumpRequest
  {
    public int TargetIndex { get; private set; }
    public int OriginalGoToIndex { get; private set; }
    public JumpRequest(int targetIndex, int originalGoToIndex)
    {
      TargetIndex = targetIndex;
      OriginalGoToIndex = originalGoToIndex;
    }
  }
  /// <summary>///Error list in ejecution time/// </summary>
  public List<Error> errors { get; private set; }
  /// <summary>///Enviroment of the variable declared/// </summary>
  private Enviroment enviroment;
  /// <summary>///Colection of all the labels declarated and the index number in the colection of statements/// </summary>
  private Dictionary<string, int> labelMap = new Dictionary<string, int>();
  /// <summary>///Colection of all the labels declarated and their lines/// </summary>
  private Dictionary<string, int> labelLineNumbers = new Dictionary<string, int>();
  /// <summary>///Max of repetition of a loop/// </summary>
  private const int MAXSteps = 10000;
  /// <summary>///Number of ejecution doing/// </summary>
  private int executionSteps = 0;
  /// <summary>///Index of the actual statement /// </summary>
  private int executingStmtIndex = -1;
  public Interpreter(List<Error> errors)
  {
    this.errors = errors;
    enviroment = new Enviroment(errors);
  }
  /// <summary>///Principal method to ejecute all statements/// </summary>
  public void interpret(List<Stmt> statements)
  {
    labelMap.Clear();
    labelLineNumbers.Clear();
    executionSteps = 0;
    executingStmtIndex = -1;
    try
    {
      PreprocessLabels(statements);
      if (errors.Count > 0) return;
    }
    catch (RuntimeError){return;}
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
      result = execute(currentStmt);
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
  /// <summary>///Execute the actual statement action /// </summary>
  private object? execute(Stmt stmt) => stmt.accept(this);
  public object? VisitGoToStmt(GoTo stmt)
  {
    object? conditionResult = null;
    Token errorContext = stmt.label?.tag ?? GetCurrentToken(new List<Stmt> { stmt }, 0)!;
    try{conditionResult = evaluate(stmt.condition);}
    catch (RuntimeError){return null;}
    catch (Exception ex)
    {
      RuntimeError(errorContext, $"Unexpected internal error evaluating GoTo condition: {ex.Message}");
      return null;
    }
    if (IsTrue(conditionResult))
    {
      if (stmt.label == null || stmt.label.tag == null)
      {
        RuntimeError(errorContext, "Internal Error: GoTo statement has missing label information at runtime.");
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
  public object? VisitLabelStmt(Label stmt) => null;
  public object? VisitSpawnStmt(Spawn stmt)
  {
    Token fallbackToken = stmt.keyword;
    Token xContext = FindToken(stmt.x) ?? fallbackToken;
    Token yContext = FindToken(stmt.y) ?? fallbackToken;
    if (!TryEvaluateAndConvert<int>(stmt.x, xContext, "Spawn X", out int x) || !TryEvaluateAndConvert<int>(stmt.y, yContext, "Spawn Y", out int y)) return null;
    if (Canva.IsOutRange(x, y)) errors.Add(new Error(xContext.line, $"Runtime Error: Spawn position ({x}, {y}) is out of the canvas bounds."));
    else Walle.Spawn(x, y);
    return null;
  }
  public object? VisitSizeStmt(Size stmt)
  {
    Token fallbackToken = stmt.keyword;
    Token context = FindToken(stmt.number) ?? fallbackToken;
    if (!TryEvaluateAndConvert<int>(stmt.number, context, "Size", out int size)) return null;
    if (size <= 0) errors.Add(new Error(context.line, $"Runtime Error: Size ({size}) must be a positive integer."));
    else Walle.Size(size);
    return null;
  }
  public object? VisitColorStmt(Color stmt)
  {
    Token fallbackToken = stmt.keyword;
    Token context = FindToken(stmt.color) ?? fallbackToken;
    if (!TryEvaluateAndConvert<string>(stmt.color, context, "Color", out string colorValue)) return null;
    if (!IsValidColor(colorValue)) errors.Add(new Error(context.line, $"Runtime Error: '{colorValue}' is not a valid color name."));
    else Walle.Color(colorValue);
    return null;
  }
  public object? VisitDrawLineStmt(DrawLine stmt)
  {
    Token fallbackToken = stmt.keyword;
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
    Token fallbackToken = stmt.keyword;
    Token dirXContext = FindToken(stmt.dirx) ?? fallbackToken;
    Token dirYContext = FindToken(stmt.diry) ?? fallbackToken;
    Token RadiusContext = FindToken(stmt.Radius) ?? fallbackToken;
    if (!TryEvaluateAndConvert<int>(stmt.dirx, dirXContext, "DrawCircle center offset X", out int dirX, IsValidDir, "Direction must be -1, 0, or 1") ||
        !TryEvaluateAndConvert<int>(stmt.diry, dirYContext, "DrawCircle center offset Y", out int dirY, IsValidDir, "Direction must be -1, 0, or 1") ||
        !TryEvaluateAndConvert<int>(stmt.Radius, RadiusContext, "DrawCircle Radius", out int Radius, r => r > 0, "Radius must be positive")) return null;
    if (Radius <= 0)
    {
      errors.Add(new Error(RadiusContext.line, $"Runtime Error: DrawCircle Radius ({Radius}) must be positive."));
      return null;
    }
    Walle.DrawCircle(dirX, dirY, Radius);
    return null;
  }
  public object? VisitDrawRectangleStmt(DrawRectangle stmt)
  {
    Token fallbackToken = stmt.keyword;
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
  public object VisitExpressionStmt(Expression stmt)
  {
    evaluate(stmt.expresion);
    return null!;
  }
  /// <summary>///Evaluete the actual expresion /// </summary>
  private object evaluate(Expresion expresion) => expresion.accept(this);
  public object VisitGetActualX(GetActualX expresion) => Walle.GetActualX();
  public object VisitGetActualY(GetActualY expresion) => Walle.GetActualY();
  public object VisitIsBrushColor(IsBrushColor expresion)
  {
    Token fallbackToken = expresion.keyword;
    Token context = FindToken(expresion.color) ?? fallbackToken;
    if (!TryEvaluateAndConvert<string>(expresion.color, context, "IsBrushColor color", out string colorValue, IsValidColor, $"Value is not a valid color name")) return false;
    return Walle.IsBrushColor(colorValue);
  }
  public object VisitIsBrushSize(IsBrushSize expresion)
  {
    Token fallbackToken = expresion.keyword;
    Token context = FindToken(expresion.size) ?? fallbackToken;
    if (!TryEvaluateAndConvert<int>(expresion.size, context, "IsBrushSize size", out int sizeValue, s => s > 0, "Size must be a positive integer")) return false;
    return Walle.IsBrushSize(sizeValue);
  }
  public object VisitGetColorCount(GetColorCount expresion)
  {
    Token fallbackToken = expresion.keyword;
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
    Token fallbackToken = expresion.keyword;
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
  public object VisitGetCanvasSize(GetCanvasSize expresion) => Canva.GetCanvasSize();
  public object visitAssign(Assign expresion)
  {
    object value = evaluate(expresion.value!);
    enviroment.assign(expresion.name!, value);
    return value;
  }
  public object visitLogical(Logical expresion)
  {
    object left = evaluate(expresion.left);
    if (expresion.Operator.type == TokenTypes.OR) if (IsTrue(left)) return left;
      else if (!IsTrue(left)) return left;
    return evaluate(expresion.right);
  }
  public object visitLiteral(Literal expresion)
  {
    if (expresion.Value is bool b) return b; if (expresion.Value is int i) return i;
    if (expresion.Value is string s)
    {
      if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intValue)) return intValue;
      return s;
    }
    if (expresion.Value == null) return null!;
    return expresion.Value;
  }
  public object visitGrouping(Grouping expresion) => evaluate(expresion.expresion!);
  public object visitUnary(Unary expresion)
  {
    object right = evaluate(expresion.rightside!);
    switch (expresion.Operator!.type)
    {
      case TokenTypes.MINUS:
        NumberOperand(expresion.Operator, right);
        try { return checked(-(int)right); }
        catch (OverflowException) { throw new RuntimeError(expresion.Operator, $"Unary minus operation resulted in arithmetic overflow for value {right}."); }
      case TokenTypes.BANG:
        return !IsTrue(right);
    }
    throw new RuntimeError(expresion.Operator, $"Unknown or unsupported unary operator '{expresion.Operator.writing}'.");
  }
  public object visitVariable(Variable expresion) => enviroment.get(expresion.name!);
  public object visitBinary(Binary expresion)
  {
    object left = evaluate(expresion.leftside!);
    object right = evaluate(expresion.rightside!);
    var opToken = expresion.Operator!;
    switch (opToken.type)
    {
      case TokenTypes.PLUS:
      case TokenTypes.MINUS:
      case TokenTypes.DIVIDE:
      case TokenTypes.PRODUCT:
      case TokenTypes.MODUL:
        NumberOperands(opToken, left, right);
        int leftInt = (int)left;
        int rightInt = (int)right;
        try
        {
          switch (opToken.type)
          {
            case TokenTypes.PLUS: return checked(leftInt + rightInt);
            case TokenTypes.MINUS: return checked(leftInt - rightInt);
            case TokenTypes.PRODUCT: return checked(leftInt * rightInt);
            case TokenTypes.DIVIDE:if (rightInt == 0) throw new RuntimeError(opToken, "Division by zero.");return leftInt / rightInt;
            case TokenTypes.MODUL:if (rightInt == 0) throw new RuntimeError(opToken, "Modulo by zero.");return leftInt % rightInt;
            default:throw new RuntimeError(opToken, $"Internal logic error: Unhandled arithmetic operator '{opToken.writing}' in inner switch.");
          }
        }
        catch (OverflowException){throw new RuntimeError(opToken, $"Arithmetic operation '{opToken.writing}' resulted in overflow.");}
      case TokenTypes.POW:
        NumberOperands(opToken, left, right);
        int baseVal = (int)left;
        int expVal = (int)right;
        if (expVal < 0) throw new RuntimeError(opToken, "Negative exponents are not supported for integer power operations.");
        if (expVal == 0) return 1;
        if (baseVal == 0) return 0;
        if (baseVal == 1) return 1;
        if (baseVal == -1) return (expVal % 2 == 0) ? 1 : -1;
        long result = 1;
        try
        {
          for (int i = 0; i < expVal; i++) result = checked(result * baseVal);
          if (result > int.MaxValue || result < int.MinValue) throw new OverflowException("Final result of power operation exceeds Int32 range.");
          return (int)result;
        }
      catch (OverflowException) {throw new RuntimeError(opToken, $"Arithmetic operation '{baseVal} ** {expVal}' resulted in overflow.");}
      case TokenTypes.GREATER:
      case TokenTypes.GREATER_EQUAL:
      case TokenTypes.LESS:
      case TokenTypes.LESS_EQUAL:
        NumberOperands(opToken, left, right);
        int compLeft = (int)left;
        int compRight = (int)right;
        switch (opToken.type)
        {
          case TokenTypes.GREATER: return compLeft > compRight;
          case TokenTypes.GREATER_EQUAL: return compLeft >= compRight;
          case TokenTypes.LESS: return compLeft < compRight;
          case TokenTypes.LESS_EQUAL: return compLeft <= compRight;
          default:throw new RuntimeError(opToken, $"Internal logic error: Unhandled comparison operator '{opToken.writing}' in inner switch.");
        }
      case TokenTypes.BANG_EQUAL: return !IsEqual(left, right);
      case TokenTypes.EQUAL_EQUAL:return IsEqual(left, right);
      default:throw new RuntimeError(opToken, $"Unknown or unsupported binary operator '{opToken.writing}'.");
    }
  }
  /// <summary>///Mark all the valid labels in the beging of the ejecution/// </summary>
  private void PreprocessLabels(List<Stmt> statements)
  {
    labelMap.Clear();
    labelLineNumbers.Clear();
    for (int i = 0; i < statements.Count; i++)
    {
      if (statements[i] is Label labelStmt)
      {
        string labelName = labelStmt.tag.writing;
        int currentLine = labelStmt.tag.line;
        if (labelMap.ContainsKey(labelName))
        {
          int originalLine = labelLineNumbers.ContainsKey(labelName) ? labelLineNumbers[labelName] : -1;
          RuntimeError(labelStmt.tag, $"Duplicate label '{labelName}' found. Original definition was on line {originalLine}.");
        }
        else
        {
          labelMap.Add(labelName, i + 1);
          labelLineNumbers.Add(labelName, currentLine);
        }
      }
    }
  }
  /// <summary>///Get the keyword in form of token of the statement in the index or FindToken in case of an expresion/// </summary>
  private Token? GetCurrentToken(List<Stmt> statements, int index)
  {
    if (index < 0 || index >= statements.Count)
    {
      if (statements.Count > 0) return FindTokenFromAnywhere(statements.Last());
      return null;
    }
    Stmt stmt = statements[index];
    if (stmt is GoTo gt && gt.label != null) return gt.label.tag;
    if (stmt is Label lbl) return lbl.tag;
    if (stmt is Spawn sp) return sp.keyword;
    if (stmt is Size sz) return sz.keyword;
    if (stmt is Color co) return co.keyword;
    if (stmt is DrawLine dl) return dl.keyword;
    if (stmt is DrawCircle dc) return dc.keyword;
    if (stmt is DrawRectangle dr) return dr.keyword;
    if (stmt is Fill fi) return fi.keyword;
    if (stmt is Expression exprStmt)
    {
      Token? exprToken = FindToken(exprStmt.expresion);
      if (exprToken != null) return exprToken;
      if (exprStmt.expresion is Assign assign) return assign.name;
    }
    return FindTokenFromAnywhere(stmt) ?? FindToken(ExtractExpression(stmt));
  }
  private Expresion? ExtractExpression(Stmt? stmt)
  {
    if (stmt is Expression exprStmt) return exprStmt.expresion;
    return null;
  }
 /// <summary>///Determinate if two objects are equals/// </summary>
  private bool IsEqual(object? left, object? right)
  {
    if (left == null && right == null) return true;
    if (left == null || right == null) return false;
    if (left.GetType() != right.GetType()) return false;
    return left.Equals(right);
  }
  /// <summary>///Determinate if an object have a true or false values /// </summary>
  private bool IsTrue(object? ObjectConcrete)
  {
    if (ObjectConcrete == null) return false;
    if (ObjectConcrete is bool booleanValue) return booleanValue;
    if (ObjectConcrete is int intValue) return intValue != 0;
    if (ObjectConcrete is string stringValue) return !string.IsNullOrEmpty(stringValue);
    return true;
  }
  /// <summary>///Check if a unary operand is a valid number /// </summary>
  private void NumberOperand(Token Operator, object? operand)
  {
    if (operand is int) return;
    string typeName = operand?.GetType().Name ?? "null";
    throw new RuntimeError(Operator, $"Operand must be a number, but got {typeName} ('{Stringify(operand)}').");
  }
  /// <summary>///Check if the binarys operands are valid numbers/// </summary>
  private void NumberOperands(Token Operator, object? left, object? right)
  {
    if (left is int && right is int) return;
    string leftTypeName = left?.GetType().Name ?? "null";
    string rightTypeName = right?.GetType().Name ?? "null";
    throw new RuntimeError(Operator, $"Operands must both be numbers for operator '{Operator.writing}', but got {leftTypeName} ('{Stringify(left)}') and {rightTypeName} ('{Stringify(right)}').");
  }
  /// <summary>///Determinate if the string value is covertible/// </summary>
  private string Stringify(object? obj)
  {
    if (obj == null) return "nil";
    if (obj is bool b) return b ? "true" : "false";
    if (obj is IFormattable formattable) return formattable.ToString(null, CultureInfo.InvariantCulture);
    return obj.ToString() ?? "nil";
  }
  /// <summary>///Determinate if the string is a valid color/// </summary>
  private bool IsValidColor(string? color)
  {
    if (string.IsNullOrWhiteSpace(color)) return false;
    string normalizedColor = color.Trim();
    var validColors = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Red", "Blue", "Green", "Yellow", "Orange", "Purple", "Black", "White", "Transparent" };
    return validColors.Contains(normalizedColor);
  }
  /// <summary>///Determinate if the number is a valid direction/// </summary>
  private bool IsValidDir(int dir) => dir <= 1 && dir >= -1;
  /// <summary>///Convert an expresion in his literal value/// </summary>
  private bool TryEvaluateAndConvert<T>(Expresion expr, Token contextToken, string argName, out T result, Func<T, bool>? validator = null, string? validationErrorMsg = null)
  {
    result = default!;
    object evaluatedValue;
    int errorLine = contextToken.line;
    try { evaluatedValue = evaluate(expr); }
    catch (RuntimeError err)
    {
      errors.Add(new Error(errorLine, $"Runtime Error evaluating argument '{argName}': {err.Message}"));
      return false;
    }
    catch (Exception ex)
    {
      errors.Add(new Error(errorLine, $"Unexpected internal error evaluating argument '{argName}': {ex.Message}"));
      return false;
    }
    try
    {
      if (evaluatedValue is T typedValue) result = typedValue;
      else if (typeof(T) == typeof(int))
      {
        int intResult;
        if (evaluatedValue is int i) intResult = i;
        else if (evaluatedValue is string s && int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedInt))
          intResult = parsedInt;
        else
        {
          errors.Add(new Error(errorLine, $"Runtime Error: Argument '{argName}' requires an integer value, but got type '{evaluatedValue?.GetType().Name ?? "null"}' (value: '{Stringify(evaluatedValue)}')."));
          return false;
        }
        result = (T)(object)intResult;
      }
      else if (typeof(T) == typeof(string)) result = (T)(object)Stringify(evaluatedValue);
      else if (typeof(T) == typeof(bool)) result = (T)(object)IsTrue(evaluatedValue);
      else
      {
        errors.Add(new Error(errorLine, $"Runtime Error: Argument '{argName}' requires a '{typeof(T).Name}' value, but got type '{evaluatedValue?.GetType().Name ?? "null"}' (value: '{Stringify(evaluatedValue)}')."));
        return false;
      }
      if (validator != null && !validator(result))
      {
        string specificError = validationErrorMsg ?? $"Validation failed for argument '{argName}'";
        errors.Add(new Error(errorLine, $"Runtime Error: {specificError} (Actual value: '{Stringify(result)}')"));
        return false;
      }
      return true;
    }
    catch (Exception ex)
    {
      errors.Add(new Error(errorLine, $"Unexpected internal error converting or validating argument '{argName}': {ex.Message}"));
      return false;
    }
  }
  /// <summary>///Return the token that correspond the expresion/// </summary>
  private Token? FindToken(Expresion? expr)
  {
    if (expr == null) return null;
    if (expr is Binary bin) return bin.Operator ?? FindToken(bin.leftside);
    if (expr is Unary un) return un.Operator ?? FindToken(un.rightside);
    if (expr is Logical log) return log.Operator ?? FindToken(log.left);
    if (expr is Assign ass) return ass.name ?? FindToken(ass.value);
    if (expr is Variable var) return var.name;
    if (expr is GetActualX gx) return gx.keyword;
    if (expr is GetActualY gy) return gy.keyword;
    if (expr is IsBrushColor ibc) return ibc.keyword ?? FindToken(ibc.color);
    if (expr is IsBrushSize ibs) return ibs.keyword ?? FindToken(ibs.size);
    if (expr is GetColorCount gcc) return gcc.keyword ?? FindToken(gcc.color);
    if (expr is IsCanvasColor icc) return icc.keyword ?? FindToken(icc.color);
    if (expr is GetCanvasSize gcs) return gcs.keyword;
    if (expr is Grouping grp) return FindToken(grp.expresion);
    if (expr is Literal) return null;
    return null;
  }
  /// <summary>///Return the token that correspond to a statement/// </summary>
  private Token? FindTokenFromAnywhere(Stmt? stmt)
  {
    if (stmt == null) return null;
    if (stmt is GoTo gt) return FindToken(gt.condition) ?? gt.label?.tag;
    if (stmt is Label lbl) return lbl.tag;
    if (stmt is Spawn sp) return sp.keyword ?? FindToken(sp.x) ?? FindToken(sp.y);
    if (stmt is Size sz) return sz.keyword ?? FindToken(sz.number);
    if (stmt is Color co) return co.keyword ?? FindToken(co.color);
    if (stmt is DrawLine dl) return dl.keyword ?? FindToken(dl.dirx) ?? FindToken(dl.diry) ?? FindToken(dl.distance);
    if (stmt is DrawCircle dc) return dc.keyword ?? FindToken(dc.dirx) ?? FindToken(dc.diry) ?? FindToken(dc.Radius);
    if (stmt is DrawRectangle dr) return dr.keyword ?? FindToken(dr.dirx) ?? FindToken(dr.diry) ?? FindToken(dr.distance) ?? FindToken(dr.width) ?? FindToken(dr.height);
    if (stmt is Fill fi) return fi.keyword;
    if (stmt is Expression exprStmt) return FindToken(exprStmt.expresion);
    return null;
  }  
  /// <summary>///Declarate and throw a Runtime Error/// </summary>
  private void RuntimeError(Token? token, string message)
  {
    int line = token?.line ?? -1;
    if (line <= 0 && errors.Any()) line = errors.Last().Location;
    if (line <= 0) line = -1;
    string errorMessage = $"[Runtime Error] ";
    if (line > 0) errorMessage += $"Line {line}: ";
    if (token != null) errorMessage += $"Near '{token.writing}': ";
    errorMessage += message;
    errors.Add(new Error(line, errorMessage));
    throw new RuntimeError(token, message);
  }
}
