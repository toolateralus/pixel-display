using pixel_renderer.FileIO;
using System.IO;

namespace pixel_renderer
{
    public class StageIO
    {
        private static void FindOrCreateStagesDirectory()
        {
            if (!Directory.Exists(Constants.WorkingRoot + Constants.StagesDir))
                Directory.CreateDirectory(Constants.WorkingRoot + Constants.StagesDir);
        }
        public static void WriteStage(Stage stage)
        {
            stage.Sync();
            FindOrCreateStagesDirectory();
            IO.WriteJson(stage, stage.Metadata);
        }

        public static Stage? ReadStage(Metadata meta)
        {
            FindOrCreateStagesDirectory();
            if (meta is null)
                return null;
            Stage? stage = IO.ReadJson<Stage>(meta);
            return stage;
        }
    }
}