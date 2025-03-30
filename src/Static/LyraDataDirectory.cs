namespace Lyra;

public static class LyraDataDirectory
{
    public static string GetDataDirectory()
    {
        var dataDirectory = OperatingSystem.IsWindows()
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LyraViewer")
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share", "LyraViewer");

        Directory.CreateDirectory(dataDirectory);
        return dataDirectory;
    }
}