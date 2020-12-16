using UnityEngine;

public struct BrushFactory 
{
    public static CellOffset[] Appended(CellOffset[] firstBrush, CellOffset[] secondBrush)
    {
        CellOffset[] returnValue = new CellOffset[firstBrush.Length + secondBrush.Length];

        for (int i = 0; i < firstBrush.Length; i++)
        {
            returnValue[i] = firstBrush[i];
        }

        for (int i = 0; i < secondBrush.Length; i++)
        {
            returnValue[firstBrush.Length + i] = secondBrush[i];
        }
        return returnValue;
    }

    public static CellOffset[] Translated(CellOffset[] source, int x, int y)
    {
        CellOffset[] returnValue = new CellOffset[source.Length];

        for (int i = 0; i < source.Length; i++)
        {
            returnValue[i] = source[i].Translated(x, y);
        }
        return returnValue;
    }
    
    public static int WidthForBrush(CellOffset[] brush)
    {
        int smallestX = brush[0].xOffset;
        int largestX = brush[0].xOffset;

        for (int i = 0; i < brush.Length; i++)
        {
            int currentXOffset = brush[i].xOffset; 
            if (currentXOffset < smallestX)
                smallestX = brush[i].xOffset;
            if (currentXOffset > largestX)
                largestX = brush[i].xOffset;
        }
        return largestX - smallestX + 1;
    }

    public static int HeightForBrush(CellOffset[] brush)
    {
        int smallestY = brush[0].yOffset;
        int largestY = brush[0].yOffset;

        for (int i = 0; i < brush.Length; i++)
        {
            int currentYOffset = brush[i].yOffset; 
            if (currentYOffset < smallestY)
                smallestY = brush[i].yOffset;
            if (currentYOffset > largestY)
                largestY = brush[i].yOffset;
        }
        return largestY - smallestY + 1;
    }
    
    

    public static CellOffset[] HorizontalTiledBrush(int boundsX, int adjustment, CellOffset[] brush)
    {
        int adjustedWidth = WidthForBrush(brush) * adjustment;

        int newBrushesCount = boundsX / adjustedWidth;

        if (newBrushesCount < 2)
        {
            return brush;
        }
        
        CellOffset[] returnValue = new CellOffset[newBrushesCount*brush.Length];

        for (int i = 0; i < newBrushesCount; i++)
        {
            for (int j = 0; j < brush.Length; j++)
            {
                returnValue[i*brush.Length + j] = brush[j].Translated(i * adjustedWidth, 0);
            }
        }

        return returnValue;
    }

    public static CellOffset[] VerticalTiledBrush(int boundsY, int adjustment, CellOffset[] brush)
    {
        int adjustedHeight = HeightForBrush(brush) * adjustment;

        Debug.Log("brush height: " + HeightForBrush(brush) + " adjustedHeight: " +adjustedHeight);
        
        int newBrushesCount = boundsY / adjustedHeight;

        if (newBrushesCount < 2)
        {
            return brush;
        }
        
        CellOffset[] returnValue = new CellOffset[newBrushesCount*brush.Length];

        for (int i = 0; i < newBrushesCount; i++)
        {
            for (int j = 0; j < brush.Length; j++)
            {
                returnValue[i*brush.Length + j] = brush[j].Translated(0, i * adjustedHeight);
            }
        }
        return returnValue;    
    }

    
    public static CellOffset[] TiledBrush(int boundsX, int boundsY, int adjustment, CellOffset[] brush)
    {
        return VerticalTiledBrush(boundsY, adjustment, HorizontalTiledBrush(boundsX, adjustment, brush));
    }
    
    
    public static CellOffset[] SingleCell()
    {
        CellOffset[] gliderPattern =
        {
            new CellOffset(0, 0)
        };
        return gliderPattern;
    }
    public static CellOffset[] SimpleGlider()
    {
        CellOffset[] gliderPattern =
        {
            new CellOffset(0, 0),
            new CellOffset(1, 0),
            new CellOffset(1, 1),
            new CellOffset(2, 1),
            new CellOffset(0, 2)
        };
      return gliderPattern;
    }
    
    public static CellOffset[] Acorn()
    {
        CellOffset[] gliderPattern =
        {
            new CellOffset(1, 0),
            new CellOffset(3, 1),
            new CellOffset(0, 2),
            new CellOffset(1, 2),
            new CellOffset(4, 2),       
            new CellOffset(5, 2),
            new CellOffset(6, 2)
        };
        return gliderPattern;
    }
    
    public static CellOffset[] RPentomino()
    {
        CellOffset[] gliderPattern =
        {
            new CellOffset(1, 0),
            new CellOffset(0, 1),
            new CellOffset(1, 1),
            new CellOffset(1, 2),
            new CellOffset(2, 2)       
        };
        return gliderPattern;
    }
    
    public static CellOffset[] Blinker()
    {
        CellOffset[] gliderPattern =
        {
            new CellOffset(0, 0),
            new CellOffset(0, 1),
            new CellOffset(0, 2)
        };
        return gliderPattern;
    }
    
    public static CellOffset[] Toad()
    {
        CellOffset[] gliderPattern =
        {
            new CellOffset(0, 0),
            new CellOffset(1, 0),
            new CellOffset(2, 0),
            new CellOffset(1, 1),
            new CellOffset(2, 1),
            new CellOffset(3, 1)
        };
        return gliderPattern;
    }
    
    public static CellOffset[] SolidThreeBlock()
    {
        CellOffset[] gliderPattern =
        {
            new CellOffset(0, 0),
            new CellOffset(1, 0),
            new CellOffset(2, 0),
            new CellOffset(0, 1),
            new CellOffset(1, 1),
            new CellOffset(2, 1),
            new CellOffset(0, 3),
            new CellOffset(1, 3),
            new CellOffset(2, 3)
        };
        return gliderPattern;
    }
    
    public static CellOffset[] GliderGun()
    {
        CellOffset[] gliderPattern =
        {
            new CellOffset(0, 3),
            new CellOffset(0, 4),
            new CellOffset(1, 3),
            new CellOffset(1, 4),
            new CellOffset(10, 2),
            new CellOffset(10, 3),
            new CellOffset(10, 4),
            new CellOffset(11, 1),
            new CellOffset(11, 5),
            new CellOffset(12, 0),
            new CellOffset(12, 6),
            new CellOffset(13, 0),
            new CellOffset(13, 6),
            new CellOffset(14, 3),
            new CellOffset(15, 1),
            new CellOffset(15, 5),
            new CellOffset(16, 2),
            new CellOffset(16, 3),
            new CellOffset(16, 4),
            new CellOffset(17, 3),
            new CellOffset(20, 4),
            new CellOffset(20, 5),
            new CellOffset(20, 6),
            new CellOffset(21, 4),
            new CellOffset(21, 5),
            new CellOffset(21, 6),
            new CellOffset(22, 3),
            new CellOffset(22, 7),
            new CellOffset(24, 2),
            new CellOffset(24, 3),
            new CellOffset(24, 7),
            new CellOffset(24, 8),
            new CellOffset(34, 5),
            new CellOffset(34, 6),
            new CellOffset(35, 5),
            new CellOffset(35, 6)
        };
        return gliderPattern;
    }

}
