namespace WALLE;
public abstract class Stmt
{
    public interface IVisitor<T>
    {
        /// <summary>
        /// Ejecute an expresion statement
        /// </summary>
        T VisitExpressionStmt(Expression stmt);
        /// <summary>
        /// Ejecute a GoTo statement
        /// </summary>
        T VisitGoToStmt(GoTo stmt);
        /// <summary>
        /// Ejecute a Label statement
        /// </summary>
        T VisitLabelStmt(Label stmt);
        /// <summary>
        /// Ejecute a Spawn statement
        /// </summary>
        T VisitSpawnStmt(Spawn stmt);
        /// <summary>
        /// Ejecute a Size statement
        /// </summary>
        T VisitSizeStmt(Size stmt);
        /// <summary>
        /// Ejecute a Color statement
        /// </summary>
        T VisitColorStmt(Color stmt);
        /// <summary>
        /// Ejecute a DrawLine statement
        /// </summary>
        T VisitDrawLineStmt(DrawLine stmt);
        /// <summary>
        /// Ejecute a Circle statement
        /// </summary>
        T VisitDrawCircleStmt(DrawCircle stmt);
        /// <summary>
        /// Ejecute a DrawRectangle statement
        /// </summary>
        T VisitDrawRectangleStmt(DrawRectangle stmt);
        /// <summary>
        /// Ejecute a Fill statement
        /// </summary>
        T VisitFillStmt(Fill stmt);
    }
    /// <summary>
    /// Assing the corresponding type of statement to ejecute
    /// </summary>
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
    /// <summary>
    /// Condition to ejecute the GoTo
    /// </summary>
    public Expresion condition { get; private set; }
    /// <summary>
    /// Label that the code is going if the GoTo ejecute
    /// </summary>
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
    /// <summary>
    /// Name of the label
    /// </summary>
    public Token tag { get; private set; }
    public Label(Token tag) => this.tag = tag;
    public override T accept<T>(IVisitor<T> visitor) => visitor.VisitLabelStmt(this);
}
public class Spawn : Stmt
{
    /// <summary>///Token representation of the keyword/// </summary>
    public Token keyword { get; private set; }
    /// <summary>
    /// Cordenate x of the position that walle is going to spawn
    /// </summary>
    public Expresion x { get; private set; }
    /// <summary>
    /// Cordenate y of the position that walle is going to spawn
    /// </summary>
    public Expresion y { get; private set; }
    public Spawn(Token keyword, Expresion x, Expresion y)
    {
        this.keyword = keyword;
        this.x = x;
        this.y = y;
    }
    public override T accept<T>(IVisitor<T> visitor) => visitor.VisitSpawnStmt(this);
}
public class Size : Stmt
{
    /// <summary>///Token representation of the keyword/// </summary>
    public Token keyword { get; private set; }
    /// <summary>
    /// New size of the pincel
    /// </summary>
    public Expresion number { get; private set; }
    public Size(Token keyword, Expresion number)
    {
        this.keyword = keyword;
        this.number = number;
    }
    public override T accept<T>(IVisitor<T> visitor) => visitor.VisitSizeStmt(this); 
}
public class Color : Stmt
{
    /// <summary>///Token representation of the keyword/// </summary>
    public Token keyword { get; private set; }
    /// <summary>
    /// New color of the pincel
    /// </summary>
    public Expresion color { get; private set; }
    public Color(Token keyword, Expresion color)
    {
        this.color = color;
        this.keyword = keyword;
    }
    public override T accept<T>(IVisitor<T> visitor) => visitor.VisitColorStmt(this);
}
public class DrawLine : Stmt
{
    /// <summary>///Token representation of the keyword/// </summary>
    public Token keyword { get; private set; }
    /// <summary>
    /// Direction of the cordenate x that the brush is going to draw 
    /// </summary>
    public Expresion dirx { get; private set; }
    /// <summary>
    /// Direction of the cordenate y that the brush is going to draw 
    /// </summary>
    public Expresion diry { get; private set; }
    /// <summary>
    /// Distance that the brush is going to draw
    /// </summary>
    public Expresion distance { get; private set; }
    public DrawLine(Token keyword, Expresion dirx, Expresion diry, Expresion distance)
    {
        this.keyword = keyword;
        this.dirx = dirx;
        this.diry = diry;
        this.distance = distance;
    }
    public override T accept<T>(IVisitor<T> visitor) =>  visitor.VisitDrawLineStmt(this);
}
public class DrawCircle : Stmt
{
    /// <summary>///Token representation of the keyword/// </summary>
    public Token keyword { get; private set; }
    /// <summary>
    /// Direction of the cordenate x that the circle center is going to be  
    /// </summary>
    public Expresion dirx { get; private set; }
    /// <summary>
    /// Direction of the cordenate y that the circle center is going to be  
    /// </summary>
    public Expresion diry { get; private set; }
    /// <summary>
    /// Distance of the pinxels to the center of the circle
    /// </summary>
    public Expresion Radius { get; private set; }
    public DrawCircle(Token keyword, Expresion dirx, Expresion diry, Expresion Radius)
    {
        this.keyword = keyword;
        this.dirx = dirx;
        this.diry = diry;
        this.Radius = Radius;
    }
    public override T accept<T>(IVisitor<T> visitor) => visitor.VisitDrawCircleStmt(this);
}
public class DrawRectangle : Stmt
{
    /// <summary>///Token representation of the keyword/// </summary>
    public Token keyword { get; private set; }
    /// <summary>
    /// Direction of the cordenate x that the rectangle center is going to be  
    /// </summary>
    public Expresion dirx { get; private set; }
    /// <summary>
    /// Direction of the cordenate y that the rectangle center is going to be  
    /// </summary>
    public Expresion diry { get; private set; }
    /// <summary>
    /// Distance to the actual position to the rectangle center
    /// </summary>
    public Expresion distance { get; private set; }
    /// <summary>
    /// Width of the rectangle
    /// </summary>
    public Expresion width { get; private set; }
    /// <summary>
    /// Height of the rectangle 
    /// </summary>
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
    public override T accept<T>(IVisitor<T> visitor) => visitor.VisitDrawRectangleStmt(this);
}
public class Fill : Stmt
{
    /// <summary>///Token representation of the keyword/// </summary>
    public Token keyword { get; private set; }
    public Fill(Token keyword) => this.keyword = keyword;
    public override T accept<T>(IVisitor<T> visitor) => visitor.VisitFillStmt(this);
}