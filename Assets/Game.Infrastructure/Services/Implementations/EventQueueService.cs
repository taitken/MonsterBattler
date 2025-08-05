using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class EventQueueService : IEventQueueService
{
    private readonly Queue<Func<Task>> queue = new();

    public void Enqueue(Func<Task> action)
    {
        queue.Enqueue(action);
    }

    public async Task ProcessAll()
    {
        while (queue.Count > 0)
        {
            var action = queue.Dequeue();
            await action();
        }
    }
}