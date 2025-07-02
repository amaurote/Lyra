using Lyra.Common.SystemExtensions;
using Lyra.Imaging.Data;
using Lyra.SdlCore;
using SkiaSharp;

namespace Lyra.Renderer.Overlay;

public class ImageInfoOverlay : IOverlay<(Composite? composite, ViewerState states)>
{
    private readonly SKPaint _textPaint = new()
    {
        Color = SKColors.White,
        IsAntialias = true
    };

    private readonly SKPaint _extrasPaint = new()
    {
        Color = SKColors.Goldenrod,
        IsAntialias = true
    };

    private readonly SKPaint _failedPaint = new()
    {
        Color = SKColors.Firebrick,
        IsAntialias = true
    };

    public float Scale { get; set; }
    public SKFont? Font { get; set; }

    public ImageInfoOverlay()
    {
        ReloadFont();
    }

    public void ReloadFont()
    {
        Font = FontHelper.GetScaledMonoFont(14, Scale);
    }

    public void Render(SKCanvas canvas, DrawableBounds drawableBounds, SKColor textPaint, (Composite? composite, ViewerState states) data)
    {
        if (Font == null)
            return;

        if (data.composite == null)
            return;

        _textPaint.Color = textPaint;

        var padding = 13 * Scale;
        var lineHeight = Font.Size + (7 * Scale);

        var textY = padding + Font.Size;
        foreach (var line in PrepareInfoLines(data.composite, data.states))
        {
            var x = padding;
            var i = 0;
            var currentPaint = _textPaint;

            while (i < line.Length)
            {
                var (tag, tagLength) = TryParseTag(line, i);
                if (tag != null)
                {
                    currentPaint = tag switch
                    {
                        "e" => _extrasPaint,
                        "f" => _failedPaint,
                        "/" => _textPaint,
                        _ => currentPaint
                    };
                    i += tagLength;
                    continue;
                }

                var nextTagIdx = line.IndexOf('<', i);
                var end = nextTagIdx == -1 ? line.Length : nextTagIdx;

                var textSpan = line[i..end];
                canvas.DrawText(textSpan, x, textY, SKTextAlign.Left, Font, currentPaint);
                x += Font.MeasureText(textSpan, currentPaint);

                i = end;
            }

            textY += lineHeight;
        }
    }

    private static (string? tag, int length) TryParseTag(string line, int index)
    {
        if (line[index] != '<' || index + 2 >= line.Length)
            return (null, 0);

        if (line[index + 2] == '>' && (line[index + 1] is 'e' or 'f' or '/'))
            return (line[index + 1].ToString(), 3);

        return (null, 0);
    }

    private List<string> PrepareInfoLines(Composite composite, ViewerState states)
    {
        var fileInfo = composite.FileInfo;
        var fileSize = fileInfo.Length switch
        {
            >= 2 * 1024 * 1024 => $"{Math.Round(fileInfo.Length / (1024.0 * 1024), 1)} MB",
            >= 1024 => $"{Math.Round(fileInfo.Length / 1024.0)} kB",
            _ => $"{fileInfo.Length} bytes"
        };

        var width = composite.Image?.Width ?? composite.Picture?.CullRect.Width ?? 0;
        var height = composite.Image?.Height ?? composite.Picture?.CullRect.Height ?? 0;

        var dirNav = string.Empty;
        if (states is { DirectoryCount: not null, DirectoryIndex: not null })
            dirNav = $" <e>({states.DirectoryIndex}/{states.DirectoryCount})</>";

        var lines = new List<string>
        {
            $"[Collection]    {states.CollectionType}  |  Dir: {composite.FileInfo.DirectoryName}/",
            $"[File]          {states.CollectionIndex}/{states.CollectionCount}{dirNav}  |  {fileInfo.Name}  |  {fileSize}",
            $"[Image]         {composite.ImageFormatType.Description()}  |  {width}x{height}"
            + (composite.IsGrayscale ? "  |  Greyscale" : ""),
            $"[Displaying]    Zoom: {states.Zoom}%  |  Display Mode: {states.DisplayMode}",
            $"[System]        Graphics API: OpenGL  |  Sampling: {states.SamplingMode}"
        };

        if (states.ShowExif && composite.ExifInfo != null)
        {
            lines.AddRange(["", "", "", "[EXIF ↯]", ""]);

            if (!composite.ExifInfo.IsValid())
            {
                lines.Add("<f>Failed to parse EXIF metadata.</>");
            }
            else if (composite.ExifInfo.HasData())
            {
                var exifLines = composite.ExifInfo.ToLines();
                lines.AddRange(exifLines);
            }
        }

        return lines;
    }
}