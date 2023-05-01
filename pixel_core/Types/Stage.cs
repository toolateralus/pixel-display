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
using System.Windows;

namespace Pixel
{
    public class Stage : Asset
    {
        [JsonProperty]
        public Metadata backgroundMetadata;
        [JsonProperty]
        public Matrix3x2 bgTransform = Matrix3x2.CreateScale(512, 512);
        [JsonProperty]
        public TextureFiltering backgroundFiltering = TextureFiltering.Point;
        [JsonProperty]
        public Vector2 backgroundOffset = new(0, 0);
        [JsonProperty]
        public JImage background = new();

        [JsonProperty]
        public Hierarchy nodes = new(); 

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

        public static Metadata DefaultBackgroundMetadata
        {
            get
            {
                if (Library.FetchMeta("Background") is not Metadata meta)
                    return new(Constants.WorkingRoot + Constants.AssetsDir + "Background" + Constants.PngExt);
                return meta;
            }
        }
        public bool NodesBusy { get; private set; }

        #region Events/Messages
        public event Action Awake;
        public event Action Update;
        public event Action<float> FixedUpdate;

        public void UpdateMethod()
        {
            Awake?.Invoke();
            Update?.Invoke();
        }
        public void FixedUpdateMethod(ref float delta)
        {
            FixedUpdate?.Invoke(delta);
        }
        public void FixedUpdateMethod(float delta)
        {
            FixedUpdate?.Invoke(delta);
        }
        #endregion


        #region Background Helpers

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
        public JImage GetBackground()
        {
            if (background == null && backgroundMetadata != null)
                return background = init_background();
            return background ?? throw new NullReferenceException(nameof(background));
        }
        private JImage init_background()
        {
            if (File.Exists(backgroundMetadata.Path))
                return background = new(new Bitmap(backgroundMetadata.Path));
            throw new MissingMetadataException($"Metadata :\"{backgroundMetadata.Path}\". File not found.");
        }
        
        #endregion
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
            if (nodes.Contains(node))
                return;

            node.ParentStage = this;
            node.SubscribeToEngine(true, this);

            if(node.parent is null)
                nodes.Add(node);
          
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
        public Node? FindNode(string name)
        {
            return nodes.Find(name);
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
            if (nodes is null)
                return null;
            List<T> components_found = new();
            for (int i = 0; i < nodes.Count; i++)
            {
                Node? root = nodes[i];
                foreach (var group in root.Components)
                    for (int i1 = 0; i1 < group.Value.Count; i1++)
                    {
                        Component? component = group.Value[i1];
                        if (component is T val)
                        components_found.Add(val);
                    }

                for (int i2 = 0; i2 < root.children.Count; i2++)
                {
                    Node? child = root.children[i2];
                    if (child.Components.ContainsKey(typeof(T)))
                    {
                        foreach (var component in child.Components)
                        {
                            var comp = component.Value.Where(c => c.GetType() == typeof(T));
                            if(comp is T val)
                                components_found.Add(val);

                        }
                    }
                }
            }
            return components_found;
        }
        public IEnumerable<Sprite> GetSprites()
        {
            if (nodes is null)
                return null;
            List<Sprite> sprites = new();
            foreach (var root in nodes)
            {
                if (root.sprite != null)
                    sprites.Add(root.sprite);

                foreach (var child in root.children)
                    if (child.sprite != null)
                        sprites.Add(child.sprite);
            }
            return sprites;
        }
        internal void RemoveNode(Node node)
        {
            unsafe
            {
                if (!nodes.Contains(node)) return;
                nodes.Remove(node);

                node.SubscribeToEngine(false, this);
                // TODO: remove this probably

                Node* objPtr = &node;

                IntPtr objIntPtr = new IntPtr(objPtr);
                Marshal.FreeHGlobal(objIntPtr);
            };
        }
      
        public override void Sync()
        {
            string defaultPath = Constants.WorkingRoot + Constants.StagesDir + "\\" + Name + Constants.StageFileExtension;
            Metadata = new(defaultPath);
        }
        #endregion Node Utils
        #region constructors
        
        [JsonConstructor]
        internal Stage(Hierarchy nodes, Metadata metadata, Metadata backgroundMetadata, string name = "Stage Asset") : base(name, true)
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
        public Stage(string name, Metadata backgroundMetadata, Hierarchy nodes, string? existingUUID = null) : base(name, true)
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