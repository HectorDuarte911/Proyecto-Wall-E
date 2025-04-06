public abstract class Stmt
{
    public interface IVisitor<R>
    {
        R VisitExpressionStmt(Expression stmt);
        R VisitVarStmt(Var stmt);
        R VisitGoToStmt(GoTo stmt);
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
public class Var : Stmt
{
    public Token name { get; private set; }
    public Expresion initializer { get; private set; }
    public Var(Token name, Expresion initializer)
    {
        this.name = name;
        this.initializer = initializer;
    }
    public override R accept<R>(IVisitor<R> visitor)
    {
        return visitor.VisitVarStmt(this);
    }

}
public class GoTo : Stmt
{
    public Expresion condition { get; private set; }
    public GoTo(Expresion condition)
    {
        this.condition = condition;
    }
    public override R accept<R>(IVisitor<R> visitor)
    {
        return visitor.VisitGoToStmt(this);
    }
}

