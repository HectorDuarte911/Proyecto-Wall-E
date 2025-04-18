namespace WALLE;
public abstract class Stmt
{
public interface IVisitor<R>
{
    R VisitExpressionStmt(Expression stmt);
    R VisitGoToStmt(GoTo stmt);
    R VisitLabelStmt(Label stmt);
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