using Newtonsoft.Json;
using Pixel.Assets;
using Pixel.FileIO;
using Pixel.Statics;
using Pixel.Types.Components;
using Pixel.Types.Physics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Pixel
{
    public class Stage : Asset
    {
        [JsonProperty]
        public Metadata backgroundMetadata;
        [JsonProperty]
        public Matrix3x2 bgTransform = Matrix3x2.CreateScale(128, 128);
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
                var stage = Interop.Stage;
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

        /// <summary>
        /// Checks whether all of the nodes in the stages have or havent been awoken, and if not, calls awake.
        /// </summary>
        /// <returns></returns>
        public bool Awake()
        {
            var awokenNodes = 0;
            int count = nodes.Count;
            for (int i = 0; i < count; i++)
                if (!nodes[i].awake)
                {
                    nodes[i].Awake();
                    awokenNodes++;
                }
            if (awokenNodes > 0)
                return false;
            return true;

        }

        public void Update()
        {
            NodesBusy = true;
            lock (nodes)
                for (int i = 0; i < nodes.Count; i++)
                {
                    Node node = nodes[i];
                    node.Update();
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
                for (int i = 0; i < nodes.Count; i++)
                {
                    Node node = nodes[i];
                    node.FixedUpdate(delta);
                }
            NodesBusy = false;
        }

        public void SetBackground(JImage value)
        {
            background = value;
        }
        public void SetBackground(Bitmap value)
        {
            background = new(value);
        }
        public void SetBackground(Color[,] value)
        {
            background = new(value);
        }
        public override void Sync()
        {
            string defaultPath = Constants.WorkingRoot + Constants.StagesDir + "\\" + Name + Constants.StageFileExtension;
            Metadata = new(defaultPath);
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
                BoundingBox2D box = new(sprite.GetCorners());
                if (!position.IsWithin(box.min, box.max))
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
                    return;

                if (nodes.Contains(newNode))
                    return;

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
                                           where node.sprite is not null
                                           select node.sprite);
            return sprites;
        }
        internal void RemoveNode(Node node)
        {
            object[] args = { node };


            if (NodesBusy)
            {
                delayedActionQueue.Enqueue((remove_node, args));
            }
            else remove_node(args);
        }
        void remove_node(object[] o)
        {
            if (o[0] is Node node)
                unsafe
                {
                    if (!nodes.Contains(node)) return;
                    nodes.Remove(node);

                    // TODO: remove this probably
                    Node* objPtr = &node;

                    IntPtr objIntPtr = new IntPtr(objPtr);
                    Marshal.FreeHGlobal(objIntPtr);
                };

        }




        #endregion Node Utils
        #region development defaults

        public static Metadata DefaultBackgroundMetadata
        {
            get
            {
                if (Library.FetchMeta("Background") is not Metadata meta)
                    return new(Constants.WorkingRoot + Constants.AssetsDir + "Background" + Constants.PngExt);
                return meta;
            }
        }


        public void AddNodes(List<Node> nodes)
        {
            foreach (var node in nodes)
            {
                if (this.nodes.Contains(node))
                    continue;

                AddNode(node);
            }
        }

        #endregion development defaults
        #region constructors
        
        [JsonConstructor]
        internal Stage(List<Node> nodes, Metadata metadata, Metadata backgroundMetadata, string name = "Stage Asset") : base(name, true)
        {
            Name = name;
            this.nodes = nodes;
            foreach (var node in this.nodes)
            {
                if (node is null)
                {
                    Interop.Log("JSON_ERROR: Null Node Removed From Stage.");
                    RemoveNode(node);
                    continue;
                }
                for (int x = 0; x < node.Components.Count; ++x)
                {
                    var type = node.Components.Keys.ElementAt(x);
                    var compList = node.Components[type];
                    for (int y = 0; y < compList.Count; ++y)
                    {
                        var component = compList[y];
                        if (component is null)
                        {
                            Interop.Log("JSON_ERROR: Null Component Removed From Node.");
                            node.RemoveComponent(component);
                        }
                        component.node ??= node;
                    }

                }
            }
            Metadata = metadata;
            this.backgroundMetadata = backgroundMetadata;
            init_background();
        }


        /// <summary>
        /// Memberwise copy constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="backgroundMeta"></param>
        /// <param name="nodes"></param>
        /// <param name="existingUUID"></param>
        public Stage(string name, Metadata backgroundMetadata, List<Node> nodes, string? existingUUID = null) : base(name, true)
        {
            this.Name = name;
            UUID = existingUUID ?? Pixel.Statics.UUID.NewUUID();
            this.nodes = nodes;
            this.backgroundMetadata = backgroundMetadata;
            init_background();
        }
        #endregion
    }
}