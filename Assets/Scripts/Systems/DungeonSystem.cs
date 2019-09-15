using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class DungeonSystem : ComponentSystem
{
    private EntityManager ActiveEntityManager;
    private EntityQuery DungeonQuery;

    protected override void OnCreate()
    {
        ActiveEntityManager = World.Active.EntityManager;
        DungeonQuery = GetEntityQuery(typeof(DungeonComponent));
    }

    protected override void OnUpdate()
    {
        Entities.With(DungeonQuery).ForEach((Entity entity, ref DungeonComponent dungeon) => {
            if (dungeon.IsPendingGenerate)
            {
                DynamicBuffer<EntityBufferElement> cellsBuffer = ActiveEntityManager.GetBuffer<EntityBufferElement>(entity); 
                Generate(ref dungeon, ref cellsBuffer);
            }
        });
    }

    private void Generate(ref DungeonComponent dungeon, ref DynamicBuffer<EntityBufferElement> cellsBuffer)
    {
        Debug.Assert(dungeon.IsPendingGenerate, "It's not pending generate.");

        dungeon.IsPendingGenerate = false;
        cellsBuffer.Clear();

        float cellWidth = GameManager.Instance().CellScale;
        for (int y = 0; y < dungeon.SizeInCell.y; y++)
        {
            for (int x = 0; x < dungeon.SizeInCell.x; x++)
            {
                float3 pos = float3.zero;
                pos.x = cellWidth * (0.5f + x);
                pos.y = cellWidth * (0.5f + y);

                Entity entity = PostUpdateCommands.CreateEntity(GameManager.Instance().CellArchetype);
                PostUpdateCommands.SetComponent(entity, new CellComponent {
                    IsWall = true,
                });
                PostUpdateCommands.SetComponent(entity, new Translation {
                    Value = pos,
                });
                PostUpdateCommands.SetComponent(entity, new Scale() {
                    Value = cellWidth,
                });

                EntityBufferElement bufferElem = new EntityBufferElement {
                    Entity = entity,
                };
                cellsBuffer.Add(bufferElem);
                //Material cellMaterial = x % 2 == 0 ? GameManager.Instance().CellGroundMaterial : GameManager.Instance().CellWallMaterial;
                /*
                PostUpdateCommands.SetSharedComponent(entity, new RenderMesh {
                    mesh = GameManager.Instance().CellMesh,
                    material = cellMaterial,
                });
                */
            }
        }

        Debug.LogFormat("Dungeon generated ({0} cells).", cellsBuffer.Length);
    }
}
