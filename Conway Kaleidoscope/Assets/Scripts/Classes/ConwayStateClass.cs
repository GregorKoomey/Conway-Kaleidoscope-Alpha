using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Burst;


public class ConwayStateClass
{
    
    //private Ticker ticker;

    //note: ushorts constrained to 2-256
    [HideInInspector]
    [Range(2, 256)]
    public ushort ColumnCount; 
    [HideInInspector]
    [Range(2, 256)]
    public ushort RowCount;
    [HideInInspector]
    public int RawCount;
    
    private NativeArray<CellOffset> _adjacentOffsets;

    public NativeArray<bool> _conwayState;
    private NativeArray<bool> _previousConwayState;
    private NativeArray<bool> _needsRecalc;
    private NativeArray<ushort> _mapAdjacents;
   // private NativeArray<byte> _conwayCount;

    
    //private NativeArray<byte> _displayState;
    //private NativeArray<ushort> _mapMirroredDimensions;

    public SparseGridIndexer _indexer;

    
    
    public ConwayStateClass(ushort colCount, ushort rowCount)
    {
        ushort adjustedColCount = colCount > (ushort)256 ? (ushort)256 : colCount; 
        adjustedColCount = colCount < (ushort)3 ? (ushort)2 : colCount; 
        ushort adjustedRowCount = rowCount > (ushort)256 ? (ushort)256 : rowCount; 
        adjustedRowCount = rowCount < (ushort)3 ? (ushort)2 : rowCount; 
        
        AllocateData(adjustedColCount, adjustedRowCount);
    }
    
    ~ConwayStateClass()
    {
        DeallocateData();
    }

    private void AllocateData(  ushort colCount,  ushort rowCount)
    {
        ColumnCount = colCount;
        RowCount = rowCount;
        RawCount = ColumnCount * RowCount;

        _indexer = new SparseGridIndexer(ColumnCount, RowCount);

        _conwayState = new NativeArray<bool>(RawCount, Allocator.Persistent);
        //Debug.Log("_conwayState length: " + _conwayState.Length);
        _needsRecalc = new NativeArray<bool>(RawCount, Allocator.Persistent);
        //Debug.Log("_needsRecalc  length: " + _needsRecalc.Length);
        _previousConwayState = new NativeArray<bool>(RawCount, Allocator.Persistent);
        //Debug.Log("_previousConwayState length: " + _previousConwayState.Length);

    
        _adjacentOffsets = BuildAdjacentOffsets();
        //Debug.Log("_adjacentOffsets length: " + _adjacentOffsets.Length);
        _mapAdjacents = BuildAdjacents(_indexer, _adjacentOffsets);
        //Debug.Log("_mapAdjacents length: " + _mapAdjacents.Length);

        
        
        
        
    }

    private void DeallocateData()
    {
        _conwayState.Dispose();
        _needsRecalc.Dispose();
        _adjacentOffsets.Dispose();
        _mapAdjacents.Dispose();


    }

    //setup functions are called after AllocateData and assume ColCount, RowCount and RawCount have been set
    private NativeArray<CellOffset> BuildAdjacentOffsets()
    {
        NativeArray<CellOffset> returnValue = new NativeArray<CellOffset>(8, Allocator.Persistent);
        returnValue[0] = new CellOffset(1, 0);
        returnValue[1] = new CellOffset(1, 1);
        returnValue[2] = new CellOffset(0, 1);
        returnValue[3] = new CellOffset(-1, 1);
        returnValue[4] = new CellOffset(-1, 0);
        returnValue[5] = new CellOffset(-1, -1);
        returnValue[6] = new CellOffset(0, -1);
        returnValue[7] = new CellOffset(1, -1);
        return returnValue;
    }
    
    private NativeArray<ushort> BuildAdjacents(SparseGridIndexer indexer, NativeArray<CellOffset> offsets)
    {
        int adjacentsLength = indexer.RawCount() * 8;
        
        NativeArray<ushort> returnValue = new NativeArray<ushort>(adjacentsLength, Allocator.Persistent);
        
  //      Debug.Log("BuildAdjacents returnValue Length: " + returnValue.Length);
        
        int columnCount = indexer.columnCount;
        int rowCount = indexer.rowCount;
        
        for (int x = 0; x < columnCount; x++)
        {
            for (int y = 0; y < rowCount; y++)
            {
                int conwayBaseIndex = indexer.RawIndexFor(x, y);
                int adjacentsBaseIndex = conwayBaseIndex * 8;

                for (int i = 0; i < offsets.Length; i++)
                {
                    int adjacentOffsetX = offsets[i].XFrom(x);
                    int adjacentOffsetY = offsets[i].YFrom(y);

                    int adjustedX = SparseGridIndexer.WrappedIndexFor(adjacentOffsetX, (ushort)columnCount);
                    int adjustedY = SparseGridIndexer.WrappedIndexFor(adjacentOffsetY, (ushort)rowCount);
                    ushort adjustedRawValue = (ushort) indexer.RawIndexFor(adjustedX, adjustedY);

                    returnValue[adjacentsBaseIndex + i] = adjustedRawValue;
                }
            }
        }
//        Debug.Log("BuildAdjacents returnValue (2) Length: " + returnValue.Length);
        return returnValue;
    }


    private void SetNeedsRecalc(int parallelGrain)//this method not fully jobbed up, parallelGrain just used for copy function
    {
        NativeArrayUtil.CopyNativeArray(_conwayState, _needsRecalc, parallelGrain);
        
        for (int i = 0; i < _conwayState.Length; i++)
        {
            if (_conwayState[i])
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
        
    }

    //single threaded
    public void TickForwardConwayState()
    {
        //              Debug.Log("tickForwardConwayState start");
        _previousConwayState.CopyFrom(_conwayState);

        SetNeedsRecalc(64);
        
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
    
    //with jobs
    public void TickForwardConwayState(int parallelGrain)
    {
        SetNeedsRecalc(parallelGrain);
        NativeArrayUtil.CopyNativeArray(_conwayState, _previousConwayState, parallelGrain);
        
        TickForwardConwayJob conwayJob = new TickForwardConwayJob();
        conwayJob.conwayState = _conwayState;
        conwayJob.mapAdjacents = _mapAdjacents;
        conwayJob.needsRecalc = _needsRecalc;
        conwayJob.previousConwayState = _previousConwayState;

        JobHandle tickJob = conwayJob.Schedule(_needsRecalc.Length, parallelGrain);
        tickJob.Complete();      
    }

    [BurstCompile]
    private struct     TickForwardConwayJob : IJobParallelFor
    {
        public NativeArray<bool> conwayState;
        public NativeArray<bool> previousConwayState; //set external to job (copy of conwayState)
        public NativeArray<bool> needsRecalc;
        public  NativeArray<ushort> mapAdjacents;
        
        
        public void Execute(int index)
        {
            if (needsRecalc[index])
            {
                int baseOffsetsAddress = index * 8;
                int adjacentCount = 0;

                for (int i = 0; i < 8; i++)
                {
                    if (previousConwayState[mapAdjacents[baseOffsetsAddress + i]])
                        adjacentCount += 1;
                    if (adjacentCount == 4)
                        break;
                }

                if (previousConwayState[index]) //cell is alive
                {
                    if (!(adjacentCount == 2) && !(adjacentCount == 3))
                        conwayState[index] = false;
                }
                else //cell is dead
                {
                    if (adjacentCount == 3)
                        conwayState[index] = true;
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
        
        //UpdateDisplay();

    }

    public void ClearMap(int grain)
    {
        NativeArrayUtil.SetAllValues(_conwayState, false, grain);
    }

    public void RandomizeState(int grain)
    {
        ClearMap(grain);

        for (int i = 0; i < _conwayState.Length / 5; i++)
        {
//            Random(conwayState.Length)
            //create random index
            int randomIndex = UnityEngine.Random.Range(0, _conwayState.Length - 1);
            _conwayState[randomIndex] = true;
        }
    }

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

}
