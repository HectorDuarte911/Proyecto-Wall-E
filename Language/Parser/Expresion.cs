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
 public R VisitIsBrushColor(IsBrushColor isbrushcolor);
 public R VisitIsBrushSize(IsBrushSize isbrushsize);
 public R VisitGetColorCount(GetColorCount getcolorcount);
 public R VisitIsCanvasColor(IsCanvasColor iscanvascolor);
 public R VisitGetCanvasSize(GetCanvasSize getcanvassize);
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
public class IsBrushColor : Expresion
{
    public override R accept<R> (IVisitor<R> visitor)
    {
        return visitor.VisitIsBrushColor(this);
    }
    public Expresion color {get;private set;}
    public IsBrushColor(Expresion color)
    {
        this.color = color;
    }
}
public class IsBrushSize : Expresion
{
    public override R accept<R> (IVisitor<R> visitor)
    {
        return visitor.VisitIsBrushSize(this);
    }
    public Expresion size {get;private set;}
    public IsBrushSize(Expresion size)
    {
        this.size = size;
    }
}
public class GetColorCount: Expresion   
{
    public override R accept<R> (IVisitor<R> visitor)
    {
        return visitor.VisitGetColorCount(this);
    }
    public Expresion color {get;private set;}
    public Expresion x1 {get;private set;}
    public Expresion y1 {get;private set;}
    public Expresion x2 {get;private set;}
    public Expresion y2 {get;private set;}
    public GetColorCount(Expresion color,Expresion x1,Expresion y1,Expresion x2,Expresion y2)
    {
        this.color = color;
        this.x1 = x1;
        this.y1 = y1;
        this.x2 = x2;
        this.y2 = y2;
    }
}
public class IsCanvasColor : Expresion
{
    public override R accept<R> (IVisitor<R> visitor)
    {
        return visitor.VisitIsCanvasColor(this);
    }
    public Expresion color {get;private set;}
    public Expresion vertical {get;private set;}
    public Expresion horizontal {get;private set;}
    public IsCanvasColor(Expresion color,Expresion vertical,Expresion horizontal)
    {
        this.color = color;
        this.vertical =vertical;
        this.horizontal = horizontal;
    }
}
public class GetCanvasSize : Expresion
{
    public override R accept<R> (IVisitor<R> visitor)
    {
        return visitor.VisitGetCanvasSize(this);
    }
}