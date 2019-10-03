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

        public static RectNode CreateBspTree(Rect fullRect, int threshold, float minSplitRatio, float maxSplitRatio)
        {
            Debug.Assert(threshold > 0, "BSP threshold should be positive.");
            Debug.Assert(minSplitRatio >= 0.0f && minSplitRatio <= 1.0f, "BSP min split ratio should be between 0.0 to 1.0.");
            Debug.Assert(maxSplitRatio >= 0.0f && maxSplitRatio <= 1.0f, "BSP max split ratio should be between 0.0 to 1.0.");
            Debug.Assert(minSplitRatio <= maxSplitRatio, "BSP min split ratio should be less than max.");

            RectNode root = new RectNode(fullRect, null, null);
            Stack<RectNode> nodeStack = new Stack<RectNode>();
            nodeStack.Push(root);
            while (nodeStack.Count > 0)
            {
                RectNode node = nodeStack.Pop();
                Rect nodeRect = node.Rect;
                if (nodeRect.Width <= threshold || nodeRect.Height <= threshold)
                {
                    continue;
                }

                Rect rectLeft = new Rect();
                Rect rectRight = new Rect();
                float splitRatio = Random.Range(minSplitRatio, maxSplitRatio);
                if (nodeRect.Width >= nodeRect.Height)
                {
                    // Split horizontally
                    int leftWidth = Mathf.FloorToInt(nodeRect.Width * splitRatio);
                    int2 rightPos = nodeRect.LowerLeftPos + new int2(leftWidth, 0);

                    rectLeft.SetRect(nodeRect.LowerLeftPos, leftWidth, nodeRect.Height);
                    rectRight.SetRect(rightPos, nodeRect.Width - leftWidth, nodeRect.Height);
                }
                else
                {
                    // Split vertically
                    int lowerHeight = Mathf.FloorToInt(nodeRect.Height * splitRatio);
                    int2 upperPos = nodeRect.LowerLeftPos + new int2(0, lowerHeight);

                    rectLeft.SetRect(nodeRect.LowerLeftPos, nodeRect.Width, lowerHeight);
                    rectRight.SetRect(upperPos, nodeRect.Width, nodeRect.Height - lowerHeight);
                }

                node.LeftNode = new RectNode(rectLeft, null, null);
                node.RightNode = new RectNode(rectRight, null, null);
                nodeStack.Push(node.LeftNode);
                nodeStack.Push(node.RightNode);
            }

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