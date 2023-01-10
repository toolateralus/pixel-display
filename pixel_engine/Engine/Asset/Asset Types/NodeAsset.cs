
using Newtonsoft.Json;
using pixel_renderer.FileIO;
using System;
using System.Collections.Generic;

namespace pixel_renderer.Assets
{
    public class NodeAsset : Asset
    {
        public string nodeName;
        public string nodeUUID; 
        public Vec2 pos, scale;
        public List<Component> components = new();  
        public List<NodeAsset> children;


        [JsonConstructor]
        public NodeAsset(string nodeUUID, string nodeName , string name, string UUID, List<Component> components, Vec2 pos, Vec2 scale, List<NodeAsset> children) : base(name, typeof(Node), UUID)
        {
            this.components = components;
            this.pos = pos;
            this.scale = scale;
            this.children = children;
            this.nodeName = nodeName;
            this.nodeUUID = nodeUUID;
        }
        public NodeAsset(Node runtimeValue) : base()
        {
            nodeName = runtimeValue.Name;
            nodeUUID = runtimeValue.UUID;
            children = runtimeValue.children.ToNodeAssets();

            pos = runtimeValue.position;
            scale = runtimeValue.scale;
            fileType = typeof(Node);

            foreach (var comp in runtimeValue.ComponentsList)
                components.Add(comp);
        }
        public Node Copy()
        {
            Node node = new(nodeName, pos, scale, UUID);
            foreach (var comp in components)
                node.AddComponent(comp);
            return node;
        }
    }
    public class ComponentAsset : Asset
    {
        [JsonIgnore]
        public Component runtimeComponent;
        public ComponentAsset(string name,Component component, Type type)
        {
            runtimeComponent = component;
            fileType = type;
            Name = name;
        }
        [JsonConstructor]
        public ComponentAsset(Component runtimeComponent, string name, Type fileType, string UUID) : base(name, fileType, UUID) 
        {
            // somehow we need to keep a constant reference to the type and any data that needs to be saved; 
            // we cannot instantiate a component of unknown type at runtime. 
            // this is potentially the sole reason the scene seems to be empty when loading from an asset: the Components list and dict
            // are always null after initializing from a file;
            this.runtimeComponent = runtimeComponent; 
        }
    }
}