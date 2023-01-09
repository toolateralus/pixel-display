﻿using Newtonsoft.Json;

namespace pixel_renderer.IO
{
    public class Metadata
    {
        public string Name = "Object Metadata";
        public string fullPath = "C:\\\\Users\\Josh\\Appdata\\Roaming\\Pixel\\Assets\\Metadata\\Error";
        public string extension = ""; 
        public string pathFromProjectRoot = "";
        private string _uuid = "";
        public string UUID { get { if (_uuid is null || _uuid == "") 
                    _uuid = pixel_renderer.UUID.NewUUID(); return _uuid; } }

        public Metadata(string name, string fullPath, string extension)
        {
            Name = name;
            this.fullPath = fullPath;
            this.extension = extension;
            pathFromProjectRoot = Project.GetPathFromRoot(fullPath);
            _uuid = pixel_renderer.UUID.NewUUID();
        }

        [JsonConstructor]
        private Metadata(string name, string fullPath, string pathFromProjectRoot, string uuid, string extension)
        {
            Name = name;
            this.fullPath = fullPath;
            this.extension = extension;
            this.pathFromProjectRoot = pathFromProjectRoot; 
            _uuid = uuid; 
        }
    }
}