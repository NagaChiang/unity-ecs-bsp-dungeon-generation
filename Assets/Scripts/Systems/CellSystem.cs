using System.Collections;
using System.Collections.Generic;
using Timespawn.UnityEcsBspDungeon.Components;
using Timespawn.UnityEcsBspDungeon.Core;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

namespace Timespawn.UnityEcsBspDungeon.Systems
{
    public class CellSystem : ComponentSystem
    {
        private EntityManager ActiveEntityManager;
        private EntityQuery OnRegisteredCellQuery;
        private EntityQuery OnChangedCellQuery;

        protected override void OnCreate()
        {
            ActiveEntityManager = World.Active.EntityManager;

            OnRegisteredCellQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {typeof(CellComponent)},
                None = new ComponentType[] {typeof(RegisteredCellComponent)},
            });

            OnChangedCellQuery = GetEntityQuery(
                ComponentType.ReadOnly<CellComponent>(),
                typeof(RenderMesh));
            OnChangedCellQuery.SetFilterChanged(typeof(CellComponent));
        }

        protected override void OnUpdate()
        {
            GameManager gameManager = GameManager.Instance();
            Debug.Assert(gameManager, "GameManager is null");

            Entities.With(OnRegisteredCellQuery).ForEach((Entity entity, ref CellComponent cellComp) =>
            {
                Entity dungeonEntity = GetSingletonEntity<DungeonComponent>();
                DungeonComponent dungeonComp = GetSingleton<DungeonComponent>();
                DynamicBuffer<EntityBufferElement> cellsBuffer = ActiveEntityManager.GetBuffer<EntityBufferElement>(dungeonEntity);
                EntityBufferElement bufferElem = new EntityBufferElement
                {
                    Entity = entity,
                };

                int index = cellComp.Coordinate.x + (cellComp.Coordinate.y * dungeonComp.SizeInCell.x);
                cellsBuffer.Insert(index, bufferElem);

                PostUpdateCommands.AddComponent(entity, new RegisteredCellComponent());
            });

            Entities.With(OnChangedCellQuery).ForEach((Entity entity, ref CellComponent cellComp) =>
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
