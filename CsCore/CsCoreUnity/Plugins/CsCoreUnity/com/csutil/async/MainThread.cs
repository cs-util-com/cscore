using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

namespace com.csutil {

    public class MainThread : MonoBehaviour {

        public static MainThread instance { get { return IoC.inject.GetOrAddComponentSingleton<MainThread>(new object()); } }

        public static bool isMainThread { get { return mainThreadRef.Equals(Thread.CurrentThread); } }

        private static Thread mainThreadRef;

        public long maxAllowedTaskDurationInMsPerFrame = 33;
        private Stopwatch stopWatch;
        private bool WasInitializedWhilePlaying { get { return stopWatch != null; } }
        private ConcurrentQueue<Action> actionsForMainThread = new ConcurrentQueue<Action>();

        private void Awake() {
            UnityEngine.Debug.Log($"MainThread_{GetHashCode()}.Awake (while Application.isPlaying={Application.isPlaying})", gameObject);
            if (mainThreadRef != null) { throw Log.e("There is already a MainThread"); }
            mainThreadRef = Thread.CurrentThread;
        }

        private void OnEnable() {
            if (mainThreadRef != Thread.CurrentThread) { mainThreadRef = Thread.CurrentThread; }
            stopWatch = Stopwatch.StartNew();
        }

        private void OnDestroy() {
            UnityEngine.Debug.Log($"MainThread_{GetHashCode()}.OnDestroy (while Application.isPlaying={Application.isPlaying})", gameObject);
            mainThreadRef = null;
        }

        private void Update() {
            if (!actionsForMainThread.IsEmpty) {
                stopWatch.Restart();
                while (!actionsForMainThread.IsEmpty) {
                    // if the tasks take too long do the rest of the waiting tasks in the next frame:
                    if (stopWatch.ElapsedMilliseconds > maxAllowedTaskDurationInMsPerFrame) {
                        Log.w("Will wait until next frame to run the remaining " + actionsForMainThread.Count + " tasks");
                        break;
                    }
                    Action a; if (actionsForMainThread.TryDequeue(out a)) {
                        try { a.Invoke(); } catch (Exception e) { Log.e(e); }
                    }
                }
            }
        }

        public static void Invoke(Action a) { instance.ExecuteOnMainThread(a); }

        public void ExecuteOnMainThread(Action a) {
            if (Application.isPlaying) { AssertV2.IsNotNull(mainThreadRef, "mainThreadRef"); }
            if (WasInitializedWhilePlaying) {
                actionsForMainThread.Enqueue(a);
            } else if (!Application.isPlaying) {
                Log.d("ExecuteOnMainThread: Application not playing, action will be instantly executed now");
                a();
            } else {
                throw Log.e("MainThread not initialized via MainThread.instance");
            }
        }

    }

}
