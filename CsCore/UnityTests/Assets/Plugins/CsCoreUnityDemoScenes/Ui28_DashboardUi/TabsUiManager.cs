using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace com.csutil.ui {

    public class TabsUiManager : ToggleGroupListener {

        /// <summary> By default this should return the prefab name for the input tab id that was selected by the user.
        /// The default implementation of this function is a direct mapping of the id onto the prefab name </summary>
        public Func<string, string> onTabRequested = (linkId) => linkId;

        /// <summary> Can be overwritten to do custom actions when a tab was selected by the user, eg if the
        /// click on a tab should not load a tab prefab but close the application </summary>
        public Action<string, ViewStack> onCustomTabClickAction = (linkId, _) => { Log.e("No action defined for " + linkId); };

        /// <summary> The Panel area needs to have a ViewStack, which will be used to load the
        /// prefab (that is returned in <see cref="onTabRequested"/>) into </summary>
        public ViewStack targetTabsPanel;

        protected override void OnActiveToggleInGroupChanged(IEnumerable<Toggle> activeToggles) {
            AssertV2.IsNotNull(targetTabsPanel, "TargetTabsPanel");
            var activeToggle = activeToggles.Single();
            var link = activeToggle.GetComponentV2<Link>();
            AssertV2.IsNotNull(link, "Link component for toggle " + activeToggle, activeToggle);
            string prefabNameOfNewView = onTabRequested(link.id);
            if (!prefabNameOfNewView.IsNullOrEmpty()) {
                ExchangeTabInPanel(targetTabsPanel, prefabNameOfNewView);
            } else {
                onCustomTabClickAction(link.id, targetTabsPanel);
            }
        }

        protected virtual void ExchangeTabInPanel(ViewStack tabsPanel, string prefabNameOfNewView) {
            var lastShownView = tabsPanel.GetLatestView();
            tabsPanel.SwitchToView(prefabNameOfNewView);
            lastShownView.Destroy();
        }

    }

}