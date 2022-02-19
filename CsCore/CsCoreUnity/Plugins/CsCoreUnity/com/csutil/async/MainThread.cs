using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace com.csutil {

    public class MainThread : MonoBehaviour {

        public static MainThread instance {
            get {
                AssertV2.IsTrue(ApplicationV2.isPlaying, "In EDIT mode!");
                return IoC.inject.GetOrAddComponentSingleton<MainThread>(new object());
            }
        }

        public static bool isMainThread { get { return mainThreadRef.Equals(Thread.CurrentThread); } }

        private static Thread mainThreadRef;

        public long maxAllowedTaskDurationInMsPerFrame = 33;
        private Stopwatch stopWatch;
        private bool WasInitializedWhilePlaying { get { return stopWatch != null; } }
        private ConcurrentQueue<Action> actionsForMainThread = new ConcurrentQueue<Action>();

        private void Awake() {
            if (mainThreadRef != null) { throw Log.e("There is already a MainThread"); }
            mainThreadRef = Thread.CurrentThread;
        }

        private void OnEnable() {
            if (mainThreadRef != Thread.CurrentThread) { mainThreadRef = Thread.CurrentThread; }
            stopWatch = Stopwatch.StartNew();
        }

        private void OnDestroy() {
            mainThreadRef = null;
        }

        private void Update() {
            if (!actionsForMainThread.IsEmpty) {
                stopWatch.Restart();
                while (!actionsForMainThread.IsEmpty) {
                    // If the tasks take too long do the rest of the waiting tasks in the next frame to not freeze main thread:
                    if (stopWatch.ElapsedMilliseconds > maxAllowedTaskDurationInMsPerFrame) { break; }
                    Action a;
                    if (actionsForMainThread.TryDequeue(out a)) {
                        try { a.Invoke(); } catch (Exception e) { Log.e(e); }
                    }
                }
            }
        }

        public static void Invoke(Action a) { instance.ExecuteOnMainThread(a); }

        public void ExecuteOnMainThread(Action a) {
            if (ApplicationV2.isPlaying) { AssertV2.IsNotNull(mainThreadRef, "mainThreadRef"); }
            if (WasInitializedWhilePlaying) {
                actionsForMainThread.Enqueue(a);
            } else if (!ApplicationV2.isPlaying) {
                Log.d("ExecuteOnMainThread: Application not playing, action will be instantly executed now");
                a();
            } else {
                throw Log.e("MainThread not initialized via MainThread.instance");
            }
        }

        public T ExecuteOnMainThread<T>(Func<T> f) {
            if (isMainThread) { return f(); }
            TaskCompletionSource<T> src = new TaskCompletionSource<T>();
            ExecuteOnMainThread(() => { try { src.SetResult(f()); } catch (Exception e) { src.SetException(e); } });
            return src.Task.Result;
        }

        public Task<T> ExecuteOnMainThreadAsync<T>(Func<Task<T>> f) {
            return ExecuteOnMainThread(() => f());
        }

    }

}
