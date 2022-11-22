using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace pixel_renderer
{
  

    public class Stage : IEnumerable
    {
        public string Name { get; set; }
        private string _uuid = "";
        
        public string UUID { get { return _uuid; } init => _uuid = pixel_renderer.UUID.NewUUID(); }
        public event Action OnQueryMade; 

        public Dictionary<string, Node> NodesByName { get; private set; } = new Dictionary<string, Node>();

        public Node[] Nodes { get; private set; }
        public Node[] FindNodesByTag(string tag)
        {
            OnQueryMade?.Invoke(); 
            IEnumerable<Node> matchingNodes = Nodes.Where(node => node.tag == tag);
            return matchingNodes.ToArray();
        }
        public Node FindNodeByTag(string tag)
        {
            OnQueryMade?.Invoke();
            return Nodes
                    .Where(node => node.tag == tag)
                    .First();
        }
        public Node FindNode(string name)
        {
            OnQueryMade?.Invoke();
            return Nodes
                    .Where(node => node.Name == name)
                    .First();
        }
        public void RefreshStageDictionary()
        {
            foreach (Node node in Nodes)
                if (!NodesByName.ContainsKey(node.Name))
                    NodesByName.Add(node.Name, node);
        }
        public void CreateNode(Node? template)
        {
            var list = Nodes.ToList();
            list.Add(template ?? Node.New);
            Nodes = list.ToArray(); 
        }

        /// <summary>
        /// update loop, fixed to the framerate of the rendering stage; 
        /// </summary>
        /// <param name="delta"></param>
        public void FixedUpdate(float delta)
        { foreach (Node node in Nodes) node.FixedUpdate(delta); }
        public void Awake()
        {
            RefreshStageDictionary(); 
            OnQueryMade += RefreshStageDictionary;
            foreach (Node node in Nodes)
            {
                node.parentStage ??= this; 
                node.Awake();
            }
        }


        public Bitmap Background;
        public static Stage Empty => new(Array.Empty<Node>());
        public static Stage New => new("New Stage", new Bitmap(256, 256), Array.Empty<Node>());
        /// <summary>
        /// Called on constructor initialization
        /// </summary>
       
      
        public Stage(Node[] nodes)
        {
            nodes = new Node[nodes.Length];
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i] = nodes[i];
            }
            Awake();
            RefreshStageDictionary();
        }
        public Stage(string Name, Bitmap Background, Node[] nodes)
        {
            this.Name = Name;
            this.Background = Background;
            Nodes = nodes;
            Awake();
            RefreshStageDictionary(); 
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
