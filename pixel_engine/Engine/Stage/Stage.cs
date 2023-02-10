using Newtonsoft.Json;
using pixel_renderer.Assets;
using pixel_renderer.FileIO;
using pixel_renderer.Scripts;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Policy;

namespace pixel_renderer
{

    public class StageRenderInfo
    {
        public int Count => spritePositions.Count;
        
        public List<Vec2> spritePositions= new ();
        public List<Vec2> spriteSizeVectors = new();
        public List<float> spriteCamDistances = new();
        public List<Color[,]> spriteColorData = new();

        public StageRenderInfo(Stage stage)
        {
             Refresh(stage);
        }
        public void Refresh(Stage stage)
        {
            var sprites = stage.GetSprites();
            int numOfSprites = sprites.Count();
            var listSize = spritePositions.Count; 
            if (numOfSprites != listSize)
            {
                for (int i = spritePositions.Count; i < numOfSprites; ++i)
                    addMemberOnTop();
                for (int i = spritePositions.Count; i > numOfSprites; --i)
                    removeFirst();
            }
            for (int i = 0; i < sprites.Count(); ++i)
            {
                Sprite sprite = sprites.ElementAt(i);
                  spritePositions[i] = sprite.parent.position;
                spriteSizeVectors[i] = sprite.size;
                  spriteColorData[i] = sprite.ColorData;
                spriteCamDistances[i] = sprite.camDistance;
            }
            void addMemberOnTop()
            {
                spritePositions.Add(Vec2.zero);
                spriteSizeVectors.Add(Vec2.zero);
                spriteColorData.Add(new Color[1, 1]);
                spriteCamDistances.Add(1f);
            }
            void removeFirst()
            {
                spritePositions.RemoveAt(0);
                spriteSizeVectors.RemoveAt(0);
                spriteColorData.RemoveAt(0);
                spriteCamDistances.RemoveAt(0);

            }
        }
    }

    public class Stage : Asset
    {
       
        [JsonProperty]
        public List<NodeAsset> NodeAssets;
      
        public string Name { get; set; }
        private string _uuid = "";
        public string UUID => _uuid;

        public bool FixedUpdateBusy { get; private set; }
        public Metadata Background;
        public Bitmap? initializedBackground;

        int genericNodeCt = 0;
        #region Node Utils
        public event Action OnNodeQueryMade;
        public List<Node> Nodes { get; private set; } = new();
        public Dictionary<string, Node> NodesByName { get; private set; } = new Dictionary<string, Node>();
        #endregion

        #region  Misc Utils
        private StageRenderInfo? stage_render_info = null;
        public StageRenderInfo StageRenderInfo
        {
            get
            {
                var wasNull = stage_render_info is null;
                var stage = Runtime.Instance.GetStage(); 
                stage_render_info ??= new(stage);
                
                if (!wasNull)
                    stage_render_info.Refresh(stage); 

                return stage_render_info;
            }
            set { stage_render_info = value; }
        }

        Queue<Action<object[]>> DelayedActionQueue = new();
        Queue<object[]> DelayedActionArgsQueue = new();
        #endregion


        #region development defaults
        public static Metadata DefaultMetadata = new("Default Stage Asset", Constants.WorkingRoot + Constants.AssetsDir + "\\DefaultStage" + Constants.AssetsFileExtension, Constants.AssetsFileExtension);
        public static Metadata DefaultBackground = new("Default Stage Asset Background", Constants.WorkingRoot + Constants.ImagesDir + "\\home.bmp", ".bmp");
        public static Stage Default()
        {
            var nodes = new List<NodeAsset>();
            nodes.Add(Player.Standard().ToAsset());

            for (int i = 0; i < 5; i++)
                nodes.Add(Rigidbody.Standard().ToAsset());

            var stage = new Stage("Default Stage", DefaultBackground, nodes);
          
            return stage;
        }
        #endregion
        #region Engine Stuff
        public void Awake()
        {
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
            void add_node(object[] o)
            {
                if (o[0] is not Node newNode) return;
                if (Nodes.Contains(newNode)) return;
                newNode.ParentStage = this;
                Nodes.Add(newNode);
            }
            object[] args = { node };

            if (FixedUpdateBusy)
            {
                DelayedActionArgsQueue.Enqueue(args);
                DelayedActionQueue.Enqueue(add_node);
            }
            else add_node(args); 

        }
        public Stage Copy()
        {
            var output = new Stage(Name, Background, NodeAssets, UUID);
            return output;
        }
        public Bitmap GetBackground()
        {
            if (File.Exists(Background.fullPath))
                return new(Background.fullPath);
            throw new MissingMetadataException($"Metadata :\"{Background.fullPath}\". File not found.");
        }

        public IEnumerable<Sprite> GetSprites()
        {
            var sprite = new Sprite();
            IEnumerable<Sprite> sprites = (from Node node in Nodes
                                           where node.TryGetComponent(out sprite)
                                           select sprite);
            return sprites;
        }
        
        public IEnumerable<T> GetAllComponents<T>() where T : Component
        {
            return from Node node in Nodes
                from T component in node.GetComponents<T>()
                   select component;
        }
        
        public Node? FindNodeWithComponent<T>() where T : Component
        {
            IEnumerable<Node> collec = from node in Nodes where node.HasComponent<T>() select node;

            if (!collec.Any())
                return null;

            Node first = collec.First();
            return first;
        }
        
        public List<Node>? FindNodesWithComponent<T>() where T : Component
        {
            IEnumerable<Node> outNodes = from node in Nodes where node.HasComponent<T>() select node;
            return outNodes.ToList();
        }
        #endregion

        public void create_generic_node()
        {
            // random variables used here;
            object[] args = r_node_args();

            int name_ct = (int)args[0];
            Vec2 pos = (Vec2)args[1];
            Vec2 scale = Vec2.one;
            var node = new Node($"NODE {name_ct}", pos, scale);

            var sprite = new Sprite(24, 24);
            node.AddComponent(sprite);
            var collider = new Collider()
            {
                size = sprite.size,
                IsTrigger = false
            };
            
            node.AddComponent(collider);

            if(JRandom.Bool()) 
                node.AddComponent<Rigidbody>();

            AddNode(node);
        }
        private object[] r_node_args()
        {
            int r_int = genericNodeCt++;
            Vec2 r_pos = JRandom.Vec2(Vec2.zero, Vec2.one * 256);

            Vec2 r_vec = JRandom.Vec2(Vec2.one * 2, Vec2.one * (Constants.CollisionCellSize - 1));
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
                r_dir 
            };
        }
        
        [JsonConstructor]
        internal Stage(string name, List<NodeAsset> nodes, Metadata metadata, Metadata background, string UUID) : base(name, UUID)
        {
            NodeAssets = nodes;
            Metadata = metadata;
            Background = background;
        }
        /// <summary>
        /// Memberwise copy constructor
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="backgroundMeta"></param>
        /// <param name="nodes"></param>
        /// <param name="existingUUID"></param>
        internal Stage(string Name, Metadata backgroundMeta, List<NodeAsset> nodes, string? existingUUID = null)
        {
            this.Name = Name;
            _uuid = existingUUID ?? pixel_renderer.UUID.NewUUID();
            Nodes = nodes.ToNodeList();
            OnNodeQueryMade = RefreshStageDictionary;

            Background = backgroundMeta;
            initializedBackground = GetBackground();
            
            Awake();
        }
        public void Sync()
        {
            Metadata = new(Name, Constants.WorkingRoot + Constants.StagesDir + "\\" + Name + Constants.StageFileExtension, Constants.StageFileExtension);
            NodeAssets = Nodes.ToNodeAssets();
            StageIO.WriteStage(this);
        }
        public Stage()
        {
        }



    }
}
