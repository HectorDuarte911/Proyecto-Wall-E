namespace WALLE;
public abstract class Stmt
{
    public interface IVisitor<T>
    {
        T VisitExpressionStmt(Expression stmt);
        T VisitGoToStmt(GoTo stmt);
        T VisitLabelStmt(Label stmt);
        T VisitSpawnStmt(Spawn stmt);
        T VisitSizeStmt(Size stmt);
        T VisitColorStmt(Color stmt);
        T VisitDrawLineStmt(DrawLine stmt);
        T VisitDrawCircleStmt(DrawCircle stmt);
        T VisitDrawRectangleStmt(DrawRectangle stmt);
        T VisitFillStmt(Fill stmt);
    }
    public abstract T accept<T>(IVisitor<T> visitor);
}
public class Expression : Stmt
{
    public Expresion expresion { get; private set; }
    public Expression(Expresion expresion) => this.expresion = expresion;
    public override T accept<T>(IVisitor<T> visitor) => visitor.VisitExpressionStmt(this);
}
public class GoTo : Stmt
{
    public Expresion condition { get; private set; }
    public Label? label { get; private set; }
    public GoTo(Expresion condition, Label? label)
    {
        this.condition = condition;
        this.label = label;
    }
    public override T accept<T>(IVisitor<T> visitor) => visitor.VisitGoToStmt(this);
}
public class Label : Stmt
{
    public override T accept<T>(IVisitor<T> visitor) => visitor.VisitLabelStmt(this);
    public Token tag { get; private set; }
    public Label(Token tag) => this.tag = tag;
}
public class Spawn : Stmt
{
    public override T accept<T>(IVisitor<T> visitor) => visitor.VisitSpawnStmt(this);
    public Token keyword { get; private set; }
    public Expresion x { get; private set; }
    public Expresion y { get; private set; }
    public Spawn(Token keyword, Expresion x, Expresion y)
    {
        this.keyword = keyword;
        this.x = x;
        this.y = y;
    }
}
public class Size : Stmt
{
    public override T accept<T>(IVisitor<T> visitor) => visitor.VisitSizeStmt(this);
    public Token keyword { get; private set; }
    public Expresion number { get; private set; }
    public Size(Token keyword, Expresion number)
    {
        this.keyword = keyword;
        this.number = number;
    }
}
public class Color : Stmt
{
    public override T accept<T>(IVisitor<T> visitor) => visitor.VisitColorStmt(this);
    public Token keyword { get; private set; }
    public Expresion color { get; private set; }
    public Color(Token keyword, Expresion color)
    {
        this.color = color;
        this.keyword = keyword;
    }
}
public class DrawLine : Stmt
{
    public override T accept<T>(IVisitor<T> visitor) =>  visitor.VisitDrawLineStmt(this);
    public Token keyword { get; private set; }
    public Expresion dirx { get; private set; }
    public Expresion diry { get; private set; }
    public Expresion distance { get; private set; }
    public DrawLine(Token keyword, Expresion dirx, Expresion diry, Expresion distance)
    {
        this.keyword = keyword;
        this.dirx = dirx;
        this.diry = diry;
        this.distance = distance;
    }
}
public class DrawCircle : Stmt
{
    public override T accept<T>(IVisitor<T> visitor) => visitor.VisitDrawCircleStmt(this);
    public Token keyword { get; private set; }
    public Expresion dirx { get; private set; }
    public Expresion diry { get; private set; }
    public Expresion Radius { get; private set; }
    public DrawCircle(Token keyword, Expresion dirx, Expresion diry, Expresion Radius)
    {
        this.keyword = keyword;
        this.dirx = dirx;
        this.diry = diry;
        this.Radius = Radius;
    }
}
public class DrawRectangle : Stmt
{
    public override T accept<T>(IVisitor<T> visitor) => visitor.VisitDrawRectangleStmt(this);
    public Token keyword { get; private set; }
    public Expresion dirx { get; private set; }
    public Expresion diry { get; private set; }
    public Expresion distance { get; private set; }
    public Expresion width { get; private set; }
    public Expresion height { get; private set; }
    public DrawRectangle(Token keyword, Expresion dirx, Expresion diry, Expresion distance, Expresion width, Expresion height)
    {
        this.keyword = keyword;
        this.dirx = dirx;
        this.diry = diry;
        this.distance = distance;
        this.width = width;
        this.height = height;
    }
}
public class Fill : Stmt
{
    public Token keyword { get; private set; }
    public Fill(Token keyword) => this.keyword = keyword;
    public override T accept<T>(IVisitor<T> visitor) => visitor.VisitFillStmt(this);
}