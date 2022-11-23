namespace pixel_renderer.Assets
{
    public class NodeAsset : Asset
    {
        public Node RuntimeValue;
        public NodeAsset(string name, Node runtimeValue)
        {
            RuntimeValue = runtimeValue;
            Name = name;
            fileType = typeof(Node);
        }
    }
}