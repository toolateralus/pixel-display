using Newtonsoft.Json;
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

    public class Stage : Asset
    {
        
        [JsonProperty]
        public Metadata Background;

        private Bitmap init_bckground;
        public Bitmap? InitializedBackground 
        { 
            get 
            {
                if (init_bckground is null && Background != null)
                    init_bckground = GetBackground();
                else init_bckground ??= new(256,256);
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
        private void RemoveNode(Node? node)
        {
            object[] args = { node };
            void remove_node(object[] o)
            {
                if (o[0] is not Node actionTimeNode) return;
                if (!nodes.Contains(actionTimeNode)) return; 
                nodes.Remove(actionTimeNode);
            }

            // this is a better way than using a lock statement, as far as i know
            if (NodesBusy)
            {
                delayedActionQueue.Enqueue((remove_node, args));
            }
            else remove_node(args);
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
        public Node[] FindNodesByTag(string tag)
        {
            IEnumerable<Node> matchingNodes = nodes.Where(node => node.tag == tag);
            return matchingNodes.ToArray();
        }
        public Node FindNodeByTag(string tag)
        {
            return nodes
                    .Where(node => node.tag == tag)
                    .First();
        }
        public Node? FindNodeWithComponent<T>() where T : Component
        {
            IEnumerable<Node> collec = from node in nodes where node.HasComponent<T>() select node;

            if (!collec.Any())
                return null;

            Node first = collec.First();
            return first;
        }
        
        public List<Node>? FindNodesWithComponent<T>() where T : Component
        {
            IEnumerable<Node> outNodes = from node in nodes where node.HasComponent<T>() select node;
            return outNodes.ToList();
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

        #region Node Utils
        [JsonProperty]
        public List<Node> nodes = new();
        public bool NodesBusy { get; private set; }
        #endregion

        #region  Misc Utils
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

        Queue<(Action<object[]>, object[])> delayedActionQueue = new();

        #endregion

        #region Physics Stuff


        #endregion
        #region development defaults

        public static Metadata DefaultBackgroundMetadata
        {
            get
            {
                if(AssetLibrary.FetchMeta("Background") is not Metadata meta)
                    return new("Error", Constants.WorkingRoot + Constants.AssetsDir + "Error" + Constants.BitmapFileExtension, Constants.BitmapFileExtension);
                return meta;
            }
        }

        public static Stage Default()
        {
            var nodes = new List<Node>();
            nodes.Add(Player.Standard());

            for (int i = 0; i < 5; i++)
                nodes.Add(Rigidbody.Standard());

            var stage = new Stage("Default Stage", DefaultBackgroundMetadata, nodes);
          
            return stage;
        }
        #endregion
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
        public void FixedUpdate(float delta)
        {
            lock (nodes)
            {
                NodesBusy = true;

                for (int i = 0; i < nodes.Count; i++)
                {
                    Node node = nodes[i];
                    node.FixedUpdate(delta);
                }

                NodesBusy = false;
            }

        }
        public void Update()
        {
            lock (nodes)
            {
                NodesBusy = true;

                for (int i = 0; i < nodes.Count; i++)
                {
                    Node node = nodes[i];
                    node.Update();
                }
                NodesBusy = false;
            }

            for (int i = 0; delayedActionQueue.Count - 1 > 0; ++i)
            {
                (Action<object[]>, object[]) kvp = delayedActionQueue.Dequeue();

                object[] args = kvp.Item2;
                Action<object[]> action = kvp.Item1;

                action.Invoke(args);
            }

        }

       


        public Stage Copy()
        {
            var output = new Stage(Name, Background, nodes, UUID);
            return output;
        }
        public Bitmap GetBackground()
        {
            if (File.Exists(Background.fullPath))
                return new(Background.fullPath);
            throw new MissingMetadataException($"Metadata :\"{Background.fullPath}\". File not found.");
        }
        
        
        #endregion

        [JsonConstructor]
        internal Stage(List<Node> nodes, Metadata metadata, Metadata background, string name = "Stage Asset") : base(name, true) 
        {
            Name = name;
            this.UUID = UUID;
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

        public override void Sync()
        {
            string defaultPath = Constants.WorkingRoot + Constants.StagesDir + "\\" + Name + Constants.StageFileExtension;
            Metadata = new(Name, defaultPath, Constants.StageFileExtension);
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
        public Stage()
        {
        }

    }
}
