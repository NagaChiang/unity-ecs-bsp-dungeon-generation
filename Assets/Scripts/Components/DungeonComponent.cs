using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct DungeonComponent : IComponentData
{
    public float2 DungeonSize;
    public float CellWidth;
    public int BspDepth;
}
