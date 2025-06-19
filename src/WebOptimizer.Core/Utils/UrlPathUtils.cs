using Microsoft.Extensions.Primitives;

namespace WebOptimizer.Utils;

/// <summary>
/// Utility class for manipulating URL paths.
/// </summary>
public static class UrlPathUtils
{
    /// <summary>
    /// Gets the directory.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>System.String.</returns>
    public static string GetDirectory(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return path;
        }

        if (path.EndsWith('/'))
        {
            return path;
        }

        var segments = new StringTokenizer(path, ['/']);

        string directory = string.Join('/', segments.SkipLast(1)) + "/";

        return directory == "/" && !path.StartsWith('/')
            ? ""
            : (path.StartsWith('/') && directory.StartsWith('/')) || !path.StartsWith('/') ? directory : $"/{directory}";
    }

    /// <summary>
    /// Gets the name of the file.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>System.String.</returns>
    /// <exception cref="ArgumentException">Path is empty</exception>
    /// <exception cref="ArgumentException">Path is a directory</exception>
    public static string? GetFileName(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path is empty");
        }

        if (path.EndsWith('/'))
        {
            throw new ArgumentException("Path is a directory");
        }

        var segments = new StringTokenizer(path, ['/']);

        return segments.Last().Value;
    }

    /// <summary>
    /// Determines whether [is absolute path] [the specified path].
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns><c>true</c> if [is absolute path] [the specified path]; otherwise, <c>false</c>.</returns>
    public static bool IsAbsolutePath(string path)
    {
        return path.StartsWith('/');
    }

    /// <summary>
    /// Makes the absolute.
    /// </summary>
    /// <param name="basePath">The base path.</param>
    /// <param name="path">The path.</param>
    /// <returns>System.String.</returns>
    /// <exception cref="ArgumentException">Neither basePath nor path are absolute</exception>
    public static string? MakeAbsolute(string basePath, string path)
    {
        return IsAbsolutePath(basePath) || IsAbsolutePath(path)
            ? IsAbsolutePath(path) ? path : Normalize($"{basePath}{(basePath.EndsWith('/') ? string.Empty : "/")}{path}")
            : throw new ArgumentException("Neither basePath nor path are absolute");
    }

    /// <summary>
    /// Makes the absolute path from include.
    /// </summary>
    /// <param name="appPath">The application path.</param>
    /// <param name="contentPath">The content path.</param>
    /// <param name="includePath">The include path.</param>
    /// <returns>System.String.</returns>
    public static string? MakeAbsolutePathFromInclude(string appPath, string contentPath, string includePath)
    {
        string contentAbsolutePath = MakeAbsolute(appPath, contentPath)!;
        string contentDirectoryPath = GetDirectory(contentAbsolutePath);

        return MakeAbsolute(contentDirectoryPath, includePath);
    }

    /// <summary>
    /// Normalizes the specified path.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>System.String.</returns>
    /// <exception cref="ArgumentException">Malformed path</exception>
    public static string? Normalize(string path)
    {
        return TryNormalize(path, out string? normalized) ? normalized : throw new ArgumentException("Malformed path", path);
    }

    /// <summary>
    /// Tries the make absolute.
    /// </summary>
    /// <param name="basePath">The base path.</param>
    /// <param name="path">The path.</param>
    /// <param name="absolutePath">The absolute path.</param>
    /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
    public static bool TryMakeAbsolute(string basePath, string path, out string? absolutePath)
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

        return TryNormalize($"{basePath}{(basePath.EndsWith('/') ? string.Empty : "/")}{path}", out absolutePath);
    }

    /// <summary>
    /// Tries the make absolute path from include.
    /// </summary>
    /// <param name="appPath">The application path.</param>
    /// <param name="contentPath">The content path.</param>
    /// <param name="includePath">The include path.</param>
    /// <param name="absolutePath">The absolute path.</param>
    /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
    public static bool TryMakeAbsolutePathFromInclude(string appPath, string contentPath, string includePath, out string? absolutePath)
    {
        if (!TryMakeAbsolute(appPath, contentPath, out string? contentAbsolutePath))
        {
            absolutePath = default;
            return false;
        }

        string contentDirectoryPath = GetDirectory(contentAbsolutePath!);

        return TryMakeAbsolute(contentDirectoryPath, includePath, out absolutePath);
    }

    /// <summary>
    /// Tries the normalize.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="normalizedPath">The normalized path.</param>
    /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
    public static bool TryNormalize(string path, out string? normalizedPath)
    {
        List<string> normalized = [];

        foreach (var segment in new StringTokenizer(path, ['/']))
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
                        else if (normalized[^1] == "..")
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
}
