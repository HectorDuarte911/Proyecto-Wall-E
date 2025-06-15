namespace WALLE;

public class Canva
{
    /// <summary>
    /// Save the state of the canvas
    /// </summary>
    protected static string[,]? canvas;
    /// <summary>
    /// Comprove if the position is aout of the canvas size
    /// </summary>
    public static bool IsOutRange(int x, int y)=> x >= canvas!.GetLength(0) || y >= canvas.GetLength(0) || x < 0 || y < 0;
    /// <summary>
    /// Get the actual size of the canvas
    /// </summary>
    public static int GetCanvasSize() => canvas!.GetLength(0);
    /// <summary>
    /// See the number of pixel of one color in a rectangle area
    /// </summary>\
    public static int GetColorCount(string? color, int x1, int y1, int x2, int y2)
    {
        int MaxX = Math.Max(x1, x2); int MinX = Math.Min(x1, x2);
        int MaxY = Math.Max(y1, y2); int MinY = Math.Min(y1, y2);
        int count = 0;
        for (int i = MinX; i <= MaxX; i++)
        {
            for (int j = MinY; j <= MaxY; j++)
            {
                if (!IsOutRange(i, j) && canvas![i, j] == color) count++;
            }
        }
        return count;
    }
    /// <summary>
    /// Comprove is the pixel in the position have the same introduce color
    /// </summary>
    public static bool IsCanvasColor(string? color, int vertical, int horizontal) => canvas![Walle.GetActualX() + vertical, Walle.GetActualY() + horizontal] == color;
    /// <summary>
    /// Create a new predeterminate canvas
    /// </summary>
    public static void InitCanvas()
    {
        canvas = new string[50, 50];
        InitInWhite(canvas);
    }
    /// <summary>
    /// Paint all the canvas to predeterminate color
    /// </summary>
    private static void InitInWhite(string?[,] WhiteCanvas)
    {
        for (int i = 0; i < WhiteCanvas.GetLength(1); i++)
        {
            for (int j = 0; j < WhiteCanvas.GetLength(0); j++)
            {
                WhiteCanvas[i, j] = "Transparent";
            }
        }
    }
    /// <summary>
    /// Redimension the size of the canvas 
    /// </summary>
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
        }
        canvas = NewCanvas!;
    }
    /// <summary>
    /// Get the color of the cell in the position
    /// </summary>
    public static string GetCellColor(int x, int y) => canvas![x, y];
    /// <summary>
    /// Set a cell color in the position
    /// </summary>
    public static void SetCellColor(int x, int y, string color)
    {
        if (!IsOutRange(x, y) && canvas != null) canvas[y, x] = color;
    }
}