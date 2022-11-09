namespace pixel_renderer
{
    using System;
    using System.Linq;
    using System.Windows.Controls;

    [Obsolete]
    public static class Debug
    {
        // really sloppy quick implementation using the most wasteful string possible -- must be totally revised.
        // crashes on any stage containing more than 10 nodes XD
        [Obsolete("DO NOT USE ON STAGES CONTAINING MORE THAN 10 NODES! IT WILL CRASH!")]
        public static string debug = ""; // move to Runtime class
        [Obsolete("DO NOT USE ON STAGES CONTAINING MORE THAN 10 NODES! IT WILL CRASH!")]
        public static bool debugging;  // move to Runtime class
        // take a string, create label, populate label, insert to feed. 
        // auto sizing XML grid could create a chat feed look with expandable fields
        [Obsolete("DO NOT USE ON STAGES CONTAINING MORE THAN 10 NODES! IT WILL CRASH!")]
        public static void Log(TextBox outputTextBox)
        {
            var runtime = Runtime.Instance;
            Stage stage = runtime.stage;
            outputTextBox.Text =
            $" ===STATS===: \n\t {Rendering.FrameRate()} Frames Per Second \n PLAYER STATS : {stage.FindNode("Player").GetComponent<Rigidbody>().GetDebugs()}\t " +
            $"\n RB_DRAG :{stage.FindNode("Player").GetComponent<Rigidbody>().GetDrag()}" +
            $"\n\t Current Room : {runtime.BackroundIndex}";
            outputTextBox.Text +=
            "\n ===HIERARCHY===";
            outputTextBox.Text +=
            $"\n\t Stage : {stage.Name} (Loaded Nodes : {stage.Nodes.Count()}) \n";
            // NODE HEIRARCHY
            outputTextBox.Text += "\n\n";
            foreach (var node in stage.Nodes)
            {
                outputTextBox.Text +=
                $" \n\t Node : \n\t  Name  : {node.Name} \n\t\t Position : {node.position.x} , {node.position.y} ";

                if (node.TryGetComponent(out Sprite sprite))
                {
                    outputTextBox.Text +=
                    $"\n\t isSprite : {sprite} \n\t\t";
                }
                if (node.TryGetComponent(out Rigidbody rb))
                {
                    outputTextBox.Text +=
                    $" isRigidbody : {rb} \n\t Velocity : {rb.velocity.x} , {rb.velocity.y}\n ";
                }
            }
            outputTextBox.Text += $" \n {debug} \n";
        }
    }

}