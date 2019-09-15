using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct DungeonComponent : IComponentData
{
    public int2 SizeInCell;
    public int BspDepth;
    public bool IsPendingGenerate;
}
