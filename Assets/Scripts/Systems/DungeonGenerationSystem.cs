using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class DungeonGenerationSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref DungeonComponent dungeonComp) =>
        {

        });
    }
}
