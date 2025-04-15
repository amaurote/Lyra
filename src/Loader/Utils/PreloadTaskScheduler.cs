using System.Collections.Concurrent;

namespace Lyra.Loader.Utils;

public class PreloadTaskScheduler : TaskScheduler
{
    private readonly BlockingCollection<Task> _tasks = new();
    private readonly List<Thread> _threads; // Hold thread references to prevent GC collection

    public PreloadTaskScheduler(int maxDegreeOfParallelism)
    {
        _threads = Enumerable.Range(0, maxDegreeOfParallelism).Select(i =>
        {
            var thread = new Thread(() =>
            {
                foreach (var task in _tasks.GetConsumingEnumerable())
                    TryExecuteTask(task);
            })
            {
                IsBackground = true,
                Name = $"PreloadWorker-{i}",
                Priority = ThreadPriority.BelowNormal
            };
            thread.Start();
            return thread;
        }).ToList();
    }

    protected override IEnumerable<Task>? GetScheduledTasks() => _tasks.ToArray();
    protected override void QueueTask(Task task) => _tasks.Add(task);
    protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) => false;
}