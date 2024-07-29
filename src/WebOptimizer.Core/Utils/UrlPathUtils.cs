using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace WebOptimizer.Utils
{
    public static class UrlPathUtils
    {
        public static bool IsAbsolutePath(string path)
        {
            return path.StartsWith("/");
        }

        public static string Normalize(string path)
        {
            if (!TryNormalize(path, out string normalized))
                throw new ArgumentException("Malformed path", path);

            return normalized;
        }

        public static bool TryNormalize(string path, out string normalizedPath)
        {
            List<string> normalized = new List<string>();

            foreach (StringSegment segment in new StringTokenizer(path, ['/']))
            {
                if (segment.HasValue)
                {
                    switch (segment.Value)
                    {
                        case ".":
                            break;

                        case "..":
                            if (normalized.Count == 1 && normalized[0] == string.Empty)
                            {
                                normalizedPath = default;
                                return false;
                            }
                            else if (normalized.Count == 0)
                            {
                                normalized.Add("..");
                            }
                            else if (normalized[normalized.Count - 1] == "..")
                            {
                                normalized.Add("..");
                            }
                            else
                            {
                                normalized.RemoveAt(normalized.Count - 1);
                            }
                            break;

                        default:
                            normalized.Add(segment.Value);
                            break;
                    }
                }
            }

            normalizedPath = string.Join('/', normalized);
            return true;
        }

        public static string MakeAbsolute(string basePath, string path)
        {
            if (!IsAbsolutePath(basePath) && !IsAbsolutePath(path))
                throw new ArgumentException("Neither basePath nor path are absolute");

            if (IsAbsolutePath(path))
                return path;

            return Normalize(basePath + (basePath.EndsWith("/") ? string.Empty : "/") + path);
        }

        public static bool TryMakeAbsolute(string basePath, string path, out string absolutePath)
        {
            if (!IsAbsolutePath(basePath) && !IsAbsolutePath(path))
            {
                absolutePath = default;
                return false;
            }

            if (IsAbsolutePath(path))
            {
                absolutePath = path;
                return true;
            }

            return TryNormalize(basePath + (basePath.EndsWith("/") ? string.Empty : "/") + path, out absolutePath);
        }

        public static string MakeAbsolutePathFromInclude(string appPath, string contentPath, string includePath)
        {
            string contentAbsolutePath = MakeAbsolute(appPath, contentPath);
            string contentDirectoryPath = GetDirectory(contentAbsolutePath);

            return MakeAbsolute(contentDirectoryPath, includePath);
        }

        public static bool TryMakeAbsolutePathFromInclude(string appPath, string contentPath, string includePath, out string absolutePath)
        {
            if (!TryMakeAbsolute(appPath, contentPath, out string contentAbsolutePath))
            {
                absolutePath = default;
                return false;
            }

            string contentDirectoryPath = GetDirectory(contentAbsolutePath);

            return TryMakeAbsolute(contentDirectoryPath, includePath, out absolutePath);
        }

        public static string GetDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            if (path.EndsWith("/"))
                return path;

            StringTokenizer segments = new StringTokenizer(path, new[] { '/' });

            string directory = string.Join('/', segments.SkipLast(1)) + "/";

            if (directory == "/" && !path.StartsWith("/"))
                return "";

            if ((path.StartsWith("/") && directory.StartsWith("/")) || !path.StartsWith("/"))
                return directory;

            return "/" + directory;
        }

        public static string GetFileName(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path is empty");

            if (path.EndsWith("/"))
                throw new ArgumentException("Path is a directory");

            StringTokenizer segments = new StringTokenizer(path, new[] { '/' });

            return segments.Last().Value;
        }
    }
}
