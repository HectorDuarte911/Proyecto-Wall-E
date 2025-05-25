namespace WALLE;
/// <summary>
/// Father class of the expresions in the code
/// </summary>
public abstract class Expresion
{
    public interface IVisitor<T>
    {
        /// <summary>
        /// Parse a binary token
        /// </summary>
        public T visitBinary(Binary binary);
        /// <summary>
        /// Parse a grouping token
        /// </summary>
        public T visitGrouping(Grouping grouping);
        /// <summary>
        /// Parse a unary token
        /// </summary>
        public T visitUnary(Unary unary);
        /// <summary>
        /// Parse a literal token
        /// </summary>
        public T visitLiteral(Literal literal);
        /// <summary>
        /// Parse a variable token
        /// </summary>
        public T visitVariable(Variable variable);
        /// <summary>
        /// Parse a assingn token
        /// </summary>
        public T visitAssign(Assign assign);
        /// <summary>
        /// Parse a logical token
        /// </summary>
        public T visitLogical(Logical logical);
        /// <summary>
        /// Parse a GetActualX token
        /// </summary>
        public T VisitGetActualX(GetActualX getaCtualx);
        /// <summary>
        /// Parse a GetActualY token
        /// </summary>
        public T VisitGetActualY(GetActualY getactualy);
        /// <summary>
        /// Parse a IsBrushColor token
        /// </summary>
        public T VisitIsBrushColor(IsBrushColor isbrushcolor);
        /// <summary>
        /// Parse a ISBrushSize token
        /// </summary>
        public T VisitIsBrushSize(IsBrushSize isbrushsize);
        /// <summary>
        /// Parse a GetColorCount token
        /// </summary>
        public T VisitGetColorCount(GetColorCount getcolorcount);
        /// <summary>
        /// Parse a IsCanvasColor token
        /// </summary>
        public T VisitIsCanvasColor(IsCanvasColor iscanvascolor);
        /// <summary>
        /// Parse a GetCanvasSize token
        /// </summary>
        public T VisitGetCanvasSize(GetCanvasSize getcanvassize);
    }
    /// <summary>
    /// Assing the corresponding type of token parse
    /// </summary>
    public abstract T accept<T>(IVisitor<T> visitor);
}
public class Assign : Expresion
{
    /// <summary>
    /// Name of the assing variable
    /// </summary>
    public Token? name { get; private set; }
    /// <summary>
    /// Value assingned to the valiable
    /// </summary>
    public Expresion? value { get; private set; }
    public Assign(Token name, Expresion value)
    {
        this.name = name;
        this.value = value;
    }
    public override T accept<T>(IVisitor<T> visitor) => visitor.visitAssign(this);
}
public class Binary : Expresion
{
    /// <summary>
    /// Expresion in the left side of the operator
    /// </summary>
    public Expresion? leftside { get; private set; }
    /// <summary>
    /// Operator of the binary expresion
    /// </summary>
    public Token? Operator { get; private set; }
    /// <summary>
    /// Expresion in the right side of the operator
    /// </summary>
    public Expresion? rightside { get; private set; }
    public Binary(Expresion leftside, Token operatortoken, Expresion rightside)
    {
        this.leftside = leftside;
        this.rightside = rightside;
        Operator = operatortoken;
    }
    public override T accept<T>(IVisitor<T> visitor) => visitor.visitBinary(this);
}
public class Grouping : Expresion
{
    /// <summary>
    /// Expresion inside the grouping simbols
    /// </summary>
    public Expresion? expresion { get; private set; }
    public Grouping(Expresion expresion) => this.expresion = expresion;
    public override T accept<T>(IVisitor<T> visitor) => visitor.visitGrouping(this);
}
public class Literal : Expresion
{
    /// <summary>
    /// Value of the literal
    /// </summary>
    public object Value { get; private set; }
    public Literal(object value) => Value = value;
    public override T accept<T>(IVisitor<T> visitor) => visitor.visitLiteral(this);
}
public class Unary : Expresion
{
    /// <summary>
    /// Operator of the unary expresion
    /// </summary>
    public Token? Operator { get; private set; }
    /// <summary>
    /// Expresion of the right side of the operator
    /// </summary>
    public Expresion? rightside { get; private set; }
    public Unary(Token operatortoken, Expresion rightside)
    {
        this.rightside = rightside;
        Operator = operatortoken;
    }
    public override T accept<T>(IVisitor<T> visitor) => visitor.visitUnary(this);
}
public class Variable : Expresion
{
    /// <summary>
    /// Name of the variable
    /// </summary>
    public Token name { get; private set; }
    public Variable(Token name) => this.name = name;
    public override T accept<T>(IVisitor<T> visitor) => visitor.visitVariable(this);
}
public class Logical : Expresion
{
    /// <summary>
    /// Expresion in the left side of the logical operator
    /// </summary>
    public Expresion left { get; private set; }
    /// <summary>
    /// Logical operator
    /// </summary>
    public Token Operator { get; private set; }
    /// <summary>
    /// Expresion in the right side of the logical operator
    /// </summary>
    public Expresion right { get; private set; }
    public Logical(Expresion left, Token Operator, Expresion right)
    {
        this.left = left;
        this.Operator = Operator;
        this.right = right;
    }
    public override T accept<T>(IVisitor<T> visitor) => visitor.visitLogical(this);
}
public class GetActualX : Expresion
{
    /// <summary>///Token representation of the keyword/// </summary>
    public Token keyword { get; private set; }
    public GetActualX(Token keyword) => this.keyword = keyword;
    public override T accept<T>(IVisitor<T> visitor) => visitor.VisitGetActualX(this);
}
public class GetActualY : Expresion
{
    /// <summary>///Token representation of the keyword/// </summary>
    public Token keyword { get; private set; }
    public GetActualY(Token keyword) => this.keyword = keyword;
    public override T accept<T>(IVisitor<T> visitor) => visitor.VisitGetActualY(this); 
}
public class IsBrushColor : Expresion
{
    /// <summary>///Token representation of the keyword/// </summary>
    public Token keyword { get; private set; }
    /// <summary>
    /// Future color of the brush
    /// </summary>
    public Expresion color { get; private set; }
    public IsBrushColor(Token keyword, Expresion color)
    {
        this.color = color;
        this.keyword = keyword;
    }
    public override T accept<T>(IVisitor<T> visitor) => visitor.VisitIsBrushColor(this);
}
public class IsBrushSize : Expresion
{
    /// <summary>///Token representation of the keyword/// </summary>
    public Token keyword { get; private set; }
    /// <summary>
    /// Future size of the brush
    /// </summary>
    public Expresion size { get; private set; }
    public IsBrushSize(Token keyword, Expresion size)
    {
        this.size = size;
        this.keyword = keyword;
    }
    public override T accept<T>(IVisitor<T> visitor) => visitor.VisitIsBrushSize(this);
}
public class GetColorCount : Expresion
{
    /// <summary>
    /// Color that is counting
    /// </summary>
    public Expresion color { get; private set; }
    /// <summary>
    /// Cordenate x of the start of the inspection area
    /// </summary>
    public Expresion x1 { get; private set; }
    /// <summary>
    /// Cordenate y of the start of the inspection area
    /// </summary>
    public Expresion y1 { get; private set; }
    /// <summary>
    /// Cordenate x of the end of the inspection area
    /// </summary> 
    public Expresion x2 { get; private set; }
     /// <summary>
    /// Cordenate y of the end of the inspection area
    /// </summary>
    public Expresion y2 { get; private set; }
    /// <summary>///Token representation of the keyword/// </summary>
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
    public override T accept<T>(IVisitor<T> visitor) => visitor.VisitGetColorCount(this);
}
public class GetCanvasSize : Expresion
{
    /// <summary>///Token representation of the keyword/// </summary>
    public Token keyword { get; private set; }
    public GetCanvasSize(Token keyword) => this.keyword = keyword;
    public override T accept<T>(IVisitor<T> visitor) => visitor.VisitGetCanvasSize(this);
}
public class IsCanvasColor : Expresion
{
    /// <summary>
    /// Quetion color
    /// </summary>
    public Expresion color { get; private set; }
    /// <summary>
    /// Vertical cordenate of the inspected position
    /// </summary>
    public Expresion vertical { get; private set; }
    /// <summary>
    /// Horizontal cordenate of the inspected position
    /// </summary>
    public Expresion horizontal { get; private set; }
    /// <summary>///Token representation of the keyword/// </summary>
    public Token keyword { get; private set; }
    public IsCanvasColor(Token keyword, Expresion color, Expresion vertical, Expresion horizontal)
    {
        this.color = color;
        this.vertical = vertical;
        this.horizontal = horizontal;
        this.keyword = keyword;
    }
    public override T accept<T>(IVisitor<T> visitor) => visitor.VisitIsCanvasColor(this);
}