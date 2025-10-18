cmake_minimum_required(VERSION 3.13)
project(OpenEXRWrapper)

# Standard settings
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

# Link OpenEXR
find_package(OpenEXR REQUIRED)

add_library(exr_native SHARED exr_native.cpp)
target_link_libraries(exr_native OpenEXR::OpenEXR)

# Ensure macOS uses @rpath and point it at @loader_path
set(CMAKE_MACOSX_RPATH ON)

# Build-time and install-time rpaths: only @loader_path
set(CMAKE_BUILD_RPATH "@loader_path")
set(CMAKE_INSTALL_RPATH "@loader_path")
set(CMAKE_INSTALL_RPATH_USE_LINK_PATH FALSE)   # don't auto-add /opt/homebrew/*

# Make build artifact use the install rpath
set(CMAKE_BUILD_WITH_INSTALL_RPATH TRUE)

if(APPLE)
  target_link_options(exr_native PRIVATE "-Wl,-rpath,@loader_path")
endif()

# Set export visibility for non-Windows
if (NOT WIN32)
    target_compile_options(exr_native PRIVATE -fvisibility=hidden)
endif ()