using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using JoshDisplay.Classes;


namespace JoshDisplay.Classes
{
    public class Stage : IEnumerable
    {
        public string? Name { get; set; }
        public string? GUID { get; private set; }
        
        public void Awake()
        {
            RefreshStageDictionary(); 
        }
        // List of nodes within stage
        public Dictionary<string, Node> nodesByName { get; private set; } = new Dictionary<string, Node>();
        public Node[] nodes { get; private set; }
        public Node FindNode(string name)
        {
            Node node = new Node();
            if (nodesByName.ContainsKey(name)) 
            {
                node = nodesByName[name];
            }
            return node;
        }
        void RefreshStageDictionary()
        {
            foreach (Node node in nodes)
            {
                if (!nodes.Contains(node))
                    nodesByName.Add(node.Name, node);
            }       
        }
        
        // Skybox image
        public Color[,] Background { get; set; } 

        // For iterating on in other scripts IENUMERARTOR

        public Stage(Node[] nodes)
        {
            nodes = new Node[nodes.Length];

            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i] = nodes[i];
            }
        }

        public Stage(string Name, Color[,] Background, Node[] nodes)
        {
            this.Name = Name;
            this.Background = Background;
            this.nodes = nodes;
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
