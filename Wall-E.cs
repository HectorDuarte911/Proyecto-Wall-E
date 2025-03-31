public class Walle 
{
    public static int Colum {get;private set;}
    public static int Row {get;private set;}
    public static int PincelSize {get;private set;}
    public static string? PincelColor {get ; private set;}
    
    public static void Spawn(int x, int y)
    {
        if(Canvas.IsOutRange(x,y))throw new Exception("IS out of range look at it");
        Row   = x;
        Colum = y;
    } 
    public static void Size(int k)
    {
        if(k % 2 != 0)k --;
        PincelSize = k;
    }
    public static void Color(string color)
    {
        PincelColor = color;
    }
    public static void DrawLine (int dirX, int dirY, int distance)
    {
       DrawLineHelper(dirX , dirY ,distance , distance);
       Spawn(Colum + dirX * distance , Colum + dirY * distance);
    }
    static void DrawLineHelper (int dirX, int dirY, int distance ,int MoveDoing)
    {
        if(distance != MoveDoing)
        {
            int RowSum =  Row + dirY * (distance - MoveDoing);
            int ColumSum = Colum + dirX * (distance - MoveDoing);
            if(!Canvas.IsOutRange(ColumSum,RowSum))
            {
             Canvas.canvas[ColumSum , RowSum] = PincelColor;  
             DrawLineHelper(dirX , dirY ,distance ,MoveDoing--);
            }
        }
    } 
    public static void DrawCircle(int dirX, int dirY,int radius)
    {
        
    }
}