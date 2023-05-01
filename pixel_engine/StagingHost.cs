
using Pixel.Assets;
using Pixel.FileIO;
using Pixel.Types.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Pixel
{
    public class StagingHost
    {
        public Node? lastSelected;
        public static Stage Standard()
        {
            var stage = new Stage("default stage", Stage.DefaultBackgroundMetadata, new());
            Node floor = Floor.Standard();

            Node node = new("camera");
            var cam = node.AddComponent<CameraController>();
           
            stage.AddNode(floor);
            stage.AddNode(node);
           
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

                DeselectNode();

                SelectNode(node);

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
        public static void Update(Stage m_stage)
        {
            m_stage.UpdateMethod();
        }
    }
}