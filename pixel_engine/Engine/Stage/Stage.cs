using Newtonsoft.Json;
using pixel_renderer.Assets;
using pixel_renderer.FileIO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;

namespace pixel_renderer
{
    public class Stage : Asset
    {
        [JsonProperty]
        public Metadata backgroundMetadata; 
        [JsonProperty]
        public Vector2 backgroundSize = new(512, 512);
        [JsonProperty]
        public TextureFiltering backgroundFiltering = TextureFiltering.Point; 
        [JsonProperty]
        public Vector2 backgroundOffset = new(0, 0);
        [JsonProperty]
        public List<Node> nodes = new();

        private Queue<(Action<object[]>, object[])> delayedActionQueue = new();
        private StageRenderInfo? stage_render_info = null;
        public StageRenderInfo StageRenderInfo
        {
            get
            {
                var wasNull = stage_render_info is null;
                var stage = Runtime.Current.GetStage();
                stage_render_info ??= new(stage);

                if (!wasNull)
                    stage_render_info.Refresh(stage);

                return stage_render_info;
            }
            set { stage_render_info = value; }
        }
        public JImage GetBackground()
        {
            if (background == null && backgroundMetadata != null)
                    return background = init_background();
            return background ?? throw new NullReferenceException(nameof(background));
        }
        public JImage background = new(); 
        public bool NodesBusy { get; private set; }
        public void Awake()
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                Node node = nodes[i];
                node.ParentStage = this;
                node.Awake();
            }
        }
        public void Update()
        {
            NodesBusy = true;
            lock (nodes)
            {

                for (int i = 0; i < nodes.Count; i++)
                {
                    Node node = nodes[i];
                    node.Update();
                }
            }
            NodesBusy = false;

            for (int i = 0; delayedActionQueue.Count - 1 > 0; ++i)
            {
                (Action<object[]>, object[]) kvp = delayedActionQueue.Dequeue();

                object[] args = kvp.Item2;
                Action<object[]> action = kvp.Item1;

                action.Invoke(args);
            }
        }
        public void FixedUpdate(float delta)
        {
                NodesBusy = true;
            lock (nodes)
            {

                for (int i = 0; i < nodes.Count; i++)
                {
                    Node node = nodes[i];
                    node.FixedUpdate(delta);
                }

            }
                NodesBusy = false;
        }
        public override void Sync()
        {
            string defaultPath = Constants.WorkingRoot + Constants.StagesDir + "\\" + Name + Constants.StageFileExtension;
            Metadata = new(Name, defaultPath, Constants.StageFileExtension);
        }

        public void SetBackground(JImage value)
        {
            background = value;
        }
        public void SetBackground(Bitmap value)
        {
            background = new(value);
        }
        public void SetBackground(Pixel[,] value)
        {
            background = new(value);
        }

        private JImage init_background()
        {
            if (File.Exists(backgroundMetadata.Path))
                return background = new(new Bitmap(backgroundMetadata.Path));
            throw new MissingMetadataException($"Metadata :\"{backgroundMetadata.Path}\". File not found.");
        }
        #region Node Utils
        public List<Node> GetNodesAtGlobalPosition(Vector2 position)
        {
            List<Node> outNodes = new List<Node>();
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                if (node.GetComponent<Sprite>() is not Sprite sprite)
                    continue;
                if (!position.IsWithin(node.Position, node.Position + sprite.size))
                    continue;
                outNodes.Add(node);
            }
            return outNodes;
        }

        public void AddNode(Node node)
        {
            void add_node(object[] o)
            {
                if (o[0] is not Node newNode)
                {
                    Runtime.Log("AddNode Failed: input was not valid node.");
                    return;
                }

                if (nodes.Contains(newNode))
                {
                    Runtime.Log("AddNode Failed: node already belongs to this stage.");
                    return;
                }
                newNode.ParentStage = this;
                nodes.Add(newNode);
            }
            object[] args = { node };

            if (NodesBusy)
                delayedActionQueue.Enqueue((add_node, args));
            else add_node(args);
        }
        public Node? FindNode(string name)
        {
            IEnumerable<Node> result = (
                from node
                in nodes
                where node.Name.Equals(name)
                select node);
            return result.Any() ? result.First() : null;
        }
        public Node FindNodeByTag(string tag)
        {
            return nodes
                    .Where(node => node.tag == tag)
                    .First();
        }
        public Node[] FindNodesByTag(string tag)
        {
            IEnumerable<Node> matchingNodes = nodes.Where(node => node.tag == tag);
            return matchingNodes.ToArray();
        }
        public List<Node>? FindNodesWithComponent<T>() where T : Component
        {
            IEnumerable<Node> outNodes = from node in nodes where node.HasComponent<T>() select node;
            return outNodes.ToList();
        }
        public Node? FindNodeWithComponent<T>() where T : Component
        {
            IEnumerable<Node> collec = from node in nodes where node.HasComponent<T>() select node;

            if (!collec.Any())
                return null;

            Node first = collec.First();
            return first;
        }
        public IEnumerable<T> GetAllComponents<T>() where T : Component
        {
            return from Node node in nodes
                   from T component in node.GetComponents<T>()
                   select component;
        }
        public IEnumerable<Sprite> GetSprites()
        {
            if (nodes is null)
                return null;

            IEnumerable<Sprite> sprites = (from Node node in nodes
                                           where node.HasComponent<Sprite>()
                                           let sprite = node.GetComponent<Sprite>()
                                           where sprite is not null
                                           select sprite);
            return sprites;
        }
        private void RemoveNode(Node? node)
        {
            object[] args = { node };
            void remove_node(object[] o)
            {
                if (o[0] is not Node actionTimeNode) return;
                if (!nodes.Contains(actionTimeNode)) return;
                nodes.Remove(actionTimeNode);
            }

            if (NodesBusy)
            {
                delayedActionQueue.Enqueue((remove_node, args));
            }
            else remove_node(args);
        }




        #endregion Node Utils
        #region development defaults

        public static Metadata DefaultBackgroundMetadata
        {
            get
            {
                if (AssetLibrary.FetchMeta("Background") is not Metadata meta)
                    return new("Background", Constants.WorkingRoot + Constants.AssetsDir + "Background" + Constants.BitmapFileExtension, Constants.BitmapFileExtension);
                return meta;
            }
        }

        public static Stage Standard()
        {
            var nodes = new List<Node>();
            nodes.Add(Player.Standard());
            Node camera = new("Camera");
            Node light = new("Light");
            light.AddComponent<Light>(); 

            camera.AddComponent<Camera>().Size = new(256,256);
            nodes.Add(camera);
            Node floorNode = Floor.Standard();
            nodes.Add(floorNode);
            for (int i = 0; i < 5; i++)
            {
                Node rbNode = Rigidbody.Standard();
                rbNode.Position = new(i * 20, -20);
                nodes.Add(rbNode);
            }

            var stage = new Stage("Default Stage", DefaultBackgroundMetadata, nodes);
            return stage;
        }

        #endregion development defaults
        #region constructors
        public Stage()
        {

        }
        [JsonConstructor]
        internal Stage(List<Node> nodes, Metadata metadata, Metadata backgroundMetadata, string name = "Stage Asset") : base(name, true)
        {
            Name = name;
            this.nodes = nodes;
            foreach (var node in this.nodes)
            {
                if (node is null)
                {
                    Runtime.Log("JSON_ERROR: Null Node Removed From Stage.");
                    RemoveNode(node);
                }

                foreach (var component in node.ComponentsList)
                {
                    if (component is null)
                    {
                        Runtime.Log("JSON_ERROR: Null Component Removed From Node.");
                        node.RemoveComponent(component);
                    }
                    component.parent ??= node;
                }
            }
            Metadata = metadata;
            this.backgroundMetadata = backgroundMetadata;
            init_background(); 
        }


        /// <summary>
        /// Memberwise copy constructor
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="backgroundMeta"></param>
        /// <param name="nodes"></param>
        /// <param name="existingUUID"></param>
        internal Stage(string Name, Metadata backgroundMetadata, List<Node> nodes, string? existingUUID = null) : this()
        {
            this.Name = Name;
            UUID = existingUUID ?? pixel_renderer.UUID.NewUUID();
            this.nodes = nodes;
            this.backgroundMetadata = backgroundMetadata;
            init_background();
        }
        #endregion
    }
}