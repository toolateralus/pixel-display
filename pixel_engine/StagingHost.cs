
using Pixel.Assets;
using Pixel.FileIO;
using Pixel.Types.Physics;
using System.Collections.Generic;
using System.Numerics;

namespace Pixel
{
    public class StagingHost
    {
        public Node? lastSelected;
        public static Stage Standard()
        {
            var nodes = new List<Node>();

            Node camera = new("camera");
            camera.AddComponent<Camera>();
            camera.Position = new(0, 0);
            float fov = 3; 
            camera.Scale = new Vector2(16, 9) * fov;

            Node floor = Floor.Standard();
            nodes.Add(floor);

            Node textTest = new("text");
            var img = textTest.AddComponent<Text>();

            img.viewportSize = new(1, 1);
            textTest.Scale = new(25, 25);
            img.viewportPosition = Vector2.Zero;
            textTest.Position = Vector2.Zero;

            Node light = Light.Standard();
            light.AddComponent<SineAnimationTests>();

            nodes.Add(textTest);
            nodes.Add(light);
            nodes.Add(camera);

            for (int i = 0; i < 5; i++)
            {
                Node rbNode = Rigidbody.Standard();
                Sprite spr = rbNode.GetComponent<Sprite>();

                Metadata meta = Library.FetchMeta("ball");
                spr.texture.SetImage(meta);

                rbNode.Position = new(i * 20, -20);
                nodes.Add(rbNode);
            }

            var stage = new Stage("default stage", Stage.DefaultBackgroundMetadata, nodes);
            return stage;
        }
        /// <summary>
        /// this is used for editor clicking.
        /// </summary>
        /// <param name="stage"></param>
        /// <param name="clickPosition"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool GetNodeAtPoint(Stage stage, Vector2 clickPosition, out Node? result)
        {
            foreach (var node in stage.nodes)
            {
                if (node == lastSelected)
                {
                    DeselectNode();
                }

                if (node.GetComponent<Sprite>() is not Sprite sprite)
                    continue;

                BoundingBox2D box = new(sprite.GetCorners());
                if (!clickPosition.IsWithin(box.min, box.max))
                    continue;

                result = node;

                SelectNode(node);

                DeselectNode();

                lastSelected = node;

                return true;
            }
            result = null;
            return false;
        }
        private static void SelectNode(Node node)
        {
            foreach (var comp in node.Components)
                foreach (var c in comp.Value)
                    c.selected_by_editor = true;
        }

        public void DeselectNode()
        {
            if (lastSelected != null)
                foreach (var comp in lastSelected.Components)
                    foreach (var c in comp.Value)
                        c.selected_by_editor = false;
        }
        public static void FixedUpdate(Stage stage)
        {
            var delta = Runtime.Current.renderHost.info.FrameTime;
            stage.FixedUpdate((float)delta);
        }
        public static void Update(Stage m_stage)
        {
            m_stage.Update();
        }
    }
}