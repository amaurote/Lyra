using System.Collections.Concurrent;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Lyra.Loader.Utils;

public static class LoadTimeEstimator
{
    private static readonly string TimeFilePath = Path.Combine(LyraDataDirectory.GetDataDirectory(), "load_time_data.yaml");
    private static readonly ConcurrentDictionary<(string Extension, int SizeBucket), List<double>> LoadTimeData = new();

    private const int UnsavedChangesThreshold = 5;
    private const int MaxSamplesPerBucket = 20;

    private static int _unsavedChangesCount;

    static LoadTimeEstimator()
    {
        LoadTimeDataFromFile();
    }

    public static void RecordLoadTime(string extension, long sizeInBytes, double loadTime)
    {
        if (!TryGetKey(extension, sizeInBytes, out var key))
            return;

        var list = LoadTimeData.GetOrAdd(key, _ => []);
        lock (list)
        {
            list.Add(loadTime);
            Logger.LogDebug($"[LoadTimeEstimator] {key.Extension}: {sizeInBytes} bytes, {loadTime} ms.");

            if (list.Count > MaxSamplesPerBucket)
                list.RemoveAt(0);
        }

        // Save periodically
        if (++_unsavedChangesCount >= UnsavedChangesThreshold)
        {
            SaveTimeDataToFile(true);
            _unsavedChangesCount = 0;
        }
    }

    public static double EstimateLoadTime(string extension, long sizeInBytes)
    {
        if (!TryGetKey(extension, sizeInBytes, out var key))
            return 0;

        if (LoadTimeData.TryGetValue(key, out var loadTimes))
        {
            // Direct match found, return the average
            lock (loadTimes)
                return loadTimes.Average();
        }

        // No exact match: Find closest available bucket
        var availableBuckets = LoadTimeData.Keys
            .Where(k => k.Extension.Equals(key.Extension))
            .Select(k => k.SizeBucket)
            .OrderBy(b => Math.Abs(b - key.SizeBucket))
            .ToList();

        if (availableBuckets.Count == 0)
            return 0; // Default estimate if no data available

        // Interpolate between the closest known buckets
        var closestBucket = availableBuckets.First();
        var fallbackKey = (key.Extension, closestBucket);

        if (LoadTimeData.TryGetValue(fallbackKey, out var fallbackTimes))
            lock (fallbackTimes)
                return fallbackTimes.Average();

        return 0;
    }

    public static void SaveTimeDataToFile(bool suppressLogging = false)
    {
        try
        {
            var snapshot = LoadTimeData.ToDictionary(
                entry => $"{entry.Key.Extension}_{entry.Key.SizeBucket}",
                entry => entry.Value.ToList()
            );

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var yaml = serializer.Serialize(snapshot);
            File.WriteAllText(TimeFilePath, yaml);

            if (!suppressLogging)
                Logger.Log("[LoadTimeEstimator] Successfully saved time data.");
        }
        catch (Exception ex)
        {
            Logger.Log($"[LoadTimeEstimator] Failed to save time data: {ex.Message}", Logger.LogLevel.Error);
        }
    }

    private static void LoadTimeDataFromFile()
    {
        if (!File.Exists(TimeFilePath))
        {
            Logger.Log("[LoadTimeEstimator] No existing time data found.");
            return;
        }

        try
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var yamlText = File.ReadAllText(TimeFilePath);
            var data = deserializer.Deserialize<Dictionary<string, List<double>>>(yamlText);

            LoadTimeData.Clear();
            foreach (var entry in data)
            {
                if (TryParseKey(entry.Key, out var ext, out var bucket))
                    LoadTimeData[(ext, bucket)] = entry.Value;
                else
                    Logger.Log($"[LoadTimeEstimator] Skipping invalid YAML entry: {entry.Key}", Logger.LogLevel.Warn);
            }

            Logger.Log("[LoadTimeEstimator] Successfully loaded time data.");
        }
        catch (Exception ex)
        {
            Logger.Log($"[LoadTimeEstimator] Failed to load time data: {ex.Message}", Logger.LogLevel.Error);
        }
    }

    private static int GetSizeBucket(long sizeInBytes)
    {
        // Bucket sizes: 256KB, 512KB, 1MB, 2MB, 4MB, etc.
        var bucket = (int)Math.Pow(2, Math.Ceiling(Math.Log(sizeInBytes / 256000.0, 2)));
        return Math.Max(bucket, 1); // Ensure minimum bucket of 1
    }

    private static bool TryGetKey(string extension, long sizeInBytes, out (string Extension, int SizeBucket) key)
    {
        key = default;
        var ext = ExtensionToFormat(extension);
        if (ext == null)
            return false;

        key = (ext, GetSizeBucket(sizeInBytes));
        return true;
    }

    private static bool TryParseKey(string key, out string ext, out int bucket)
    {
        var parts = key.Split('_', 2);
        if (parts.Length == 2 && int.TryParse(parts[1], out bucket))
        {
            ext = parts[0];
            return true;
        }

        ext = null!;
        bucket = 0;
        return false;
    }

    private static string? ExtensionToFormat(string extension)
    {
        if (string.IsNullOrEmpty(extension))
            return null;

        return extension.ToLower() switch
        {
            ".jpg" => "JPEG",
            ".jpeg" => "JPEG",
            ".heic" => "HEIF",
            ".heif" => "HEIF",
            ".tif" => "TIFF",
            ".tiff" => "TIFF",
            _ => extension.Replace(".", string.Empty).ToUpper()
        };
    }
}