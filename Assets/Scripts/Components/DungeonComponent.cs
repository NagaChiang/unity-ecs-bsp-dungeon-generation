using System.Collections;
using System.Collections.Generic;
using Timespawn.UnityEcsBspDungeon.Dungeons;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Timespawn.UnityEcsBspDungeon.Components
{
    [System.Serializable]
    public struct DungeonComponent : IComponentData
    {
        public int2 SizeInCell;
        public int MinRoomLengthInCells;
        public int MaxRoomLengthInCells;

        [Range(0.0f, 1.0f)]
        public float MinSplitRatio;

        [Range(0.0f, 1.0f)]
        public float MaxSplitRatio;

        public bool IsPendingGenerate;
    }

    public struct RegisteredDungeonComponent : IComponentData
    {

    }
}