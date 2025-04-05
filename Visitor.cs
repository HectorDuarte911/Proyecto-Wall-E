public interface IVisitor<R>
{
 public object visitBinary(Binary binary);
 public object visitGrouping(Grouping grouping);
 public object visitUnary(Unary unary);
 public object visitLiteral(Literal literal);
}