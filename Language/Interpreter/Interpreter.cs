
namespace WALLE;
public class Void{}
public class Interpreter : Expresion.IVisitor<object>,Stmt.IVisitor<Void>
{
public List<Error> errors {get ; private set;}
public Enviroment enviroment;
public Interpreter(List<Error> errors)
{
  this.errors = errors;
  enviroment = new Enviroment(errors);
} 
public void interpret(List<Stmt>statements, int begin)
{
    bool validline= false;
    if(IsVoid(statements))
    {
    if(statements.Count == 1)validline = true;
    }
    else
    {
    foreach (Stmt statement in statements)
    {
     if(statement is Label){validline = true;break;}
     if(statement is Expression expresion){if(expresion.expresion is Assign){validline = true;break;}}   
    }
    }
    if(!validline){errors.Add(new Error(1,"Expect a valid statement"));}
    else{
    bool flag =false;
    while(begin < statements.Count){
    if(statements[begin] is GoTo ){
    GoTo? aux = statements[begin] as GoTo;
    if(IsTrue(aux!.condition)){
    flag = true;
    begin = aux.label!.tag.line;break;
    }
    }else execute(statements[begin]);
    begin++;
    }
    if(flag)interpret(statements,begin);
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
 return new Void();
}
public Void VisitLabelStmt(Label stmt)
{
  return new Void();
}
public Void VisitSpawnStmt(Spawn stmt)
{
    string xstr = stmt.x.Value as string;
    string ystr = stmt.y.Value as string;
    int x = Convert.ToInt32(xstr);
    int y = Convert.ToInt32(ystr);
    if(Canva.IsOutRange(x,y))errors.Add(new Error(1,"The position is out the bounds of the canvas"));
    else Walle.Spawn(x,y);
    return new Void();
}
public Void VisitSizeStmt(Size stmt)
{
  string sizestring = stmt.number.Value as string;
  int size = Convert.ToInt32(sizestring);
  if(size <= 0)errors.Add(new Error(1,"The size of the pincel can't be negative"));
  else Walle.Size(size);
  return new Void();
}
public Void VisitColorStmt(Color stmt)
{
  string Color = stmt.color.Value as string;
  if(!IsValidColor(Color))errors.Add(new Error(1,"The word introducted is not a valid color"));
  else Walle.Color(Color);
  return new Void();
}
public Void VisitDrawLineStmt(DrawLine stmt)
{
  string dirxstring = NegativeComprove(stmt.dirx);
  int dirX = Convert.ToInt32(dirxstring);
  string dirystring = NegativeComprove(stmt.diry);
  int dirY = Convert.ToInt32(dirystring);
  if(!IsValidDir(dirX) || !IsValidDir(dirY))errors.Add(new Error(1,"The direction introduce is not a valid one , a direction most be one of the number 1,-1 or 0"));
  else {
  string distancestring = stmt.distance.Value as string;
  int Distance = Convert.ToInt32(distancestring);
  Walle.DrawLine(dirX,dirY,Distance);
  }
  return new Void();
}

public Void VisitDrawCircleStmt(DrawCircle stmt)
{
  string dirxstring = NegativeComprove(stmt.dirx);
  int dirX = Convert.ToInt32(dirxstring);
  string dirystring = NegativeComprove(stmt.diry);
  int dirY = Convert.ToInt32(dirystring);
  if(!IsValidDir(dirX) || !IsValidDir(dirY))errors.Add(new Error(1,"The direction introduce is not a valid one , a direction most be one of the number 1,-1 or 0"));
  else {
  string radiusstring = stmt.radius.Value as string;
  int Radius = Convert.ToInt32(radiusstring);
  Walle.DrawCircle(dirX,dirY,Radius);
  }
  return new Void();
}
public Void VisitDrawRectangleStmt(DrawRectangle stmt)
{
  string dirxstring = NegativeComprove(stmt.dirx);
  int dirX = Convert.ToInt32(dirxstring);
  string dirystring = NegativeComprove(stmt.diry);
  int dirY = Convert.ToInt32(dirystring);
  if(!IsValidDir(dirX) || !IsValidDir(dirY))errors.Add(new Error(1,"The direction introduce is not a valid one , a direction most be one of the number 1,-1 or 0"));
  else {
  string distancestring = stmt.distance.Value as string;
  int Distance = Convert.ToInt32(distancestring);
  string widthstring = stmt.width.Value as string;
  int Width = Convert.ToInt32(widthstring);
  string heightstring = stmt.height.Value as string;
  int Height= Convert.ToInt32(heightstring);
  Walle.DrawRectangle(dirX,dirY,Distance,Width,Height);
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
  int x = Walle.GetActualX();
  return x;
}
public object VisitGetActualY(GetActualY expresion)
{
  int y = Walle.GetActualY();
  return y;
}
public object visitAssign(Assign expresion)
{
 object value = evaluate(expresion.value!);
 enviroment.assign(expresion.name!,value);
 return value;
}
public object visitLogical(Logical expresion)
{
 object left = evaluate(expresion.left);
 if(expresion.Operator.type == TokenTypes.OR)
 {
    if(IsTrue(left))return left;
 }
 else if (!IsTrue(left))return left;
 return evaluate(expresion.right);
}
public object visitLiteral(Literal expresion)
{
return expresion.Value;
}
public object visitGrouping(Grouping expresion)
{
return evaluate(expresion.expresion!);
}
public object visitUnary(Unary expresion)
{
    object right  = evaluate(expresion.Rightside!);
    switch (expresion.Operator!.type)
    {
        case TokenTypes.MINUS:
        NumberOperand(expresion.Operator,right);
        Console.WriteLine(-Convert.ToInt32(right));
        return -Convert.ToInt32(right);
        case TokenTypes.BANG :
        return !IsTrue(right);
        case TokenTypes.POW :
        NumberOperand(expresion.Operator,right);
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
    switch(expresion.Operator!.type)
    {
        case TokenTypes.MINUS:
        NumberOperands(expresion.Operator,left,right);
        return Convert.ToInt32(left) - Convert.ToInt32(right);
        case TokenTypes.PLUS:
        NumberOperands(expresion.Operator,left,right);
        return Convert.ToInt32(left) + Convert.ToInt32(right);
        case TokenTypes.MODUL:
        NumberOperands(expresion.Operator,left,right);
        return Convert.ToInt32(left) % Convert.ToInt32(right);
        case TokenTypes.DIVIDE:
        NumberOperands(expresion.Operator,left,right);
        return Convert.ToInt32(left) / Convert.ToInt32(right);
        case TokenTypes.PRODUCT:
        NumberOperands(expresion.Operator,left,right);
        return Convert.ToInt32(left) * Convert.ToInt32(right);
        case TokenTypes.GREATER:
        NumberOperands(expresion.Operator,left,right);
        return Convert.ToInt32(left) > Convert.ToInt32(right);
        case TokenTypes.GREATER_EQUAL:
        NumberOperands(expresion.Operator,left,right);
        return Convert.ToInt32(left) >= Convert.ToInt32(right);
        case TokenTypes.LESS:
        NumberOperands(expresion.Operator,left,right); 
        return Convert.ToInt32(left) < Convert.ToInt32(right);
        case TokenTypes.LESS_EQUAL:
        NumberOperands(expresion.Operator,left,right);
        return Convert.ToInt32(left) <= Convert.ToInt32(right);
        case TokenTypes.BANG_EQUAL:
        return !IsEqual(left,right);
        case TokenTypes.EQUAL_EQUAL:
        return IsEqual(left,right);
    }
    return null!;
}
private bool IsEqual(object left,object right)
{
    if(left == null && right == null)return true;
    if(left == null)return false;
    return left == right;
}
private bool IsTrue(object ObjectConcrete)
{
    if(ObjectConcrete == null)return false;
    if(ObjectConcrete is bool booleanValue)return booleanValue;
    return true;
}
private object evaluate(Expresion expresion)
{
    return expresion.accept(this);
}
private void NumberOperand(Token Operator,object operand)
{
if(operand is string operandtring  && int.TryParse(operandtring,out int operandInt))return;
errors.Add(new Error(Operator.line,"Operand must be a number"));
}
private void NumberOperands(Token Operator,object left,object right)
{
 if(!IsAritmetic(left) && !IsAritmetic(right))errors.Add(new Error (Operator.line,"Operands must be a numbers"));
}
private bool IsAritmetic(object expresion)
{
   string exp = expresion as string;
 if(Convert.ToInt32(exp) != null)return true;
 return false;
}
private bool IsVoid(List<Stmt>statements)
  {
    foreach (Stmt stmt in statements)
    {
      if(stmt is Spawn)return true;
      if(stmt is GoTo)return true;
      if(stmt is Size)return true;
      if(stmt is Color)return true;
      if(stmt is DrawLine)return true;
      if(stmt is DrawCircle)return true;
      if(stmt is DrawRectangle)return true;
      if(stmt is Fill)return true;
    }
    return false;
  }
private bool IsValidColor(string color)
{
  switch(color)
  {
    case "Red":
    case "Blue":
    case "Green":
    case "Yellow":
    case "Orange":
    case "Purple":
    case "Black":
    case "White":
    case "Transparent":return true;
    default: return false;
  }
}
private bool IsValidDir(int dir)
{
 return dir == 1 || dir == -1 || dir == 0 ;
}
private string NegativeComprove(object dir)
{
  if(dir is Literal ){
    Literal lit = dir as Literal;
    return lit.Value as string;
  }
  else{
    Unary una = dir as Unary;
    Literal lit = una.Rightside as Literal;
    return una.Operator.writing + lit.Value as string;
  }
}
}
