
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

        public Node Copy()
        {
            Node node = new(nodeName, pos, scale, UUID);
            foreach (var comp in components)
                node.AddComponent(comp);
            return node;
        }
        
        public List<Component> components = new();  
        public List<NodeAsset> children;

        [JsonConstructor]
        public NodeAsset(string nodeUUID, string nodeName , string name, string UUID, List<Component> components, Vec2 pos, Vec2 scale, List<NodeAsset> children) : base(name, UUID)
        {
            nodeName = name;
            Name = name;
            this.UUID = UUID; 

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
            Name = nodeName; 

            nodeUUID = runtimeValue.UUID;
            UUID = nodeUUID; 
            
            children = runtimeValue.children.ToNodeAssets();
            
            pos = runtimeValue.position;
            scale = runtimeValue.scale;
            
            foreach (var comp in runtimeValue.ComponentsList)
                components.Add(comp);
        }
        
     
    }
    public class ComponentAsset : Asset
    {
        [JsonIgnore]
        public Component runtimeComponent;
        public ComponentAsset(string name,Component component)
        {
            runtimeComponent = component;
            Name = name;
        }

        [JsonConstructor]
        public ComponentAsset(Component runtimeComponent, string name, string UUID) : base(name, UUID) 
        {
            this.runtimeComponent = runtimeComponent; 
            this.UUID = UUID;
            Name = name;
        }
    }
}