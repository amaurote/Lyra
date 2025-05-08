using System.Reflection;

namespace Lyra.Common;

public static class ImageFormat
{
    static ImageFormat()
    {
        foreach (var format in Enum.GetValues<ImageFormatType>())
        {
            var memberInfo = typeof(ImageFormatType).GetMember(format.ToString()).FirstOrDefault();
            if (memberInfo != null && memberInfo.GetCustomAttribute<DisabledTypeAttribute>() != null) 
                Logger.Warning($"[ImageFormat] {format} is disabled.");
        }

        foreach (var format in Enum.GetValues<ImageFormatType>())
        {
            var memberInfo = typeof(ImageFormatType).GetMember(format.ToString()).FirstOrDefault();
            if (memberInfo != null && memberInfo.GetCustomAttribute<DisabledPreloadAttribute>() != null) 
                Logger.Warning($"[ImageFormat] {format} preload is disabled.");
        }
    }

    public static bool IsSupported(string? extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
            return false;

        var format = GetImageFormat(extension);
        if (format == ImageFormatType.Unknown)
            return false;

        var memberInfo = typeof(ImageFormatType).GetMember(format.ToString()).FirstOrDefault();
        return memberInfo == null || memberInfo.GetCustomAttribute<DisabledTypeAttribute>() == null;
    }

    public static bool IsPreloadDisabled(string extension)
    {
        var format = GetImageFormat(extension);
        var memberInfo = typeof(ImageFormatType).GetMember(format.ToString()).FirstOrDefault();
        if (memberInfo != null)
            return memberInfo.GetCustomAttribute<DisabledPreloadAttribute>() != null;

        return false;
    }

    public static ImageFormatType GetImageFormat(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
            throw new ArgumentException("Extension cannot be null or empty.", nameof(extension));

        extension = extension.StartsWith('.') ? extension.ToLowerInvariant() : '.' + extension.ToLowerInvariant();

        foreach (var format in Enum.GetValues<ImageFormatType>())
        {
            var memberInfo = typeof(ImageFormatType).GetMember(format.ToString()).FirstOrDefault();
            if (memberInfo != null)
            {
                var attr = memberInfo.GetCustomAttribute<FileExtensionAttribute>();
                if (attr != null && attr.Extensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
                    return format;
            }
        }

        return ImageFormatType.Unknown;
    }
}