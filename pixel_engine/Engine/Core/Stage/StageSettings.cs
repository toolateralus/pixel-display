namespace pixel_renderer.Assets
{
    public class StageSettings
    {
        public string name { get; set; }
        public string UUID { get; set; } 
        public StageSettings(string name, string UUID) 
        {
          this.name = name;
          this.UUID = UUID;
        }
    }
}