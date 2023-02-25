using Newtonsoft.Json;
using pixel_renderer.Assets;
using pixel_renderer.FileIO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace pixel_renderer
{
    public class Stage : Asset
    {
        [JsonProperty]
        public Metadata Background; 
        [JsonProperty]
        public Vec2 backgroundSize = new(512, 512);
        [JsonProperty]
        public TextureFiltering backgroundFiltering = TextureFiltering.Point; 
        [JsonProperty]
        public Vec2 backgroundOffset = new(0, 0);
        private Bitmap init_bckground;
        public Bitmap? InitializedBackground
        {
            get
            {
                if (init_bckground is null && Background != null)
                    init_bckground = GetBackground();
                else init_bckground ??= new(256, 256);
                return init_bckground;
            }
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
        public override void Sync()
        {
            string defaultPath = Constants.WorkingRoot + Constants.StagesDir + "\\" + Name + Constants.StageFileExtension;
            Metadata = new(Name, defaultPath, Constants.StageFileExtension);
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
        #region Node Utils

        [JsonProperty]
        public List<Node> nodes = new();
        public bool NodesBusy { get; private set; }

        #endregion Node Utils

        #region Misc Utils

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
        #endregion Misc Utils



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

        #region Engine Stuff

        public void Awake()
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                Node node = nodes[i];
                node.ParentStage = this;
                node.Awake();
            }
        }
        public Stage Copy()
        {
            var output = new Stage(Name, Background, nodes, UUID);
            return output;
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
        public Bitmap GetBackground()
        {
            if (File.Exists(Background.Path))
                return new(Background.Path);
            throw new MissingMetadataException($"Metadata :\"{Background.Path}\". File not found.");
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
        #endregion Engine Stuff

        public Stage()
        {
        }
        [JsonConstructor]
        internal Stage(List<Node> nodes, Metadata metadata, Metadata background, string name = "Stage Asset") : base(name, true)
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
            Background = background;
        }
        public List<Node> GetNodesAtGlobalPosition(Vec2 position)
        {
            List<Node> outNodes = new List<Node>();
            for(int i = 0; i < nodes.Count; i++)
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
        /// <summary>
        /// Memberwise copy constructor
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="backgroundMeta"></param>
        /// <param name="nodes"></param>
        /// <param name="existingUUID"></param>
        internal Stage(string Name, Metadata backgroundMeta, List<Node> nodes, string? existingUUID = null) : this()
        {
            this.Name = Name;
            UUID = existingUUID ?? pixel_renderer.UUID.NewUUID();
            this.nodes = nodes;
            Background = backgroundMeta;
        }
    }
}