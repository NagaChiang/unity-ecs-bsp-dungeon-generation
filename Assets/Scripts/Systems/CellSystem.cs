using System.Collections;
using System.Collections.Generic;
using Timespawn.UnityEcsBspDungeon.Components;
using Timespawn.UnityEcsBspDungeon.Core;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

namespace Timespawn.UnityEcsBspDungeon.Systems
{
    public class CellSystem : ComponentSystem
    {
        private EntityManager ActiveEntityManager;
        private EntityQuery OnCreateCellQuery;
        private EntityQuery ChangedCellQuery;

        protected override void OnCreate()
        {
            ActiveEntityManager = World.Active.EntityManager;

            OnCreateCellQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {typeof(CellComponent)},
                None = new ComponentType[] {typeof(CellRegisteredComponent)},
            });

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

            Entities.With(OnCreateCellQuery).ForEach((Entity entity, ref CellComponent cellComp) =>
            {
                Entity dungeonEntity = GetSingletonEntity<DungeonComponent>();
                DungeonComponent dungeonComp = GetSingleton<DungeonComponent>();
                DynamicBuffer<EntityBufferElement> cellsBuffer = ActiveEntityManager.GetBuffer<EntityBufferElement>(dungeonEntity);
                EntityBufferElement bufferElem = new EntityBufferElement
                {
                    Entity = entity,
                };

                int index = cellComp.Coordinate.x + (cellComp.Coordinate.y * dungeonComp.SizeInCell.y);
                cellsBuffer.Insert(index, bufferElem);

                PostUpdateCommands.AddComponent(entity, new CellRegisteredComponent());
            });

            Entities.With(ChangedCellQuery).ForEach((Entity entity, ref CellComponent cellComp) =>
            {
                Material cellMaterial = cellComp.IsWall ? gameManager.CellWallMaterial : gameManager.CellGroundMaterial;
                PostUpdateCommands.SetSharedComponent(entity, new RenderMesh
                {
                    mesh = gameManager.CellMesh,
                    material = cellMaterial,
                });
            });
        }
    }
}
