using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IEventQueueService
{
    void Enqueue(Func<Task> action);

    Task ProcessAll();
}