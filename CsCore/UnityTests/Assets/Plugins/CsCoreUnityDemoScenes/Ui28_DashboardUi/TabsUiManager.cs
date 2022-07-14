using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.ui {

    public abstract class TabsUiManager : ToggleGroupListener {

        public Dictionary<Toggle, GameObject> tabs { get; private set; }

        protected override void OnActiveToggleInGroupChanged(IEnumerable<Toggle> activeToggles) {
            tabs = GetTabs();
            var toggleForTab = activeToggles.Single();
            foreach (var tab in tabs.Values) { tab.SetActiveV2(false); }
            tabs[toggleForTab].SetActiveV2(true);
        }

        protected abstract Dictionary<Toggle, GameObject> GetTabs();

    }

}