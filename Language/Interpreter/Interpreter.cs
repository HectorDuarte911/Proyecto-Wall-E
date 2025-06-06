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
/// <summary>/// Error throw in the argument evaluation proces/// </summary>
class ArgumentEvaluationException : RuntimeError
{
  public ArgumentEvaluationException(Token token, string message) : base(token, message) { }
}
// <summary>///Ejecute the statements/// </summary>
public class Interpreter : Expresion.IVisitor<object>, Stmt.IVisitor<object?>
{
  /// <summary>///All the types of binary operation /// </summary>
  private readonly Dictionary<TokenTypes, IBinaryOperation> BinaryOperations;
  /// <summary>///Action of change a line whith a GoTo statement///</summary>
  internal class JumpException : Exception
  {
    public int TargetIndex { get; }
    public JumpException(int targetIndex) : base(null, null) => TargetIndex = targetIndex;
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
    BinaryOperations = new Dictionary<TokenTypes, IBinaryOperation>{
            { TokenTypes.PLUS, new AddOperation() },
            { TokenTypes.MINUS, new SubtractOperation() }, // Suponiendo que creas esta clase
            { TokenTypes.PRODUCT, new ProductOperation() },
            { TokenTypes.DIVIDE, new DivideOperation() },
            { TokenTypes.MODUL, new ModuloOperation() },
            { TokenTypes.POW, new PowerOperation() },
            { TokenTypes.GREATER, new GreaterThanOperation() },
            { TokenTypes.GREATER_EQUAL, new GreaterEqualOperation() },
            { TokenTypes.LESS, new LessThanOperation() },
            { TokenTypes.LESS_EQUAL, new LessEqualOperation() }
        };
  }
  /// <summary>///Principal method to ejecute all statements/// </summary>
  public void interpret(List<Stmt> statements)
  {
    try
    {
      PreprocessLabels(statements);
      if (errors.Count > 0) return;
      int currentStatementIndex = 0;
      while (currentStatementIndex < statements.Count)
      {
        executionSteps++;
        if (executionSteps > MAXSteps) throw new RuntimeError(GetCurrentToken(statements, currentStatementIndex), "Maximum execution steps exceeded. Possible infinite loop detected.");
        executingStmtIndex = currentStatementIndex;
        try
        {
          execute(statements[executingStmtIndex]);
          currentStatementIndex++;
        }
        catch (JumpException jump) { currentStatementIndex = jump.TargetIndex; }
      }
    }
    catch (RuntimeError err) { ReportRuntimeError(err); }
  }
  /// <summary>/// Report an exist RuntimeError/// </summary>
  private void ReportRuntimeError(RuntimeError err)
  {
    int line = err.token?.line ?? (executingStmtIndex >= 0 ? GetCurrentToken(null!, executingStmtIndex)?.line ?? -1 : -1);
    string where = err.token?.writing ?? "at runtime";
    string errorMessage = $"[Runtime Error] Line {line} near '{where}': {err.Message}";
    if (!errors.Any(e => e.Location == line && e.Argument.Contains(err.Message))) errors.Add(new Error(line, errorMessage));
  }
  private object? execute(Stmt stmt) => stmt.accept(this);
  public object? VisitGoToStmt(GoTo stmt)
  {
    object conditionResult = evaluate(stmt.condition);
    if (IsTrue(conditionResult))
    {
      if (stmt.label == null || stmt.label.tag == null) throw new RuntimeError(GetCurrentToken(new List<Stmt> { stmt }, 0), "Internal Error: GoTo statement has missing label information.");
      string labelName = stmt.label.tag.writing;
      if (labelMap.TryGetValue(labelName, out int targetIndex)) throw new JumpException(targetIndex);
      else throw new RuntimeError(stmt.label.tag, $"Undefined label '{labelName}' referenced in GoTo.");
    }
    return null;
  }
  public object? VisitLabelStmt(Label stmt) => null;
  public object? VisitSpawnStmt(Spawn stmt)
  {
    try
    {
      Token fallbackToken = stmt.keyword;
      int x = EvaluateAndConvert<int>(stmt.x, FindToken(stmt.x) ?? fallbackToken, "Spawn X");
      int y = EvaluateAndConvert<int>(stmt.y, FindToken(stmt.y) ?? fallbackToken, "Spawn Y"); if (Canva.IsOutRange(x, y)) throw new RuntimeError(stmt.keyword, $"La posición de Spawn ({x}, {y}) está fuera de los límites del lienzo.");
      Walle.Spawn(x, y);
    }
    catch (RuntimeError err) { errors.Add(new Error(err.token?.line ?? -1, err.Message)); }
    return null;
  }
  public object? VisitSizeStmt(Size stmt)
  {
    try
    {
      Token context = FindToken(stmt.number) ?? stmt.keyword;
      int size = EvaluateAndConvert<int>(stmt.number, context, "Size", s => s > 0, "The size must be a positive inter");
      Walle.Size(size);
    }
    catch (RuntimeError err) { errors.Add(new Error(err.token?.line ?? -1, err.Message)); }
    return null;
  }
  public object? VisitColorStmt(Color stmt)
  {
    try
    {
      Token context = FindToken(stmt.color) ?? stmt.keyword;
      string colorValue = EvaluateAndConvert<string>(stmt.color, context, "Color", IsValidColor, "Is not a valid color name");
      Walle.Color(colorValue);
    }
    catch (RuntimeError err) { errors.Add(new Error(err.token?.line ?? -1, err.Message)); }
    return null;
  }
  public object? VisitDrawLineStmt(DrawLine stmt)
  {
    try
    {
      Token fallbackToken = stmt.keyword;
      int dirX = EvaluateAndConvert<int>(stmt.dirx, FindToken(stmt.dirx) ?? fallbackToken, "DrawLine dirX", IsValidDir, "Direction must be -1, 0, or 1");
      int dirY = EvaluateAndConvert<int>(stmt.diry, FindToken(stmt.diry) ?? fallbackToken, "DrawLine dirY", IsValidDir, "Direction must be -1, 0, or 1");
      int distance = EvaluateAndConvert<int>(stmt.distance, FindToken(stmt.distance) ?? fallbackToken, "DrawLine distance");
      Walle.DrawLine(dirX, dirY, distance);
    }
    catch (RuntimeError err) { errors.Add(new Error(err.token?.line ?? -1, err.Message)); }
    return null;
  }
  public object? VisitDrawCircleStmt(DrawCircle stmt)
  {
    try
    {
      Token fallbackToken = stmt.keyword;
      int dirX = EvaluateAndConvert<int>(stmt.dirx, FindToken(stmt.dirx) ?? fallbackToken, "DrawCircle center offset X", IsValidDir, "Direction must be -1, 0, o 1");
      int dirY = EvaluateAndConvert<int>(stmt.diry, FindToken(stmt.diry) ?? fallbackToken, "DrawCircle center offset Y", IsValidDir, "Direction must be -1, 0, o 1");
      int radius = EvaluateAndConvert<int>(stmt.Radius, FindToken(stmt.Radius) ?? fallbackToken, "DrawCircle Radius", r => r > 0, "The radius must be a positive inter");
      Walle.DrawCircle(dirX, dirY, radius);
    }
    catch (RuntimeError err) { errors.Add(new Error(err.token?.line ?? -1, err.Message)); }
    return null;
  }
  public object? VisitDrawRectangleStmt(DrawRectangle stmt)
  {
    try
    {
      Token fallbackToken = stmt.keyword;
      int dirX = EvaluateAndConvert<int>(stmt.dirx, FindToken(stmt.dirx) ?? fallbackToken, "DrawRectangle center offset X", IsValidDir, "Direction must be -1, 0, o 1");
      int dirY = EvaluateAndConvert<int>(stmt.diry, FindToken(stmt.diry) ?? fallbackToken, "DrawRectangle center offset Y", IsValidDir, "Direction must be -1, 0, o 1");
      int distance = EvaluateAndConvert<int>(stmt.distance, FindToken(stmt.distance) ?? fallbackToken, "DrawRectangle distance", r => r > 0, "The sitance to a center must be a positive inter");
      int width = EvaluateAndConvert<int>(stmt.width, FindToken(stmt.width) ?? fallbackToken, "DrawRectangle width", r => r > 0, "The width must be a positive inter");
      int height = EvaluateAndConvert<int>(stmt.width, FindToken(stmt.width) ?? fallbackToken, "DrawRectangle height", r => r > 0, "The height must be a positive inter");
      Walle.DrawRectangle(dirX, dirY, distance, width, height);
    }
    catch (RuntimeError err) { errors.Add(new Error(err.token?.line ?? -1, err.Message)); }
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
    Token context = FindToken(expresion.color) ?? expresion.keyword;
    string colorValue = EvaluateAndConvert<string>(expresion.color, context, "IsBrushColor color", IsValidColor, "The value is not a valid color name");
    return Walle.IsBrushColor(colorValue);
  }
  public object VisitIsBrushSize(IsBrushSize expresion)
  {
    Token context = FindToken(expresion.size) ?? expresion.keyword;
    int sizeValue = EvaluateAndConvert<int>(expresion.size, context, "IsBrushSize size", s => s > 0, "The size must be a positive inter");
    return Walle.IsBrushSize(sizeValue);
  }
  public object VisitGetColorCount(GetColorCount expresion)
  {
    Token fallbackToken = expresion.keyword;
    string colorValue = EvaluateAndConvert<string>(expresion.color, FindToken(expresion.color) ?? fallbackToken, "GetColorCount color", IsValidColor, "Invalid color name");
    int x1 = EvaluateAndConvert<int>(expresion.x1, FindToken(expresion.x1) ?? fallbackToken, "GetColorCount x1");
    int y1 = EvaluateAndConvert<int>(expresion.y1, FindToken(expresion.y1) ?? fallbackToken, "GetColorCount y1");
    int x2 = EvaluateAndConvert<int>(expresion.x2, FindToken(expresion.x2) ?? fallbackToken, "GetColorCount x2");
    int y2 = EvaluateAndConvert<int>(expresion.y2, FindToken(expresion.y2) ?? fallbackToken, "GetColorCount y2");
    if (Canva.IsOutRange(x1, y1) || Canva.IsOutRange(x2, y2)) throw new RuntimeError(fallbackToken, $"The coordenates for GetColorCount ({x1},{y1} or {x2},{y2}) are out of the size of the canvas.");
    return Canva.GetColorCount(colorValue, x1, y1, x2, y2);
  }
  public object VisitIsCanvasColor(IsCanvasColor expresion)
  {
    Token fallbackToken = expresion.keyword;
    string colorValue = EvaluateAndConvert<string>(expresion.color, FindToken(expresion.color) ?? fallbackToken, "IsCanvasColor color", IsValidColor, "Invalid color name");
    int vertical = EvaluateAndConvert<int>(expresion.vertical, FindToken(expresion.vertical) ?? fallbackToken, "IsCanvasColor vertical offset");
    int horizontal = EvaluateAndConvert<int>(expresion.horizontal, FindToken(expresion.horizontal) ?? fallbackToken, "IsCanvasColor horizontal offset");
    int targetX = Walle.GetActualX() + vertical;
    int targetY = Walle.GetActualY() + horizontal;
    if (Canva.IsOutRange(targetX, targetY)) throw new RuntimeError(fallbackToken, $"The calculate position to IsCanvasColor ({targetX}, {targetY}) is out the canvas limits.");
    return Canva.IsCanvasColor(colorValue, vertical, horizontal);
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
  public object visitLiteral(Literal expresion) => expresion.Value;
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
    if (opToken.type == TokenTypes.BANG_EQUAL) return !IsEqual(left, right);
    if (opToken.type == TokenTypes.EQUAL_EQUAL) return IsEqual(left, right);
    if (BinaryOperations.TryGetValue(opToken.type, out var operation))
    {
      NumberOperands(opToken, left, right);
      try { return operation.Execute(opToken, left, right); }
      catch (RuntimeError) { throw; }
      catch (Exception ex) { throw new RuntimeError(opToken, $"Internal error during operation '{opToken.writing}': {ex.Message}"); }
    }
    throw new RuntimeError(opToken, $"Unknown or unsupported binary operator '{opToken.writing}'.");
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
  private T EvaluateAndConvert<T>(Expresion expr, Token contextToken, string argName, Func<T, bool>? validator = null, string? validationErrorMsg = null)
  {
    object evaluatedValue;
    int errorLine = contextToken.line;
    try { evaluatedValue = evaluate(expr); }
    catch (RuntimeError err) { throw new RuntimeError(err.token ?? contextToken, $"Error evaluando el argumento '{argName}': {err.Message}"); }
    catch (Exception ex) { throw new RuntimeError(contextToken, $"Error interno inesperado evaluando el argumento '{argName}': {ex.Message}"); }
    T result;
    try
    {
      if (evaluatedValue is T typedValue) result = typedValue;
      else if (typeof(T) == typeof(int))
      {
        int intResult;
        if (evaluatedValue is int i) intResult = i;
        else throw new RuntimeError(contextToken, $"El argumento '{argName}' requiere un valor entero, pero se obtuvo el tipo '{evaluatedValue?.GetType().Name ?? "null"}' (valor: '{Stringify(evaluatedValue)}').");
        result = (T)(object)intResult;
      }
      else if (typeof(T) == typeof(string)) result = (T)(object)Stringify(evaluatedValue);
      else if (typeof(T) == typeof(bool)) result = (T)(object)IsTrue(evaluatedValue);
      else throw new RuntimeError(contextToken, $"El argumento '{argName}' requiere un valor de tipo '{typeof(T).Name}', pero se obtuvo '{evaluatedValue?.GetType().Name ?? "null"}' (valor: '{Stringify(evaluatedValue)}').");
    }
    catch (Exception ex) { throw new RuntimeError(contextToken, $"Error interno inesperado convirtiendo el argumento '{argName}': {ex.Message}"); }
    if (validator != null && !validator(result))
    {
      string specificError = validationErrorMsg ?? $"La validación falló para el argumento '{argName}'";
      throw new RuntimeError(contextToken, $"{specificError} (valor actual: '{Stringify(result)}')");
    }
    return result;
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
