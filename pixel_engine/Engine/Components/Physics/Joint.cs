namespace pixel_renderer
{
    public class Joint : Component
    {
        [Method]
        void AttachBodiesByName()
        {
            if (Runtime.Current.GetStage() is Stage stage)
            {
                var a = stage.FindNode(Body_A_Name);
                var b = stage.FindNode(Body_B_Name);
                
                if (a is null && !A_Body_Is_Me)
                {
                    Runtime.Log($"Body A of Joint on {node.Name} was not set, and it's not in self select mode. No connection was made.");
                    return;
                }
                else if (A_Body_Is_Me) a = node; 
                if (b is null)
                {
                    Runtime.Log($"Body B of Joint on {node.Name} was not set. No connection was made.");
                    return;
                }

                dominant = a;
                submissive = b; 
            }
        }
        public Node dominant;
        public Node submissive;
        public bool A_Body_Is_Me = true; 
        [Field] string Body_A_Name = "";
        [Field] string Body_B_Name = "Player";
    }
}