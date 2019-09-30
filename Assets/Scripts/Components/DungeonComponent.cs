using System;
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

        public int ExtraPathNum;

        [HideInInspector]
        public bool IsPendingGenerate;

        public override string ToString()
        {
            String str = "SizeInCell = " + SizeInCell + "\n"
                         + "RoomLengthInCells = (" + MinRoomLengthInCells + ", " + MaxRoomLengthInCells + ")\n"
                         + "SplitRatio = (" + MinSplitRatio.ToString("0.00") + ", " + MaxSplitRatio.ToString("0.00") + ")\n"
                         + "ExtraPathNum = " + ExtraPathNum + "\n"
                         + "IsPendingGenerate = " + IsPendingGenerate;

            return str;
        }
    }

    public struct RegisteredDungeonComponent : IComponentData
    {

    }
}