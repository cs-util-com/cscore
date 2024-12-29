using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace com.csutil.model.ecs {

    public interface IDoEntityTaskInBackground {
        /// <summary>
        /// - If there is already a task for that entity in the queue it will be replaced with the new task
        /// </summary>
        /// <param name="entityId"> The context of the task, e.g. the entity id that the task is about </param>
        /// <param name="taskToDoInBackground"> The task that should be executed in the background </param>
        /// <returns> The returned task will be completed when the task was added to the queue </returns>
        Task SetTaskFor(string entityId, Action taskToDoInBackground);
    }

    public class DoEntityTaskInBackground : IDoEntityTaskInBackground {

        private ConcurrentDictionary<string, Action> waitingTasks = new ConcurrentDictionary<string, Action>();
        private ConcurrentDictionary<string, Task> runningTasks = new ConcurrentDictionary<string, Task>();

        public Task SetTaskFor(string entityId, Action taskToDoInBackground) {
            lock (entityId) {
                if (runningTasks.TryGetValue(entityId, out var runningTask) && !runningTask.IsCompleted) {
                    // There is currently already a task running for that entity
                    waitingTasks[entityId] = taskToDoInBackground;
                    return Task.CompletedTask;
                }
                var addedTask = TaskV2.Run(() => {
                    try {
                        taskToDoInBackground();
                    } finally {
                        runningTasks.TryRemove(entityId, out _);
                        if (waitingTasks.TryRemove(entityId, out var nextTask)) {
                            SetTaskFor(entityId, nextTask);
                        }
                    }
                });
                runningTasks[entityId] = addedTask;
                return Task.CompletedTask;
            }
        }
    }

}