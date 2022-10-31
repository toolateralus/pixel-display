using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing; 

namespace PixelRenderer.Components
{
    public class Stage : IEnumerable
    {
        public string? Name { get; set; }
        public string? GUID { get; private set; }

        // List of nodes within stage
        public Dictionary<string, Node> nodesByName { get; private set; } = new Dictionary<string, Node>();
        public Node[] nodes { get; private set; }
        public Node FindNode(string name)
        {
            if (nodesByName.ContainsKey(name))
            {
                return nodesByName[name];
            }
            return null;
        }
        public void RefreshStageDictionary()
        {
            foreach (Node node in nodes)
            {
                if (!nodesByName.ContainsKey(node.Name))
                    nodesByName.Add(node.Name, node); 
            }
        }
        public void Update(float delta)
        {
            foreach (Node node in nodes) node.Update(); 
            Collision.CheckCollision(this);
        }
        // Skybox image
        public Bitmap Background { get; set; }

        // Constructors
        public Stage(Node[] nodes)
        {
            nodes = new Node[nodes.Length];

            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i] = nodes[i];
            }
            RefreshStageDictionary();
        }

        public Stage(string Name, Bitmap Background, Node[] nodes)
        {
            this.Name = Name;
            this.Background = Background;
            this.nodes = nodes;
            RefreshStageDictionary();
        }

        // Implementation for the GetEnumerator method.
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }
        public NodeEnum GetEnumerator()
        {
            return new NodeEnum(nodes);
        }
    }
    public class NodeEnum : IEnumerator
    {
        public Node[] _stage;

        // Enumerators are positioned before the first element
        // until the first MoveNext() call.
        int position = -1;

        public NodeEnum(Node[] list)
        {
            _stage = list;
        }

        public bool MoveNext()
        {
            position++;
            return (position < _stage.Length);
        }

        public void Reset()
        {
            position = -1;
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public Node Current
        {
            get
            {
                try
                {
                    return _stage[position];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }

}
