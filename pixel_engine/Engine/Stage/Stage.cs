using pixel_renderer.Assets;
using pixel_renderer.FileIO;
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

    public class Stage
    {
        public Stage() { }
        public Stage(string Name, Metadata backgroundMeta, List<NodeAsset> nodes, string? existingUUID = null)
        {
            _uuid = existingUUID ?? pixel_renderer.UUID.NewUUID();
            this.backgroundMeta = backgroundMeta;
            backgroundImage = GetBackground(backgroundMeta);
            this.Name = Name;
            Nodes = nodes.ToNodeList();
            Awake();
        }

        public Metadata backgroundMeta;
        public Bitmap backgroundImage;
        
        public event Action? OnNodeQueryMade;
        
        public List<Node> Nodes { get; private set; } = new();
        
        public Dictionary<string, Node> NodesByName { get; private set; } = new Dictionary<string, Node>();
        
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

        private Bitmap GetBackground(Metadata meta)
        {
            if (File.Exists(backgroundMeta.fullPath))
                return new(meta.fullPath);
            throw new MissingMetadataException($"Metadata fullpath:\"{meta.fullPath}\". File not found.");
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
            Vec2 r_pos = JRandom.ScreenPosition();
            Vec2 r_vec = JRandom.Vec2(Vec2.one* 2, Vec2.one * (Constants.CollisionCellSize - 1));
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

        public Node? FindNodeWithComponent<T>() where T : Component
        {
            IEnumerable<Node> outNode = from node in Nodes let hasComponent = node.HasComponent<T>()  where hasComponent select node;
            if (outNode.Count() == 0) return null; 
            Node first = outNode.First();
            return first; 
        }
        public List<Node>? FindNodesWithComponent<T>() where T : Component
        {
            IEnumerable<Node> outNodes = from node in Nodes let hasComponent = node.HasComponent<T>() where hasComponent select node;
            return outNodes.ToList();
        }
    }
}
