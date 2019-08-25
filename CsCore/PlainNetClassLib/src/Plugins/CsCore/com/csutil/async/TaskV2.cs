using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.csutil {

    public class TaskV2 {

        public static Task Delay(TimeSpan t) { return Delay((int)t.TotalMilliseconds); }

        public static Task Delay(int millisecondsDelay) {
            return IoC.inject.GetOrAddSingleton<TaskV2>(null).DelayTask(millisecondsDelay);
        }

        protected virtual Task DelayTask(int millisecondsDelay) {
            return Task.Delay(millisecondsDelay);
        }

    }

}
