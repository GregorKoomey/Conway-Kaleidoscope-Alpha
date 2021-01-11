using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;





//public struct SetConwayPreviousEntityState : IJobForEachWithEntity<>





//component types

public struct ConwayState : IComponentData
{
    public bool State;
}

public struct ConwayPrevState : IComponentData
{
    public bool State;
}

public struct ConwayNeedsRecalc : IComponentData
{
    public bool State;
}

public struct AdjacentCellEast : IComponentData
{
    public Entity Cell;
}

public struct AdjacentCellNorthEast : IComponentData
{
    public Entity Cell;
}

public struct AdjacentCellNorth : IComponentData
{
    public Entity Cell;
}

public struct AdjacentCellNorthWest : IComponentData
{
    public Entity Cell;
}

public struct AdjacentCellWest : IComponentData
{
    public Entity Cell;
}

public struct AdjacentCellSouthWest : IComponentData
{
    public Entity Cell;
}

public struct AdjacentCellSouth : IComponentData
{
    public Entity Cell;
}

public struct AdjacentCellSouthEast : IComponentData
{
    public Entity Cell;
}
