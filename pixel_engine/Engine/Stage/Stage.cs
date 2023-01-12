using pixel_renderer.Assets;
using pixel_renderer.FileIO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace pixel_renderer
{
    public class Stage
    {
        public Stage() { }
        public Stage(string Name, Metadata backgroundMeta, List<NodeAsset> nodes, string? existingUUID = null)
        {
            _uuid = existingUUID ?? pixel_renderer.UUID.NewUUID();
            GetBackground(backgroundMeta);
            this.Name = Name;
            Nodes = nodes.ToNodeList();
            Awake();
        }
        public Bitmap backgroundImage;
        
        public event Action OnNodeQueryMade;
        public List<Node> Nodes { get; private set; } = new();
        public Dictionary<string, Node> NodesByName { get; private set; } = new Dictionary<string, Node>();

        Queue<Action<object[]>> DelayedActionQueue = new();
        Queue<object[]> DelayedActionArgsQueue = new();

        /// <summary>
        ///  used to keep track of how many generic nodes have been instantiated for naming
        /// </summary>
        int genericNodeCt = 0;
        public string Name { get; set; }
        private string _uuid = "";
        public string UUID => _uuid;

        public bool FixedUpdateBusy { get; private set; }

        public void Awake()
        {
            OnNodeQueryMade += RefreshStageDictionary;
            for (int i = 0; i < Nodes.Count; i++)
            {
                Node node = Nodes[i];
                node.ParentStage = this;
                node.Awake();
            }
        }
        public void FixedUpdate(float delta)
        {
            lock (Nodes)
            {
                FixedUpdateBusy = true;

                for (int i = 0; i < Nodes.Count; i++)
                {
                    Node node = Nodes[i];
                    node.FixedUpdate(delta);
                }

                FixedUpdateBusy = false;
            }

            for(int i = 0; DelayedActionQueue.Count - 1 > 0; ++i)
            {
               Action<object[]> action = DelayedActionQueue.Dequeue();
               object[] args = DelayedActionArgsQueue.Dequeue();
               action(args);
            }
        }
        
        public void RefreshStageDictionary()
        {
            foreach (Node node in Nodes)
            {
                if (node.Name is null) continue;
                if (!NodesByName.ContainsKey(node.Name))
                    NodesByName.Add(node.Name, node);
            }

            List<Node> nodesToRemove = new();

            foreach (var pair in NodesByName)
                if (!Nodes.Contains(pair.Value))
                    nodesToRemove.Add(pair.Value);

            nodesToRemove.Clear();
        }
        public Node[] FindNodesByTag(string tag)
        {
            OnNodeQueryMade?.Invoke();
            IEnumerable<Node> matchingNodes = Nodes.Where(node => node.tag == tag);
            return matchingNodes.ToArray();
        }
        public Node FindNodeByTag(string tag)
        {
            OnNodeQueryMade?.Invoke();
            return Nodes
                    .Where(node => node.tag == tag)
                    .First();
        }
        public Node? FindNode(string name)
        {
            OnNodeQueryMade?.Invoke();
            IEnumerable<Node> result = (
                from node
                in Nodes
                where node.Name.Equals(name)
                select node); 
            return result.Any() ? result.First() : null; 

        }
        public void AddNode(Node node)
        {
            Action<object[]> add_node = (o) => { Nodes.Add(o[0] as Node); };
            object[] args = { node };

            if (FixedUpdateBusy)
            {
                DelayedActionArgsQueue.Enqueue(args);
                DelayedActionQueue.Enqueue(add_node);
            }
            else add_node(args); 

        }
       
        public void create_generic_node()
        {
            // random variables used here;
            object[] args = r_node_args();
            
            var node = new Node($"NODE {(int)args[0]}", (Vec2)args[1], Vec2.one);
            var sprite = new Sprite((Vec2)args[2], (Color)args[3], false);
            node.AddComponent(sprite);
            node.AddComponent(new Rigidbody() { IsTrigger = false });
            node.AddComponent(new Wind((Direction)args[5]));
            AddNode(node);
        }
        private object[] r_node_args()
        {
            int r_int = genericNodeCt++;
            Vec2 r_pos = JRandom.ScreenPosition();
            Vec2 r_vec = JRandom.Vec2(Vec2.one, Vec2.one * 15);
            Color r_color = JRandom.Color();
            bool r_bool = JRandom.Bool();
            Direction r_dir = JRandom.Direction();
            return new object[]
            {
                r_int,
                r_pos,
                r_vec,
                r_color,
                r_bool,
                r_dir };
        }
        
        public IEnumerable<Sprite> GetSprites()
        {
            var sprite = new Sprite();
            IEnumerable<Sprite> sprites = (from Node node in Nodes
                                           where node.TryGetComponent(out sprite)
                                           select sprite);
            return sprites;
        }
        private Bitmap GetBackground(Metadata meta)
        {
            return backgroundImage = new(meta.fullPath);
            throw new MissingMetadataException("Metadata not found."); 
        }
    }
}
