using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace com.csutil {

    public class MainThread : MonoBehaviour {

        private static MainThread _instance;

        public static MainThread instance {
            get {
                if (_instance.IsNullOrDestroyed()) {
                    if (Application.isPlaying) {
                        _instance = new GameObject("MainThread").AddComponent<MainThread>();
                        _instance.mainThreadRef = Thread.CurrentThread;
                        _instance.stopWatch = Stopwatch.StartNew();
                        DontDestroyOnLoad(_instance.gameObject);
                    } else {
                        throw Log.e("MainThread not initialized during playmode via MainThread.instance");
                    }
                }
                return _instance;
            }
        }

        /// <summary> Will be false if the app is still in initialization phase and the
        /// main thread is not yet ready to use </summary>
        public static bool IsReadyToUse => _instance != null;

        public static bool isMainThread => IsReadyToUse && instance.mainThreadRef.Equals(Thread.CurrentThread);

        private Thread mainThreadRef;

        // If a task takes longer than this it will be split up over multiple frames, default value is 33ms which is about 30fps
        public long maxAllowedTaskDurationInMsPerFrame = 33;
        private Stopwatch stopWatch;
        private readonly ConcurrentQueue<Action> actionsForMainThread = new ConcurrentQueue<Action>();

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

        [Obsolete("It's recommended to use the async version that returns a task instead")]
        public static T Invoke<T>(Func<T> a) { return instance.ExecuteOnMainThread(a); }

        public static Task<T> Invoke<T>(Func<Task<T>> a) { return instance.ExecuteOnMainThreadAsync(a); }

        public static Task Invoke(Func<Task> a) {
            return instance.ExecuteOnMainThreadAsync(async () => {
                await a();
                return true;
            });
        }

        public static Task Invoke<T>(Func<Task> a) {
            return instance.ExecuteOnMainThreadAsync(async () => {
                await a();
                return true;
            });
        }

        public void ExecuteOnMainThread(Action a) {
            actionsForMainThread.Enqueue(a);
        }

        [Obsolete("It's recommended to use ExecuteOnMainThreadAsync instead")]
        public T ExecuteOnMainThread<T>(Func<T> f) {
            if (isMainThread) { return f(); } // To ensure the main thread cant block itself
            TaskCompletionSource<T> src = new TaskCompletionSource<T>();
            ExecuteOnMainThread(() => {
                try {
                    src.SetResult(f());
                } catch (Exception e) {
                    src.SetException(e);
                }
            });
            return src.Task.Result;
        }

        public async Task<T> ExecuteOnMainThreadAsync<T>(Func<Task<T>> f) {
            TaskCompletionSource<Task<T>> tcs = new TaskCompletionSource<Task<T>>();
            ExecuteOnMainThread(() => {
                try {
                    tcs.SetResult(f());
                } catch (Exception e) {
                    tcs.SetException(e);
                }
            });
            var taskT = await tcs.Task; // Wait for the tcs to be set
            return await taskT; // Wait for the async task itself to be complete
        }

    }

}