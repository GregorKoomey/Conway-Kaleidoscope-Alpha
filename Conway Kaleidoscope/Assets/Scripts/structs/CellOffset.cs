using Unity.Burst;

[BurstCompile]
public struct CellOffset
{
    public readonly int xOffset;
    public readonly int yOffset;

    public CellOffset(int xOffset, int yOffset)
    {
        this.xOffset = xOffset;
        this.yOffset = yOffset;
    }
    
    public int XFrom(int baseX)
    {
        return baseX + xOffset;
    }
    
    public int YFrom(int baseY)
    {
        return baseY + yOffset;
    }

    public CellOffset Combine(CellOffset cel)
    {
        int newXOffset = this.xOffset + cel.xOffset;
         int newYOffset = this.yOffset + cel.yOffset;
        return new CellOffset(newXOffset, newYOffset);
    }

    public CellOffset Translated(int x, int y)
    {
        return new CellOffset(this.xOffset + x, this.yOffset + y);
    }
}
