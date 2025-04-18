namespace WALLE;
public class Error
{
    public string Argument { get; private set; }

    public int Location {get; private set;}

    public Error(int location, string argument)
    {
        Argument = argument;
        Location = location;
    }
}