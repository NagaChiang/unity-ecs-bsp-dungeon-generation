using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Timespawn.UnityEcsBspDungeon.Dungeons
{
    public struct Rect
    {
        public int2 LowerLeftPos;
        public int Width;
        public int Height;

        public Rect(int2 lowerLeftPos, int width, int height)
        {
            LowerLeftPos = lowerLeftPos;
            Width = width;
            Height = height;
        }

        public void SetRect(int2 lowerLeftPos, int width, int height)
        {
            LowerLeftPos = lowerLeftPos;
            Width = width;
            Height = height;
        }

        public List<int2> GetPositions()
        {
            List<int2> positions = new List<int2>();
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    positions.Add(LowerLeftPos + new int2(x, y));
                }
            }

            return positions;
        } 

        public List<int2> GetInnerPositions()
        {
            List<int2> positions = new List<int2>();
            for (int y = 1; y < Height - 1; y++)
            {
                for (int x = 1; x < Width - 1; x++)
                {
                    positions.Add(LowerLeftPos + new int2(x, y));
                }
            }

            return positions;
        }
    }
}