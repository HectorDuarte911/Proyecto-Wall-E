
public class Canvas
{
   public static string ?[,] canvas = new string [100,100]; 

   public static bool IsOutRange(int x , int y)
   {
    if(x >= canvas.Length || y >= canvas.Length || x < 0 || y < 0)return true;
    return false;
   }
   public static void InitCanvas()
   {
    for (int i = 0; i < canvas.Length; i++)
    {
        for (int j = 0; j < canvas.Length; j++)
        {
            canvas[i,j] = "White";
        }
    }
   }
   public static void RedimensionCanvas(int dim)
   {
    string? [,] NewCanvas = new string[dim,dim];
    int MaxDim= Math.Max(dim,canvas.Length);
    for(int i =0 ; i < MaxDim ;i++)
    {
        for(int j =0 ;j < MaxDim;j++)
        {
            NewCanvas[i,j] = canvas[i,j];
        }
        canvas = NewCanvas;
    }
   }
}