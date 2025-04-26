using System.ComponentModel;

namespace Lyra.Imaging.Data;

public struct ExifInfo
{
    public string Make;
    public string Model;
    public string Lens;

    [EmptyLine]
    [Description("Exposure Time")]
    public string ExposureTime;

    [Description("Apeture")]
    public string FNumber;

    [Description("ISO")]
    public string Iso;

    [EmptyLine]
    public string Taken;

    [EmptyLine]
    [Description("GPS Latitude")]
    public string GpsLatitude;

    [Description("GPS Longitude")]
    public string GpsLongitude;

    [EmptyLine]
    public string Compression;

    public string Software;

    public bool HasData()
    {
        var fields = typeof(ExifInfo).GetFields();
        var info = this;
        return fields.Any(field => 
            field.GetValue(info) is string value && !string.IsNullOrWhiteSpace(value));
    }
    
    public List<string> ToLines()
    {
        var lines = new List<string>();
        var fields = typeof(ExifInfo).GetFields();

        foreach (var field in fields)
        {
            if (field.IsDefined(typeof(EmptyLineAttribute), false))
                lines.Add(string.Empty);
            
            var value = field.GetValue(this) as string;
            if (!string.IsNullOrWhiteSpace(value))
            {
                var description = field.GetCustomAttributes(typeof(DescriptionAttribute), false)
                    .OfType<DescriptionAttribute>()
                    .FirstOrDefault()?.Description ?? field.Name;

                lines.Add($"{description}: {value}");
            }
        }

        return PostProcessLines(lines);
    }

    private List<string> PostProcessLines(List<string> lines)
    {
        var cleanedLines = new List<string>();
        var lastWasEmpty = false;

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                if (lastWasEmpty) 
                    continue; // Allow one empty line
                
                cleanedLines.Add(string.Empty);
                lastWasEmpty = true;
            }
            else
            {
                cleanedLines.Add(line);
                lastWasEmpty = false;
            }
        }

        // Remove leading/trailing empty lines
        while (cleanedLines.Count > 0 && string.IsNullOrWhiteSpace(cleanedLines[0]))
            cleanedLines.RemoveAt(0);

        while (cleanedLines.Count > 0 && string.IsNullOrWhiteSpace(cleanedLines[^1]))
            cleanedLines.RemoveAt(cleanedLines.Count - 1);

        return cleanedLines;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class EmptyLineAttribute : Attribute;