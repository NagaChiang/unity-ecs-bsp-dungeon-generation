using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

public class DungeonSystem : ComponentSystem
{
    private EntityManager ActiveEntityManager;
    private EntityQuery DungeonQuery;

    private float LastStepElapsedTime;

    protected override void OnCreate()
    {
        ActiveEntityManager = World.Active.EntityManager;
        DungeonQuery = GetEntityQuery(typeof(DungeonComponent));
    }

    protected override void OnUpdate()
    {
        LastStepElapsedTime += Time.deltaTime;
        float secPerStep = GameManager.Instance().SecondPerStep;
        if (LastStepElapsedTime >= secPerStep)
        {
            return;
        }
        LastStepElapsedTime -= secPerStep;

        Entities.With(DungeonQuery).ForEach((Entity entity, ref DungeonComponent dungeon) =>
        {
            DynamicBuffer<EntityBufferElement> cellsBuffer = ActiveEntityManager.GetBuffer<EntityBufferElement>(entity);
            if (dungeon.IsPendingGenerate)
            {
                Generate(ref dungeon, ref cellsBuffer);
            }
            else
            {
                // Randomly dig
                int2 size = dungeon.SizeInCell;
                int2 coord = new int2(Random.Range(0, size.x), Random.Range(0, size.y));
                SetWall(ref cellsBuffer, size.x, coord, false);
            }
        });
    }

    private void Generate(ref DungeonComponent dungeon, ref DynamicBuffer<EntityBufferElement> cellsBuffer)
    {
        Debug.Assert(dungeon.IsPendingGenerate, "It's not pending generate.");

        dungeon.IsPendingGenerate = false;

        cellsBuffer.Clear();
        cellsBuffer.ResizeUninitialized(dungeon.SizeInCell.x * dungeon.SizeInCell.y);

        float cellWidth = GameManager.Instance().CellScale;
        for (int y = 0; y < dungeon.SizeInCell.y; y++)
        {
            for (int x = 0; x < dungeon.SizeInCell.x; x++)
            {
                float3 pos = float3.zero;
                pos.x = cellWidth * (0.5f + x);
                pos.y = cellWidth * (0.5f + y);

                Entity entity = PostUpdateCommands.CreateEntity(GameManager.Instance().CellArchetype);
                PostUpdateCommands.SetComponent(entity, new CellComponent
                {
                    Coordinate = new int2(x, y),
                    IsWall = true,
                });
                PostUpdateCommands.SetComponent(entity, new Translation
                {
                    Value = pos,
                });
                PostUpdateCommands.SetComponent(entity, new Scale()
                {
                    Value = cellWidth,
                });
            }
        }
        
        Debug.LogFormat("Dungeon generated ({0} cells).", cellsBuffer.Length);
    }

    private void SetWall(ref DynamicBuffer<EntityBufferElement> cellsBuffer, int sizeInCellX, int2 coord, bool isWall)
    {
        int index = (sizeInCellX * coord.y) + coord.x;
        Debug.AssertFormat(index >= 0 && index < cellsBuffer.Length, "Index {0} is not within buffer length {1}", index, cellsBuffer.Length);

        Entity entity = cellsBuffer[index].Entity;
        CellComponent cellComp = ActiveEntityManager.GetComponentData<CellComponent>(entity);
        if (cellComp.IsWall != isWall)
        {
            cellComp.IsWall = isWall;
            PostUpdateCommands.SetComponent(entity, cellComp);
        }
    }
}
