#include <OpenEXR/ImfArray.h>
#include <OpenEXR/ImfRgba.h>
#include <OpenEXR/ImfRgbaFile.h>
#include <cstring>

#ifdef _WIN32
#define EXR_API __declspec(dllexport)
#else
#define EXR_API __attribute__((visibility("default")))
#endif

extern "C" {
EXR_API bool load_exr_rgba(const char *path, float **out_pixels, int *width, int *height) {
    try {
        Imf::RgbaInputFile file(path);
        Imath::Box2i dw = file.dataWindow();
        int w = dw.max.x - dw.min.x + 1;
        int h = dw.max.y - dw.min.y + 1;

        *width = w;
        *height = h;

        Imf::Array2D<Imf::Rgba> pixels;
        pixels.resizeErase(h, w);
        file.setFrameBuffer(&pixels[0][0] - dw.min.x - dw.min.y * w, 1, w);
        file.readPixels(dw.min.y, dw.max.y);

        *out_pixels = new float[w * h * 4];
        for (int y = 0; y < h; ++y) {
            for (int x = 0; x < w; ++x) {
                int i = y * w + x;
                (*out_pixels)[i * 4 + 0] = pixels[y][x].r;
                (*out_pixels)[i * 4 + 1] = pixels[y][x].g;
                (*out_pixels)[i * 4 + 2] = pixels[y][x].b;
                (*out_pixels)[i * 4 + 3] = pixels[y][x].a;
            }
        }

        return true;
    } catch (...) {
        *out_pixels = nullptr;
        *width = *height = 0;
        return false;
    }
}

EXR_API void free_exr_pixels(float *ptr) { delete[] ptr; }
}
