using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GridPlayer
{
    public static class MediaFileHelper
    {
        private static readonly string[] SupportedExtensions = {
            ".mp4", ".m4v", ".mov", ".avi", ".wmv", ".mkv", ".flv", ".mpg", ".mpeg", ".ts",
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp"
        };

        public static IEnumerable<string> GetAllMediaFiles(string[] paths)
        {
            var result = new List<string>();
            if (paths == null) return result;

            foreach (var path in paths)
            {
                if (Directory.Exists(path))
                {
                    try
                    {
                        var files = Directory.EnumerateFiles(path, "*.*", new EnumerationOptions
                        {
                            RecurseSubdirectories = true,
                            IgnoreInaccessible = true
                        })
                        .Where(f => SupportedExtensions.Contains(Path.GetExtension(f).ToLower()));
                        result.AddRange(files);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error accessing directory {path}: {ex.Message}");
                    }
                }
                else if (File.Exists(path))
                {
                    if (SupportedExtensions.Contains(Path.GetExtension(path).ToLower()))
                    {
                        result.Add(path);
                    }
                }
            }
            return result;
        }
    }
}
