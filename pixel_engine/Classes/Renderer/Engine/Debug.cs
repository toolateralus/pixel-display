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
            outputTextBox.Text =$" {Rendering.FrameRate()} Frames Per Second"; 
            
        }
    }

}