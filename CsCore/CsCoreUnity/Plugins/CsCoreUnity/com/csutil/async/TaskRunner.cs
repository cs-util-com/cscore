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

    public class TaskRunner : MonoBehaviour {

        public static TaskRunner instance { get { return IoC.inject.GetOrAddComponentSingleton<TaskRunner>(new object()); } }

        private ConcurrentQueue<MonitoredTask> monitoredTasks = new ConcurrentQueue<MonitoredTask>();

        public MonitoredTask RunInBackground(Action<CancellationToken> action) {
            return RunInBackground(action, TaskScheduler.Default);
        }

        public MonitoredTask RunInBackground(Action<CancellationToken> action, TaskScheduler scheduler) {
            var cancelToken = new CancellationTokenSource();
            var t = Task.Factory.StartNew(() => {
                action(cancelToken.Token);
            }, cancelToken.Token, TaskCreationOptions.None, scheduler);
            return MonitorTask(t, cancelToken);
        }

        public MonitoredTask MonitorTask(Task t, CancellationTokenSource cancelToken) {
            var mt = new MonitoredTask() {
                task = t,
                cancelTask = () => { cancelToken.Cancel(); }
            };
            monitoredTasks.Enqueue(mt);
            return mt;
        }

        private void OnDestroy() {
            Log.d("TaskRunner.OnDestroy: Checking " + monitoredTasks.Count + " tasks, if they need to be stopped");
            while (!monitoredTasks.IsEmpty) {
                MonitoredTask t; if (monitoredTasks.TryDequeue(out t)) {
                    try { t.cancelTask(); } catch (Exception e) { Log.e(e); }
                }
            }
        }

        public class MonitoredTask {
            public Task task;
            public Action cancelTask;
        }

    }

}
