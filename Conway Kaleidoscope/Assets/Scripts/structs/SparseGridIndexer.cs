//intended as external to source nativeArray or array of columncount* rowcount length
public struct SparseGridIndexer
{
    public readonly int columnCount;
    public readonly int rowCount;

//    potential error if indexed reference data mismatches with count values
    public SparseGridIndexer(int colCount, int rowCount)
    {
        this.columnCount = colCount;
        this.rowCount = rowCount;
    }

    public int RawCount()
    {
        return columnCount * rowCount;
    }

    //test these two methods (should they swap logic and/or is there more?)
    public int XForRawIndex(int rawIndex)
    {
        int returnValue = rawIndex % columnCount;
        return returnValue;
    }

    public int YForRawIndex(int rawIndex)
    {
        int returnValue = rawIndex % rowCount;
        return returnValue;
    }
 
    public int RawIndexFor(int x, int y)
    {
        int adjustedX;
        int adjustedY;

        if (x < 0)
            adjustedX = 0;
        else
        if (x > (columnCount - 1))
            adjustedX = columnCount - 1;
        else
            adjustedX = x;
        if (y < 0)
            adjustedY = 0;
        else
        if (y > (rowCount - 1))
            adjustedY = rowCount - 1;
        else
            adjustedY = y;


       int cellIndex = adjustedX  + (adjustedY * columnCount);

        return cellIndex;
    }

    public static int BoundedIndexFor(int query, ushort bounds)
    {
        if ((query < 0) || (bounds == 0))
            return 0;

        if (query >= (int) bounds)
            return (int) bounds - 1;

        return query;
    }

    public static int WrappedIndexFor(int query, ushort bounds)
    {
        if ((0 <= query) && (query < bounds))
        {
            return query;
        }

        if (query < 0)
        {
            int adjustedQuery = query;
            if ((0 - adjustedQuery) > bounds)
            {
                adjustedQuery = query % bounds;
            }

            return bounds + adjustedQuery;
        }
        
        return query % bounds;
    }

    public static int MirroredIndexFor(int query, ushort bounds)
    {
        return bounds - 1 - query;
    }

}

/*
 void testWrappedIndex() {
        int[] testValues = {-9, -8, -7, -6, -5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9};

        ushort boundsLimit = 5;

        for (int i = 0; i < testValues.Length; i++)
        {
            int testValue = testValues[i];
            Debug.Log("testValue: " + testValue + " bounds: " +boundsLimit + " Result: " + SparseGridIndexer.wrappedIndexFor(testValue, boundsLimit));
        }
        }
 */


