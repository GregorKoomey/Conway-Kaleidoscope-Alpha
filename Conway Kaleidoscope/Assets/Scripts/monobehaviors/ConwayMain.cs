using System;
using Unity.Collections;
using UnityEngine;
using TMPro;
using Unity.Burst;
using Unity.Jobs;

public class ConwayMain : MonoBehaviour
{
    //public GameObject kaleidoscopeDropDown;
    public GameObject pausedDropdown;
    //public GameObject brushesDropDown;
    
    [Range(2, 256)] [SerializeField] private ushort gridSize;
   
    //public Camera myCamera;

    [SerializeField] private GameObject displayObjectPrefab;

    public bool useParallelLogic;
    //private Ticker ticker;

    public ushort ColumnCount;
    public ushort RowCount;
    public int RawCount;
    
    private GameObject _displayObject;
    private ConwayDisplayGrid _display;


    private Color[] _displayColors;

    private NativeArray<CellOffset> _adjacentOffsets;

    private NativeArray<bool> _conwayState;
    private NativeArray<bool> _previousConwayState;
    private NativeArray<bool> _needsRecalc;
    private NativeArray<ushort> _mapAdjacents;

    //adjacency info for job code
    private NativeArray<bool> _adj_0;
    private NativeArray<bool> _adj_1;
    private NativeArray<bool> _adj_2;
    private NativeArray<bool> _adj_3;
    private NativeArray<bool> _adj_4;
    private NativeArray<bool> _adj_5;
    private NativeArray<bool> _adj_6;
    private NativeArray<bool> _adj_7;
    
    private NativeArray<byte> _displayState;
    private NativeArray<ushort> _mapMirroredDimensions;

    private SparseGridIndexer _indexer;

    private bool _tickerActive;
    private bool _mirrorDisplay;

    private TMP_Dropdown _pauseDropDownComponent;

    void Start()
    {
        Debug.Log("test xxx");

        ColumnCount = gridSize;
        RowCount = gridSize;
        RawCount = ColumnCount * RowCount;
        
        _pauseDropDownComponent = pausedDropdown.GetComponentInChildren<TMP_Dropdown>();

        _adjacentOffsets = new NativeArray<CellOffset>(8, Allocator.Persistent);
        SetAdjacentOffsets();

        _tickerActive = false;
        _mirrorDisplay = true;
        
        //enable ticker logic
        Ticker.OnTick += delegate(object sender, Ticker.OnTickArgs args)
        {
            //Debug.Log("ticker");
            if (_tickerActive)
            {
                if (useParallelLogic)
                {
                    //Debug.Log("parallel enabled");
                    TickForwardConwayStateParallel();
                }
                else
                {
                    TickForwardConwayState();
                }
                
                UpdateDisplay();
            }
        };
        
        InitializeAll();

        CenterBrush(BrushFactory.GliderGun());
        
        UpdateDisplay();

    }

    private void InitializeAll()
    {
        _indexer = new SparseGridIndexer(ColumnCount, RowCount);

        Camera.main.orthographicSize = ((ColumnCount + 1) / 2) + 1;
        

        _displayColors = ColorFactory.GrayScaleReversed(5);



        int stateLength = (ColumnCount * RowCount);
        _conwayState = new NativeArray<bool>(stateLength, Allocator.Persistent);
        _previousConwayState= new NativeArray<bool>(stateLength, Allocator.Persistent);
        _needsRecalc = new NativeArray<bool>(stateLength, Allocator.Persistent);
 
        _adj_0 = new NativeArray<bool>(stateLength, Allocator.Persistent);
        _adj_1 = new NativeArray<bool>(stateLength, Allocator.Persistent);
        _adj_2 = new NativeArray<bool>(stateLength, Allocator.Persistent);
        _adj_3 = new NativeArray<bool>(stateLength, Allocator.Persistent);
        _adj_4 = new NativeArray<bool>(stateLength, Allocator.Persistent);
        _adj_5 = new NativeArray<bool>(stateLength, Allocator.Persistent);
        _adj_6 = new NativeArray<bool>(stateLength, Allocator.Persistent);
        _adj_7 = new NativeArray<bool>(stateLength, Allocator.Persistent);
        
        
        _displayState = new NativeArray<byte>(stateLength, Allocator.Persistent);

        _mapAdjacents = new NativeArray<ushort>(stateLength * 8, Allocator.Persistent);
        _mapMirroredDimensions = new NativeArray<ushort>(stateLength * 4, Allocator.Persistent);

        SetAdjacents();
        SetMapMirroredDimensions();
        
        //LogState();
        
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
        _conwayState.Dispose();
        _previousConwayState.Dispose();
        _needsRecalc.Dispose();

        _adj_0.Dispose();
        _adj_1.Dispose();
        _adj_2.Dispose();
        _adj_3.Dispose();
        _adj_4.Dispose();
        _adj_5.Dispose();
        _adj_6.Dispose();
        _adj_7.Dispose();
        
        //needsRecalcList.Dispose();
        
        _displayState.Dispose();

        _mapAdjacents.Dispose();
        _mapMirroredDimensions.Dispose();

//        DeleteDisplay(); //

    }

    
    

    private void SetAdjacents()
    {
        for (int x = 0; x < ColumnCount; x++)
        {
            for (int y = 0; y < RowCount; y++)
            {
                int conwayBaseIndex = _indexer.RawIndexFor(x, y);
                int adjacentsBaseIndex = conwayBaseIndex * 8;

                for (int i = 0; i < _adjacentOffsets.Length; i++)
                {
     //               Debug.Log("x: " + x + " y: " + y + " i: " + i);

                    int adjacentOffsetX = _adjacentOffsets[i].XFrom(x);
                    int adjacentOffsetY = _adjacentOffsets[i].YFrom(y);

  //                  Debug.Log("adjacentOffsetX: " + adjacentOffsetX + " adjacentOffsetY: " + adjacentOffsetY);
                    
                    
                    
                    int adjustedX = SparseGridIndexer.WrappedIndexFor(adjacentOffsetX, ColumnCount);
                    int adjustedY = SparseGridIndexer.WrappedIndexFor(adjacentOffsetY, RowCount);
                    ushort adjustedRawValue = (ushort) _indexer.RawIndexFor(adjustedX, adjustedY);
                    

 //                   Debug.Log("AdjustedX: " + adjustedX + " AdjustedY: " + adjustedY + "adjustedRawValue: " + adjustedRawValue);
                    
                    _mapAdjacents[adjacentsBaseIndex + i] =
                        (ushort) _indexer.RawIndexFor(adjustedX, adjustedY);

//                    Debug.Log("Adjacent rawIndex value" + mapAdjacents[adjacentsBaseIndex+i]);
                }
            }
        }
        
    }


    private void SetAdjacentOffsets()
    {
        _adjacentOffsets[0] = new CellOffset(1, 0);
        _adjacentOffsets[1] = new CellOffset(1, 1);
        _adjacentOffsets[2] = new CellOffset(0, 1);
        _adjacentOffsets[3] = new CellOffset(-1, 1);
        _adjacentOffsets[4] = new CellOffset(-1, 0);
        _adjacentOffsets[5] = new CellOffset(-1, -1);
        _adjacentOffsets[6] = new CellOffset(0, -1);
        _adjacentOffsets[7] = new CellOffset(1, -1);
    }

    public void SetMapMirroredDimensions()
    {
        for (int x = 0; x < ColumnCount; x++)
        {
            for (int y = 0; y < RowCount; y++)
            {
                int dimensionalRawIndex = _indexer.RawIndexFor(x, y) * 4;
                int mirroredX = SparseGridIndexer.MirroredIndexFor(x, ColumnCount);
                int mirroredY = SparseGridIndexer.MirroredIndexFor(y, RowCount);
                
//                Debug.Log("x: " + x + " y: " + y +" mirroredX: " + mirroredX + " mirroredY: " + mirroredY);
                

                ushort firstIndex = (ushort) _indexer.RawIndexFor(x, y);
                ushort secondIndex = (ushort) _indexer.RawIndexFor(mirroredX, y);
                ushort thirdIndex = (ushort) _indexer.RawIndexFor(mirroredX, mirroredY);
                ushort fourthIndex = (ushort) _indexer.RawIndexFor(x, mirroredY);

                _mapMirroredDimensions[dimensionalRawIndex + 0] = firstIndex;
                _mapMirroredDimensions[dimensionalRawIndex + 1] = secondIndex;
                _mapMirroredDimensions[dimensionalRawIndex + 2] = thirdIndex;
                _mapMirroredDimensions[dimensionalRawIndex + 3] = fourthIndex;
            }
        }
    }
    
    private void SetNeedsRecalc()
    {
        _needsRecalc.CopyFrom(_conwayState);
        _previousConwayState.CopyFrom(_conwayState);
       // needsRecalcList.Clear();

        
        
//        NativeArray<bool>.Copy(conwayState, needsRecalc);

        for (int i = 0; i < _previousConwayState.Length; i++)
        {
            if (_previousConwayState[i])
            {
                int adjacentsIndex = i * 8;
                for (int j = 0; j < 8; j++)
                {
                    int currentAdjacentIndex = adjacentsIndex + j;
                    int currentAdjacentRawIndex = _mapAdjacents[currentAdjacentIndex];
                    _needsRecalc[currentAdjacentRawIndex] = true;
                }
            }
        }
        
        for (int i=0;i<_needsRecalc.Length;i++)
        {
            if (_needsRecalc[i])
            {
                int adjacentsIndex = i * 8;

                for (int j = 0; j < 8; j++)
                {
                    int currentAdjacentIndex = adjacentsIndex + j;
                    int currentAdjacentRawIndex = _mapAdjacents[currentAdjacentIndex];
                    switch (j)
                    {
                        case 0:
                            if (_conwayState[currentAdjacentRawIndex])
                                _adj_0[i] = true;
                            else
                                _adj_0[i] = false;
                            break;
                        case 1:
                            if (_conwayState[currentAdjacentRawIndex])
                                _adj_1[i] = true;
                            else
                                _adj_1[i] = false;
                            break;
                        case 2:
                            if (_conwayState[currentAdjacentRawIndex])
                                _adj_2[i] = true;
                            else
                                _adj_2[i] = false;
                            break;
                        case 3:
                            if (_conwayState[currentAdjacentRawIndex])
                                _adj_3[i] = true;
                            else
                                _adj_3[i] = false;
                            break;
                        case 4:
                            if (_conwayState[currentAdjacentRawIndex])
                                _adj_4[i] = true;
                            else
                                _adj_4[i] = false;
                            break;
                        case 5:
                            if (_conwayState[currentAdjacentRawIndex])
                                _adj_5[i] = true;
                            else
                                _adj_5[i] = false;
                            break;
                        case 6:
                            if (_conwayState[currentAdjacentRawIndex])
                                _adj_6[i] = true;
                            else
                                _adj_6[i] = false;
                            break;
                        case 7:
                            if (_conwayState[currentAdjacentRawIndex])
                                _adj_7[i] = true;
                            else
                                _adj_7[i] = false; 
                            break;
                    }
                }
            }
        }

        //LogParallelizedAdjacents();
    }

    private void GenerateDisplay()
    {
        Transform myTransform = this.transform;

        Vector3 tempVec = new Vector3(0, 0, 0f);
        Quaternion tempQuat = Quaternion.identity;

        _displayObject = Instantiate(displayObjectPrefab, tempVec, tempQuat, myTransform);
        _display = _displayObject.GetComponent<ConwayDisplayGrid>();
        _display.InitializeData(_indexer, _displayState, _displayColors);

        Transform displayObjectTransform = _displayObject.GetComponent<Transform>();
        displayObjectTransform.SetParent(myTransform);
        displayObjectTransform.position = displayObjectTransform.position + DisplayOriginVector();
    }

    public Vector3 DisplayOriginVector()
    {
        float xOffset = 0 - (_indexer.columnCount / 2 + _indexer.columnCount / 6);
        float yOffset = 0 - _indexer.rowCount / 2; 

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
        for (int i = 0; i < _conwayState.Length; i++)
        {
            if (_conwayState[i])
                _displayState[i] = 4; //hack magic number based on 5 levels of gray in display Color array
            else
                _displayState[i] = 0; //hack magic number based on 5 levels of gray in display Color array
        }
    }

    private void SetMirroredDisplayState()
    {
        for (int i=0; i < _displayState.Length; i++)
        {
            _displayState[i] = 0;
        }
        
        for (int i = 0; i < _conwayState.Length; i++)
        {
            int mirroredDisplayBaseIndex = i * 4;

            if (_conwayState[i])
            {
                _displayState[_mapMirroredDimensions[mirroredDisplayBaseIndex]] += 1;
                _displayState[_mapMirroredDimensions[mirroredDisplayBaseIndex+1]] += 1;
                _displayState[_mapMirroredDimensions[mirroredDisplayBaseIndex+2]] += 1;
                _displayState[_mapMirroredDimensions[mirroredDisplayBaseIndex+3]] += 1;
            }
        }
    }

    
    
    
    private void TickForwardConwayState()
    {
        //              Debug.Log("tickForwardConwayState start");
        _previousConwayState.CopyFrom(_conwayState);

        SetNeedsRecalc();
        
        for (int i = 0; i < _needsRecalc.Length; i++)
        {
            //           Debug.Log("tickForwardConwayState inside first loop: " + i);
            if (_needsRecalc[i])
            {
//                Debug.Log("tickForwardConwayState needsRecalc: " + i );

                bool alive = _previousConwayState[i]; //same length as needsRecalc
                int adjacentIndex = i * 8;
                int adjacentAliveCount = 0;

                for (int j = 0; j < 8; j++)
                {
                    if (_previousConwayState[_mapAdjacents[adjacentIndex + j]])
                    {
                        adjacentAliveCount += 1;
                    }
                }

                //              Debug.Log("tickForwardConwayState adjacentLiveCount: " + adjacentAliveCount);
                if (alive)
                {
                    if ((adjacentAliveCount < 2)|| (adjacentAliveCount > 3))  
                        _conwayState[i] = false;
                }
                else //cell is dead
                {
                    if (adjacentAliveCount == 3)
                    {
                        _conwayState[i] = true;
                    }
                }
            }
        }

        //       Debug.Log("tickForwardConwayState finish");

    }

    private void TickForwardConwayStateParallel()
    {
        //copy conwayState into previousConwayState
        _previousConwayState.CopyFrom(_conwayState);

        //set needsRecalc
        //copy conwayState into needsRecalc array (overwrite)
        //set all living adjacents (might imply possible race condition if parallel)
        SetNeedsRecalc();
        
        //for all true in needsRecalc, recalc conwayState entry - add adjacents for recalc cell, switch on living and sum conditions
        //parallelize
        //input: needsRecalc (indexed source), adjacents, previousConwayState
        //output target: conwayState
        TickForwardJob parallelConwayTick = new TickForwardJob();
        parallelConwayTick.jobConwayState = _conwayState;
        parallelConwayTick.jobPreviousConwayState = _previousConwayState;
        parallelConwayTick.jobNeedsRecalc = _needsRecalc;
        parallelConwayTick.jobAdj_0 = _adj_0;
        parallelConwayTick.jobAdj_1 = _adj_1;
        parallelConwayTick.jobAdj_2 = _adj_2;
        parallelConwayTick.jobAdj_3 = _adj_3;
        parallelConwayTick.jobAdj_4 = _adj_4;
        parallelConwayTick.jobAdj_5 = _adj_5;
        parallelConwayTick.jobAdj_6 = _adj_6;
        parallelConwayTick.jobAdj_7 = _adj_7;
        
        JobHandle tickJob = parallelConwayTick.Schedule(_conwayState.Length, 50);
        tickJob.Complete();
    }
    
    [BurstCompile]
    private struct TickForwardJob : IJobParallelFor
    {
        public NativeArray<bool> jobConwayState;
        
        [ReadOnly]
        public NativeArray<bool> jobPreviousConwayState;
        [ReadOnly]
        public NativeArray<bool> jobNeedsRecalc;
        [ReadOnly]
        public NativeArray<bool> jobAdj_0;
        [ReadOnly]
        public NativeArray<bool> jobAdj_1;
        [ReadOnly]
        public NativeArray<bool> jobAdj_2;
        [ReadOnly]
        public NativeArray<bool> jobAdj_3;
        [ReadOnly]
        public NativeArray<bool> jobAdj_4;
        [ReadOnly]
        public NativeArray<bool> jobAdj_5;
        [ReadOnly]
        public NativeArray<bool> jobAdj_6;
        [ReadOnly]
        public NativeArray<bool> jobAdj_7;
        
        public void Execute(int index)
        {
            if (jobNeedsRecalc[index])
            {
                byte livingAdjacentCount = 0;

                if (jobAdj_0[index])
                    livingAdjacentCount += 1;
                if (jobAdj_1[index])
                    livingAdjacentCount += 1;
                if (jobAdj_2[index])
                    livingAdjacentCount += 1;
                if (jobAdj_3[index])
                    livingAdjacentCount += 1;
                if (jobAdj_4[index])
                    livingAdjacentCount += 1;
                if (jobAdj_5[index])
                    livingAdjacentCount += 1;
                if (jobAdj_6[index])
                    livingAdjacentCount += 1;
                if (jobAdj_7[index])
                    livingAdjacentCount += 1;
                
                if (jobPreviousConwayState[index]) //cell is alive
                {
                    if (!(livingAdjacentCount==2) && !(livingAdjacentCount==3))
                        jobConwayState[index] = false;
                }
                else //cell is dead
                {
                    if (livingAdjacentCount == 3)
                        jobConwayState[index] = true;
                }
            }
            
        }
    }
    
    
    
    public void ToggleStateAt(int x, int y)
    {
        int rawIndex = _indexer.RawIndexFor(x, y);
        //Debug.Log("ToggleStateAt rawIndex: " + rawIndex);
        if (_conwayState[rawIndex])
            _conwayState[rawIndex] = false;
        else
            _conwayState[rawIndex] = true;
        
        UpdateDisplay();

    }

    public void ClearMap()
    {
        for (int i = 0; i < _conwayState.Length; i++)
        {
            _conwayState[i] = false;
        }

        _tickerActive = false;
    }

    public void RandomizeState()
    {
        ClearMap();

        for (int i = 0; i < _conwayState.Length / 5; i++)
        {
//            Random(conwayState.Length)
            //create random index
            int randomIndex = UnityEngine.Random.Range(0, _conwayState.Length - 1);
            _conwayState[randomIndex] = true;
        }
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

        for (int i = 0; i < brush.Length; i++)
        {
            CellOffset tempOffset = brush[i];

            int newX = tempOffset.XFrom(x);
            int newY = tempOffset.YFrom(y);

            int newRawIndex = _indexer.RawIndexFor(newX, newY);
            if (newRawIndex < _conwayState.Length)
                _conwayState[newRawIndex] = true;
        }

    }

    public void CenterBrush(CellOffset[] brush)
    {
        //Debug.Log("center brush");
        int halfColumnCount = ColumnCount / 2;
        int halfRowCount = RowCount / 2;

        int halfBrushWidth = BrushFactory.WidthForBrush(brush) / 2;
        int halfBrushHeight = BrushFactory.HeightForBrush(brush) / 2;

        Debug.Log("halfBrushWidth: " + halfBrushWidth + " halfColumnCount: " + halfColumnCount + " halfBrushHeight: " + halfBrushHeight + " halfRowCount: "  + halfRowCount);
        if ((halfBrushWidth > halfColumnCount) || (halfBrushHeight > halfRowCount))
            SetBrushAt(0, 0, brush);
        else
            SetBrushAt(halfColumnCount - halfBrushWidth, halfRowCount - halfBrushHeight, brush);
    }

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

    private void LogParallelizedAdjacents()
    {
        LogSimpleState("\rNeeds Recalc",  _needsRecalc);
        
        Debug.Log("\rAdjacent State: \r\r");
        LogSimpleState("Adjacent_0",  _adj_0);
        LogSimpleState("Adjacent_1",  _adj_1);
        LogSimpleState("Adjacent_2",  _adj_2);
        LogSimpleState("Adjacent_3",  _adj_3);
        LogSimpleState("Adjacent_4",  _adj_4);
        LogSimpleState("Adjacent_5",  _adj_5);
        LogSimpleState("Adjacent_6",  _adj_6);
        LogSimpleState("Adjacent_7",  _adj_7);

    }

    private void OnDestroy()
    {
        Ticker.OnTick -= delegate {  }; //unclear if this correct
        DeleteAll();
        _adjacentOffsets.Dispose();
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


