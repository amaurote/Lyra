using System.Runtime.InteropServices;
using Lyra.Common;

namespace Lyra;

static class Program
{
    private static void Main()
    {
        LogSetup();
        Logger.Info($"[Application] Application started on {RuntimeInformation.RuntimeIdentifier}");

        Imaging.ImageStore.Initialize();

        try
        {
            using var viewer = new SdlCore.SdlCore();
            viewer.Run();
        }
        catch (Exception ex)
        {
            Logger.Error($"[Unhandled Exception]: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private static void LogSetup()
    {
        Logger.SetLogStrategy(Logger.LogStrategy.Both);
        Logger.SetLogDebugMode(false);
        Logger.ClearLog();
    }
}