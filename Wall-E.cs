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
        if (k % 2 == 0) k = k - 1;
        PincelSize = k;
    }
    public static void Color(string color)
    {
        PincelColor = color;
    }
    public static void DrawLine(int dirX, int dirY, int distance)
    {
        if (dirX == 0 || dirY == 0) DrawLineVerHor(dirX, dirY, distance);
        else DrawLineCurve(dirX, dirY, distance);
    }
    static void DrawLineCurve(int dirX, int dirY, int distance)
    {
        for (int i = 0; i < distance; i++)
        {
            int size = PincelSize;
            if (i == distance - 1) size = PincelSize - 1;
            DrawLineHelper(dirX, 0, size, size);
            DrawLineHelper(0, dirY, size, size);
            int NewColum = Colum + dirX;
            int NewRow = Row + dirY;
            if (!IsOutRange(NewColum, NewRow))
            {
                Spawn(NewColum, NewRow);
            }
        }
    }
    static void DrawLineVerHor(int dirX, int dirY, int distance)
    {
        DrawLineHelper(dirX, dirY, distance, distance);
        for (int i = 1; i <= ((PincelSize - 1) / 2); i++)
        {
            int NewRow = Row + dirX;
            int NewColum = Colum + dirY;
            if (!IsOutRange(NewColum, NewRow))
            {
                Spawn(NewColum, NewRow);
                DrawLineHelper(dirX, dirY, distance, distance);
            }
            NewRow = Colum * -1;
            NewColum = Row * -1;
            if (!IsOutRange(NewColum, NewRow))
            {
                Spawn(NewColum, NewRow);
                DrawLineHelper(dirX, dirY, distance, distance);
            }
        }
        Spawn(Colum + dirX * distance, Row + dirY * distance);
    }
    static void DrawLineHelper(int dirX, int dirY, int distance, int MoveDoing)
    {
        if (MoveDoing != 0)
        {
            int RowSum = Row + dirY * (distance - MoveDoing);
            int ColumSum = Colum + dirX * (distance - MoveDoing);
            if (!IsOutRange(ColumSum, RowSum))
            {
                canvas![ColumSum, RowSum] = PincelColor!;
                DrawLineHelper(dirX, dirY, distance, MoveDoing - 1);
            }
        }
    }
    public static void DrawCircle(int dirX, int dirY, int radius)
    {
        int centerX = dirX * radius + Colum;
        int centerY = dirY * radius + Row;
        if (dirX != 0 && dirY != 0)
        {
            centerX--; centerY--;
        }
    }
    public static void DrawRectangle(int dirX, int dirY, int distance, int width, int height)
    {
        int centerX = dirX * distance + Colum;
        int centerY = dirY * distance + Row;
        if (dirX != 0 && dirY != 0)
        {
            centerX--; centerY--;
        }
        if (width % 2 == 0) width++;
        if (height % 2 == 0) height++;
        for (int j = centerY - height; j <= centerY + height; j += 2 * height)
        {
            if (!IsOutRange(0, j)) break;
            for (int i = centerX - width; i <= centerX + width; i++)
            {
                if (!IsOutRange(i, j)) canvas![i, j] = PincelColor!;
            }
        }
        for (int j = centerX - width; j <= centerX + width; j += 2 * width)
        {
            if (!IsOutRange(j, 0)) break;
            for (int i = centerY - height; i <= centerY + height; i++)
            {
                if (!IsOutRange(j, i)) canvas![j, i] = PincelColor!;
            }
        }
        Spawn(centerX, centerY);
    }
    public static void Fill()
    {
        string? colorfill = canvas![Colum, Row];
        if (colorfill != PincelColor)
        {
            bool[,] mask = new bool[canvas.GetLength(0), canvas.GetLength(1)];
            FillAssistent(mask, colorfill, false);
            for (int i = 0; i < mask.GetLength(0); i++)
            {
                for (int j = 0; j < mask.GetLength(0); j++)
                {
                    if (mask[i, j]) canvas[i, j] = PincelColor!;
                }
            }
        }
    }
    private static void FillAssistent(bool[,] mask, string? colorfill, bool flag)
    {

        if (flag || CanFill(mask, colorfill, Colum, Row))
        {
            int[] dirx = { 0, 1, 0, -1 };
            int[] diry = { 1, 0, -1, 0 };
            for (int i = 0; i < 4; i++)
            {
                int newx = Colum + dirx[i];
                int newy = Row + diry[i];
                if (!IsOutRange(newx, newy))
                {
                    if (canvas![newx, newy] == colorfill)
                    {
                        Spawn(newx, newy);
                        mask[newx, newy] = true;
                        break;
                    }
                }
            }
        }
        else
        {
            bool flagfor = false;
            for (int i = 0; i < mask.GetLength(0); i++)
            {
                for (int j = 0; j < mask.GetLength(0); j++)
                {
                    if (mask[i, j])
                    {
                        if (CanFill(mask, colorfill, i, j))
                        {
                            Spawn(i, j);
                            break;
                        }
                    }
                }
                if (flagfor) break;
            }
            if (flagfor) FillAssistent(mask, colorfill, true);
        }
    }
    private static bool CanFill(bool[,] mask, string? colorfill, int x, int y)
    {
        int[] dirx = { 0, 1, 0, -1 };
        int[] diry = { 1, 0, -1, 0 };
        for (int i = 0; i < 4; i++)
        {
            int newx = Colum + dirx[i];
            int newy = Row + diry[i];
            if (canvas![newx, newy] == colorfill && !mask[newx, newy]) return true;
        }
        return false;
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

}