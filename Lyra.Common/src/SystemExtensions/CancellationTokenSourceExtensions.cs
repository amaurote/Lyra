using System.Runtime.CompilerServices;

namespace Lyra.Common.SystemExtensions;

public static class CancellationTokenSourceExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CancelSilently(this CancellationTokenSource? cts)
    {
        if (cts is null) return;
        try
        {
            cts.Cancel();
        }
        catch (ObjectDisposedException)
        {
        }
        catch (AggregateException)
        {
            // Registered callbacks threw; intentionally ignored here.
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CancelAndDisposeSilently(this CancellationTokenSource? cts)
    {
        if (cts is null) return;
        try
        {
            cts.Cancel();
        }
        catch (ObjectDisposedException)
        {
        }
        catch (AggregateException)
        {
        }
        finally
        {
            cts.Dispose();
        }
    }
}