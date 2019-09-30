using System;
using System.Collections;
using System.Collections.Generic;
using Timespawn.UnityEcsBspDungeon.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Timespawn.UnityEcsBspDungeon.Core
{
    public class GameManager : MonoBehaviour
    {
        [Header("Dungeon")]
        public DungeonComponent DefaultDungeonSettings;

        [Header("Cell")]
        public float CellScale;
        public Mesh CellMesh;
        public Material CellGroundMaterial;
        public Material CellWallMaterial;

        // Archetypes
        public EntityArchetype DungeonArchetype;
        public EntityArchetype CellArchetype;

        private static GameManager PrivateInstance;
        private EntityManager ActiveEntityManager;
        private Entity DungeonEntity;

        public static GameManager Instance()
        {
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

            ActiveEntityManager = World.Active.EntityManager;

            DungeonEntity = ActiveEntityManager.CreateEntity(DungeonArchetype);
            ActiveEntityManager.SetComponentData(DungeonEntity, DefaultDungeonSettings);
        }

        private void Start()
        {
            UIManager.Instance().Init(DefaultDungeonSettings);
            UIManager.Instance().GenerateButton_OnClick();
        }

        public void GenerateDungeon(DungeonComponent dungeonCompData, int seed)
        {
            Random.InitState(seed);
            
            // Setup dungeon
            DungeonComponent dungeonComp = ActiveEntityManager.GetComponentData<DungeonComponent>(DungeonEntity);
            dungeonComp = dungeonCompData;
            dungeonComp.SizeInCell = DefaultDungeonSettings.SizeInCell;
            dungeonComp.IsPendingGenerate = true;
            
            ActiveEntityManager.SetComponentData(DungeonEntity, dungeonComp);
        }

        private void CreateArchetypes()
        {
            EntityManager entityManager = World.Active.EntityManager;

            DungeonArchetype = entityManager.CreateArchetype(
                typeof(DungeonComponent),
                typeof(EntityBufferElement)
            );

            CellArchetype = entityManager.CreateArchetype(
                typeof(CellComponent),
                typeof(Translation),
                typeof(Scale),
                typeof(LocalToWorld),
                typeof(RenderMesh)
            );
        }
    }
}