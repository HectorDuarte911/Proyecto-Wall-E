/// <summary>
/// Father class of the expresions in the code
/// </summary>
public abstract class Expresion
{
    /// <summary>
/// Represents all the operation between two expresions
/// </summary>
public class Binary : Expresion
{
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
/// <summary>
/// Represents the agrouping of an expresion
/// </summary>
public class Grouping : Expresion
{
    public Expresion? expresion {get;}
    public Grouping(Expresion expresion)
    {
      this.expresion = expresion;
    }
}
/// <summary>
/// Represent the diferent literals
/// </summary>
public class Literal : Expresion
{
    public object Value {get;}
    public Literal(object value)
    {
        Value = value;
    }
}
/// <summary>
/// Represent all the 
/// </summary>
public class Unary : Expresion
{
    public Token? Operator {get;}
    public Expresion? Rightside {get;}
    public Unary(Token operatortoken,Expresion rightside)
    {
        Rightside = rightside;
        Operator = operatortoken;
    }
}
}
