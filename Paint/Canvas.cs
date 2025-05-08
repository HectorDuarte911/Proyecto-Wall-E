namespace WALLE;

public class Canva
{
    protected static string[,]? canvas;
    public static bool IsOutRange(int x, int y)
    {
        return x >= canvas!.GetLength(0) || y >= canvas.GetLength(0) || x < 0 || y < 0;
    }
    public static int GetCanvasSize()
    {
        return canvas!.GetLength(0);
    }
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
    public static bool IsCanvasColor(string? color, int vertical, int horizontal)
    {
        return canvas![Walle.GetActualX() + vertical, Walle.GetActualY() + horizontal] == color;
    }
    public static void InitCanvas()
    {
        canvas = new string[50, 50];
        InitInWhite(canvas);
    }
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
    public static string GetCellColor(int x, int y)
    {
        return canvas![x, y];
    }
    public static void SetCellColor(int x, int y, string color)
    {
        if (!IsOutRange(x, y) && canvas != null) canvas[y, x] = color;
    }
}