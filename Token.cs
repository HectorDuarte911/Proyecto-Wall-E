public class Token
{
    /// <summary>
    /// Type of the token
    /// </summary>
    public TokenTypes type{get;private set;}
    /// <summary>
    /// Token in string
    /// </summary>
    public string writing{get;private set;}
    /// <summary>
    /// 
    /// </summary>
    public object literal{get;private set;}
    /// <summary>
    /// Line of the token
    /// </summary>
    public int line{get;private set;}
    /// <summary>
    /// Constructor of Token
    /// </summary>
    /// <param name="type"></param>
    /// <param name="writing"></param>
    /// <param name="literal"></param>
    /// <param name="line"></param>
    public Token(TokenTypes type,string writing , object literal, int line)
    {
        this.type = type;
        this.writing = writing;
        this.literal = literal;
        this.line = line;
    }
    public string toString()
    {
        return type + " " + writing + " " + literal;
    }
}