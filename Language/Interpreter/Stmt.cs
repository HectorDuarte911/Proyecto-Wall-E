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
    public Label? label { get; private set; }
    public GoTo(Expresion condition, Label? label)
    {
        this.condition = condition;
        this.label = label;
    }
    public override R accept<R>(IVisitor<R> visitor)
    {
        return visitor.VisitGoToStmt(this);
    }
}
public class Label : Stmt
{
    public override R accept<R>(IVisitor<R> visitor)
    {
        return visitor.VisitLabelStmt(this);
    }
    public Token tag { get; private set; }
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
    public Token KeywordToken { get; private set; }
    public Expresion x { get; private set; }
    public Expresion y { get; private set; }
    public Spawn(Token keywordToken, Expresion x, Expresion y)
    {
        KeywordToken = keywordToken;
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
    public Token KeywordToken { get; private set; }
    public Expresion number { get; private set; }
    public Size(Token keywordToken, Expresion number)
    {
        KeywordToken = keywordToken;
        this.number = number;
    }
}
public class Color : Stmt
{
    public override R accept<R>(IVisitor<R> visitor)
    {
        return visitor.VisitColorStmt(this);
    }
    public Token KeywordToken { get; private set; }
    public Expresion color { get; private set; }
    public Color(Token keywordToken, Expresion color)
    {
        this.color = color;
        KeywordToken = keywordToken;
    }
}
public class DrawLine : Stmt
{
    public override R accept<R>(IVisitor<R> visitor)
    {
        return visitor.VisitDrawLineStmt(this);
    }
    public Token KeywordToken { get; private set; }
    public Expresion dirx { get; private set; }
    public Expresion diry { get; private set; }
    public Expresion distance { get; private set; }
    public DrawLine(Token keywordToken, Expresion dirx, Expresion diry, Expresion distance)
    {
        KeywordToken = keywordToken;
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
    public Token KeywordToken { get; private set; }
    public Expresion dirx { get; private set; }
    public Expresion diry { get; private set; }
    public Expresion radius { get; private set; }
    public DrawCircle(Token keywordToken, Expresion dirx, Expresion diry, Expresion radius)
    {
        KeywordToken = keywordToken;
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
    public Token KeywordToken { get; private set; }
    public Expresion dirx { get; private set; }
    public Expresion diry { get; private set; }
    public Expresion distance { get; private set; }
    public Expresion width { get; private set; }
    public Expresion height { get; private set; }
    public DrawRectangle(Token keywordToken, Expresion dirx, Expresion diry, Expresion distance, Expresion width, Expresion height)
    {
        KeywordToken = keywordToken;
        this.dirx = dirx;
        this.diry = diry;
        this.distance = distance;
        this.width = width;
        this.height = height;
    }
}
public class Fill : Stmt
{
    public Token KeywordToken { get; private set; }
    public Fill(Token keywordToken)
    {
        KeywordToken = keywordToken;
    }
    public override R accept<R>(IVisitor<R> visitor)
    {
        return visitor.VisitFillStmt(this);
    }
}