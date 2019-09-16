using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[InternalBufferCapacity(0)]
public struct EntityBufferElement : IBufferElementData
{
    public Entity Entity;
}
