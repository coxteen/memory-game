using System;
using System.IO;

namespace MemoryGame.Services
{
    public static class AssetPathResolver
    {
        public static string Resolve(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return path;
            }

            if (File.Exists(path))
            {
                return Path.GetFullPath(path);
            }

            string normalizedPath = path.Replace('\\', '/');
            int resourceIndex = normalizedPath.IndexOf("/res/", StringComparison.OrdinalIgnoreCase);
            if (resourceIndex >= 0)
            {
                normalizedPath = normalizedPath[(resourceIndex + 1)..];
            }
            else if (!normalizedPath.StartsWith("res/", StringComparison.OrdinalIgnoreCase))
            {
                if (Path.IsPathRooted(path))
                {
                    return path;
                }
            }

            string relativePath = normalizedPath.Replace('/', Path.DirectorySeparatorChar);
            return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, relativePath));
        }
    }
}