using Unity.Collections;
using UnityEngine;

public class ConwayDisplayGrid : MonoBehaviour
{
    [SerializeField]
    private GameObject cellObject;

    private SparseGridIndexer gridIndexer;
    private NativeArray<byte> state;
    private NativeArray<byte> backupState;
    private Color[] colors;
    
    private GameObject[] cells;
    private SpriteRenderer[] cellRenderers;
    
    public void InitializeData(SparseGridIndexer index, NativeArray<byte> displayState, Color[] colorarray)
    {
        gridIndexer = index;
        state = displayState;
        colors = colorarray;
        backupState = new NativeArray<byte>(state.Length, Allocator.Persistent);
    //    backupState.CopyFrom(state);
        
        cells = new GameObject[gridIndexer.RawCount()];
        cellRenderers = new SpriteRenderer[gridIndexer.RawCount()];
       GenerateGrid();
    }

    public void UpdateDisplay()
    {
        for (int i = 0; i < state.Length; i++)
        {
 //           Debug.Log("UpdateDisplay cellIndex: " + i + " colorIndex: " + state[i]);
            if (backupState[i] != state[i])
                SetCellColorIndex(i, state[i]);    
        }
        backupState.CopyFrom(state);
    }
    
    private GameObject CellFor(int x, int y){
        return cells[gridIndexer.RawIndexFor(x , y)];
    }

    private GameObject CellFor(int index)
    {
        return cells[index];
    }
    
    private void SetCellColor(GameObject cell, Color color)
    {
        SpriteRenderer tempSr = cell.GetComponent<SpriteRenderer>();
        tempSr.color = color;
    }
    
    private void SetCellColorIndex(int rawIndex, int colorIndex)
    {
        GameObject cell = CellFor(rawIndex);
        Color cellColor;
        int colorsLength = colors.Length;
        int adjustedColorIndex = SparseGridIndexer.BoundedIndexFor(state[rawIndex], (ushort) colorsLength);
        
 //       Debug.Log("rawIndex: " + rawIndex + " adjustedIndex: " + adjustedColorIndex + " colorsLength: " + colorsLength);
        cellColor = colors[adjustedColorIndex];
        
        cellRenderers[rawIndex].color = cellColor;
    }
    
    
    public Vector3 OriginVector()
    {
        return new Vector3( -gridIndexer.columnCount * .5f, -gridIndexer.rowCount * .5f, 0f);
    }
    
    private void GenerateGrid()
    {
        //note: this refers to the transform of GameObject to which this script attaches
        Transform myTransform = this.transform;
        int cellIndex = 0;
        
        for (int y = 0; y < gridIndexer.rowCount; y++)
        {
            for (int x = 0; x < gridIndexer.columnCount; x++)
            {
                Vector3 tempVec = new Vector3(x, y, 0f);
                Quaternion tempQuat = Quaternion.identity;
                GameObject tempCell = Instantiate(cellObject, tempVec, tempQuat, myTransform);
                //SpriteRenderer sr = tempCell.GetComponent<SpriteRenderer>();
                cells[cellIndex] = tempCell;
                cellRenderers[cellIndex] = tempCell.GetComponent<SpriteRenderer>();
                cellIndex += 1;
            }
        }
        
        UpdateDisplay();
    }

    private void OnDestroy()
    {
        backupState.Dispose();
    }
}
