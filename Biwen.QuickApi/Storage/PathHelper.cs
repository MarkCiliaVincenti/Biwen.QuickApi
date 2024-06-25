﻿namespace Biwen.QuickApi.Storage
{
    internal static class PathHelper
    {
        private const string DATA_DIRECTORY = "|DataDirectory|";

        public static string? ExpandPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            path = path.Replace('/', Path.DirectorySeparatorChar);
            path = path.Replace('\\', Path.DirectorySeparatorChar);

            if (!path.StartsWith(DATA_DIRECTORY, StringComparison.OrdinalIgnoreCase))
                return Path.GetFullPath(path);

            string? dataDirectory = GetDataDirectory();
            int length = DATA_DIRECTORY.Length;
            if (path.Length <= length)
                return dataDirectory;

            string relativePath = path.Substring(length);
            char c = relativePath[0];

            if (c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar)
                relativePath = relativePath.Substring(1);

            string fullPath = Path.Combine(dataDirectory ?? String.Empty, relativePath);
            fullPath = Path.GetFullPath(fullPath);

            return fullPath;
        }

        public static string? GetDataDirectory()
        {
            try
            {
                string? dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
                if (string.IsNullOrEmpty(dataDirectory))
                    dataDirectory = AppContext.BaseDirectory;

                if (!string.IsNullOrEmpty(dataDirectory))
                    return Path.GetFullPath(dataDirectory);
            }
            catch (Exception)
            {
                return null;
            }

            return null;
        }

        /// <summary>
        /// NormalizePath
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string NormalizePath(this string path)
        {
            if (String.IsNullOrEmpty(path))
                return path;

            if (Path.DirectorySeparatorChar == '\\')
                path = path.Replace('/', Path.DirectorySeparatorChar);
            else if (Path.DirectorySeparatorChar == '/')
                path = path.Replace('\\', Path.DirectorySeparatorChar);

            return path;
        }


    }
}
