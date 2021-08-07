using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModuleBased.Node
{
    /// <summary>
    /// An interface to handle the node infrastructure
    /// </summary>
    public interface INode
    {
        /// <summary>
        /// The parent of this node
        /// </summary>
        INode Parent { get; set; }

        /// <summary>
        /// The original name of node
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The name of node in path
        /// </summary>
        string PathName { get; }

        /// <summary>
        /// The path string of node
        /// </summary>
        string NodePath { get; }

        /// <summary>
        /// Get all nodes under this node
        /// </summary>
        IEnumerable<INode> GetChildren();

        /// <summary>
        /// Get index of node under this current node
        /// </summary>
        /// <param name="node">The child node under this node</param>
        int IndexOf(INode node);

        /// <summary>
        /// Get the node of specific path
        /// </summary>
        /// <param name="pathName">The path of node</param>
        /// <param name="node">Output the node</param>
        bool TryGetNode(string pathName, out INode node);

        /// <summary>
        /// Add a node under this node
        /// </summary>
        void AddNode(INode node);

        /// <summary>
        /// Remove node under this node with specific name
        /// </summary>
        bool RemoveNode(string nodeName);

        /// <summary>
        /// Remove node under this node if existed
        /// </summary>
        bool RemoveNode(INode node);
    }

    public static class NodeUtils
    {
        public static readonly char PathSepChar = '/';
        public static readonly string BackPathStr = "..";

        #region -- Exceptions --
        public static readonly NodeNotFoundException PathNotFoundException = new NodeNotFoundException("The node of the specific path is not found.");
        #endregion

        /// <summary>
        /// Traversal all nodes under start
        /// </summary>
        public static IEnumerable<INode> Traversal(this INode start) 
        {
            Stack<INode> stack = new Stack<INode>();
            List<INode> list = new List<INode>();
            INode node;
            stack.Push(start);
            while (stack.Count > 0)
            {
                node = stack.Pop();
                list.Add(node);
                var macro = node;
                // check if has next
                if (macro != null)
                {
                    foreach (var child in macro.GetChildren().Reverse())
                    {   
                        stack.Push(child);
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Check if origin is under the target
        /// </summary>
        public static bool Under<T>(this T origin, T target) where T : INode
        {
            INode node = origin.Parent;
            while(node != null)
            {
                if (ReferenceEquals(node, target))
                    return true;
                node = node.Parent;
            }
            return false;
        }

        public static INode GetNode(this INode origin, string path)
        {
            INode curNode = origin;
            INode subNode;
            path = path.TrimStart(PathSepChar);
            string[] nodeNames = path.Split(PathSepChar);
            for (int i = 0; i < nodeNames.Length; i++)
            {
                string nextNodeName = nodeNames[i];
                // check back path
                if (nextNodeName.Equals(BackPathStr))
                {
                    subNode = curNode.Parent;
                }
                else if (curNode.TryGetNode(nextNodeName, out subNode))
                {
                    // final
                    if (i == nodeNames.Length - 1)
                    {
                        return subNode;
                    }
                }
                // next node
                var macro = subNode;
                if (macro == null)
                    throw PathNotFoundException;
                curNode = macro;
            }
            throw PathNotFoundException;
        }
    }

    public class NodeNotFoundException : Exception
    {
        public NodeNotFoundException(string msg) : base(msg) { }
    }
}
