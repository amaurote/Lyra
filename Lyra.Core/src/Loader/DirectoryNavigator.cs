using Lyra.Common;

namespace Lyra.Loader;

public static class DirectoryNavigator
{
    private static string _currentDirectory = string.Empty;
    private static string? _anchorFile;

    private static List<string> _imageList = [];
    private static int _currentIndex = -1;
    private static bool _singleDirectory;

    public static void SearchImages(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Path.Exists(path))
            throw new ArgumentException("[DirectoryNavigator] Invalid path!", nameof(path));

        if ((File.GetAttributes(path) & FileAttributes.Directory) != 0)
        {
            _currentDirectory = path;
            _anchorFile = null;
        }
        else
        {
            _anchorFile = path;
            _currentDirectory = Path.GetDirectoryName(path) ?? throw new ArgumentException("[DirectoryNavigator] Invalid path!", nameof(path));
        }

        var files = FilePathProcessor.ProcessImagePaths([_currentDirectory], out _singleDirectory);
        SetImageList(files, _anchorFile);
        Logger.Info($"[DirectoryNavigator] {_imageList.Count} images in directory.");
    }

    public static void LoadCollection(List<string> paths)
    {
        _anchorFile = null;
        var files = FilePathProcessor.ProcessImagePaths(paths, out _singleDirectory);
        SetImageList(files, _anchorFile);
        Logger.Info($"[DirectoryNavigator] {_imageList.Count} files in collection.");
    }

    private static void SetImageList(List<string> files, string? anchorFile)
    {
        _imageList = files;
        _currentIndex = (anchorFile != null)
            ? _imageList.IndexOf(anchorFile)
            : (_imageList.Count > 0 ? 0 : -1);
    }

    public static string? GetCurrent()
    {
        if (_imageList.Count > 0 && _currentIndex >= 0 && _currentIndex < _imageList.Count)
            return _imageList[_currentIndex];

        return null;
    }

    public static void MoveToNext()
    {
        if (_imageList.Count == 0 || _currentIndex < 0)
            return;

        if (_currentIndex < _imageList.Count - 1)
            _currentIndex++;
    }

    public static void MoveToPrevious()
    {
        if (_imageList.Count == 0)
            return;

        if (_currentIndex > 0)
            _currentIndex--;
    }

    public static void MoveToFirst()
    {
        if (_imageList.Count == 0)
            return;

        _currentIndex = 0;
    }

    public static void MoveToLast()
    {
        if (_imageList.Count == 0)
            return;

        _currentIndex = _imageList.Count - 1;
    }

    public static bool HasNext()
    {
        return _imageList.Count > 0 && _currentIndex < _imageList.Count - 1;
    }

    public static bool HasPrevious()
    {
        return _imageList.Count > 0 && _currentIndex > 0;
    }

    /// <summary>
    /// Returns a slice of image file paths centered on the current image,
    /// including the current image and up to <paramref name="depth"/> images
    /// before and after it in the collection.
    /// </summary>
    /// <param name="depth">
    /// The number of images to include before and after the current index.
    /// For example, a depth of 2 will return up to 5 items: two before, the current, and two after.
    /// </param>
    /// <returns>
    /// An array of image file paths representing the window around the current image.
    /// If the collection is empty or no current index is set, returns an empty array.
    /// </returns>
    public static string[] GetRange(int depth)
    {
        List<string> paths = [];

        if (depth < 0 || _imageList.Count == 0 || _currentIndex < 0)
            return paths.ToArray();

        var start = Math.Max(0, _currentIndex - depth);
        var end = Math.Min(_imageList.Count - 1, _currentIndex + depth);

        for (var i = start; i <= end; i++)
        {
            paths.Add(_imageList[i]);
        }

        return paths.ToArray();
    }

    public static (int index, int count) GetIndex() => (_currentIndex + 1, _imageList.Count);

    public static CollectionType GetCollectionType()
    {
        if (_imageList.Count > 0)
        {
            if (_singleDirectory && _anchorFile != null)
                return CollectionType.SingleDirectoryCollection;
            if (_singleDirectory && _anchorFile == null)
                return CollectionType.SingleDirectorySelection;
            if (!_singleDirectory) 
                return CollectionType.MultiDirectorySelection;
        }

        return CollectionType.Undefined;
    }
}