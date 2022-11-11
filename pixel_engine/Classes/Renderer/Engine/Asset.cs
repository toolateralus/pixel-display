﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Newtonsoft.Json;
using Color = System.Drawing.Color;

namespace pixel_renderer
{
    /// <summary>
    /// base class for all pixel_engine asset references; 
    /// </summary>
    public class Asset
    {
        public string pathFromRoot = "";
        public string fileSize = "";
        public string fileType = "";
        public Type runtimeType; 
        public string UUID = "";
        public string Name = "NewAsset";
        public Asset() { }
        public Asset(string name)
        {
            Name = name; 
        }
        public Asset(string pathFromRoot, string fileType, Type runtimeType)
        {
            this.pathFromRoot = pathFromRoot;
            this.fileType = fileType;
            this.runtimeType = runtimeType; 
            UUID = pixel_renderer.UUID.NewUUID();
            AssetLibrary.Register(GetType(), this);
        }
    }

    public class BitmapAsset : Asset
    {
        public Bitmap bitmap; 
        public Color[,] colors 
        {
            get  => ColorArrayFromBitmap(bitmap);
        }
        public static Bitmap BitmapFromColorArray(Color[,] colors)
        {
            Bitmap bitmap = new(64, 64);  
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    bitmap.SetPixel(i, j, colors[i, j]);
                }
            }
            return bitmap;
        }
        public static Color[,] ColorArrayFromBitmap(Bitmap bitmap)
        {
            Color[,] s = new Color[bitmap.Width, bitmap.Height];
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    s[i, j] = bitmap.GetPixel(i, j);
                }
            }
            return s;
        }
    }

    public class FontAsset : Asset 
    {
        new public string Name = "NewFontAsset";
        public Dictionary<char, Bitmap> characters = new();

       
    }

    public static class FontAssetFactory
    {
        public static FontAsset CreateFont(int start, int end, Bitmap[] characters)
        {
            FontAsset fontAsset = new();
            char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            for (int i = start; i < end; i++)
            {
                if (i > alpha.Length || i > characters.Length)
                {
                    MessageBox.Show("Font asset could not be created, Index out of range.");
                    return null; 
                }
                fontAsset.characters.Add(alpha[i], characters[i]);
            }
            if (fontAsset.characters.Count <= 0)
            {
                MessageBox.Show("Font is empty.");
            }
            return fontAsset; 
        }

        /// <summary>
        /// Really expensive text rendering; 
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        internal static Dictionary<Bitmap, Vec2> ToString(FontAsset asset, string text)
        {
            Dictionary<Bitmap, Vec2> output = new();
            int i = 0;
            foreach (char character in text)
            {
                var x = character;
                if (char.IsLower(character))
                {
                     x = char.ToUpper(character); 
                }
                var img = (Bitmap)asset.characters[x].Clone(); 
                output.Add(img, new Vec2(i + img.Width, 0));
                i++;
            }
            return output; 
        }

        internal static void InitializeDefaultFont()
        {
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var path = appdata + "\\Pixel\\Images\\Font";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            IEnumerable<string> files = Directory.GetFiles(path);
            int i = 0; 
            foreach (string file in files)
            {
                BitmapAsset bitmap = new()
                {
                    bitmap = new(file),
                    Name = $"{'a' + i}"
                };
                i++;
                AssetLibrary.Register(typeof(BitmapAsset), bitmap);
            }
        }
    }

    public static class AssetIO
    {
        public static bool skippingOperation = false; 
        public static string Path => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Pixel\\Assets";
        public static void SaveAsset(Asset data, string fileName)
        {
            if (!Directory.Exists(Path))
            {
                Directory.CreateDirectory(Path);
            }
            if (File.Exists(Path + "/" + fileName + ".json"))
            {
                if (!skippingOperation)
                {
                    var overwriteWarningResult = MessageBox.Show($"Are you sure you want to overwrite {fileName}.json ? \n found at {Path}",
                                            "", MessageBoxButton.YesNoCancel, MessageBoxImage.Asterisk,
                                             MessageBoxResult.No, MessageBoxOptions.RtlReading);
                    var doForAllResult = MessageBox.Show($"Do for all (uses last choice)",
                                            "", MessageBoxButton.YesNo, MessageBoxImage.Warning,
                                             MessageBoxResult.No, MessageBoxOptions.RtlReading);
                    if (doForAllResult == MessageBoxResult.Yes)
                    {
                        skippingOperation = true; 
                    }
                    if (overwriteWarningResult != MessageBoxResult.Yes)
                    {
                        return; 
                    }
                }
            }
            TextWriter writer = new StreamWriter(Path + "/" + fileName + ".json");
            var json = JsonSerializer.Create();
            json.Serialize(writer, data);
            writer.Close();
        }
        public static Asset? ReadAssetFile(string fileName)
        {
            if (!Directory.Exists(Path))
            {
                Directory.CreateDirectory(Path);
                return null; 
            }
            JsonSerializer serializer = new();
            StreamReader reader = new(fileName);
            Asset asset = new();
            using JsonTextReader json = new(reader);
            try
            {
                asset = serializer.Deserialize<Asset>(json) ?? null;
            }
            catch (Exception e)
            {
                throw e;
            }
            return asset;
        }
    }
    public static class AssetPipeline
    {
        public static string Path => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Pixel\\Assets";
        public static void ImportAsync()
        {
            if (Runtime.Instance.running)
            {
                var msg = MessageBox.Show("Pixel needs to Import: Press OK to start.", "Asset Importer", MessageBoxButton.OK);
                if (msg == MessageBoxResult.Cancel)
                {
                    return;
                }
            }
            if (!Directory.Exists(Path))
            {
                Directory.CreateDirectory(Path); 
            }
            
            foreach(var thisPath in Directory.GetFiles(Path))
            {
                var asset = Import(thisPath);
                if (asset is not null)
                    AssetLibrary.Register(asset.GetType(), asset);
            };
            _ = MessageBox.Show("Import complete!", "Asset Importer", MessageBoxButton.OK);
            

        }
        public static Asset? Import(string path)
        {
            if (!File.Exists(path)) return null; 
            return AssetIO.ReadAssetFile(path);
        }
    }
    public static class AssetLibrary
    {
        public static Dictionary<Type, List<Asset>> LoadedAssets = new();
        /// <summary>
        /// Try to retrieve Asset by UUID and Type@ ..\AppData\Assets\$path$
        /// </summary>
        /// <param name="type"></param>
        /// <param name="path"></param>
        /// <exception cref="NotImplementedException"></exception>
        public static bool TryFetch<T>(string name, out T result) where T : Asset
        {
            result = null; 
            if (LoadedAssets.TryGetValue(typeof(T), out List<Asset> found))
            {
                foreach (var _asset in found)
                {
                    if (_asset.Name.Equals(name))
                    {
                        result = _asset as T; 
                    }
                }
                return true;
            }
            return false;
        }
        /// <summary>
        /// Save the currently loaded asset database.
        /// </summary>
        public static void Sync()
        {
            var library = GetLibrary();
            if (library is null) return; 
            AssetIO.skippingOperation = false; 
            foreach (var asset in library) AssetIO.SaveAsset(asset, asset.Name);
        }
        internal static  List<Asset>? GetLibrary()
        {
            List<Asset> library = new();  
            foreach (var key in LoadedAssets)
                foreach (var item in key.Value)
                    library.Add(item);

            return library; 
        }
        internal static void Register(Type type, Asset asset)
        {
            if (!LoadedAssets.ContainsKey(type))
            {
                LoadedAssets.Add(type, new List<Asset>());
            }
            LoadedAssets[type].Add(asset);
        }
        internal static void Unregister(Type type, string Name)
        {
            foreach (var asset in LoadedAssets[type])
            {
                if (asset.Name.Equals(Name))
                {
                    LoadedAssets[type].Remove(asset);
                }
            }
        }
    }
}