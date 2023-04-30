using System.Collections.Generic;
using System.Net;

namespace Pixel
{
    public class Hierarchy : List<Node>
    {
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
        public int GetRootIndex(string name)
        {
            foreach (var i in this)
                if (i.Name == name)
                    return IndexOf(i);
            return -1;
        }
        public Node? RootSearch(int rootIndex)
        {
            if (Count < rootIndex)
                return null;

            return this[rootIndex];
        }
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