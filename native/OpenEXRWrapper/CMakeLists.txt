cmake_minimum_required(VERSION 3.10)
project(OpenEXRWrapper)

# Link OpenEXR
find_package(OpenEXR REQUIRED)

add_library(exr_native SHARED exr_native.cpp)
target_link_libraries(exr_native OpenEXR::OpenEXR)

# Set export visibility for non-Windows
if (NOT WIN32)
    target_compile_options(exr_native PRIVATE -fvisibility=hidden)
endif ()