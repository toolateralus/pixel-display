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
            RefreshStageDictionary();
            OnQueryMade += RefreshStageDictionary;
            foreach (Node node in Nodes)
            {
                node.ParentStage ??= this;
                node.Awake();
            }
        }
        public BitmapAsset Background = new("", null);
        public static Stage New => new("New Stage", new("",new Bitmap(256, 256)), new());
        public StageSettings Settings => new(Name, this.UUID);
        [JsonConstructor]
        public Stage(string Name, BitmapAsset Background, List<Node> nodes)
        {
            this.Name = Name;
            this.Background = Background;
            Nodes = nodes;
            Awake();
            RefreshStageDictionary();
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
    }
   
}
