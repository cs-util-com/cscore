using System;
using System.Threading.Tasks;
using com.csutil.keyvaluestore;
using com.csutil.json;

namespace com.csutil.model.immutable {

    public class ResetStoreAction { }

    public class ReplayRecorder<T> {

        private IDataStore<T> targetStore;
        private IJsonReader jsonReader = TypedJsonHelper.NewTypedJsonReader();
        private IJsonWriter jsonWriter = TypedJsonHelper.NewTypedJsonWriter();
        private IKeyValueStore persistance;
        public int recordedActionsCount { get; private set; }
        public bool isRecording = true;

        public ReplayRecorder(IKeyValueStore persistance = null) {
            if (persistance == null) { persistance = new InMemoryKeyValueStore(); }
            this.persistance = persistance;
        }

        public Middleware<T> CreateMiddleware() {
            return (IDataStore<T> store) => {
                targetStore = store;
                return (Dispatcher innerDispatcher) => {
                    return (action) => {
                        try {
                            var dispatcherResult = innerDispatcher(action);
                            RecordEntry(action, dispatcherResult);
                            return dispatcherResult;
                        } catch (Exception e) { RecordEntry(action, null, e.Message); throw; }
                    };
                };
            };
        }

        private void RecordEntry(object action, object dispatcherResult, string exception = null) {
            if (!isRecording) { return; }
            if (action is ResetStoreAction) { throw Log.e("The recorded actions will include a ResetStoreAction"); }
            AssertV2.IsFalse(action is Delegate, "The recorder received a delegate action, should be prevented by Thunk");
            try {
                Entry nextEntry = new Entry() { action = action, e = "" + exception };
                persistance.Set(GetId(recordedActionsCount), jsonWriter.Write(nextEntry));
                recordedActionsCount++;
            } catch (Exception e) { Log.e("Could not record action " + action, e); }
        }

        private string GetId(int i) { return "" + i; }

        public void ResetStore() {
            if (targetStore == null) { throw new NullReferenceException("Not connected to any store yet, use CreateMiddleware()"); }
            var oldIsRecordingValue = isRecording;
            isRecording = false; // Dont record the ResetStoreAction
            var oldState = targetStore.GetState();
            targetStore.Dispatch(new ResetStoreAction());
            var newState = targetStore.GetState();
            if (recordedActionsCount > 0 && Object.Equals(oldState, newState)) {
                throw new Exception("The store does not implement the ResetStoreAction");
            }
            isRecording = oldIsRecordingValue;
        }

        public void ClearRecording() {
            persistance.RemoveAll();
            recordedActionsCount = 0;
        }

        public async Task ReplayStore(int delayBetweenStepsInMs = 0) {
            await ReplayStore(delayBetweenStepsInMs, recordedActionsCount);
        }

        public async Task ReplayStore(int delayBetweenStepsInMs, int nrOfActionsToReplay) {
            if (nrOfActionsToReplay > recordedActionsCount) {
                throw new ArgumentException("nrOfActionsToReplay=" + nrOfActionsToReplay + " > recordedActionsCount=" + recordedActionsCount);
            }
            ResetStore();
            var oldRecordingValue = isRecording;
            isRecording = false;
            for (int i = 0; i < nrOfActionsToReplay; i++) {
                if (delayBetweenStepsInMs > 0) { await TaskV2.Delay(delayBetweenStepsInMs); }
                await DispatchRecordedEntry(i);
            }
            isRecording = oldRecordingValue;
        }

        private async Task DispatchRecordedEntry(int entryNr) {
            var nextEntryJson = await persistance.Get<string>(GetId(entryNr), null);
            var nextEntry = jsonReader.Read<Entry>(nextEntryJson);
            try {
                targetStore.Dispatch(nextEntry.action);
            } catch (System.Exception e) {
                CompareErrors(nextEntry, e);
            }
        }

        private void CompareErrors(Entry oldEntry, Exception newError) {
            if (oldEntry.e != newError.Message) {
                Log.e(new Exception("Old error differed: " + oldEntry.e, newError));
            }
        }

        private class Entry {
            public object action;
            public string e;
        }

    }

}