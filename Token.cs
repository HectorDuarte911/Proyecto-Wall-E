public class Token
{
    /// <summary>
    /// Type of the token
    /// </summary>
    TokenTypes type;
    /// <summary>
    /// Token in string
    /// </summary>
    string writing;
    /// <summary>
    /// 
    /// </summary>
    Object literal;
    /// <summary>
    /// Line of the token
    /// </summary>
    int line;
    /// <summary>
    /// Constructor of Token
    /// </summary>
    /// <param name="type"></param>
    /// <param name="writing"></param>
    /// <param name="literal"></param>
    /// <param name="line"></param>
    public Token(TokenTypes type,string writing , Object literal, int line)
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