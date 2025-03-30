﻿using System.Runtime.InteropServices;
using Lyra;
using Lyra.SdlCore;
using static Lyra.Logger;

class Program
{
    private static void Main()
    {
        LogSetup();
        Log($"[Application] Application started on {RuntimeInformation.RuntimeIdentifier}");

        _ = NativeLibraryLoader.Instance;

        try
        {
            using var viewer = new SdlCore();
            viewer.Run();
        }
        catch (Exception ex)
        {
            Log($"[Unhandled Exception]: {ex.Message}", LogLevel.Error);
        }
    }

    private static void LogSetup()
    {
        SetLogStrategy(LogStrategy.Console);
        SetLogDebugMode(true);
        ClearLog();
    }
}