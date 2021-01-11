using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using UnityEngine;
public struct NativeArrayUtil 
{
    public static void SetAllValues(NativeArray<bool> array, bool val, int parallelGrain)
    {
        ParallelSetBoolValJob setJob = new ParallelSetBoolValJob();
        setJob.array = array;
        setJob.value = val;

        JobHandle tickJob = setJob.Schedule(array.Length, parallelGrain);
        tickJob.Complete();       
 }

    [BurstCompile]
    private struct ParallelSetBoolValJob : IJobParallelFor
    {
        public NativeArray<bool> array;
        public bool value;

        public void Execute(int index)
        {
            if (array[index] != value)
                array[index] = value;
        }
    }

    public static void SetAllValues(NativeArray<byte> array, byte val, int parallelGrain)
    {
        ParallelSetByteValJob setJob = new ParallelSetByteValJob();
        setJob.array = array;
        setJob.value = val;

        JobHandle tickJob = setJob.Schedule(array.Length, parallelGrain);
        tickJob.Complete();       
    }

    [BurstCompile]
    private struct ParallelSetByteValJob : IJobParallelFor
    {
        public NativeArray<byte> array;
        public byte value;

        public void Execute(int index)
        {
            if (array[index] != value)
                array[index] = value;
        }
    }

    public static void CopyNativeArray(NativeArray<bool> source, NativeArray<bool> destination, int parallelGrain)
    {
        ParallelBoolCopyJob parallelCopyTick = new ParallelBoolCopyJob();
        parallelCopyTick.source = source;
        parallelCopyTick.destination = destination;

        int copyLength = source.Length > destination.Length ? destination.Length : source.Length;
        
        //should inner loop batch count be function of copyLength?
        JobHandle tickJob = parallelCopyTick.Schedule(copyLength, parallelGrain);
        tickJob.Complete();       
    }

    [BurstCompile]
    private struct ParallelBoolCopyJob : IJobParallelFor
    {
        public NativeArray<bool> source;
        public NativeArray<bool> destination;
        
        public void Execute(int index)
        {
            if (destination[index] != source[index])
                destination[index] = source[index];
        }
    } 
    
    //set random value?
    public static void RandomizeNativeArray(NativeArray<bool> source, int chanceInTen, int parallelGrain)
    {
        ParallelBoolRandomizeJob parallelCopyTick = new ParallelBoolRandomizeJob();
        
        parallelCopyTick.source = source;
        parallelCopyTick.chanceInTen = chanceInTen;

        //should inner loop batch count be function of copyLength?
        JobHandle tickJob = parallelCopyTick.Schedule(source.Length, parallelGrain);
        tickJob.Complete();       
    }

    [BurstCompile]
    private struct ParallelBoolRandomizeJob : IJobParallelFor
    {
        public NativeArray<bool> source;
        public int chanceInTen;
        
        public void Execute(int index)
        {
            int randomValue = UnityEngine.Random.Range(0, 10);
            if (randomValue < chanceInTen) 
                source[index] = true;
        }
    } 

    public static void CopyNativeArray(NativeArray<byte> source, NativeArray<byte> destination, int parallelGrain)
    {
        ParallelByteCopyJob parallelCopyTick = new ParallelByteCopyJob();
        parallelCopyTick.source = source;
        parallelCopyTick.destination = destination;

        int copyLength = source.Length > destination.Length ? destination.Length : source.Length;
        
        //should inner loop batch count be function of copyLength?
        JobHandle tickJob = parallelCopyTick.Schedule(copyLength, parallelGrain);
        tickJob.Complete();       
    }

    [BurstCompile]
    private struct ParallelByteCopyJob : IJobParallelFor
    {
        public NativeArray<byte> source;
        public NativeArray<byte> destination;
        
        public void Execute(int index)
        {
            if (destination[index] != source[index])
                destination[index] = source[index];
        }
    } 

    
}
