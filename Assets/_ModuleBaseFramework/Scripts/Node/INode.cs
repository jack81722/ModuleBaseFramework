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
        /// Get all nodes under this node
        /// </summary>
        IEnumerable<INode> GetChildren();

        /// <summary>
        /// Get index of node under this current node
        /// </summary>
        /// <param name="node">The child node under this node</param>
        int IndexOf(INode node);

        /// <summary>
        /// Add a node under this node
        /// </summary>
        void AddNode(INode node);

        
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
    }

    public class NodeNotFoundException : Exception
    {
        public NodeNotFoundException(string msg) : base(msg) { }
    }
}
