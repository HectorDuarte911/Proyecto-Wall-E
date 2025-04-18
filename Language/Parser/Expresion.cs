namespace WALLE;
/// <summary>
/// Father class of the expresions in the code
/// </summary>
public abstract class Expresion
{
public interface IVisitor<R>
{
 public R visitBinary(Binary binary);
 public R visitGrouping(Grouping grouping);
 public R visitUnary(Unary unary);
 public R visitLiteral(Literal literal);
 public R visitVariable(Variable variable);
 public R visitAssign(Assign assign);
 public R visitLogical(Logical logical);
 public R VisitGetActualX(GetActualX getaCtualx);
 public R VisitGetActualY(GetActualY getactualy);
}
 public abstract R accept<R>(IVisitor <R> visitor);
}
public class Assign : Expresion
{
    public Token? name {get;private set;}
    public Expresion? value {get ;private set;}  
    public Assign(Token name,Expresion  value)
    {
        this.name = name;
        this.value = value;
    }
    public override R accept<R>(IVisitor<R> visitor)
    {
        return visitor.visitAssign(this);
    }
}
/// <summary>
/// Represents all the operation between two expresions
/// </summary>
public class Binary : Expresion
{
    public override R accept<R>(IVisitor<R> visitor)
    {
        return visitor.visitBinary(this);
    }
    public Expresion? Leftside {get;}
    public Token? Operator {get;}
    public Expresion? Rightside {get;}
    public Binary(Expresion leftside ,Token operatortoken,Expresion rightside)
    {
        Leftside = leftside;
        Rightside = rightside;
        Operator = operatortoken;
    }
}
/// <summary>
/// Represents the agrouping of an expresion
/// </summary>
public class Grouping : Expresion
{
    public override R accept<R>(IVisitor<R> visitor)
    {
        return visitor.visitGrouping(this);
    }
    public Expresion? expresion {get;}
    public Grouping(Expresion expresion)
    {
      this.expresion = expresion;
    }
}
/// <summary>
/// Represent the diferent literals
/// </summary>
public class Literal : Expresion
{
    public override R accept<R>(IVisitor<R> visitor)
    {
        return visitor.visitLiteral(this);
    }
    public object Value {get;}
    public Literal(object value)
    {
        Value = value;
    }
}
/// <summary>
/// Represent all the 
/// </summary>
public class Unary : Expresion
{
    public override R accept<R>(IVisitor<R> visitor)
    {
        return visitor.visitUnary(this);
    }
    public Token? Operator {get;}
    public Expresion? Rightside {get;}
    public Unary(Token operatortoken,Expresion rightside)
    {
        Rightside = rightside;
        Operator = operatortoken;
    }
}
public class Variable : Expresion
{
   public override R accept<R>(IVisitor<R> visitor)
    {
        return visitor.visitVariable(this);
    }
    public Token name {get;}
    public Variable(Token name)
    {
        this.name = name;
    } 
}
public class Logical : Expresion
{
   public override R accept<R>(IVisitor<R> visitor)
    {
        return visitor.visitLogical(this);
    }
    public Expresion left {get; private set;}
    public Token Operator {get ; private set;}
    public Expresion right {get;private set;}
    public Logical (Expresion left,Token Operator,Expresion right)
    {
        this.left =left;
        this.Operator = Operator;
        this.right =right;
    }
}
public class GetActualX : Expresion
{
    public override R accept<R>(IVisitor<R> visitor)
    {
        return visitor.VisitGetActualX(this);
    }
}
public class GetActualY : Expresion
{
    public override R accept<R>(IVisitor<R> visitor)
    {
        return visitor.VisitGetActualY(this);
    }
}
