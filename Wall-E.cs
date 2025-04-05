public class Walle 
{
    public static int Colum {get;private set;}
    public static int Row {get;private set;}
    public static int PincelSize {get;private set;}
    public static string? PincelColor {get ; private set;}
    public static void Spawn(int x, int y)
    {
        if(Canva.IsOutRange(x,y))throw new Exception("Is out of range look at it");
        Row   = y;
        Colum = x;
    } 
    public static void Size(int k)
    {
        if(k % 2 == 0)k = k -1;
        PincelSize = k;
    }
    public static void Color(string color)
    {
        PincelColor = color;
    }
    public static void DrawLine (int dirX, int dirY, int distance)
    {
        if(dirX == 0 || dirY == 0)DrawLineVerHor(dirX,dirY,distance);
        else DrawLineCurve(dirX,dirY,distance);
    }
    static void DrawLineCurve(int dirX,int dirY,int distance)
    {
        for (int i = 0; i < distance; i++)
        {
            int size = PincelSize;
            if( i == distance - 1)size = PincelSize-1;
            DrawLineHelper(dirX,0,size,size);
            DrawLineHelper(0,dirY,size,size);
            int NewColum  =Colum + dirX; 
            int NewRow = Row + dirY;
            if(!Canva.IsOutRange(NewColum,NewRow))
            {
                Spawn(NewColum,NewRow);
            }
        }
    }
    static void DrawLineVerHor(int dirX, int dirY, int distance)
    {
        DrawLineHelper(dirX , dirY ,distance , distance);
        for(int i = 1 ; i <= ((PincelSize - 1) / 2 );i++)
        {
            Console.WriteLine("PININI");
          int NewRow = Row + dirX ;
          int NewColum = Colum + dirY;
          if(!Canva.IsOutRange(NewColum,NewRow))
          {
          Spawn(NewColum,NewRow);
          DrawLineHelper(dirX , dirY ,distance , distance);
          }
          NewRow = Colum * -1;
          NewColum =Row  * -1;
          if(!Canva.IsOutRange(NewColum,NewRow))
          {
          Spawn(NewColum,NewRow);
          DrawLineHelper(dirX , dirY ,distance , distance);
          }
        }
       Spawn(Colum + dirX * distance , Row + dirY * distance);
    }
    static void DrawLineHelper (int dirX, int dirY, int distance ,int MoveDoing)
    {
        if(MoveDoing != 0)
        {
            int RowSum =  Row + dirY * (distance - MoveDoing);
            int ColumSum = Colum + dirX * (distance - MoveDoing);
            if(!Canva.IsOutRange(ColumSum,RowSum))
            {
             Canva.canvas[ColumSum , RowSum] = PincelColor;
             DrawLineHelper(dirX , dirY ,distance ,MoveDoing-1);
            }
        }
    } 
    public static void DrawCircle(int dirX, int dirY,int radius)
    {
        
    }
}