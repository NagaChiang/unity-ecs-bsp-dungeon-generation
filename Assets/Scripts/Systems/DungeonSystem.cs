using System.Collections;
using System.Collections.Generic;
using Timespawn.UnityEcsBspDungeon.Components;
using Timespawn.UnityEcsBspDungeon.Core;
using Timespawn.UnityEcsBspDungeon.Dungeons;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;
using Rect = Timespawn.UnityEcsBspDungeon.Dungeons.Rect;

namespace Timespawn.UnityEcsBspDungeon.Systems
{
    public class DungeonSystem : ComponentSystem
    {
        private EntityManager ActiveEntityManager;
        private EntityQuery OnRegisteredDungeonQuery;
        private EntityQuery DungeonQuery;

        private float LastStepElapsedTime;

        protected override void OnCreate()
        {
            ActiveEntityManager = World.Active.EntityManager;

            OnRegisteredDungeonQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {typeof(DungeonComponent)},
                None = new ComponentType[] {typeof(RegisteredDungeonComponent)},
            });

            DungeonQuery = GetEntityQuery(
                typeof(DungeonComponent),
                typeof(RegisteredDungeonComponent));
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

            Entities.With(OnRegisteredDungeonQuery).ForEach((Entity entity, ref DungeonComponent dungeon) =>
            {
                DynamicBuffer<EntityBufferElement> cellsBuffer = ActiveEntityManager.GetBuffer<EntityBufferElement>(entity);
                Initialize(ref dungeon, ref cellsBuffer);

                PostUpdateCommands.AddComponent(entity, new RegisteredDungeonComponent());
            });

            Entities.With(DungeonQuery).ForEach((Entity entity, ref DungeonComponent dungeon) =>
            {
                DynamicBuffer<EntityBufferElement> cellsBuffer = ActiveEntityManager.GetBuffer<EntityBufferElement>(entity);
                if (dungeon.IsPendingGenerate)
                {
                    GenerateDungeon(ref dungeon, ref cellsBuffer);
                }
            });
        }

        private void Initialize(ref DungeonComponent dungeon, ref DynamicBuffer<EntityBufferElement> cellsBuffer)
        {
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
        }

        private void GenerateDungeon(ref DungeonComponent dungeon, ref DynamicBuffer<EntityBufferElement> cellsBuffer)
        {
            Debug.Assert(dungeon.IsPendingGenerate, "It's not pending generate.");

            dungeon.IsPendingGenerate = false;

            Rect fullRect = new Rect(int2.zero, dungeon.SizeInCell.x, dungeon.SizeInCell.y);
            RectNode root = RectNode.CreateBspTree(fullRect, dungeon.MaxRoomLengthInCells, dungeon.MinSplitRatio, dungeon.MaxSplitRatio);
            List<RectNode> leafs = RectNode.GetLeafs(root);
            List<Room> rooms = new List<Room>();
            foreach (RectNode leaf in leafs)
            {
                Room room = GenerateRoom(leaf.Rect, dungeon.MinRoomLengthInCells, dungeon.SizeInCell.x, ref cellsBuffer);
                rooms.Add(room);
            }
        }

        private Room GenerateRoom(Rect area, int minLength, int sizeInCellX, ref DynamicBuffer<EntityBufferElement> cellsBuffer)
        {
            int width = Random.Range(Mathf.Min(minLength, area.Width), area.Width);
            int height = Random.Range(Mathf.Min(minLength, area.Height), area.Height);
            int2 lowerLeftPos = area.LowerLeftPos + new int2(Random.Range(0, area.Width - width), Random.Range(0, area.Height - height));
            Rect roomRect = new Rect(lowerLeftPos, width, height);
            Room room = new Room(roomRect);

            List<int2> innerPositions = roomRect.GetInnerPositions();
            foreach (int2 pos in innerPositions)
            {
                SetWall(ref cellsBuffer, sizeInCellX, pos, false);
            }

            return room;
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
}