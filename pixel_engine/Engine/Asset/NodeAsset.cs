using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Windows.Documents;

namespace pixel_renderer.Assets
{
    public class NodeAsset : Asset
    {
        public string nodeName;
        public string nodeUUID; 
        public List<ComponentAsset> components = new();
        public Vec2 pos, scale;
        public List<NodeAsset> children;

        [JsonConstructor]
        public NodeAsset(string nodeUUID, string nodeName , string name, string UUID, List<ComponentAsset> components, Vec2 pos, Vec2 scale, List<NodeAsset> children) : base(name, typeof(Node), UUID)
        {
            this.components = components;
            this.pos = pos;
            this.scale = scale;
            this.children = children;
            this.nodeName = nodeName;
            this.nodeUUID = nodeUUID;
        }
        public NodeAsset(Node runtimeValue)
        {
            nodeName = runtimeValue.Name;
            nodeUUID = runtimeValue.UUID;
            children = runtimeValue.children.ToNodeAssets() ;
            pos = runtimeValue.position;
            scale = runtimeValue.scale;
            fileType = typeof(Node);
            foreach (var comp in runtimeValue.ComponentsList)
            components.Add(comp.ToAsset());
        }
        public Node Copy()
        {
            Node node = new Node(nodeName, pos, scale, UUID);
            foreach (var comp in components)
                node.AddComponent((Component)Activator.CreateInstance(comp.fileType));
            return node; 
        }
    }

    public class ComponentAsset : Asset
    {
        //public Component runtimeValue;
        public ComponentAsset(string name, Component component, Type type)
        {
           // runtimeValue = component;
            fileType = type;
            Name = name;
        }
        [JsonConstructor]
        public ComponentAsset(string name, Type type, string UUID) : base(name, type, UUID)
        {
        }
    }
}