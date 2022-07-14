using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.ui {

    public class TabsUiManager : ToggleGroupListener {

        public ViewStack TargetTabsPanel;
        private Action<ViewStack, IEnumerable<Toggle>> action;

        protected override void OnActiveToggleInGroupChanged(IEnumerable<Toggle> activeToggles) {
            action?.Invoke(TargetTabsPanel, activeToggles);
        }

        public void Setup(Action<ViewStack, IEnumerable<Toggle>> action) {
            this.action = action;
        }

    }

}