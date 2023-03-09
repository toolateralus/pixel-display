using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace pixel_renderer
{

    public class Constants
    {

        public const int FramerateSampleThreshold = 60;
        public static Vector2 MaxResolution = new(3840, 3840);
        public static Vector2 MinResolution = new(4, 4);

        public static char[] unsupported_chars = { '_', '-', '.', '`' };
        public static char[] int_chars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        public const string ImagesDir = "\\Images";  // Images Import folder (temporary solution until assets are done, for importing backgrounds)
        public const string ProjectsDir = "\\Projects"; // Project files 
        public const string AssetsDir = "\\Assets";   // Asset files (user - created)
        public const string StagesDir = "\\Stages"; //Stage files
        // metadata is saved next to all files.

        public const string AssetsFileExtension = ".asset";  // .asset {Pixel Asset Data}
        public const string ProjectFileExtension = ".pixel";   // .pixel {Pixel Project}
        public const string MetadataFileExtension = ".meta";  // .meta {File Metadata} 
        public const string StageFileExtension = ".stage";    // .stage {Stage File}
        
        public static string WorkingRoot = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Pixel";    // Root directory for resources

        public const string BitmapFileExtension = ".bmp"; // .bmp {The only image format currently supported.}
        public static string[] ReadableExtensions = 
        { 
            BitmapFileExtension,
            AssetsFileExtension,
            ProjectFileExtension,
            StageFileExtension,
        };

        public static List<Type> GetInheritedTypesFromBase<T>()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
               .SelectMany(domainAssembly => domainAssembly.GetTypes())
               .Where(type => typeof(T).IsAssignableFrom(type)).ToList();
            return types;
        }
        public static T?[] NullifyDuplicatesInArray<T>(in T?[] collection)
        {
            HashSet<T> comparison = new();
            foreach (var obj in collection)
            {
                if (comparison.Contains(obj))
                {
                    collection[Array.IndexOf(collection, obj)] = default; 
                    continue;
                }
                comparison.Add(obj);
            }
            return collection; 
        }
        public static List<T> RemoveDuplicatesFromList<T>(List<T> collection)
        {
            HashSet<T> comparison = new();
            foreach (var obj in collection)
            {
                if (comparison.Contains(obj))
                {
                    collection.Remove(obj);
                    continue;
                }
                comparison.Add(obj);
            }
            return collection; 
        }
    }

}


