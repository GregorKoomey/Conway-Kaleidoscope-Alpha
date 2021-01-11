using System;
using Unity.Collections;
using UnityEngine;
using TMPro;
using Unity.Burst;
using Unity.Jobs;
using Unity.Entities;
using Unity.Entities.CodeGeneratedJobForEach;

public class ConwayMain : MonoBehaviour
{
    public GameObject pausedDropdown;
    
    [Range(2, 256)] [SerializeField] private ushort gridSize;
    public bool useParallelLogic;

    private int parallelGrain;


    [SerializeField] private GameObject displayObjectPrefab;


    [HideInInspector]
    public ushort ColumnCount;
    [HideInInspector]
    public ushort RowCount;
    [HideInInspector]
    public int RawCount;
    
    private GameObject _displayObject;
    private ConwayDisplayGrid _display;


    private Color[] _displayColors;

    private ConwayStateClass _myConwayState;
    private ConwayDisplayState _myConwayDisplayState;
    
    private NativeArray<byte> _displayState;

    private bool _tickerActive;
    private bool _mirrorDisplay;

    private TMP_Dropdown _pauseDropDownComponent;

    void Start()
    {
        Debug.Log("test xxx");

        parallelGrain = 64;

        ColumnCount = gridSize;
        RowCount = gridSize;
        RawCount = ColumnCount * RowCount;
        
        _pauseDropDownComponent = pausedDropdown.GetComponentInChildren<TMP_Dropdown>();

        _tickerActive = false;
        _mirrorDisplay = true;
        
        //enable ticker logic
        Ticker.OnTick += delegate(object sender, Ticker.OnTickArgs args)
        {
            if (_tickerActive)
            {
                TickForwardConwayState();
                
                UpdateDisplay();
            }
        };
        
        InitializeAll();

        CenterBrush(BrushFactory.GliderGun());
        
        UpdateDisplay();

    }

    private void InitializeAll()
    {

        Camera.main.orthographicSize = ((ColumnCount + 1) / 2) + 1;

        _displayColors = ColorFactory.GrayScaleReversed(5);

        int stateLength = (ColumnCount * RowCount);
        
        _displayState = new NativeArray<byte>(stateLength, Allocator.Persistent);
        
        _myConwayState = new ConwayStateClass(ColumnCount, RowCount); ///ushort -> byte for params
        _myConwayDisplayState = new ConwayDisplayState(_displayState, _myConwayState._indexer);

        
        if (_mirrorDisplay)
            SetMirroredDisplayState();
        else
            SetSimpleDisplayState();

        GenerateDisplay();

        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        if (!_tickerActive)
            _pauseDropDownComponent.value = 0;
        
        if (_mirrorDisplay)
            SetMirroredDisplayState();
        else
            SetSimpleDisplayState();
        _display.UpdateDisplay();
        
    }
    

    private void DeleteAll()
    {
        _displayState.Dispose();
    }

    
    
    
    private void GenerateDisplay()
    {
        Transform myTransform = this.transform;

        Vector3 tempVec = new Vector3(0, 0, 0f);
        Quaternion tempQuat = Quaternion.identity;

        _displayObject = Instantiate(displayObjectPrefab, tempVec, tempQuat, myTransform);
        _display = _displayObject.GetComponent<ConwayDisplayGrid>();
        _display.InitializeData(_myConwayState._indexer, _displayState, _displayColors);

        Transform displayObjectTransform = _displayObject.GetComponent<Transform>();
        displayObjectTransform.SetParent(myTransform);
        displayObjectTransform.position = displayObjectTransform.position + DisplayOriginVector();
    }

    public Vector3 DisplayOriginVector()
    {
        float xOffset = 0 - (_myConwayState._indexer.columnCount / 2 + _myConwayState._indexer.columnCount / 6);
        float yOffset = 0 - _myConwayState._indexer.rowCount / 2; 

        Vector3 returnValue = new Vector3(xOffset, yOffset, 0);
        //Debug.Log("OriginVector: " + returnValue);
        
        return  returnValue;
    }
    
    
    
    /* apparently not a thing
    private void DeleteDisplay()
    {
        GetComponent<Transform>().DetachChildren(); //is this sufficient?
    }
    */

    public void SetTimerOn(bool on)
    {
        _tickerActive = on;
        UpdateDisplay();
    }

    public void SetKaleidoscope(bool on)
    {
        _mirrorDisplay = on;
        UpdateDisplay();

    }
   
    private void SetSimpleDisplayState()
    {
        _myConwayDisplayState.SetSimpleDisplayState(_myConwayState._conwayState, 64);
    }

    private void SetMirroredDisplayState()
    {
        _myConwayDisplayState.SetMirroredDisplayState(_myConwayState._conwayState);
    }

    
    
    
    private void TickForwardConwayState()
    {
        _myConwayState.TickForwardConwayState();
        
        //reinsert when the parallel logic works right
        /*
        if (useParallelLogic)
            _myConwayState.TickForwardConwayState(parallelGrain);
        else
            _myConwayState.TickForwardConwayState();
            */
    }

    
    
    public void ToggleStateAt(int x, int y)
    {
        _myConwayState.ToggleStateAt(x, y);
        UpdateDisplay();

    }

    public void ClearMap()
    {
        _myConwayState.ClearMap(parallelGrain);
        _tickerActive = false;
    }

    public void RandomizeState()
    {
        _myConwayState.RandomizeState(parallelGrain);
        _tickerActive = false;
    }


    private void SetTestCells()
    {
        //setBrushAt(0, 1, BrushFactory.singleCell());
        //setBrushAt(5, 1, BrushFactory.singleCell());
        //setBrushAt(10, 1, BrushFactory.singleCell());
        //        setBrushAt(0, 0, BrushFactory.solidThreeBlock());
        //       setBrushAt(10, 11, BrushFactory.singleCell());
        /*
        setBrushAt(0, 0, BrushFactory.simpleGlider());
        setBrushAt(0, 5, BrushFactory.simpleGlider());
        setBrushAt(5, 0, BrushFactory.simpleGlider());
        setBrushAt(5, 5, BrushFactory.simpleGlider());
        setBrushAt(10, 0, BrushFactory.simpleGlider());
        setBrushAt(0, 10, BrushFactory.simpleGlider());
        setBrushAt(10, 5, BrushFactory.simpleGlider());
        setBrushAt(5, 10, BrushFactory.simpleGlider());
        setBrushAt(10, 0, BrushFactory.simpleGlider());
        setBrushAt(10, 10, BrushFactory.simpleGlider());
        */
         //       setBrushAt(64, 3, BrushFactory.acorn());
          //          setBrushAt(30, 25, BrushFactory.RPentomino());
        //        setBrushAt(5, 3, BrushFactory.Blinker());
        SetBrushAt(64, 64, BrushFactory.GliderGun());
    }
    
    
    
    //*** todo: should handle wrapping of indices 
    public void SetBrushAt(int x, int y, CellOffset[] brush)
    {
        _myConwayState.SetBrushAt(x, y, brush);
    }

    public void CenterBrush(CellOffset[] brush)
    {
        _myConwayState.CenterBrush(brush);
        
    }

    /*
    private void LogState()
    {
        String tempString = "";
        for (int i = 0; i<_conwayState.Length;
            i++)
        {
            if (_conwayState[i])
                tempString += "t ";
            else
                tempString += "f ";
        }
        Debug.Log("conway state: " + tempString);

        LogAdjacents();
        LogMirrored();
    }

    private void LogAdjacents()
    {
        for (int i = 0; i < _conwayState.Length; i++)
        {
            int tempBaseIndex = i * 8;
            string tempString = "cell adjacents for index " + i + ": ";
            
            for (int j = 0; j < 8; j++)
            {
                tempString += " " + _mapAdjacents[tempBaseIndex + j];

            }
            Debug.Log(tempString);
            
        }
    }

    private void LogMirrored()
    {
        for (int i = 0; i < _conwayState.Length; i++)
        {
            int tempBaseIndex = i * 4;
            string tempString = "Mirrored for index " + i + ": ";
            
            for (int j = 0; j < 4; j++)
            {
                tempString += " " + _mapMirroredDimensions[tempBaseIndex + j];

            }
            Debug.Log(tempString);
            
        }
    }

    private void LogSimpleState(string titleString, NativeArray<bool> inputState)
    {
        string tempString = titleString;

        tempString += ":\r";
        
        for (int i = 0; i < inputState.Length; i++)
        {
            tempString += inputState[i];
            tempString += " ";
        }

        tempString += "\r\r";
        Debug.Log(tempString);
    }
*/

    /*
    private void LogParallelizedAdjacents()
    {
        LogSimpleState("\rNeeds Recalc",  _needsRecalc);
        
        Debug.Log("\rAdjacent State: \r\r");
        LogSimpleState("Adjacent_0",  _adj_val_0);
        LogSimpleState("Adjacent_1",  _adj_val_1);
        LogSimpleState("Adjacent_2",  _adj_val_2);
        LogSimpleState("Adjacent_3",  _adj_val_3);
        LogSimpleState("Adjacent_4",  _adj_val_4);
        LogSimpleState("Adjacent_5",  _adj_val_5);
        LogSimpleState("Adjacent_6",  _adj_val_6);
        LogSimpleState("Adjacent_7",  _adj_val_7);

    }
    */

    private void OnDestroy()
    {
        Ticker.OnTick -= delegate {  }; //unclear if this correct
        DeleteAll();
       // _adjacentOffsets.Dispose();
    }
    
    //use (and probably fix) when UI driven resize is possible
    /*
    private void ResetAll(byte columns, byte rows)
    {
        DeleteAll();

        ColumnCount = columns;
        RowCount = rows;

        InitializeAll();
    }
    */
}




//the following does not work
/*
private void TickForwardConwayStateParallel()
    {
        //copy conwayState into previousConwayState
        previousConwayState.CopyFrom(conwayState);

        //set needsRecalc
            //copy conwayState into needsRecalc array (overwrite)
            //set all living adjacents (might imply possible race condition if parallel)
        SetNeedsRecalc();
        
        //for all true in needsRecalc, recalc conwayState entry - add adjacents for recalc cell, switch on living and sum conditions
            //parallelize
                //input: needsRecalc (indexed source), adjacents, previousConwayState
                //output target: conwayState
        TickForwardJob parallelConwayTick = new TickForwardJob();
        parallelConwayTick.jobConwayState = conwayState;
        parallelConwayTick.jobPreviousConwayState = previousConwayState;
        parallelConwayTick.jobNeedsRecalcList = needsRecalcList;

        JobHandle tickJob = parallelConwayTick.Schedule(needsRecalcList.Length, 1);
        tickJob.Complete();
    }

    private struct ConwayCellData
    {
        public ushort index;
        public bool adj_0;
        public bool adj_1;
        public bool adj_2;
        public bool adj_3;
        public bool adj_4;
        public bool adj_5;
        public bool adj_6;
        public bool adj_7;

        public int sumAdjacents()
        {
            int returnValue = 0;

            if (adj_0)
                returnValue += 1;
            if (adj_1)
                returnValue += 1;
            if (adj_2)
                returnValue += 1;
            if (adj_3)
                returnValue += 1;
            if (adj_4)
                returnValue += 1;
            if (adj_5)
                returnValue += 1;
            if (adj_6)
                returnValue += 1;
            if (adj_7)
                returnValue += 1;

            return returnValue;
        }
    }
    
    private struct TickForwardJob : IJobParallelFor
    {
        public NativeArray<bool> jobConwayState;

        [ReadOnly]
        public NativeList<ConwayCellData> jobNeedsRecalcList;
        public NativeArray<bool> jobPreviousConwayState;
        
        public void Execute(int index)
        {
            ConwayCellData tempData = jobNeedsRecalcList[index];
            int adjacencyCount = tempData.sumAdjacents();
            
            if (jobConwayState[tempData.index])
            {
                if ((adjacencyCount < 2)|| (adjacencyCount > 3))  
                    jobConwayState[tempData.index] = false;
            }
            else
            {
                if (adjacencyCount == 3)
                {
                    jobConwayState[tempData.index] = true;
                }
                
            }
        }
    }
    */


