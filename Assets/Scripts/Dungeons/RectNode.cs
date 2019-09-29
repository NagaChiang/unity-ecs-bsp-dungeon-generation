using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Timespawn.UnityEcsBspDungeon.Dungeons
{
    public class RectNode
    {
        public Rect Rect;
        public RectNode LeftNode;
        public RectNode RightNode;

        public RectNode(Rect rect, RectNode leftNode, RectNode rightNode)
        {
            Rect = rect;
            LeftNode = leftNode;
            RightNode = rightNode;
        }

        public static RectNode CreateBspTree(Rect parentRect, int threshold, float minSplitRatio, float maxSplitRatio)
        {
            Debug.Assert(threshold > 0, "BSP threshold should be positive.");
            Debug.Assert(minSplitRatio >= 0.0f && minSplitRatio <= 1.0f, "BSP min split ratio should be between 0.0 to 1.0.");
            Debug.Assert(maxSplitRatio >= 0.0f && maxSplitRatio <= 1.0f, "BSP max split ratio should be between 0.0 to 1.0.");
            Debug.Assert(minSplitRatio <= maxSplitRatio, "BSP min split ratio should be less than max.");

            RectNode root = new RectNode(parentRect, null, null);
            if (parentRect.Width <= threshold || parentRect.Height <= threshold)
            {
                return root;
            }

            Rect rect1 = new Rect();
            Rect rect2 = new Rect();
            float splitRatio = Random.Range(minSplitRatio, maxSplitRatio);
            if (parentRect.Width >= parentRect.Height)
            {
                // Split horizontally
                int leftWidth = Mathf.FloorToInt(parentRect.Width * splitRatio);
                int2 rightPos = parentRect.LowerLeftPos + new int2(leftWidth, 0);

                rect1.SetRect(parentRect.LowerLeftPos, leftWidth, parentRect.Height);
                rect2.SetRect(rightPos, parentRect.Width - leftWidth, parentRect.Height);
            }
            else
            {
                // Split vertically
                int lowerHeight = Mathf.FloorToInt(parentRect.Height * splitRatio);
                int2 upperPos = parentRect.LowerLeftPos + new int2(0, lowerHeight);

                rect1.SetRect(parentRect.LowerLeftPos, parentRect.Width, lowerHeight);
                rect2.SetRect(upperPos, parentRect.Width, parentRect.Height - lowerHeight);
            }

            root.LeftNode = CreateBspTree(rect1, threshold, minSplitRatio, maxSplitRatio);
            root.RightNode = CreateBspTree(rect2, threshold, minSplitRatio, maxSplitRatio);
            
            return root;
        }

        public List<RectNode> GetLeafs()
        {
            List<RectNode> leafs = new List<RectNode>();

            Stack<RectNode> nodeStack = new Stack<RectNode>();
            nodeStack.Push(this);
            while (nodeStack.Count > 0)
            {
                RectNode node = nodeStack.Pop();
                if (node.LeftNode == null && node.RightNode == null)
                {
                    leafs.Add(node);
                    continue;
                }

                nodeStack.Push(node.LeftNode);
                nodeStack.Push(node.RightNode);
            }

            return leafs;
        }

        public RectNode GetRandomLeaf()
        {
            RectNode leaf = null;
            Stack<RectNode> nodeStack = new Stack<RectNode>();
            nodeStack.Push(this);
            while (nodeStack.Count > 0)
            {
                RectNode node = nodeStack.Pop();
                if (node.LeftNode == null && node.RightNode == null)
                {
                    leaf = node;
                    break;
                }

                RectNode randomNode = Random.value > 0.5f ? node.LeftNode : node.RightNode;
                nodeStack.Push(randomNode);
            }

            return leaf;
        }
    }
}