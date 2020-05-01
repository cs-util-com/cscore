using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.csutil {

    public static class IAppFlowExtensions {

        public static T WithBasicTrackingActive<T>(this T self) where T : IAppFlow {
            self.ActivateInjectionTracking();
            self.ActivateMethodTracking();
            self.ActivateSystemTracking();
            self.ActivateMutationTracking();
            self.ActivatePresenterTracking();
            return self;
        }

        public static void ActivateInjectionTracking(this IAppFlow self) {
            EventBus.instance.Subscribe(self, EventConsts.catInjection, (string injected) => {
                self.TrackEvent(EventConsts.catInjection, injected);
            });
        }

        public static void ActivateSystemTracking(this IAppFlow self) {
            EventBus.instance.Subscribe(self, EventConsts.catSystem + EventConsts.APP_VERSION_CHANGED,
                (string oldVersion, string newVersion) => {
                    self.TrackEvent(EventConsts.catSystem, EventConsts.APP_VERSION_CHANGED, oldVersion, newVersion);
                });
            EventBus.instance.Subscribe(self, EventConsts.catSystem + EventConsts.INET_CHANGED,
                (bool oldState, bool newState) => {
                    self.TrackEvent(EventConsts.catSystem, EventConsts.INET_CHANGED, oldState, newState);
                });
            EventBus.instance.Subscribe(self, EventConsts.catSystem + EventConsts.LOW_MEMORY, () => {
                self.TrackEvent(EventConsts.catSystem, EventConsts.LOW_MEMORY);
            });
        }

        public static void ActivateMethodTracking(this IAppFlow self) {
            EventBus.instance.Subscribe(self, EventConsts.catMethod, (string methodName) => {
                self.TrackEvent(EventConsts.catMethod, methodName);
            });
            EventBus.instance.Subscribe(self, EventConsts.catMethod + " ENTERED",
                (string methodName, object[] args) => {
                    self.TrackEvent(EventConsts.catMethod, methodName, args);
                });
            EventBus.instance.Subscribe(self, EventConsts.catMethod + " DONE",
                (string methodName, Stopwatch timing) => {
                    self.TrackEvent(EventConsts.catMethod, methodName + " DONE", timing);
                });
        }

        public static void ActivateMutationTracking(this IAppFlow self) {
            EventBus.instance.Subscribe(self, EventConsts.catMutation, (string actionString, object action) => {
                self.TrackEvent(EventConsts.catMutation, actionString, action);
            });
        }

        public static void ActivatePresenterTracking(this IAppFlow self) {
            EventBus.instance.Subscribe(self, EventConsts.catPresenter + EventConsts.LOAD_START,
                (string name, object presenter, object model) => {
                    self.TrackEvent(EventConsts.catPresenter, EventConsts.LOAD_START + name, presenter, model);
                });
            EventBus.instance.Subscribe(self, EventConsts.catPresenter + EventConsts.LOAD_DONE,
                (string name, object presenter, object model) => {
                    self.TrackEvent(EventConsts.catPresenter, EventConsts.LOAD_DONE + name, presenter, model);
                });
        }

    }

}
