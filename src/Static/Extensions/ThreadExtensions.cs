namespace Lyra.Static.Extensions;

public static class ThreadExtensions
{
    public static string GetNameOrId(this Thread thread) =>
        thread.Name ?? thread.ManagedThreadId.ToString();
}