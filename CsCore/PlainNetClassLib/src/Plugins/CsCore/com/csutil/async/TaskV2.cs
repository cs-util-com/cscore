using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.csutil {

    public class TaskV2 {

        private static int lastOverhead = 0;

        public static async Task Delay(int millisecondsDelay) {
            millisecondsDelay = Math.Max(millisecondsDelay - lastOverhead, 1);
            var t = Stopwatch.StartNew();
            await IoC.inject.GetOrAddSingleton<TaskV2>(null).DelayTask(millisecondsDelay);
            t.Stop();
            lastOverhead = (int)(t.ElapsedMilliseconds - millisecondsDelay);
            if (lastOverhead < 0) { // The wait was shorter then requested:
                await Delay(-lastOverhead); // wait the additional difference
            }
        }

        public static Task Delay(TimeSpan t) { return Delay((int)t.TotalMilliseconds); }

        protected virtual Task DelayTask(int millisecondsDelay) { return Task.Delay(millisecondsDelay); }

        public static Task Run(Action action) {
            return IoC.inject.GetOrAddSingleton<TaskV2>(null).RunTask(action);
        }

        protected virtual Task RunTask(Action action) { return Task.Run(action); }

        public static Task Run(Func<Task> asyncAction) {
            return IoC.inject.GetOrAddSingleton<TaskV2>(null).RunTask(asyncAction);
        }

        protected virtual Task RunTask(Func<Task> asyncAction) { return Task.Run(asyncAction); }

        public static Task<T> Run<T>(Func<Task<T>> asyncAction) {
            return IoC.inject.GetOrAddSingleton<TaskV2>(null).RunTask(asyncAction);
        }

        protected virtual Task<T> RunTask<T>(Func<Task<T>> asyncAction) { return Task.Run(asyncAction); }

    }

}
