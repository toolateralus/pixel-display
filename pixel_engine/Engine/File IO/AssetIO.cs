using Newtonsoft.Json;
using System;
using System.Drawing;
using System.IO;
using System.Text.Json.Nodes;
using System.Windows;
using pixel_renderer.Assets;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Markup;

namespace pixel_renderer.FileIO
{
    public class AssetIO
    {
        public static string Path => Constants.WorkingRoot + Constants.AssetsDir;
        internal static void FindOrCreateAssetsDirectory()
        {
            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);
        }

        public static void WriteMetadata(KeyValuePair<Metadata, Asset> pair)
        {
            string fullPath = new(pair.Key.fullPath.ToCharArray());
            string extension = new(pair.Key.extension.ToCharArray());
            string name = new(pair.Key.Name);

            string thisPath = fullPath;
            string thisExt = Constants.MetadataFileExtension;

            Metadata meta = new(name, fullPath, extension);
            Metadata this_meta = new(name, fullPath, extension);


            if (thisPath.Contains(meta.extension)
                && meta.extension != thisExt)
            {
                thisPath = thisPath.Replace(meta.extension, "");

                if (thisPath.Contains(thisExt))
                    thisPath = thisPath.Replace(Constants.MetadataFileExtension, "");

                thisPath += thisExt;
                this_meta.fullPath = thisPath;
                this_meta.pathFromProjectRoot = Project.GetPathFromRoot(thisPath);
                this_meta.extension = thisExt;
            }
            IO.WriteJson(meta, this_meta);
        }

        /// <summary>
        /// Checks for the existence of the Assets directory and if it exists, tries to read from the location of the data specified in the metadata object, then registers it to the AssetLibrary..
        /// </summary>
        /// <param name="meta"></param>
        /// <returns>Asset if found, else null </returns>
        public static void WriteAsset(Asset data, Metadata meta)
        {
            FindOrCreateAssetsDirectory();
            IO.WriteJson(data, meta);
            AssetLibrary.Register(meta, data);
            WriteMetadata(new(meta, data));
        }
       
    }
}
