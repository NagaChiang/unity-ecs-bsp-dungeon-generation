﻿using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Timespawn.UnityEcsBspDungeon.Components
{
    public struct CellComponent : IComponentData
    {
        public int2 Coordinate;
        public bool IsWall;
    }

    public struct RegisteredCellComponent : IComponentData
    {
    }
}