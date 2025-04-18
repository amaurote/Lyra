#include <cstdio>
#include <cstdlib>
#include <cstring>
#include "rgbe.h"

#ifdef _WIN32
  #define HDR_API __declspec(dllexport)
#else
#define HDR_API __attribute__((visibility("default")))
#endif

extern "C" {

HDR_API bool load_hdr_rgba(const char *path, float **out_pixels, int *width, int *height) {
    FILE *file = fopen(path, "rb");
    if (!file)
        return false;

    RGBE_ReadHeader(file, width, height, nullptr);

    int totalPixels = (*width) * (*height);
    float *buffer = (float *) malloc(sizeof(float) * totalPixels * 4);
    if (!buffer) {
        fclose(file);
        return false;
    }

    float *rgb = (float *) malloc(sizeof(float) * totalPixels * 3);
    if (!rgb) {
        fclose(file);
        free(buffer);
        return false;
    }

    if (RGBE_ReadPixels_RLE(file, rgb, *width, *height) < 0) {
        fclose(file);
        free(buffer);
        free(rgb);
        return false;
    }

    fclose(file);

    for (int i = 0; i < totalPixels; ++i) {
        buffer[i * 4 + 0] = rgb[i * 3 + 0];
        buffer[i * 4 + 1] = rgb[i * 3 + 1];
        buffer[i * 4 + 2] = rgb[i * 3 + 2];
        buffer[i * 4 + 3] = 1.0f; // Alpha
    }

    free(rgb);
    *out_pixels = buffer;
    return true;
}

HDR_API void free_hdr_pixels(float *ptr) {
    free(ptr);
}
}
