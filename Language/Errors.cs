namespace WALLE;
public class Error
{
    /// <summary>
    /// Message of the commit error
    /// </summary>
    public string Argument { get; private set; }
    /// <summary>
    /// Line that contains the error
    /// </summary>
    public int Location { get; private set; }
    public Error(int location, string argument)
    {
        Argument = argument;
        Location = location;
    }
}