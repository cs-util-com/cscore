using UnityEngine;
using System.Collections;
using System.Threading;

namespace com.csutil {

    public static class ThreadingExtensions {

        public static Coroutine StartCoroutineInBgThread(this MonoBehaviour self, IEnumerator routine) {
            AsyncTask t;
            return StartCoroutineInBgThread(self, routine, out t);
        }

        public static Coroutine StartCoroutineInBgThread(this MonoBehaviour self, IEnumerator routine, out AsyncTask task) {
            task = new AsyncTask(routine);
            return self.StartCoroutine(task);
        }

    }

    public static class ThreadSwitcher {
        /// <summary> Yield return it to switch to Unity main thread. </summary>
        public static readonly object ToMainThread;
        /// <summary> Yield return it to switch to background thread. </summary>
        public static readonly object ToBackgroundThread;

        static ThreadSwitcher() {
            ToMainThread = new object();
            ToBackgroundThread = new object();
        }
    }

    public enum TaskState { Init, Running, Done, Cancelled, Error }

    public class AsyncTask : IEnumerator {
        public object Current { get; private set; }

        public void Reset() { throw new System.NotSupportedException("Not support calling Reset() on iterator."); }

        // inner running state used by state machine;
        private enum RunningState { Init, RunningAsync, PendingYield, ToBackground, RunningSync, CancellationRequested, Done, Error }

        // routine user want to run
        private readonly IEnumerator _innerRoutine;

        // current running state
        private RunningState _state;
        // last running state
        private RunningState _previousState;
        // temporary stores current yield return value until we think Unity coroutine engine is OK to get it
        private object _pendingCurrent;

        public TaskState State {
            get {
                switch (_state) {
                    case RunningState.CancellationRequested:
                        return TaskState.Cancelled;
                    case RunningState.Done:
                        return TaskState.Done;
                    case RunningState.Error:
                        return TaskState.Error;
                    case RunningState.Init:
                        return TaskState.Init;
                    default:
                        return TaskState.Running;
                }
            }
        }

        public System.Exception Exception { get; private set; }

        public AsyncTask(IEnumerator routine) {
            _innerRoutine = routine;
            // runs into background first;
            _state = RunningState.Init;
        }

        public void Cancel() {
            if (State == TaskState.Running) {
                GotoState(RunningState.CancellationRequested);
            }
        }

        /// <summary> A co-routine that waits the task. </summary>
        public IEnumerator Wait() {
            while (State == TaskState.Running) { yield return null; }
        }

        // thread safely switch running state;
        private void GotoState(RunningState state) {
            if (_state == state) { return; }

            lock (this) {
                // maintainance the previous state;
                _previousState = _state;
                _state = state;
            }
        }

        // thread safely save yield returned value;
        private void SetPendingCurrentObject(object current) {
            lock (this) { _pendingCurrent = current; }
        }

        /// <summary> Runs next iteration. </summary>
        /// <returns> true for continue, otherwise false </returns>
        public bool MoveNext() {
            // no running for null;
            if (_innerRoutine == null) { return false; }

            // set current to null so that Unity not get same yield value twice;
            Current = null;

            // loops until the inner routine yield something to Unity;
            while (true) {
                // a simple state machine;
                switch (_state) {
                    // first, goto background;
                    case RunningState.Init:
                        GotoState(RunningState.ToBackground);
                        break;
                    // running in background, wait a frame;
                    case RunningState.RunningAsync:
                        return true;
                    // runs on main thread;
                    case RunningState.RunningSync:
                        MoveNextUnity();
                        break;
                    // need switch to background;
                    case RunningState.ToBackground:
                        GotoState(RunningState.RunningAsync);
                        // call the thread launcher;
                        MoveNextAsync();
                        return true;
                    // something was yield returned;
                    case RunningState.PendingYield:
                        if (_pendingCurrent == ThreadSwitcher.ToBackgroundThread) {
                            // do not break the loop, switch to background;
                            GotoState(RunningState.ToBackground);
                        } else if (_pendingCurrent == ThreadSwitcher.ToMainThread) {
                            // do not break the loop, switch to main thread;
                            GotoState(RunningState.RunningSync);
                        } else {
                            // not from the SwitchThreadHelper, then Unity should get noticed,
                            // Set to Current property to achieve this;
                            Current = _pendingCurrent;
                            // yield from background thread, or main thread?
                            if (_previousState == RunningState.RunningAsync) {
                                // if from background thread, 
                                // go back into background in the next loop;
                                _pendingCurrent = ThreadSwitcher.ToBackgroundThread;
                            } else {
                                // otherwise go back to main thread the next loop;
                                _pendingCurrent = ThreadSwitcher.ToMainThread;
                            }
                            // end this iteration and Unity get noticed;
                            return true;
                        }
                        break;
                    // done running, pass false to Unity;
                    case RunningState.Done:
                    case RunningState.CancellationRequested:
                    default:
                        return false;
                }
            }
        }

        // background thread launcher;
        private void MoveNextAsync() {
            ThreadPool.QueueUserWorkItem(
                new WaitCallback(BackgroundRunner));
        }

        // background thread function;
        private void BackgroundRunner(object state) {
            // just run the sync version on background thread;
            MoveNextUnity();
        }

        // run next iteration on main thread;
        private void MoveNextUnity() {
            try {
                // run next part of the user routine;
                if (_innerRoutine.MoveNext()) {
                    // something has been yield returned, handle it;
                    SetPendingCurrentObject(_innerRoutine.Current);
                    GotoState(RunningState.PendingYield);
                } else {
                    // user routine simple done;
                    GotoState(RunningState.Done);
                }
            }
            catch (System.Exception ex) {
                this.Exception = ex;
                Log.e(ex);
                GotoState(RunningState.Error); // Terminate the task
            }
        }

    }

}