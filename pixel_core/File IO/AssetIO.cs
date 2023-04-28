using Pixel.Statics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pixel.FileIO
{
    public class AssetIO
    {

        public static bool FindOrCreatePath(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                return false;
            }
            return true;
        }
        internal static void GuaranteeUniqueName(Metadata meta, Asset asset)
        {
            IO.GetDir(meta, out string name, out string dir);

            _ = FindOrCreatePath(dir);

            name = DuplicateCheck(name, dir, asset);

            asset.Upload();
        }
       
        private static string DuplicateCheck(string fullName, string dir, Asset asset)
        {
            var fileNameSplit = fullName.Split(".").ToList();
            var extension = fileNameSplit.Last();
            fileNameSplit.RemoveAt(fileNameSplit.Count - 1);
            var name = string.Join('.', fileNameSplit);

            string fullPath = $"{dir}{fullName}";

            Metadata meta = new(fullPath);

            Asset foundAsset = IO.ReadJson<Asset>(meta);

            if (File.Exists(fullPath) && foundAsset is not null && foundAsset.UUID == asset.UUID)
                return fullName;

            string nameWithoutNums = "";

            for (List<char> chars = name.ToList(); chars.Count > 0; chars.RemoveAt(chars.Count - 1))
            {
                if (!Constants.int_chars.Contains(chars.Last()))
                {
                    nameWithoutNums = string.Concat(chars);
                    break;
                }
            }

            List<int> duplicateNames = new();
            foreach (var path in Directory.EnumerateFiles(dir))
            {
                var splitPath = path.Split("\\").Last().Split('.').ToList();
                if (splitPath.Last() != extension)
                    continue;
                splitPath.RemoveAt(splitPath.Count - 1);
                var fileName = string.Join('.', splitPath);

                List<char> numbers = new();
                for (List<char> chars = fileName.ToList(); chars.Count > 0; chars.RemoveAt(chars.Count - 1))
                {
                    if (!Constants.int_chars.Contains(chars.Last()))
                    {
                        if (string.Concat(chars) != nameWithoutNums)
                            break;
                        duplicateNames.Add(string.Concat(numbers).ToInt());
                        if (duplicateNames.Count >= 1000)
                            throw new FileNamingException($"There are too many files already named \"{nameWithoutNums}.{extension}\"");
                        break;
                    }
                    numbers.Insert(0, chars.Last());
                }
            }
            for (int i = 1; i < 1000; i++)
            {
                if (duplicateNames.Contains(i))
                    continue;
                Interop.Log($"Number {i} added to file \"{nameWithoutNums}.{extension}\"");
                return $"{nameWithoutNums}{i}.{extension}";
            }
            throw new Exception("Unknown Exception");
        }
    }
}
