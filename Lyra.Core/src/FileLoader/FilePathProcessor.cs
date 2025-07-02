using Lyra.Common;

namespace Lyra.FileLoader;

public static class FilePathProcessor
{
    private static readonly StringComparer PathComparer = OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

    public static List<string> ProcessImagePaths(List<string> paths, out bool singleDirectory)
    {
        var allFiles = paths
            .Select(Path.GetFullPath)
            .SelectMany(path =>
            {
                if (File.Exists(path))
                    return [path];
                else if (Directory.Exists(path))
                    return GetAllFiles(path);
                else
                    return [];
            })
            .ToHashSet(PathComparer);

        var supported = allFiles
            .Where(file => ImageFormat.IsSupported(Path.GetExtension(file)))
            .OrderBy(f => f, PathComparer)
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

    private static IEnumerable<string> GetAllFiles(string directory)
    {
        try
        {
            return Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories);
        }
        catch (Exception ex)
        {
            Logger.Warning($"[FilePathProcessor] Failed to enumerate '{directory}': {ex.Message}");
            return [];
        }
    }
}