namespace WALLE;
/// <summary>
/// Father class of the expresions in the code
/// </summary>
public abstract class Expresion
{
public interface IVisitor<T>
{
 public T visitBinary(Binary binary);
 public T visitGrouping(Grouping grouping);
 public T visitUnary(Unary unary);
 public T visitLiteral(Literal literal);
 public T visitVariable(Variable variable);
 public T visitAssign(Assign assign);
 public T visitLogical(Logical logical);
 public T VisitGetActualX(GetActualX getaCtualx);
 public T VisitGetActualY(GetActualY getactualy);
 public T VisitIsBrushColor(IsBrushColor isbrushcolor);
 public T VisitIsBrushSize(IsBrushSize isbrushsize);
 public T VisitGetColorCount(GetColorCount getcolorcount);
 public T VisitIsCanvasColor(IsCanvasColor iscanvascolor);
 public T VisitGetCanvasSize(GetCanvasSize getcanvassize);
}
public abstract T accept<T>(IVisitor <T> visitor);
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
    public override T accept<T>(IVisitor<T> visitor) => visitor.visitAssign(this);
}
public class Binary : Expresion
{
    public override T accept<T>(IVisitor<T> visitor) => visitor.visitBinary(this);
    public Expresion? leftside { get; private set; }
    public Token? Operator {get;private set;}
    public Expresion? rightside {get;private set;}
    public Binary(Expresion leftside ,Token operatortoken,Expresion rightside)
    {
        this.leftside = leftside;
        this.rightside = rightside;
        Operator = operatortoken;
    }
}
public class Grouping : Expresion
{
    public override T accept<T>(IVisitor<T> visitor) => visitor.visitGrouping(this);
    public Expresion? expresion {get;private set;}
    public Grouping(Expresion expresion) => this.expresion = expresion;
}
public class Literal : Expresion
{
    public override T accept<T>(IVisitor<T> visitor) => visitor.visitLiteral(this);
    public object Value {get;private set;}
    public Literal(object value) => Value = value;
}
public class Unary : Expresion
{
    public override T accept<T>(IVisitor<T> visitor) => visitor.visitUnary(this);
    public Token? Operator {get;private set;}
    public Expresion? rightside {get;private set;}
    public Unary(Token operatortoken,Expresion rightside)
    {
        this.rightside = rightside;
        Operator = operatortoken;
    }
}
public class Variable : Expresion
{
   public override T accept<T>(IVisitor<T> visitor) => visitor.visitVariable(this);
   public Token name {get;private set;}
   public Variable(Token name) =>  this.name = name;
}
public class Logical : Expresion
{
   public override T accept<T>(IVisitor<T> visitor) => visitor.visitLogical(this);
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
    public Token keyword { get; private set; }
    public override T accept<T>(IVisitor<T> visitor) => visitor.VisitGetActualX(this);
    public GetActualX(Token keyword) => this.keyword = keyword;
}
public class GetActualY : Expresion
{
    public Token keyword { get; private set; }
    public override T accept<T>(IVisitor<T> visitor) => visitor.VisitGetActualY(this);
    public GetActualY(Token keyword) => this.keyword = keyword;
}
public class IsBrushColor : Expresion
{
    public override T accept<T> (IVisitor<T> visitor) =>  visitor.VisitIsBrushColor(this);
    public Token keyword { get; private set; }
    public Expresion color { get; private set; }
    public IsBrushColor(Token keyword, Expresion color)
    {
        this.color = color;
        this.keyword = keyword;
    }
}
public class IsBrushSize : Expresion
{
    public override T accept<T> (IVisitor<T> visitor) => visitor.VisitIsBrushSize(this);
    public Token keyword { get; private set; }
    public Expresion size { get; private set; }
    public IsBrushSize(Token keyword, Expresion size)
    {
        this.size = size;
        this.keyword = keyword;
    }
}
public class GetColorCount : Expresion
{
    public override T accept<T>(IVisitor<T> visitor) => visitor.VisitGetColorCount(this);
    public Expresion color { get; private set; }
    public Expresion x1 { get; private set; }
    public Expresion y1 { get; private set; }
    public Expresion x2 { get; private set; }
    public Expresion y2 { get; private set; }
    public Token keyword { get; private set; }
    public GetColorCount(Token keyword, Expresion color, Expresion x1, Expresion y1, Expresion x2, Expresion y2)
    {
        this.color = color;
        this.x1 = x1;
        this.y1 = y1;
        this.x2 = x2;
        this.y2 = y2;
       this.keyword = keyword;
    }
}
public class GetCanvasSize : Expresion
{
    public override T accept<T>(IVisitor<T> visitor) => visitor.VisitGetCanvasSize(this);
    public Token keyword { get; private set; }
    public GetCanvasSize(Token keyword)
    {
        this.keyword = keyword;
    }
}
public class IsCanvasColor : Expresion
{
    public override T accept<T> (IVisitor<T> visitor) => visitor.VisitIsCanvasColor(this);
    public Expresion color {get;private set;}
    public Expresion vertical {get;private set;}
    public Expresion horizontal {get;private set;}
    public Token keyword { get; private set; }
    public IsCanvasColor(Token keyword, Expresion color, Expresion vertical, Expresion horizontal)
    {
        this.color = color;
        this.vertical = vertical;
        this.horizontal = horizontal;
        this.keyword = keyword;
    }
}