using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Concurrent;
using System.Threading;
using System.Diagnostics;
using System.Collections;

namespace com.csutil {

    /// <summary>
    /// If an asyn task is started manually in Unity is should be created through this TaskRunner instead of
    /// using Task.Run or Task.Factory manually, to connect it to the scenes lifecycle. 
    /// </summary>
    public class TaskRunner : MonoBehaviour {

        public static TaskRunner instance { get { return IoC.inject.GetOrAddComponentSingleton<TaskRunner>(new object()); } }

        private ConcurrentQueue<InteruptableTask> monitoredTasks = new ConcurrentQueue<InteruptableTask>();

        public MonitoredTask RunInBackground(Func<CancellationToken, Task> asyncAction) {
            return RunInBackground(asyncAction, TaskScheduler.Default);
        }

        public MonitoredTask RunInBackground(Func<CancellationToken, Task> asyncAction, TaskScheduler scheduler, TaskCreationOptions o = TaskCreationOptions.None) {
            var cancelToken = new CancellationTokenSource();
            Task task = TaskFactoryStartNew(() => asyncAction(cancelToken.Token),
                cancelToken.Token, o, scheduler).Unwrap();
            var mt = new MonitoredTask() { task = task, cancelTask = () => { cancelToken.Cancel(); } };
            return AddToMonitoredTasks(mt, cancelToken);
        }

        public MonitoredTask<T> RunInBackground<T>(Func<CancellationToken, Task<T>> asyncFunction) {
            return RunInBackground<T>(asyncFunction, TaskScheduler.Default);
        }

        public MonitoredTask<T> RunInBackground<T>(Func<CancellationToken, Task<T>> asyncFunction, TaskScheduler scheduler, TaskCreationOptions o = TaskCreationOptions.None) {
            var cancelToken = new CancellationTokenSource();
            Task<T> task = TaskFactoryStartNew(() => asyncFunction(cancelToken.Token),
                cancelToken.Token, o, scheduler).Unwrap();
            var mt = new MonitoredTask<T>() { task = task, cancelTask = () => { cancelToken.Cancel(); } };
            return AddToMonitoredTasks(mt, cancelToken);
        }

        private Task<T> TaskFactoryStartNew<T>(Func<T> p, CancellationToken token, TaskCreationOptions o, TaskScheduler scheduler) {
            if (EnvironmentV2.isWebGL) { throw Log.e("WebGL cant handle Task.Factory.StartNew!"); }
            return Task.Factory.StartNew(p, token, o, scheduler);
        }

        public T AddToMonitoredTasks<T>(T task, CancellationTokenSource c) where T : InteruptableTask {
            monitoredTasks.Enqueue(task);
            return task;
        }

        private void OnDestroy() {
            Log.d("TaskRunner.OnDestroy: Checking " + monitoredTasks.Count + " tasks, if they need to be stopped");
            while (!monitoredTasks.IsEmpty) {
                InteruptableTask t; if (monitoredTasks.TryDequeue(out t)) {
                    try { t.cancelTask(); } catch (Exception e) { Log.e(e); }
                }
            }
        }

        public class InteruptableTask { public Action cancelTask; }
        public class MonitoredTask : InteruptableTask { public Task task; }
        public class MonitoredTask<T> : InteruptableTask { public Task<T> task; }

    }

}
