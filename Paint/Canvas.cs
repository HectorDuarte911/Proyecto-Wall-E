
using System.Drawing;
using Spectre.Console;

public class Canva
{
/// <summary>
    /// Canvas  panel to save the actual colors in the diferents positions
    /// </summary>
protected static string [,]? canvas ;
/// <summary>
/// Comprobate if the position selected is out of the dimension of the actual canvas
/// </summary>
/// <param name="x">Position of the column</param>
/// <param name="y">Position of the row</param>
/// <returns></returns>
protected static bool IsOutRange(int x, int y)
{
    if (x >= canvas!.Length || y >= canvas.Length || x < 0 || y < 0) return true;
    return false;
}
public static int GetCanvasSize()
{
return canvas!.GetLength(0);
}
public static int GetColorCount(string? color,int x1, int y1,int x2, int y2)
{
    int MaxX = Math.Max(x1,x2);int MinX = Math.Min(x1,x2);
    int MaxY = Math.Max(y1,y2);int MinY = Math.Min(y1,y2);
    int count = 0;
     for (int i = MinX; i <= MaxX; i++)
    {
        for (int j = MinY; j <= MaxY; j++)
        {
            if(!IsOutRange(i,j) && canvas![i,j] == color)count++;
        }
    }
    return count;
}
public static bool IsCanvasColor(string ? color , int vertical , int horizontal)
{
    return canvas![Walle.GetActualX() + vertical , Walle.GetActualY() + horizontal] == color;
} 
/// <summary>
/// Initialisation of the predeterminate canvas
/// </summary>
public static void InitCanvas()
{
canvas = new string[50, 50];
InitInWhite(canvas);
}
/// <summary>
/// Put the canvas in white
/// </summary>
/// <param name="WhiteCanvas">canvas to transform to white</param>
private static void InitInWhite(string?[,] WhiteCanvas)
{
    for (int i = 0; i < WhiteCanvas.GetLength(1); i++)
    {
        for (int j = 0; j < WhiteCanvas.GetLength(0); j++)
        {
            WhiteCanvas[i,j] = "White";
        }
        }
    }
/// <summary>
    /// Redimension the canvas to other biger or smaller 
    /// </summary>
    /// <param name="dim">The square dimension of the new canvas</param>
public static void RedimensionCanvas(int dim)
{
    string?[,] NewCanvas = new string[dim, dim];
    InitInWhite(NewCanvas);
    int MinDim = Math.Min(dim, canvas!.GetLength(0));
    for (int i = 0; i < MinDim; i++)
    {
        for (int j = 0; j < MinDim; j++)
        {
            NewCanvas[i, j] = canvas[i, j];
        }
        canvas = NewCanvas!;
    }
}
//View example
// public static void Main(string[]args)
// {
//     InitCanvas();
//     Walle.Size(1);
//     Walle.Spawn(3,3);
//     Walle.Color("red");
//     RedimensionCanvas(20);
//     Walle.DrawRectangle(1,1,4,5,9);
//     string color = string.Empty;
//     for (int i = 0; i < 20; i++)
//     {
//         for (int j = 0; j < 20; j++)
//         {   
//             if(canvas![j,i] == "red")
//             {
//                 color += $"[DarkRed]RED[/] ";
//             }
//             else color += "WHT ";
//         }
//         color += '\n';
//     }
//     AnsiConsole.Markup(color);
// }
}