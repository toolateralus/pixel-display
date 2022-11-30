using Newtonsoft.Json;
using pixel_renderer.Assets;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace pixel_renderer
{


    public class Stage
    {
        public string Name { get; set; }
        private string _uuid = "";

        public string UUID { get { return _uuid; } init => _uuid = pixel_renderer.UUID.NewUUID(); }
        public event Action OnQueryMade;

        [JsonIgnore]
        public Dictionary<string, Node> NodesByName { get; private set; } = new Dictionary<string, Node>();
        public List<Node> Nodes { get; private set; } = new(); 
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
        /// <summary>
        /// ordered as follows : 0 = Int, 1 =  ScreenPos Vec2, 2 =  1-15 Vec2,  3 = Color, 4 = Bool, 5 = direction;  
        /// </summary>
        /// <returns></returns>
        public object[] r_node_args()
        {
            int r_int = JRandom.Int(0,255);
            Vec2 r_pos = JRandom.ScreenPosition();
            Vec2 r_vec = JRandom.Vec2(Vec2.one, Vec2.one * 15);
            Color r_color = JRandom.Color();
            bool r_bool = JRandom.Bool();
            Direction r_dir = JRandom.Direction();
            return new object[] { r_int, r_pos, r_vec, r_color, r_bool, r_dir }; 
        }
        public void CreateGenericNode()
        {
            // random variables used here;
            object[] args = r_node_args(); 
            var node = new Node($"NODE {(int)args[0]}", (Vec2)args[1], Vec2.one);
            var sprite = new Sprite((Vec2)args[2], (Color)args[3], (bool)args[4]); 
            node.AddComponent(sprite);
            node.AddComponent(new Rigidbody()
            {
                IsTrigger = false,
                usingGravity = true,
                drag = .1f
            });
            node.AddComponent(new Wind((Direction)args[5]));
            AddNode(node);
        }
        public void RefreshStageDictionary()
        {
            foreach (Node node in Nodes)
            {
                if(node.Name is null) continue;
                if (!NodesByName.ContainsKey(node.Name))
                    NodesByName.Add(node.Name, node);
            }
               
        }
        public void AddNode(Node node) => Nodes.Add(node);
        public void FixedUpdate(float delta)
        { foreach (Node node in Nodes) node.FixedUpdate(delta); }
        public void Awake()
        {
            OnQueryMade += RefreshStageDictionary;
            foreach (Node node in Nodes)
            {
                node.ParentStage = this;
                node.Awake();
            }
        }
        public BitmapAsset Background = new("", null);
        public StageSettings Settings => new(Name, this.UUID);
        [JsonConstructor]
        public Stage(string Name, BitmapAsset Background, List<NodeAsset> nodes)
        {
            this.Name = Name;
            this.Background = Background;
            Nodes = nodes.ToNodeList();
            Awake();
        }
        public IEnumerable<Sprite> GetSprites()
        {
            var sprite = new Sprite(); 
            IEnumerable<Sprite> sprites =(from Node node in Nodes
                                          where node.TryGetComponent(out sprite)
                                          select sprite);
            return sprites;  
        }
        public Stage Reset()
        {
            List<StageAsset> stageAssets = Runtime.Instance.LoadedProject.stages;
            foreach (StageAsset asset in stageAssets)
                if (asset.settings.UUID.Equals(UUID) 
                    && asset.settings is not null)
                     return asset.Copy();
            throw new NullStageException("Stage not found on reset call"); 
        }
        internal void Dispose()
        {
            NodesByName.Clear();
            Nodes.Clear();
            Reset(); 
        }
    }
   
}
