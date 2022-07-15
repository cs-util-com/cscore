using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace com.csutil.ui {

    public class TabsUiManager : ToggleGroupListener {

        /// <summary> By default this should return the prefab name for the input tab id that was selected by the user.
        /// The default implementation of this function is a direct mapping of the id onto the prefab name </summary>
        public Func<string, string> onTabRequested;

        /// <summary> Can be overwritten to do custom actions when a tab was selected by the user, eg if the
        /// click on a tab should not load a tab prefab but close the application </summary>
        public Action<string, ViewStack> onCustomTabClickAction = (linkId, _) => { Log.e("No action defined for " + linkId); };

        /// <summary> The Panel area needs to have a ViewStack, which will be used to load the
        /// prefab (that is returned in <see cref="onTabRequested"/>) into </summary>
        public ViewStack targetTabsPanel;

        /// <summary> Can be changed to define how quick after a new tab was selected the logic should be executed to
        /// select and switch to that selected tab. Typically the default value does not need to be changed </summary>
        public double debounceDelayInMs = 50;

        private Action<IEnumerable<Toggle>> OnActiveToggleInGroupChangedAction;

        void Start() {
            AssertV2.IsNotNull(targetTabsPanel, "TargetTabsPanel");
            if (onTabRequested == null) { onTabRequested = (linkId) => linkId; } // Default mapper
        }

        protected override void OnActiveToggleInGroupChanged(IEnumerable<Toggle> activeToggles) {
            if (OnActiveToggleInGroupChangedAction == null) {
                OnActiveToggleInGroupChangedAction = HandleActiveToggleInGroupChanged;
                OnActiveToggleInGroupChangedAction = OnActiveToggleInGroupChangedAction.AsThrottledDebounce(debounceDelayInMs, true);
            }
            OnActiveToggleInGroupChangedAction(activeToggles);
        }

        private void HandleActiveToggleInGroupChanged(IEnumerable<Toggle> activeToggles) {
            var activeToggle = activeToggles.Single();
            var link = activeToggle.GetComponent<Link>();
            AssertV2.IsNotNull(link, "Link component for toggle " + activeToggle, activeToggle);
            string prefabNameOfNewView = onTabRequested(link.id);
            if (!prefabNameOfNewView.IsNullOrEmpty()) {
                ExchangePanel(prefabNameOfNewView);
            } else {
                onCustomTabClickAction(link.id, targetTabsPanel);
            }
        }

        private void ExchangePanel(string prefabNameOfNewView) {
            var lastShownView = targetTabsPanel.GetLatestView();
            targetTabsPanel.SwitchToView(prefabNameOfNewView);
            lastShownView.Destroy();
        }

    }

}