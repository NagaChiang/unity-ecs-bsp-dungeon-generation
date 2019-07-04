using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Dungeon Generation")]
    public DungeonComponent DungeonSettings;
    public RenderMesh CellRenderMeshSettings;

    // Archetypes
    public EntityArchetype DungeonArchetype;
    public EntityArchetype CellArchetype;

    private void Awake()
    {
        CreateArchetypes();

        EntityManager entityManager = World.Active.EntityManager;

        Entity dungeon = entityManager.CreateEntity(DungeonArchetype);
        entityManager.SetComponentData(dungeon, DungeonSettings);

        Entity cell = entityManager.CreateEntity(CellArchetype);
        entityManager.SetSharedComponentData(cell, CellRenderMeshSettings);
    }

    private void CreateArchetypes()
    {
        EntityManager entityManager = World.Active.EntityManager;

        DungeonArchetype = entityManager.CreateArchetype
        (
            typeof(DungeonComponent)
        );

        CellArchetype = entityManager.CreateArchetype
        (
            typeof(CellComponent),
            typeof(Translation),
            typeof(RenderMesh),
            typeof(LocalToWorld)
        );
    }
}
