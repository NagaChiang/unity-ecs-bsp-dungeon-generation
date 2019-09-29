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

            dungeon.IsPendingGenerate = true;
        }

        private void GenerateDungeon(ref DungeonComponent dungeon, ref DynamicBuffer<EntityBufferElement> cellsBuffer)
        {
            Debug.Assert(dungeon.IsPendingGenerate, "It's not pending generate.");

            dungeon.IsPendingGenerate = false;

            // Set all cells as wall
            SetWallAll(ref cellsBuffer, true);

            // Rooms
            Rect fullRect = new Rect(int2.zero, dungeon.SizeInCell.x, dungeon.SizeInCell.y);
            RectNode root = RectNode.CreateBspTree(fullRect, dungeon.MaxRoomLengthInCells, dungeon.MinSplitRatio, dungeon.MaxSplitRatio);
            List<RectNode> leafs = root.GetLeafs();
            Dictionary<Rect, Room> rooms = new Dictionary<Rect, Room>();
            foreach (RectNode leaf in leafs)
            {
                Room room = GenerateRoom(leaf.Rect, dungeon.MinRoomLengthInCells, dungeon.SizeInCell.x, ref cellsBuffer);
                rooms.Add(leaf.Rect, room);
            }

            // Paths
            Stack<RectNode> nodeStack = new Stack<RectNode>();
            nodeStack.Push(root);
            while (nodeStack.Count > 0)
            {
                RectNode node = nodeStack.Pop();
                if (node.LeftNode == null && node.RightNode == null)
                {
                    continue;
                }

                Room roomLeft = rooms[node.LeftNode.GetRandomLeaf().Rect];
                Room roomRight = rooms[node.RightNode.GetRandomLeaf().Rect];
                GeneratePath(roomLeft, roomRight, dungeon.SizeInCell.x, ref cellsBuffer);

                nodeStack.Push(node.LeftNode);
                nodeStack.Push(node.RightNode);
            }

            // Extra paths
            if (root != null && root.LeftNode != null && root.RightNode != null)
            {
                for (int count = 0; count < dungeon.ExtraPathNum; count++)
                {
                    Room roomLeft = rooms[root.LeftNode.GetRandomLeaf().Rect];
                    Room roomRight = rooms[root.RightNode.GetRandomLeaf().Rect];
                    GeneratePath(roomLeft, roomRight, dungeon.SizeInCell.x, ref cellsBuffer);
                }
            }
        }

        private Room GenerateRoom(Rect area, int minLength, int sizeInCellX, ref DynamicBuffer<EntityBufferElement> cellsBuffer)
        {
            int width = Random.Range(Mathf.Min(minLength, area.Width), area.Width + 1);
            int height = Random.Range(Mathf.Min(minLength, area.Height), area.Height + 1);
            int2 lowerLeftPos = area.LowerLeftPos + new int2(Random.Range(0, area.Width - width), Random.Range(0, area.Height - height) + 1);
            Rect roomRect = new Rect(lowerLeftPos, width, height);
            DigInnerArea(roomRect, sizeInCellX, ref cellsBuffer);

            Room room = new Room(roomRect);
            return room;
        }

        private void GeneratePath(Room room1, Room room2, int sizeInCellX, ref DynamicBuffer<EntityBufferElement> cellsBuffer)
        {
            int2 pos1 = room1.GetRect().GetRandomInnerPosition();
            int2 pos2 = room2.GetRect().GetRandomInnerPosition();
            int2 offset = pos2 - pos1;
            int horizontalLength = Mathf.Abs(offset.x) + 1;
            int verticalLength = Mathf.Abs(offset.y) + 1;
            
            bool isHorizontalFirst = Random.value > 0.5f;
            if (isHorizontalFirst)
            {
                // Horizontal first
                if (offset.x >= 0)
                {
                    // Right
                    DigArea(new Rect(pos1, horizontalLength, 1), sizeInCellX, ref cellsBuffer);

                    if (offset.y >= 0)
                    {
                        // Up
                        DigArea(new Rect(pos2.x, pos1.y, 1, verticalLength), sizeInCellX, ref cellsBuffer);
                    }
                    else
                    {
                        // Down
                        DigArea(new Rect(pos2.x, pos2.y, 1, verticalLength), sizeInCellX, ref cellsBuffer);
                    }
                }
                else
                {
                    // Left
                    DigArea(new Rect(pos2.x, pos1.y, horizontalLength, 1), sizeInCellX, ref cellsBuffer);

                    if (offset.y >= 0)
                    {
                        // Up
                        DigArea(new Rect(pos2.x, pos1.y, 1, verticalLength), sizeInCellX, ref cellsBuffer);
                    }
                    else
                    {
                        // Down
                        DigArea(new Rect(pos2.x, pos2.y, 1, verticalLength), sizeInCellX, ref cellsBuffer);
                    }
                }
            }
            else
            {
                // Vertical first
                if (offset.y >= 0)
                {
                    // Up
                    DigArea(new Rect(pos1, 1, verticalLength), sizeInCellX, ref cellsBuffer);

                    if (offset.x >= 0)
                    {
                        // Right
                        DigArea(new Rect(pos1.x, pos2.y, horizontalLength, 1), sizeInCellX, ref cellsBuffer);
                    }
                    else
                    {
                        // Left
                        DigArea(new Rect(pos2, horizontalLength, 1), sizeInCellX, ref cellsBuffer);
                    }
                }
                else
                {
                    // Down
                    DigArea(new Rect(pos1.x, pos2.y, 1, verticalLength), sizeInCellX, ref cellsBuffer);

                    if (offset.x >= 0)
                    {
                        // Right
                        DigArea(new Rect(pos1.x, pos2.y, horizontalLength, 1), sizeInCellX, ref cellsBuffer);
                    }
                    else
                    {
                        // Left
                        DigArea(new Rect(pos2, horizontalLength, 1), sizeInCellX, ref cellsBuffer);
                    }
                }
            }
        }

        private void DigArea(Rect rect, int sizeInCellX, ref DynamicBuffer<EntityBufferElement> cellsBuffer)
        {
            List<int2> positions = rect.GetPositions();
            foreach (int2 pos in positions)
            {
                SetWall(ref cellsBuffer, sizeInCellX, pos, false);
            }
        }

        private void DigInnerArea(Rect rect, int sizeInCellX, ref DynamicBuffer<EntityBufferElement> cellsBuffer)
        {
            List<int2> innerPositions = rect.GetInnerPositions();
            foreach (int2 pos in innerPositions)
            {
                SetWall(ref cellsBuffer, sizeInCellX, pos, false);
            }
        }

        private void SetWall(ref DynamicBuffer<EntityBufferElement> cellsBuffer, int sizeInCellX, int2 pos, bool isWall)
        {
            int index = (sizeInCellX * pos.y) + pos.x;
            Debug.AssertFormat(index >= 0 && index < cellsBuffer.Length, "Index {0} is not within buffer length {1}", index, cellsBuffer.Length);

            Entity entity = cellsBuffer[index].Entity;
            CellComponent cellComp = ActiveEntityManager.GetComponentData<CellComponent>(entity);
            if (cellComp.IsWall != isWall)
            {
                cellComp.IsWall = isWall;
                PostUpdateCommands.SetComponent(entity, cellComp);
            }
        }

        private void SetWallAll(ref DynamicBuffer<EntityBufferElement> cellsBuffer, bool isWall)
        {
            for (int i = 0; i < cellsBuffer.Length; i++)
            {
                Entity entity = cellsBuffer[i].Entity;
                CellComponent cellComp = ActiveEntityManager.GetComponentData<CellComponent>(entity);
                if (cellComp.IsWall != isWall)
                {
                    cellComp.IsWall = isWall;
                    PostUpdateCommands.SetComponent(entity, cellComp);
                }
            }
        }
    }
}