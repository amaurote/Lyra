# Lyra Viewer

Lyra Viewer is built primarily using **SDL3** and **SkiaSharp**, combining the speed and flexibility of native rendering
with a streamlined .NET architecture.

---

## Overview

Lyra Viewer is a high-performance image viewer built with **SDL3** and **SkiaSharp**, designed for **speed, minimalism, and smooth navigation**. It provides a distraction-free way to preview and inspect image assets — ideal for creative professionals that use images as a core resource in their workflow, such as:

- 2D/3D artists
- Game developers
- Environment designers
- Asset pipeline engineers
- Graphic designers and photographers  

Lyra is optimized for quickly browsing large collections of texture maps, HDRIs, baked image assets, and previews exported from tools like **Blender**, **Quixel Bridge**, and other content creation pipelines.

---

## Supported Image Formats

> _Note: Crossed-out formats are not implemented yet._

### **Common Texture Formats (Essential)**

| Format           | Description                                                                   |
|------------------|-------------------------------------------------------------------------------|
| `.png`           | Widely used lossless format, ideal for albedo, roughness, etc.                |
| `.jpg` / `.jpeg` | Lossy format used for color/albedo maps or previews.                          |
| `.jfif`          | JPEG File Interchange Format — a JPEG variant seen in EXIF/exported previews. |
| `.tga`           | Game engine-friendly format supporting alpha channels.                        |
| `.tif` / `.tiff` | High-quality textures, often 16/32-bit, used in VFX.                          |
| `.bmp`           | Rare but used in older or internal pipelines.                                 |

### **Modern / Web-Friendly Formats**

| Format                      | Description                                                              |
|-----------------------------|--------------------------------------------------------------------------|
| `.webp`                     | Lightweight format with alpha support, used for previews and thumbnails. |
| `.heif` / `.heic` / `.avif` | High Efficiency format common on macOS/iOS systems.                      |

### **High Dynamic Range Formats**

| Format | Description                                                                |
|--------|----------------------------------------------------------------------------|
| `.exr` | OpenEXR format for HDR data (e.g. height, displacement, environment maps). |
| `.hdr` | Radiance HDR format, used in lighting environments and skies.              |

### **Compressed / GPU-Friendly Formats**

| Format               | Description                                                             |
|----------------------|-------------------------------------------------------------------------|
| ~~`.dds`~~           | ~~DirectDraw Surface, used in real-time engines (DXT/BC compression).~~ |
| ~~`.ktx` / `.ktx2`~~ | ~~Modern GPU-optimized format for real-time textures.~~                 |

### **Vector Formats / Specialized Formats**

| Format     | Description                                            |
|------------|--------------------------------------------------------|
| ~~`.svg`~~ | ~~Scalable vector graphics for masks or UI overlays.~~ |
| ~~`.psd`~~ | ~~Photoshop files (preview-only support).~~            |

### **Minor Formats**

| Format      | Description                                                           |
|-------------|-----------------------------------------------------------------------|
| `.ico`      | Icon files, often exported from Blender or as part of UI asset packs. |
| ~~`.icns`~~ | ~~macOS icon format, used in application packaging.~~                 |
| ~~`.jp2`~~  | ~~JPEG 2000, rare but sometimes seen in asset pipelines.~~            |

---

## **Keyboard Shortcuts & Controls**

| Key            | Action                                       |
|----------------|----------------------------------------------|
| `←` / `→`      | Previous / Next image                        |
| `Home` / `End` | First / Last image                           |
| `+` / `-`      | Zoom in / Zoom out                           |
| `Mouse Wheel`  | Zoom at cursor position                      |
| `0`            | Toggle **Fit to Screen** / **Original Size** |
| `F`            | Toggle fullscreen                            |
| ~~`B`~~        | ~~Toggle background mode~~                   |
| ~~`I`~~        | ~~Toggle image information overlay~~         |
| `Esc`          | Exit application                             |

- **Drag & Drop:** Open a file or directory by dragging it into Lyra Viewer.

---

## **Prerequisites & Dependencies**

**Lyra Viewer** is built with **.NET Runtime 9.0** and integrates several high-performance libraries designed to handle
modern image formats and fast rendering:

| Library                  | Purpose                                        | License       |
|--------------------------|------------------------------------------------|---------------|
| **SDL3-CS**              | Core graphics, input, and windowing            | zlib          |
| **SkiaSharp**            | Hardware-accelerated 2D rendering              | BSD-3-Clause  |
| **LibHeifSharp**         | HEIF/HEIC format decoding                      | LGPL-3.0      |
| **SixLabors.ImageSharp** | Support for TGA, TIFF, and legacy formats      | Apache 2.0    |
| **MetadataExtractor**    | Extracts EXIF & image metadata                 | Apache 2.0    |
| **OpenEXR**              | High dynamic range (HDR) image decoding (.exr) | BSD-3-Clause  |
| **rgbe**                 | Radiance HDR (.hdr) image decoding             | Public Domain |

For more information, visit the relevant **GitHub repositories**:

- [SDL3-CS](https://github.com/ethereal-developers-club/SDL3-CS)
- [SkiaSharp](https://github.com/mono/SkiaSharp)
- [LibHeifSharp](https://github.com/0xC0000054/libheif-sharp)
- [SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp)
- [MetadataExtractor](https://github.com/drewnoakes/metadata-extractor-dotnet)
- [OpenEXR](https://github.com/AcademySoftwareFoundation/openexr)
- [rgbe](https://www.graphics.cornell.edu/~bjw/rgbe.html)

---

## Future Ideas

- Channel preview (R, G, B, A, RGB)
- Color space indication (sRGB / linear)
- Display metadata (EXIF, bit depth, resolution, ICC profile)
- Histogram or levels display for HDR/EXR workflows
- Support for multi-layer EXR and PSD navigation
- Integration with asset management tools (e.g., Bridge folder syncing)
- Batch export or format conversion

---

