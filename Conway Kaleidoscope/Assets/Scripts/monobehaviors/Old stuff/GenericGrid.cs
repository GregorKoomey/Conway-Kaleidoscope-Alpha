//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
//using Unity.Mathematics;

public class Grid<V> where V: struct
{
    public int ColumnCount;
    public int RowCount;

    public V[] Values;

    public Grid (int colCount, int rowCount, V defaultValue)
    {
        this.ColumnCount = colCount;
        this.RowCount = rowCount;
        this.Values = new V[colCount * rowCount];

        SetAllValues(defaultValue);
    }

    public void SetState(V[] values)
    {
        for (int i = 0; i < Values.Length; i++)
        {
            if (i >= values.Length)
                break;
            else
                Values[i] = values[i];
        }
    }

    public void SetSubState(int originX, int originY, int subColumns, int subRows, V[] values)
    {


    }

    //test these two methods (should they swap logic and/or is there more?)
    public int XForRawIndex(int rawIndex)
    {
        int returnValue = rawIndex % ColumnCount;
        return returnValue;
    }

    public int YForRawIndex(int rawIndex)
    {
        int returnValue = rawIndex % RowCount;
        return returnValue;
    }

    
    
    public V ValueFor(int rawIndex)
    {
        return Values[rawIndex];
    }

    public V ValueFor(int x, int y){
        return ValueFor(RawIndexFor(x , y));
    }

    public void SetValueFor(int rawIndex, V value)
    {
        Values[rawIndex] = value;
    }

    public void SetValueFor(int x, int y, V value)
    {
        SetValueFor(RawIndexFor(x, y), value);
    }

    public void SetAllValues(V value)
    {
        for (int i = 0; i < Values.Length; i++)
        {
            Values[i] = value;
        }
    }

    public int RawIndexFor(int x, int y)
    {
        int adjustedX;
        int adjustedY;

        if (x < 0)
            adjustedX = 0;
        else
        if (x > ColumnCount - 1)
            adjustedX = ColumnCount;
        else
            adjustedX = x;
        if (y < 0)
            adjustedY = 0;
        else
        if (y > RowCount - 1)
            adjustedY = RowCount;
        else
            adjustedY = y;

//        int cellIndex = (x * rowCount) + y;
        int cellIndex = (adjustedX * RowCount) + adjustedY;

        Debug.Log("Grid cellxy: " + x + " " + y + " rawIndex: " + cellIndex + "cells.Length: " + Values.Length);

        return cellIndex;
    }
    
    

}
