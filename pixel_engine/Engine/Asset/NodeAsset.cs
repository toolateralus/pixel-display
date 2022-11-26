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
        public Components Components = new();
        public Vec2 pos, scale;
        public List<NodeAsset> children;

        [JsonConstructor]
        public NodeAsset(string nodeUUID, string nodeName , Components components, Vec2 pos, Vec2 scale, List<NodeAsset> children)
        {
            Components = components;
            this.pos = pos;
            this.scale = scale;
            this.children = children;
            this.nodeName = nodeName;
            this.nodeUUID = nodeUUID;
        }
        public NodeAsset(string name, Node runtimeValue)
        {
            nodeName = runtimeValue.Name;
            UUID = runtimeValue.UUID;
            pos = runtimeValue.position;
            scale = runtimeValue.scale;
            fileType = typeof(Node);
            foreach (var comp in runtimeValue.ComponentsList)
            Components.Add(comp.ToAsset());
        }
        public Node Copy()
        {
            Node node = new Node();
            foreach (var comp in Components)
                node.AddComponent((Component)Activator.CreateInstance(comp.fileType));
            node.Name = nodeName;
            node.UUID = nodeUUID;
            node.position = pos;
            node.scale = scale;
            return node; 
        }
    }

    public class ComponentAsset : Asset
    {
        public Component runtimeValue;
        public ComponentAsset(string name, Component component, Type type)
        {
            runtimeValue = component;
            fileType = type; 
            Name = name;
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [JsonArray]
    public class Components : List<ComponentAsset>
    { }
}