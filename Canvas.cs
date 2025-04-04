
public class Canva
{
    /// <summary>
    /// Canvas  panel to save the actual colors in the diferents positions
    /// </summary>
    public static string?[,] canvas { get; private set; }
    /// <summary>
    /// Comprobate if the position selected is out of the dimension of the actual canvas
    /// </summary>
    /// <param name="x">Position of the column</param>
    /// <param name="y">Position of the row</param>
    /// <returns></returns>
    public static bool IsOutRange(int x, int y)
    {
        if (x >= canvas.Length || y >= canvas.Length || x < 0 || y < 0) return true;
        return false;
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
    static void InitInWhite(string?[,] WhiteCanvas)
    {
        for (int i = 0; i < WhiteCanvas.GetLength(1); i++)
        {
            for (int j = 0; j < WhiteCanvas.GetLength(0); j++)
            {
                WhiteCanvas[i, j] = "White";
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
        int MaxDim = Math.Max(dim, canvas.Length);
        for (int i = 0; i < MaxDim; i++)
        {
            for (int j = 0; j < MaxDim; j++)
            {
                NewCanvas[i, j] = canvas[i, j];
            }
            canvas = NewCanvas;
        }
    }
}