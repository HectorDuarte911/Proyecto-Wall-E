namespace WALLE;
public abstract class Stmt
{
public interface IVisitor<R>
{
    R VisitExpressionStmt(Expression stmt);
    R VisitGoToStmt(GoTo stmt);
    R VisitLabelStmt(Label stmt);
    R VisitSpawnStmt(Spawn stmt);
    R VisitSizeStmt(Size stmt);
    R VisitColorStmt(Color stmt);
    R VisitDrawLineStmt(DrawLine stmt);
    R VisitDrawCircleStmt(DrawCircle stmt);
    R VisitDrawRectangleStmt(DrawRectangle stmt);
    R VisitFillStmt(Fill stmt);
}
public abstract R accept<R>(IVisitor<R> visitor);
}
public class Expression : Stmt
{
    public Expresion expresion { get; private set; }
    public Expression(Expresion expresion)
    {
        this.expresion = expresion;
    }
    public override R accept<R>(IVisitor<R> visitor)
    {
        return visitor.VisitExpressionStmt(this);
    }
}
public class GoTo : Stmt
{
    public Expresion condition { get; private set; }
    public Label? label {get; private set;}
    public GoTo(Expresion condition,Label? label)
    {
        this.condition = condition;
        this.label = label;
    }
    public override R accept<R>(IVisitor<R> visitor)
    {
        return visitor.VisitGoToStmt(this);
    }
}
public class Label :  Stmt
{
public override R accept<R>(IVisitor<R> visitor)
{
    return visitor.VisitLabelStmt(this);
}
 public Token tag {get; private set;}
 public Label(Token tag)
 {
    this.tag = tag;
 }
}
public class Spawn : Stmt
{ 
   public override R accept<R>(IVisitor<R> visitor)
  {
    return visitor.VisitSpawnStmt(this);
  }
  public Literal x {get;private set;}
  public Literal y {get;private set;}
  public Spawn(Literal x , Literal y)
  {
    this.x = x;
    this.y = y;
  }
}
public class Size : Stmt
{
    public override R accept<R>(IVisitor<R> visitor)
    {
        return visitor.VisitSizeStmt(this);
    }
    public Literal number {get;private set;}
    public Size(Literal number)
    {
       this.number = number;
    }
}
public class Color : Stmt
{
    public override R accept<R>(IVisitor<R> visitor)
    {
        return visitor.VisitColorStmt(this);
    }
    public Literal color {get;private set;}
    public Color(Literal color)
    {
        this.color = color;
    }
}
public class DrawLine : Stmt
{
    public override R accept<R>(IVisitor<R> visitor)
    {
        return visitor.VisitDrawLineStmt(this);
    }
    public object dirx {get;private set;}
    public object diry {get;private set;}
    public Literal distance {get;private set;}
    public DrawLine(object dirx, object diry , Literal distance)
    {
        this.dirx = dirx;
        this.diry = diry;
        this.distance = distance;
    }
}
public class DrawCircle : Stmt
{
    public override R accept<R>(IVisitor<R> visitor)
    {
        return visitor.VisitDrawCircleStmt(this);
    }
    public object dirx {get;private set;}
    public object diry {get;private set;}
    public Literal radius {get;private set;}
    public DrawCircle(object dirx, object diry , Literal radius)
    {
        this.dirx = dirx;
        this.diry = diry;
        this.radius = radius;
    }
}
public class DrawRectangle : Stmt
{
    public override R accept<R>(IVisitor<R> visitor)
    {
        return visitor.VisitDrawRectangleStmt(this);
    }
    public object dirx {get;private set;}
    public object diry {get;private set;}
    public Literal distance {get;private set;}
    public Literal width {get;private set;}
    public Literal height {get;private set;}
    public DrawRectangle(object dirx , object diry , Literal distance , Literal width , Literal height)
    {
        this.dirx = dirx;
        this.diry = diry;
        this.distance = distance;
        this.width = width;
        this.height = height;
    }
}
public class Fill : Stmt
{
    public override R accept<R>(IVisitor<R> visitor)
    {
        return visitor.VisitFillStmt(this);
    }
}