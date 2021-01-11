using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

public class ConwayDisplayState
{
    public NativeArray<byte> DisplayState;
    private NativeArray<ushort> MirroredDisplayIndices;

    private SparseGridIndexer indexer;


    public ConwayDisplayState(NativeArray<byte> displayState, SparseGridIndexer indexer)
    {
        this.indexer = indexer;
        DisplayState = displayState;
        MirroredDisplayIndices = BuildMapMirroredDimensions(indexer);
    }

    ~ConwayDisplayState()
    {
        MirroredDisplayIndices.Dispose();
    }

    public void SetSimpleDisplayState(NativeArray<bool> conwayState, byte value)
    {
        byte bottomValue = 0;//hack magic number 
        
        for (int i = 0; i < conwayState.Length; i++)
        {
            if (conwayState[i])
            {
                if (DisplayState[i] != value)
                    DisplayState[i] = value; 
            }
            else
                if (DisplayState[i] != bottomValue)
                    DisplayState[i] = bottomValue; 
        }
    }

    public void SetMirroredDisplayState(NativeArray<bool> conwayState)
    {
        for (int i=0; i < DisplayState.Length; i++)
        {
            DisplayState[i] = 0;
        }
        
        for (int i = 0; i < conwayState.Length; i++)
        {
            int mirroredDisplayBaseIndex = i * 4;

            if (conwayState[i])
            {
                DisplayState[MirroredDisplayIndices[mirroredDisplayBaseIndex]] += 1;
                DisplayState[MirroredDisplayIndices[mirroredDisplayBaseIndex+1]] += 1;
                DisplayState[MirroredDisplayIndices[mirroredDisplayBaseIndex+2]] += 1;
                DisplayState[MirroredDisplayIndices[mirroredDisplayBaseIndex+3]] += 1;
            }
        }
    }
    
    
    
    
    
    
    private NativeArray<ushort> BuildMapMirroredDimensions(SparseGridIndexer indexer)
    {
        NativeArray<ushort> returnValue = new NativeArray<ushort>(indexer.RawCount() * 4, Allocator.Persistent);
        
        for (int x = 0; x < indexer.columnCount; x++)
        {
            for (int y = 0; y < indexer.rowCount; y++)
            {
                int dimensionalRawIndex = indexer.RawIndexFor(x, y) * 4;
                ushort mirroredX = (ushort)SparseGridIndexer.MirroredIndexFor(x, (ushort)indexer.columnCount);
                ushort mirroredY = (ushort)SparseGridIndexer.MirroredIndexFor(y, (ushort)indexer.rowCount);
                
                ushort firstIndex = (ushort) indexer.RawIndexFor(x, y);
                ushort secondIndex = (ushort) indexer.RawIndexFor(mirroredX, y);
                ushort thirdIndex = (ushort) indexer.RawIndexFor(mirroredX, mirroredY);
                ushort fourthIndex = (ushort) indexer.RawIndexFor(x, mirroredY);

                returnValue[dimensionalRawIndex + 0] = firstIndex;
                returnValue[dimensionalRawIndex + 1] = secondIndex;
                returnValue[dimensionalRawIndex + 2] = thirdIndex;
                returnValue[dimensionalRawIndex + 3] = fourthIndex;
            }
        }
        return returnValue;
    }
    
    
    
}
