using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace pixel_renderer
{
    public class Stage : IEnumerable
    {
        public string Name { get; set; }
        public string UUID { get; private set; }
        public event Action OnHierarchyChanged;
        public Dictionary<string, Node> NodesByName { get; private set; } = new Dictionary<string, Node>();
        public Node[] Nodes { get; private set; }
        public Node FindNode(string name)
        {
            if (NodesByName.ContainsKey(name))
            {
                return NodesByName[name];
            }
            return null;
        }
        public void RefreshStageDictionary()
        {
            foreach (Node node in Nodes)
            {
                if (!NodesByName.ContainsKey(node.Name))
                    NodesByName.Add(node.Name, node);
            }
        }


        /// <summary>
        /// update loop, fixed to the framerate of the rendering stage; 
        /// </summary>
        /// <param name="delta"></param>
        public void FixedUpdate(float delta)
        {
            foreach (Node node in Nodes) node.FixedUpdate(delta);
        }
        public void Awake()
        { 
            foreach (Node node in Nodes) node.Awake(); 
        }
           

        public Bitmap Background { get; set; }
        public static Stage Empty => new(Array.Empty<Node>());
        public static Stage New => new("New Stage", new Bitmap(256, 256), Array.Empty<Node>());
        /// <summary>
        /// Called on constructor initialization
        /// </summary>
        public void Init()
        {
            UUID = pixel_renderer.UUID.NewUUID(); 
            RefreshStageDictionary();
            GetEvents();
        }
        private void GetEvents()
        {
            OnHierarchyChanged += RefreshStageDictionary;
        }
        public Stage(Node[] nodes)
        {
            nodes = new Node[nodes.Length];
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i] = nodes[i];
            }
            Init(); 
        }
        public Stage(string Name, Bitmap Background, Node[] nodes)
        {
            this.Name = Name;
            this.Background = Background;
            Nodes = nodes;
            Init();
        }

        // for IEnumerator implementation;
        public NodeEnum GetEnumerator()
        {
            return new NodeEnum(Nodes);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    public class NodeEnum : IEnumerator
    {
        int position = -1;
        public Node[] _stage;
        public NodeEnum(Node[] list)
        {
            _stage = list;
        }
        public bool MoveNext()
        {
            position++;
            return position < _stage.Length;
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
