namespace WALLE;
using System;
/// <summary>/// To ejecute binarys operations/// </summary>
public interface IBinaryOperation
{
    object Execute(Token op, object left, object right);
}
/// <summary>/// To ejecute '+' operation /// </summary> 
public class AddOperation : IBinaryOperation
{
    public object Execute(Token op, object left, object right)
    {
        if (left is int leftInt && right is int rightInt)
        {
            try{return checked(leftInt + rightInt);}
            catch (OverflowException){ throw new RuntimeError(op, "Arithmetic operation '+' resulted in overflow.");}
        }
        throw new RuntimeError(op, "Operands must be numbers for '+'.");
    }
}
/// <summary>/// To ejecute '>' operation /// </summary>
public class GreaterThanOperation : IBinaryOperation
{
    public object Execute(Token op, object left, object right)
    {
        if (left is int leftInt && right is int rightInt)return leftInt > rightInt;
        throw new RuntimeError(op, "Operands must be numbers for '>'.");
    }
}   
/// <summary>/// To ejecute '-' operation /// </summary>
public class SubtractOperation : IBinaryOperation
{
    public object Execute(Token op, object left, object right)
    {
        if (left is int leftInt && right is int rightInt) return leftInt - rightInt;
        throw new RuntimeError(op, "Operands must be numbers for '-'.");
    }
}
/// <summary>/// To ejecute '*' operation /// </summary>
public class ProductOperation() : IBinaryOperation
{
    public object Execute(Token op, object left, object right)
    {
        if (left is int leftInt && right is int rightInt) return leftInt * rightInt;
        throw new RuntimeError(op, "Operands must be numbers for '*'.");
    }
}
/// <summary>/// To ejecute '/' operation /// </summary>
public class DivideOperation : IBinaryOperation
{
    public object Execute(Token op, object left, object right)
    {
        if (left is int leftInt && right is int rightInt)return leftInt / rightInt;
        throw new RuntimeError(op, "Operands must be numbers for '/'.");
    }
}
/// <summary>/// To ejecute '%' operation /// </summary>
public class ModuloOperation : IBinaryOperation
{
    public object Execute(Token op, object left, object right)
    {
        if (left is int leftInt && right is int rightInt)return leftInt % rightInt;
        throw new RuntimeError(op, "Operands must be numbers for '%'.");
    }
}
/// <summary>/// To ejecute '**' operation /// </summary>
public class PowerOperation : IBinaryOperation
{
    public object Execute(Token op, object left, object right)
    {
        if (left is int leftInt && right is int rightInt) return Math.Pow(leftInt, rightInt);
        throw new RuntimeError(op, "Operands must be numbers for '**'.");
    }
}
/// <summary>/// To ejecute '>=' operation /// </summary>
public class GreaterEqualOperation : IBinaryOperation
{
    public object Execute(Token op, object left, object right)
    {
        if (left is int leftInt && right is int rightInt) return leftInt >= rightInt;
        throw new RuntimeError(op, "Operands must be numbers for '>='.");
    }
} 
/// <summary>/// To ejecute '<' operation /// </summary>
public class LessThanOperation : IBinaryOperation
{
    public object Execute(Token op, object left, object right)
    {
        if (left is int leftInt && right is int rightInt)return leftInt < rightInt;
        throw new RuntimeError(op, "Operands must be numbers for '<'.");
    }
} 
/// <summary>/// To ejecute '<=' operation /// </summary>
public class LessEqualOperation : IBinaryOperation
{
    public object Execute(Token op, object left, object right)
    {
        if (left is int leftInt && right is int rightInt)return leftInt <= rightInt;
        throw new RuntimeError(op, "Operands must be numbers for '<='.");
    }
} 
