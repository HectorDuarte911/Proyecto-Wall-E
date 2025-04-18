
namespace WALLE;
public class Void{}
public class Interpreter : Expresion.IVisitor<object>,Stmt.IVisitor<Void>
{
public List<Error> errors {get ; private set;}
private Enviroment enviroment;
public Interpreter(List<Error> errors)
{
  this.errors = errors;
  enviroment = new Enviroment(errors);
} 
public void interpret(List<Stmt>statements, int begin)
{
    bool validline= false;
    foreach (Stmt statement in statements)
    {
     if(statement is Label)
     {
        validline = true;
        break;
     }
     if(statement is Expression expresion)
     {
        if(expresion.expresion is Assign)
        {
         validline = true;
         break;   
        }
     }   
    }
    if(!validline)
    {
     errors.Add(new Error(1,"Expect a valid statement"));
    }
    else
    {
        bool flag =false;
    while(begin < statements.Count){
    if(statements[begin] is GoTo ){
    GoTo? aux = statements[begin] as GoTo;
    if(IsTrue(aux!.condition)){
        flag = true;
        begin = aux.label!.tag.line;
        break;
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
}
