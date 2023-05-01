using System.Collections.Generic;
using System.Net;

namespace Pixel
{
    /// <summary>
    /// The container that the Stage uses to store nodes and make queries.
    /// </summary>
    public class Hierarchy : List<Node>
    {
        /// <summary>
        /// Finds a node based on its name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Node? Find(string name)
        {
            foreach (var node in this)
            {
                if (node.Name == name)
                    return node;

                foreach(var child in node.children)
                    if (child.Name == name)
                        return child;
            }
            return null;
        }
        /// <summary>
        /// Finds the index of a node if it exists as a root in the stage hierarchy based on its name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int RootIndex(string name)
        {
            foreach (var i in this)
                if (i.Name == name)
                    return IndexOf(i);
            return -1;
        }
        /// <summary>
        /// Searches for a node by index in the root nodes.
        /// </summary>
        /// <param name="rootIndex"></param>
        /// <returns></returns>
        public Node? RootSearch(int rootIndex)
        {
            if (Count < rootIndex)
                return null;

            return this[rootIndex];
        }
        /// <summary>
        /// searches for a child under the specified root by index
        /// </summary>
        /// <param name="rootIndex"></param>
        /// <param name="child_index"></param>
        /// <returns></returns>
        public Node? ChildSearch(int rootIndex, int child_index)
        {
            if (Count < rootIndex)
                return null;

            if (this[rootIndex] is not Node node)
                return null;

            if (node.children.Count < child_index)
                return null;

            if (node.children[child_index] is not Node child)
                return null;

            return child;
        }
    }
}