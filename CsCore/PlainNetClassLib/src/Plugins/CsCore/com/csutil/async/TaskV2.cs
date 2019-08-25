using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.csutil {

    public class TaskV2 {

        public static Task Delay(int millisecondsDelay) {
            return IoC.inject.GetOrAddSingleton<TaskV2>(null).DelayTask(millisecondsDelay);
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

    }

}
