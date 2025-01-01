using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace com.csutil.model.ecs {

    public class ExpensiveRegularAsyncTaskInBackground {

        private ConcurrentDictionary<string, Action> waitingTasks = new ConcurrentDictionary<string, Action>();
        private ConcurrentDictionary<string, Task> runningTasks = new ConcurrentDictionary<string, Task>();

        /// <summary>
        /// - If there is already a task for that entity in the queue it will be replaced with the new task
        /// </summary>
        /// <param name="contextId"> The context of the task, e.g. the entity id that the task is about </param>
        /// <param name="taskToDoInBackground"> The task that should be executed in the background </param>
        /// <returns> The returned task will be completed when the task was added to the queue </returns>
        public Task SetTaskFor(string contextId, Action taskToDoInBackground) {
            lock (contextId) {
                if (runningTasks.TryGetValue(contextId, out var runningTask) && !runningTask.IsCompleted) {
                    // There is currently already a task running for that entity
                    waitingTasks[contextId] = taskToDoInBackground;
                    return Task.CompletedTask;
                }
                var addedTask = TaskV2.Run(() => {
                    try {
                        taskToDoInBackground();
                    } finally {
                        runningTasks.TryRemove(contextId, out _);
                        if (waitingTasks.TryRemove(contextId, out var nextTask)) {
                            SetTaskFor(contextId, nextTask);
                        }
                    }
                });
                runningTasks[contextId] = addedTask;
                return Task.CompletedTask;
            }
        }
    }

}