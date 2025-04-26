using Lyra.Imaging.Data;
using MetadataExtractor.Formats.Exif;
using Directory = MetadataExtractor.Directory;

namespace Lyra.Imaging.Pipeline;

internal static class MetadataProcessor
{
    // private static readonly Dictionary<string, string> ExifMetadataMap = new()
    // {
    //     // Camera & Lens Information
    //     { "Make", "Make" },
    //     { "Model", "Model" },
    //     { "Lens", "Lens" },
    //     // { "Focal Length", "FocalLength" },
    //     // { "Focal Length (35mm Eq.)", "FocalLengthIn35mmFormat" },
    //
    //     // Exposure & Settings
    //     { "Exposure Time", "ExposureTime" },
    //     { "F-Number", "FNumber" },
    //     { "ISO Speed Ratings", "ISO" },
    //     // { "Exposure Compensation", "ExposureBiasValue" },
    //     // { "Flash", "Flash" },
    //
    //     // Date & Time
    //     { "Date/Time", "Taken" },
    //
    //     // GPS Position (Check only if available)
    //     { "GPS Latitude", "GPSLatitude" },
    //     { "GPS Longitude", "GPSLongitude" },
    //     // { "GPS Altitude", "GPSAltitude" },
    //
    //     // Image Details
    //     // { "Orientation", "Orientation" },
    //     // { "Color Space", "ColorProfile" },
    //     // { "Data Precision", "BitDepth" },
    //
    //     // Software / Processing
    //     // { "Software", "Software" },
    // };
    
    public static ExifInfo ProcessMetadata(IReadOnlyList<Directory> directories)
    {
        var exifInfo = new ExifInfo();

        var ifd0 = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
        var subIfd = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
        var gps = directories.OfType<GpsDirectory>().FirstOrDefault();
        
        exifInfo.Make = ifd0?.GetDescription(ExifDirectoryBase.TagMake) ?? string.Empty;
        exifInfo.Model = ifd0?.GetDescription(ExifDirectoryBase.TagModel) ?? string.Empty;
        exifInfo.Lens = subIfd?.GetDescription(ExifDirectoryBase.TagLensModel) ?? string.Empty;

        exifInfo.ExposureTime = subIfd?.GetDescription(ExifDirectoryBase.TagExposureTime) ?? string.Empty;
        exifInfo.FNumber = subIfd?.GetDescription(ExifDirectoryBase.TagFNumber) ?? string.Empty;
        exifInfo.Iso = subIfd?.GetDescription(ExifDirectoryBase.TagIsoEquivalent) ?? string.Empty;

        exifInfo.Taken = subIfd?.GetDescription(ExifDirectoryBase.TagDateTimeOriginal) ?? string.Empty;

        exifInfo.GpsLatitude = gps?.GetDescription(GpsDirectory.TagLatitude) ?? string.Empty;
        exifInfo.GpsLongitude = gps?.GetDescription(GpsDirectory.TagLongitude) ?? string.Empty;
        
        exifInfo.Compression = subIfd?.GetDescription(ExifDirectoryBase.TagCompression) ?? string.Empty;
        exifInfo.Software = ifd0?.GetDescription(ExifDirectoryBase.TagSoftware) ?? string.Empty;
        
        return exifInfo;
    }
}