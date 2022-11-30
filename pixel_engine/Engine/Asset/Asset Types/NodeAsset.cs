using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
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
            {
                var name = comp.fileType.Name;
                switch(name)
                {
                    case "Text":
                        node.AddComponent(comp.runtimeComponent as Text);
                        break;
                    case "Player":
                        node.AddComponent(comp.runtimeComponent as Player);
                        break;
                    case "Rigidbody":
                        node.AddComponent(comp.runtimeComponent as Rigidbody);
                        break;
                    case "Sprite":
                        node.AddComponent(comp.runtimeComponent as Sprite);
                        break;
                    case "Wind":
                        node.AddComponent(comp.runtimeComponent as Wind);
                        break;
                    default: 
                        throw new MissingComponentException();

                }
                 
            }
            return node;
        }

        private static List<Type> GetInheritedTypesFromBase<T>()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
               .SelectMany(domainAssembly => domainAssembly.GetTypes())
               .Where(type => typeof(T).IsAssignableFrom(type)).ToList();
            return types; 
        }
    }

    public class ComponentAsset : Asset
    {
        public Component runtimeComponent;
        [JsonConstructor]
        public ComponentAsset(Component runtimeComponent, string name, Type fileType, string UUID) : base(name, fileType, UUID)
        {
            this.runtimeComponent = runtimeComponent;
        }
        public ComponentAsset(string name, Component component, Type type)
        {
            runtimeComponent = component;
            fileType = type;
            Name = name;
        }
    }
  
  
}