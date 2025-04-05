public class Interpreter : IVisitor<object>
{
public object visitLiteral(Literal expresion)
{
return expresion.Value;
}
public object visitGrouping(Grouping expresion)
{
return evaluate(expresion.expresion);
}
public object visitUnary(Unary expresion)
{
    object right  = evaluate(expresion.Rightside);
    switch (expresion.Operator.type)
    {
        case TokenTypes.BANG :
        return !IsTrue(right);
        case TokenTypes.POW :
        NumberOperand(expresion.Operator,right);
        return (int)right * (int)right;
    }
    return null;
}
public object visitBinary(Binary expresion)
{
    object left = evaluate(expresion.Leftside);
    object right = evaluate(expresion.Rightside);
    switch(expresion.Operator.type)
    {
        case TokenTypes.MINUS:
        NumberOperands(expresion.Operator,left,right);
        return (int)left - (int)right;
        case TokenTypes.PLUS:
        NumberOperands(expresion.Operator,left,right);
        return (int)left + (int)right;
        case TokenTypes.DIVIDE:
        NumberOperands(expresion.Operator,left,right);
        return (int)left / (int)right;
        case TokenTypes.PRODUCT:
        NumberOperands(expresion.Operator,left,right);
        return (int)left * (int)right;
        case TokenTypes.GREATER:
        NumberOperands(expresion.Operator,left,right);
        return (int)left > (int)right;
        case TokenTypes.GREATER_EQUAL:
        NumberOperands(expresion.Operator,left,right);
        return (int)left >= (int)right;
        case TokenTypes.LESS:
        NumberOperands(expresion.Operator,left,right); 
        return (int)left < (int)right;
        case TokenTypes.LESS_EQUAL:
        NumberOperands(expresion.Operator,left,right);
        return (int)left <= (int)right;
        case TokenTypes.BANG_EQUAL:
        return !IsEqual(left,right);
        case TokenTypes.EQUAL_EQUAL:
        return IsEqual(left,right);

    }
    return null;
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
if(operand is int)return;
throw new RuntimeError(Operator,"Operand must be a number");
}
private void NumberOperands(Token Operator,object left,object right)
{
if(left is int && right is int)return;
throw new RuntimeError(Operator,"Operands must be a numbers");
}
public void interpret(Expresion expresion)
{
    try
    {
        object value = evaluate(expresion);
        Console.WriteLine(stringif(value));
    }
    catch (RuntimeError error)
    {
      Language.runtimerror(error);
    }
}
private string stringif(object obj)
{
    if(obj == null)return "null";
    return obj.ToString();
}
}
public class RuntimeError : Exception
{
 public Token token {get;}
 public RuntimeError(Token token,string message):base(message)
 {
    this.token = token;
 }
}