namespace WALLE;
public class Walle : Canva
{
    private static int Colum;
    private static int Row;
    private static int PincelSize;
    private static string? PincelColor;
    public static void Spawn(int x, int y)
    {
        if (IsOutRange(x, y)) throw new Exception("Is out of range look at it");
        Row = y;
        Colum = x;
    }
    public static void Size(int k)
    {
        if (k % 2 == 0) k --;
        PincelSize = k;
    }
    public static void Color(string color)
    {
        PincelColor = color;
    }
    public static int GetActualX()
    {
        return Colum;
    }
    public static int GetActualY()
    {
        return Row;
    }
    public static bool IsBrushColor(string? color)
    {
        return color == PincelColor;
    }
    public static bool IsBrushSize(int size)
    {
        return size == PincelSize;
    }
    public static void DrawLine(int dirX, int dirY, int distance)
    {
        DrawSquare(dirX,dirY,distance); 
    }
    private static void DrawSquare(int dirX, int dirY,int distance)
    {
        if(distance != 0)
        {
        int radius = PincelSize / 2; 
        for(int i = Colum - radius ; i <= Colum + radius ;i++)
        {
            for(int j = Row - radius ; j <=  Row + radius;j++)
            {
              if(!IsOutRange(i,j))canvas![i,j] = PincelColor!;
            }
        }
        if(!IsOutRange(Colum + dirX,Row + dirY))Spawn(Colum + dirX,Row + dirY);
        DrawSquare(dirX,dirY,distance - 1);
        }
    }
    public static void DrawCircle(int dirX, int dirY, int radius)
    {  
        if(radius % 2 == 0)radius--;
        int centerX = dirX * radius + Colum;
        int centerY = dirY * radius + Row;
        int[] x = {1,-1,0, 0,1,-1, 1,-1};
        int[] y = {0, 0,1,-1,1,-1,-1, 1};
        for(int i =0 ;i < 8 ;i++)
        {
            DrawBorder(centerX,centerY,radius,x[i],y[i]);
        }
        if(!IsOutRange(centerX,centerY))Spawn(centerX,centerY);
    }
    private static void DrawBorder(int centerX , int centerY , int radius,int dirx,int dirY)
    {
        int newX = 0 , newY = 0;
             if(dirx ==  1 && dirY == 0){newX = centerX + radius; newY = centerY + radius / 2;}
        else if(dirx == -1 && dirY == 0){newX = centerX - radius; newY = centerY - radius / 2;}
        else if(dirY ==  1 && dirx == 0){newY = centerY + radius; newX = centerX + radius / 2;}
        else if(dirY == -1 && dirx == 0){newY = centerY - radius; newX = centerX - radius / 2;}
        else if(dirx ==  1 && dirY ==  1){newX = centerX + radius - 1; newY = centerY - radius / 2 - 1;radius = radius / 2;}
        else if(dirx == -1 && dirY == -1){newX = centerX - radius + 1; newY = centerY + radius / 2 + 1;radius = radius / 2;}
        else if(dirY ==  1 && dirx == -1){newY = centerY - radius + 1; newX = centerX - radius / 2 - 1;radius = radius / 2;}
        else if(dirY == -1 && dirx ==  1){newY = centerY + radius - 1; newX = centerX + radius / 2 + 1;radius = radius / 2;}
       for(int i = 0 ; i < radius;i++)
        {  
            if(i != 0)
            {
            newX -= dirY;
            newY -= dirx;
            }
            if(!IsOutRange(newX,newY))
            {
               Spawn(newX,newY);
               DrawLine(-dirY ,-dirx,radius -i);
               break;
            }
        }
    }
    public static void DrawRectangle(int dirX, int dirY, int distance, int width, int height)
    {
        if(width % 2 == 0)width--;if(height % 2 == 0)height--;
        int centerX = dirX * distance + Colum;
        int centerY = dirY * distance + Row;
        int[] x = {1,-1,0, 0};
        int[] y = {0, 0,1,-1};
        for (int i = 0; i < 4; i++)
        {
           DrawBorderRect(centerX,centerY,width,height,x[i],y[i]); 
        } 
         if(!IsOutRange(centerX,centerY))Spawn(centerX,centerY);
    }
    private static void DrawBorderRect(int centerX,int centerY,int width,int height,int dirX , int dirY)
    {
        int newX = 0, newY = 0 , radius = 0;
             if(dirX ==  1){newX = centerX + width / 2 + 1 ; newY = centerY + height / 2 + 1;radius = height;}
        else if(dirX == -1){newX = centerX - width / 2 - 1 ; newY = centerY - height / 2 - 1;radius = height;}
        else if(dirY ==  1){newY = centerY - height / 2 - 1; newX = centerX + width / 2 + 1;radius = width;}
        else if(dirY == -1){newY = centerY + height / 2 + 1; newX = centerX - width / 2 - 1;radius = width;}
        
        for(int i = 0 ; i < radius + 1;i++)
        {   
        if(i!=0)
        {
        newX -= dirY;
        newY -= dirX;
        }
            if(!IsOutRange(newX,newY))
            {
               Spawn(newX,newY);
               DrawLine(-dirY ,-dirX,radius + 1 - i );
               break;
            }
        }
    }
    public static void Fill()
    {
     bool[,] mask = new bool[canvas.GetLength(0),canvas.GetLength(1)];
     string color = canvas[Colum,Row];
     mask = FillHelper(mask,Colum,Row,color);
     for (int i = 0; i < canvas.GetLength(0); i++)
     {
        for (int j = 0; j < canvas.GetLength(1); j++)
        {
            if(mask[i,j])canvas[i,j] = PincelColor;
        }
     }
    }
    private static bool[,] FillHelper(bool[,]mask,int col , int row,string color)
    {
        mask[col,row] = true;
        int [] x = {1,0, 0,-1};
        int [] y = {0,1,-1, 0};
        for (int i = 0; i < 4 ; i++)
        {
            int newcol = col + x[i];
            int newrow = row + y[i];
            if(!IsOutRange(newcol,newrow))
            {
                if(!mask[newcol,newrow] && canvas[newcol,newrow] == color)
                {
                  mask[newcol,newrow] = true;
                  mask = FillHelper(mask,newcol,newrow,color);
                }
            }
        }
        return mask;
    }
}   