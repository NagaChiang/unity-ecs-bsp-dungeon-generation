using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Dungeon")]
    public DungeonComponent DungeonSettings;

    [Header("Cell")]
    public float CellScale;
    public Mesh CellMesh;
    public Material CellGroundMaterial;
    public Material CellWallMaterial;

    // Archetypes
    public EntityArchetype DungeonArchetype;
    public EntityArchetype CellArchetype;

    private static GameManager PrivateInstance;

    public static GameManager Instance()
    {
        if (!PrivateInstance)
        {
            GameObject obj = new GameObject("GameManager");
            PrivateInstance = obj.AddComponent<GameManager>();
        }

        return PrivateInstance;
    }

    private void Awake()
    {
        if (PrivateInstance)
        {
            Destroy(this);
        }
        else
        {
            PrivateInstance = this;
        }

        CreateArchetypes();

        EntityManager entityManager = World.Active.EntityManager;

        Entity dungeon = entityManager.CreateEntity(DungeonArchetype);
        entityManager.SetComponentData(dungeon, DungeonSettings);
    }

    private void CreateArchetypes()
    {
        EntityManager entityManager = World.Active.EntityManager;

        DungeonArchetype = entityManager.CreateArchetype (
            typeof(DungeonComponent),
            typeof(EntityBufferElement)
        );

        CellArchetype = entityManager.CreateArchetype (
            typeof(CellComponent),
            typeof(Translation),
            typeof(Scale),
            typeof(LocalToWorld),
            typeof(RenderMesh)
        );
    }
}
