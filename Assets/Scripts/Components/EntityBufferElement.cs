using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Timespawn.UnityEcsBspDungeon.Components
{
    [InternalBufferCapacity(10)]
    public struct EntityBufferElement : IBufferElementData
    {
        public Entity Entity;
    }
}
