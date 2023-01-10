using Newtonsoft.Json;
using pixel_renderer.Assets;
using pixel_renderer.IO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Windows.Media.Imaging;

namespace pixel_renderer
{
    public class Stage
    {
        public Dictionary<string, Node> NodesByName { get; private set; } = new Dictionary<string, Node>();
        public Stage() { }

        public Stage(string Name, Metadata Background, List<NodeAsset> nodes, string? existingUUID = null)
        {
            _uuid = existingUUID ?? pixel_renderer.UUID.NewUUID();
            GetBackground(Background);

            this.Name = Name;
            Nodes = nodes.ToNodeList();
            Awake();
        }
        private Bitmap GetBackground(Metadata meta)
        {
            if (meta is not null && backgroundImage is null)
                if(FindOrCreateMetadataFile(meta))
                    return backgroundImage = new(meta.fullPath + meta.extension);
            throw new MissingMetadataException("Metadata not found."); 
        }

        public static bool FindOrCreateMetadataFile(Metadata meta)
        {
            var exists = File.Exists(meta.fullPath + meta.extension);
            if (exists)
                return true;
            else
            {
                var stream = File.Create(meta.fullPath + meta.extension);
                using var writer = new StreamWriter(stream);
                writer.Write(meta);
            }
            return false;
        }

        public List<Node> Nodes { get; private set; } = new();
        public Bitmap backgroundImage;
        public event Action OnNodeQueryMade;

        /// <summary>
        ///  used to keep track of how many generic nodes have been instantiated for naming
        /// </summary>
        int genericNodeCt = 0;

        public string Name { get; set; }
        private string _uuid = "";
        public string UUID => _uuid;
        public void FixedUpdate(float delta)
        {
            foreach (Node node in Nodes)
                node.FixedUpdate(delta);
        }
        public void Awake()
        {
            OnNodeQueryMade += RefreshStageDictionary;
            foreach (Node node in Nodes)
            {
                node.ParentStage = this;
                node.Awake();
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
        public Node FindNode(string name)
        {
            OnNodeQueryMade?.Invoke();
            return Nodes
                    .Where(node => node.Name == name)
                    .First();
        }
        public IEnumerable<Sprite> GetSprites()
        {
            var sprite = new Sprite();
            IEnumerable<Sprite> sprites = (from Node node in Nodes
                                           where node.TryGetComponent(out sprite)
                                           select sprite);
            return sprites;
        }
        public void AddNode(Node node) => Nodes.Add(node);
        public Stage Reset()
        {
            List<StageAsset> stageAssets = Runtime.Instance.LoadedProject.stages;
            foreach (StageAsset asset in stageAssets)
            {
                var stage = asset.Copy();
                if (stage is not null
                    && stage.UUID.Equals(UUID)) return stage;
            }
            throw new NullStageException("Stage not found on reset call");
        }

        public void create_generic_node()
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
    }
}
