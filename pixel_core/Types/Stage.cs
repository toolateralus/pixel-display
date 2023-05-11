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
    /// <summary>
    /// The container that represents a 2D Scene.
    /// </summary>
    public class Stage : Asset
    {
        [JsonProperty]
        public Metadata? backgroundMetadata;
        [JsonProperty]
        public Matrix3x2 bgTransform = Matrix3x2.CreateScale(512, 512);
        [JsonProperty]
        public TextureFiltering backgroundFiltering = TextureFiltering.Point;
        [JsonProperty]
        public Vector2 backgroundOffset = new(0, 0);
        /// <summary>
        /// The base image/ skybox that everything else will be drawn on/behind.
        /// </summary>
        [JsonProperty]
        public JImage background = new();

        /// <summary>
        /// the collection of nodes that belong to this stage.
        /// </summary>
        [JsonProperty]
        public Hierarchy nodes = new(); 

        private StageRenderInfo? stage_render_info = null;
        /// <summary>
        /// A collection of data used to render this stage's renderables.
        /// </summary>
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

        /// <summary>
        /// Backup background image data.
        /// </summary>
        public static Metadata? DefaultBackgroundMetadata
        {
            get
            {
                if (Library.FetchMeta("Background") is not Metadata meta)
                    return null;
                return meta;
            }
        }

        #region Events/Messages
        public Action OnDrawShapes;
        public event Action Awake;
        public event Action Update;
        public event Action<float> FixedUpdate;
        public void UpdateMethod()
        {
            Awake?.Invoke();
            Update?.Invoke();
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
            if (backgroundMetadata is not null && File.Exists(backgroundMetadata.Path))
                return background = new(new Bitmap(backgroundMetadata.Path));
            return background = new(CBit.SolidColorSquare(new(16,16), System.Drawing.Color.Gray));
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

            node.SubscribeToEngine(true);

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
                yield break;

            for (int i = 0; i < nodes.Count; i++)
            {
                Node node = nodes[i];
                if (node is null)
                    continue;

                lock (node.Components)
                {

                    for (int z = 0; z < node.Components.Count; ++z)
                    {

                        var group = node.Components.ElementAt(z);

                        if (group.Value is null || group.Value.Count == 0)
                            continue;

                        List<T> components = group.Value.OfType<T>().ToList();
                        foreach (T component in components)
                        {
                            yield return component;
                        }
                    }
                }

                for (int i1 = 0; i1 < node.children.Count; i1++)
                {
                    Node child = node.children[i1];
                    if (child.Components.TryGetValue(typeof(T), out List<Component>? components))
                    {
                        // Defensive copying to allow modification while enumerating
                        List<T> components_found = components.OfType<T>().ToList();

                        foreach (T component in components_found)
                        {
                            yield return component;
                        }
                    }
                }
            }
        }
        public IEnumerable<Sprite> GetSprites()
        {
            if (nodes is null)
                return null;
            List<Sprite> sprites = new();
            for (int i = 0; i < nodes.Count; i++)
            {
                Node? root = nodes[i];
                if (root.sprite != null)
                    sprites.Add(root.sprite);

                for (int i1 = 0; i1 < root.children.Count; i1++)
                {
                    Node? child = root.children[i1];
                    if (child.sprite != null)
                        sprites.Add(child.sprite);
                }
            }
            return sprites;
        }
        internal void RemoveNode(Node node)
        {
            if (!nodes.Contains(node))
                return;
            nodes.Remove(node);
            node.SubscribeToEngine(false);
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
        public Stage(string name, Metadata? backgroundMetadata, Hierarchy nodes, string? existingUUID = null) : base(name, true)
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