
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
    string? xstr = stmt.x.Value as string;
    string? ystr = stmt.y.Value as string;
    int x = Convert.ToInt32(xstr);
    int y = Convert.ToInt32(ystr);
    if (Canva.IsOutRange(x, y)) errors.Add(new Error(1, "The position is out the bounds of the canvas"));
    else Walle.Spawn(x, y);
    return new Void();
  }
  public Void VisitSizeStmt(Size stmt)
  {
    string? sizestring = stmt.number.Value as string;
    int size = Convert.ToInt32(sizestring);
    if (size <= 0) errors.Add(new Error(1, "The size of the pincel can't be negative"));
    else Walle.Size(size);
    return new Void();
  }
  public Void VisitColorStmt(Color stmt)
  {
    string? Color = stmt.color.Value as string;
    if (!IsValidColor(Color!)) errors.Add(new Error(1, "The word introducted is not a valid color"));
    else Walle.Color(Color!);
    return new Void();
  }
  public Void VisitDrawLineStmt(DrawLine stmt)
  {
    string? dirxstring = NegativeComprove(stmt.dirx);
    int dirX = Convert.ToInt32(dirxstring);
    string? dirystring = NegativeComprove(stmt.diry);
    int dirY = Convert.ToInt32(dirystring);
    if (!IsValidDir(dirX) || !IsValidDir(dirY)) errors.Add(new Error(1, "The direction introduce is not a valid one , a direction most be one of the number 1,-1 or 0"));
    else
    {
      string? distancestring = stmt.distance.Value as string;
      int Distance = Convert.ToInt32(distancestring);
      Walle.DrawLine(dirX, dirY, Distance);
    }
    return new Void();
  }
  public Void VisitDrawCircleStmt(DrawCircle stmt)
  {
    string? dirxstring = NegativeComprove(stmt.dirx);
    int dirX = Convert.ToInt32(dirxstring);
    string? dirystring = NegativeComprove(stmt.diry);
    int dirY = Convert.ToInt32(dirystring);
    if (!IsValidDir(dirX) || !IsValidDir(dirY)) errors.Add(new Error(1, "The direction introduce is not a valid one , a direction most be one of the number 1,-1 or 0"));
    else
    {
      string? radiusstring = stmt.radius.Value as string;
      int Radius = Convert.ToInt32(radiusstring);
      Walle.DrawCircle(dirX, dirY, Radius);
    }
    return new Void();
  }
  public Void VisitDrawRectangleStmt(DrawRectangle stmt)
  {
    string? dirxstring = NegativeComprove(stmt.dirx);
    int dirX = Convert.ToInt32(dirxstring);
    string? dirystring = NegativeComprove(stmt.diry);
    int dirY = Convert.ToInt32(dirystring);
    if (!IsValidDir(dirX) || !IsValidDir(dirY)) errors.Add(new Error(1, "The direction introduce is not a valid one , a direction most be one of the number 1,-1 or 0"));
    else
    {
      string? distancestring = stmt.distance.Value as string;
      int Distance = Convert.ToInt32(distancestring);
      string? widthstring = stmt.width.Value as string;
      int Width = Convert.ToInt32(widthstring);
      string? heightstring = stmt.height.Value as string;
      int Height = Convert.ToInt32(heightstring);
      Walle.DrawRectangle(dirX, dirY, Distance, Width, Height);
    }
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

    string? color = expresion.color.Value as string;
    if (IsValidColor(color!)) return Walle.IsBrushColor(color);
    else
    {
      errors.Add(new Error(1, "The word introducted is not a valid color"));
      return false;
    }
  }
  public object VisitIsBrushSize(IsBrushSize expresion)
  {
    string? sizeStr = expresion.size.Value as string;
    int size = Convert.ToInt32(sizeStr);
    return Walle.IsBrushSize(size);
  }
  public object VisitGetColorCount(GetColorCount expresion)
  {
    string? color = expresion.color.Value as string;
    if (!IsValidColor(color!))
    {
      errors.Add(new Error(1, "The word introducted is not a valid color"));
      return 0;
    }
    else
    {
      string? x1Str = expresion.x1.Value as string;
      int x1 = Convert.ToInt32(x1Str);
      string? y1Str = expresion.y1.Value as string;
      int y1 = Convert.ToInt32(y1Str);
      string? x2Str = expresion.x2.Value as string;
      int x2 = Convert.ToInt32(x2Str);
      string? y2Str = expresion.y2.Value as string;
      int y2 = Convert.ToInt32(y2Str);
      if (Canva.IsOutRange(x1, y1) || Canva.IsOutRange(x2, y2))
      {
        errors.Add(new Error(1, "The position indroduce is out of the range of the canvas"));
        return 0;
      }
      return Canva.GetColorCount(color, x1, y1, x2, y2);
    }
  }
  public object VisitIsCanvasColor(IsCanvasColor expresion)
  {
    string? color = expresion.color.Value as string;
    if (!IsValidColor(color!))
    {
      errors.Add(new Error(1, "The word introducted is not a valid color"));
      return false;
    }
    else
    {
      string? verticalStr = expresion.vertical.Value as string;
      int vertical = Convert.ToInt32(verticalStr);
      string? horizontalStr = expresion.horizontal.Value as string;
      int horizontal = Convert.ToInt32(horizontalStr);
      if (Canva.IsOutRange(Walle.GetActualX() + vertical, Walle.GetActualY() + horizontal))
      {
        errors.Add(new Error(1, "The position indroduce is out of the range of the canvas"));
        return false;
      }
      return Canva.IsCanvasColor(color, vertical, horizontal);
    }
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
}
