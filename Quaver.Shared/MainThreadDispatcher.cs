using System;
using System.Collections.Concurrent;
using System.Threading;

public static class MainThreadDispatcher
{
    private static readonly ConcurrentQueue<Action> queue = new();
    private static int mainThreadId;

    public static void Initialize()
    {
        mainThreadId = Thread.CurrentThread.ManagedThreadId;
    }

    public static void Enqueue(Action action)
    {
        queue.Enqueue(action);
    }

    public static void ExecuteAll()
    {
        while (queue.TryDequeue(out var action))
        {
            action();
        }
    }

    public static T RunOnMainThread<T>(Func<T> func)
    {
        if (IsMainThread)
            return func();

        T result = default;
        bool done = false;

        Enqueue(() =>
        {
            result = func();
            done = true;
        });

        // Spin until done
        while (!done)
            Thread.Yield();

        return result;
    }

    public static bool IsMainThread =>
        Thread.CurrentThread.ManagedThreadId == mainThreadId;
}
