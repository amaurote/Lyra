using Lyra.Common;

namespace Lyra.FileLoader;

public static class FilePathProcessor
{
    public static FileLoaderRecursion FileLoaderRecursion { get; set; } = FileLoaderRecursion.AsDesigned;
    
    private static readonly StringComparer PathComparer = OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

    public static List<string> ProcessImagePaths(List<string> paths, bool? recurseSubdirs, out bool? singleDirectory)
    {
        paths = ValidateAndNormalizePaths(paths);
        if (paths.Count == 0)
        {
            singleDirectory = null;
            return [];
        }

        var finalRecurse = FileLoaderRecursion switch
        {
            FileLoaderRecursion.Always => true,
            FileLoaderRecursion.Never => false,
            _ => recurseSubdirs ?? !IsSameDirectoryGroup(paths)
        };

        var allFiles = paths
            .SelectMany(path =>
            {
                if (File.Exists(path))
                    return [path];
                else if (Directory.Exists(path))
                    return GetAllFiles(path, finalRecurse);
                else
                    return [];
            })
            .ToHashSet(PathComparer);

        var supported = allFiles
            .Where(file => ImageFormat.IsSupported(Path.GetExtension(file)))
            .Select(full => new
            {
                Full = full,
                Dir = Path.GetDirectoryName(full) ?? string.Empty,
                Name = Path.GetFileName(full)
            })
            .OrderBy(x => x.Dir, PathComparer)
            .ThenBy(x => x.Name, PathComparer)
            .Select(x => x.Full)
            .ToList();

        var uniqueDirectories = supported
            .Select(Path.GetDirectoryName)
            .Where(d => d != null)
            .Distinct(PathComparer)
            .ToList();

        singleDirectory = uniqueDirectories.Count == 1;

        Logger.Info($"[FilePathProcessor] Collected {supported.Count} supported files from dropped paths.");

        return supported;
    }

    private static IEnumerable<string> GetAllFiles(string directory, bool recurseSubdirs)
    {
        var searchOption = recurseSubdirs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        try
        {
            return Directory.EnumerateFiles(directory, "*", searchOption);
        }
        catch (Exception ex)
        {
            Logger.Warning($"[FilePathProcessor] Failed to enumerate '{directory}': {ex.Message}");
            return [];
        }
    }

    /// <summary>
    /// Validates and normalizes a collection of paths:
    /// <list type="bullet">
    /// <item><description>Converts relative paths to absolute using <see cref="Path.GetFullPath(string)"/>.</description></item>
    /// <item><description>Removes duplicates (case-insensitive on Windows, case-sensitive on Unix-like systems).</description></item>
    /// <item><description>Excludes non-existing files and directories.</description></item>
    /// <item><description>Trims trailing directory separators for directories.</description></item>
    /// </list>
    /// </summary>
    private static List<string> ValidateAndNormalizePaths(IEnumerable<string>? paths)
    {
        if (paths is null)
            return [];

        var distinct = new HashSet<string>(PathComparer);
        var results = new List<string>();

        foreach (var path in paths)
        {
            if (string.IsNullOrWhiteSpace(path))
                continue;

            string fullPath;
            try
            {
                fullPath = Path.GetFullPath(path);
            }
            catch
            {
                continue;
            }

            string canonical;
            if (Directory.Exists(fullPath))
            {
                canonical = Path.TrimEndingDirectorySeparator(fullPath);
            }
            else if (File.Exists(fullPath))
            {
                canonical = fullPath;
            }
            else
            {
                continue;
            }

            if (distinct.Add(canonical))
                results.Add(canonical);
        }

        return results;
    }

    /// <summary>
    /// Determines whether the provided paths belong to the same non-recursive directory group.
    /// </summary>
    /// <remarks>
    /// The group is valid if all items share the same top-level directory (no subdirectories mixed in).
    /// Mixing a directory with one of its subdirectories, or files from different directories, is invalid.
    /// </remarks>
    /// <param name="paths">A collection of validated and normalized file or directory paths.</param>
    /// <returns>
    /// <see langword="true"/> if all items share the same top-level directory and no subdirectories are included; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the collection is null, empty, or contains invalid paths. 
    /// Call <see cref="ValidateAndNormalizePaths"/> first.
    /// </exception>
    private static bool IsSameDirectoryGroup(IEnumerable<string> paths)
    {
        if (paths is null)
            throw new ArgumentException("Path collection cannot be null.");

        var pathList = paths.Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
        if (pathList.Count == 0)
            throw new ArgumentException("Invalid paths in the collection. Validate first!");

        // Case: single directory path
        if (pathList.Count == 1 && Directory.Exists(pathList[0]))
        {
            var dir = pathList[0];
            // Directory with subdirectories is not considered a single-directory group
            var hasSubdirs = Directory.EnumerateDirectories(dir, "*", SearchOption.TopDirectoryOnly).Any();
            return !hasSubdirs;
        }

        string? baseDir = null;

        // First pass — determine candidate base directory
        foreach (var raw in pathList)
        {
            var full = Path.GetFullPath(raw);

            string candidate;
            if (Directory.Exists(full))
            {
                candidate = full;
            }
            else if (File.Exists(full))
            {
                var parent = Path.GetDirectoryName(full) ?? throw new ArgumentException($"Cannot determine parent directory for file: {full}");
                candidate = parent;
            }
            else
            {
                throw new ArgumentException($"Path does not exist (not validated): {full}");
            }

            if (baseDir is null)
            {
                baseDir = candidate;
            }
            else if (!PathComparer.Equals(candidate, baseDir))
            {
                return false;
            }
        }

        // Second pass — enforce non-recursive rule
        foreach (var raw in pathList)
        {
            var full = Path.GetFullPath(raw);

            if (Directory.Exists(full))
            {
                if (!PathComparer.Equals(full, baseDir))
                    return false;
            }
            else
            {
                var parent = Path.GetDirectoryName(full)!;
                if (!PathComparer.Equals(parent, baseDir))
                    return false;
            }
        }

        return true;
    }
}