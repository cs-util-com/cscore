﻿using com.csutil;
using com.csutil.keyvaluestore;
using com.csutil.ui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil {

    public static class ActionMenuExtensions {

        public static async Task<ActionMenu.Entry> ShowActionMenu(this ViewStack self, ActionMenu menu, string menuPrefabName = ActionMenu.DEFAULT_MENU) {
            self.ThrowErrorIfNull("ViewStack");
            var eventName = "ShowActionMenu " + menu.menuId;
            var timing = Log.MethodEntered(eventName);
            var menuUiGo = self.ShowView(menuPrefabName);
            var presenter = new ActionMenu.Presenter();
            presenter.targetView = menuUiGo;
            try {
                var selectedEntry = (await presenter.LoadModelIntoView(menu)).clickedEntry;
                menuUiGo.Destroy(); // Close the menu UI again
                EventBus.instance.Publish(EventConsts.catMethod, eventName + " done: " + selectedEntry.name);
                Log.MethodDone(timing);
                return selectedEntry;
            }
            catch (TaskCanceledException) { // If the user canceled the action selection:
                menuUiGo.Destroy(); // Close the menu UI again
                EventBus.instance.Publish(EventConsts.catMethod, eventName + " canceled");
                Log.MethodDone(timing);
                return null;
            }
        }
        
        public static ActionMenu.Entry AddEntry(this ActionMenu self, ActionMenu.Entry entryToAdd) {
            self.entries.Add(entryToAdd);
            return entryToAdd;
        }

    }

}

namespace com.csutil.ui {

    /// <summary> A generic action menu that supports different view modes and introduces a reusable interaction pattern for all parameter-free actions </summary>
    public class ActionMenu {

        public const string DEFAULT_TITLE = "Menus/ActionMenu/ActionMenuTitle1";
        public const string DEFAULT_MENU = "Menus/ActionMenu/ActionMenu1";
        public const string DEFAULT_LIST_ENTRY = "Menus/ActionMenu/ListModeEntry1";
        public const string DEFAULT_ICON_ENTRY = "Menus/ActionMenu/IconModeEntry1";

        public string titlePrefabName = DEFAULT_TITLE;

        public enum ViewMode { iconsOnly, noDescr, full }

        public string menuId { get; }
        public string title;
        public List<Entry> entries = new List<Entry>();
        public bool isMenuFavoriteLogicEnabled = true;
        public Entry clickedEntry;
        public ViewMode viewMode = ViewMode.full; // start with full mode

        private IKeyValueStore persistedSettings = new PlayerPrefsStore();

        /// <summary> Automatically create favorite toggle actions </summary>
        public bool autoCreateFavActions = true;

        /// <summary> Will be triggered when the user tries to cancel the menu, if false is returned the cancel is rejected </summary>
        public Func<bool> onCancel = delegate { return true; };

        public ActionMenu(string id, string title = null) {
            this.menuId = id;
            this.title = title;
        }

        internal virtual int SortMenuEntries(Entry x, Entry y) {
            AssertV3.IsNotNull(x, "ActionMenu.Entry x");
            AssertV3.IsNotNull(y, "ActionMenu.Entry y");
            if (x.isFavorite == y.isFavorite) { return 0; }
            if (x.isFavorite && !y.isFavorite) { return -1; }
            return 1;
        }

        private async Task SetupFavoriteLogic() {
            AssertV3.IsTrue(entries.Count() == entries.Map(e => e.id).Distinct().Count(), () => "!! Entries with same id found in menu " + menuId);
            if (isMenuFavoriteLogicEnabled) {
                foreach (var e in entries) { await SetupFavoriteLogic(e); }
            }
        }

        internal virtual async Task SetupFavoriteLogic(Entry e) {
            e.isFavorite = await persistedSettings.Get(GetFullEntryId(e), e.isFavorite);
            if (e.onFavoriteToggled == null) {
                e.onFavoriteToggled = (isNowChecked) => {
                    persistedSettings?.Set(GetFullEntryId(e), isNowChecked);
                    if (isNowChecked) { Toast.Show("Saved as favorite: " + e.name); }
                    return true;
                };
            }
        }

        /// <summary> Can be overwritten to customize the text and description of the "More" entry </summary>
        internal virtual Entry NewShowMoreEntry() { return new Entry("", MenuIcons.More, "More", "Show all actions"); }

        private string GetFullEntryId(Entry entry) { return menuId + " - " + entry.id; }

        public class Entry {

            public string iconModeEntryPrefabName = ActionMenu.DEFAULT_ICON_ENTRY;
            public string listModeEntryPrefabName = ActionMenu.DEFAULT_LIST_ENTRY;

            public string id { get; }
            public string icon;
            public string name;
            public string descr;
            /// <summary> If false the entry will be shown but disabled </summary>
            public bool isEnabled = true;
            public Action<GameObject> onClicked;

            internal bool isFavorite;
            internal Func<bool, bool> onFavoriteToggled;

            /// <summary> Creates a new menu entry </summary>
            /// <param name="icon"> A material UI text font icon, see https://shanfan.github.io/material-icons-cheatsheet/ </param>
            /// <param name="name">A short call to action what the action will do </param>
            /// <param name="descr">A longer descr. of the action </param>
            /// <param name="id"> if null will use name as the id </param>
            public Entry(string icon, string name, string descr = null, string id = null) {
                this.icon = icon;
                this.name = name;
                this.descr = descr;
                this.id = id == null ? name : id;
            }

            public virtual bool IsInSearchResults(string searchString, bool showOnlyFavorites) {
                if (showOnlyFavorites && !this.isFavorite) { return false; }
                searchString = searchString.ToLowerInvariant();
                if (name.ToLowerInvariant().Contains(searchString)) { return true; }
                if (descr != null && descr.ToLowerInvariant().Contains(searchString)) { return true; }
                return false;
            }

            public virtual GameObject CreateIconEntryUi(ActionMenu menu, TaskCompletionSource<Entry> taskComplSource) {
                var entry = this;
                var iconGo = ResourcesV2.LoadPrefab(entry.iconModeEntryPrefabName);
                iconGo.GetComponentInChildren<Text>().text = entry.icon;
                var button = iconGo.GetComponentInChildren<Button>();
                button.interactable = entry.isEnabled;
                iconGo.GetComponentInChildren<CanvasGroup>().enabled = !entry.isEnabled;
                button.SetOnClickAction(btnGo => {
                    if (menu.entries.Contains(entry)) {
                        menu.clickedEntry = entry;
                        EventBus.instance.Publish(EventConsts.catUi + UiEvents.ACTION_MENU, menu.GetFullEntryId(entry));
                        taskComplSource.TrySetResult(entry);
                    }
                    entry.onClicked.InvokeIfNotNull(btnGo);
                });
                return iconGo;
            }

            public virtual GameObject CreateListEntryUi(ActionMenu menu, bool showOnlyFavs, TaskCompletionSource<Entry> taskComplSource) {
                var entry = this;
                var listEntryUiGo = ResourcesV2.LoadPrefab(entry.listModeEntryPrefabName);
                var map = listEntryUiGo.GetLinkMap();
                map.Get<Text>("Icon").text = entry.icon;
                map.Get<Text>("Title").text = entry.name;
                var description = map.Get<Text>("Description");
                description.gameObject.SetActiveV2(menu.viewMode == ViewMode.full && !entry.descr.IsNullOrEmpty());
                description.text = entry.descr;
                var button = map.Get<Button>("ActionSelected");
                button.interactable = entry.isEnabled;
                map.Get<CanvasGroup>("ActionSelected").enabled = !entry.isEnabled;
                button.SetOnClickAction(go => {
                    if (menu.entries.Contains(entry)) {
                        menu.clickedEntry = entry;
                        EventBus.instance.Publish(EventConsts.catUi + UiEvents.ACTION_MENU, menu.GetFullEntryId(entry));
                        taskComplSource.TrySetResult(entry);
                    }
                    entry.onClicked.InvokeIfNotNull(go);
                });
                var onFavorite = map.Get<Toggle>("FavoriteToggle");
                onFavorite.gameObject.SetActiveV2(!showOnlyFavs && entry.onFavoriteToggled != null);
                onFavorite.isOn = entry.isFavorite;
                onFavorite.SetOnValueChangedAction(entry.onFavoriteToggled);
                return listEntryUiGo;
            }

        }

        public class Presenter : Presenter<ActionMenu> {

            public GameObject targetView { get; set; }
            public bool showOnlyFavorites = false;
            private Dictionary<string, Link> map;
            private TaskCompletionSource<Entry> taskComplSource;

            public async Task OnLoad(ActionMenu menu) {
                AssertV3.IsNull(taskComplSource, "taskComplSource");
                taskComplSource = new TaskCompletionSource<Entry>();
                map = targetView.GetLinkMap();
                menu.viewMode = await menu.persistedSettings.Get(menu.menuId + " viewMode", menu.viewMode);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                menu.SetupFavoriteLogic();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                showOnlyFavorites = menu.entries.Any(x => x.isFavorite);
                SetupForViewMode(menu);
                await taskComplSource.Task;
            }

            private void SetupForViewMode(ActionMenu menu) {
                map.Get<Button>("ToggleBetweenViewModes").SetOnClickAction(delegate {
                    menu.viewMode = GetNextViewMode(menu);
                    SetupForViewMode(menu);
                    menu.persistedSettings.Set(menu.menuId + " viewMode", menu.viewMode);
                });
                map.Get<Button>("Backdrop").SetOnClickAction(delegate {
                    if (menu.onCancel()) { taskComplSource.SetCanceled(); }
                });

                var isIconMode = menu.viewMode == ViewMode.iconsOnly;
                map.Get<GameObject>("IconGridContainer").SetActiveV2(isIconMode);
                map.Get<GameObject>("EntryList").SetActiveV2(!isIconMode);
                var nextViewMode = GetNextViewMode(menu);
                map.Get<GameObject>("IconSwitchToFull").SetActiveV2(nextViewMode == ViewMode.full);
                map.Get<GameObject>("IconSwitchToNoDescr").SetActiveV2(nextViewMode == ViewMode.noDescr);
                map.Get<GameObject>("IconSwitchToIconsOnly").SetActiveV2(nextViewMode == ViewMode.iconsOnly);

                Dictionary<Entry, GameObject> entryUis = null;
                if (isIconMode) {
                    map.Get<GameObject>("Title").SetActiveV2(!menu.title.IsNullOrEmpty());
                    map.Get<Text>("TitleText").text = menu.title;
                    entryUis = FillTargetUi(menu, map.Get<GameObject>("EntryGrid"));
                } else {
                    entryUis = FillTargetUi(menu, map.Get<GameObject>("EntryList"));
                }
                SetupSearchUi(entryUis);
            }

            private ViewMode GetNextViewMode(ActionMenu menu) {
                if (menu.viewMode == ViewMode.iconsOnly) { return ViewMode.full; }
                if (menu.viewMode == ViewMode.noDescr) { return ViewMode.iconsOnly; }
                // When ViewMode.full and any descriptions switch to no descr mode:
                if (menu.entries.Any(x => !x.descr.IsNullOrEmpty())) { return ViewMode.noDescr; }
                // Else skip directly to iconsOnly mode:
                return ViewMode.iconsOnly;
            }

            private Dictionary<Entry, GameObject> FillTargetUi(ActionMenu menu, GameObject parentUi) {
                foreach (var child in parentUi.GetChildren()) { child.Destroy(); } // clear previous children
                if (!menu.title.IsNullOrEmpty() && menu.viewMode != ViewMode.iconsOnly) {
                    var titleUi = parentUi.AddChild(ResourcesV2.LoadPrefab(menu.titlePrefabName));
                    var titleMap = titleUi.GetLinkMap();
                    titleMap.Get<Text>("TitleText").text = menu.title;
                }
                var sortedEntries = new List<Entry>(menu.entries);
                if (showOnlyFavorites) { sortedEntries.Add(NewShowMoreEntry(menu)); }
                sortedEntries.Sort(menu.SortMenuEntries);
                return sortedEntries.ToDictionary(entry => entry, entry => {
                    if (menu.viewMode == ViewMode.iconsOnly) {
                        return parentUi.AddChild(entry.CreateIconEntryUi(menu, taskComplSource));
                    } else {
                        return parentUi.AddChild(entry.CreateListEntryUi(menu, showOnlyFavorites, taskComplSource));
                    }
                });
            }

            private Entry NewShowMoreEntry(ActionMenu menu) {
                Entry showMore = menu.NewShowMoreEntry();
                showMore.isFavorite = true;
                showMore.onClicked = delegate {
                    showOnlyFavorites = false; // Turn off fav. only mode
                    SetupForViewMode(menu); // Force UI to rebuild
                };
                return showMore;
            }

            private void SetupSearchUi(Dictionary<Entry, GameObject> entryUis) {
                var search = map.Get<InputField>("Search");
                var clearSearchBtn = map.Get<Button>("ClearSearch");
                search.SetOnValueChangedActionThrottled((searchString) => SetEntryUisActiveBasedOnSearchString(searchString, entryUis, clearSearchBtn));
                clearSearchBtn.SetOnClickAction(delegate {
                    search.text = "";
                    SetEntryUisActiveBasedOnSearchString(search.text, entryUis, clearSearchBtn);
                });
                SetEntryUisActiveBasedOnSearchString(search.text, entryUis, clearSearchBtn);
            }

            private void SetEntryUisActiveBasedOnSearchString(string searchString, Dictionary<Entry, GameObject> entryUis, Button clearSearchBtn) {
                foreach (var ui in entryUis) { ui.Value.SetActiveV2(ui.Key.IsInSearchResults(searchString, showOnlyFavorites)); }
                clearSearchBtn.gameObject.SetActiveV2(!searchString.IsNullOrEmpty());
            }

        }

    }

    public static class MenuIcons {
        public const string Add = "";
        public const string Edit = "";
        public const string Delete = "";
        public const string Confirm = "";
        public const string More = "";
        public const string Cut = "";
        public const string Copy = "";
        public const string Paste = "";
        public const string Move = "";
        public const string Save = "";
        public const string Share = "";
        public const string Like = "";
    }

}