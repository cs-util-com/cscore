using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace com.csutil.analytics {

    /// <summary> Collects all broadcasted log metrics (via <see cref="Log.MethodEntered(string, object[])"/>) to 
    /// measure and display the performance </summary>
    public class MethodAnalytics : IDisposable {

        private const string methodStart = EventConsts.catMethod + EventConsts.START;
        private const string methodDone = EventConsts.catMethod + EventConsts.DONE;

        public bool includeMethodArguments = false;
        public Entry Root { get; private set; }
        public Entry Current { get; private set; }

        public MethodAnalytics() {
            var s = this;
            EventBus.instance.Subscribe(s, methodStart, (string m, object[] a) => { OnMethodStart(m, a); });
            EventBus.instance.Subscribe(s, methodDone, (string m, Stopwatch t) => { OnMethodDone(m, t); });
        }

        public void Dispose() {
            EventBus.instance.Unsubscribe(this, methodStart);
            EventBus.instance.Unsubscribe(this, methodDone);
        }

        private void OnMethodStart(string methodName, object[] args) {
            Entry e = NewEntryFor(methodName, args);
            e.parent = Current;
            if (Current != null) {
                if (Current.Then == null) { Current.Then = new List<Entry>(); }
                Current.Then.Add(e);
            } else {
                Root = e;
            }
            Current = e;
        }

        private Entry NewEntryFor(string methodName, object[] args) {
            var e = new Entry() { MethodName = methodName };
            if (includeMethodArguments && !args.IsNullOrEmpty()) {
                List<string> filtered = new List<string>();
                for (int i = 0; i < args.Length - 1; i++) {
                    var arg = "" + args[i];
                    if (!(arg).IsNullOrEmpty()) { filtered.Add(arg); }
                }
                if (!filtered.IsNullOrEmpty()) { e.Args = filtered; }
            }
            return e;
        }

        private void OnMethodDone(string methodName, Stopwatch timing) {
            if (Current == null) { return; }
            if (timing.ElapsedMilliseconds > 0) {
                Current.DurationInMs = timing.ElapsedMilliseconds;
            }
            if (Current.MethodName == methodName) { Current = Current.parent; }
        }

        public override string ToString() {
            return JsonConvert.SerializeObject(Root, new JsonSerializerSettings() {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            });
        }

        public class Entry {
            public string MethodName { get; set; }
            public long? DurationInMs { get; set; }
            public IEnumerable<string> Args { get; internal set; }
            internal Entry parent;
            public List<Entry> Then { get; set; }
        }

    }

}