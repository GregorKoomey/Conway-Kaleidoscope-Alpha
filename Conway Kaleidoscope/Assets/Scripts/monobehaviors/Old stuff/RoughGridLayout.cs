//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
using UnityEngine;

public class RoughGridLayout : MonoBehaviour
{
    public GameObject cellObject;

    public int rowCount= 2;
    public int columnCount = 2;
    
    public GameObject[] cells;

    private Color[] colors;
    
    // Start is called before the first frame update
    void Start()
    {
        cells = new GameObject[rowCount * columnCount];
        GenerateGrid();
        this.transform.Translate(-columnCount * .5f, -rowCount * .5f, 0);
        
        colors = ColorFactory.GrayScaleReversed(5);

        foreach (var color in colors)
        {
            Debug.Log("color:" + color);
            
        }
        
    }

    public Vector3 OriginVector()
    {
        return new Vector3( -columnCount * .5f, -rowCount * .5f, 0f);
    }

    public int RawIndexFor(int x, int y)
    {
        int adjustedX;
        int adjustedY;

        if (x < 0)
            adjustedX = 0;
        else
            if (x > columnCount - 1)
                adjustedX = columnCount;
            else
                adjustedX = x;
        if (y < 0)
            adjustedY = 0;
        else
            if (y > rowCount - 1)
                adjustedY = rowCount;
            else
                adjustedY = y;

        int cellIndex = (adjustedX * rowCount) + adjustedY;

        Debug.Log("cellxy: " + x + " " + y + " rawIndex: " + cellIndex + "cells.Length: " + cells.Length);

        return cellIndex;
    }

    public GameObject CellFor(int x, int y){
        return cells[RawIndexFor(x , y)];
    }

    public GameObject CellFor(int index)
    {
        return cells[index];
    }

    public void SetCellColor(GameObject cell, Color color)
    {
        SpriteRenderer tempSr = cell.GetComponent<SpriteRenderer>();
        tempSr.color = color;
    }

    public void SetCellColorIndex(int rawIndex, int colorIndex)
    {
        GameObject cell = CellFor(rawIndex);
        Color cellColor;

        int colorsLength = colors.Length;

        if (colorsLength < colorIndex)
            cellColor = colors[colorsLength - 1];
        else if (colorIndex < 0)
            cellColor = colors[0];
        else
            cellColor = colors[colorIndex];
        
        SetCellColor(cell, cellColor);


    }

    private void GenerateGrid()
    {
        Transform myTransform = this.transform;

        int cellIndex = 0;

        for (int x = 0; x < columnCount; x++)
        {
            for (int y = 0; y < rowCount; y++)
            {

                Vector3 tempVec = new Vector3(x, y, 0f);
                
                Quaternion tempQuat = Quaternion.identity;

                GameObject tempCell = Instantiate(cellObject, tempVec, tempQuat, myTransform);

                SpriteRenderer sr = tempCell.GetComponent<SpriteRenderer>();
                
                    
                    
                    
                cells[cellIndex] = tempCell;
                cellIndex += 1;
            }
        }

        GameObject temp = CellFor(0, 3);
        SetCellColor(temp, Color.black);

        temp = CellFor(15, 10);
        SetCellColor(temp, Color.blue);
        
    }

}
