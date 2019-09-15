using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

public class CellSystem : ComponentSystem
{
    private EntityManager ActiveEntityManager;
    private EntityQuery ChangedCellQuery;

    protected override void OnCreate()
    {
        ActiveEntityManager = World.Active.EntityManager;

        ChangedCellQuery = GetEntityQuery(
            ComponentType.ReadOnly<CellComponent>(),
            typeof(RenderMesh)
        );
        ChangedCellQuery.SetFilterChanged(typeof(CellComponent));
    }

    protected override void OnUpdate()
    {
        GameManager gameManager = GameManager.Instance();
        Debug.Assert(gameManager, "GameManager is null");

        Entities.With(ChangedCellQuery).ForEach((Entity entity, ref CellComponent cellComp) => {
            Material cellMaterial = cellComp.IsWall ? gameManager.CellWallMaterial : gameManager.CellGroundMaterial;
            PostUpdateCommands.SetSharedComponent(entity, new RenderMesh {
                mesh = gameManager.CellMesh,
                material = cellMaterial,
            });
        });
    }
}
